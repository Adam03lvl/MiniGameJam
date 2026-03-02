using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
  public Horse horse;
  public TextMeshProUGUI scoreText;
  public TextMeshProUGUI healthText;
  public TextMeshProUGUI deathScreen;
  public TextMeshProUGUI Start;

  void Update()
  {
    if (horse.GameStarted)
    {
      Start.text = "";
    }
    else return;

    scoreText.text = $"Score\n{horse.score}";
    healthText.text = $"Health\n{horse.health}";
    if (horse.health <= 0)
    {
      deathScreen.text = "THE HORSE F* DIED";
      StartCoroutine(horse.delayCall(2f, () => { deathScreen.text = ""; }));
    }
  }
}
