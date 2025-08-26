using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// two modes really, freecam and orbit
// BUT EVERYTHING'S PARENTED TO A GIVEN PLANET!!!
public class CameraController : MonoBehaviour
{
    public float cameraMovespeed;

    void Update()
    {
        Vector3 directionToMoveIn = Vector3.zero;

        directionToMoveIn += transform.right * Input.GetAxis("Horizontal") * cameraMovespeed;
        directionToMoveIn += transform.forward * Input.GetAxis("Vertical") * cameraMovespeed;
        directionToMoveIn += transform.up * Input.GetAxis("Elevation") * cameraMovespeed;

        // using Update() to allow for fast input processing, deltaTime should avoid FPS messing with stuff
        transform.position += directionToMoveIn * Time.deltaTime;

        // keeping all keybinds here for now
        if (Input.GetKeyDown("m"))
        {
            if (!RenderingManager.I.isMapActive)
            {
                RenderingManager.I.OpenMap();
            }
            else
            {
                RenderingManager.I.CloseMap();
            }
        }

        // even this one
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SolarSystemController.I.universalTimeScale += 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SolarSystemController.I.universalTimeScale -= 1;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SolarSystemController.I.universalTimeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SolarSystemController.I.universalTimeScale =0;
        }
    }
}
