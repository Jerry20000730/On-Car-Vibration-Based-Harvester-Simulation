using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    public Camera any_camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = WorldTouchPos();
            Debug.Log("点击-世界位置" + pos);
        }
    }

    public Vector3 WorldTouchPos()
    {
        return any_camera.ScreenToWorldPoint(GetTouchPosition());
    }

    public static Vector3 GetTouchPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return Input.mousePosition;
#else
        return Input.touches[0].position;
#endif
    }
}
