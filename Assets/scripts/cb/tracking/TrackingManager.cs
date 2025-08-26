using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TrackingManager : MonoBehaviour
{
    private static TrackingManager _instance;

    public static TrackingManager Instance
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
    public trackedbody_mono[] bodies;
    private float editorRefreshDelay;
    private float lastEditorRefresh;

    public Material m_line;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        universalTime = 0;
        universalTimeScale = 1;
        editorRefreshDelay = 1;
        lastEditorRefresh = -10;
    }

    public void InitializeSystem(solarsystem system)
    {
        bodies = new trackedbody_mono[system.bodies.Length];
        // first, before anything else, we need to create the objects for the planets based on the solar system
        for (int i = 0; i < system.bodies.Length; i++)
        {
            GameObject g_newBody = new GameObject();
            g_newBody.name = system.bodies[i].name;

            trackedbody_mono comp = g_newBody.AddComponent<trackedbody_mono>();
            // pass over the data
            comp.config = system.bodies[i].config;
            comp.orbit = system.bodies[i].orbit;
            comp.pose = system.bodies[i].pose;

            comp.orbit.data = comp;

            g_newBody.transform.SetParent(transform);
            bodies[i] = comp;

            if (system.bodies[i].orbit.parentIndex != -1)
            {
                bodies[i].orbit.parent = bodies[system.bodies[i].orbit.parentIndex].orbit;
            }
        }

        // *** FROM HERE ON WE USE LOCAL BODIES ARRAY ***

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

        if (bodies[bodyIndex].orbit.isGrandparent) { return; }

        t_newPlotter.gameObject.AddComponent<LineRenderer>();
        t_newPlotter.gameObject.GetComponent<LineRenderer>().sharedMaterial = m_line;
        t_newPlotter.gameObject.GetComponent<LineRenderer>().material.color = Color.blue;
        t_newPlotter.gameObject.AddComponent<Plotter>().l = t_newPlotter.gameObject.GetComponent<LineRenderer>();
    }

    void RefreshPlotter(int bodyIndex)
    {
        Plotter comp = t_plotterContainer.GetChild(bodyIndex).GetComponent<Plotter>();
        if (comp == null) { return; }
        comp.Plot(bodies[bodyIndex].orbit.SampleFullOrbit(2000));
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
            bodies[i].orbit.Initialize();
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
            t_iconContainer.GetChild(n).position = Camera.main.WorldToScreenPoint(bodies[n].pose.position * Sys.mapViewScalingFactor);
        }
    }

    public void UpdateAllPlanetPositions()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].orbit.isGrandparent) { continue; }
            bodies[i].transform.position = bodies[i].orbit.GetPositionAtTime(universalTime);
        }
    }

    // the actual transform objects, both scale and position, so they appear where they would in the actual world
    // using the CameraController's position var (NOT transform.position) to get where the camera is in space
    public void UpdateAllPlanetTransforms()
    {

    }

    public void IncreaseTimewarpSpeed()
    {
        if (universalTimeScale == -1)
        {
            universalTimeScale = 1;
        }
        else if (universalTimeScale < 0)
        {
            universalTimeScale /= 10;
        }
        else
        {
            universalTimeScale *= 10;
        }
    }

    public void DecreaseTimewarpSpeed()
    {
        if (universalTimeScale == 1)
        {
            universalTimeScale = -1;
        }
        else if (universalTimeScale < 0)
        {
            universalTimeScale *= 10;
        }
        else
        {
            universalTimeScale /= 10;
        }
    }
}
