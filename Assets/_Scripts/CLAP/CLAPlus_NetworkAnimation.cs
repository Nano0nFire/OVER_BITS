using UnityEngine;
using Unity.Netcode;

public class CLAPlus_NetworkAnimation : NetworkBehaviour
{
    [SerializeField] Animator animator;

    void Update()
    {
        
    }

    // アニメーションパラメーターを設定する汎用関数
    public void SetAnimationParameter<T>(string parameterName, T value)
    {
        if (!IsOwner) return;

        if (typeof(T) == typeof(float))
        {
            animator.SetFloat(parameterName, (float)(object)value);
            SetFloatParameterServerRpc(parameterName, (float)(object)value);
        }
        else if (typeof(T) == typeof(int))
        {
            animator.SetInteger(parameterName, (int)(object)value);
            SetIntParameterServerRpc(parameterName, (int)(object)value);
        }
        else if (typeof(T) == typeof(bool))
        {
            animator.SetBool(parameterName, (bool)(object)value);
            SetBoolParameterServerRpc(parameterName, (bool)(object)value);
        }
        else if (typeof(T) == typeof(string))
        {
            animator.SetTrigger(parameterName);
            SetTriggerParameterServerRpc(parameterName);
        }
    }

    // パラメーターの同期をサーバーに通知するRPC (float)
    [ServerRpc]
    private void SetFloatParameterServerRpc(string parameterName, float value)
    {
        SetFloatParameterClientRpc(parameterName, value);
    }

    // パラメーターの同期をサーバーに通知するRPC (int)
    [ServerRpc]
    private void SetIntParameterServerRpc(string parameterName, int value)
    {
        SetIntParameterClientRpc(parameterName, value);
    }

    // パラメーターの同期をサーバーに通知するRPC (bool)
    [ServerRpc]
    private void SetBoolParameterServerRpc(string parameterName, bool value)
    {
        SetBoolParameterClientRpc(parameterName, value);
    }

    // パラメーターの同期をサーバーに通知するRPC (trigger)
    [ServerRpc]
    private void SetTriggerParameterServerRpc(string parameterName)
    {
        SetTriggerParameterClientRpc(parameterName);
    }

    // クライアントに同期するRPC (float)
    [ClientRpc]
    private void SetFloatParameterClientRpc(string parameterName, float value)
    {
        if (IsOwner) return;  // 自分自身の処理は無視する
        animator.SetFloat(parameterName, value);
    }

    // クライアントに同期するRPC (int)
    [ClientRpc]
    private void SetIntParameterClientRpc(string parameterName, int value)
    {
        if (IsOwner) return;
        animator.SetInteger(parameterName, value);
    }

    // クライアントに同期するRPC (bool)
    [ClientRpc]
    private void SetBoolParameterClientRpc(string parameterName, bool value)
    {
        if (IsOwner) return;
        animator.SetBool(parameterName, value);
    }

    // クライアントに同期するRPC (trigger)
    [ClientRpc]
    private void SetTriggerParameterClientRpc(string parameterName)
    {
        if (IsOwner) return;
        animator.SetTrigger(parameterName);
    }
}
