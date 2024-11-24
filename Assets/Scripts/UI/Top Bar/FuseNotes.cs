using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseNotes : MonoBehaviour
{
    public void ToggleFuseMode()
    {
        Placement.instance.allowFusedNotePlacement = !Placement.instance.allowFusedNotePlacement;
    }
}
