using TMPro;
using UnityEngine;

// just controlling the timewarp menu, that's it
public class TimewarpMenu : MonoBehaviour
{
    public TextMeshProUGUI timeScaleDisplay;

    void Update()
    {
        timeScaleDisplay.text = TrackingManager.Instance.universalTimeScale.ToString() + "x";
    }
}
