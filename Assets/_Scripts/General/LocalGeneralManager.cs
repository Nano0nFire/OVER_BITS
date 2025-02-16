using Cinemachine;
using UnityEngine;

public class LocalGeneralManager : MonoBehaviour
{
    public GameObject MainMenu;
    public CinemachineVirtualCamera CVCamera;
    public DACS.Entities.EntityManagementSystem emSystem;
    public UI_PlayerSettings UI_playerSettings;
    public static LocalGeneralManager Instance;
    void Start()
    {
        Instance = this;
    }
}
