using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadScreenshot : MonoBehaviour {

    RawImage rImage;
    Toggle enc;

    // Use this for initialization
    void Start () {
        rImage = transform.Find("RawImage").GetComponent<RawImage>();
        enc = transform.Find("ToggleEnc").GetComponent<Toggle>();
    }
	
	public void Load()
    {
        rImage.texture = FileManagement.ImportTexture("capture.jpg", enc.isOn);
    }
}
