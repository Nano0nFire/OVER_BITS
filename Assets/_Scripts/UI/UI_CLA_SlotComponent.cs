using UnityEngine;
using CLAPlus;

public class UI_CLA_SlotComponent : MonoBehaviour
{
    public int listIndex; // ModelIDsのインデックス
    public int DataListNum; // ModelIDs内のlistIndex番目の値
    public CustomLifeAvatar cla;

    public void OnClick()
    {
        cla.ModelIDs[listIndex] = DataListNum;
    }
}
