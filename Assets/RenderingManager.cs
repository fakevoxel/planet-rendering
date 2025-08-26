using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingManager : MonoBehaviour
{
    private static RenderingManager _instance;

    public static RenderingManager I {
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

    void Awake()
    {
        I = this;
        c = GetComponent<Camera>();
    }

    private Camera c;

    public LayerMask normalView;
    public LayerMask mapView;
    public bool isMapActive;

    void Start()
    {
        c.cullingMask = normalView;
        CloseMap();
    }

    public void OpenMap()
    {
        c.cullingMask = mapView;
        isMapActive = true;
        SolarSystemController.I.t_iconContainer.gameObject.SetActive(true);
    }
    public void CloseMap()
    {
        c.cullingMask = normalView;
        isMapActive = false;
        SolarSystemController.I.t_iconContainer.gameObject.SetActive(false);
    }
}
