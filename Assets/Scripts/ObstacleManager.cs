using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
  [Header("Objects Setup")]
  public GameObject[] ObstaclePrefabs;
  public GameObject groundPlane;

  [Tooltip(
      "How many times the game should try to look for a valid spawn position for an obstacle."
  )]
  public int maxAttempts;
  public float minDistanceBetweenObstacles;

  [Header("Gameplay")]
  public int obstacleLimit;

  private Dictionary<Vector2, GameObject> obstaclePositions =
      new Dictionary<Vector2, GameObject>();

  void Start()
  {
    while (obstaclePositions.Count < obstacleLimit)
    {
      SpawnObstacle();
    }
  }

  void Update()
  {
    CheckObstacleCount();
  }

  private void SpawnObstacle()
  {
    int randomIndex = Random.Range(0, ObstaclePrefabs.Length);

    Vector2 randomPosition = Vector3.zero;
    bool foundValidPosition = false;
    int attempts = 0;

    Bounds bounds = groundPlane.GetComponent<Renderer>().bounds;

    while (!foundValidPosition && attempts < maxAttempts)
    {
      float randomX = Random.Range(
          bounds.min.x + 15,
          bounds.max.x - 15
      );
      float randomZ = Random.Range(
          bounds.min.z + 15,
          bounds.max.z - 15
      );

      randomPosition = new Vector2(randomX, randomZ);

      if (IsPositionValid(randomPosition))
      {
        foundValidPosition = true;
      }

      attempts++;
    }

    if (foundValidPosition)
    {
      float yRot = Random.Range(0f, 360f);
      Quaternion rotation = Quaternion.Euler(0, yRot, 0);

      GameObject newObstacle = Instantiate(ObstaclePrefabs[randomIndex], new(randomPosition.x, 0, randomPosition.y), rotation);
      obstaclePositions.Add(randomPosition, newObstacle);
    }
    else
    {
      Debug.LogWarning(
          "Could not find a valid position for the obstacle after "
              + maxAttempts
              + " attempts."
      );
    }
  }

  private bool IsPositionValid(Vector2 position)
  {
    foreach (var existingPosition in obstaclePositions.Keys)
    {
      if (Vector2.Distance(position, existingPosition) < minDistanceBetweenObstacles)
      {
        return false;
      }
    }
    return true;
  }

  public void RemoveObstacle(Vector3 position)
  {
    Vector2 pos = new(position.x, position.z);
    if (obstaclePositions.ContainsKey(pos))
    {
      GameObject obstacleToRemove = obstaclePositions[pos];
      obstaclePositions.Remove(pos);
      Destroy(obstacleToRemove);
    }
  }

  private void CheckObstacleCount()
  {
    if (getObstacleCount() < obstacleLimit)
    {
      SpawnObstacle();
    }
  }

  private int getObstacleCount()
  {
    GameObject[] obstaclesInScene = GameObject.FindGameObjectsWithTag("obstacle");
    return obstaclesInScene.Length;
  }
}
