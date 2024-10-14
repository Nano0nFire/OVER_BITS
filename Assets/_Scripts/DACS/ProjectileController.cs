using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEditor;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private DACSObjectPooler objectPool;
    public Vector2 ProjectileSpread;
    public float ProjectileDropForce, chargeTime;
    private Vector3 startPosition;
    private TrailRenderer trailRenderer;
    private RaycastHit hit;
    private DACS_P_Configs configs;
    private Transform StartPoint;
    public bool isEnable = false;
    public bool isSetting;
    public int ProjectileID;
    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }
    
    public void SetProjectile(Transform startPoint, DACS_P_Configs config, DACSObjectPooler pooler)
    {
        StartPoint = startPoint;
        configs = config;
        objectPool = pooler;
        isEnable = true;
        isSetting = false;
    }
    
    public void SetDir()
    {
        startPosition = StartPoint.transform.position + StartPoint.transform.right * configs.hzOffset + StartPoint.transform.up * configs.vOffset;
        transform.SetPositionAndRotation(startPosition, StartPoint.rotation);
    }

    void Update()
    {
        if (!isEnable && !isSetting) return;
        if (isSetting) SetDir();
        if (Physics.SphereCast(transform.position, configs.ColRadius, transform.forward, out hit, configs.ColMaxDistance))
        {
            DisableObject();
        }
        else return;
    }

    public void DisableObject()
    {
        objectPool.ReturnObjectToPool(gameObject);
        trailRenderer.Clear(); 
        isEnable = false;
        isSetting = false;
    }
}

