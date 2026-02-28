using UnityEngine;

public class camera : MonoBehaviour
{
  public Transform player;
  public Camera main;

  void Update()
  {
    transform.position = new(player.position.x, 0f, player.position.z);
    if (Input.GetMouseButtonDown(2))
    {
      main.orthographic = !main.orthographic;
    }
  }
}
