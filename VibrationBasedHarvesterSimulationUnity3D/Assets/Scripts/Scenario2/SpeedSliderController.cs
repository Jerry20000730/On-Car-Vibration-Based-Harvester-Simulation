using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSliderController : MonoBehaviour
{
    private Slider speed_slider;
    private TextMeshProUGUI speed_value;
    // ratio (unity unit to real life)
    private float ratio_to_reality;

    void Start()
    {
        speed_slider = GameObject.Find("Canvas/speed_slider").GetComponent<Slider>();
        speed_value = GameObject.Find("Canvas/speed_value").GetComponent<TextMeshProUGUI>();
        speed_slider.onValueChanged.AddListener(delegate { onValueChanged(); });
        ratio_to_reality = 1.0f / 96.0f;
        speed_value.text = "0.1m/s";
    }

    void onValueChanged()
    {
        speed_value.text = speed_slider.value * ratio_to_reality + "m/s";
        Debug.Log(speed_slider.value);
        GameObject.Find("Car").GetComponent<Car_controller_free>().SendMessage("updateSpeed", speed_slider.value);
    }

}
