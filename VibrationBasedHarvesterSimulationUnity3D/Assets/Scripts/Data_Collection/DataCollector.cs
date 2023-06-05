using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

public class DataCollector
{
    private DataTable dt;
    private bool isAppend;

    public DataCollector()
    {
        dt = new DataTable("Car Exp Data For Deep Learning");
        dt.Columns.Add("terrainID");
        dt.Columns.Add("position");
        dt.Columns.Add("direction");
        dt.Columns.Add("average_power");
        isAppend = ReadConfig.getAppend() == "true" ? true : false;
    }
    
    public void record(string terrainID, int position, int direction, float power)
    {
        DataRow dr = dt.NewRow();
        dr["terrainID"] = terrainID;
        dr["position"] = position;
        dr["direction"] = direction;
        dr["average_power"] = power;
        dt.Rows.Add(dr);
        Debug.Log(string.Format("Unity Client> [INFO] Data recorded: terrainID: {0}, position: {1}, direction: {2}, power: {3}", terrainID, position, direction, power));
    }

    public void SaveCSV(string filePath)
    {
        FileInfo fi = new FileInfo(filePath);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }
        using (StreamWriter sw = new StreamWriter(filePath, append: true, encoding: System.Text.Encoding.UTF8))
        {
            string data = "";
            if (!isAppend)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName.ToString();
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            
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
        }
    }
}
