using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DACSMainController : MonoBehaviour
{
    [SerializeField] private DACS_P_ScriptableObject ScriptableObject;
    [SerializeField] private DACSObjectPooler ObjectPooler;
    [SerializeField] private GameObject Player;
    [SerializeField] private int UseID = 0;
    private Dictionary<GameObject, bool> canShot = new();
    private Dictionary<GameObject, bool> isPressing = new();
    private Dictionary<GameObject, float> pressTime = new();
    private ProjectileController PC;


    public void Update()
    {
        // {
        //     pressTime += Time.deltaTime;

        //     if (canShot)
        //     {
        //         canShot = false;
        //         StartCoroutine(GetProjectileCoroutine());
        //         Debug.Log("Set");
        //     }
        // }
    }
    public void SpawnProjectile (InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartProjectile(Player, UseID);
        }
        if (context.canceled)
        {
            EndProjectile(Player);
        }
    }

    public void StartProjectile(GameObject User, int ID)
    {
        DACS_P_Configs configs = ScriptableObject.P_ScriptableObject[ID];

        if
        (
            configs.FireType == FireTypes.Fullauto ||
            configs.FireType == FireTypes.ChargeAndSingle ||
            configs.FireType == FireTypes.ChargeAndMulti ||
            configs.FireType == FireTypes.SetAndShot
        )
        {
            isPressing[User] = true;
            StartCoroutine(GetProjectileCoroutine(ID, User));
        }

        else if (canShot[User])
        {
            canShot[User] = false;
            StartCoroutine(GetProjectileCoroutine(ID, User));
        }
    }

    public void EndProjectile(GameObject User)
    {
        isPressing[User] = false;
        pressTime[User] = 0;
    }

     private IEnumerator GetProjectileCoroutine(int ID, GameObject User)
    {
        int PA, BA, BurstCount;
        GameObject curObj;
        float FR, BR;
        float t= 0;
        DACS_P_Configs configs = ScriptableObject.P_ScriptableObject[ID];
        FireTypes fireTypes = configs.FireType;

        Debug.Log("MoveCo");

        PA = configs.ProjectileAmount;
        FR = configs.FireRate;
        BA = configs.BurstAmount;
        BR = configs.BurstRate;

        switch (fireTypes)
        {
            case FireTypes.Fullauto:
                do{
                    for (int count = 0; count < PA; count++) StartCoroutine(MoveCoroutine(GetProjectile(ID, User), configs, User));

                    yield return new WaitForSeconds(FR);
                }while (isPressing[User]);
                break;

            case FireTypes.Semiauto:
                for (int count = 0; count < PA; count++) StartCoroutine(MoveCoroutine(GetProjectile(ID, User), configs, User));
                break;

            case FireTypes.Burst:
                for (BurstCount = 0; BurstCount < BA; BurstCount++)
                {
                    for (int count = 0; count < PA; count++) StartCoroutine(MoveCoroutine(GetProjectile(ID, User), configs, User));
                    yield return new WaitForSeconds(BR);
                }
                break;

            case FireTypes.ChargeAndSingle:
                curObj = GetProjectile(ID, User);
                curObj.GetComponent<ProjectileController>().isSetting = true;
                do{
                    t += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }while (isPressing[User]);
                pressTime[User] = t;
                Debug.Log("A" + pressTime[User]);
                StartCoroutine(MoveCoroutine(curObj, configs, User));
                break;

            case FireTypes.ChargeAndMulti:
                int objCount = 0;
                GameObject[] CurObj = new GameObject[configs.MaxCharge];

                do
                {
                    if (pressTime[User] > configs.ChargePerProjectile && objCount < configs.MaxCharge) //objCount < MaxChargeで一度にスタックできる弾の数を制限
                    {
                        CurObj[objCount] = GetProjectile(ID, User);
                        CurObj[objCount].GetComponent<ProjectileController>().isSetting = true;

                        objCount ++;

                        pressTime[User] = 0;
                    }
                    else pressTime[User] += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }while (isPressing[User]);
                Debug.Log("A" + pressTime[User]);

                for (int i = 0; i < objCount; i++)
                {
                    StartCoroutine(MoveCoroutine(CurObj[i], configs, User));
                    yield return new WaitForSeconds(BR);
                }
                break;

            case FireTypes.SetAndShot:
                objCount = 0;
                GetProjectile(ID, User);
                yield return new WaitUntil(() => !isPressing[User]);
                break;
        }

        yield return new WaitForSeconds(FR);
        canShot[User] = true;
    }

    private GameObject GetProjectile(int ID, GameObject User)
    {
        GameObject SpawnO = ObjectPooler.GetObjectFromPool(ID, User.transform);
        SpawnO.transform.SetLocalPositionAndRotation(User.transform.position, User.transform.rotation);
        SpawnO.GetComponent<ProjectileController>().SetProjectile(User.transform, ScriptableObject.P_ScriptableObject[ID], ObjectPooler);

        return SpawnO;
    }

    private IEnumerator MoveCoroutine(GameObject Obj, DACS_P_Configs configs, GameObject User)
    {
        if (configs.UseTrail) SetTrail(Obj, configs);
        ProjectileController PC = Obj.GetComponent<ProjectileController>();

        Vector3 StartPointDirection = User.transform.forward;
        Vector3 startPosition = Player.transform.position + Player.transform.right * configs.hzOffset + Player.transform.up * configs.vOffset;
        Vector3 randomDirection =
        Quaternion.Euler(
            Random.Range(-configs.ProjectileSpreadV, configs.ProjectileSpreadV),
            Random.Range(-configs.ProjectileSpreadHz, configs.ProjectileSpreadHz),
            Random.Range(-configs.ProjectileSpreadV, configs.ProjectileSpreadV)
        ) * StartPointDirection;

        Vector3 endPosition = startPosition + randomDirection * configs.MaxRange;

        float distance = Vector3.Distance(startPosition, endPosition);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * configs.ProjectileSpeed * (1 + pressTime[User] / 10) / distance;
            float DropForce = configs.ProjectileDrop * Mathf.Pow(t * distance, 2) / 10000;
            Obj.transform.position =
            new Vector3(
                Vector3.Lerp(startPosition, endPosition, t).x,
                Vector3.Lerp(startPosition, endPosition, t).y - DropForce,
                Vector3.Lerp(startPosition, endPosition, t).z
            );

            yield return null;
        }

        if(PC.isEnable) PC.DisableObject();
    }
    private void SetTrail(GameObject Obj, DACS_P_Configs configs)
    {
        TrailRenderer trailRenderer = Obj.GetComponent<TrailRenderer>();

        trailRenderer.widthCurve = configs.TrailCurve;
        trailRenderer.time = configs.TrailTime;
        trailRenderer.material = configs.TrailMaterial;
    }
}

