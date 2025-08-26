using TMPro;
using UnityEngine;

public class TimewarpMenu : MonoBehaviour
{
    public TextMeshProUGUI timeScaleDisplay;

    void Update()
    {
        timeScaleDisplay.text = SolarSystemController.I.universalTimeScale.ToString() + "x";
    }
}
