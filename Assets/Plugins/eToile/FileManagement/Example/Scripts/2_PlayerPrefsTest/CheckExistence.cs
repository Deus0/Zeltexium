using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CheckExistence : MonoBehaviour {

    Dropdown selectedFile;
    Image dropdownColor;

    // Use this for initialization
    void Start () {
        selectedFile = transform.Find("Dropdown").GetComponent<Dropdown>();
        dropdownColor = transform.Find("Dropdown").GetComponent<Image>();
    }
	
	public void CheckFileExistence()
    {
        if (FileManagement.HasKey(selectedFile.captionText.text))
            dropdownColor.color = Color.green;
        else
            dropdownColor.color = Color.red;
    }

    public void ResetColor()
    {
        dropdownColor.color = Color.white;
    }

    public void DeleteFile()
    {
        FileManagement.DeleteKey(selectedFile.captionText.text);
        dropdownColor.color = Color.white;
    }

    public void DeleteAllFiles()
    {
        FileManagement.DeleteAll();
        dropdownColor.color = Color.white;
    }
}
