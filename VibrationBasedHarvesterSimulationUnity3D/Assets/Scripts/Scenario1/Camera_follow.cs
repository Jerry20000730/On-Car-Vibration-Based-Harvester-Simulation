using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_follow : MonoBehaviour
{
    #region
    public Transform followObject;
    Vector3 vector;

    #endregion

    #region
    // Use this for initialization
    void Start()
    {
        vector = this.transform.position - followObject.position;
    }

    private void LateUpdate()
    {
        ToFollow();
    }
    #endregion

    #region

    void ToFollow()
    {
        this.transform.position = followObject.position + new Vector3(vector.x, this.transform.position.y - followObject.position.y, vector.z);
    }
    #endregion
}
