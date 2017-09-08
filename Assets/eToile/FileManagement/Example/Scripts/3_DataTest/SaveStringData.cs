using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveStringData : MonoBehaviour {

    Toggle toggleEnc0;
    InputField inputF;
    Text labelOut;
    Toggle toggleEnc1;

    void Start()
    {
        // Connect UI elements:
        toggleEnc0 = transform.Find("ToggleEnc (0)").GetComponent<Toggle>();
        inputF = transform.Find("InputField").GetComponent<InputField>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
        toggleEnc1 = transform.Find("ToggleEnc (1)").GetComponent<Toggle>();
    }

    public void SaveData()
    {
        bool enc = toggleEnc0.isOn;
        string data = inputF.text;
        // Saving the file
        FileManagement.SaveFile("stringData", data, enc);
    }
    public void ReadData()
    {
        bool enc = toggleEnc1.isOn;
        // Reading the file
        labelOut.text = FileManagement.ReadFile<string>("stringData", enc);
    }
}
