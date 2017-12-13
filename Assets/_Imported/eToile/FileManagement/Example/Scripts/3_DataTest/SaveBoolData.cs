using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveBoolData : MonoBehaviour {

    Toggle toggleEnc0;
    Dropdown dDown;
    Text labelOut;
    Toggle toggleEnc1;

    void Start()
    {
        print(Application.persistentDataPath);
        // Connect UI elements:
        toggleEnc0 = transform.Find("ToggleEnc (0)").GetComponent<Toggle>();
        dDown = transform.Find("Dropdown").GetComponent<Dropdown>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
        toggleEnc1 = transform.Find("ToggleEnc (1)").GetComponent<Toggle>();
    }

    // Int data event:
    public void SaveData()
    {
        bool enc = toggleEnc0.isOn;
        // Saving the file
        bool option = (dDown.value == 0) ? false : true;
        FileManagement.SaveFile("boolData", option, enc);
    }

    public void ReadData()
    {
        bool enc = toggleEnc1.isOn;
        // Reading the file
        bool data = FileManagement.ReadFile<bool>("boolData", enc);
        labelOut.text = data.ToString();
    }
}
