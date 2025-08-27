using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;

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

    public Mesh planetMesh;
    public Material[] bodyMaterials; // to be replaced with more systems soon

    private DoubleVector3[] firstPositionFrames;
    private DoubleVector3[] secondPositionFrames;
    private float timeBetweenInterjections;
    private float lastInterjectionTime;
    private bool isCalculatingInterjection;
    private bool isInterjectionReady;
    private float interjectEpsilon = 0.5f;
    private float timeToApplyInterjection;

    public GameObject p_genericPlanet;

    void Awake()
    {
        Instance = this;

        Application.targetFrameRate = 60;
        universalTime = 0;
        universalTimeScale = 1;
        editorRefreshDelay = 1;
        lastEditorRefresh = -10;

        timeBetweenInterjections = 5; // seconds
        lastInterjectionTime = 0;

        isInterjectionReady = false;
        isCalculatingInterjection = false;
    }

    public void InitializeSystem(solarsystem system)
    {
        bodies = new trackedbody_mono[system.bodies.Length];
        // first, before anything else, we need to create the objects for the planets based on the solar system
        for (int i = 0; i < system.bodies.Length; i++)
        {
            GameObject g_newBody = new GameObject();
            g_newBody.name = system.bodies[i].name;

            // g_newBody.AddComponent<MeshFilter>().sharedMesh = planetMesh;
            // g_newBody.AddComponent<MeshRenderer>().sharedMaterial = bodyMaterials[i];
            //g_newBody.transform.localScale = Vector3.one * system.bodies[i].config.equitorialRadius * 2f;

            trackedbody_mono comp = g_newBody.AddComponent<trackedbody_mono>();
            // pass over the data
            comp.config = system.bodies[i].config;
            comp.orbit = system.bodies[i].orbit;
            comp.pose = system.bodies[i].pose;

            comp.orbit.data = comp;
            comp.pose.data = comp;

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

        firstPositionFrames = new DoubleVector3[bodies.Length];
        secondPositionFrames = new DoubleVector3[bodies.Length];
    }

    public void InitializeTerrain()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            if (i == 1)
            {
                GameObject g_newPlanetObj = Instantiate(p_genericPlanet, Vector3.zero, Quaternion.identity);
                g_newPlanetObj.transform.SetParent(transform.GetChild(i));

                g_newPlanetObj.GetComponent<Planet>().radius = bodies[i].config.equitorialRadius;

                g_newPlanetObj.GetComponent<Planet>().Initialize();
            }
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

        t_newPlotter.gameObject.GetComponent<Plotter>().l.useWorldSpace = false;
    }

    void RefreshPlotter(int bodyIndex)
    {
        Plotter comp = t_plotterContainer.GetChild(bodyIndex).GetComponent<Plotter>();
        if (comp == null) { return; }
        if (bodies[bodyIndex].orbit.parent != null) {
            comp.Plot(bodies[bodyIndex].orbit.SampleFullOrbit(1000), bodies[bodyIndex].orbit.parentIndex);
        } else {
            comp.Plot(bodies[bodyIndex].orbit.SampleFullOrbit(1000), -1);
        }
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
    public void UpdateAll()
    {
        universalTime += Time.deltaTime * universalTimeScale;

        UpdateAllPlanetPositions(Time.deltaTime * universalTimeScale);
        UpdateAllPlanetIcons();

        // interjecting the planet positions
        if (!isCalculatingInterjection)
        {
            // (no interjects during timewarp)
            if (universalTimeScale == 1 && universalTime > lastInterjectionTime + timeBetweenInterjections)
            {
                isCalculatingInterjection = true;
                CalculateInterjection(universalTime + 10);
                timeToApplyInterjection = universalTime + 10;

                lastInterjectionTime = universalTime;
            }
        }
        else
        {
            if (isInterjectionReady && universalTime > timeToApplyInterjection)
            {

                if (universalTimeScale != -1)
                {
                    Debug.Log("Injerjection ignored due to timewarp!");
                }
                else if (Mathf.Abs(universalTime - timeToApplyInterjection) > 0.5f) {
                    Debug.Log("Injerjection ignored due to time difference!");
                }
                else
                {
                    for (int i = 0; i < bodies.Length; i++)
                    {
                        //Debug.Log("Interjection applied! New pose: " + firstPositionFrames[i].x.ToString() + "," + firstPositionFrames[i].y.ToString() + "," + firstPositionFrames[i].z.ToString());
                        Debug.Log("Interjection applied! Error: " + (bodies[i].pose.localPosition.x - firstPositionFrames[i].x).ToString() + "," + (bodies[i].pose.localPosition.y - firstPositionFrames[i].y).ToString() + "," + (bodies[i].pose.localPosition.z - firstPositionFrames[i].z).ToString());

                        bodies[i].pose.localPosition = firstPositionFrames[i];
                        bodies[i].pose.velocity = secondPositionFrames[i].Sub(firstPositionFrames[i]).Div(0.5f);
                    }
                }

                for (int i = 0; i < bodies.Length; i++)
                {
                    // clearing
                    firstPositionFrames[i] = new DoubleVector3(0, 0, 0);
                    secondPositionFrames[i] = new DoubleVector3(0, 0, 0);
                }

                isCalculatingInterjection = false;
                isInterjectionReady = false;

                lastInterjectionTime = universalTime;
            }
        }
    }

    public async void CalculateInterjection(float timeToCalculate)
    {
        Debug.Log("Interjection started!");

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].orbit.isGrandparent)
            {
                firstPositionFrames[i] = new DoubleVector3(0, 0, 0);
                secondPositionFrames[i] = new DoubleVector3(0, 0, 0);
            }
            else
            {
                firstPositionFrames[i] = await HighPrecisionPlanetPose(i, timeToCalculate);
                secondPositionFrames[i] = await HighPrecisionPlanetPose(i, timeToCalculate + interjectEpsilon);
            }
        }

        // once we have the interjection, wait to apply it
        isInterjectionReady = true;
        Debug.Log("Interjection finished. Time: " + (universalTime - lastInterjectionTime).ToString());
    }

    public Task<DoubleVector3> HighPrecisionPlanetPose(int bodyIndex, float time)
    {
        return Task.Factory.StartNew(() => bodies[bodyIndex].orbit.GetPositionAtTime(time, 2000000));
    }

    public void UpdateAllPlanetIcons()
    {
        // now the icons
        for (int n = 0; n < t_iconContainer.childCount; n++)
        {
            // UNIVERSAL POSITION PLS THX
            t_iconContainer.GetChild(n).position = Camera.main.WorldToScreenPoint(bodies[n].pose.GetPosition().Mul(Sys.mapViewScalingFactor).ToVector3());
        }
    }

    public void UpdateAllPlanetPositions(float delta)
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].orbit.isGrandparent) { continue; }
            if (universalTimeScale == 1)
            {
                bodies[i].pose.StepNewtonian(delta);
            }
            else
            {
                bodies[i].pose.localPosition = bodies[i].orbit.GetPositionAtTime(universalTime, 1000);
            }
        }
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
