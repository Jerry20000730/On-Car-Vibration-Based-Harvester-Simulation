using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void MenuScene()
    {
        BatUtility.runBatProcess("stop.bat");
        BatUtility.KillProjectApplication();
        SceneManager.LoadScene("Menu");
    }

    public void SceneA()
    {
        SceneManager.LoadScene("Scenario1");
    }

    public void SceneB()
    {
        SceneManager.LoadScene("Scenario2");
    }

    public void SceneC()
    {
        SceneManager.LoadScene("Data Collection");
    }

    public void SceneD()
    {
        SceneManager.LoadScene("Prediction");
    }

}
