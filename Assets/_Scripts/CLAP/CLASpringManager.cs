using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CLASpringManager : MonoBehaviour
{
    [SerializeField] List<HairSettings> hairSettings;
    public float hzMovement{ get; private set; }
    public float vMovement{ get; private set; }
    [SerializeField] float hzSpeed, vSpeed, ySpeed;
    [SerializeField] Rigidbody rb;


    void Start()
    {
        StartSetting();
    }

    void Update()
    {
        SuspectPhysics();
        Debug.Log(vSpeed);
    }
    
    void StartSetting()
    {
        foreach (var hs in hairSettings)
        {
            if (hs.UseParent) SetComponent(hs.target, hs.target.parent, hs.rotationRange);
            else SetComponent(hs.target, hs.parent, hs.rotationRange);
        }
    }

    void SetComponent(Transform target, Transform pivotPoint, float rotationRange)
    {
        CLASpringSystem ss = target.AddComponent<CLASpringSystem>();

        ss.pivotPoint = pivotPoint;
        ss.rotationRange = rotationRange;
        ss.manager = GetComponent<CLASpringManager>();
    }

    void SuspectPhysics()
    {
        vSpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.forward) * 100) / 100;
        hzSpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.right) * 100) / 100;
        ySpeed = Mathf.Round(Vector3.Dot(rb.linearVelocity, transform.up) * 100) / 100;


    }
}

class HairSettings
{
    public Transform target;
    public bool UseParent = true;
    public Transform parent = null;
    public float rotationRange = 0.1f;
}
