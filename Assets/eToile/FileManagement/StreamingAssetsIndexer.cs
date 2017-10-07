#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

/*
 * This script runs on editor only and will be not included in the final build.
 * 
 * This script creates a file Index based in the StreamingAssets folder content.
 * This Index is used to retrieve the folder content for Android and WebGL platforms
 * due to there is no dynamic access to this folder.
 */

class StreamingAssetsIndexer : AssetPostprocessor
{
    // Asset content modification event:
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // Creates the StreamingAssets folder:
        if (FileManagement.DirectoryExists(Application.streamingAssetsPath))
        {
            // Add first the files, then the folders and iterate through each one.
            string content = "";
            GetFolderContent(Application.streamingAssetsPath, ref content);
            if (content != "")
                content = content.Substring(0, content.Length - 1);
            // Saves into StreamingAssets to be included in the export:
            FileManagement.SaveFile(Application.streamingAssetsPath + "/FMSA_Index", content, false, true);
        }
    }
    // Recursive function to retrieve all content of folders and subfolders:
    static void GetFolderContent(string fullPath, ref string content)
    {
        // Add files first:
        string[] files = System.IO.Directory.GetFiles(fullPath);
        for (int i = 0; i < files.Length; i++)
        {
            if (!files[i].Contains(".meta"))    // Discard meta files.
            {
                content += files[i].Substring(files[i].IndexOf("StreamingAssets")).Replace('\\', '/') + ",F;";
            }
        }
        // Then add folders:
        string[] folders = System.IO.Directory.GetDirectories(fullPath);
        for (int i = 0; i < folders.Length; i++)
        {
            content += folders[i].Substring(files[i].IndexOf("StreamingAssets")).Replace('\\', '/') + ",D;";      // D means directory.
        }
        // Continues only if there is something to index
        if(content != "")
        {
            // Discard last separator:
            content = content.Substring(0, content.Length - 1);
            content += "|";     // Block separator.
            // Iterate subdirectories:
            for (int i = 0; i < folders.Length; i++)
            {
                GetFolderContent(folders[i], ref content);
            }
        }
    }
}

#endif