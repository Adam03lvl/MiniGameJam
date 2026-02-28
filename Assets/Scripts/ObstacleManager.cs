using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Setup")]
    public GameObject[] obstacles;
    public GameObject floor;
    public float floorLimitOffset = 5f;

    [Tooltip(
        "How many times the game should try to look for a valid spawn position for an obstacle."
    )]
    public int maxAttempts = 50;
    public float minDistanceBetweenObstacles = 5f;

    [Header("Gameplay")]
    public int obstacleLimit = 10;

    private Vector3 floorPos;

    private Dictionary<Vector3, GameObject> obstaclePositions =
        new Dictionary<Vector3, GameObject>();

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
        int randomIndex = Random.Range(0, obstacles.Length);

        MeshRenderer floorMesh = floor.GetComponent<MeshRenderer>();

        Bounds bounds = floorMesh.bounds;

        Vector3 randomPosition = Vector3.zero;
        bool foundValidPosition = false;
        int attempts = 0;

        while (!foundValidPosition && attempts < maxAttempts)
        {
            float randomX = Random.Range(
                bounds.min.x + floorLimitOffset,
                bounds.max.x - floorLimitOffset
            );
            float randomZ = Random.Range(
                bounds.min.z + floorLimitOffset,
                bounds.max.z - floorLimitOffset
            );

            randomPosition = new Vector3(randomX, 0, randomZ);

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

            GameObject newObstacle = Instantiate(obstacles[randomIndex], randomPosition, rotation);
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

    private bool IsPositionValid(Vector3 position)
    {
        foreach (var existingPosition in obstaclePositions.Keys)
        {
            if (Vector3.Distance(position, existingPosition) < minDistanceBetweenObstacles)
            {
                return false;
            }
        }
        return true;
    }

    public void RemoveObstacle(Vector3 position)
    {
        GameObject obstacleToRemove = obstaclePositions[position];
        if (obstaclePositions.ContainsKey(position))
        {
            obstaclePositions.Remove(position);
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
