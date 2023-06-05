using System.IO;
using uFileBrowser;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FileSelection : MonoBehaviour
{
    public GameObject filebrowser;
    private int sceneID;

    void OnEnable()
    {
        filebrowser.SetActive(false);
    }

    public void onButtonForSceneBClickedToOpenFileBrowser()
    {
        sceneID = 1;
        filebrowser.SetActive(true);
    }

    public void onButtonForSceneCClickedToOpenFileBrowser()
    { 
        sceneID = 2;
        filebrowser.SetActive(true);
    }

    public void onButtonForSceneDClickedToOpenFileBrowser()
    {
        sceneID = 3;
        filebrowser.SetActive(true);
    }

    public void onFileBrowserSubmitClicked()
    {
        string path = filebrowser.GetComponent<FileBrowser>().AddressPath + filebrowser.GetComponent<FileBrowser>().FileName;
        Debug.Log(path);
        string extension = Path.GetExtension(path);
        if (extension != ".raw")
        {
            return;
        }
        else
        {
            TerrainSetting.getTerrainSetting().setFilePath(path);
            Debug.Log("The filepath for the terrain file has been successfully set " + path);
            if (sceneID == 1)
                SceneManager.LoadScene("Scenario2");
            else if (sceneID == 2)
                SceneManager.LoadScene("Data Collection");
            else
                SceneManager.LoadScene("Prediction");
        }
    }
}
