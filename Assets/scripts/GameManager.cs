using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProgramStartMode
{
    Normal, InstantLoad, Editor
    // similar to SUPPLY RUN we basically have: main menu, game, system editor
    // (instead of: main menu, game, chunk editor)
}

// okay so we're not technically a *game* yet but still using this concept because I thought it worked well

// FOR REFERENCE: like main.java, all we're doing is calling other functions
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
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

    public ProgramStartMode startMode;
    public solarsystem loadedSystem; // will eventually be loaded from disk

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (startMode == ProgramStartMode.InstantLoad)
        {
            InitializeSystem();
        }
        else if (startMode == ProgramStartMode.Editor)
        {
            InitializeSystem();
        }
    }

    void InitializeSystem()
    {
        TrackingManager.Instance.InitializeSystem(loadedSystem);
        RenderingManager.Instance.InitializeSystem(loadedSystem);

        TrackingManager.Instance.InitializeTerrain();

        RenderingManager.Instance.SwitchToPlayer();
    }

    void Update()
    {
        if (startMode == ProgramStartMode.InstantLoad)
        {
            RenderingManager.Instance.UpdateAll();
            TrackingManager.Instance.UpdateAll();
            
        }
        // else if (startMode == ProgramStartMode.Editor)
        // {
        //     TrackingManager.Instance.RefreshAllOrbits();
        //     TrackingManager.Instance.UpdateAllPlanetIcons();
        // }
    }
}
