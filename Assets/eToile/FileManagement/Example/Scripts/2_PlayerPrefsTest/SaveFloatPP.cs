using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveFloatPP : MonoBehaviour {

    InputField inputF;
    Text labelOut;

    // Use this for initialization
    void Start () {
        // Connect UI elements:
        inputF = transform.Find("InputField").GetComponent<InputField>();
        labelOut = transform.Find("LabelOutput").Find("Text").GetComponent<Text>();
    }

    public void SaveData()
    {
        string label = inputF.text;
        // Saving the file
        if(label != "")
        {
            float data = float.Parse(label);
            FileManagement.SetFloat("floatDataPP", data); // Save data
        }
    }
    public void ReadData()
    {
        // Reading the file
        float data = FileManagement.GetFloat("floatDataPP");  // Get data
        labelOut.text = data.ToString();
    }
}
