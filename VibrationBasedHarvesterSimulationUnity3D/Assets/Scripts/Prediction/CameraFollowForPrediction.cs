using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowForPrediction : MonoBehaviour
{
    public Transform followObject;
    Vector3 offsetPosition;
    Vector3 targetPosition;
    private float height = 30.0f;
    private float distance = 20.0f;
    private float followSpeed = 4.0f;
    private float scrollSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        offsetPosition = this.transform.position - followObject.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Follow();
        ScrollView();
    }
    void Follow()
    {
        targetPosition = followObject.position + Vector3.up * height - followObject.forward * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.LookAt(followObject);
    }

    void ScrollView()
    {
        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        distance = Mathf.Clamp(distance, 10, 100);
        offsetPosition = offsetPosition.normalized * distance;
    }
}
