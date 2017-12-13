using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveDefaultData : MonoBehaviour {

    InputField inputF;
    Toggle toggleEnc;

    void Start()
    {
        // Connect UI elements:
        inputF = transform.Find("InputField").GetComponent<InputField>();
        toggleEnc = transform.Find("ToggleEnc").GetComponent<Toggle>();
    }

    public void SaveData()
    {
        // Saving the file
        FileManagement.SaveFile("data.txt", inputF.text, toggleEnc.isOn);
    }
}
