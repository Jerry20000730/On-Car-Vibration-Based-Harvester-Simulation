using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Power_display_update : MonoBehaviour
{
    private Label _power_display;
    private Label _speed_display;
    private Label _height_display;
    public Rigidbody my_rigidbody;
    public WheelCollider my_wheel;
    public UIDocument ui;

    // omega of the wheel
    private float omega;

    // amplitude of the road;
    private float Y;

    // calculator
    private Vibration_calculator vc;

    // ratio (unity unit to real life)
    private float ratio_to_reality;

    // scenario
    private int scenario;


    void OnEnable()
    {
        my_rigidbody = GetComponent<Rigidbody>();
        _power_display = ui.rootVisualElement.Q<GroupBox>().Q<GroupBox>().Q<Label>("power_indicator");
        _speed_display = ui.rootVisualElement.Q<GroupBox>().Q<GroupBox>().Q<Label>("speed_indicator");
        _height_display = ui.rootVisualElement.Q<GroupBox>().Q<GroupBox>().Q<Label>("height_indicator");
        ratio_to_reality = 1.0f / 96.0f;
    }

    // Update is called once per frame
    void Update()
    {
        switch (scenario)
        {
            case 1:
                if (my_rigidbody.velocity.magnitude != 0)
                {
                    omega = my_rigidbody.velocity.magnitude / my_wheel.radius;
                    Y = 2.5f * ratio_to_reality;
                    vc = new Vibration_calculator(omega, Y);
                    _power_display.text = "Power: " + String.Format("{0:N2}", vc.calculate() * 1000) + "mW";
                    _speed_display.text = "Speed: " + String.Format("{0:N2}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
                    _height_display.text = "Height: " + my_wheel.transform.TransformPoint(my_wheel.center) + "(wheel)";
                }
                break;
            case 2:
                if (my_rigidbody.velocity.magnitude != 0)
                {
                    omega = my_rigidbody.velocity.magnitude / my_wheel.radius;
                    Y = 1.0f * ratio_to_reality;
                    vc = new Vibration_calculator(omega, Y);
                    _power_display.text = "Power: " + String.Format("{0:N2}", vc.calculate() * 1000) + "mW";
                    _speed_display.text = "Speed: " + String.Format("{0:N2}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
                    _height_display.text = "Height: " + my_wheel.transform.TransformPoint(my_wheel.center) + "(wheel)";
                }
                break;
            case 3:
                if (my_rigidbody.velocity.magnitude != 0)
                {
                    omega = my_rigidbody.velocity.magnitude / my_wheel.radius;
                    Y = 0.5f * ratio_to_reality;
                    vc = new Vibration_calculator(omega, Y);
                    _power_display.text = "Power: " + String.Format("{0:N2}", vc.calculate() * 1000) + "mW";
                    _speed_display.text = "Speed: " + String.Format("{0:N2}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
                    _height_display.text = "Height: " + my_wheel.transform.TransformPoint(my_wheel.center) + "(wheel)";
                }
                break;
            case 0:
                _power_display.text = "Power: 0.0mW";
                _speed_display.text = "Speed: " + String.Format("{0:N1}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
                _height_display.text = "Height: " + my_wheel.transform.TransformPoint(my_wheel.center) + "(wheel)";
                break;
            default:
                _power_display.text = "Power: 0.0mW";
                _speed_display.text = "Speed: " + String.Format("{0:N1}", my_rigidbody.velocity.magnitude * ratio_to_reality) + "m/s";
                _height_display.text = "Height: " + my_wheel.transform.TransformPoint(my_wheel.center) + "(wheel)";
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Åöµ½ÁË");
        if (collision.collider.tag.Equals("Terrain-start-1"))
        {
            this.scenario = 1;
        }
        else if (collision.collider.tag.Equals("Terrain-start-2"))
        {
            this.scenario = 2;
        }
        else if (collision.collider.tag.Equals("Terrain-start-3"))
        {
            this.scenario = 3;
        }
        else
        {
            this.scenario = 0;
        }
    }
}

class Vibration_calculator
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


    public Vibration_calculator(float omega, float Y)
    {
        this.omega = omega;
        this.Y = Y;
    }

    void initialize()
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

    public float calculate()
    {
        initialize();
        if (this.omega == 0)
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
}
