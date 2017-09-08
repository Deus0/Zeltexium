using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadSprite : MonoBehaviour {

    InputField inputF;
    Image image;
    Toggle enc;

    void Start()
    {
        inputF = transform.Find("InputField").GetComponent<InputField>();
        image = transform.Find("Image").GetComponent<Image>();
        enc = transform.Find("ToggleEnc").GetComponent<Toggle>();
    }

    public void Load()
    {
        image.sprite = FileManagement.ImportSprite(inputF.text, enc.isOn);   // Searches the StreamingAssets folder too
    }
}
