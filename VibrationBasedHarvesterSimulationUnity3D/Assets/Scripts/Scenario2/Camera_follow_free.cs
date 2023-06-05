using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Camera_follow_free : MonoBehaviour
{
    public Transform followObject;
    Vector3 offsetPosition;
    Vector3 targetPosition;
    private float height = 15.0f;
    private float distance = 30.0f;
    private float followSpeed = 4.0f;
    private float scrollSpeed = 10f;

    // Use this for initialization
    void Start()
    {
        offsetPosition = this.transform.position - followObject.position;
    }

    private void LateUpdate()
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
