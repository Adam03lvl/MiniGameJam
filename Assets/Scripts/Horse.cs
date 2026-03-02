using System;
using System.Collections;
using UnityEngine;
public class Horse : MonoBehaviour
{
  public ObstacleManager obstacleManager;

  [SerializeField] private Rigidbody rb;
  [SerializeField] private LayerMask layerMask;
  [SerializeField] private float rotationSpeed = 5f;

  [Header("Speed")]
  [SerializeField] private float maxRadius = 10f;
  [SerializeField] public float maxVelocity = 0.2f;

  [Header("Jump")]
  [SerializeField] private float jumpMultiplier = 1f;
  [SerializeField] private float minJumpHeight = 1f;
  [SerializeField] private float maxJumpHeight = 4f;
  [SerializeField] private float jumpHeight;
  [SerializeField] private float velocityFactor = 1f;
  public float velocity = 0f;
  public int score = 0;
  public int health = 3;

  public Animator animator;
  public GameObject damage;
  public GameObject healthPrefab;
  public MeshRenderer ground;

  public Material red;
  public Material blue;
  public Material green;

  private Camera cam;
  private GameObject sphere;
  private Vector3 mouseDirection;

  private bool isGrounded = true;
  private bool isChargingJump = false;
  private bool isDying = false;
  private bool isInvincible = false;
  private bool healthSpawned = false;
  public bool GameStarted = false;

  void Start()
  {
    cam = Camera.main;
    sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    sphere.GetComponent<MeshRenderer>().material = red;
    Destroy(sphere.GetComponent<Collider>());
    sphere.transform.localScale = new(0.2f, 0.2f, 0.2f);
  }

  private void Update()
  {

    if (!GameStarted)
    {
      if (Input.GetKeyDown(KeyCode.Space))
      {
        GameStarted = true;
      }

      return;
    }

    maxVelocity += .01f * Time.deltaTime;

    if (isDying) return;
    if (health < 3 && !healthSpawned && score % 20 == 0)
    {
      healthSpawned = true;
      float randomX = UnityEngine.Random.Range(
          ground.bounds.min.x + 15,
          ground.bounds.max.x - 15
      );
      float randomZ = UnityEngine.Random.Range(
          ground.bounds.min.z + 15,
          ground.bounds.max.z - 15
      );
      GameObject h = Instantiate(healthPrefab, new(randomX, .5f, randomZ), Quaternion.Euler(new(0, 0, 0)));
    }

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
    isInvincible = false;
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
      sphere.GetComponent<MeshRenderer>().material = green;
    }
    else if (velocity > 0)
    {
      velocity -= velocityFactor * Time.deltaTime;
    }

    if (Input.GetMouseButtonUp(0))
    {
      sphere.GetComponent<MeshRenderer>().material = red;
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
      sphere.transform.localScale = Vector3.one * jumpHeight * .5f;
      sphere.GetComponent<MeshRenderer>().material = blue;
    }

    if (Input.GetMouseButtonUp(1) && isGrounded && isChargingJump)
    {
      float finalHeight = Mathf.Min(jumpHeight, maxJumpHeight);
      float velocity = Mathf.Sqrt(finalHeight * -2 * Physics.gravity.y * 3f);
      rb.AddForce((Vector3.up * velocity) + transform.forward, ForceMode.Impulse);

      sphere.transform.localScale = new(0.2f, 0.2f, 0.2f);
      isGrounded = false;
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
      mouseDirection =
          new Vector3(hit.point.x, transform.position.y, hit.point.z) - transform.position;
      sphere.transform.position = hit.point;
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.CompareTag("hurdle"))
    {
      if (!isInvincible)
      {
        health--;
        velocity *= .5f;
        StartCoroutine(InvincibilityCoroutine());
      }

      StartCoroutine(delayCall(1f, () =>
      {
        obstacleManager.RemoveObstacle(collision.transform.position);
      }));
    }
  }

  private IEnumerator InvincibilityCoroutine()
  {
    isInvincible = true;
    damage.SetActive(true);
    yield return new WaitForSeconds(2f);
    damage.SetActive(false);
    isInvincible = false;
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.layer == 3)
    {
      isGrounded = true;
      isChargingJump = false;
    }

    if (other.gameObject.layer == 6)
    {
      health++;
      Destroy(other.gameObject);
      healthSpawned = false;
    }

    if (other.gameObject.layer == 9 && Input.GetMouseButton(0))
    {
      score += 5;
      other.gameObject.SetActive(false);
      StartCoroutine(delayCall(1f, () =>
      {
        obstacleManager.RemoveObstacle(other.transform.position);
      }));
    }
  }

  public IEnumerator delayCall(float seconds, Action callback)
  {
    yield return new WaitForSeconds(seconds);
    callback();
  }
}