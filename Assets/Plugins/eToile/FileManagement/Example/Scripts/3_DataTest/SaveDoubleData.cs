using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveDoubleData : MonoBehaviour {

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
            double data = double.Parse(label);
            FileManagement.SaveFile("doubleData", data, enc);
        }
    }
    public void ReadData()
    {
        bool enc = toggleEnc1.isOn;
        // Reading the file
        double data = FileManagement.ReadFile<double>("doubleData", enc);
        labelOut.text = data.ToString();
    }
}
