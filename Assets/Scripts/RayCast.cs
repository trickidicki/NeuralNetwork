using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RayCast : MonoBehaviour
{
    public float RayCast_Length = 5.0f;

    public int rayCount = 7;
    public RaycastHit[] rays;
    public float[] hits;
    private float segmentAngle;

    private BoxCollider box;

    // Use this for initialization
    void Start()
    {
        box = GetComponent<BoxCollider>();

        rays = new RaycastHit[rayCount];
        hits = new float[rayCount];

        if (rayCount <= 1)
            rayCount = 5;

        segmentAngle = 180.0f / (rayCount - 1);
    }

    // Update is called once per frame
    void Update()
    {
        var origin = transform.TransformPoint(box.center);
        for (var n = 0; n < rayCount; n++)
        {
            float angle = -90.0f + n * segmentAngle;
            Vector3 vec = Quaternion.AngleAxis(angle, transform.up) * -transform.forward;
            hits[n] = CastRay(origin, vec, n);
        }
    }

    public List<float> GetProbes()
    {
        return new List<float>(hits);
    }

    float CastRay(Vector3 origin, Vector3 vec, int n)
    {
        RaycastHit hitInfo;
        bool col = Physics.Raycast(origin, vec, out hitInfo);
        Color color = Color.HSVToRGB((float)n / rayCount, 1f, 1f);
        Debug.DrawLine(origin, hitInfo.point, color, 0f, true);
        return Normalise(hitInfo.distance);
    }

    public float Normalise(float i)
    {
        return 1 - (i > RayCast_Length ? RayCast_Length : i) / RayCast_Length;     //Clamp maximum depth
    }
}