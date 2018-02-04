using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveStringPP : MonoBehaviour {

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
        string data = inputF.text;
        // Saving the file
        FileManagement.SetString("stringDataPP", data);   // Save data
    }
    public void ReadData()
    {
        // Reading the file
        labelOut.text = FileManagement.GetString("stringDataPP"); // Get data
    }
}
