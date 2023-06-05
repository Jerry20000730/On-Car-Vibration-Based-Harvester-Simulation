using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_Body : MonoBehaviour
{
    public Rigidbody wheel_rigidbody;
    private Rigidbody my_rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        my_rigidbody = GetComponent<Rigidbody>();
        my_rigidbody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        my_rigidbody.transform.position = new Vector3(wheel_rigidbody.position.x, wheel_rigidbody.position.y + 4.0f, wheel_rigidbody.position.z);
    }
}
