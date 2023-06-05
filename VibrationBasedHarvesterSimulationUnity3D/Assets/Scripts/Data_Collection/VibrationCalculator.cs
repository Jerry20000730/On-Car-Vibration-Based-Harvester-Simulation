using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationCalculator
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

    public float getY()
    {
        return Y;
    }
}
