using UnityEngine;

public class lookAtCamera : MonoBehaviour
{
  void Update()
  {
    transform.LookAt(2 * transform.position - Camera.main.transform.position);
  }
}
