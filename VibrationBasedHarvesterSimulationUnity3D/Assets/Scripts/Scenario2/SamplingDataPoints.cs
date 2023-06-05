using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SamplingDataPoints
{
    public float time;
    public float height;
}

[Serializable]
public class SamplingDataPointsCollection
{
    [SerializeField]
    List<SamplingDataPoints> sampling_data_points_list;
    public SamplingDataPointsCollection(List<SamplingDataPoints> list)
    {
        this.sampling_data_points_list = list;
    } 
}
