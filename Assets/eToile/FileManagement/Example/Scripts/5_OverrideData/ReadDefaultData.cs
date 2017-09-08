using UnityEngine;
using UnityEngine.UI;

public class ReadDefaultData : MonoBehaviour {

    Text labelOut;
    Toggle toggleEnc;

    void Start()
    {
        // Connect UI elements:
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
        toggleEnc = transform.Find("ToggleEnc").GetComponent<Toggle>();
    }

    public void ReadData()
    {
        labelOut.text = "";
        labelOut.text = FileManagement.ReadFile<string>("data.txt", toggleEnc.isOn);
    }
}
