using LitJson;
using UnityEngine;
using System.IO;

public static class ReadConfig
{
    private static JsonData config;

    public static void initializeConfig()
    {
        TextReader textReader = new StreamReader(Application.streamingAssetsPath + "/config.json");
        config = JsonMapper.ToObject(textReader);
        textReader.Close();
    }

    private static void saveJson()
    {
        string path = Application.streamingAssetsPath + "/config.json";
        StreamWriter sw = new StreamWriter(path, false);
        sw.Write(config.ToJson());
        sw.Close();
    }

    public static string getAppend()
    {
        return (string)config["isAppend"];
    }

    public static void setAppend(bool append)
    {
        if (append)
        {
            config["isAppend"] = "true";
        } else
        {
            config["isAppend"] = "false";
        }
        saveJson();
    }

    public static string getTerrainID()
    {
        return (string)config["terrainID"];
    }

    public static void setTerrainID(string terrainID)
    {
        config["terrainID"] = terrainID;
        saveJson();
    }

    public static float getRuntimeForEachIteration()
    {
        return float.Parse((string)config["runtimeEach"]);
    }

    public static void setRuntimeForEachIteration(float runtimeForEachIteration)
    {
        config["runtimeEach"] = runtimeForEachIteration.ToString();
        saveJson();
    }

    public static string getDataFileName()
    {
        return (string)config["dataFileName"];
    }

    public static void setDataFileName(string dataFileName)
    {
        config["dataFileName"] = dataFileName;
        saveJson();
    }

    public static string getProjection()
    {
        return (string)config["projection"];
    }

    public static void setProjection(string projection)
    {
        config["projection"] = projection;
        saveJson();
    }
}
