using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] InputActionAsset _inputActionAsset;

    // 初期化
    private void Awake()
    {
        // 「Player」というMapを追加
        var hotbarMap = _inputActionAsset.AddActionMap("Hotbar");

        // 「Jump」というActionをPlayer Mapに追加
        var priSlotAction = CreateBtnInput(hotbarMap, "priSlot", "<Keyboard>/1");
        // Jump Actionのperformedコールバックだけ拾う
        priSlotAction.performed += OnJump;
    }

    // 後処理
    private void OnDisable()
    {
        // Input Action Assetの破棄
        if (_inputActionAsset != null)
            Destroy(_inputActionAsset);
    }

    // Jump Actionコールバック
    private void OnJump(InputAction.CallbackContext context)
    {
        // ログ出力
        print("Jump");
    }

    InputAction CreateBtnInput(InputActionMap map, string name, string key)
    {
        return map.AddAction(name, InputActionType.Button, key);
    }
}