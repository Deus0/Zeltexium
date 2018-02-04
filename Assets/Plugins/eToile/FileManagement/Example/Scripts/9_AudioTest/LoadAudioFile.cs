using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * Example script to demonstrate the use of audio capabilities.
 * 
 * Loads and plays audio files from StreamingAssets folder.
 */

public class LoadAudioFile : MonoBehaviour {

    AudioSource audioSource;

    Text wavText;
    Text mp3Text;
    Text oggText;

    // Use this for initialization
    void Start () {
        audioSource = gameObject.GetComponent<AudioSource>();

        wavText = transform.Find("WavText").GetComponent<Text>();
        mp3Text = transform.Find("Mp3Text").GetComponent<Text>();
        oggText = transform.Find("OggText").GetComponent<Text>();
    }
	
    public void PlaySound(string extension)
    {
        string fileName = "Example." + extension;
        // Detect the file extension:
        Text temp = null;
        switch (extension)
        {
            case "wav":
                temp = wavText;
                break;
            case "mp3":
                temp = mp3Text;
                break;
            case "ogg":
                temp = oggText;
                break;
        }
        // Load the audio file:
        if (FileManagement.FileExists(fileName))
        {
            audioSource.clip = FileManagement.ImportAudio(fileName);
            // Shows compatibility message depending on platform compatibility:
            if (audioSource.clip != null)
            {
                audioSource.Play();
                temp.text = "OK.";
            }
            else
            {
                temp.text = "Not supported.";
            }
        }
        else
        {
            // File not found:
            temp.text = "Not found.";
        }
    }
}
