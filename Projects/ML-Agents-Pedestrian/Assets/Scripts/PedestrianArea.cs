using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MLAgents;
using UnityEngine.UI;

public class PedestrianArea : MonoBehaviour
{
    public int trainingAreaIndex;
    public List<GameObject> pedestrianList;
    public List<GameObject> pedestrianSpawnerList;
    public List<Transform> agentSpawnList;
    public CarAgent agent;
    public TextMeshPro cumulativeRewardText;
    public bool debug = false;
    public TextMeshProUGUI debugRewardText;
    public Text brainText;
    public List<GameObject> checkpointList;
    private int checkpointsRemaining;
    public Dictionary<string, int> collisionList = new Dictionary<string, int>();
    public List<Vector3> collisionPositionList = new List<Vector3>();

    /// <summary>
    /// Reset the training area, obtain pedestrian prefabs and initialise collision list with prefab names
    /// </summary>
    void Start()
    {
        List<GameObject> prefabs = GameObject.FindGameObjectWithTag("SingleSpawner").GetComponent<PedestrianSpawner>().GetPrefabs();
        if (collisionList.Count <= 0)
        {
            foreach(GameObject prefab in prefabs)
            {
                collisionList.Add(prefab.name, 0);
            }
        }
    }

    /// <summary>
    /// Increment the stored collision value for the passed pedestrian model
    /// </summary>
    /// <param name="key">Pedestrian model name</param>
    public void AddCollision(string key)
    {
        int value = collisionList[key];
        value++;
        collisionList[key] = value;
    }

    /// <summary>
    /// Change the value of the remaining checkpoints by the given value
    /// </summary>
    /// <param name="value">Value to adjust checkpoints by</param>
    public void AdjustRemainingCheckpoints(int value)
    {
        checkpointsRemaining += value;
    }

    /// <summary>
    /// Return the current number of checkpoints remaining
    /// </summary>
    /// <returns>Count of the remaining active checkpoints</returns>
    public int GetRemainingCheckpoints()
    {
        return checkpointsRemaining;
    }

    /// <summary>
    /// Reset the training environment in user input
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetArea();
        }
        cumulativeRewardText.text = agent.GetCumulativeReward().ToString("0.00");
        if (debugRewardText != null)
            debugRewardText.text = agent.GetCumulativeReward().ToString("0.00");
    }

    /// <summary>
    /// Randomize whether the current spawner is enabled.
    /// </summary>
    public void RandomizeSpawner()
    {
        foreach(GameObject spawner in pedestrianSpawnerList)
        {
            PedestrianSpawner spawnScript = spawner.GetComponent<PedestrianSpawner>();
            if (spawner.gameObject.CompareTag("CurveSpawner"))
                spawnScript.percentActive = Academy.Instance.FloatProperties.GetPropertyWithDefault("spawn_rate_curve", 20.0f);
            else if (spawner.gameObject.CompareTag("SingleSpawner"))
                spawnScript.percentActive = Academy.Instance.FloatProperties.GetPropertyWithDefault("spawn_rate_single", 20.0f);
            else if (spawner.gameObject.CompareTag("CrossingSpawner"))
                spawnScript.percentActive = Academy.Instance.FloatProperties.GetPropertyWithDefault("spawn_rate_crossing", 75.0f);

            if (spawnScript.percentActive <= 0)
                spawnScript.enabled = false;
            else if (Random.Range(0, 100) > spawnScript.percentActive)
                spawnScript.enabled = false;
            else
                spawnScript.enabled = true;

            if (spawnScript.enabled && Vector3.Distance(agent.gameObject.transform.position, spawner.transform.position) > 10)
            {
                if (spawnScript.GetPrefabs().Count > 0)
                    spawnScript.SpawnPedestrian();
            }
            spawnScript.rotationRandomizerMultiplier = Academy.Instance.FloatProperties.GetPropertyWithDefault("pedestrian_rotation_randomizer", 1.0f);

        }
    }

    /// <summary>
    /// Set the agent as finished, remove all pedestrians, reset the agent position, randomize all spawners and reset checkpoints
    /// </summary>
    public void ResetArea()
    {
        agent.Done();
        RemovePedestrians();
        PlaceAgent();
        RandomizeSpawner();
        ResetCheckpoints();
    }

    /// <summary>
    /// Set all checkpoints to active and restore remaining checkpoints to full value
    /// </summary>
    private void ResetCheckpoints()
    {
        foreach (GameObject holder in checkpointList)
        {
            foreach (Transform checkpoint in holder.transform)
            {
                checkpoint.gameObject.SetActive(true);
            }
        }
        checkpointsRemaining = checkpointList.Count;
    }

    /// <summary>
    /// Randomly set the agent's position and rotation from spawn position array
    /// </summary>
    private void PlaceAgent()
    {
        Rigidbody agentRB = agent.GetComponent<Rigidbody>();
        agentRB.velocity = Vector3.zero;
        agentRB.angularVelocity = Vector3.zero;
        agent.ResetAgent();

        int spawnerIndex = Random.Range(0, agentSpawnList.Count);
        agent.transform.position = agentSpawnList[spawnerIndex].position;
        agent.transform.rotation = agentSpawnList[spawnerIndex].rotation;
    }

    /// <summary>
    /// Remove all pedestrians from the environment
    /// </summary>
    private void RemovePedestrians()
    {
        if (pedestrianList != null)
        {
            foreach (GameObject pedestrian in pedestrianList)
                Destroy(pedestrian);
        }

        pedestrianList = new List<GameObject>();
    }
}
