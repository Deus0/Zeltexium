using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadTexture : MonoBehaviour {

    InputField inputF;
    RawImage rImage;
    Toggle enc;

    void Start () {
        inputF = transform.Find("InputField").GetComponent<InputField>();
        rImage = transform.Find("RawImage").GetComponent<RawImage>();
        enc = transform.Find("ToggleEnc").GetComponent<Toggle>();
    }

    public void Load()
    {
        rImage.texture = FileManagement.ImportTexture(inputF.text, enc.isOn);
    }
}
