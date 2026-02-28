using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] private Rigidbody rb;
  [SerializeField] private LayerMask layerMask;
  [SerializeField] private float rotationSpeed = 5f;
  [Header("Speed")]
  [SerializeField] private float maxRadius = 10f;
  [SerializeField] private float maxVelocity = 0.2f;
  [Header("Jump")]
  [SerializeField] private float jumpMultiplier = 1f;
  [SerializeField] private float minJumpHeight = 1f;
  [SerializeField] private float maxJumpHeight = 4f;
  [SerializeField] private float jumpHeight;
  [SerializeField] private float velocityFactor = 1f;

  private float velocity = 0f;
  private float rotationFactor;

  private Camera cam;
  private GameObject sphere;
  private Vector3 mouseDirection;

  private bool isGrounded = true;
  private bool isChargingJump = false;

  void Start()
  {
    cam = Camera.main;
    sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    sphere.GetComponent<MeshRenderer>().material.color = Color.red;
    Destroy(sphere.GetComponent<Collider>());
    sphere.transform.localScale = new(0.2f, 0.2f, 0.2f);
  }

  private void Update()
  {

    if (rb.linearVelocity.y < 0)
    {
      rb.linearVelocity += Vector3.up * Physics.gravity.y * 2f * Time.deltaTime;
    }
    else if (rb.linearVelocity.y > 0 && !isChargingJump)
    {
      rb.linearVelocity += Vector3.up * Physics.gravity.y * 2f * Time.deltaTime;
    }

    MousePosition();
    followMouse();
    handleJump();

    if (Input.GetKeyDown(KeyCode.R))
    {
      transform.position = new(0, 2, 0);
      velocity = 0f;
    }
  }

  private void followMouse()
  {
    maxVelocity += 0.1f * Time.deltaTime;
    rotationFactor = rotationSpeed /* * (1 / mouseDirection.magnitude)*/ * Time.deltaTime;

    if (isGrounded)
    {
      transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(mouseDirection),
        rotationFactor);
    }

    if (Input.GetMouseButton(0) && velocity < maxVelocity)
    {
      velocity += Mathf.Min(mouseDirection.magnitude, maxRadius) * Time.deltaTime;
      sphere.GetComponent<MeshRenderer>().material.color = Color.green;
    }
    else if (velocity > 0)
    {
      velocity -= velocityFactor * Time.deltaTime;
    }

    if (Input.GetMouseButtonUp(0))
    {
      sphere.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    rb.position += transform.forward * velocity * Time.deltaTime;
  }

  private void handleJump()
  {
    if (Input.GetMouseButtonDown(1) && isGrounded)
    {
      isChargingJump = true;
      jumpHeight = minJumpHeight;
    }

    if (Input.GetMouseButton(1) && isChargingJump && jumpHeight < maxJumpHeight)
    {
      jumpHeight += jumpMultiplier * Time.deltaTime;
    }

    if (Input.GetMouseButtonUp(1) && isGrounded && isChargingJump)
    {
      float finalHeight = Mathf.Min(jumpHeight, maxJumpHeight);
      float velocity = Mathf.Sqrt(finalHeight * -2 * Physics.gravity.y * 3f);
      rb.AddForce((Vector3.up * velocity) + transform.forward, ForceMode.Impulse);

      isGrounded = false;
      isChargingJump = false;
    }
  }

  private void jump(float height)
  {
    float velocity = Mathf.Sqrt(height * -2 * Physics.gravity.y * 3f);
    rb.AddForce(Vector3.up * velocity, ForceMode.Impulse);
  }

  private void MousePosition()
  {
    Vector3 mouse = Input.mousePosition;
    Ray camCastPoint = cam.ScreenPointToRay(mouse);
    RaycastHit hit;
    if (Physics.Raycast(camCastPoint, out hit, Mathf.Infinity, layerMask))
    {
      mouseDirection = new Vector3(hit.point.x, transform.position.y, hit.point.z) - transform.position;
      sphere.transform.position = hit.point;
    }
  }
  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.layer == 3)
    {
      isGrounded = true;
      isChargingJump = false;
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.layer == 6)
    {
      velocity *= 2;
      Destroy(other.gameObject);
    }

    if (other.gameObject.layer == 7)
    {
      velocity *= 2;
      Destroy(other.gameObject);
    }

    if (other.gameObject.layer == 8 && Input.GetMouseButton(1))
    {
      jump(3);
      Destroy(other.gameObject);
    }
  }
}