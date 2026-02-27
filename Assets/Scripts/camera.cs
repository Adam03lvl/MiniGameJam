using UnityEngine;

public class camera : MonoBehaviour
{
  public Transform player;

  void Update()
  {
    transform.position = player.position;
  }
}
