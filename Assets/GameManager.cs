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
public class GameManager : MonoBehaviour
{
    public ProgramStartMode startMode;

    void Start()
    {
        if (startMode == ProgramStartMode.InstantLoad)
        {
            SolarSystemController.I.InitializeSystem();
        }
        else if (startMode == ProgramStartMode.Editor)
        {
            SolarSystemController.I.InitializeSystem();
        }
    }

    void Update()
    {
        if (startMode == ProgramStartMode.InstantLoad)
        {
            SolarSystemController.I.UpdateAllPlanets();
        }
        else if (startMode == ProgramStartMode.Editor)
        {
            SolarSystemController.I.RefreshAllOrbits();
            SolarSystemController.I.UpdateAllPlanetIcons();
        }
    }
}
