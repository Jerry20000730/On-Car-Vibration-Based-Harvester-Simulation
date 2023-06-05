using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TerrainUtil
{
    public static List<FileInfo> getFiles()
    {
        string path = string.Format("{0}", Application.dataPath + "/" + "Heightmaps" + "/");

        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*.raw");
            List<FileInfo> file_list = new List<FileInfo>();
            for (int i = 0; i < files.Length; i++)
            {
                // ignore meta configuration file
                if (!files[i].Name.EndsWith(".meta"))
                {
                    file_list.Add(files[i]);
                }
            }

            return file_list;
        } else
        {
            Debug.LogError("[ERROR] There is no such file directory");
            return null;
        }
    }
}
