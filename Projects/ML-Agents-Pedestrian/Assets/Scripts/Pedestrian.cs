using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class Pedestrian : MonoBehaviour
{
    GameObject pedestrian;
    Animator anim;
    [SerializeField]
    float movementSpeed;
    float counter = 0;
    float destroyTime = 20;
    PedestrianArea trainingArea;

    /// <summary>
    /// Set animation speed and parent training area
    /// </summary>
    void Start()
    {
        pedestrian = this.gameObject;
        anim = GetComponent<Animator>();
        movementSpeed = 1.0f * (1- (Random.Range(-0.3f, 0.3f) * Academy.Instance.FloatProperties.GetPropertyWithDefault("pedestrian_speed_randomizer", 0.0f)));
        anim.SetFloat("AnimMultiplier", movementSpeed);
        pedestrian.GetComponent<CapsuleCollider>().radius = 0.25f;
        trainingArea = this.transform.root.GetComponent<PedestrianArea>();
    }

    /// <summary>
    /// Update pedestrian position and increment counter; destroy pedestrian once timer reached
    /// </summary>
    void Update()
    {
        pedestrian.transform.position += pedestrian.transform.forward * Time.deltaTime * movementSpeed;
        anim.SetFloat("AnimMultiplier", movementSpeed);
        counter += Time.deltaTime;
        if (counter > destroyTime)
        {
            trainingArea.pedestrianList.Remove(this.gameObject);
            Destroy(this.gameObject);
        }

    }

    // Unused, but has functionality if deciding to implement in future
    /// <summary>
    /// Determine whether the pedestrian collides with a boundary and destroy the pedestrian
    /// </summary>
    /// <param name="other">Object collided with</param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered");
        if (other.tag == "Boundary")
        {
            trainingArea.pedestrianList.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
