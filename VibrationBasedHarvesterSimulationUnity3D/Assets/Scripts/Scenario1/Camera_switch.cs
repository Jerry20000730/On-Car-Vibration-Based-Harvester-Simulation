using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_switch : MonoBehaviour
{
    // declare two cameras
    public Camera Car_camera_above;
    public Camera Car_camera_side;
    public Camera Car_camera_front;

    // Start is called before the first frame update
    void Start()
    {
        Car_camera_above.enabled = true;
        Car_camera_side.enabled = false;
        Car_camera_front.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Car_camera_above.enabled = true;
            Car_camera_side.enabled = false;
            Car_camera_front.enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Car_camera_above.enabled = false;
            Car_camera_side.enabled = true;
            Car_camera_front.enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Car_camera_above.enabled = false;
            Car_camera_side.enabled = false;
            Car_camera_front.enabled = true;
        }
    }
}
