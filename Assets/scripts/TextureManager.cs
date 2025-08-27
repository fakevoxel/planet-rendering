using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    private static TextureManager _instance;

    public static TextureManager Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.Log("You messed up buddy.");
                Destroy(value);
            }
        }
    }

    [Header("CONTROLS")]
    public bool generateTexture;
    [Header("SETTINGS")]
    public int selectedProfile;
    public TextureProfile[] textures;

    void Awake() {
        Instance = this;

        Sys.perlin = new Perlin();
    }

    void Update() {
        if (generateTexture) {
            generateTexture = false;

            // Select the profile we want to use
            TextureProfile currentProfile = textures[selectedProfile];
            currentProfile.filePath = "Assets/" + currentProfile.name + ".png";

            // Create a blank texture with the specified size
            currentProfile.texture = new Texture2D(currentProfile.textureResolution.x, currentProfile.textureResolution.y);

            // Loop through each pixel of the texture
            for (int x = 0; x < currentProfile.textureResolution.x; x++) {
                for (int y = 0; y < currentProfile.textureResolution.x; y++) {
                    //texture.SetPixel(x, y, new Color((point.x + 1) / 2, (point.y + 1) / 2, (point.z + 1) / 2, 1));
                    //Vector3 point = UVToPoint((float)x / (Resolution.x - 1), (float)y / (Resolution.x - 1)).normalized;
                    
                    // float noise1 = (float)Perlin_OLD.Noise(point.x * 2, point.y * 2, point.z * 2) * 2;
                    // float noise2 = (float)Perlin_OLD.Noise(point.x * 6, point.y * 6, point.z * 6);
                    // float noise3 = (float)Perlin_OLD.Noise(point.x * 20, point.y * 20, point.z * 20) / 4;
                    //--//

                    // Create an array for storing noise values
                    float[] noiseValues = new float[currentProfile.noiseLayers.Length];

                    for (int i = 0; i < currentProfile.noiseLayers.Length; i++) {
                        NoiseLayer currentLayer = currentProfile.noiseLayers[i];
                        
                        if (currentLayer.layerType == 0) { // x axis only
                            // Sampling based on the x axis (uv coordinate) of the texture
                            if (currentProfile.useUnityNoise) {
                                noiseValues[i] += Mathf.PerlinNoise((float)x / (currentProfile.textureResolution.x - 1) * currentLayer.frequency, 0) * currentLayer.amplitude;
                            }
                            else {
                                noiseValues[i] += ((float)Sys.perlin.Noise((float)x / (currentProfile.textureResolution.x - 1) * currentLayer.frequency, 0, 0) + 1) / 2 * currentLayer.amplitude;
                            }
                        }
                        else if (currentLayer.layerType == 1) { // y axis only
                            // Sampling based on the y axis (uv coordinate) of the texture
                            if (currentProfile.useUnityNoise) {
                                noiseValues[i] += Mathf.PerlinNoise(0, (float)y / (currentProfile.textureResolution.y - 1) * currentLayer.frequency) * currentLayer.amplitude;
                            }
                            else {
                                noiseValues[i] += ((float)Sys.perlin.Noise(0, (float)y / (currentProfile.textureResolution.y - 1) * currentLayer.frequency, 0) + 1) / 2 * currentLayer.amplitude;
                            }
                        }
                        else if (currentLayer.layerType == 2) { // spherical
                            // Generate a point on a sphere based on the uv coordinate of the texture
                            Vector3 point = UVToPoint((float)x / (currentProfile.textureResolution.x - 1), (float)y / (currentProfile.textureResolution.y - 1)).normalized;
                            // Use that point to sample a noise value
                            noiseValues[i] += (float)(Sys.perlin.Noise(point.x * currentLayer.frequency, point.y * currentLayer.frequency, point.z * currentLayer.frequency) + 1) / 2 * currentLayer.amplitude;
                        }
                        else if (currentLayer.layerType == 3) { // x and y axis
                            if (currentProfile.useUnityNoise) {
                                noiseValues[i] += Mathf.PerlinNoise((float)x / (currentProfile.textureResolution.x - 1) * currentLayer.frequency, (float)y / (currentProfile.textureResolution.y - 1) * currentLayer.frequency) * currentLayer.amplitude;
                            }
                            else {
                                noiseValues[i] += ((float)Sys.perlin.Noise((float)x / (currentProfile.textureResolution.x - 1) * currentLayer.frequency, (float)y / (currentProfile.textureResolution.y - 1) * currentLayer.frequency, 0) + 1) / 2 * currentLayer.amplitude;
                            }
                        }
                    }
                    // cloud texture
                    //texture.SetPixel(1 - x, 1 - y, new Color(noise1 + noise2 + noise3, noise2 * 5, 0, 1));

                    // biome texture
                    //texture.SetPixel(1 - x, 1 - y, new Color(noise1 + noise2 + noise3, 0, 0, 1));

                    float[] colorValues  = new float[4];
                    for (int i = 0; i < 4; i++) {
                        for (int j = 0; j < GetColorChannel(i, currentProfile).Length; j++) {
                            colorValues[i] += noiseValues[GetColorChannel(i, currentProfile)[j]];
                        }
                        colorValues[i] += currentProfile.colorOffsets[i];
                        colorValues[i] *= currentProfile.colorMultipliers[i];
                    }

                    int pixelX = x;
                    int pixelY = y;
                    if (currentProfile.invertX) {pixelX = -x-1;}
                    if (currentProfile.invertY) {pixelY = -y-1;}

                    if (!currentProfile.useAlphaChannel) {colorValues[3]=1;}

                    if (!currentProfile.interpolateColor) {
                        currentProfile.texture.SetPixel(pixelX, pixelY, new Color(colorValues[0], colorValues[1], colorValues[2], colorValues[3]));
                    }
                    else {
                        currentProfile.texture.SetPixel(pixelX, pixelY, Color.Lerp(currentProfile.col1, currentProfile.col2, colorValues[0]));
                    }
                }
            }

            //save texture to file
            byte[] png = currentProfile.texture.EncodeToPNG();
            File.WriteAllBytes(currentProfile.filePath, png);
            AssetDatabase.Refresh();
        }
    }

    // Produces a float that reflects the RED channel of a texture profile
    public float ReadSphericalTextureProfile(TextureProfile _profile, Vector3 _vector) {
        float value = 0;

        for (int i = 0; i < _profile.noiseLayers.Length; i++) {
            if (_profile.noiseLayers[i].layerType == 2) {
                NoiseLayer currentLayer = _profile.noiseLayers[i];
                value += (float)(Sys.perlin.Noise(_vector.x * currentLayer.frequency, _vector.y * currentLayer.frequency, _vector.z * currentLayer.frequency) + 1) / 2 * currentLayer.amplitude;
            }
        }

        value += _profile.colorOffsets[0];
        value *= _profile.colorMultipliers[0];

        return Mathf.Clamp(value, 0, 1);
    } 

    // Convert a uv coordinate to a point
    Vector3 UVToPoint(float u, float v) {
        float x = Mathf.Cos(Mathf.PI * (0.5f - v)) * Mathf.Sin(2 * Mathf.PI * (u - 0.5f));
        float y = Mathf.Sin(Mathf.PI * (0.5f - v));
        float z = Mathf.Cos(Mathf.PI * (0.5f - v)) * Mathf.Cos(2 * Mathf.PI * (u - 0.5f));
        return new Vector3(x, y, z);
    }

    // Convert a point to a uv coordinate
    Vector2 UVFromPoint(Vector3 _point) {
        float u = Mathf.Atan2(_point.x, _point.z) / (2*Mathf.PI) + 0.5f;
        float v = -Mathf.Asin(_point.y) / Mathf.PI + 0.5f;
        return new Vector2(u, v);
    }

    int[] GetColorChannel(int _index, TextureProfile _profile) {
        if (_index == 0) {
            return _profile.colorChannelR;
        }
        else if (_index == 0) {
            return _profile.colorChannelG;
        }
        else if (_index == 0) {
            return _profile.colorChannelB;
        }
        else { 
            return _profile.colorChannelA;
        }
    }  
}

[System.Serializable]
public class TextureProfile {
    // Settings
    public string name;
    public string filePath;

    // Texture settings
    public Vector2Int textureResolution; 
    public Texture2D texture;
    public NoiseLayer[] noiseLayers;
    // The indices of the noise layers added to each color channel (rgb)
    public int[] colorChannelR;
    public int[] colorChannelG;
    public int[] colorChannelB;
    public int[] colorChannelA;
    // The multiplier to put on each color channel
    public float[] colorMultipliers;
    public float[] colorOffsets;

    public bool useUnityNoise;

    public bool useAlphaChannel;
    public bool invertX;
    public bool invertY;
    public bool interpolateColor;
    public Color col1;
    public Color col2;
}

// A layer of noise, with amplitude, scale, etc.
[System.Serializable]
public class NoiseLayer {
    public int layerType; // x only (0), y only (1), spherical x,y,z (2), standard x,y (3)
    public float amplitude;
    public float frequency;
}
