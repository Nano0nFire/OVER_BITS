using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonEntityStatus : MonoBehaviour
{
    public float hp
    {
        get
        {
            return _hp;
        }
        set
        {
            if (value > 0)
            {
                _hp = value;
            }
            else
            {
                _hp = 0;
                OnDeath();
            }
        }
    }
    float _hp;

    public virtual void OnDeath() // オーバーライド前提
    {
        Debug.LogWarning("ERROR : No Action (CommonEntityStatus, OnDeath)");
    }
}
