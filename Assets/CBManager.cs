using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CBManager : MonoBehaviour
{
    private static CBManager _instance;

    public static CBManager Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("You messed up buddy.");
                Destroy(value);
            }
        }
    }
    public float planetRotSpeed;
    public float originRadius;
    public float renderRadius;
    public cbr_floatingentity[] celestialBodies;
    public List<cbr_floatingentity> spacecraft;
    public cbr_floatingentity player;
    public List<cbr_fixedentity> structures;
    public Vector3 worldOffset;
    public cbr_floatingentity entityInControl;

    public void Awake() {
        Instance = this;
        
        worldOffset = Vector3.zero;

        // celestialBodies = new FloatingEntity[2] {
        //     new FloatingEntity(GameObject.Find("Planet").transform),
        //     new FloatingEntity(GameObject.Find("Sun").transform, new Vector3(-1000000000, 0, 0), 200000000)};

        celestialBodies = new cbr_floatingentity[1] {new cbr_floatingentity(GameObject.Find("Planet").transform)};
        player = new cbr_floatingentity(GameObject.Find("Player").transform);

        spacecraft = new List<cbr_floatingentity>();
        structures = new List<cbr_fixedentity>();
    }


    // Update the entities based on their listed positions
    public void Update() {
        if (entityInControl.reference != null) {
            if (entityInControl.reference.position.magnitude > originRadius) {
                Vector3 shoveFactor = -entityInControl.reference.position;
                // player is too far from (0, 0, 0) so shove em' back
                worldOffset += shoveFactor;
                for (int i = 0; i < spacecraft.Count; i++) {
                    spacecraft[i].reference.position += shoveFactor;
                }
                player.reference.position += shoveFactor;
            }
        }

        // planets
        for (int i = 0; i < celestialBodies.Length; i++) {
            celestialBodies[i].Refresh();
        }

        // ships
        for (int i = 0; i < spacecraft.Count; i++) {
            spacecraft[i].Refresh();
        }

        // player
        player.Refresh();

        for (int i = 0; i < structures.Count; i++)
        {
            structures[i].Refresh();
        }
    }

    public cbr_floatingentity GetEntityFromTransform(Transform _input) {
        for (int i = 0; i < spacecraft.Count; i++) {
            if (spacecraft[i].reference == _input) {
                return spacecraft[i];
            }
        }

        return null;
    }

    // returns the offset of the camera relative to the entity in control
    public Vector3 GetCameraOffset() {
        return CameraController.Instance.position;
    }

    // Add a ship to the list (when a ship is created)
    public void AddShip(Transform _toAdd) {
        spacecraft.Add(new cbr_floatingentity(_toAdd));
    }

    public void AddShip(Transform _toAdd, Vector3 _pos, Vector3 _rot) {
        spacecraft.Add(new cbr_floatingentity(_toAdd, _pos, _rot));
    }

    // Add a structure to the list (when a structure is generated)
    public void AddStructure(Transform _toAdd, int _body) {
        structures.Add(new cbr_fixedentity(_toAdd, _body));
    }

    public bool EntityInsideRenderRadius(cbr_floatingentity _entity) {
        return true; // change this
    }

    public Vector3 GetOriginInGameSpace() {
        return worldOffset;
    }

    // Functions for switching between entities
    public void SwitchToShip(int _shipId) {
        entityInControl = spacecraft[_shipId];
    }

    public void SwitchToShip(Transform _shipTransform) {
        // find the entity that matches the transform
        int foundShip = -1;
        for (int i = 0; i < spacecraft.Count; i++) {
            if (spacecraft[i].reference == _shipTransform) {
                foundShip = i;
                break;
            }
        }

        entityInControl = spacecraft[foundShip];
    }

    public void SwitchToPlayer() {
        entityInControl = player;
    }

    public Vector3 AdjustVector(Vector3 _v, int _id) {
        return new Vector3(_v.z * Mathf.Sin(celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * Mathf.Cos(celestialBodies[_id].rotation.y * (Mathf.PI / 180)), _v.y, _v.z * Mathf.Cos(celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * -Mathf.Sin(celestialBodies[_id].rotation.y * (Mathf.PI / 180)));
    }

    public Vector3 AdjustVectorReverse(Vector3 _v, int _id) {
        return new Vector3(_v.z * Mathf.Sin(-celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * Mathf.Cos(-celestialBodies[_id].rotation.y * (Mathf.PI / 180)), _v.y, _v.z * Mathf.Cos(-celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * -Mathf.Sin(-celestialBodies[_id].rotation.y * (Mathf.PI / 180)));
    }

    public Vector3 AdjustVector(Vector3 _v, float _amount) {
        return new Vector3(_v.z * Mathf.Sin(_amount * (Mathf.PI / 180)) + _v.x * Mathf.Cos(_amount * (Mathf.PI / 180)), _v.y, _v.z * Mathf.Cos(_amount * (Mathf.PI / 180)) + _v.x * -Mathf.Sin(_amount * (Mathf.PI / 180)));
    }

    public List<Vector4> GetPositions() {
        Vector4[] toReturn = new Vector4[celestialBodies.Length];
        for (int i = 0; i < toReturn.Length; i++) {
            toReturn[i] = celestialBodies[i].position - player.position;
        }
        return toReturn.ToList();
    }
    
    public List<float> GetAngles() {
        float[] toReturn = new float[celestialBodies.Length];
        for (int i = 0; i < toReturn.Length; i++) {
            toReturn[i] = celestialBodies[i].rotation.y;
        }
        return toReturn.ToList();
    }

    // public List<float> GetScaleFactors() {
    //     float[] toReturn = new float[celestialBodies.Length];
    //     for (int i = 0; i < toReturn.Length; i++) {
    //         toReturn[i] = GameUtils.planets[i].transform.localScale.x;
    //     }
    //     return toReturn.ToList();
    // }
}
