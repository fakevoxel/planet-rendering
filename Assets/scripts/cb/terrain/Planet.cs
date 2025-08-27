using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{
    public bool realView;
    public Material planetMaterial;

    [Range(2, 256)]
    public int resolution = 10;

    public float radius;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public Transform player;
    private List<TerrainFace> faces;
    private List<TerrainFace> newFaces;

    public float[] detailLevels;

    public void Initialize()
    {
        faces = new List<TerrainFace>();
        newFaces = new List<TerrainFace>();

        player = GameObject.Find("player").transform; // bad call lolol

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = new Vector3[6] { Vector3.up, -Vector3.up, Vector3.right, -Vector3.right, Vector3.forward, -Vector3.forward };

        for (int i = 0; i < 6; i++)
        {
            GameObject meshObj = null;

            if (meshFilters[i] == null)
            {
                meshObj = new GameObject("mesh");
                meshObj.transform.SetParent(transform);

                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(planetMaterial);
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i], new Vector3(0, 0, resolution), radius);
            terrainFaces[i].reference = meshObj;
            terrainFaces[i].level = 0;
            terrainFaces[i].parent = null;

            terrainFaces[i].startingFace = i;
            terrainFaces[i].hashCode = null;
        }

        for (int i = 0; i < 6; i++)
        {
            terrainFaces[i].ConstructMesh();
        }

        for (int i = 0; i < terrainFaces.Length; i++)
        {
            faces.Add(terrainFaces[i]);
        }
    }

    void FixedUpdate()
    {
        if (faces != null)
        {
            UpdateFaces();
        }
    }

    void UpdateFaces()
    {
        newFaces.Clear();
    
        foreach (TerrainFace current in faces)
        {   
            if (current.reference.activeSelf && current.level > 0)
            {
                int count = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (Vector3.Distance(current.parent.children[i].reference.GetComponent<MeshRenderer>().bounds.center, player.position) > detailLevels[current.level - 1])
                    {
                        count++;
                    }
                }

                if (count == 4)
                {
                    current.reference.SetActive(false);
                    if (current.parent != null)
                    {
                        current.parent.reference.SetActive(true);
                    }
                }
            }

            if (current.children != null)
            {
                if (current.reference.name == "mesh")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        newFaces.Add(current.children[i]);
                    }

                    current.reference.name = "cached mesh";
                    current.reference.SetActive(false);
                }
            }
            else if (detailLevels.Length > current.level + 1 && Vector3.Distance(current.reference.GetComponent<MeshRenderer>().bounds.center, player.position) < detailLevels[current.level + 1])
            {
                Subdivide(current);
            }

            if (current.hashCode != null && current.reference.activeSelf)
            {
                GetNeighborLOD(current.hashCode, current.reference.GetComponent<MeshRenderer>(), current.startingFace);
            }
        }

        foreach(TerrainFace current in newFaces)
        {
            faces.Add(current);
        }
    }

    public void Subdivide(TerrainFace input)
    {
        input.children = new TerrainFace[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject meshObj = new GameObject("mesh");
            meshObj.transform.SetParent(transform);
            meshObj.transform.localScale = Vector3.one;

            meshObj.transform.position = TrackingManager.Instance.bodies[0].transform.position;

            MeshFilter newFilter = meshObj.AddComponent<MeshFilter>();
            meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(planetMaterial);
            newFilter.sharedMesh = new Mesh();

            float res = (float)resolution;
            
            input.children[i] = new TerrainFace(newFilter.sharedMesh, resolution, input.localUp, new Vector3(input.dims.x + (res * (input.dims.z / res)) / 2f * ((float)i % 2f), input.dims.y + (res * (input.dims.z/ res)) / 2 * Mathf.Floor((float)i / 2f), (res * (input.dims.z/ res)) / 2f), radius);
            input.children[i].reference = meshObj;
            input.children[i].level = input.level + 1;
            input.children[i].parent = input;

            input.children[i].startingFace = input.startingFace;
            input.children[i].hashCode = input.hashCode + i.ToString();
        }

        for (int i = 0; i < 4; i++)
        {
            input.children[i].ConstructMesh();

            if (input.level + 1 > detailLevels.Length - 5)
            {
                input.children[i].reference.AddComponent<MeshCollider>();
            }
        }
    }

    void GetNeighborLOD(string hashCode, MeshRenderer output, int startingFace)
    {
        int lastDigit = int.Parse(hashCode[hashCode.Length - 1].ToString());

        bool lod1 = false;
        bool lod2 = false;

        if (lastDigit == 0)
        {
            lod1 = CheckNeighborLOD(hashCode, 0, startingFace);
            lod2 = CheckNeighborLOD(hashCode, 3, startingFace);
        }
        else if (lastDigit == 1)
        {
            lod1 = CheckNeighborLOD(hashCode, 0, startingFace);
            lod2 = CheckNeighborLOD(hashCode, 2, startingFace);
        }
        else if (lastDigit == 2)
        {
            lod1 = CheckNeighborLOD(hashCode, 1, startingFace);
            lod2 = CheckNeighborLOD(hashCode, 3, startingFace);
        }
        else if (lastDigit == 3)
        {
            lod1 = CheckNeighborLOD(hashCode, 1, startingFace);
            lod2 = CheckNeighborLOD(hashCode, 2, startingFace);
        }
        
        if (!lod1 || !lod2)
        {
            output.sharedMaterial = planetMaterial;
        }
        else
        {
            output.sharedMaterial = planetMaterial;
        }
    }

    bool CheckNeighborLOD(string hashCode, int dir, int startingFace)
    {
        string neighborCode = ComputeHashValue(hashCode, dir);

        if (IsOnBorder(hashCode, dir))
        {
            startingFace = ChangeSide(startingFace, dir);
            neighborCode = ChangeHash(neighborCode, startingFace, dir);
        }
        
        TerrainFace currentFace = terrainFaces[startingFace];
        
        for (int i = 0; i < neighborCode.Length; i++)
        {
            if (currentFace.hashCode == null && currentFace.children == null)
            {
                return false;
            }

            if (currentFace.children == null && currentFace.hashCode.Length != hashCode.Length)
            {
                return false;
            }
            else
            {
                currentFace = currentFace.children[int.Parse(neighborCode[i].ToString())];
            }
        }

        if (!currentFace.reference.activeSelf)
        {
            if (currentFace.children == null)
            {
                return false;
            }

            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!currentFace.children[i].reference.activeSelf)
                {
                    count++;
                }
            }

            if (count == 4)
            {
                return false;
            }
        }

        return true;
    }

    bool IsOnBorder(string hashCode, int dir)
    {
        if (dir == 0)
        {
            if (!hashCode.Contains("2") && !hashCode.Contains("3"))
            {
                return true;
            }
        }
        else if (dir == 1)
        {
            if (!hashCode.Contains("1") && !hashCode.Contains("0"))
            {
                return true;
            }
        }
        else if (dir == 2)
        {
            if (!hashCode.Contains("0") && !hashCode.Contains("2"))
            {
                return true;
            }
        }
        else if (dir == 3)
        {
            if (!hashCode.Contains("3") && !hashCode.Contains("1"))
            {
                return true;
            }
        }

        return false;
    }

    string ChangeHash(string hashCode, int side, int dir)
    {
        string newHashCode = null;

        if (dir == 0)
        {
            if (side % 2 == 0)
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode = "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode = "2";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode += "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode += "2";
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode = "3";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode = "1";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode += "3";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode += "1";
                        }
                    }
                }
            }
        }
        else if (dir == 1)
        {
            if (side % 2 == 0)
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode = "3";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode = "1";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode += "3";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode += "1";
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode = "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode = "2";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode += "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode += "2";
                        }
                    }
                }
            }
        }
        else if (dir == 2)
        {
            if (side % 2 != 0)
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode = "1";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode = "0";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode += "1";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode += "0";
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode = "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode = "3";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "0"))
                        {
                            newHashCode += "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "2"))
                        {
                            newHashCode += "3";
                        }
                    }
                }
            }
        }
        else if (dir == 3)
        {
            if (side % 2 != 0)
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode = "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode = "1";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode += "0";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode += "1";
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < hashCode.Length; i++)
                {
                    if (i == 0)
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode = "3";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode = "2";
                        }
                    }
                    else
                    {
                        if (string.Equals(hashCode[i].ToString(), "1"))
                        {
                            newHashCode += "3";
                        }
                        else if (string.Equals(hashCode[i].ToString(), "3"))
                        {
                            newHashCode += "2";
                        }
                    }
                }
            }
        }

        return newHashCode;
    }
    
    int ChangeSide(int side, int dir)
    {
        if (dir == 0)
        {
            if (side == 0)
            {
                side = 4;
            }
            else if (side == 1)
            {
                side = 4;
            }
            else if (side == 2)
            {
                side = 0;
            }
            else if (side == 3)
            {
                side = 0;
            }
            else if (side == 4)
            {
                side = 2;
            }
            else if (side == 5)
            {
                side = 2;
            }
        }
        else if (dir == 1)
        {
            if (side == 0)
            {
                side = 5;
            }
            else if (side == 1)
            {
                side = 5;
            }
            else if (side == 2)
            {
                side = 1;
            }
            else if (side == 3)
            {
                side = 1;
            }
            else if (side == 4)
            {
                side = 3;
            }
            else if (side == 5)
            {
                side = 3;
            }
        }
        else if (dir == 2)
        {
            if (side == 0)
            {
                side = 2;
            }
            else if (side == 1)
            {
                side = 3;
            }
            else if (side == 2)
            {
                side = 4;
            }
            else if (side == 3)
            {
                side = 5;
            }
            else if (side == 4)
            {
                side = 0;
            }
            else if (side == 5)
            {
                side = 1;
            }
        }
        else if (dir == 3)
        {
            if (side == 0)
            {
                side = 3;
            }
            else if (side == 1)
            {
                side = 2;
            }
            else if (side == 2)
            {
                side = 5;
            }
            else if (side == 3)
            {
                side = 4;
            }
            else if (side == 4)
            {
                side = 1;
            }
            else if (side == 5)
            {
                side = 0;
            }
        }

        return side;
    }

    string ComputeHashValue(string input, int dir)
    {
        string hashCode = input;

        for (int i = hashCode.Length - 1; i >= 0; i--)
        {
            int digit = int.Parse(hashCode[i].ToString());

            int newDigit = -1;
            string newHash = null;

            newDigit = FlipNumber(digit, dir);
            for (int _i = 0; _i < hashCode.Length; _i++)
            {
                if (_i == i)
                {
                    if (string.IsNullOrEmpty(newHash))
                    {
                        newHash = newDigit.ToString();
                    }
                    else
                    {
                        newHash += newDigit.ToString();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(newHash))
                    {
                        newHash = hashCode[_i].ToString();
                    }
                    else
                    {
                        newHash += hashCode[_i];
                    }
                }
            }

            hashCode = newHash;

            if (dir == 0)
            {
                if (digit == 2 || digit == 3)
                {
                    break;
                }
            }
            else if (dir == 1)
            {
                if (digit == 1 || digit == 0)
                {
                    break;
                }
            }
            else if (dir == 2)
            {
                if (digit == 0 || digit == 2)
                {
                    break;
                }
            }
            else if (dir == 3)
            {
                if (digit == 1 || digit == 3)
                {
                    break;
                }
            }
        }

        return hashCode;
    }

    int FlipNumber(int num, int dir)
    {
        if (dir == 0 || dir == 1)
        {
            if (num == 0)
            {
                num = 2;
            }
            else if (num == 1)
            {
                num = 3;
            }
            else if (num == 2)
            {
                num = 0;
            }
            else if (num == 3)
            {
                num = 1;
            }
        }
        else if (dir == 2 || dir == 3)
        {
            if (num == 0)
            {
                num = 1;
            }
            else if (num == 1)
            {
                num = 0;
            }
            else if (num == 2)
            {
                num = 3;
            }
            else if (num == 3)
            {
                num = 2;
            }
        }

        return num;
    }
}