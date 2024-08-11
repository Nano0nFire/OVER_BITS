using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralManager : MonoBehaviour
{
    [SerializeField] GameObject MainMenu;

    public void UIexit(InputAction.CallbackContext context)
    {
        if (context.performed)
            if (MainMenu.activeSelf) MainMenu.SetActive(false);
            else MainMenu.SetActive(true);

    }
}
