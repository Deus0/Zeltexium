using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * This script controls the FileBrowser window behaviour, it shows folder content and
 * also admits navigating through the unified virtual drive PersistentData + StreamingAssets.
 * 
 * Parameters of SetBrowserWindow():
 * - selectionReturn: Funtion to be called when navigation/selection ends, and returns the selected item/path.
 * - iniPath: Path where to start browsing.
 * - fullPath: Determines if browsing in restricted (false) or unrestricted (true) mode.
 * - selectionMode: Determines teh type of item to be selected ("F" for files, "D" for directories).
 * - save: enables the "save" mode, so the file name can be written instead of selected.
 * - lockPath: The minimum fixed path where the browser has access.
 */

public class FileBrowser : MonoBehaviour {

    // Configurable parameters:
    public int minWidth = 400;
    public int minHeight = 300;
    public GameObject ContentItem;  // Prefab representing files or folders.
    [SerializeField] float defaultItemSize = 0.05f; // Percentage of the canvas height (the height of the screen in canvas units)
    [SerializeField] List<string> filter;           // File extensions to filter rendered list.

    // UI elements:
    RectTransform canvas;
    Transform browserUI;            // The browser window itself.
    InputField currentPath;         // Input field with the displayed path.
    Transform content;              // List of items (ContentItem items).
    string selectedItem = "";       // Last valid item selection.
    InputField inputSelection;      // Selected visible field (without path).
    Button selectionButton;
    Text selectionButtonLabel;      // Used to set the button label "Open" or "Select".
    Dropdown filterDropdown;        // The list of available file extension filters.
    Slider sizeSlider;              // Slider to set the size of the items.
    ScrollRect contentWindow;

    // Selection path control:
    public delegate void OnPathSelected(string path);
    OnPathSelected _return;         // "Function" called to pass the selected path relative to the pase folder, when selection ends.
    List<string> navHistory = new List<string>();
    bool browsingHistory = false;   // Activated while the user browses back the navHistory.
    int navIndex = 0;

    // Internal status:
    bool _fullPath = false;         // Starts as protected "safe mode" by default.
    string _lockPath = "";          // The path to where the browser is limited to access (can't go outside this folder).
    string _selectionType;          // Remembers last selection type.
    string _selectionMode = "F";    // Sets the type of items allowed to be selected ("F" or "D").
    bool _save = false;             // Activates the "save" mode.

    // Confirmation window:
    GameObject confirmation;        // Confirmation window (enable/disable).
    Text confirmLabel;              // Label of the confirmation window.
    // New name window:
    GameObject newNameWindow;       // NewName window (enable/disable).
    Text newNameLabel;              // Label of the "new name" window.
    InputField inputNewName;        // InputField with the "new name".
    // Error message window:
    GameObject errorMessage;        // Error message window (enable/disable).
    Text errorMsgLabel;             // Label of the confirmation window.

    // Copy/Paste:
    string sourcePath;              // Source file/folder to be copied or cutted.
    string sourceType;              // The type of the selected source: "F" or "D".
    bool moveSourcePath;            // Move or copy the sourcePath?
    
    // Confirmation window control:
    delegate void ConfirmationAction();
    ConfirmationAction _action;

    // Use this for initialization
    private void Awake()
    {
        // Connects every UI element in the window:
        canvas = gameObject.GetComponent<RectTransform>();
        browserUI = transform.Find("BrowserWindow");
        currentPath = browserUI.Find("InputCurrentPath").GetComponent<InputField>();
        content = browserUI.Find("ContentWindow").Find("Viewport").Find("Content");
        contentWindow = browserUI.Find("ContentWindow").GetComponent<ScrollRect>();
        inputSelection = browserUI.Find("InputSelection").GetComponent<InputField>();
        selectionButton = browserUI.Find("ButtonSelect").GetComponent<Button>();
        selectionButtonLabel = selectionButton.transform.Find("Text").GetComponent<Text>();
        // Confirmation window:
        confirmation = browserUI.Find("Confirmation").gameObject;
        confirmLabel = confirmation.transform.Find("Label").GetComponent<Text>();
        confirmation.SetActive(false);
        // NewName window:
        newNameWindow = browserUI.Find("NewName").gameObject;
        newNameLabel = newNameWindow.transform.Find("Label").GetComponent<Text>();
        inputNewName = newNameWindow.transform.Find("InputNewName").GetComponent<InputField>();
        newNameWindow.SetActive(false);
        // Error message window:
        errorMessage = browserUI.Find("ErrorMessage").gameObject;
        errorMsgLabel = errorMessage.transform.Find("Label").GetComponent<Text>();
        errorMessage.SetActive(false);
        // Optional UI components:
        Transform uiItem;
        if (uiItem = browserUI.Find("SizeSlider"))
            sizeSlider = uiItem.GetComponent<Slider>();
        if (uiItem = browserUI.Find("FilterDropdown"))
            filterDropdown = uiItem.GetComponent<Dropdown>();
        // Start browser:
        SetBrowserWindow(null);
    }

    private void LateUpdate()
    {
        // TODO: Optimize this:
        SetContentSize();
    }

    /// <summary>Set browser return event, first path to show and work mode (full path or override)</summary>
    public void SetBrowserWindow(OnPathSelected selectionReturn, string iniPath = "", bool fullPath = false, string selectionMode = "F", bool save = false, string lockPath = "")
    {
        _selectionMode = selectionMode;                                 // The type of item to be selected ("F" or "D")
        _return = selectionReturn;                                      // Saves the return method.
        _lockPath = FileManagement.NormalizePath(lockPath);             // The browser will access to this directory and subdirectories only.
        string _ini = FileManagement.NormalizePath(iniPath);
        currentPath.text = FileManagement.Combine(_lockPath, _ini);     // Sets custom path, always relative to _lockPath.
        _fullPath = fullPath;                                           // Remembers access mode.
        _save = save;                                                   // Set "save" mode.
        inputSelection.interactable = _save;                            // Enables/disables the file input field.
        // Show default content:
        navHistory.Clear();
        RememberPath();
        ShowFolderContent();
    }

    /// <summary>Set file filters (using a List of strings)</summary>
    public void SetBrowserWindowFilter(List<string> newFilter)
    {
        // Other extensions:
        filter = newFilter;
        // First item (all extensions at once):
        if (filter.Count > 1)
        {
            string allFilters = string.Join(";", newFilter.ToArray());
            filter.Insert(0, allFilters);
        }
        // Update the Dropdown:
        filterDropdown.ClearOptions();
        filterDropdown.AddOptions(filter);
        // Refresh view correcting the input path (just in case):
        CorrectInputPath();
    }
    /// <summary>Set file filters (using a string array)</summary>
    public void SetBrowserWindowFilter(string[] newFilter)
    {
        SetBrowserWindowFilter(new List<string>(newFilter));
    }
    /// <summary>Set file filters (semicolon separated arguments)</summary>
    public void SetBrowserWindowFilter(string newFilter)
    {
        SetBrowserWindowFilter(newFilter.Split(';'));
    }

    /// <summary>Closes returning the selected file (Also called by ContentItem when DoubleClick)</summary>
    public void ReturnSelectedFile()
    {
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        inputSelection.text = FileManagement.NormalizePath(inputSelection.text);
        if (inputSelection.text != "" && (_selectionType == _selectionMode || _save))
        {
            if (_return != null)
            {
                currentPath.text = FileManagement.Combine(currentPath.text, inputSelection.text);
                _return(FileManagement.NormalizePath(currentPath.text));
            }
            CloseFileBrowser();
        }
        else
        {
            GoToNextFolder();
        }
    }

    /// <summary>Closes the browser window</summary>
    public void CloseFileBrowser()
    {
        GameObject.Destroy(gameObject);
    }

    /// <summary>Adds every item to the list (updates the content view)</summary>
    void ShowFolderContent()
    {
        int listSize = 0;
        // Delete list content:
        while(content.childCount > 0)
        {
            content.GetChild(0).GetComponent<ContentItem>().Delete();
        }
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        if(currentPath.text == "" && _fullPath == true)
        {
            // Show available logical drives instead of content:
            string[] drives = FileManagement.ListLogicDrives();
            for (int d = 0; d < drives.Length; d++)
            {
                GameObject item = GameObject.Instantiate(ContentItem);
                item.GetComponent<ContentItem>().SetItem(content, drives[d], "D");
            }
            listSize += drives.Length;
        }
        else
        {
            // Get directories:
            string[] directories = FileManagement.ListDirectories(currentPath.text, true, _fullPath);
            if (directories != null)
            {
                for (int d = 0; d < directories.Length; d++)
                {
                    GameObject item = GameObject.Instantiate(ContentItem);
                    item.GetComponent<ContentItem>().SetItem(content, directories[d], "D");
                }
                listSize += directories.Length;
            }
            // Get files:
            string[] _filter = filter.ToArray();
            if(filterDropdown != null)
                _filter = filterDropdown.captionText.text.Split(';');
            string[] files = FileManagement.ListFiles(currentPath.text, _filter, true, _fullPath);
            if (files != null)
            {
                for (int f = 0; f < files.Length; f++)
                {
                    GameObject item = GameObject.Instantiate(ContentItem);
                    item.GetComponent<ContentItem>().SetItem(content, files[f], "F");
                }
                listSize += files.Length;
            }
            // Exception detection (empty or access denied):
            if (directories == null)
            {
                if (FileManagement.DirectoryExists(currentPath.text))
                {
                    // Access denied:
                    GameObject item = GameObject.Instantiate(ContentItem);
                    item.GetComponent<ContentItem>().SetItem(content, "Access denied", "I");
                    listSize = 1;
                }
                else
                {
                    // Folder not exists:
                    GameObject item = GameObject.Instantiate(ContentItem);
                    item.GetComponent<ContentItem>().SetItem(content, "Folder not exists", "I");
                    listSize = 1;
                }
            }
            else if (directories.Length == 0 && files.Length == 0)
            {
                // Folder is empty:
                GameObject item = GameObject.Instantiate(ContentItem);
                item.GetComponent<ContentItem>().SetItem(content, "Folder is empty", "I");
                listSize = 1;
            }
        }

        // Set size of rendered elements:
        SetContentSize();
        // Reset selection (Because showing a new folder):
        inputSelection.text = "";
        selectedItem = "";
        selectionButtonLabel.text = "Select";
        selectionButton.interactable = false;
        contentWindow.verticalNormalizedPosition = 1f;  // Send list to the top.
    }

    /// <summary>Set the size of the items and the list accordingly</summary>
    public void SetContentSize()
    {
        // Set list size:
        float dynamicHeight = canvas.rect.height * defaultItemSize;
        if(sizeSlider != null)
            dynamicHeight = canvas.rect.height * sizeSlider.value;
        content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, content.childCount * dynamicHeight);
    }

    /// <summary>Sets the rendered path from InputField (navigation)</summary>
    public void CorrectInputPath()
    {
        // Avoid navigation outside LockPath:
        if (!_fullPath)
        {
            currentPath.text = currentPath.text.Replace("..", "");
            currentPath.text = currentPath.text.Replace("/.", "");
        }
        // Force the LockPath as the minimum accessible path:
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        if (!currentPath.text.StartsWith(_lockPath))
            currentPath.text = _lockPath;
        ShowFolderContent();
        // Remember navigation path:
        RememberPath();
    }
    /// <summary>Go to parent folder (navigation)</summary>
    public void GoToParentFolder()
    {
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        string parentPath = FileManagement.GetParentDirectory(currentPath.text);
        currentPath.text = parentPath;
        CorrectInputPath();     // Normalizes and forces the Lock Path.
    }
    /// <summary>Go to next folder (navigation)</summary>
    public void GoToNextFolder()
    {
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        currentPath.text = FileManagement.Combine(currentPath.text, selectedItem);
        ShowFolderContent();
        // Remember navigation path:
        RememberPath();
    }
    /// <summary>Remember new rendered path (navigation)</summary>
    void RememberPath()
    {
        if (!browsingHistory && (FileManagement.DirectoryExists(currentPath.text, true, _fullPath) || currentPath.text == "") )
        {
            // Do not remember the path if has not changed:
            if (navHistory.Count == 0)
            {
                navHistory.Add(currentPath.text);
                navIndex = navHistory.Count - 1;
            }
            else if (currentPath.text != navHistory[navHistory.Count - 1])
            {
                navIndex++;
                if(navIndex < navHistory.Count)
                {
                    navHistory.Insert(navIndex, currentPath.text);
                    // Update history:
                    navHistory.RemoveRange(navIndex + 1, navHistory.Count - navIndex - 1);
                }
                else
                {
                    navHistory.Add(currentPath.text);
                    navIndex = navHistory.Count - 1;
                }
            }
        }
        browsingHistory = false;
    }
    /// <summary>Browse back from history path (navigation)</summary>
    public void BrowseBack()
    {
        // Index calculation:
        browsingHistory = true;
        navIndex--;
        if (navIndex < 0)
            navIndex = 0;
        // Set the path:
        currentPath.text = navHistory[navIndex];
        CorrectInputPath();
    }
    /// <summary>Browse forward from history path (navigation):</summary>
    public void BrowseFwd()
    {
        // Index calculation:
        browsingHistory = true;
        navIndex++;
        if (navIndex == navHistory.Count)
            navIndex--;
        // Set the path:
        currentPath.text = navHistory[navIndex];
        CorrectInputPath();
    }

    /// <summary>Function called by ContentItem (prefab) to display item selection options</summary>
    public void UpdateSelectedItem(string item, string type)
    {
        // The type of item determines the "Select" button behaviour:
        _selectionType = type;
        if (_selectionType == _selectionMode)
        {
            // If the items matches the selection mode, allows to select and close:
            inputSelection.text = item;
            selectedItem = item;
            if (_save)
                selectionButtonLabel.text = "Save";
            else
                selectionButtonLabel.text = "Select";
            selectionButton.interactable = true;
        }
        else if(_selectionType == "D")
        {
            // When selecting only files, folders shows the "Open" option.
            inputSelection.text = "";
            selectedItem = item;
            selectionButtonLabel.text = "Open";
            selectionButton.interactable = true;
        }
        else
        {
            // Reset selection (Because no selection allowed):
            inputSelection.text = "";
            selectedItem = "";
            selectionButtonLabel.text = "Select";
            selectionButton.interactable = false;
        }
    }
    /// <summary>Enables or disables the action button accordingly</summary>
    public void EnableSelectButton()
    {
        inputSelection.text = FileManagement.NormalizePath(inputSelection.text);
        if (_save && inputSelection.text != "")
        {
            selectionButtonLabel.text = "Save";
            selectionButton.interactable = true;
        }
    }

    /// <summary>Returns the path being rendered</summary>
    public string GetCurrentPath()
    {
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        return currentPath.text;
    }
    /// <summary>Dragging the browser window</summary>
    public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData)
    {
        var pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
        if (pointerData == null) return;
        // Update position of the selected UI object:
        Vector3 currentPosition = browserUI.position;
        currentPosition.x += pointerData.delta.x;
        currentPosition.y += pointerData.delta.y;
        browserUI.position = currentPosition;
    }
    /// <summary>Resizing the browser window</summary>
    public void OnResize(UnityEngine.EventSystems.BaseEventData eventData)
    {
        UnityEngine.EventSystems.PointerEventData pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
        if (pointerData == null) return;
        RectTransform rt = browserUI.GetComponent<RectTransform>();
        float proportion = canvas.rect.width / Screen.width;
        // Save current size:
        Vector2 offsetMax = rt.offsetMax;
        Vector2 offsetMin = rt.offsetMin;
        // Apply new size:
        rt.offsetMax = new Vector2(rt.offsetMax.x + pointerData.delta.x * proportion, rt.offsetMax.y);  // Width
        rt.offsetMin = new Vector2(rt.offsetMin.x, rt.offsetMin.y + pointerData.delta.y * proportion);  // Height
        // Control min size:
        Rect size = rt.rect;
        if (size.width < minWidth)
            rt.offsetMax = offsetMax;
        if (size.height < minHeight)
            rt.offsetMin = offsetMin;
    }

    /// <summary>Asks for confirmation before deletion (Deletes files or folders)</summary>
    public void PromtDeleteSelection()
    {
        if(selectedItem != "")
        {
            string path = FileManagement.Combine(currentPath.text, selectedItem);
            switch (_selectionType)
            {
                case "F":
                    if (FileManagement.FileExists(path, false, _fullPath))
                    {
                        confirmLabel.text = "Delete this file permanently? " + selectedItem;
                        confirmation.SetActive(true);
                        _action = DeleteFile;       // Set the delegate.
                    }
                    else
                    {
                        PromtErrorMessage("Can't delete. The file is read only (" + selectedItem + ").");
                    }
                    break;
                case "D":
                    if (FileManagement.DirectoryExists(path, false, _fullPath))
                    {
                        confirmLabel.text = "Delete this folder and all of its content? " + selectedItem;
                        confirmation.SetActive(true);
                        _action = DeleteFolder;     // Set the delegate.
                    }
                    else
                    {
                        PromtErrorMessage("Can't delete. The folder is read only (" + selectedItem + ").");
                    }
                    break;
            }
        }
    }
    void DeleteFile()
    {
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        FileManagement.DeleteFile(FileManagement.Combine(currentPath.text, selectedItem), _fullPath);
        Cancel();
    }
    void DeleteFolder()
    {
        currentPath.text = FileManagement.NormalizePath(currentPath.text);
        FileManagement.DeleteDirectory(FileManagement.Combine(currentPath.text, selectedItem), _fullPath);
        Cancel();
    }

    /// <summary>Asks for the name of the new folder</summary>
    public void PromtNewFolderName()
    {
        newNameWindow.SetActive(true);
        newNameLabel.text = "Plase write the new folder name:";
        inputNewName.ActivateInputField();
        inputNewName.text = "";
        _action = NewFolder;
    }
    void NewFolder()
    {
        if (inputNewName.text != "")
        {
            // Create the new folder:
            currentPath.text = FileManagement.NormalizePath(currentPath.text);
            string directory = FileManagement.Combine(currentPath.text, inputNewName.text);
            FileManagement.CreateDirectory(directory);
            inputNewName.text = "";
            newNameWindow.SetActive(false);
            Cancel();
        }
    }

    /// <summary>Asks for the new name (renames files and folders)</summary>
    public void PromptForRename()
    {
        if (selectedItem != "")
        {
            string path = FileManagement.Combine(currentPath.text, selectedItem);
            if (!FileManagement.FileExists(path, false, _fullPath) && _selectionType == "F")
            {
                PromtErrorMessage("Can't rename. The file is read only (" + selectedItem + ").");
            }
            else if (!FileManagement.DirectoryExists(path, false, _fullPath) && _selectionType == "D")
            {
                PromtErrorMessage("Can't rename. The folder is read only (" + selectedItem + ").");
            }
            else
            {
                newNameWindow.SetActive(true);
                newNameLabel.text = "Plase write a new name for: " + selectedItem;
                inputNewName.ActivateInputField();
                inputNewName.text = selectedItem;
                _action = Rename;
            }
        }
    }
    void Rename()
    {
        if (inputNewName.text != "")
        {
            // Rename the file or folder:
            currentPath.text = FileManagement.NormalizePath(currentPath.text);
            string source = FileManagement.Combine(currentPath.text, selectedItem);
            string dest = FileManagement.Combine(currentPath.text, inputNewName.text);
            FileManagement.Rename(source, dest, _fullPath, _fullPath);
            Cancel();
        }
    }

    /// <summary>Shows an error message</summary>
    void PromtErrorMessage(string msg)
    {
        errorMsgLabel.text = msg;
        errorMessage.SetActive(true);
        _action = Cancel;
    }

    /// <summary>Confirm action</summary>
    public void Confirm()
    {
        _action();              // Execute delegated action.
    }
    /// <summary>Cancel action</summary>
    public void Cancel()
    {
        if (confirmation.activeInHierarchy)
            confirmation.SetActive(false);
        if (newNameWindow.activeInHierarchy)
            newNameWindow.SetActive(false);
        if (errorMessage.activeInHierarchy)
            errorMessage.SetActive(false);
        ShowFolderContent();    // Update the view.
    }

    /// <summary>Cut a path (not clipboard)</summary>
    public void Cut()
    {
        if (selectedItem != "")
        {
            // Can move files or folders (Excepting from StreamingAssets):
            string path = FileManagement.Combine(currentPath.text, selectedItem);
            if (!FileManagement.FileExists(path, false, _fullPath) && _selectionType == "F")
            {
                PromtErrorMessage("Can't cut. The file is read only (" + selectedItem + ").");
            }
            else if (!FileManagement.DirectoryExists(path, false, _fullPath) && _selectionType == "D")
            {
                PromtErrorMessage("Can't cut. The folder is read only (" + selectedItem + ").");
            }
            else
            {
                sourceType = _selectionType;
                sourcePath = FileManagement.NormalizePath(path);
                moveSourcePath = true;
            }
        }
    }
    /// <summary>Copy a path (not clipboard)</summary>
    public void Copy()
    {
        if(selectedItem != "")
        {
            sourcePath = FileManagement.Combine(currentPath.text, selectedItem);
            sourcePath = FileManagement.NormalizePath(sourcePath);
            sourceType = _selectionType;
            moveSourcePath = false;
        }
    }
    /// <summary>Paste a path (not clipboard)</summary>
    public void Paste()
    {
        string pastePath = FileManagement.Combine(currentPath.text, FileManagement.GetFileName(sourcePath));
        pastePath = FileManagement.NormalizePath(pastePath);
        if(sourcePath != pastePath)
        {
            if (moveSourcePath)
            {
                // Move files or folders:
                FileManagement.Move(sourcePath, pastePath, _fullPath, _fullPath);
            }
            else
            {
                // Copy files or folders:
                if (sourceType == "F")
                    FileManagement.CopyFile(sourcePath, pastePath, true, _fullPath, _fullPath);
                else if (sourceType == "D")
                    FileManagement.CopyDirectory(sourcePath, pastePath, true, _fullPath, _fullPath);
            }
            ShowFolderContent();
        }
    }
}
