using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class Requester
{
    private readonly Thread _runnerThread;
    private bool Running;
    private string SamplingPointsData;
    private string lastSamplingData;

    /// <summary>
    ///  here the unity acts as a client, sending messages to
    ///  server, which utilize python to calculate
    ///  the fast fourier transform
    /// </summary>
    public Requester()
    {
        // create a thread instead of calling directly
        // otherwise would block unity
        // from doing ui tasks
        _runnerThread = new Thread((object callback) =>
        {
            ForceDotNet.Force();
            using (RequestSocket client = new RequestSocket())
            {
                client.Connect("tcp://localhost:5555");

                while (Running)
                {
                    if (SamplingPointsData != null && lastSamplingData != SamplingPointsData)
                    {
                        lastSamplingData = SamplingPointsData;
                        // implement sending message
                        Debug.Log("Unity Client> [INFO] Request: sending sampling points");

                        client.SendFrame(SamplingPointsData);
                        String message = null;
                        bool gotMessage = false;
                        while (Running)
                        {
                            gotMessage = client.TryReceiveFrameString(out message);
                            if (gotMessage) break;
                        }
                        if (gotMessage)
                        {
                            Debug.Log("Unity Client> [INFO] Received: " + message);
                            if (message.Contains("Error"))
                            {
                                continue;
                            } else
                            {
                                float Y_approx = float.Parse(message);
                                ((Action<float>)callback)(Y_approx);
                            }
                        }
                    }
                }
            }
            NetMQConfig.Cleanup();
        });
    }

    public void Start(Action<float> callback)
    {
        Running = true;
        _runnerThread.Start(callback);
    }

    public void Stop()
    {
        Running = false;
        // block main thread, wait for _runningThread to finish its job first, so we can be sure that
        // _runnerThread will end before main thread end
        _runnerThread.Join();
    }

    public void setSamplingData(string data)
    {
        SamplingPointsData = data;
    }
}
