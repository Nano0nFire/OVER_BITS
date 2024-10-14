using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;

public class UIGeneral : MonoBehaviour
{
    public ClientGeneralManager generalManager;
    public CustomLifeAvatar cla;
    [SerializeField] List<PanelSwitcher> psList;
    [SerializeField] List<SlotGenerator> SGList;
    NetworkManager nwManager;

    void Start()
    {
        nwManager = NetworkManager.Singleton;
    }

    void OnEnable()
    {
        foreach (var item in psList) item.EnableUI();
        foreach (var item in SGList)
        {
            item.cla = cla;
            item.EnableUI();
        }
    }
    void OnDisable()
    {
        foreach (var item in SGList) item.DisableUI();
    }

    public void UIexit(GameObject Menu)
    {
        Menu.SetActive(!Menu.activeSelf);

        if (Menu == generalManager.MainMenu)
        {
            generalManager.UseInput = Menu.activeSelf;
        }
    }

    public void CLACombine()
    {
        cla.Combiner();
    }

    public void StartHost()
    {
        nwManager.StartHost();
        nwManager.SceneManager.LoadScene("TestWorld", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        nwManager.StartClient();
    }
}
