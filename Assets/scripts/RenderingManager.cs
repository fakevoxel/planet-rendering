using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RenderingManager : MonoBehaviour
{
    private static RenderingManager _instance;

    public static RenderingManager Instance
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

    public Camera c;

    public LayerMask normalView;
    public LayerMask mapView;
    public bool isMapActive;

    public float planetRotSpeed;
    public float originRadius;
    public float renderRadius;
    public cbr_floatingentity[] celestialBodies;
    public cbr_floatingentity player;
    //public List<cbr_fixedentity> structures;
    public Vector3 worldOffset;
    public cbr_floatingentity entityInControl;

    void Awake()
    {
        Instance = this;

        worldOffset = Vector3.zero;
    }

    void Start()
    {
        c.cullingMask = normalView;
        CloseMap();
    }

    public void InitializeSystem(solarsystem system)
    {
        celestialBodies = new cbr_floatingentity[system.bodies.Length];
        for (int i = 0; i < system.bodies.Length; i++)
        {
            celestialBodies[i] = new cbr_floatingentity(TrackingManager.Instance.transform.GetChild(i));
            celestialBodies[i].defaultScale = 1;
        }

        player = new cbr_floatingentity(GameObject.Find("player").transform);
    }

    public void UpdateAll()
    {
        // planets
        for (int i = 0; i < celestialBodies.Length; i++)
        {
            celestialBodies[i].position = TrackingManager.Instance.bodies[i].pose.GetPosition();
            celestialBodies[i].Refresh();
        }
        // player
        player.Refresh();

        if (entityInControl.reference != null)
        {
            if (entityInControl.reference.position.magnitude > originRadius)
            {
                Vector3 shoveFactor = -entityInControl.reference.position;
                // player is too far from (0, 0, 0) so shove em' back
                worldOffset += shoveFactor;
                // for (int i = 0; i < spacecraft.Count; i++)
                // {
                //     spacecraft[i].reference.position += shoveFactor;
                // }
                player.reference.position += shoveFactor;
            }
        }
    }

    public void OpenMap()
    {
        c.cullingMask = mapView;
        isMapActive = true;
        TrackingManager.Instance.t_iconContainer.gameObject.SetActive(true);

        c.transform.SetParent(null);
    }
    public void CloseMap()
    {
        c.cullingMask = normalView;
        isMapActive = false;
        TrackingManager.Instance.t_iconContainer.gameObject.SetActive(false);

        if (entityInControl.reference != null) {c.transform.SetParent(entityInControl.reference);}
        c.transform.localPosition = Vector3.zero;

        c.transform.localEulerAngles = Vector3.zero;
    }

    // returns the offset of the camera relative to the entity in control
    public Vector3 GetCameraOffset() {
        return CameraController.Instance.GetPositionOffset();
    }

    // // Add a ship to the list (when a ship is created)
    // public void AddShip(Transform _toAdd) {
    //     spacecraft.Add(new cbr_floatingentity(_toAdd));
    // }

    // public void AddShip(Transform _toAdd, Vector3 _pos, Vector3 _rot) {
    //     spacecraft.Add(new cbr_floatingentity(_toAdd, _pos, _rot));
    // }

    // // Add a structure to the list (when a structure is generated)
    // public void AddStructure(Transform _toAdd, int _body) {
    //     structures.Add(new cbr_fixedentity(_toAdd, _body));
    // }

    public bool EntityInsideRenderRadius(cbr_floatingentity _entity) {
        return true; // change this
    }

    public Vector3 GetOriginInGameSpace() {
        return worldOffset;
    }

    // // Functions for switching between entities
    // public void SwitchToShip(int _shipId) {
    //     entityInControl = spacecraft[_shipId];
    // }

    // public void SwitchToShip(Transform _shipTransform) {
    //     // find the entity that matches the transform
    //     int foundShip = -1;
    //     for (int i = 0; i < spacecraft.Count; i++) {
    //         if (spacecraft[i].reference == _shipTransform) {
    //             foundShip = i;
    //             break;
    //         }
    //     }

    //     entityInControl = spacecraft[foundShip];
    // }

    public void SwitchToPlayer() {
        entityInControl = player;
    }

    public Vector3 AdjustVector(Vector3 _v, int _id) {
        return new Vector3(_v.z * Mathf.Sin((float)celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * Mathf.Cos((float)celestialBodies[_id].rotation.y * (Mathf.PI / 180)), _v.y, _v.z * Mathf.Cos((float)celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * -Mathf.Sin((float)celestialBodies[_id].rotation.y * (Mathf.PI / 180)));
    }

    public Vector3 AdjustVectorReverse(Vector3 _v, int _id) {
        return new Vector3(_v.z * Mathf.Sin((float)-celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * Mathf.Cos((float)-celestialBodies[_id].rotation.y * (Mathf.PI / 180)), _v.y, _v.z * Mathf.Cos((float)-celestialBodies[_id].rotation.y * (Mathf.PI / 180)) + _v.x * -Mathf.Sin((float)-celestialBodies[_id].rotation.y * (Mathf.PI / 180)));
    }

    public Vector3 AdjustVector(Vector3 _v, float _amount) {
        return new Vector3(_v.z * Mathf.Sin(_amount * (Mathf.PI / 180)) + _v.x * Mathf.Cos(_amount * (Mathf.PI / 180)), _v.y, _v.z * Mathf.Cos(_amount * (Mathf.PI / 180)) + _v.x * -Mathf.Sin(_amount * (Mathf.PI / 180)));
    }

    // public List<D> GetPositions() {
    //     Vector4[] toReturn = new Vector3[celestialBodies.Length];
    //     for (int i = 0; i < toReturn.Length; i++) {
    //         toReturn[i] = celestialBodies[i].position.Sub(player.position);
    //     }
    //     return toReturn.ToList();
    // }
    
    public List<float> GetAngles() {
        float[] toReturn = new float[celestialBodies.Length];
        for (int i = 0; i < toReturn.Length; i++) {
            toReturn[i] = (float)celestialBodies[i].rotation.y;
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
