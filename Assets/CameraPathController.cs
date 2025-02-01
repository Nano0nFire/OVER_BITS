using Cinemachine;
using UnityEngine;

public class CameraPathController : MonoBehaviour
{
    public CinemachineDollyCart dollyCart;
    public float speed = 2f;

    void Update()
    {
        dollyCart.m_Position += speed * Time.deltaTime;
    }
}

