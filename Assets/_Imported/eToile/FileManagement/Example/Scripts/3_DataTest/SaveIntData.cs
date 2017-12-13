using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Data save example.
 */

public class SaveIntData : MonoBehaviour {

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
            int data = int.Parse(label);
            FileManagement.SaveFile("intData", data, enc);
        }
    }
    public void ReadData()
    {
        bool enc = toggleEnc1.isOn;
        // Reading the file
        int data = FileManagement.ReadFile<int>("intData", enc);
        labelOut.text = data.ToString();
    }
}
