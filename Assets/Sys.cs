using UnityEngine;

public class Sys : MonoBehaviour
{
    void Awake()
    {
        sprites = ins_sprites;
    }
    public static float gravConstantScaleFactor = 1f;
    public static float gravConstant = 1f;

    public static float mapViewScalingFactor = 0.00000001f;

    // resources that are used across the game
    // (trying something a little less prefab-y, today)
    public Sprite[] ins_sprites;
    public static Sprite[] sprites;
}
