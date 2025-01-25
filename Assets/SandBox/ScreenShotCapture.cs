using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenShotCapture : MonoBehaviour
{
    [Serializable]
    private enum FileExtension
    {
        png,
        jpg,
        exr,
        tga,
    }
    [Tooltip("Futage will be capture from this camera")]
    [SerializeField] Camera m_Camera;
    [SerializeField] private FileExtension fileExtension = FileExtension.png;
    [SerializeField] private KeyCode captureInput = KeyCode.Mouse1;
    [SerializeField] private string filePath;
    string FileName => "ScreenShoot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" +Screen.width + "x" + Screen.height + "." + fileExtension.ToString();

    private void LateUpdate()
    {
        if (Input.GetKeyDown(captureInput))
           CaptureScreenShot();
    }

    [ContextMenu(nameof(CaptureScreenShot))]
    public void CaptureScreenShot()
    {
        StartCoroutine(Capture());
        IEnumerator Capture()
        { 
            // Create a new RenderTexture
            RenderTexture tempActiveTexture = RenderTexture.active;
            RenderTexture tempCameraTargetTexture = m_Camera.targetTexture;
            RenderTexture.active = new RenderTexture(Screen.width, Screen.height, 25);
            m_Camera.targetTexture = RenderTexture.active;
            m_Camera.Render();
        
            // Create a new Texture2D
            Texture2D image = new Texture2D(RenderTexture.active.width, RenderTexture.active.height);
            image.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
            image.Apply();

            // Reset RenderTexture and camera targetTexture
            RenderTexture.active = tempActiveTexture;
            m_Camera.targetTexture = tempCameraTargetTexture;

            // Convert to bytes and destroy the texture
            byte[] bytes = GetImageAsByte(image);
            yield return null;
            DestroyImmediate(image);

            // Save the image
            string path = filePath == string.Empty ? Application.dataPath : filePath;
            File.WriteAllBytes(Path.Combine(path, FileName), bytes);
            Debug.Log("File Saved At: " + Path.Combine(path, FileName));
        }
    }

    private byte[] GetImageAsByte(Texture2D image)
    {
        switch (fileExtension)
        {
            case FileExtension.png:
                return image.EncodeToPNG();
            case FileExtension.jpg:
                return image.EncodeToJPG();
            case FileExtension.exr:
                return image.EncodeToEXR();
            case FileExtension.tga:
                return image.EncodeToTGA();
            default:
                return null;
        }
    }
}
