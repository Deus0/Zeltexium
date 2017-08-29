using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeltex.Voxels;
using Zeltex.Util;
using System.IO;

/// <summary>
/// Run functions in the editor
/// </summary>
[ExecuteInEditMode]
public class EditorFunctions : MonoBehaviour
{
    public bool IsRunCommand = false;
    public string Command;
    public List<string> MyCommands = new List<string>();
    [Header("References")]
    public VoxelManager MyDatabase;
	
	// Update is called once per frame
	void Update ()
    {
	    if (IsRunCommand)
        {
            IsRunCommand = false;
            /*Texture2D MyTexture = MyDatabase.MyMaterial.mainTexture as Texture2D;
            // Encode texture into PNG
            byte[] MyBytes = MyTexture.EncodeToPNG();
            string FilePath = FileUtil.GetWorldFolderPath() + "TileMap.png";
            File.WriteAllBytes(FilePath, MyBytes);*/
        }
    }

    IEnumerator UploadPNG()
    {
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        // For testing purposes, also write to a file in the project folder
        // File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);


        // Create a Web Form
        WWWForm form = new WWWForm();
        form.AddField("frameCount", Time.frameCount.ToString());
        form.AddBinaryData("fileUpload", bytes);

        // Upload to a cgi script
        WWW w = new WWW("http://localhost/cgi-bin/env.cgi?post", form);
        yield return w;

        if (w.error != null)
        {
            Debug.Log(w.error);
        }
        else {
            Debug.Log("Finished Uploading Screenshot");
        }
    }
}
