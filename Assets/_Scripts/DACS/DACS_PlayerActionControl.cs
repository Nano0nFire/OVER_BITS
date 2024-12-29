using CLAPlus.AnimationControl;
using UnityEngine;
using UnityEngine.InputSystem;

public class DACS_PlayerActionControl : MonoBehaviour // LocalOnly
{
    [SerializeField] ItemDataBase itemDataBase;
    [HideInInspector] public DACS_InventorySystem invSystem; // ClientGeneralManagerが設定
    [HideInInspector] public HandControl handControl; // ClientGeneralManagerが設定
    [SerializeField] DACS_Projectile projectile;

    public void OnRightClick(InputAction.CallbackContext callback)
    {
        var itemData = invSystem.SelectedItem;

        if (itemData.FirstIndex == -1) // 取得不良時はFirstIndexが-1になっているのでここで確認する
            return;

        var data = itemDataBase.GetItem(itemData.FirstIndex, itemData.SecondIndex).R_Function;

        if (callback.performed)
            switch (GetIndex(data, 0))
            {
                case 1:
                    handControl.ChangeHandState(1);
                    break;
            }

        if (callback.canceled)
            switch (GetIndex(data, 0))
            {
                case 1:
                    handControl.ChangeHandState(0);
                    break;
            }
    }

    public void OnLeftClick(InputAction.CallbackContext callback)
    {
        var itemData = invSystem.SelectedItem;

        if (itemData.FirstIndex == -1) // 取得不良時はFirstIndexが-1になっているのでここで確認する
            return;

        var data = itemDataBase.GetItem(itemData.FirstIndex, itemData.SecondIndex).L_Function;

        if (callback.performed)
            switch (GetIndex(data, 0))
            {
                case 1:
                    projectile.StartShooting(GetIndex(data, 2), GetIndex(data, 4), GetIndex(data, 6));
                    break;
            }
    }
    /// <summary>
    /// 引数の文字列について指定した開始地点を一桁目として開始地点の次の文字を二桁目とした整数を返す <br/>
    /// ここでは文字列が8桁と定められているためエラー検出はされていない
    /// </summary>
    /// <param name="chars"></param>
    /// <param name="startPos"></param>
    /// <returns></returns>
    int GetIndex(string chars, int startPos)
    {
        return (chars[startPos] - '0' ) * 10 + chars[startPos + 1] - '0';
    }
}
