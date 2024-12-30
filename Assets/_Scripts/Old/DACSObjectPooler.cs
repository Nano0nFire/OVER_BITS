using System.Collections.Generic;
using UnityEngine;
using DACS.Projectile;

public class DACSObjectPooler : MonoBehaviour
{
    public ProjectileSO DACSSO;
    private Queue<GameObject> objectPool = new Queue<GameObject>();

    public GameObject GetObjectFromPool(int ID, Transform Pos)
    {
        if (objectPool.Count == 0)
        {
            GameObject obj = Instantiate(DACSSO.P_ScriptableObject[ID].ProjectileObject, transform);
            obj.transform.position = Pos.position;
            Debug.Log("NewInstance");
            return obj;
        }
        else
        {
            GameObject Obj = objectPool.Dequeue();
            Obj.transform.position = Pos.position;
            Obj.SetActive(true);
            Debug.Log("CurObject");
            return Obj;
        }
    }
    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
    }

}
