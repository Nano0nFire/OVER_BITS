using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLAPUIConector : MonoBehaviour
{
    public int listIndex; // ModelIDsのインデックス
    public int DataListNum; // ModelIDs内のlistIndex番目の値
    public CustomLifeAvatar cla;

    public void SetCLAP()
    {
        cla.ModelIDs[listIndex] = DataListNum;
    }
}
