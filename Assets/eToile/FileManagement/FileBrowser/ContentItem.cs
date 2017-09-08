using UnityEngine;
using UnityEngine.UI;
using System.Collections;


/*
 * This script controls every item displayed in the browser window.
 * Items represents files and folders contained in the displayed path.
 * 
 * Parameters of SetItem():
 * - parent: the transform if the list container, will parent automatically.
 * - name: the name of the item (file or folder name)
 * - type: The item may represent three types of item (F: file, D: directory or folder, I: information).
 * - thumb: Attempts to load a thumbnail from the file (images only).
 */

public class ContentItem : MonoBehaviour {

    // Icons to identify the items:
    public Sprite[] icons;

    Toggle toggle;
    Text nameLabel;
    FileBrowser browser;
    string _type;           // Remembers what kind of item is representing ("F" or "D").

    // Double click recognition:
    float doubleClickTimeLimit = 0.3f;
    bool clickedOnce = false;
    float count = 0f;

    /// <summary>Set Item properties (called from outside)</summary>
    public void SetItem(Transform parent, string name, string type, bool thumb = false)
    {
        _type = type;
        transform.SetParent(parent, false);
        toggle = gameObject.GetComponent<Toggle>();
        toggle.group = parent.GetComponent<ToggleGroup>();
        nameLabel = transform.Find("Label").GetComponent<Text>();
        nameLabel.text = name;
        Image icon = transform.Find("Icon").GetComponent<Image>();
        if (type == "I")
            icon.sprite = icons[0];
        else if (type == "D")
            icon.sprite = icons[1];
        else if (type == "F")
        {
            // Set icon depending on file extension:
            switch (FileManagement.GetFileExtension(name).ToLower())
            {
                // Text files:
                case ".txt":
                case ".doc":
                    icon.sprite = icons[3];
                    break;
                // Image files:
                case ".bmp":
                case ".jpg":
                case ".png":
                    if(thumb)
                    {
                        string path = FileManagement.Combine(parent.root.GetComponent<FileBrowser>().GetCurrentPath(), name);
                        icon.sprite = FileManagement.ImportSprite(path);    // Thumbnail.
                    }
                    else
                    {
                        icon.sprite = icons[4];     // Descriptive icon.
                    }
                    break;
                // Audio files:
                case ".wav":
                case ".mp3":
                case ".ogg":
                    icon.sprite = icons[5];
                    break;
                default:
                    icon.sprite = icons[2];
                    break;
            }
        }
        browser = parent.root.GetComponent<FileBrowser>();
    }

    /// <summary>Set itself as selected item and starts detecting double click gesture</summary>
    public void SetSelectedItem()
    {
        if(toggle.isOn)
        {
            browser.UpdateSelectedItem(nameLabel.text, _type);
            StartCoroutine(ClickEvent());
        }
    }

    /// <summary>Deletes itself</summary>
    public void Delete()
    {
        toggle.group = null;
        transform.SetParent(null);
        GameObject.Destroy(gameObject);
    }

    /// <summary>Gesture recognition to execute selection on double click</summary>
    public IEnumerator ClickEvent()
    {
        if (!clickedOnce && count < doubleClickTimeLimit)
        {
            clickedOnce = true;
        }
        else
        {
            clickedOnce = false;
            yield break;  //If the button is pressed twice, don't allow the second function call to fully execute.
        }
        yield return new WaitForEndOfFrame();

        while (count < doubleClickTimeLimit)
        {
            if (!clickedOnce)
            {
                count = 0f;
                DoubleClick();
                clickedOnce = false;
                yield break;
            }
            count += Time.deltaTime;// increment counter by change in time between frames
            yield return null; // wait for the next frame
        }
        count = 0f;
        SingleClick();
        clickedOnce = false;
    }
    /// <summary>Double clic event</summary>
    void DoubleClick()
    {
        if (_type == "D")
            browser.GoToNextFolder();       // If it's a directory, navigates.
        else
            browser.ReturnSelectedFile();   // Otherwise atempts to select and close.
    }
    /// <summary>Clic event</summary>
    void SingleClick()
    {
        // Selection event is automatic (not needed).
    }
}
