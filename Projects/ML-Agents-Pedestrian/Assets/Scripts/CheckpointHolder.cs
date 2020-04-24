using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHolder : MonoBehaviour
{
    private PedestrianArea trainingArea;

    /// <summary>
    /// Obtain the parent training area and assign this object to its checkpoint list
    /// </summary>
    void Start()
    {
        trainingArea = this.transform.root.GetComponent<PedestrianArea>();
        trainingArea.checkpointList.Add(this.gameObject);
    }
}
