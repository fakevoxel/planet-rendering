using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    public GameObject reference;
    public TerrainFace parent;

    public string hashCode;
    public int startingFace;

    Mesh mesh;
    int resolution;
    float radius;

    public Vector3 dims;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public int level;

    public TerrainFace[] children;
    public List<Vector3> treeLocations;
    public List<float> treeRotations;
    public List<int> treeType;

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, Vector3 dims, float radius)
    {
        this.mesh = mesh;
        this.dims = dims;
        this.resolution = resolution;
        this.localUp = localUp;
        this.radius = radius;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        treeLocations = new List<Vector3>();
        treeRotations = new List<float>();
        treeType = new List<int>();

        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector3[] normals = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        int[] indices = new int[(resolution - 1) * (resolution - 1) * 6];

        if (parent != null) {
            if (parent.treeLocations != null) {
                treeLocations = parent.treeLocations;  
                treeRotations = parent.treeRotations;
                treeType = parent.treeType;
            }
        }

        int triIndex = 0;
        float scale = dims.z / (float)resolution;

        for (int i = 0, y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                Vector2 percent = new Vector2(x, y) / (resolution - 1);

                float xOff = dims.x / (float)resolution - (scale / 2) * (1 / scale - 1);
                float yOff = dims.y / (float)resolution - (scale / 2) * (1 / scale - 1);
                xOff /= scale;
                yOff /= scale;
                Vector3 pointOnUnitCube = localUp + axisA * (percent.x + xOff - 0.5f) * 2 * scale + axisB * (percent.y + yOff - 0.5f) * 2 * scale;
                
                //without normalizing it it would be a cube
                vertices[i] = pointOnUnitCube.normalized * radius;
                vertices[i] += pointOnUnitCube.normalized * Sys.PlanetTerrainHeight(vertices[i]);
                vertices[i] = RenderingManager.Instance.AdjustVector(vertices[i].normalized, 0) * vertices[i].magnitude;
                // if (level == 9) {
                //     treeLocations.Add(vertices[i]);
                //     treeRotations.Add((float)Utils.perlin.Noise(vertices[i].x * 100, vertices[i].y * 100, vertices[i].z * 100) * 90);
                //     treeType.Add(GameUtils.GetBiomeIndexAtPoint((vertices[i] * scale).normalized, vertices[i].magnitude));
                // }
                normals[i] = RenderingManager.Instance.AdjustVector(pointOnUnitCube.normalized, 0);
                
                // the 23 and 24 is based off of resolution
                uvs[i] = new Vector2((float)x / 23 * scale + dims.x / 24, (float)y / 23 * scale + dims.y / 24);

                if (x != resolution - 1 && y != resolution - 1)
                {
                    indices[triIndex] = i;
                    indices[triIndex + 1] = i + resolution + 1;
                    indices[triIndex + 2] = i + resolution;

                    indices[triIndex + 3] = i;
                    indices[triIndex + 4] = i + 1;
                    indices[triIndex + 5] = i + resolution + 1;

                    triIndex += 6;
                }
            }
        }
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.uv = uvs;

        reference.layer = Sys.planetLayerMaskInt;
    }
}