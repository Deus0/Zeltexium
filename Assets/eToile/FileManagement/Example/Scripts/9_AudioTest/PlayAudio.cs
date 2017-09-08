using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Example script to demonstrate the use of audio capabilities.
 * 
 * Loads and plays audio files from any selected path (Using the file browser).
 */

public class PlayAudio : MonoBehaviour {

    public GameObject fileBrowser;
    AudioSource _source;
    Text playerText;

    // Use this for initialization
    void Start ()
    {
        _source = transform.GetComponent<AudioSource>();
        playerText = transform.Find("PlayerText").GetComponent<Text>();
    }

    // Instantiates a file browser and sets the path selection event:
    public void OpenFileBrowser()
    {
        // Creates a browser windows and sets its behaviour mode:
        GameObject browserInstance = GameObject.Instantiate(fileBrowser);
        browserInstance.GetComponent<FileBrowser>().SetBrowserWindow(OnPathSelected, Application.persistentDataPath, true);
        string[] filter = { ".wav", ".mp3", ".ogg" };
        browserInstance.GetComponent<FileBrowser>().SetBrowserWindowFilter(filter);
    }

    // You should use this function signature in order to receive properly:
    void OnPathSelected(string path)
    {
        playerText.text = FileManagement.GetFileName(path);
        AudioClip _clip = FileManagement.ImportAudio(path, false, false, true);
        // The clip will be null if not parsed correctly:
        if(_clip != null)
        {
            _source.clip = _clip;
            _source.Play();
        }
        else
        {
            playerText.text = "-";
        }
    }

    public void Play()
    {
        if (_source.clip != null)
            _source.Play();
        else
            playerText.text = "-";
    }

    public void Pause()
    {
        if (_source.isPlaying)
            _source.Pause();
    }

    public void Stop()
    {
        if (_source.isPlaying)
            _source.Stop();
    }
}
