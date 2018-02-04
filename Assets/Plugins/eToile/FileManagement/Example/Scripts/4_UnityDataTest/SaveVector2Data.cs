using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Data save example.
 */

public class SaveVector2Data : MonoBehaviour {

    Toggle toggleEnc0;
    InputField inputX;
    InputField inputY;
    Text labelOut;

    // Use this for initialization
    void Start ()
    {
        // Connect UI elements:
        inputX = transform.Find("InputX").GetComponent<InputField>();
        inputY = transform.Find("InputY").GetComponent<InputField>();
        toggleEnc0 = transform.Find("ToggleEnc (0)").GetComponent<Toggle>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
    }

    public void SaveData()
    {
        float x = (inputX.text != "") ? float.Parse(inputX.text) : 0f;
        float y = (inputY.text != "") ? float.Parse(inputY.text) : 0f;
        bool enc = toggleEnc0.isOn;
        // Saving the file
        Vector2 data = new Vector2(x,y);
        FileManagement.SaveFile("v2Data", data, enc);
    }

    public void ReadData()
    {
        bool enc = toggleEnc0.isOn;
        // Reading the file
        Vector2 data = FileManagement.ReadFile<Vector2>("v2Data", enc);
        labelOut.text = data.ToString();
    }
}
