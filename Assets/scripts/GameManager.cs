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
    public ProgramStartMode startMode;
    public solarsystem loadedSystem; // will eventually be loaded from disk

    void Start()
    {
        if (startMode == ProgramStartMode.InstantLoad)
        {
            TrackingManager.Instance.InitializeSystem(loadedSystem);
        }
        else if (startMode == ProgramStartMode.Editor)
        {
            TrackingManager.Instance.InitializeSystem(loadedSystem);
        }
    }

    void Update()
    {
        if (startMode == ProgramStartMode.InstantLoad)
        {
            TrackingManager.Instance.UpdateAllPlanets();
        }
        else if (startMode == ProgramStartMode.Editor)
        {
            TrackingManager.Instance.RefreshAllOrbits();
            TrackingManager.Instance.UpdateAllPlanetIcons();
        }
    }
}
