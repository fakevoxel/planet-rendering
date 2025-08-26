using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// combines the three tracking classes
[System.Serializable]
public class trackedbody
{
    public string name;
    public cbt_config config;
    public cbt_orbit orbit;
    public cbt_poseinfo pose;
}
