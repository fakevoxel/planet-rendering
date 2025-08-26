using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingManager : MonoBehaviour
{
    private static RenderingManager _instance;

    public static RenderingManager Instance {
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

    private Camera c;

    public LayerMask normalView;
    public LayerMask mapView;
    public bool isMapActive;

    public float planetRotSpeed;
    public float originRadius;
    public float renderRadius;
    public cbr_floatingentity[] celestialBodies;
    public List<cbr_floatingentity> spacecraft;
    public cbr_floatingentity player;
    public List<cbr_fixedentity> structures;
    public Vector3 worldOffset;
    public cbr_floatingentity entityInControl;

    void Awake()
    {
        Instance = this;
        c = GetComponent<Camera>();

        worldOffset = Vector3.zero;

        // celestialBodies = new cbr_floatingentity[1] {new cbr_floatingentity(GameObject.Find("Planet").transform)};
        // player = new cbr_floatingentity(GameObject.Find("Player").transform);

        // spacecraft = new List<cbr_floatingentity>();
        // structures = new List<cbr_fixedentity>();
    }

    void Start()
    {
        c.cullingMask = normalView;
        CloseMap();
    }

    public void OpenMap()
    {
        c.cullingMask = mapView;
        isMapActive = true;
        TrackingManager.Instance.t_iconContainer.gameObject.SetActive(true);
    }
    public void CloseMap()
    {
        c.cullingMask = normalView;
        isMapActive = false;
        TrackingManager.Instance.t_iconContainer.gameObject.SetActive(false);
    }
}
