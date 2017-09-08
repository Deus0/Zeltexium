using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * Example script intended to demonstrate the use of the FileBrowser prefab.
 * 
 * You should use this function signature in order to work properly:
 * void YourFunction(string)
 * 
 * Then pass it as an argument to receive the file path.
 */

public class OpenFileSelector : MonoBehaviour
{
    public GameObject fileBrowser;  // Drag the FileBrowser prefab here (in the editor).
    InputField selectionLabel;

    Toggle fullPath;
    Dropdown mode;
    Toggle save;
    InputField iniPath;
    InputField lockPath;

    // Use this for initialization
    void Start()
    {
        Transform panelShow = transform.parent.Find("PanelShow");
        fullPath = panelShow.Find("ToggleFullPath").GetComponent<Toggle>();
        mode = panelShow.Find("Dropdown").GetComponent<Dropdown>();
        save = panelShow.Find("ToggleSave").GetComponent<Toggle>();
        iniPath = panelShow.Find("InputIniPath").GetComponent<InputField>();
        lockPath = panelShow.Find("InputLockPath").GetComponent<InputField>();
        selectionLabel = transform.Find("FilePathLabel").GetComponent<InputField>();
    }

    // You should use this function signature in order to receive properly:
    void OnPathSelected(string path)
    {
        selectionLabel.text = path;
    }

    // Instantiates a file browser and sets the path selection event:
    public void OpenFileBrowser()
    {
        // Creates a browser windows and sets its behaviour mode:
        GameObject browserInstance = GameObject.Instantiate(fileBrowser);
        browserInstance.GetComponent<FileBrowser>().SetBrowserWindow(OnPathSelected, iniPath.text, fullPath.isOn, mode.captionText.text.Substring(0,1), save.isOn, lockPath.text);
    }

    public void SetDefaultPath()
    {
        if (fullPath.isOn)
            iniPath.text = Application.persistentDataPath;
        else
            iniPath.text = "";
    }
}
