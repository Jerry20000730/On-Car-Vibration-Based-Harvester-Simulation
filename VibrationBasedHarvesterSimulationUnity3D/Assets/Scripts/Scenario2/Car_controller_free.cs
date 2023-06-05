using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Car_controller_free : MonoBehaviour
{

    // two wheel colliders
    public WheelCollider Wheel_Left_Collider;
    public WheelCollider Wheel_Right_Collider;

    public Rigidbody my_rigidbody;
    public Camera followCamera;

    // two wheel model corresponding to four wheel colliders
    public Transform Wheel_Left_Model;
    public Transform Wheel_Right_Model;

    // speed of the car
    private float speed;
    // max angle
    private float maxAngle = 45f;
    // current angle
    private float currentAngle;
    // angle speed
    private float angleSpeed = 30f;

    private bool isChangingDirection = false;

    // Start is called before the first frame update
    void Start()
    {
        speed = 10f;
        my_rigidbody = gameObject.GetComponent<Rigidbody>();
        currentAngle = my_rigidbody.rotation.eulerAngles.y;
        my_rigidbody.isKinematic = true;
        Wheel_Left_Collider.isTrigger = false;
        Wheel_Right_Collider.isTrigger = false;

        my_rigidbody.transform.position = new Vector3(my_rigidbody.transform.position.x, currentHeightY(my_rigidbody.transform.position, Wheel_Left_Collider), my_rigidbody.transform.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        // ground layer
        int Rmask = LayerMask.GetMask("Ground");

        Vector3 Point_dir = transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(transform.position, Point_dir, out hit, 50.0f, Rmask))
        {
            Quaternion NextRot = Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.Cross(transform.forward, hit.normal)), hit.normal);
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Lerp(transform.rotation, NextRot, 0.1f));
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            my_rigidbody.isKinematic = false;
            my_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            if (!isChangingDirection)
                this.my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
            else
                this.my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
        }

        // WheelMove(Wheel_Left_Model);
        // WheelMove(Wheel_Right_Model);

        WheelRotate();

        WheelModelUpdate(Wheel_Left_Model, Wheel_Left_Collider);
        WheelModelUpdate(Wheel_Right_Model, Wheel_Right_Collider);

        if (my_rigidbody.velocity.magnitude != this.speed && my_rigidbody.isKinematic == false)
        {
            if (!isChangingDirection) this.my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
            else this.my_rigidbody.velocity = this.my_rigidbody.velocity = my_rigidbody.transform.forward * this.speed;
        }
    }

    void WheelMove(Transform t)
    {
        t.RotateAround(t.position, t.right, 5f);
    }

    void WheelRotate()
    {
        // logic of turning left
        if (Input.GetKey(KeyCode.A))
        {
            isChangingDirection = true;
            Wheel_Left_Collider.steerAngle -= angleSpeed * Time.deltaTime;
            Wheel_Right_Collider.steerAngle -= angleSpeed * Time.deltaTime;
            currentAngle -= angleSpeed * Time.deltaTime;
            my_rigidbody.transform.localEulerAngles = new Vector3(0, currentAngle, 0);

            // if the steerAngle is bigger than the max angle, set the constraints on angle.
            if (Wheel_Left_Collider.steerAngle < -maxAngle || Wheel_Right_Collider.steerAngle < -maxAngle)
            {
                Wheel_Left_Collider.steerAngle = Wheel_Right_Collider.steerAngle = -maxAngle;
            }
        }

        // logic of turning right
        if (Input.GetKey(KeyCode.D))
        {
            isChangingDirection = true;
            Wheel_Left_Collider.steerAngle += angleSpeed * Time.deltaTime;
            Wheel_Right_Collider.steerAngle += angleSpeed * Time.deltaTime;
            currentAngle += angleSpeed * Time.deltaTime;
            my_rigidbody.transform.localEulerAngles = new Vector3(0, currentAngle, 0);

            // if the steerAngle is bigger than the max angle, set the constraints on angle.
            if (Wheel_Left_Collider.steerAngle > maxAngle || Wheel_Right_Collider.steerAngle > maxAngle)
            {
                Wheel_Left_Collider.steerAngle = Wheel_Right_Collider.steerAngle = maxAngle;
            }
        }

        // logic of releasing the turning left or right button
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            isChangingDirection = false;
            Wheel_Left_Collider.steerAngle = Wheel_Right_Collider.steerAngle = 0;
        }
    }

    void WheelModelUpdate(Transform t, WheelCollider wc)
    {
        Quaternion rot = t.rotation;
        wc.GetWorldPose(out _, out rot);
        t.rotation = rot;
    }

    public void updateSpeed(float updateSpeed)
    {
        speed = updateSpeed;
    }

    float currentHeightY(Vector3 pos, WheelCollider wheel)
    {
        return Mathf.Abs(wheel.transform.localPosition.y) + wheel.radius + Terrain.activeTerrain.SampleHeight(pos);
    }
}
