
// Encryption method directive:
//#define USE_AES

using System.Linq;
using UnityEngine;

#if UNITY_WEBGL
// Plugin (WebGL):
using System.Runtime.InteropServices;
#endif

#if USE_AES
// AES Encryption (Not compatible with WindowsPhone):
using System.Security.Cryptography;
#endif

/*
 * File Management:
 * 
 * Easy control over files. All data is saved in binary so preventing data loss or corruption.
 * This asset was downloaded from the Unity AssetStore:
 * 
 * https://www.assetstore.unity3d.com/en/#!/content/67183
 * 
 * V 1.1 Features:
 * - You may chose between Xor or Aes encryption algorythms manually.
 * Define or comment the USE_AES directive to switch encryption methods.
 * (AES encryption is not available on Windows Phone platforms)
 * - Images can be easily encrypted.
 * 
 * V 1.2 Features:
 * - New feature: Unity types Vector2, Vector3, Vector4, Quaternion, Rect, Color and Color32 now supported for save and load.
 * - New feature: Load AudioClip from file (WebGL not supported yet).
 * - New feature: Increased WebGL save size (Now images can be saved in web browser too!!!).
 * - New feature: Save and load Arrays or Lists in a single line of code.
 * - New feature: Automated virtual StreamingAssets file/directory index. Now StreamingAssets can be accessed on Android and WebGL as real folders!!!
 * - New feature: Directory tools: Exists, Create, Delete and Empty content (StreamingAssets is read only).
 * - New feature: Get lists of files or directories from a given path.
 * - New feature: Get a List<byte[]> with the whole content of a given folder (matching the file name list ListFiles()).
 * - BugFix: File existance detection wasn't working on Android/StreamingAssets.
 * - BugFix: Optimized reading very big strings.
 * 
 * V 1.3 Features:
 * - New feature: Add raw data to an existing file with AddRawData().
 * - New feature: You can drag and resize the BrowserWindow.
 * - New feature: Added several configurations that can be combined to achieve different Browser behaviours.
 * - New feature: Added NormalizePath(), GetExtension() and GetFileExtension() interfaces (check documentation).
 * - New feature: Added CopyFile(), CopyDirectory(), Move() and Rename() (fully compatible with files and folders).
 * - New feature: Added Cut, Copy and Paste buttons to the FileBrowser.
 * - New feature: Added Delete, Rename and New folder buttons to the FileBrowser.
 * - New feature: Creates the requested path when writing to disk.
 * - New feature: WAV files import now supported in WebGL (uses a custom wrapper).
 * - BugFix: Important improvements in path interpretation (OSX bugfixes).
 * 
 * V 1.4 Features:
 * - New Feature: ReadList and ReadArray overflow allowing multiple separatos. (Useful for CSV parsing)
 * - New Feature: ReadAllLines from any platform using multiple line separators.
 * - New Feature: Navigate back and fwd in FileBrowser window.
 * - BugFix: Extended WAV files compatibility.
 * - BugFix: Importing audio in Android.
 * - BugFix: Not copying empty folders.
 * - BugFix: Retrieving wring files from StreamingAssets index.
 * 
 * V 1.5 Features:
 * - New Feature: Filters the ListFiles view by multiple file extensions.
 * - New Feature: File list filtering is integrated to the FileBrowser prefab.
 * - New Feature: Dropdown with the desired file extensions from the filter.
 * - BugFix: Thread safe.
 */

public static class FileManagement
{
    // For AES encryption, 'key' must be 16, 24 or 32 bytes length.
    // Xor encryption uses any length.

    // IMPORTANT: DON'T FORGET TO SET YOUR OWN KEY (Keys must never be written in plain text)
    private readonly static byte[] key = { 217, 134, 151, 168, 185, 202, 129, 135, 150, 130, 141, 201, 210, 167, 198, 169 };

    private static string[] blocks;     // This is the StreamingAssets index for Android and WebGL.

    private static string persistentDataPath = Application.persistentDataPath;
    private static string streamingAssetsPath = Application.streamingAssetsPath;

#if UNITY_WINRT
    /// <summary>Saves a new file (overwrites if exists an older one)</summary>
    public static void SaveRawFile(string name, byte[] content, bool enc = false, bool fullPath = false)
    {
        if (name != "")
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);
            if (content != null)
            {
                CreateDirectory(GetParentDirectory(name));
                if (enc)
                    UnityEngine.Windows.File.WriteAllBytes(name, Encrypt(content, key));
                else
                    UnityEngine.Windows.File.WriteAllBytes(name, content);
            }
            else
                Debug.LogError("[FileManagement.SaveRawFile] Exception: Trying to save null data.");
        }
        else
            Debug.LogError("[FileManagement.SaveRawFile] Can't save an unnamed file.");
    }

    /// <summary>Returns the byte[] content of a file</summary>
    public static byte[] ReadRawFile(string name, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        byte[] contentArray = { };
        string path = name;

        if (name != "")
        {
            // First checks if file exists no matter where:
            if (FileExists(path, checkSA, fullPath))
            {
                if (!fullPath)
                {
                    // Checks if file exists in PersistentDataPath only:
                    if (FileExists(name, false))
                        path = Combine(persistentDataPath, name);
                    else if (checkSA)    // Then checks StreamingAssets if desired.
                        path = Combine(streamingAssetsPath, name);
                }
                // Read content normally:
                contentArray = UnityEngine.Windows.File.ReadAllBytes(path);
            }
            else
                Debug.LogWarning("[FileManagement.ReadRawFile] File not found: " + path);

            // Decryption:
            if (contentArray.Length > 0 && enc)
                contentArray = Decrypt(contentArray, key);
        }
        else
            Debug.LogError("[FileManagement.ReadRawFile] Can't read an unnamed file.");
        return contentArray;
    }

    /// <summary>Deletes a file</summary>
    public static void DeleteFile(string name, bool fullPath = false)
    {
        if (FileExists(name, false, fullPath))
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);
            UnityEngine.Windows.File.Delete(name);
        }
        else
            Debug.LogWarning("[FileManagement.DeleteFile] File not found: " + name);
    }

    /// <summary>Load audio file into AudioClip</summary>
    public static AudioClip ImportAudio(string file, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        if (!fullPath)
        {
            // Checks if file exists in PersistentDataPath only:
            if (FileExists(file, false))
                file = Combine(persistentDataPath, file);
            else if (checkSA)    // Then checks StreamingAssets if desired.
                file = Combine(streamingAssetsPath, file);
        }

        file = "file://" + NormalizePath(file);

        // Imports audio clip:
        WWW www = new WWW(file);
        while (!www.isDone) { };    // Simulates synchronous reading.

        if (string.IsNullOrEmpty(www.error))
            return www.audioClip;

        Debug.LogError("[FileManagement.ImportAudio] WWW error: " + www.error);
        return null;
    }

#elif UNITY_WEBGL

    [DllImport("__Internal")] private static extern void SyncFiles();
    [DllImport("__Internal")] private static extern int ReadFileLen(string url);   // Downloads a file and returns teh length.
    [DllImport("__Internal")] private static extern System.IntPtr ReadData();
    [DllImport("__Internal")] public static extern void ShowMessage(string msg);    // Shows messages in browser for debugging.

    // Legacy interfaces (for cookies):
    [DllImport("__Internal")] private static extern void WriteCookie(string name, string value, string expires = null, bool sec = false, string path = null, string domain = null);
    [DllImport("__Internal")] private static extern void DeleteCookie(string name, string path = null, string domain = null);
    [DllImport("__Internal")] private static extern int ReadCookieLen(string name);
    [DllImport("__Internal")] private static extern void DeleteAllCookies();

    /// <summary>Saves a new file (overwrites if exists an older one)</summary>
    public static void SaveRawFile(string name, byte[] content, bool enc = false, bool fullPath = false)
    {
        if (name != "")
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);

            if (content != null)
            {
                CreateDirectory(GetParentDirectory(name));
                if (enc)
                    System.IO.File.WriteAllBytes(name, Encrypt(content, key));
                else
                    System.IO.File.WriteAllBytes(name, content);

                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    SyncFiles();
            }
            else
            {
                Debug.LogError("[FileManagement.SaveRawFile] Exception: Trying to save null data.");
                //ShowMessage("[FileManagement.SaveRawFile] Exception: Trying to save null data.");
            }
        }
        else
        {
            Debug.LogError("[FileManagement.SaveRawFile] Can't save an unnamed file.");
            //ShowMessage("[FileManagement.SaveRawFile] Can't save an unnamed file.");
        }
    }

    /// <summary>Returns the byte[] content of a file</summary>
    public static byte[] ReadRawFile(string name, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        byte[] contentArray = { };
        string path = name;
        if (name != "")
        {
            // First checks if file exists no matter where:
            if (FileExists(path, checkSA, fullPath))
            {
                if (!fullPath)
                {
                    // Checks if file exists in PersistentDataPath only:
                    if (FileExists(name, false))
                        path = Combine(persistentDataPath, name);
                    else if (checkSA)    // Then checks StreamingAssets if desired.
                        path = Combine(streamingAssetsPath, name);
                }
                // Chose correct method to read:
                if (path.Contains("://") && Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    // Read content from StreamingAssets:
                    int contentLen = ReadFileLen(path);     // Get file length.
                    if (contentLen > 0)
                    {
                        contentArray = new byte[contentLen];
                        Marshal.Copy(ReadData(), contentArray, 0, contentLen);
                    }
                    else
                    {
                        Debug.LogWarning("[FileManagement.ReadRawFile] File was deleted in server side: " + path);
                        //ShowMessage("[FileManagement.ReadRawFile] File was deleted in server side: " + path);
                    }
                }
                else
                {
                    // Read content normally:
                    contentArray = System.IO.File.ReadAllBytes(path);
                }
            }
            else
            {
                Debug.LogWarning("[FileManagement.ReadRawFile] File not found: " + path);
                //ShowMessage("[FileManagement.ReadRawFile] File not found: " + path);
            }

            // Decryption:
            if (contentArray.Length > 0 && enc)
                contentArray = Decrypt(contentArray, key);
        }
        else
        {
            Debug.LogError("[FileManagement.ReadRawFile] Can't read an unnamed file.");
            //ShowMessage("[FileManagement.ReadRawFile] Can't read an unnamed file.");
        }
        return contentArray;
    }

    /// <summary>Deletes a file</summary>
    public static void DeleteFile(string name, bool fullPath = false)
    {
        if (FileExists(name, false, fullPath))
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);
            System.IO.File.Delete(name);

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                SyncFiles();
        }
        else
        {
            Debug.LogWarning("[FileManagement.DeleteFile] File not found: " + name);
            //ShowMessage("[FileManagement.DeleteFile] File not found: " + name);
        }
    }

    /// <summary>Checks a name into the automatic StreamingAssets index file (WebGL)</summary>
    private static bool CheckNameOnIndex(string name, string type)
    {
        // First block is StreamingAssets, then there are every subfolders:
        if(blocks == null)
        {
            // Load blocks first time:
            string indexPath = streamingAssetsPath + "/FMSA_Index";
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                int contentLen = ReadFileLen(indexPath);     // Get file length.
                if (contentLen > 0)
                {
                    // Get index directly:
                    byte[] contentArray = new byte[contentLen];
                    Marshal.Copy(ReadData(), contentArray, 0, contentLen);
                    // Convert byte[] to string:
                    char[] chars = new char[contentArray.Length];
                    for (int c = 0; c < contentArray.Length; c++)
                        chars[c] = (char)contentArray[c];
                    string content = new string(chars);
                    // Get blocks:
                    blocks = content.Split('|');
                }
                else
                {
                    blocks = new string[0];
                    Debug.LogWarning("[FileManagement.CheckNameOnIndex] Index file not found: " + indexPath);
                    //ShowMessage("[FileManagement.CheckNameOnIndex] Index file not found: " + indexPath);
                }
            }
            else
            {
                // To allow testing in editor:
                byte[] contentArray = { };
                if (System.IO.File.Exists(indexPath))
                    contentArray = System.IO.File.ReadAllBytes(indexPath);
                // Convert byte[] to string:
                char[] chars = new char[contentArray.Length];
                for (int c = 0; c < contentArray.Length; c++)
                    chars[c] = (char)contentArray[c];
                string content = new string(chars);
                // Get blocks:
                blocks = content.Split('|');
            }
        }
        // Checks name existance:
        name = name.Replace('\\', '/');     // Normalize path format with index.
        for (int b = 0; b < blocks.Length; b++)
        {
            // Search every folder and subfodler separatelly:
            string[] entries = blocks[b].Split(';');
            for (int e = 0; e < entries.Length; e++)
            {
                string[] data = entries[e].Split(',');  // [0] file path and name, [1] type ("F" or "D").
                data[0] = data[0].Replace('\\', '/');
                if (name == data[0] && data[1] == type)
                    return true;
            }
        }
        return false;
    }

    /// <summary>Checks a virtual path into the index and returns the names matching (WebGL)</summary>
    private static string[] GetNamesOnIndex(string name, string type)
    {
        if (blocks == null)
            CheckNameOnIndex("", "");
        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
        name = name.Replace('\\', '/');     // Normalize path format with index.
        name = name.Replace("//", "/");            // Allow root folder (empty path).
        // Collect name occurrencies:
        bool exit = false;
        for (int b = 0; b < blocks.Length; b++)
        {
            // Search every folder and subfodler separatelly:
            string[] entries = blocks[b].Split(';');
            for (int e = 0; e < entries.Length; e++)
            {
                string[] data = entries[e].Split(',');  // [0] file path and name, [1] type ("F" or "D").
                data[0] = data[0].Replace('\\', '/');
                if (data[0].Contains(name) && type == data[1])
                {
                    list.Add(data[0]);
                    exit = true;
                }
            }
            if (exit)
                break;
        }
        return list.ToArray();
    }

    /// <summary>Load audio file into AudioClip</summary>
    public static AudioClip ImportAudio(string file, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        string fileExt = GetFileExtension(file).ToLower();
        if(fileExt == ".wav")
        {
            /* WAV file format:
             * size - Name          - (index) Description.
             * 4 - ChunkID          - (0)   "RIFF"
             * 4 - ChunkSize        - (4)   file size minus 8 (RIFF(4) + ChunkSize(4)).
             * 4 - Format           - (8)   "WAVE"
             * 
             * 4 - Subchunk1ID      - (12)  "fmt "
             * 4 - Subchunk1Size    - (16)  16 for PCM (20 to 36)
             * 2 - AudioFormat      - (20)  1 for PCM (other values implies some compression).
             * 2 - NumChannels      - (22)  Mono = 1, Stereo = 2, etc.
             * 4 - SampleRate       - (24)  8000, 22050, 44100, etc.
             * 4 - ByteRate         - (28)  == SampleRate * NumChannels * (BitsPerSample/8)
             * 2 - BlockAlign       - (32)  == NumChannels * (BitsPerSample/8)
             * 2 - BitsPerSample    - (34)  8 bits = 8, 16 bits = 16, etc.
             * (Here goes the extra data pointed by Subchunk1Size > 16)
             * 
             * 4 - Subchunk2ID      - (36) "data"
             * 4 - Subchunk2Size    - (40)
             * Subchunk2Size - Data - (44)
             */

            byte[] wavFile = ReadRawFile(file, enc, checkSA, fullPath);
            //int _chunkSize = System.BitConverter.ToInt32(wavFile, 4);     // Not used.
            int _subchunk1Size = System.BitConverter.ToInt32(wavFile, 16);
            int _audioFormat = System.BitConverter.ToInt16(wavFile, 20);
            int _numChannels = System.BitConverter.ToInt16(wavFile, 22);
            int _sampleRate = System.BitConverter.ToInt32(wavFile, 24);
            //int _byteRate = System.BitConverter.ToInt32(wavFile, 28);     // Not used.
            //int _blockAlign = System.BitConverter.ToInt16(wavFile, 32);   // Not used.
            int _bitsPerSample = System.BitConverter.ToInt16(wavFile, 34);  // Not used?
            // Find where data starts:
            int _dataIndex = 20 + _subchunk1Size;
            for(int i = _dataIndex; i < wavFile.Length; i++)
            {
                if (wavFile[i] == 'd' && wavFile[i + 1] == 'a' && wavFile[i + 2] == 't' && wavFile[i + 3] == 'a')
                {
                    _dataIndex = i + 4;
                    break;
                }
            }
            // Data parameters:
            int _subchunk2Size = System.BitConverter.ToInt32(wavFile, _dataIndex);  // Data size.
            int _sampleSize = _bitsPerSample / 8;                                   // Size of a sample.
            int _sampleCount = _subchunk2Size / _sampleSize;                        // How many samples into data.
            // WAV method:
            if (_audioFormat == 1)
            {
                float[] _audioBuffer = new float[_sampleCount];  // Size for all available channels.
                for (int i = 0; i < _sampleCount; i++)
                {
                    int sampleIndex = _dataIndex + i * _sampleSize;
                    float sample = System.BitConverter.ToInt16(wavFile, sampleIndex) / 32768.0F;
                    _audioBuffer[i] = sample;
                }
                // Create the AudioClip:
                AudioClip audioClip = AudioClip.Create(GetFileName(file), _sampleCount, _numChannels, _sampleRate, false);
                audioClip.SetData(_audioBuffer, 0);
                return audioClip;
            }
            else
            {
                Debug.LogError("[FileManagement.ImportAudio] Compressed wav format not supported.");
                return null;
            }
        }
        else
        {
            Debug.LogError("[FileManagement.ImportAudio] " + fileExt + " format not supported in this platform.");
            return null;
        }
    }

#else

    /// <summary>Saves a new file (overwrites if exists an older one)</summary>
    public static void SaveRawFile(string name, byte[] content, bool enc = false, bool fullPath = false)
	{
        if (name != "")
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);

            if (content != null)
            {
                CreateDirectory(GetParentDirectory(name));
                if (enc)
                    System.IO.File.WriteAllBytes(name, Encrypt(content, key));
                else
                    System.IO.File.WriteAllBytes(name, content);
            }
            else
                Debug.LogError("[FileManagement.SaveRawFile] Exception: Trying to save null data.");
        }
        else
            Debug.LogError("[FileManagemnt.SaveRawFile] Can't save an unnamed file.");
    }

    /// <summary>Returns the byte[] content of a file</summary>
    public static byte[] ReadRawFile(string name, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        byte[] contentArray = { };
        string path = name;

        if (name != "")
        {
            // First checks if file exists no matter where:
            if (FileExists(path, checkSA, fullPath))
            {
                if (!fullPath)
                {
                    // Checks if file exists in PersistentDataPath only:
                    if (FileExists(name, false))
                        path = Combine(persistentDataPath, name);
                    else if (checkSA)    // Then checks StreamingAssets if desired.
                        path = Combine(streamingAssetsPath, name);
                }
                // Chose correct method to read:
                if (path.Contains("://") && Application.platform == RuntimePlatform.Android)
                {
                    // Read content:
                    WWW www = new WWW(path);
                    while (!www.isDone) { };    // Simulates synchronous reading (don't use on WebGL).
                    if (string.IsNullOrEmpty(www.error))
                        contentArray = www.bytes;
                    else
                        Debug.LogError("[FileManagement.ReadRawFile] WWW error: " + www.error);
                }
                else
                {
                    // Read content normally:
                    contentArray = System.IO.File.ReadAllBytes(path);
                }
            }
            else
                Debug.LogWarning("[FileManagement.ReadRawFile] File not found: " + path);

            // Decryption:
            if (contentArray.Length > 0 && enc)
                contentArray = Decrypt(contentArray, key);
        }
        else
            Debug.LogError("[FileManagement.ReadRawFile] Can't read an unnamed file.");

        return contentArray;
    }

    /// <summary>Deletes a file</summary>
    public static void DeleteFile(string name, bool fullPath = false)
    {
        if (FileExists(name, false, fullPath))
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);
            System.IO.File.Delete(name);
        }
        else
            Debug.LogWarning("[FileManagement.DeleteFile] File not found: " + name);
    }

    /// <summary>Checks a name into the automatic StreamingAssets index file (Android)</summary>
    private static bool CheckNameOnIndex(string name, string type)
    {
        // First block is StreamingAssets, then there are every subfolders:
        if (blocks == null)
        {
            // Load blocks first time:
            string indexPath = Combine(streamingAssetsPath, "FMSA_Index");
            byte[] contentArray = { };
            if (indexPath.Contains("://") && Application.platform == RuntimePlatform.Android)
            {
                // Read content:
                WWW www = new WWW(indexPath);
                while (!www.isDone) { };    // Simulates synchronous reading (don't use on WebGL).
                if (string.IsNullOrEmpty(www.error))
                    contentArray = www.bytes;
            }
            else
            {
                // Read content normally (It allows testing in Editor for Android):
                indexPath = NormalizePath(indexPath);
                if (System.IO.File.Exists(indexPath))
                    contentArray = System.IO.File.ReadAllBytes(indexPath);
                else
                    contentArray = new byte[0];
            }
            // If file was successfully found:
            if (contentArray != null)
            {
                char[] chars = new char[contentArray.Length];
                for (int c = 0; c < contentArray.Length; c++)
                    chars[c] = (char)contentArray[c];
                string content = new string(chars);
                blocks = content.Split('|');
            }
            else
            {
                Debug.LogWarning("[FileManagement.CheckNameOnIndex] Index file not found: " + indexPath);
            }
        }
        // Checks name existence:
        name = name.Replace('\\', '/');      // Normalize path format with index.
        for (int b = 0; b < blocks.Length; b++)
        {
            // Search every folder and subfodler separatelly:
            string[] entries = blocks[b].Split(';');
            for (int e = 0; e < entries.Length; e++)
            {
                string[] data = entries[e].Split(',');  // [0] file path and name, [1] type ("F" or "D").
                data[0] = data[0].Replace('\\', '/');
                if (name == data[0] && type == data[1])
                    return true;
            }
        }
        return false;
    }

    /// <summary>Checks a virtual path into the index and returns the names matching (Android)</summary>
    private static string[] GetNamesOnIndex(string name, string type)
    {
        if (blocks == null)
            CheckNameOnIndex("", "");   // Forces load the index (if not already loaded)
        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
        name = name.Replace("//", "/");                 // Allow root folder (empty path).
        name = name.Replace('\\', '/');                 // Normalize path format with index.
        // Collect name occurrencies:
        bool exit = false;
        for (int b = 0; b < blocks.Length; b++)
        {
            // Search every folder and subfodler separatelly:
            string[] entries = blocks[b].Split(';');
            for (int e = 0; e < entries.Length; e++)
            {
                string[] data = entries[e].Split(',');  // [0] file path and name, [1] type ("F" or "D").
                data[0] = NormalizePath(data[0]);
                if (data[0].StartsWith(name) && type == data[1])
                {
                    list.Add(data[0]);
                    exit = true;
                }
            }
            if (exit)
                break;
        }
        return list.ToArray();
    }

    /// <summary>Load audio file into AudioClip</summary>
    public static AudioClip ImportAudio(string file, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        if (!fullPath)
        {
            // Checks if file exists in PersistentDataPath only:
            if (FileExists(file, false))
            {
                file = Combine(persistentDataPath, file);
                file = "file://" + file;
            }
            else if (checkSA)    // Then checks StreamingAssets if desired.
            {
                file = Combine(streamingAssetsPath, file);
                if(Application.platform != RuntimePlatform.Android)
                    file = "file://" + file;
            }
        }
        else
        {
            file = "file://" + file;
        }

        // Imports audio clip:
        WWW www = new WWW(file);
        while (!www.isDone) { };    // Simulates synchronous reading.

        if (string.IsNullOrEmpty(www.error))
            return www.GetAudioClip();

        Debug.LogError("[FileManagement.ImportAudio] WWW error: " + www.error);
        return null;
    }

#endif

    /// <summary>Checks file existence (checks PersistentData, then StreamingAssets)</summary>
    public static bool FileExists(string name, bool checkSA = true, bool fullPath = false)
    {
        // Check existance:
        bool result = false;
        if (!fullPath)
        {
            // Check PersistentData path first:
            string path = Combine(persistentDataPath, name);
            result = System.IO.File.Exists(path);
            if (!result && checkSA)
            {
                // Then check StreamingAssets path:
#if UNITY_ANDROID || UNITY_WEBGL
                result = CheckNameOnIndex("StreamingAssets/"+name, "F");
#else
                path = Combine(streamingAssetsPath, name);
                result = System.IO.File.Exists(path);
#endif
            }
        }
        else
        {
            // Direct check:
            result = System.IO.File.Exists(name);
        }
        return result;
    }

    /// <summary>Save file with convertion (StreamingAssets folder is read only)</summary>
    public static void SaveFile<T>(string name, T content, bool enc = false, bool fullPath = false)
    {
        string temp = content.ToString();
        byte[] contentArray = new byte[temp.Length];
        for (int c = 0; c < contentArray.Length; c++) contentArray[c] = (byte)temp[c];
        SaveRawFile(name, contentArray, enc, fullPath);
    }

    /// <summary>Read file with conversion</summary>
    public static T ReadFile<T>(string name, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        byte[] contentArray = { };
        string content = "";
        T val = default(T);   // Return value.
        contentArray = ReadRawFile(name, enc, checkSA, fullPath);

        // Restore the string to parse:
        if (contentArray.Length > 0)
        {
            char[] chars = new char[contentArray.Length];
            for (int c = 0; c < contentArray.Length; c++)
                chars[c] = (char)contentArray[c];
            content = new string(chars);
        }
        else
            return default(T);  // There is no string, so return the apropriate null value.

        try
        {
            val = CustomParser<T>(content);
        }
        catch (System.FormatException)
        {
            Debug.LogError("[FileManagement.ReadFile] Exception: FormatException - Trying to read data in the wrong format. (" + typeof(T) + "): " + name);
        }

        return val;
    }

    /// <summary>Saves arrays or lists of one dimension</summary>
    public static void SaveArray<T>(string name, T[] content, char separator = (char)0x09, bool enc = false, bool fullPath = false)
    {
        string save = "";
        for (int i = 0; i < content.Length; i++)
        {
            save += content[i].ToString();
            if (i < (content.Length - 1))
                save += separator;
        }
        if (content.Length > 0)
            SaveFile(name, save, enc, fullPath);
        else
            Debug.LogError("[FileManagement.SaveArray] Trying to save empty array: " + name);
    }
    /// <summary>Saves arrays or lists of one dimension</summary>
    public static void SaveArray<T>(string name, System.Collections.Generic.List<T> content, char separator = (char)0x09, bool enc = false, bool fullPath = false)
    {
        SaveArray(name, content.ToArray(), separator, enc, fullPath);
    }

    /// <summary>Reads a one dimension Array from file</summary>
    public static T[] ReadArray<T>(string name, char separator = (char)0x09, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        return ReadList<T>(name, separator, enc, checkSA, fullPath).ToArray();
    }
    /// <summary>Reads a one dimension Array from file</summary>
    public static T[] ReadArray<T>(string name, string[] separator, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        return ReadList<T>(name, separator, enc, checkSA, fullPath).ToArray();
    }

    /// <summary>Reads a List from file</summary>
    public static System.Collections.Generic.List<T> ReadList<T>(string name, char separator = (char)0x09, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();
        string[] content = ReadFile<string>(name, enc, checkSA, fullPath).Split(separator);
        for(int i = 0; i < content.Length; i++)
        {
            list.Add(CustomParser<T>(content[i]));
        }
        return list;
    }
    /// <summary>Reads a List from file</summary>
    public static System.Collections.Generic.List<T> ReadList<T>(string name, string[] separator, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();
        string[] content = ReadFile<string>(name, enc, checkSA, fullPath).Split(separator, System.StringSplitOptions.None);
        for (int i = 0; i < content.Length; i++)
        {
            list.Add(CustomParser<T>(content[i]));
        }
        return list;
    }

    /// <summary>Reads all text lines from a file</summary>
    public static string[] ReadAllLines(string name, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        string[] eol = { "\r\n", "\n", "\r" };
        return ReadArray<string>(name, eol, enc, checkSA, fullPath);
    }

    /// <summary>Loads a JPG/PNG image from file and returns a Texture2D</summary>
    public static Texture2D ImportTexture(string file, bool enc = false, bool checkSA = true, bool fullPath = false)
	{
		byte[] image = ReadRawFile(file, enc, checkSA, fullPath);
		Texture2D texture = new Texture2D(2,2); // Assigns minimum size
		texture.LoadImage(image);
        texture.Apply();
		return texture;
	}

    /// <summary>Loads a JPG/PNG image from file and returns a Sprite</summary>
    public static Sprite ImportSprite(string file, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        Sprite icon = null;
        Texture2D texture = ImportTexture(file, enc, checkSA, fullPath);
        texture.Apply();
        if (texture.width >= 32 && texture.height >= 32)  // Minimum valid size
            icon = Sprite.Create(texture, new Rect(new Vector2(0f, 0f), new Vector2(texture.width, texture.height)), new Vector2(0f, 0f));
        return icon;
    }

    /// <summary>Saves a texture to file formated as JPG (StreamingAssets folder is read only)</summary>
    public static void SaveJpgTexture(string name, Texture texture, int quality = 75, bool enc = false, bool fullPath = false)
    {
        Texture2D image = (Texture2D)texture;
        SaveRawFile(name, image.EncodeToJPG(quality), enc, fullPath);
    }

    /// <summary>Saves a texture to file formated as PNG (StreamingAssets folder is read only)</summary>
    public static void SavePngTexture(string name, Texture texture, bool enc = false, bool fullPath = false)
    {
        Texture2D image = (Texture2D)texture;
        SaveRawFile(name, image.EncodeToPNG(), enc, fullPath);
    }

    /// <summary>Adds text lines to an existing file (StreamingAssets folder is read only)</summary>
    public static void AddLogLine(string name, string content, bool deleteDate = false, bool enc = false, bool fullPath = false)
    {
        string savedContent = ReadFile<string>(name, enc, false, fullPath); // Files can't be saved dynamically in StreamingAssets.
        if (savedContent == null)
        {
            if (deleteDate)
                savedContent = content;
            else
                savedContent = System.DateTime.Now.ToString() + " - " + content;
        }
        else
        {
            savedContent += System.Environment.NewLine;
            if (deleteDate)
                savedContent += content;
            else
                savedContent += System.DateTime.Now.ToString() + " - " + content;
        }
        // Save new content:
        SaveFile(name, savedContent, enc, fullPath);
    }

    /// <summary>Adds raw data to an existing file (StreamingAssets folder is read only)</summary>
    public static void AddRawData(string name, byte[] content, bool enc = false, bool fullPath = false)
    {
        byte[] file = ReadRawFile(name, enc, true, fullPath);   // Can read a default file and add raw data, then saves into PersistentData.
        byte[] data = new byte[file.Length + content.Length];
        System.Buffer.BlockCopy(file, 0, data, 0, file.Length); // Add file first.
        System.Buffer.BlockCopy(content, 0, data, file.Length, content.Length); // Add raw data.
        // If requested name doesn't exists, it's created:
        SaveRawFile(name, data, enc, fullPath);
    }

    /// <summary>Checks directory existence (checks PersistentData, then StreamingAssets)</summary>
    public static bool DirectoryExists(string folder, bool checkSA = true, bool fullPath = false)
    {
        // Check existance:
        bool result = false;
        if (!fullPath)
        {
            // Check PersistentData path first:
            string path = Combine(persistentDataPath, folder);
            result = System.IO.Directory.Exists(path);
            if (!result && checkSA)
            {
                // Then check StreamingAssets path:
#if UNITY_ANDROID || UNITY_WEBGL
                result = CheckNameOnIndex("StreamingAssets/" + folder, "D");
#else
                path = Combine(streamingAssetsPath, folder);
                result = FileManagement.DirectoryExists(path);
#endif
            }
        }
        else
        {
            // Direct check:
            result = System.IO.Directory.Exists(folder);
            if (!result && checkSA)
            {
                // Then check StreamingAssets path:
#if UNITY_ANDROID || UNITY_WEBGL
                int StreamingAssetsIndex = folder.IndexOf("StreamingAssets/");
                string FolderNameToCheck = folder.Substring(StreamingAssetsIndex);
                if (FolderNameToCheck[FolderNameToCheck.Length - 1] == '/')
                {
                    FolderNameToCheck = FolderNameToCheck.Substring(0, FolderNameToCheck.Length - 1);
                }
                //Debug.LogError("Checking if directory exists for webgl: " + FolderNameToCheck);
                result = CheckNameOnIndex(FolderNameToCheck, "D");
#else
                path = Combine(streamingAssetsPath, folder);
                result = FileManagement.DirectoryExists(path);
#endif
            }
        }
        return result;
    }

    /// <summary>Create directory (StreamingAssets folder is read only)</summary>
    public static void CreateDirectory(string name, bool fullPath = false)
    {
            if (!fullPath)
                name = Combine(persistentDataPath, name);
            System.IO.Directory.CreateDirectory(name);
#if UNITY_WEBGL
            if(Application.platform == RuntimePlatform.WebGLPlayer)
                SyncFiles();
#endif
    }

    /// <summary>Delete directory and its content (StreamingAssets is read only)</summary>
    public static void DeleteDirectory(string name, bool fullPath = false)
    {
        if (DirectoryExists(name, false, fullPath))
        {
            if (!fullPath)
                name = Combine(persistentDataPath, name);
            System.IO.Directory.Delete(name, true);
#if UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                SyncFiles();
#endif
        }
        else
        {
            Debug.LogWarning("[FileManagement.DeleteDirectory] Can't delete, directory not found: " + name);
        }
    }

    /// <summary>Delete directory content (StreamingAssets is read only)</summary>
    public static void EmptyDirectory(string name = "", bool filesOnly = true, bool fullPath = false)
    {
        if (DirectoryExists(name, false, fullPath))
        {
            // Delete all files:
            string[] files = ListFiles(name, false, fullPath);
            for (int i = 0; i < files.Length; i++)
            {
                string path = Combine(name, files[i]);
                DeleteFile(path, fullPath);
            }
            // Delete all fodlers also (if requested):
            if (!filesOnly)
            {
                string[] folders = ListDirectories(name, false, fullPath);
                for (int i = 0; i < folders.Length; i++)
                {
                    string path = Combine(name, folders[i]);
                    DeleteDirectory(path, fullPath);
                }
            }

#if UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                SyncFiles();
#endif
        }
        else
        {
            Debug.LogWarning("[FileManagement.EmptyDirectory] Can't delete folder content, folder not found: " + name);
        }
    }

    /// <summary>List directory content (files only)</summary>
    public static string[] ListFiles(string folder, bool checkSA = true, bool fullPath = false)
    {
        // Check existance:
        string[] result = null;
        try
        {
            if (!fullPath)
            {
                // Check PersistentData path first:
                string path = Combine(persistentDataPath, folder);
                path = NormalizePath(path);
                if (DirectoryExists(path, false, true))
                {
                    result = System.IO.Directory.GetFiles(path);
                    result = FilterPathNames(result);
                }
                // Then check StreamingAssets path:
                if (checkSA)
                {
#if UNITY_ANDROID || UNITY_WEBGL
                    if(DirectoryExists(folder))
                    {
                        if (result != null)
                        {
                            string[] temp = GetNamesOnIndex("StreamingAssets/" + folder + "/", "F");
                            temp = FilterPathNames(temp);
                            result = result.Union(temp).ToArray();
                        }
                        else
                        {
                            result = GetNamesOnIndex("StreamingAssets/" + folder + "/", "F");
                            result = FilterPathNames(result);
                        }
                    }
#else
                    path = Combine(streamingAssetsPath, folder);
                    path = NormalizePath(path);
                    if (DirectoryExists(path, false, true))
                    {
                        if (result != null)
                        {
                            string[] temp = System.IO.Directory.GetFiles(path);
                            temp = FilterPathNames(temp);
                            result = result.Union(temp).ToArray();

                        }
                        else
                        {
                            result = System.IO.Directory.GetFiles(path);
                            result = FilterPathNames(result);
                        }
                    }
#endif
                }
            }
            else
            {
#if UNITY_ANDROID || UNITY_WEBGL
                // Checks directly:
                if (checkSA)
                {
                    if (DirectoryExists(folder, checkSA, fullPath))
                    {
                        int StreamingAssetsIndex = folder.IndexOf("StreamingAssets/");
                        string FolderNameToCheck = folder.Substring(StreamingAssetsIndex);
                        /*if (FolderNameToCheck[FolderNameToCheck.Length - 1] == '/')
                        {
                            FolderNameToCheck = FolderNameToCheck.Substring(0, FolderNameToCheck.Length - 1);
                        }*/
                        //Debug.LogError("Listing Files in folder for webgl: " + FolderNameToCheck);
                        if (result != null)
                        {
                            string[] temp = GetNamesOnIndex(FolderNameToCheck, "F");
                            temp = FilterPathNames(temp);
                            result = result.Union(temp).ToArray();
                        }
                        else
                        {
                            result = GetNamesOnIndex(FolderNameToCheck, "F");
                            result = FilterPathNames(result);
                        }
                    }
                }
#else
                if (DirectoryExists(folder, checkSA, fullPath))
                {
                    result = System.IO.Directory.GetFiles(folder);
                    result = FilterPathNames(result);
                }
#endif
            }
            SortPathNames(result);
            
            // Error message:
            if (result == null)
                Debug.LogWarning("[FileManagement.ListFiles] Can't read folder content, folder not found: " + folder);

            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[FileManagement.ListFiles] Error: " + e.Message);
            return result;
        }
    }
    /// <summary>List directory content (files only) filtering by extension</summary>
    public static string[] ListFiles(string folder, string[] filter, bool checkSA = true, bool fullPath = false)
    {
        string[] files = ListFiles(folder, checkSA, fullPath);
        if(filter.Length > 0 && files != null)
        {
            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
            // Check every file in the list:
            for (int i = 0; i < files.Length; i++)
            {
                // Check file extensions:
                for (int f = 0; f < filter.Length; f++)
                {
                    if (filter[f] == "" || filter[f].ToLower() == GetFileExtension(files[i]).ToLower())
                    {
                        result.Add(files[i]);
                        break;
                    }
                }
            }
            return result.ToArray();
        }
        return files;
    }

    /// <summary>List directory content (folders only)</summary>
    public static string[] ListDirectories(string folder, bool checkSA = true, bool fullPath = false)
    {
        // Check existance:
        string[] result = null;
        try
        {
            if (!fullPath)
            {
                // Check PersistentData path first:
                string path = Combine(persistentDataPath, folder);
                if (DirectoryExists(path, false, true))
                {
                    result = System.IO.Directory.GetDirectories(path);
                    result = FilterPathNames(result);
                }
                // Then check StreamingAssets path:
                if (checkSA)
                {
#if UNITY_ANDROID || UNITY_WEBGL
                if (DirectoryExists(folder))
                {
                    if (result != null)
                    {
                        string[] temp = GetNamesOnIndex("StreamingAssets/" + folder + "/", "D");    // Search for subfolders.
                        temp = FilterPathNames(temp);
                        result = result.Union(temp).ToArray();
                    }
                    else
                    {
                        result = GetNamesOnIndex("StreamingAssets/" + folder + "/", "D");    // Search for subfolders.
                        result = FilterPathNames(result);
                    }
                }
#else
                    path = Combine(streamingAssetsPath, folder);
                    if (DirectoryExists(path, false, true))
                    {
                        if (result != null)
                        {
                            string[] temp = System.IO.Directory.GetDirectories(path);
                            temp = FilterPathNames(temp);
                            result = result.Union(temp).ToArray();
                        }
                        else
                        {
                            result = System.IO.Directory.GetDirectories(path);
                            result = FilterPathNames(result);
                        }
                    }
#endif
                }
            }
            else
            {
                // Checks directly:
                if (DirectoryExists(folder, false, true))
                {
                    result = System.IO.Directory.GetDirectories(folder);
                    result = FilterPathNames(result);
                }
            }
            SortPathNames(result);
            // Error message:
            if (result == null)
                Debug.LogWarning("[FileManagement.ListDirectories] Can't read folder content, folder not found: " + folder);
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[FileManagement.ListDirectories] Error: " + e.Message);
            return result;
        }
    }

    /// <summary>Gets all files into a directory as a list of byte arrays</summary>
    public static System.Collections.Generic.List<byte[]> ReadDirectoryContent(string folder, bool enc = false, bool checkSA = true, bool fullPath = false)
    {
        System.Collections.Generic.List<byte[]> content = null;
        if (DirectoryExists(folder, checkSA, fullPath))
        {
            content = new System.Collections.Generic.List<byte[]>();
            string[] list = ListFiles(folder, checkSA, fullPath);
            for(int l = 0; l < list.Length; l++)
            {
                string path = Combine(folder, list[l]);
                byte[] buffer = ReadRawFile(path, enc, checkSA, fullPath);
                content.Add(buffer);
            }
        }
        else
        {
            Debug.LogWarning("[FileManagement.ReadDirectoryContent] Can't read folder content, folder not found: " + folder);
        }
        return content;
    }
    
    /// <summary>Copies a file (not directories)</summary>
    public static void CopyFile(string source, string dest, bool checkSA = true, bool fullPathSource = false, bool fullPathDest = false)
    {
        if (source != "" && dest != "")
        {
            source = NormalizePath(source);
            dest = NormalizePath(dest);
            if (FileExists(source, checkSA, fullPathSource)) // If source exists on persistent data, full path or StreamingAssets.
            {
                byte[] file = ReadRawFile(source, false, checkSA, fullPathSource);
                SaveRawFile(dest, file, false, fullPathDest);
            }
            else
            {
                Debug.LogWarning("[FileManagement.CopyFile] Source file not found: " + source);
            }
        }
    }
    
    /// <summary>Copies a directory with all its content recursively</summary>
    public static void CopyDirectory(string source, string dest, bool checkSA = true, bool fullPathSource = false, bool fullPathDest = false)
    {
        if (source != "" && dest != "")
        {
            source = NormalizePath(source);
            dest = NormalizePath(dest);
            if (DirectoryExists(source, checkSA, fullPathSource)) // If source exists on persistent data, full path or StreamingAssets.
            {
                string[] files = ListFiles(source, checkSA, fullPathSource);            // Get the list of files.
                string[] folders = ListDirectories(source, checkSA, fullPathSource);    // Get the list of folders.
                // Copy every file:
                for(int f = 0; f < files.Length; f++)
                {
                    string pathS = NormalizePath(Combine(source, files[f]));    // Source file path
                    string pathD = NormalizePath(Combine(dest, files[f]));      // Destination file path
                    CopyFile(pathS, pathD, checkSA, fullPathSource, fullPathDest);
                }
                // Iterate on every folder:
                for(int f = 0; f < folders.Length; f++)
                {
                    string pathS = NormalizePath(Combine(source, folders[f]));  // Source folder path.
                    string pathD = NormalizePath(Combine(dest, folders[f]));    // Destination folder path.
                    CopyDirectory(pathS, pathD, checkSA, fullPathSource, fullPathDest);
                }
                CreateDirectory(dest);
            }
            else
            {
                Debug.LogWarning("[FileManagement.CopyDirectory] Source path not found: " + source);
            }
        }
    }

    /// <summary>Moves files or directories</summary>
    public static void Move(string source, string dest, bool fullPathSource = false, bool fullPathDest = false)
    {
        if (source != "" && dest != "")
        {
            // If exists in persistent data or fullPath, then moves (StreamingAssets not allowed):
            if (FileExists(source, false, fullPathSource) || DirectoryExists(source, false, fullPathSource))
            {
                if (!fullPathSource)
                    source = Combine(persistentDataPath, source);
                if (!fullPathDest)
                    dest = Combine(persistentDataPath, dest);
                CreateDirectory(GetParentDirectory(dest));
                System.IO.File.Move(source, dest);
            }
            else
                Debug.LogWarning("[FileManagement.Move] Source file not found: " + source);
        }
    }
    /// <summary>Renames files or directories</summary>
    public static void Rename(string source, string dest, bool fullPathSource = false, bool fullPathDest = false)
    {
        // If exists in persistent data or fullPath, then moves (StreamingAssets not allowed):
        if (FileExists(source, false, fullPathSource) || DirectoryExists(source, false, fullPathSource))
            Move(source, dest, fullPathSource, fullPathDest);
        else
            Debug.LogWarning("[FileManagement.Rename] Source file not found: " + source);
    }

    /// <summary>Gets the parent directory of a file/folder</summary>
    public static string GetParentDirectory(string path)
    {
        path = NormalizePath(path);
        int slash = path.LastIndexOf('/');
        if (slash >= 0)
        {
            if (path == System.IO.Path.GetPathRoot(path))
                path = "";
            else
                path = path.Substring(0, slash);
        }
        else
            path = "";
        return NormalizePath(path);
    }

    /// <summary>Combines both paths into a single path correctly</summary>
    public static string Combine(string path1, string path2)
    {
        return System.IO.Path.Combine(path1, path2);
    }

    /// <summary>Normalizes a path name</summary>
    public static string NormalizePath(string path)
    {
        path = path.Replace('\\', '/');                         // Uses slashes only.
        path = path.Replace("//", "/");                         // Replaces double slashes.
        if (path.Length >= 1 && path[path.Length - 1] == '/')   // Delete the last slash (to prevent DirectoryExists from failure)
            path = path.Substring(0, path.Length - 1);
        if (path.Length > 1 && path[path.Length - 1] == ':')    // To prevent failure for "C:/" like paths.
            path += '/';
        return path;
    }

    /// <summary>Retrieves a file/folder name from a path</summary>
    public static string GetFileName(string path)
    {
        path = NormalizePath(path);
        return System.IO.Path.GetFileName(path);
    }

    /// <summary>Gets the file extension (even from a path)</summary>
    public static string GetFileExtension(string path)
    {
        int point = path.LastIndexOf('.');
        string ext = "";
        if (point > 0)
            ext = path.Substring(point);
        return ext;
    }

    /// <summary>Parse strings to requested types</summary>
    private static T CustomParser<T>(string content)
    {
        T val;                          // Return value.
        string[] values = { };          // Separate values to be parsed.
        switch (typeof(T).ToString())
        {
            case "UnityEngine.Vector2":
                content = content.Substring(1, content.Length - 2);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Vector2(float.Parse(values[0]), float.Parse(values[1])), typeof(T));
                break;
            case "UnityEngine.Vector3":
                content = content.Substring(1, content.Length - 2);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])), typeof(T));
                break;
            case "UnityEngine.Vector4":
                content = content.Substring(1, content.Length - 2);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Vector4(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])), typeof(T));
                break;
            case "UnityEngine.Quaternion":
                content = content.Substring(1, content.Length - 2);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])), typeof(T));
                break;
            case "UnityEngine.Rect":
                content = content.Substring(1, content.Length - 2);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Rect(float.Parse(values[0].Split(':')[1]), float.Parse(values[1].Split(':')[1]), float.Parse(values[2].Split(':')[1]), float.Parse(values[3].Split(':')[1])), typeof(T));
                break;
            case "UnityEngine.Color":
                content = content.Substring(5, content.Length - 6);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])), typeof(T));
                break;
            case "UnityEngine.Color32":
                content = content.Substring(5, content.Length - 6);     // Delete parentheses
                values = content.Split(',');
                val = (T)System.Convert.ChangeType(new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3])), typeof(T));
                break;
            default:
                val = (T)System.Convert.ChangeType(content, typeof(T));
                break;
        }
        return val;
    }

    /// <summary>Deletes the path strings and leaves the file/directory names</summary>
    private static string[] FilterPathNames(string[] names)
    {
        if (names != null)
        {
            for (int r = 0; r < names.Length; r++)
            {
                names[r] = System.IO.Path.GetFileName(names[r]);
            }
        }
        return names;
    }

    /// <summary>Sorts the names alphabetically</summary>
    private static string[] SortPathNames(string[] names)
    {
        if (names != null)
        {
            System.Array.Sort(names);
        }
        return names;
    }

    /// <summary>Encryption call</summary>
    private static byte[] Encrypt(byte[] data, byte[] key)
    {
#if USE_AES
        return AesEncrypt(data, key);           // Aes encryption.
#else
        return XorEncryptDecrypt(data, key);    // Xor encryption.
#endif
    }
    /// <summary>Decryption call</summary>
    private static byte[] Decrypt(byte[] data, byte[] key)
    {
#if USE_AES
        return AesDecrypt(data, key);           // Aes decryption.
#else
        return XorEncryptDecrypt(data, key);   // Xor decryption.
#endif
    }

#if USE_AES
    /// <summary>AES initialization</summary>
    private static readonly AesManaged aes;
    /// <summary>Class constructor</summary>
    static FileManagement()
    {
        aes = new AesManaged { Key = key };
        aes.BlockSize = key.Length * 8;     // Key size expresed in bits (16 * 8 = 128).
    }
    /// <summary>AES encryption</summary>
    private static byte[] AesEncrypt(byte[] data, byte[] key)
    {
        using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, key))
        {
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
    /// <summary>AES decryption</summary>
    private static byte[] AesDecrypt(byte[] data, byte[] key)
    {
        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, key))
        {
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
#else
    /// <summary>Basic xor encryption/decryption algorithm</summary>
    private static byte[] XorEncryptDecrypt(byte[] data, byte[] key)
    {
        int d = 0;
        int k = 0;
        byte[] output = new byte[data.Length];

        while (d < data.Length)
        {
            while (k < key.Length && d < data.Length)
            {
                output[d] = (byte)(data[d] ^ key[k]);
                d++;
                k++;
            }
            k = 0;
        }
        return output;
    }
#endif

    /********************************
        PlayerPrefs compatibility:
    ********************************/
    /// <summary>Deletes all files into PersistentData folder</summary>
    public static void DeleteAll()
    {
        EmptyDirectory();
    }
    /// <summary>Deletes a key</summary>
    public static void DeleteKey(string key)
    {
        DeleteFile(key);
    }
    /// <summary>Reads a float value from disk</summary>
    public static float GetFloat(string key, float defaultValue = 0.0F)
    {
        float retVal = defaultValue;
        if (FileExists(key))
            retVal = ReadFile<float>(key);
        return retVal;
    }
    /// <summary>Reads an int value from disk</summary>
    public static int GetInt(string key, int defaultValue = 0)
    {
        int retVal = defaultValue;
        if (FileExists(key))
            retVal = ReadFile<int>(key);
        return retVal;
    }
    /// <summary>Reads a string from disk</summary>
    public static string GetString(string key, string defaultValue = "")
    {
        string retVal = defaultValue;
        if (FileExists(key))
            retVal = ReadFile<string>(key);
        return retVal;
    }
    /// <summary>Checks the existence of a key</summary>
    public static bool HasKey(string key)
    {
        return FileExists(key);
    }
    /// <summary>This has no efect (PlayerPrefs compatibility)</summary>
    public static void Save()
    {
        Debug.Log("[FileManagement.Save] This method has no effect, data is already saved.");
    }
    /// <summary>Saves a float value to disk</summary>
    public static void SetFloat(string key, float value)
    {
        SaveFile(key, value);
    }
    /// <summary>Saves an int value to disk</summary>
    public static void SetInt(string key, int value)
    {
        SaveFile(key, value);
    }
    /// <summary>Saves a string to disk</summary>
    public static void SetString(string key, string value)
    {
        SaveFile(key, value);
    }
    // Non standard interfaces:
    // ------------------------
    /// <summary>Reads a bool value from disk</summary>
    public static bool GetBool(string key, bool defaultValue = false)
    {
        bool retVal = defaultValue;
        if (FileExists(key))
            retVal = ReadFile<bool>(key);
        return retVal;
    }
    /// <summary>Reads a double value from disk</summary>
    public static double GetDouble(string key, double defaultValue = 0)
    {
        double retVal = defaultValue;
        if (FileExists(key))
            retVal = ReadFile<double>(key);
        return retVal;
    }
    /// <summary>Saves a bool value to disk</summary>
    public static void SetBool(string key, bool value)
    {
        SaveFile(key, value);
    }
    /// <summary>Saves a double value to disk</summary>
    public static void SetDouble(string key, double value)
    {
        SaveFile(key, value);
    }

    /********************************
        Experimental functionality:
    ********************************/

    /// <summary>Open the work folder in system explorer (not widely supported)</summary>
#if UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS || UNITY_WINRT
    public static void OpenFolder(string path = "", bool fullPath = false)
    {
        Debug.LogError("[FileManagement.OpenFolder] Not supported in this platform.");
    }
#else
    public static void OpenFolder(string path = "", bool fullPath = false)
    {
        if (!fullPath)
            path = persistentDataPath + "/" + path;
        System.Diagnostics.Process.Start(path);
    }
#endif

#if UNITY_WINRT
    /// <summary>Serialize object (works with default & user defined types only)</summary>
    public static byte[] ObjectToByteArray(object obj)
    {
        Debug.LogError("[FileManagement.ObjectToByteArray] Not supported in this platform.");
        return null;
    }
    /// <summary>Deseerialize object (works with default & user defined types only)</summary>
    public static object ByteArrayToObject(byte[] arrBytes)
    {
        Debug.LogError("[FileManagement.ByteArrayToObject] Not supported in this platform.");
        return null;
    }
#else
    /// <summary>Serialize object (works with default & user defined types only)</summary>
    public static byte[] ObjectToByteArray(object obj)
    {
        if (obj == null) return null;
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        bf.Serialize(ms, obj);
        return ms.ToArray();
    }
    /// <summary>Deseerialize object (works with default & user defined types only)</summary>
    public static object ByteArrayToObject(byte[] arrBytes)
    {
        System.IO.MemoryStream memStream = new System.IO.MemoryStream();
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        memStream.Write(arrBytes, 0, arrBytes.Length);
        memStream.Seek(0, System.IO.SeekOrigin.Begin);
        return bf.Deserialize(memStream);
    }
#endif

    /// <summary>List logic drives</summary>
#if UNITY_WINRT
    public static string[] ListLogicDrives()
    {
        Debug.LogError("[FileManagement.ListLogicDrives] Not supported in this platform.");
        return null;
    }
#else
    public static string[] ListLogicDrives()
    {
        return System.IO.Directory.GetLogicalDrives();
    }
#endif

}
