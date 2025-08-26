using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class cbt_poseinfo
{
    // these vars are used so I don't have to do math more than once
    public Vector3 position; // updated on call of GetPositionAtTime()
    public Vector3 velocity;

    public void StepNewtonian()
    {

    }
}
