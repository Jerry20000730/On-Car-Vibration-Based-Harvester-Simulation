using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCarController : MonoBehaviour
{
    // two wheel colliders
    public WheelCollider Wheel_Left_Collider;
    public WheelCollider Wheel_Right_Collider;

    public Rigidbody my_rigidbody;

    // two wheel model corresponding to four wheel colliders
    public Transform Wheel_Left_Model;
    public Transform Wheel_Right_Model;

    // speed of the car
    private float speed;

    // Start is called before the first frame update
    void Start()
    {
        speed = 0.5f;
        my_rigidbody = gameObject.GetComponent<Rigidbody>();
        my_rigidbody.isKinematic = true;
        Wheel_Left_Collider.isTrigger = false;
        Wheel_Right_Collider.isTrigger = false;
    }

    // Update is called once per frame
    void Update()
    {
        my_rigidbody.isKinematic = false;
        my_rigidbody.freezeRotation = true;
        this.my_rigidbody.velocity = Vector3.left * this.speed;

        WheelModel_Update(Wheel_Left_Model, Wheel_Left_Collider);
        WheelModel_Update(Wheel_Right_Model, Wheel_Right_Collider);

        exceed();

        if (my_rigidbody.velocity.magnitude < this.speed && my_rigidbody.isKinematic == false)
        {
            this.my_rigidbody.velocity = Vector3.left * this.speed;
        }

    }

    void WheelModel_Update(Transform t, WheelCollider wheel_collider)
    {
        t.RotateAround(t.position, t.right, 5f);
    }

    void exceed()
    {
        if (my_rigidbody.transform.position.x <= 4.0f)
        {
            my_rigidbody.transform.position = new Vector3(32.0f, 234.381f, 19.74f);
        }
    }
}
