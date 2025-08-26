using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SolarSystemController : MonoBehaviour
{
    private static SolarSystemController _instance;

    public static SolarSystemController I
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

    public float universalTime;
    public float universalTimeScale;

    public Transform t_plotterContainer;
    public Transform t_iconContainer;
    public BodyPosition[] bodies; // sun is always first
    private float editorRefreshDelay;
    private float lastEditorRefresh;

    public Material m_line;

    void Awake()
    {
        I = this;
        Application.targetFrameRate = 60;
        universalTime = 0;
        universalTimeScale = 1;
        editorRefreshDelay = 1;
        lastEditorRefresh = -10;
    }

    public void InitializeSystem()
    {
        CalculatePlanetOrbits();

        // initializing the plotters
        for (int i = 0; i < bodies.Length; i++)
        {
            // map icons first. not all bodies need a plotter, but they all need an icon

            CreatePlanetIcon();
            CreatePlotter(i);
            RefreshPlotter(i);
        }
    }

    public void RefreshAllOrbits()
    {
        CalculatePlanetOrbits();

        for (int i = 0; i < bodies.Length; i++)
        {
            RefreshPlotter(i);
        }
    }

    void CreatePlanetIcon()
    {
        RectTransform t_newIcon = new GameObject().AddComponent<RectTransform>();
        t_newIcon.gameObject.AddComponent<Image>();
        t_newIcon.GetComponent<Image>().sprite = Sys.sprites[0];
        t_newIcon.SetParent(t_iconContainer);
        t_newIcon.sizeDelta = Vector2.one * 50;
    }

    void CreatePlotter(int bodyIndex)
    {
        Transform t_newPlotter = new GameObject().transform;
        t_newPlotter.SetParent(t_plotterContainer);
        t_newPlotter.gameObject.name = bodies[bodyIndex].name;
        t_newPlotter.gameObject.layer = 6;

        if (bodies[bodyIndex].isGrandparent) { return; }

        t_newPlotter.gameObject.AddComponent<LineRenderer>();
        t_newPlotter.gameObject.GetComponent<LineRenderer>().sharedMaterial = m_line;
        t_newPlotter.gameObject.GetComponent<LineRenderer>().material.color = Color.blue;
        t_newPlotter.gameObject.AddComponent<Plotter>().l = t_newPlotter.gameObject.GetComponent<LineRenderer>();
    }

    void RefreshPlotter(int bodyIndex)
    {
        Plotter comp = t_plotterContainer.GetChild(bodyIndex).GetComponent<Plotter>();
        if (comp == null) { return; }
        comp.Plot(bodies[bodyIndex].SampleFullOrbit(2000));
    }

    public void CalculatePlanetOrbits()
    {
        if (Time.time > lastEditorRefresh + editorRefreshDelay)
        {
            lastEditorRefresh = Time.time;
        }
        else { return; }
        // initializing the bodies
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].Initialize();
        }
    }

    // this is a function so i can insert logic for swapping sim methods
    public void SetTimeScale(float newScale)
    {
        universalTimeScale = newScale;
    }

    // using the default unity physics timestep to update planets
    public void UpdateAllPlanets()
    {
        universalTime += Time.deltaTime * universalTimeScale;

        UpdateAllPlanetPositions();
        UpdateAllPlanetTransforms();
        UpdateAllPlanetIcons();
    }

    public void UpdateAllPlanetIcons()
    {
        // now the icons
        for (int n = 0; n < t_iconContainer.childCount; n++)
        {
            // UNIVERSAL POSITION PLS THX
            t_iconContainer.GetChild(n).position = Camera.main.WorldToScreenPoint(bodies[n].position * Sys.mapViewScalingFactor);
        }
    }

    public void UpdateAllPlanetPositions()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].isGrandparent) { continue; }
            bodies[i].transform.position = bodies[i].GetPositionAtTime(universalTime);
        }
    }

    // the actual transform objects, both scale and position, so they appear where they would in the actual world
    // using the CameraController's position var (NOT transform.position) to get where the camera is in space
    public void UpdateAllPlanetTransforms()
    {
        
    }
}
