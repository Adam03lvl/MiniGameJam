using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] private Rigidbody rb;
  [SerializeField] private LayerMask layerMask;
  [SerializeField] private float maxSpeed = 10f;
  [SerializeField] private float maxJumpForce = 600f;
  [SerializeField] private float minJumpForce = 100f;
  [SerializeField] private float jumpMultiplier = 1f;
  [SerializeField] private float fallMultiplier = 2.5f;        // heavier fall gravity
  [SerializeField] private float lowJumpMultiplier = 2f;       // snappier release on short hops
  [SerializeField] private float forwardJumpMultiplier = 2f;       // snappier release on short hops
  [SerializeField] private float rotationSpeed = 5f;
  [SerializeField] private float maxVelocity = 20f;
  private float velocity = 0f;
  private float velocityFactor = 0.01f;
  private float jumpForce;
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
    MousePosition();
    ApplyHorseGravity();

    rotationFactor = rotationSpeed * (1/mouseDirection.magnitude) * Time.deltaTime;

    if (isGrounded)
    {
      transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(mouseDirection),
        rotationFactor);
    }

    if (Input.GetMouseButton(0) && velocity < maxVelocity)
    {
      velocity += Mathf.Min(mouseDirection.magnitude, maxSpeed) * 0.01f;
      sphere.GetComponent<MeshRenderer>().material.color= Color.green;
    }
    else if (velocity > 0)
    {
      velocity -= velocityFactor;
    }

    if (Input.GetMouseButtonUp(0))
    {
      sphere.GetComponent<MeshRenderer>().material.color = Color.red;
    }

    rb.position += transform.forward * velocity * Time.deltaTime;

    if (Input.GetMouseButtonDown(1) && isGrounded)
    {
      isChargingJump = true;
      jumpForce = minJumpForce;
    }

    if (Input.GetMouseButton(1) && isChargingJump && jumpForce < maxJumpForce)
    {
      jumpForce += jumpMultiplier * Time.deltaTime;
    }

    if (Input.GetMouseButtonUp(1) && isGrounded && isChargingJump)
    {
      Vector3 flatForward = new Vector3(mouseDirection.x, 0f, mouseDirection.z).normalized;
      rb.AddForce((Vector3.up * jumpForce) + transform.forward * forwardJumpMultiplier, ForceMode.Impulse);

      isGrounded = false;
      isChargingJump = false;
      jumpForce = minJumpForce;
    }
  }

  private void ApplyHorseGravity()
  {
    if (rb.linearVelocity.y < 0)
    {
      // Horse comes down heavy
      rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
    }
    else if (rb.linearVelocity.y > 0 && !isChargingJump)
    {
      // Short hops don't float — horse weight pulls it back
      rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
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
}