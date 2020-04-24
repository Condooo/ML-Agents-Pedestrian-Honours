using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CollisionDataPlacement : MonoBehaviour
{
    private string path = "Assets/Resources/collision_Positions.txt";

    public float radius = 1.0f;
    public float intensity = 1.0f;
    private int count = 0;

    private List<Vector3> posList;
    public Vector4[] positions;
    public float[] radiuses;
    public float[] intensities;
    Vector4[] properties;
    public Material material;

    /// <summary>
    /// Read collision data from file, use position data to create heatmap to heatmap material shader for evaluation
    /// </summary>
    void Start()
    {
        Vector3 inputPos = new Vector3();

        string inputLine = "";

        if (File.Exists(path))
        {
            StreamReader reader = new StreamReader(path);
            posList = new List<Vector3>();

            while (!reader.EndOfStream)
            {
                inputLine = reader.ReadLine();
                string[] splitLine = inputLine.Split(',');
                for (int i = 0; i < splitLine.Length; i++)
                    splitLine[i] = splitLine[i].Trim();
                inputPos.Set(float.Parse(splitLine[0]), float.Parse(splitLine[1]), float.Parse(splitLine[2]));
                count++;
                posList.Add(inputPos);
            }


            string result = reader.ReadLine();
            reader.Close();

            positions = new Vector4[count];
            radiuses = new float[count];
            intensities = new float[count];
            properties = new Vector4[count];

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = new Vector4(posList[i].x, posList[i].y, posList[i].z);
                radiuses[i] = radius;
                intensities[i] = intensity;
                properties[i] = new Vector4(radiuses[i], intensities[i], 0, 0);
            }

            material.SetInt("_Points_Length", positions.Length);

            material.SetVectorArray("_Points", positions);
            material.SetVectorArray("_Properties", properties);
        }

    }

    /// <summary>
    /// Update heatmap properties if changed in editor
    /// </summary>
    void Update()
    {
        material.SetInt("_Points_Length", positions.Length);
        for (int i = 0; i < positions.Length; i++)
        {
            radiuses[i] = radius;
            intensities[i] = intensity;
            properties[i] = new Vector4(radiuses[i], intensities[i], 0, 0);
        }

        material.SetVectorArray("_Points", positions);
        material.SetVectorArray("_Properties", properties);
    }
}
