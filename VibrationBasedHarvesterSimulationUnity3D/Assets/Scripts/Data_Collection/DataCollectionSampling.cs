using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollectionSampling
{
    private float time = 1.0f;
    private float past_time = 0.0f;
    private float sampling_rate = 100f;
    float interval;

    private WheelCollider Wheel_left_collider;
    private WheelCollider Wheel_right_collider;

    // list to store sampling data points
    private List<SamplingDataPoints> sampling_data_list;

    public DataCollectionSampling(WheelCollider left, WheelCollider right)
    {
        this.Wheel_left_collider = left;
        this.Wheel_right_collider = right;

        interval = time / sampling_rate;
        sampling_data_list = new List<SamplingDataPoints>();
        SamplingDataPoints dp = new SamplingDataPoints();
        dp.time = float.Parse(past_time.ToString("f2"));
        dp.height = centerY(Wheel_left_collider, Wheel_right_collider);
        sampling_data_list.Add(dp);
    }

    public void onSampling()
    {
        // record the initial point
        interval -= Time.deltaTime;
        // if interval <= 0, which means 0.01s has passed
        // record a point
        if (interval <= 0)
        {
            SamplingDataPoints dp = new SamplingDataPoints();
            interval = time / sampling_rate;
            past_time += interval;
            dp.time = float.Parse(past_time.ToString("f2"));
            dp.height = centerY(Wheel_left_collider, Wheel_right_collider);
            sampling_data_list.Add(dp);
        }
    }

    public bool isEnd()
    {
        return past_time > time;
    }

    float centerY(WheelCollider left, WheelCollider right)
    {
        return (left.transform.TransformPoint(left.center).x + right.transform.TransformPoint(right.center).y) / 2;
    }

    public string getJSON()
    {
        String json = JsonUtility.ToJson(new SamplingDataPointsCollection(sampling_data_list));
        Debug.Log("Unity Client> [INFO] Json content: " + json);
        return json;
    }

    public void Reset()
    {
        past_time = 0.0f;
        sampling_data_list.Clear();
        SamplingDataPoints dp = new SamplingDataPoints();
        dp.time = float.Parse(past_time.ToString("f2"));
        dp.height = centerY(Wheel_left_collider, Wheel_right_collider);
        sampling_data_list.Add(dp);
    }
}
