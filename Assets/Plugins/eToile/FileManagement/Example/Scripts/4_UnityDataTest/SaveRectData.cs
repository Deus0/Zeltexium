using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Data save example.
 */

public class SaveRectData : MonoBehaviour {

    Toggle toggleEnc0;
    InputField inputX;
    InputField inputY;
    InputField inputW;
    InputField inputH;
    Text labelOut;

    // Use this for initialization
    void Start()
    {
        // Connect UI elements:
        inputX = transform.Find("InputX").GetComponent<InputField>();
        inputY = transform.Find("InputY").GetComponent<InputField>();
        inputW = transform.Find("InputW").GetComponent<InputField>();
        inputH = transform.Find("InputH").GetComponent<InputField>();
        toggleEnc0 = transform.Find("ToggleEnc (0)").GetComponent<Toggle>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
    }

    public void SaveData()
    {
        float x = (inputX.text != "") ? float.Parse(inputX.text) : 0f;
        float y = (inputY.text != "") ? float.Parse(inputY.text) : 0f;
        float w = (inputW.text != "") ? float.Parse(inputW.text) : 0f;
        float h = (inputH.text != "") ? float.Parse(inputH.text) : 0f;
        bool enc = toggleEnc0.isOn;
        // Saving the file
        Rect data = new Rect(x, y, w, h);
        FileManagement.SaveFile("rData", data, enc);
    }

    public void ReadData()
    {
        bool enc = toggleEnc0.isOn;
        // Reading the file
        Rect data = FileManagement.ReadFile<Rect>("rData", enc);
        labelOut.text = data.ToString();
    }
}
