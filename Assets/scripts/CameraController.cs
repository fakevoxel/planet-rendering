using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// two modes really, freecam and orbit
// BUT EVERYTHING'S PARENTED TO A GIVEN PLANET!!!
public class CameraController : MonoBehaviour
{
    private static CameraController _instance;

    public static CameraController Instance
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

    void Awake()
    {
        Instance = this;
    }

    public float cameraMovespeed;

    public int parentBodyIndex = 0; // the sun, by default
    public Vector3 positionOffset;
    public float panSpeed;

    void Update()
    {
        Vector3 directionToMoveIn = Vector3.zero;

        directionToMoveIn += transform.right * Input.GetAxis("Horizontal");
        directionToMoveIn += transform.forward * Input.GetAxis("Vertical");
        directionToMoveIn += transform.up * Input.GetAxis("Elevation");

        RenderingManager.Instance.entityInControl.position = TrackingManager.Instance.bodies[parentBodyIndex].pose.GetPosition().Add(Vector3.forward * TrackingManager.Instance.bodies[parentBodyIndex].config.equitorialRadius * 1.25f).Add(positionOffset);

        if (transform.parent != null)
        {
            // using Update() to allow for fast input processing, deltaTime should avoid FPS messing with stuff
            positionOffset += directionToMoveIn.normalized * cameraMovespeed * Time.deltaTime;
        }
        else
        {
            transform.position += directionToMoveIn.normalized * cameraMovespeed * Time.deltaTime;
        }

        if (Input.GetMouseButton(1))
        {
            transform.Rotate(Vector3.up * -Input.GetAxis("Mouse X") * panSpeed, Space.World);
            transform.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * panSpeed, Space.Self);
        }

        // keeping all keybinds here for now
        if (Input.GetKeyDown("m"))
        {
            if (!RenderingManager.Instance.isMapActive)
            {
                RenderingManager.Instance.OpenMap();
            }
            else
            {
                RenderingManager.Instance.CloseMap();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (parentBodyIndex < TrackingManager.Instance.bodies.Length - 1)
            {
                parentBodyIndex++;
            } else { parentBodyIndex = 0; }
        }

        // even this one
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TrackingManager.Instance.IncreaseTimewarpSpeed();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TrackingManager.Instance.DecreaseTimewarpSpeed();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TrackingManager.Instance.universalTimeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TrackingManager.Instance.universalTimeScale = 0;
        }
    }

    public Vector3 GetPositionOffset()
    {
        return Vector3.zero;
    }
}
