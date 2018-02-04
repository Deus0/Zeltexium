using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveFloatData : MonoBehaviour {

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
        string label = inputF.text;
        bool enc = toggleEnc0.isOn;
        // Saving the file
        if(label != "")
        {
            float data = float.Parse(label);
            FileManagement.SaveFile("floatData", data, enc);
        }
    }
    public void ReadData()
    {
        bool enc = toggleEnc1.isOn;
        // Reading the file
        float data = FileManagement.ReadFile<float>("floatData", enc);
        labelOut.text = data.ToString();
    }
}
