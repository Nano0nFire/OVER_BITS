using UnityEngine;

public class LobbyCameraControl : MonoBehaviour
{
    [SerializeField] Transform target; // 視点の対象

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
        }
    }
}
