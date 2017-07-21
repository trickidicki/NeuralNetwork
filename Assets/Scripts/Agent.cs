using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour
{
    public bool hasFailed = false;
    private float headingAngle; //Degrees

    //public NNet neuralnet;
    public Genome genome;
    public RayCast raycast;

    public float MAX_ROTATION; //max rotate speed
    public float _SPEED;

    public float leftForce;
    public float rightForce;
    public float leftTheta;
    public float rightTheta;

    hit hit;
    int framecount;
    float fitness;
    float distanceTravelled;
    Vector3 lastPoint;
    float totalRot;
    float cummulativeAngle;
    bool selfDrive = false;

    // Use this for initialization
    void Start()
    {
        hit = gameObject.GetComponent<hit>();
        ClearFailure();

        raycast = gameObject.GetComponent<RayCast>();

        //neuralnet.CreateNet(2, raycast.rayCount, 12, 2);

        leftForce = 0.0f;
        rightForce = 0.0f;
        leftTheta = 0.0f;
        rightTheta = 0.0f;
    }

    void Restart()
    {
        ClearFailure();
    }

    // Update is called once per frame
    void Update()
    {
        if (!selfDrive)
        {
            UpdateFitness();
            if (CheckFailure())
                return;
        }

        if( genome == null)
        {
            return;
        }

        if (selfDrive || !hasFailed)
        {
            genome.net.SetInput(raycast.GetProbes());
            genome.net.refresh();

            leftForce = genome.net.GetOutput(0);
            rightForce = genome.net.GetOutput(1);

            leftTheta = MAX_ROTATION * leftForce;
            rightTheta = MAX_ROTATION * rightForce;

            headingAngle += (leftTheta - rightTheta) * Time.fixedDeltaTime;

            float speed = (Mathf.Abs(leftForce + rightForce)) / 2;
            speed *= _SPEED;

            speed = Clamp(speed, -_SPEED, _SPEED);
        }
    }

    private int ElapsedTime()
    {
        return Time.frameCount - framecount;
    }

    private void UpdateRotation()
    {
        Vector3 facing = transform.TransformDirection(Vector3.forward);
        facing.y = 0;

        float angle = Vector3.Angle(lastPoint, facing);
        cummulativeAngle += Mathf.Abs(angle);
        //if (Vector3.Cross(lastPoint, facing).y < 0)
        //    angle *= -1;

        totalRot += angle;
        lastPoint = facing;
    }

    private void UpdateFitness()
    {
        distanceTravelled += Vector3.Distance(transform.position, lastPoint);
        UpdateRotation();

        int elapsedTime = ElapsedTime();
        if (elapsedTime > 0)
        {
            fitness = elapsedTime + distanceTravelled / elapsedTime;
            var angular = 0.2f * Mathf.Abs(totalRot / cummulativeAngle);
            fitness *= angular; // Peanalise extra angular change
        }

        fitness += hit.checkpoints * 10;
    }

    public float GetFitness()
    {
        return fitness;
    }

    private bool CheckFailure()
    {
        hasFailed = hit.crash || Mathf.Abs(totalRot) > 1080.0f;
        return hasFailed;
    }

    //public void Attach(NNet net, bool justDrive = false)
    public void Attach(Genome genome, bool justDrive = false)
    {
        this.genome = genome;
        this.ClearFailure();
        if (justDrive)
        {
            selfDrive = true;
        }
    }

    public void ClearFailure()
    {
        hasFailed = false;
        if (hit != null)
        {
            hit.crash = false;
            hit.checkpoints = 0;
        }
        distanceTravelled = 0.0f;
        framecount = Time.frameCount;

        totalRot = 0.0f;
        cummulativeAngle = 0.0f;
        lastPoint = transform.TransformDirection(Vector3.forward);
        lastPoint.y = 0;
        headingAngle = 0.0f;
    }

    public float Clamp(float val, float min, float max)
    {
        if (val < min)
        {
            return min;
        }
        if (val > max)
        {
            return max;
        }
        return val;
    }
}

