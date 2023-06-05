using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DataCollectionCameraUtility : MonoBehaviour
{
    private static int width = 1024;
    private static int height = 1024;

    public static void startScreenshot(Camera camera, string terrainID, string position, string direction)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(terrainID, position, direction);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
    }

    public static void ScreenShotForPrediction(Camera camera, string position, string direction)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot = ScaleTexture(screenShot, width / 8, height / 8);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(position, direction);
        System.IO.File.WriteAllBytes(filename, bytes);
    }

    public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
        float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    static string ScreenShotName(string terrainID, string position, string direction)
    {
        return string.Format("{0}/ScreenShots/{1}_{2}_{3}_{4}.png",
                             Application.streamingAssetsPath,
                             terrainID, position, direction,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    static string ScreenShotName(string position, string direction)
    {
        return string.Format("{0}/PredictionCache/{1}_{2}_{3}_pred.png",
                             Application.streamingAssetsPath,
                             position, direction,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
}
