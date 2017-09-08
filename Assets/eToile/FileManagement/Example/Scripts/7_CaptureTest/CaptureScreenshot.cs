using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CaptureScreenshot : MonoBehaviour
{
    RawImage rImage;
    Texture2D capture;
    Toggle enc;

    // Use this for initialization
    void Start()
    {
        rImage = transform.Find("RawImage").GetComponent<RawImage>();
        capture = new Texture2D(Screen.width, Screen.height);
        enc = transform.Find("ToggleEnc").GetComponent<Toggle>();
    }

    public void Capture()
    {
        StartCoroutine(TakeScreenshot());
    }

    IEnumerator TakeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        // Screenshot:
        capture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capture.Apply();
        // Show capture on screen:
        rImage.texture = capture;
        // Save screenshot picking from a texture:
        FileManagement.SaveJpgTexture("capture.jpg", rImage.texture, 100, enc.isOn);
    }

    public void Delete()
    {
        rImage.texture = null;
        FileManagement.DeleteFile("capture.jpg");
    }
}
