using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveIntPP : MonoBehaviour {

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
            int data = int.Parse(label);
            FileManagement.SetInt("intDataPP", data); // Save data
        }
    }
    public void ReadData()
    {
        // Reading the file
        int data = FileManagement.GetInt("intDataPP");    // Get data
        labelOut.text = data.ToString();
    }
}
