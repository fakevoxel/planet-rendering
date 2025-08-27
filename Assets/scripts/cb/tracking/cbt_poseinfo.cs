using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class cbt_poseinfo
{
    // these vars are used so I don't have to do math more than once
    public DoubleVector3 localPosition;
    public DoubleVector3 velocity;

    public trackedbody_mono data;

    // using physics stuff to move the planets is better for short timespans
    public void StepNewtonian(float delta)
    {
        localPosition = localPosition.Add(velocity.Mul(delta));

        velocity = velocity.Add(localPosition.Mul(-1).Norm().Mul(delta).Mul(Sys.gravConstant).Mul(data.orbit.parent.data.config.mass).Div(localPosition.Mag()).Div(localPosition.Mag()));
    }

    public DoubleVector3 GetPosition()
    {
        if (data.orbit.parentIndex != -1)
        {
            return localPosition.Add(data.orbit.parent.data.pose.GetPosition());
        }
        else
        {
            return localPosition;
        }
    }
}
