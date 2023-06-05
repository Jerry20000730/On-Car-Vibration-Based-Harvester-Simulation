using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class Power_display_free : MonoBehaviour
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
    private Sampling sp;

    // requester: for communication between unity and python server
    private Requester requester;

    // listening on this queue, to see if there is an update
    // if so, update in main ui thread
    private readonly ConcurrentQueue<Action> messageQueue = new ConcurrentQueue<Action>();

    // ratio (unity unit to real life)
    private float ratio_to_reality;

    void OnEnable()
    {
        // setting the terrain
        TerrainSetting.getTerrainSetting().setTerrain(Terrain.activeTerrain);
        TerrainSetting.getTerrainSetting().sethmWidth(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().sethmHeight(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().setmResolution(Terrain.activeTerrain.terrainData.heightmapResolution);
        TerrainSetting.getTerrainSetting().setmFlipVertically(false);
        // begin setting the terrain
        TerrainSetting.getTerrainSetting().setHeightmap();

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
        sp = new Sampling(Wheel_left_collider, Wheel_right_collider);
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

    class VibrationCalculator
    {
        // parameters regarding the external factors
        private float omega;
        private float Y;
        private float f_n;
        private float omega_n;

        // parameters of the ratio between omega and omega_n
        private double r;

        // parameters regarding vibration-based device
        private float m;
        private float k;
        private float c_s;
        private float F_R;
        private float L_a;
        private float L_t;
        private float R_L;
        private float R_c;

        // parameters regarding coil
        private float c_e;
        private float K;
        private float N;
        private float B;
        private float lc;
        private float f_coil;

        public VibrationCalculator()
        {
            this.omega = 0.0f;
            this.Y = 0.0f;
        }
        public VibrationCalculator(float omega, float Y)
        {
            this.omega = omega;
            this.Y = Y;
        }

        void Initialize()
        {
            this.m = 0.2451f;
            this.k = 1347.7f;
            this.c_s = 0.54f;
            this.F_R = 0.4f;
            this.L_a = 0.05f;
            this.L_t = 0.095f;
            this.R_L = 15;
            this.R_c = 6.1f;

            this.N = 340;
            this.B = 0.233f;
            this.lc = 0.044f;
            this.f_coil = 0.7f;
            this.K = this.N * this.B * this.lc * this.f_coil;
            this.c_e = this.K * this.K / (this.R_L + this.R_c);

            this.omega_n = Mathf.Sqrt(k / m);
            this.f_n = this.omega_n / 2 * Mathf.PI;
            this.r = this.omega_n / this.omega;
        }

        public float Calculate()
        {
            Initialize();
            if (this.omega == 0.0f || this.Y == 0.0f)
            {
                return 0.0f;
            }
            else
            {
                double G, H, S, Q, Z, zeta, epsilon, phi;
                double Voltage;
                double Power;
                zeta = (this.c_e + this.c_s) / (2 * this.m * this.omega_n);
                Q = Mathf.Sqrt((float)((1 + Mathf.Pow((float)(2 * zeta * r), 2.0f)) / (Mathf.Pow((float)(1 - Math.Pow(r, 2.0)), 2.0f) + Mathf.Pow((float)(2 * zeta * r), 2.0f))));
                G = (Math.Sinh(Mathf.PI * zeta / r) - Mathf.Sin((float)(Mathf.PI * Mathf.Sqrt(1 - Mathf.Pow((float)zeta, 2.0f)) / r)) *
                    zeta / Math.Sqrt(1 - Math.Pow(zeta, 2.0))) / (Math.Cosh(Math.PI * zeta / r) +
                    Math.Cos(Mathf.PI * Mathf.Sqrt(1 - Mathf.Pow((float)zeta, 2.0f)) / r));
                H = (Mathf.Sin((float)(Math.PI * Math.Sqrt(1 - Mathf.Pow((float)zeta, 2.0f)) / r)) *
                    zeta / Mathf.Sqrt((float)(1 - Mathf.Pow((float)zeta, 2.0f)))) / (r * Mathf.Sqrt((float)(1 - Mathf.Pow((float)zeta, 2.0f)))
                    * (Math.Cosh(Math.PI * zeta / r) + Mathf.Cos((float)(Math.PI * Mathf.Sqrt(1 - Mathf.Pow((float)zeta, 2.0f)) / r))));
                S = (-1 * G * F_R / k) + Y * Mathf.Sqrt((float)(Mathf.Pow((float)Q, 2.0f) * Mathf.Pow((float)r, 4.0f) - Mathf.Pow((float)((H * F_R) / (Y * k)), 2.0f)));
                epsilon = Mathf.Asin((float)((-1 * H * F_R) / (k * Q * Mathf.Pow((float)r, 2.0f))));
                phi = Mathf.Atan((float)(2 * zeta * r / (1 - Math.Pow(r, 2.0)))) + epsilon;
                Z = Math.Sqrt(Mathf.Pow((float)S, 2.0f) + Mathf.Pow((float)Y, 2.0f) + 2 * S * Y * Mathf.Cos((float)phi));
                Voltage = this.K * this.omega * L_t * Z * R_L / (L_a * (R_L + R_c));
                Power = Mathf.Pow((float)Voltage, 2.0f) / R_L;

                return (float)Power;
            }
        }

        public void setOmega(float omega)
        {
            this.omega = omega;
        }

        public void setY(float Y)
        {
            this.Y = Y;
        }
    }

    class Sampling
    {
        private float time = 1.0f;
        private float past_time = 0.0f;
        private float sampling_rate = 100f;
        float interval;

        private WheelCollider Wheel_left_collider;
        private WheelCollider Wheel_right_collider;

        // list to store sampling data points
        private List<SamplingDataPoints> sampling_data_list;

        public Sampling(WheelCollider left, WheelCollider right)
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
            Debug.Log("[Info] Json content: " + json);
            return json;
        }

        public void Reset()
        {
            past_time = 0.0f;
            sampling_data_list = new List<SamplingDataPoints>();
            SamplingDataPoints dp = new SamplingDataPoints();
            dp.time = float.Parse(past_time.ToString("f2"));
            dp.height = centerY(Wheel_left_collider, Wheel_right_collider);
            sampling_data_list.Add(dp);
        }

    }
}


