using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject[] allModels;
    public GameObject[] successfullModels;
    public GameObject[] unsuccessfulModels;
    public GameObject[] darkModels;

    public Dropdown dropdown;
    public Dropdown cameraDropdown;

    private List<PedestrianSpawner> spawners;
    private List<PedestrianArea> trainingAreas;

    public Camera followCamera;
    public Camera topCamera;

    public GameObject[] summaries;

    /// <summary>
    /// Obtain all spawners in all training areas and set the main camera
    /// </summary>
    void Start()
    {
        spawners = new List<PedestrianSpawner>();
        trainingAreas = new List<PedestrianArea>();

        followCamera.enabled = true;
        topCamera.enabled = false;

        GameObject[] areaObjects = GameObject.FindGameObjectsWithTag("TrainingArea");
        foreach(GameObject area in areaObjects)
        {
            trainingAreas.Add(area.GetComponent<PedestrianArea>());
        }
        

        GameObject[] crossingSpawners = GameObject.FindGameObjectsWithTag("CrossingSpawner");
        GameObject[] curveSpawners = GameObject.FindGameObjectsWithTag("CurveSpawner");
        GameObject[] singleSpawners = GameObject.FindGameObjectsWithTag("SingleSpawner");

        foreach (GameObject spawner in crossingSpawners)
        {
            spawners.Add(spawner.GetComponent<PedestrianSpawner>());
        }
        foreach(GameObject spawner in curveSpawners)
        {
            spawners.Add(spawner.GetComponent<PedestrianSpawner>());
        }
        foreach (GameObject spawner in singleSpawners)
        {
            spawners.Add(spawner.GetComponent<PedestrianSpawner>());
        }

    }

    /// <summary>
    /// Toggle the active camera depending on dropdown selection
    /// </summary>
    public void SelectCamera()
    {
        switch (cameraDropdown.value)
        {
            case 0:
                followCamera.enabled = true;
                topCamera.enabled = false;
                break;
            case 1:
                topCamera.enabled = true;
                followCamera.enabled = false;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Set all spawner prefabs to the dropdown selection
    /// </summary>
    public void SetPedestrianModels()
    {
        GameObject[] selectedModels = null;

        switch (dropdown.value)
        {
            case 0:
                selectedModels = allModels;
                break;
            case 1:
                selectedModels = successfullModels;
                break;
            case 2:
                selectedModels = unsuccessfulModels;
                break;
            case 3:
                selectedModels = darkModels;
                break;
            default: break;
        }

        for (int i = 0; i < summaries.Length; i++)
        {
            if (dropdown.value == i)
                summaries[i].SetActive(true);
            else
                summaries[i].SetActive(false);
        }

        if (selectedModels != null)
        {
            foreach (PedestrianSpawner spawner in spawners)
            {
                spawner.SetPrefabs(selectedModels);
            }
        }
        else
        {
            Debug.LogError("selectedModels returned null");
        }

        foreach(PedestrianArea trainingArea in trainingAreas)
        {
            trainingArea.ResetArea();
        }

    }
}
