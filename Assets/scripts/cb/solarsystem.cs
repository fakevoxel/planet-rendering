using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// solar systems use "keys", which look like real-life star classifications
// example: 123456E1K3R4
// the first 6 numbers are the world seed, the letters how many bodies there are
// this gives a bit more control over how you want the system to look
// E -> earth-like planets
// J -> jovian planets (gas giants)
// R -> rocky planets (no atmo)

[System.Serializable]
public class solarsystem
{
    public trackedbody[] bodies;
}
