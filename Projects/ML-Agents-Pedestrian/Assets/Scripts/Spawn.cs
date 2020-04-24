using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    private PedestrianArea trainingArea;

    /// <summary>
    /// Assign the current spawner to the parent training environment
    /// </summary>
    void Start()
    {
        trainingArea = this.transform.root.GetComponent<PedestrianArea>();
        trainingArea.agentSpawnList.Add(this.transform);
    }
}
