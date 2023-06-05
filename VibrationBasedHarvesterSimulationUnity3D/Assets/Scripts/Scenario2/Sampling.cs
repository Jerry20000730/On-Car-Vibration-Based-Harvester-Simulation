using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Sampling : MonoBehaviour
{
    // Start is called before the first frame update
    float time = 1.0f;
    float past_time = 0.0f;
    float sampling_rate = 100f;
    float interval;
    bool isSampling = false;
    bool isClicked = false;
    public Transform Wheel_Left_Model;
    public Button SamplingButton;
    TextMeshProUGUI SamplingButtonText;

    DataTable dt;

    void Start()
    {
        interval = time / sampling_rate;
        SamplingButtonText = SamplingButton.GetComponentInChildren<TextMeshProUGUI>();
        SamplingButtonText.text = "Start Sampling";
        SamplingButton.onClick.AddListener(ButtonClick);
        dt = new DataTable("test");
        dt.Columns.Add("time");
        dt.Columns.Add("height");
    }

    // Update is called once per frame
    void Update()
    {
        if (isSampling && isClicked)
        {
            DataRow dr = dt.NewRow();
            dr["time"] = past_time;
            dr["height"] = Wheel_Left_Model.transform.position.y;
            dt.Rows.Add(dr);
            interval -= Time.deltaTime;
            if (interval <= 0)
            {
                interval = time / sampling_rate;
                past_time += interval;
                dr = dt.NewRow();
                dr["time"] = float.Parse(past_time.ToString("f2"));
                dr["height"] = Wheel_Left_Model.transform.position.y;
                dt.Rows.Add(dr);
            }
        } else if (!isSampling && isClicked)
        {
            isSampling = false;
            Debug.Log("Sampling stop");
            isSampling = false;
            SamplingButtonText.text = "Start Sampling";
            string filePath = Application.streamingAssetsPath + "\\data.csv";
            SaveCSV(filePath, dt);
            Debug.Log("Save Finished");
        }
    }

    void ButtonClick()
    {
        if (!isSampling)
        {
            isSampling = true;
            SamplingButtonText.text = "Sampling...";
        } else
        {
            Debug.Log("Sampling stop");
            isSampling = false;
            isClicked = false;
            SamplingButtonText.text = "Start Sampling";
            var colors = SamplingButton.colors;
            colors.selectedColor = Color.white;
            SamplingButton.colors = colors;
        }
        isClicked = true;
    }

    // Write table data to files
    public static void SaveCSV(string filePath, DataTable dt)
    {
        FileInfo fi = new FileInfo(filePath);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }
        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                string data = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName.ToString();
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        string str = dt.Rows[i][j].ToString();
                        data += str;
                        if (j < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);
                }
                sw.Close();
                fs.Close();
            }
        }
    }
}
