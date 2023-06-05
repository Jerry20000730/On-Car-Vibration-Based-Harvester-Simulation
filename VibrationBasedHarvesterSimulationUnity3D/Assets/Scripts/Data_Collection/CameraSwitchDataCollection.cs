using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchDataCollection : MonoBehaviour
{
    public Camera front_camera_perspective;
    public Camera front_camera_orthographic;
    private Camera front_camera;
    public Camera third_party_camera;
    public Camera god_camera;

    private Camera last_camera;

    private string projection;

    private void Start()
    {
        projection = ReadConfig.getProjection();
        if (projection == "perspective")
        {
            front_camera = front_camera_perspective;
        }
        else if (projection == "orthographic")
        {
            front_camera = front_camera_orthographic;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            if (third_party_camera.enabled == true)
            {
                last_camera = third_party_camera;
            }
            else if (front_camera.enabled == true)
            {
                last_camera = front_camera;
            }
            switchToGod();
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            resume(last_camera);
        }
    }

    void resume(Camera camera)
    {
        god_camera.enabled = false;
        camera.enabled = true;
    }

    void switchToGod()
    {
        god_camera.enabled = true;
        third_party_camera.enabled = false;
        front_camera.enabled = false;
    }
}
