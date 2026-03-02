using UnityEngine;

public class camera : MonoBehaviour
{
  public Transform player;
  public Camera main;

  [Header("Bounds")]
  public float minX = -10f;
  public float maxX = 10f;
  public float minZ = -10f;
  public float maxZ = 10f;

  void Update()
  {
    float clampedX = Mathf.Clamp(player.position.x, minX, maxX);
    float clampedZ = Mathf.Clamp(player.position.z, minZ, maxZ);

    transform.position = new Vector3(clampedX, 0f, clampedZ);
  }
}
