using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
  public Horse horse;
  public TextMeshProUGUI scoreText;
  public TextMeshProUGUI healthText;
  public TextMeshProUGUI deathScreen;

  void Update()
  {
    scoreText.text = $"Score: {horse.score}";
    healthText.text = $"Health: {horse.health}";
    if (horse.health <= 0)
    {
      deathScreen.text = "THE HORSE F* DIED";
      StartCoroutine(horse.delayCall(2f, () => { deathScreen.text = ""; }));
    }
  }
}
