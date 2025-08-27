using System;
using UnityEngine;

// GLOBALLY ACCESSIBLE INFORMATION
// basically one big storage container
public class Sys : MonoBehaviour
{
    void Start()
    {
        sprites = ins_sprites;
        mapViewScalingFactor = 20f / GameManager.Instance.loadedSystem.bodies[1].orbit.iPosition.x;
    }
    public static float gravConstantScaleFactor = 1f;
    public static float gravConstant = 1f;

    public static float mapViewScalingFactor;

    // resources that are used across the game
    // (trying something a little less prefab-y, today)
    public Sprite[] ins_sprites;
    public static Sprite[] sprites;

    public static Perlin perlin;
    
    public static LayerMask planetLayerMask = 1 << 8;
    public static int planetLayerMaskInt = 8;

    public static float PlanetTerrainHeight(Vector3 v)
    {
        v = v.normalized;

        float noiseValue = TextureManager.Instance.ReadSphericalTextureProfile(TextureManager.Instance.textures[0], v);

        return (noiseValue * 100) - 70;
    }
}

[System.Serializable]
public class DoubleVector3
{
    public double x;
    public double y;
    public double z;

    public DoubleVector3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public DoubleVector3(Vector3 v)
    {
        this.x = v.x;
        this.y = v.y;
        this.z = v.z;
    }

    public DoubleVector3 Mul(double other)
    {
        return new DoubleVector3(x * other, y * other, z * other);
    }
    public DoubleVector3 Div(double other)
    {
        return new DoubleVector3(x / other, y / other, z / other);
    }
    public DoubleVector3 Add(DoubleVector3 other)
    {
        return new DoubleVector3(other.x + x, other.y + y, other.z + z);
    }
    public DoubleVector3 Add(Vector3 other)
    {
        return new DoubleVector3(other.x + x, other.y + y, other.z + z);
    }
    public DoubleVector3 Sub(DoubleVector3 other)
    {
        return new DoubleVector3(x - other.x, y - other.y, z - other.z);
    }
    public DoubleVector3 Sub(Vector3 other)
    {
        return new DoubleVector3(other.x - x, other.y - y, other.z - z);
    }
    public DoubleVector3 Norm()
    {
        double l = Math.Sqrt(x * x + y * y + z * z);
        return new DoubleVector3(x / l, y / l, z / l);
    }
    public double Mag()
    {
        return Math.Sqrt(x * x + y * y + z * z);
    }


    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }
}
