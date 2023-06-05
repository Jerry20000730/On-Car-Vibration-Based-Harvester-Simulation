using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchForPrediction : MonoBehaviour
{
    public Camera car_camera;
    public Camera third_party_camera;
    public Camera god_camera;

    private Camera last_camera;

    // Start is called before the first frame update
    void Start()
    {
        god_camera.enabled = true;
        third_party_camera.enabled = false;
        car_camera.enabled = false; 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            god_camera.enabled = false;
            car_camera.enabled = false;
            third_party_camera.enabled = true;
        }
    }
}
