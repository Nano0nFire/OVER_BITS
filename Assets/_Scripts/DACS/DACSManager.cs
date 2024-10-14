using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Loading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class DACSManager : MonoBehaviour
{
    // [SerializeField] private DACSScriptableObject DACSSO;
    // [SerializeField] private Transform StartPoint;
    // private int DACSID = 0;
    // private DACSConfigs DACSConfig;
    // private GameObject[] SpawnObj = new GameObject[100];
    // private Queue<GameObject> objectPool = new Queue<GameObject>();
    // private bool canAttack = true, isPressing;
    // private int count, objCount;
    // private float pressTime = 0;
    // private ProjectileController PC;
    // private FireTypes FireType;

    // private void Start()
    // {        

    // }
    
    // public void Update()
    // {
    //     if (!isPressing) return;
    //     else
    //     {
    //         pressTime += Time.deltaTime;

    //         if (canAttack) 
    //         {
    //             canAttack = false;
    //             StartCoroutine(SetProjectileCoroutine(DACSID));
    //         }
    //     }
    // }
    // // プールからオブジェクトを取得する
    // public GameObject GetObjectFromPool()
    // {
    //     if (objectPool.Count == 0)
    //     {
    //         GameObject obj = Instantiate(DACSConfig.ProjectileObject, transform);
    //         return obj;
    //     }
    //     else
    //     {    
    //         GameObject Obj = objectPool.Dequeue();
    //         Obj.transform.position = transform.position;
    //         Obj.SetActive(true);
    //         return Obj;
    //     }
    // }
    // public void ReturnObjectToPool(GameObject obj)
    // {
    //     obj.SetActive(false);
    //     objectPool.Enqueue(obj);
    // }


    // private IEnumerator SetProjectileCoroutine(int ID)
    // {
    //     int PA = DACSSO.DACSData[ID].ProjectileAmount;
    //     int BA = DACSSO.DACSData[ID].BurstAmount;
    //     float FR = DACSSO.DACSData[ID].FireRate;
    //     float BR = DACSSO.DACSData[ID].BurstRate;

    //     DACSID = ID;

    //     objCount = 0;
        
    //     switch (FireType)
    //     {
    //         case FireTypes.Fullauto:
    //             for (count = 0; count < PA; count++) SetProjectile(objCount);
    //             break;
    //         case FireTypes.Burst:
    //             for (int BurstCount = 0; BurstCount < BA; BurstCount++) 
    //             {
    //                 for (count = 0; count < PA; count++) SetProjectile(objCount);
    //                 yield return new WaitForSeconds(BR);
    //             }
    //             break;
    //         case FireTypes.Semiauto:
    //             for (count = 0; count < PA; count++) SetProjectile(objCount);
    //             break;
    //         case FireTypes.ChargeAndSingle:
    //             objCount = 0;
    //             SetProjectile(objCount);
    //             PC.isSetting = true;
    //             yield return new WaitUntil(() => !isPressing);
    //             PC.StartMove();
    //             break;
    //         case FireTypes.ChargeAndMulti:
    //             do
    //             {
    //             if (pressTime > DACSConfig.ChargePerProjectile && objCount < DACSConfig.MaxCharge) //objCount < 10で一度にスタックできる弾の数を制限
    //             {
    //                     SetProjectile(objCount); 
    //                     PC.isSetting = true;

    //                     objCount ++;

    //                     pressTime = 0;
    //             }

    //             yield return new WaitForSeconds(Time.deltaTime);
    //             }while (isPressing);
                
    //             for (int i = 0; i < objCount; i++)
    //             {
    //                 PC = SpawnObj[i].GetComponent<ProjectileController>();
    //                 PC.isSetting = false;
    //                 PC.StartMove();
    //                 yield return new WaitForSeconds(BR);
    //             }
    //             break;
    //         case FireTypes.SetAndShot:
    //             objCount = 0;
    //             SetProjectile(objCount);
    //             PC.isSetting = true;
    //             yield return new WaitUntil(() => !isPressing);
    //             PC.StartMove();
    //             break;
    //     }
    //     yield return new WaitForSeconds(FR);
    //     canAttack = true;
    // }

    // private void SetProjectile(int objectCount)
    // {
    //     SpawnObj[objectCount] = GetObjectFromPool();
    //     PC = SpawnObj[objectCount].GetComponent<ProjectileController>();
    //     PC.SetProjectile(StartPoint, GetComponent<DACSManager>(), DACSConfig, DACSSO, DACSID);
    //     if (DACSConfig.UseTrail) PC.SetTrail();
    // }

}
