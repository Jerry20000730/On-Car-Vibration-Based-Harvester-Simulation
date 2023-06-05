using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class DataCollectionPowerDisplay : MonoBehaviour
{
    private Label _power_display;
    private Label _speed_display;
    private Label _height_display;
    private Label _terrainID_display;

    public Rigidbody my_rigidbody;
    public WheelCollider Wheel_left_collider;
    public WheelCollider Wheel_right_collider;

    public UIDocument ui;

    // omega of the wheel
    private float omega = 0.0f;

    // calculator
    private VibrationCalculator vc;

    // sampling utility
    private DataCollectionSampling sp;

    // requester: for communication between unity and python server
    private Requester requester;

    // listening on this queue, to see if there is an update
    // if so, update in main ui thread
    private readonly ConcurrentQueue<Action> messageQueue = new ConcurrentQueue<Action>();

    // ratio (unity unit to real life)
    private float ratio_to_reality;

    void OnEnable()
    {
        my_rigidbody = GetComponent<Rigidbody>();
        _power_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("power_indicator");
        _speed_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("speed_indicator");
        _height_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("height_indicator");
        _terrainID_display = ui.rootVisualElement.Q<GroupBox>("information").Q<Label>("terrainID_indicator");
        ratio_to_reality = 1.0f / 96.0f;

        _power_display.text = "Power: 0.0mW";
        _speed_display.text = "Speed: " + String.Format("{0:N1}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
        _height_display.text = "Height: " + center(Wheel_left_collider, Wheel_right_collider) + "(wheel)";
        _terrainID_display.text = "TerrainID: " + Path.GetFileName(TerrainSetting.getTerrainSetting().getFilePath());
    }

    private void Start()
    {
        vc = new VibrationCalculator();
        sp = new DataCollectionSampling(Wheel_left_collider, Wheel_right_collider);
        BatUtility.runBatProcess("run.bat");
        requester = new Requester();
        requester.Start((float Y_approx) => messageQueue.Enqueue(() =>
            {
                vc.setY(Y_approx);
            }
        ));
    }

    // Update is called once per frame
    void Update()
    {
        _speed_display.text = "Speed: " + String.Format("{0:N2}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
        _power_display.text = "Power: " + String.Format("{0:N2}", vc.Calculate() * 1000) + "mW";
        _height_display.text = "Height: " + center(Wheel_left_collider, Wheel_right_collider) + "(wheel)";

        if (!sp.isEnd())
        {
            sp.onSampling();
        } else
        {
            requester.setSamplingData(sp.getJSON());
            sp.Reset();
        }
        omega = my_rigidbody.velocity.magnitude / Wheel_left_collider.radius;
        vc.setOmega(omega);

        if (!messageQueue.IsEmpty)
        {
            Action action;
            while(messageQueue.TryDequeue(out action))
            {
                action.Invoke();
            }
        }
    }

    private void OnDestroy()
    {
        requester.Stop();
        BatUtility.runBatProcess("stop.bat");
    }

    Vector3 center(WheelCollider left, WheelCollider right)
    {
        return new Vector3((left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).x) / 2,
                           (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).y) / 2,
                           (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).z) / 2);
    }
}


