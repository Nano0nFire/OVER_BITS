using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLAPUIConector : MonoBehaviour
{
    public int ListType;
    public int DataListNum;

    public void SetCLAP(CustomLifeAvatar clap)
    {
        clap.SelectedEquipmentIDs[ListType] = DataListNum;
    }
}
