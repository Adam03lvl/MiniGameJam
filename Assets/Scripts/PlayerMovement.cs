using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  [SerializeField] private Rigidbody rb;
  [SerializeField] private float speed = 10.0f;
  [SerializeField] private LayerMask layerMask;
  private Camera cam;
  private GameObject sphere;
  private Vector3 forward;

  void Start()
  {
    cam = Camera.main;
    sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    sphere.GetComponent<MeshRenderer>().material.color = Color.red;
    Destroy(sphere.GetComponent<Collider>());
  }

  private void FixedUpdate()
  {
    Vector3 mouse = Input.mousePosition;
    Ray castPoint = cam.ScreenPointToRay(mouse);
    RaycastHit hit;

    if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, layerMask))
    {
      forward = hit.point - transform.position;
      sphere.transform.position = hit.point;
      transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
    }

    if (Input.GetMouseButton(0))
    {
      rb.position += forward * speed * Time.deltaTime;
    }
  }
}
