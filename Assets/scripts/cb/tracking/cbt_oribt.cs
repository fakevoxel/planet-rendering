using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// all hail kepler
// keeping as Mono so I can ref transform
[System.Serializable]
public class cbt_orbit
{
    public bool isGrandparent; // sun don't move 
    public int parentIndex;
    public cbt_orbit parent; // (parent), grabbing parents data from here (namely mass and position) instead of having dupe variables

    public trackedbody_mono data;

    // i for initial
    public Vector3 iPosition;
    public Vector3 iVelocity;
    public float iRadius;
    public float iAngle;
    public float iRadialVelocity;
    public float iTransverseVelocity;
    public float iPhaseShift;
    public float iM;
    public float iN;

    public float orbitalPeriod;
    public float orbitalEccentricity;

    public void Initialize()
    {
        // STARTING CONDITIONS:
        // set via inspector

        // this.isGrandparent = isGrandparent;
        // iPosition = pos;
        // iVelocity = v;

        if (isGrandparent) { return; } // no params for the sun

        // ORBITAL PARAMS:
        // we CANNOT use unity positions because those won't be reliable
        // remember engine space vs. game space?

        // also assuming everything happens in the x, z plane (for now)
        iRadius = iPosition.magnitude;

        iAngle = Mathf.Atan2(iPosition.z, iPosition.x);

        iRadialVelocity = iVelocity.z * Mathf.Sin(iAngle) + iVelocity.x * Mathf.Cos(iAngle);
        iTransverseVelocity = iVelocity.z * Mathf.Cos(iAngle) - iVelocity.x * Mathf.Sin(iAngle);

        iM = Sys.gravConstant * parent.data.config.mass / iRadius / iRadius / iTransverseVelocity / iTransverseVelocity; // GetComponent is okay cuz its run once
        iN = Mathf.Sqrt(((1 / iRadius) - iM) * ((1 / iRadius) - iM) + (iRadialVelocity / iRadius / iTransverseVelocity) * (iRadialVelocity / iRadius / iTransverseVelocity));

        iPhaseShift = Mathf.Sign(iRadialVelocity * iTransverseVelocity) * Mathf.Acos(((1 / iRadius) - iM) / iN) - iAngle;

        orbitalEccentricity = iN / iM;

        orbitalPeriod = (iM * 2 * Mathf.PI) / (Mathf.Abs(iTransverseVelocity) * iRadius * Mathf.Pow(iM * iM - iN * iN, 1.5f));

        data.pose.localPosition = new DoubleVector3(iPosition);
        data.pose.velocity = new DoubleVector3(iVelocity);
    }

    // the polar function for an elipse, adapted
    public float DistFromFocus(float angle)
    {
        return 1 / (iM + iN * Mathf.Cos(angle + iPhaseShift));
    }

    public float MeanAnomaly(float time)
    {
        return Mathf.Pow((iM * iM - iN * iN), 3f / 2f) / iM * iRadius * iTransverseVelocity * time + 2 * Mathf.Atan(Mathf.Sqrt((iM - iN) / (iM + iN)) * Mathf.Tan((iAngle + iPhaseShift) / 2)) - orbitalEccentricity * Mathf.Sqrt(iM * iM - iN * iN) * Mathf.Sin(iAngle + iPhaseShift) * DistFromFocus(iAngle);
    }

    public float EccentricAnomaly(float time, int p)
    {
        float meanAnomaly = MeanAnomaly(time);

        int n = p; // precision of integral
        float step = (Mathf.PI - 0) / (float)n;
        float sum = (EccentricIntegrationValue(0, meanAnomaly) + EccentricIntegrationValue(Mathf.PI, meanAnomaly)) * 0.5f;

        for (int i = 1; i < n; i++)
        {
            sum += EccentricIntegrationValue((float)i * (float)step, meanAnomaly);
        }

        return sum * step;
    }

    public float EccentricIntegrationValue(float phi, float meanAnomaly)
    {
        return Mathf.Floor((phi - orbitalEccentricity * Mathf.Sin(phi) + meanAnomaly) / (Mathf.PI * 2f)) - Mathf.Floor((phi - orbitalEccentricity * Mathf.Sin(phi) - meanAnomaly) / (Mathf.PI * 2f));
    }

    public float TrueAnomaly(float time, int p)
    {
        float eccentricAnomaly = EccentricAnomaly(time, p);

        return 2 * Mathf.Atan(Mathf.Tan(eccentricAnomaly / 2f) * Mathf.Sqrt((iM + iN) / (iM - iN))) - iPhaseShift;
    }

    public DoubleVector3 GetPositionAtTime(float time, int precision)
    {
        float trueAnomaly = TrueAnomaly(time, precision);
        float radius = DistFromFocus(trueAnomaly);

        Vector3 result = new Vector3(radius * Mathf.Cos(trueAnomaly), 0, radius * Mathf.Sin(trueAnomaly));
        //data.pose.position = new DoubleVector3(result).Add(parent.data.pose.position);
        return new DoubleVector3(result);
    }

    public Vector3[] SampleFullOrbit(int pointCount)
    {
        Vector3[] result = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float currentTime = orbitalPeriod / ((float)pointCount - 1f) * (float)i;

            result[i] = GetPositionAtTime(currentTime, 1000).ToVector3();
        }

        return result;
    }
}
