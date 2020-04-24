using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PedestrianSpawner : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> prefabs;
    [SerializeField]
    private float spawnTimer = 1;
    private float counter = 0;
    [SerializeField]
    [Range(0,1)]
    public float rotationRandomizerMultiplier = 0;
    [SerializeField]
    [Range(0, 100)]
    public float percentActive = 100;
    private GameObject trainingAreaObject;
    private PedestrianArea trainingArea;
    private float spawnTimerStart;

    /// <summary>
    /// Add this object to the parent training environment's pedestrian spawner list and obtain initial values
    /// </summary>
    void Start()
    {
        trainingAreaObject = this.transform.root.gameObject;
        trainingArea = trainingAreaObject.GetComponent<PedestrianArea>();
        trainingArea.pedestrianSpawnerList.Add(this.gameObject);
        spawnTimerStart = spawnTimer;
        rotationRandomizerMultiplier = Academy.Instance.FloatProperties.GetPropertyWithDefault("pedestrian_rotation_randomizer", 1.0f);
    }

    /// <summary>
    /// Return an array of the current pedestrian prefabs
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetPrefabs()
    {
        return prefabs;
    }

    /// <summary>
    /// Replace the pedestrian model list
    /// </summary>
    /// <param name="models">Prefab models to replace current pedestrian prefab list</param>
    public void SetPrefabs(GameObject[] models){
        prefabs.Clear();
        foreach(GameObject model in models)
        {
            prefabs.Add(model);
        }
    }


    /// <summary>
    /// Spawn a random pedestrian model with a random rotation within the allowed variance.
    /// </summary>
    public void SpawnPedestrian()
    {
        // Determine pedestrian model to spawn
        int rng = Random.Range(0, prefabs.Count);

        // Randomise pedestrian rotation
        float rotY = this.transform.rotation.eulerAngles.y;
        if (this.gameObject.CompareTag("CrossingSpawner"))
            rotY += Random.Range(-5 * rotationRandomizerMultiplier, 5 * rotationRandomizerMultiplier);
        else
            rotY += Random.Range(-10 * rotationRandomizerMultiplier, 10 * rotationRandomizerMultiplier);
        Quaternion rotation = Quaternion.Euler(this.transform.localRotation.x, rotY, this.transform.localRotation.z);

        // Instantiate a new pedestrian object
        GameObject pedestrian = Instantiate<GameObject>(prefabs[rng], this.transform.position, rotation);
        pedestrian.name = pedestrian.name.Replace("(Clone)", "");
        pedestrian.transform.localScale *= Random.Range(0.85f, 1.0f);
        pedestrian.transform.parent = trainingAreaObject.transform;

        // Add newly spawned pedestrian to list of current pedestrians
        trainingArea.pedestrianList.Add(pedestrian);
    }

    /// <summary>
    /// Call UpdateSpawner each frame
    /// </summary>
    void Update()
    {
        UpdateSpawner();
    }

    /// <summary>
    /// Increment counter and spawn a new pedestrian when the spawn timer is reached.
    /// </summary>
    private void UpdateSpawner()
    {
        counter += Time.deltaTime;
        if (counter >= spawnTimer)
        {
            if (prefabs.Count > 0 && Vector3.Distance(this.transform.position, trainingArea.agent.gameObject.transform.position) > 10)
                SpawnPedestrian();
            counter = 0;
            spawnTimer = spawnTimerStart * Random.Range(0.5f, 1.5f);
        }
    }
}
