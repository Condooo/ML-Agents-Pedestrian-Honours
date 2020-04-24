using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    [Tooltip("The speed the car moves forward at")]
    private float maxSpeed;
    private float movementSpeed = 0;
    [Tooltip("The turning speed of the car")]
    public float turnSpeed = 120f;
    public float trackingStartSpeed = 0;
    private float turnValue = 0;
    private int pedestriansHit = 0;
    private int checkpointsHit = 0;

    private PedestrianArea pedestrianArea;
    new private Rigidbody agentRB;

    private TrainingData dataManager;

    /// <summary>
    /// Obtain the training data manager for training data storage
    /// </summary>
    void Start()
    {
        dataManager = GameObject.FindGameObjectWithTag("TrainingManager").GetComponent<TrainingData>();
    }

    /// <summary>
    /// Return the current count of pedestrians hit
    /// </summary>
    /// <returns>Total number of pedestrians hit at this point</returns>
    public int GetPedestriansHit()
    {
        return pedestriansHit;
    }

    /// <summary>
    /// Return the current count of checkpoints remaining
    /// </summary>
    /// <returns>Total number of checkpoints still active</returns>
    public int GetCheckpointsHit()
    {
        return checkpointsHit;
    }

    /// <summary>
    /// Return the current maximum speed of the agent
    /// </summary>
    /// <returns>Maximum speed value</returns>
    public float GetSpeed()
    {
        return maxSpeed;
    }

    /// <summary>
    /// Set all agent's movement and rotation to zero
    /// </summary>
    public void ResetAgent()
    {
        turnValue = 0;
        movementSpeed = 0;
        maxSpeed = Academy.Instance.FloatProperties.GetPropertyWithDefault("movement_speed", 5.0f);
    }

    /// <summary>
    /// Initializes agent when enabled
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        pedestrianArea = GetComponentInParent<PedestrianArea>();
        pedestrianArea.agent = this;
        maxSpeed = Academy.Instance.FloatProperties.GetPropertyWithDefault("movement_speed", 5.0f);
        agentRB = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Perform actions determined by the action vector values
    /// </summary>
    /// <param name="vectorAction">List of actions to take with respective values</param>
    public override void AgentAction(float[] vectorAction)
    {
        // Current speed as percentage of maximum
        float speedMult = movementSpeed / maxSpeed;

        // Determine forward movement
        float forwardValue = vectorAction[0];
        if (forwardValue > 0)
        {
            movementSpeed += (10 * Time.fixedDeltaTime);
            if (movementSpeed > maxSpeed)
                movementSpeed = maxSpeed;
            speedMult = movementSpeed / maxSpeed;
            AddReward(0.001f * speedMult);
        }

        // Determine rotation
        if (vectorAction[1] == 1.0f)    // Left
        {
            turnValue -= Time.fixedDeltaTime;
            float turnMax = -1.0f;
            if (turnValue < turnMax)
                turnValue = turnMax;
            float turnPercent = turnValue / turnMax;
            AddReward(-0.01f * turnPercent * speedMult);
        }
        else if (vectorAction[1] == 2.0f)   // Right
        {
            turnValue += Time.fixedDeltaTime;
            float turnMax = 1.0f;
            if (turnValue > turnMax)
                turnValue = turnMax;
            float turnPercent = turnValue / turnMax;
            AddReward(-0.01f * turnPercent * speedMult);
        }

        // Apply movement
        agentRB.MovePosition(transform.position + transform.forward * movementSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnValue * (turnSpeed * speedMult) * Time.fixedDeltaTime);

        // Decelerate
        movementSpeed *= 0.95f;
        if (movementSpeed > 0)
            turnValue *= 0.95f;

        // Encourage forward movement
        if (maxStep > 0)
            AddReward(-1f / maxStep);

        // Reset area at max step count
        if (GetStepCount() >= maxStep)
        {
            pedestrianArea.ResetArea();
        }
    }

    /// <summary>
    /// Request agent action and decision on step counts and begin tracking data when tracking speed reached
    /// </summary>
    private void FixedUpdate()
    {
        if (!dataManager.timeActive && maxSpeed >= trackingStartSpeed)
            dataManager.timeActive = true;
        if (GetStepCount() % 5 == 0)
            RequestDecision();
        else
            RequestAction();
    }

    /// <summary>
    /// Detect collision with pedestrians or boundaries and reset the training environment
    /// </summary>
    /// <param name="collision">Object collided with</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Pedestrian"))
        {
            pedestrianArea.collisionPositionList.Add(transform.localPosition);
            pedestrianArea.AddCollision(collision.gameObject.name);
            AddReward(-1f);
            if (maxSpeed >= trackingStartSpeed)
                pedestriansHit++;
            pedestrianArea.ResetArea();
        }
        if (collision.transform.CompareTag("CarBoundary"))
        {
            //AddReward(-1f);
            pedestrianArea.ResetArea();
        }
    }

    /// <summary>
    /// Detect collision with checkpoint triggers and give reward
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Checkpoint"))
        {
            if (maxSpeed >= trackingStartSpeed)
                checkpointsHit++;
            AddReward(1f);
            Transform checkpointHolder = other.transform.parent;
            foreach(Transform child in checkpointHolder)
            {
                child.gameObject.SetActive(false);
            }
            pedestrianArea.AdjustRemainingCheckpoints(-1);
        }
        if (other.transform.CompareTag("CheckpointMini"))
        {
            Transform checkpointHolder = other.transform.parent;
            AddReward(0.1f);
            foreach (Transform child in checkpointHolder)
            {
                child.gameObject.SetActive(false);
            }
            pedestrianArea.AdjustRemainingCheckpoints(-1);
        }

        // No checkpoints remaining = lap complete, reward given
        if (pedestrianArea.GetRemainingCheckpoints() <= 0)
        {
            AddReward(1f);
            pedestrianArea.ResetArea();
        }
    }

    /// <summary>
    /// Allow for direct user control
    /// </summary>
    /// <returns>VectorArray used to determine agent action</returns>
    public override float[] Heuristic()
    {
        float forwardValue = 0f;
        float turnValue = 0f;
        if (Input.GetKey(KeyCode.W))
            forwardValue = 1.0f;
        if (Input.GetKey(KeyCode.A))
            turnValue = 1.0f;
        if (Input.GetKey(KeyCode.D))
            turnValue = 2.0f;

        // Vector array passed to AgentAction to determine action performed
        return new float[] { forwardValue, turnValue };
    }
}
