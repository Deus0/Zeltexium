using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Data save example.
 */

public class SaveVector4Data : MonoBehaviour {

    Toggle toggleEnc0;
    InputField inputX;
    InputField inputY;
    InputField inputZ;
    InputField inputW;
    Text labelOut;

    // Use this for initialization
    void Start()
    {
        // Connect UI elements:
        inputX = transform.Find("InputX").GetComponent<InputField>();
        inputY = transform.Find("InputY").GetComponent<InputField>();
        inputZ = transform.Find("InputZ").GetComponent<InputField>();
        inputW = transform.Find("InputW").GetComponent<InputField>();
        toggleEnc0 = transform.Find("ToggleEnc (0)").GetComponent<Toggle>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
    }

    public void SaveData()
    {
        float x = (inputX.text != "") ? float.Parse(inputX.text) : 0f;
        float y = (inputY.text != "") ? float.Parse(inputY.text) : 0f;
        float z = (inputZ.text != "") ? float.Parse(inputZ.text) : 0f;
        float w = (inputW.text != "") ? float.Parse(inputW.text) : 0f;
        bool enc = toggleEnc0.isOn;
        // Saving the file
        Vector4 data = new Vector4(x, y, z, w);
        FileManagement.SaveFile("v4Data", data, enc);
    }

    public void ReadData()
    {
        bool enc = toggleEnc0.isOn;
        // Reading the file
        Vector4 data = FileManagement.ReadFile<Vector4>("v4Data", enc);
        labelOut.text = data.ToString();
    }
}
