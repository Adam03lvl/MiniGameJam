using UnityEngine;

public class camera : MonoBehaviour
{
  public Transform player;

  void Update()
  {
    transform.position = new(player.position.x, 0f, player.position.z);
  }
}
