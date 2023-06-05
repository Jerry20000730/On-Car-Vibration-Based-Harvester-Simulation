using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

public class BatUtility
{
    private static Process batProcess;
    public static void runBatProcess(string batFileName)
    {
        try
        {
            batProcess = new Process();
            string fullpath = Application.streamingAssetsPath + "/" + "PythonScripts" + "/";
            UnityEngine.Debug.Log("path: " + fullpath);
            var processStartInfo = new ProcessStartInfo("cmd.exe", "/c chcp 437&&" + "cd " + fullpath + " && " + batFileName);
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
            processStartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            batProcess.StartInfo = processStartInfo;
            batProcess.Start();

            batProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.Log("Python Output>" + e.Data);
            batProcess.BeginOutputReadLine();

            batProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                UnityEngine.Debug.LogError("Python Error>" + e.Data);
            batProcess.BeginErrorReadLine();
        } catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
        }
    }

    private static string GetCurProcessName()
    {
        string process_name = Path.GetFileNameWithoutExtension(batProcess.MainModule.FileName);
        return process_name;
    }

    public static void KillProjectApplication()
    {
        string processName = GetCurProcessName();
        Process[] processes = Process.GetProcesses();
        foreach (Process process in processes)
        {
            try
            {
                if (!process.HasExited && process.ProcessName == processName)
                    process.Kill();
            }
            catch (InvalidOperationException ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }
    }
}
