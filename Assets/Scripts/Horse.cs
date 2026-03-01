using System;
using System.Collections;
using UnityEngine;

public class Horse : MonoBehaviour
{
  public ObstacleManager obstacleManager;

  [SerializeField]
  private Rigidbody rb;

  [SerializeField]
  private LayerMask layerMask;

  [SerializeField]
  private float rotationSpeed = 5f;

  [Header("Speed")]
  [SerializeField]
  private float maxRadius = 10f;

  [SerializeField]
  public float maxVelocity = 0.2f;

  [Header("Jump")]
  [SerializeField]
  private float jumpMultiplier = 1f;

  [SerializeField]
  private float minJumpHeight = 1f;

  [SerializeField]
  private float maxJumpHeight = 4f;

  [SerializeField]
  private float jumpHeight;

  [SerializeField]
  private float velocityFactor = 1f;

  [SerializeField]
  private Animator animator;

  public float velocity = 0f;
  public int score = 0;
  public int health = 3;

  private Camera cam;
  private GameObject sphere;
  private Vector3 mouseDirection;

  private bool isGrounded = true;
  private bool isChargingJump = false;
  private bool isDying = false;

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
    if (isDying) return;

    if (health <= 0)
    {
      isDying = true;
      transform.Translate(new(0f, 1f, 0f));
      transform.Rotate(new(0f, 0f, 90f));
      velocity = 0f;
      StartCoroutine(delayCall(2f, reset));
    }
    animator.SetFloat("velocity", velocity);

    ApplyGravity();
    MousePosition();
    followMouse();
    handleJump();

    if (Input.GetKeyDown(KeyCode.R))
      reset();
  }

  private void reset()
  {
    health = 3;
    score = 0;
    transform.position = Vector3.zero;
    velocity = 0f;
    isDying = false;
  }

  private void ApplyGravity()
  {
    if (rb.linearVelocity.y < 0)
    {
      rb.linearVelocity += Vector3.up * Physics.gravity.y * 2f * Time.deltaTime;
    }
    else if (rb.linearVelocity.y > 0 && !isChargingJump)
    {
      rb.linearVelocity += Vector3.up * Physics.gravity.y * 2f * Time.deltaTime;
    }
  }

  private void followMouse()
  {
    if (isGrounded)
    {
      transform.rotation = Quaternion.Slerp(
          transform.rotation,
          Quaternion.LookRotation(mouseDirection),
          rotationSpeed * Time.deltaTime
      );
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

    rb.position += transform.forward * Mathf.Max(velocity, 0) * Time.deltaTime;
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
      mouseDirection =
          new Vector3(hit.point.x, transform.position.y, hit.point.z) - transform.position;
      sphere.transform.position = hit.point;
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.tag.Equals("hurdle"))
    {
      health--;
      obstacleManager.RemoveObstacle(collision.transform.position);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.layer == 3)
    {
      isGrounded = true;
      isChargingJump = false;
    }

    if (other.gameObject.layer == 9 && Input.GetMouseButton(0))
    {
      score += 5;
      StartCoroutine(delayCall(1f, () =>
      {
        obstacleManager.RemoveObstacle(other.transform.position);
      }));
    }

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
  public IEnumerator delayCall(float seconds, Action callback)
  {
    yield return new WaitForSeconds(seconds);
    callback();
  }
}

