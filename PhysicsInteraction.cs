using UnityEngine;
using UnityEngine.UI;

public class PhysicsInteraction : MonoBehaviour
{
    [SerializeField] private float maxGrabDistance = 20f; // Maximum raycast distance to check if there is a rigidbody on the way
    [SerializeField] private float maxSphereDistance = 25f; // Maximum sphere distance from the camera when moving shpere with mouse
    [SerializeField] private float grabSpring = 40f;  // Adjust grab strength values / SpringJoint values
    [SerializeField] private float grabDamper = 0.2f; // Adjust grab strength values / SpringJoint values
    [SerializeField] private float throwForce = 10.0f;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private float sphereMoveSpeed = 0.1f; // move sphere with mouse speed 
    [SerializeField] private bool renderSphereMesh = true;  //render sphere mesh
    [SerializeField] private Image image1;  // Can grab image
    [SerializeField] private Image image2;  // Is grabbing image

    private GameObject _sphere;
    private Rigidbody _sphereRb;
    private Rigidbody _hitRigidbody;
    private bool _isShooting;
    private RaycastHit _hitInfo;

    [SerializeField] private Camera mainCamera;
    private void Start()
    {
        _sphere = Instantiate(spherePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        _sphere.transform.parent = mainCamera.transform;
        _sphereRb = _sphere.GetComponent<Rigidbody>();
    }

    void Update()
    {
        ShootRaycast();

        if (Input.GetMouseButton(0) && _isShooting)
        {
            ApplySpringConstraint();
            MoveSphereWithMouse();

            if (Input.GetMouseButtonDown(1) && _hitRigidbody != null)
            {
                ThrowObject(_hitRigidbody);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetShooting();
        }
    }

    void ShootRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out _hitInfo, maxGrabDistance))
        {
            if (Input.GetMouseButtonDown(0))
            {
                _hitRigidbody = _hitInfo.collider.GetComponent<Rigidbody>();
                MoveSphere(_hitInfo.point);
                _isShooting = true;
            }

            if (_isShooting == false)
            {
                DisplayImage(image1); // Display the first image if raycasting to a rigidbody                
            }
            else HideImage(image1);

            if (!_hitInfo.collider.GetComponent<Rigidbody>()) HideImage(image1);
        }
        else HideImage(image1);
    }

    void MoveSphere(Vector3 position)
    {;
        _sphere.transform.position = position;
        _sphere.transform.parent = mainCamera.transform;

        // Enable/disable rendering of the sphere mesh based on the checkbox value
        Renderer sphereRenderer = _sphere.GetComponent<Renderer>();
        if (sphereRenderer)
        {
            sphereRenderer.enabled = renderSphereMesh;
        }
    }

    void ApplySpringConstraint()
    {
        if (_hitRigidbody && _sphere )
        {
            SpringJoint spring = _hitRigidbody.gameObject.GetComponent<SpringJoint>();
            if (!spring)
            {
                spring = _hitRigidbody.gameObject.AddComponent<SpringJoint>();
                spring.autoConfigureConnectedAnchor = false;
                spring.connectedBody = _sphereRb;
                spring.connectedAnchor = Vector3.zero;
                spring.spring = grabSpring;
                spring.damper = grabDamper;

                spring.massScale = 1f;
                spring.minDistance = 0.1f;
                spring.maxDistance = 0f;

                // Set the spring joint anchors
                Vector3 localHitPoint = _hitInfo.point - _hitRigidbody.gameObject.transform.position;
                Vector3 scaleOfHitObject = _hitRigidbody.gameObject.transform.localScale;
                Quaternion rotation = Quaternion.Euler(_hitRigidbody.transform.rotation.eulerAngles);
                rotation.x *= -1;
                rotation.y *= -1;
                rotation.z *= -1;
                Vector3 rotatedLocalHitPoint = rotation * localHitPoint;
                rotatedLocalHitPoint = new Vector3(
                    rotatedLocalHitPoint.x/scaleOfHitObject.x,
                    rotatedLocalHitPoint.y/scaleOfHitObject.y,
                    rotatedLocalHitPoint.z/scaleOfHitObject.z);
                //Debug.Log(rotatedLocalHitPoint);
                spring.anchor = rotatedLocalHitPoint;
                
                DisplayImage(image2);
            }
        }
    }

    void MoveSphereWithMouse()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        if (_sphere)
        {
            float scrollWheelFactor = Input.GetAxis("Mouse ScrollWheel") * 5;
            Vector3 sphereMovement = mainCamera.transform.forward * sphereMoveSpeed * mouseY + scrollWheelFactor*Camera.main.transform.forward;
            Vector3 spherePos = _sphere.transform.position + sphereMovement;

            // Clamp sphere position relative to the camera
            float sphereDistanceFromCamera = Vector3.Distance(Camera.main.transform.position, spherePos);
            if (sphereDistanceFromCamera <= maxSphereDistance && sphereDistanceFromCamera > 0.4f)
            {
                _sphere.transform.position = spherePos;
            }
        }
    }

    void ResetShooting()
    {
        if (_hitRigidbody)
        {
            Destroy(_hitRigidbody.GetComponent<SpringJoint>());
        }

        _isShooting = false;
        _hitRigidbody = null;
        image2.gameObject.SetActive(false);
    }

    void ThrowObject(Rigidbody objectToThrow)
    {
        Vector3 throwDirection = mainCamera.transform.forward;
        objectToThrow.AddForce(throwDirection * throwForce,ForceMode.Impulse);
        ResetShooting();
    }

    void DisplayImage(Image img)
    {
        img.gameObject.SetActive(true);
    }

    void HideImage(Image img)
    {
        img.gameObject.SetActive(false);
    }
}
