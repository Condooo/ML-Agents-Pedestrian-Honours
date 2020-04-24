using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrainingData : MonoBehaviour
{
    public static GameObject[] trainingAreas;
    public static List<Vector3> collisionPositions;
    public static Dictionary<string, int> collisionInfo;
    public float simStopTime = 0;
    public bool timeActive = false;

    public float simTime = 0;

    /// <summary>
    /// Obtain training environments and load collision position information if available
    /// </summary>
    private void Start()
    {
        trainingAreas = GameObject.FindGameObjectsWithTag("TrainingArea");
        collisionPositions = new List<Vector3>();
        collisionInfo = new Dictionary<string, int>();
        if (File.Exists("Assets/Resources/collision_Positions.txt"))
            LoadCollisionPositions();
    }

    /// <summary>
    /// Parse previous collision position information from file and store in collisionPositions list
    /// </summary>
    private void LoadCollisionPositions()
    {
        string path = "Assets/Resources/collision_Positions.txt";
        StreamReader reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            var pt = line.Split(","[0]);
            var x = float.Parse(pt[0]);
            var y = float.Parse(pt[1]);
            var z = float.Parse(pt[2]);
            Vector3 position = new Vector3(x, y, z);
            collisionPositions.Add(position);
        }
        reader.Close();
        foreach(Vector3 position in collisionPositions)
        {
            Debug.Log(position);
        }
    }

    /// <summary>
    /// Populates collision data and position lists with values from training environments
    /// </summary>
    public static void PopulateLists()
    {
        trainingAreas = GameObject.FindGameObjectsWithTag("TrainingArea");
        // COLLISION INFO
        if (collisionInfo.Count <= 0)
        {
            List<GameObject> prefabs = GameObject.FindGameObjectWithTag("SingleSpawner").GetComponent<PedestrianSpawner>().GetPrefabs();
            if (prefabs.Count > 0)
            {
                foreach (GameObject prefab in prefabs)
                {
                    collisionInfo.Add(prefab.name, 0);
                }
            }
            for (int i = 0; i < trainingAreas.Length; i++)
            {
                PedestrianArea area = trainingAreas[i].GetComponent<PedestrianArea>();
                // COLLISION POSITIONS
                foreach (Vector3 position in area.collisionPositionList)
                    collisionPositions.Add(position);
                // COLLISION INFO
                foreach (KeyValuePair<string, int> collision in area.collisionList)
                {
                    int value = collisionInfo[collision.Key];
                    int newValue = area.collisionList[collision.Key];
                    value += newValue;
                    collisionInfo[collision.Key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Manually save data on input, or automaticaly save and stop playing once simulation stop time reached
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            Save();
        if (timeActive)
            simTime += Time.deltaTime;

        if (simTime >= simStopTime)
        {
            Save();
            //UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    /// <summary>
    /// Save collision data to file
    /// </summary>
    public void SaveData()
    {
        string path = "Assets/Resources/training_Data.txt";

        // Write text
        StreamWriter writer = new StreamWriter(path);
        foreach (KeyValuePair<string,int> pair in collisionInfo)
        {
            writer.WriteLine(pair.Key + ": " + pair.Value);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// Populate data lists and perform all save functions
    /// </summary>
    public void Save()
    {
        PopulateLists();
        SaveData();
        SavePositions();
        SaveCollisionRate();
    }

    /// <summary>
    /// Adds together all pedestrian and checkpoint collisions across training areas and determines collision rate, by number of checkpoints hit to each pedestrian, and saves to file
    /// </summary>
    public void SaveCollisionRate() {
        string path = "Assets/Resources/collision_Rate.txt";

        int pedestriansHit = 0;
        int checkpointsHit = 0;
        float speed = 0;

        StreamWriter writer = new StreamWriter(path);
        foreach (GameObject area in trainingAreas)
        {
            CarAgent agent = area.GetComponent<PedestrianArea>().agent;
            pedestriansHit += agent.GetPedestriansHit();
            checkpointsHit += agent.GetCheckpointsHit();
            speed = agent.GetSpeed();
        }

        writer.WriteLine("Agent speed: " + speed);
        writer.WriteLine("Pedestrian collision: " + pedestriansHit);
        writer.WriteLine("Checkpoints hit: " + checkpointsHit);
        float collisionRate;
        if (pedestriansHit > 0)
        {
            collisionRate = (float)checkpointsHit / (float)pedestriansHit;
            writer.WriteLine("Checkpoint/pedestrian rate: " + collisionRate);
        }
        else
        {
            writer.WriteLine("Checkpoint/pedestrian rate: NO PEDESTRIANS HIT");
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SavePositions()
    {
        string path = "Assets/Resources/collision_Positions.txt";

        StreamWriter writer = new StreamWriter(path);
        foreach (Vector3 position in collisionPositions)
        {
            string positionString = position.ToString();
            positionString = positionString.Substring(1, positionString.Length - 2);
            writer.WriteLine(positionString);
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// Perform all save functions when application closed
    /// </summary>
    private void OnApplicationQuit()
    {
        Save();
    }
}
