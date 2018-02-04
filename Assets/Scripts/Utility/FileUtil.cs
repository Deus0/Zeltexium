using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Zeltex.Items;
using Zeltex.Quests;
using Zeltex.Dialogue;
using Zeltex.Characters;
using MakerGuiSystem;
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// Utility functions for Zeltex
/// </summary>
namespace Zeltex.Util
{

    [System.Serializable]
    public enum FilePathType
    {
        Normal,
        PersistentPath,
        StreamingPath
    }
 
    /// <summary>
    /// Util for handling the files, and data packets.
    /// </summary>
    public static class FileUtil
    {
        public static StringBuilder MyStringBuilder = new StringBuilder();

        #region MapDirectories

        public static void Save()
        {
            //PlayerPrefs.GetInt(FileUtil.PersistentPathKey, 1);
        }

        public static bool SetPersistentPath(FilePathType NewState)
        {
            if (DataManager.Get().MyFilePathType != NewState)
            {
                DataManager.Get().MyFilePathType = NewState;
                PlayerPrefs.SetInt(DataManager.Get().PathTypeKey, (int)DataManager.Get().MyFilePathType);
                return true;
            }
            else
            {
                return false;
            }
            /*else if (!NewState && MyFilePathType == FilePathType.PersistentPath)
            {
                MyFilePathType = FilePathType.Normal;
                PlayerPrefs.SetInt(PathTypeKey, (int)MyFilePathType);
                return true;
            }
            else
            {
                return false;
            }*/
        }
#endregion

#region FileUtil

        public static bool DoesFileExist(string FilePath)
        {
            try
            {
                StreamReader MyStreamReader = new StreamReader(FilePath);
                MyStreamReader.Close();
                return true;
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError("FileNotFound: " + e.ToString());
                return false;
            }
        }

        public static void Move(string Path1, string Path2)
        {
            if (File.Exists(Path1))
            {
                Debug.Log("Renaming: " + Path1 + " - As: " + Path2);
                File.Move(Path1, Path2);
                if (File.Exists(Path1 + ".meta")) // delete meta file too from unity
                {
                    File.Move(Path1 + ".meta", Path2 + ".meta");
                }
/*#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("SyncFiles");
#endif*/
            }
        }

        public static void Delete(string Path)
        {
            if (File.Exists(Path))
            {
                Debug.Log("Deleting file: " + Path);
                File.Delete(Path);
                Path += ".meta";
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }
/*#if UNITY_WEBGL && !UNITY_EDITOR
                Application.ExternalCall("SyncFiles");
#endif*/
            }
            else
            {
                Debug.Log("File: " + Path + " Was not deleted. File did not exist.");
            }
        }

        public static string Load(string Path)
        {
/*#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("SyncFiles");
#endif*/
            if (File.Exists(Path))
            {
                return File.ReadAllText(Path);
            }
            else
            {
                return "";
            }
        }

        public static void Save(string Path, string Data)
        {
            File.WriteAllText(Path, Data);
/*#if UNITY_WEBGL
            Application.ExternalCall("SyncFiles");
#endif*/
        }
        public static byte[] LoadBytes(string Path)
        {
/*#if UNITY_WEBGL
            Application.ExternalCall("SyncFiles");
#endif*/
            if (File.Exists(Path))
            {
                return File.ReadAllBytes(Path);
            }
            else
            {
                return new byte[0];
            }
        }

        public static void SaveBytes(string Path, byte[] Data)
        {
            File.WriteAllBytes(Path, Data);
/*#if UNITY_WEBGL
            Application.ExternalCall("SyncFiles");
#endif*/
        }

#endregion
        
#region StringUtil

        /// <summary>
        /// Converts a string to a List<string>
        /// </summary>
        public static List<string> ConvertToList(string MyInput)
        {
            List<string> MyList = new List<string>();
            if (MyInput != null)
            {
                string[] MyArray = MyInput.Split('\n');
                MyList.AddRange(MyArray);
            }
            return MyList;
        }

        /// <summary>
        /// Converts a List<string></string> to a string
        /// </summary>
        public static string ConvertToSingle(string[] MyList)
        {
            if (MyList.Length > 0)
            {
                for (int i = 0; i < MyList.Length - 1; i++)
                {
                    MyStringBuilder.Append(MyList[i]);
                    MyStringBuilder.Append("\n");
                }
                MyStringBuilder.Append(MyList[MyList.Length - 1]);
                string MyString = MyStringBuilder.ToString();
                MyStringBuilder = new StringBuilder();
                return MyString;
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// Converts a List<string></string> to a string
        /// </summary>
        public static string ConvertToSingle(List<string> MyList)
        {
            if (MyList.Count > 0)
            {
                for (int i = 0; i < MyList.Count - 1; i++)
                {
                    MyStringBuilder.Append(MyList[i]);
                    MyStringBuilder.Append("\n");
                }
                MyStringBuilder.Append(MyList[MyList.Count - 1]);
                string MyString = MyStringBuilder.ToString();
                MyStringBuilder = new StringBuilder();
                return MyString;
            }
            else
            {
                return "";
            }
        }
#endregion

#region Other

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            System.Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        
        /// <summary>
        /// Are 2 lists teh same
        /// </summary>
        public static bool AreListsTheSame(List<string> MyNames1, List<string> MyNames2)
        {
            if (MyNames1.Count != MyNames2.Count)
                return false;
            for (int i = 0; i < MyNames1.Count; i++)
                if (MyNames1[i] != MyNames2[i])
                    return false;

            return true;
        }

        public static void CloneDirectory(string DirectoryPath1, string DirectoryPath2, string Extension)
        {
            List<string> FilesOfType = GetFilesOfType(DirectoryPath1, Extension);
            for (int i = 0; i < FilesOfType.Count; i++)
            {
                string FileName = Path.GetFileName(FilesOfType[i]);
                File.Copy(FilesOfType[i], DirectoryPath2 + FileName);
            }
        }

        public static List<string> GetFilesOfType(string FilePath, string Extension)
        {
            List<string> Paths = new List<string>();
            if (Directory.Exists(FilePath))
            {
                string[] MyFiles = Directory.GetFiles(FilePath);
                for (int i = 0; i < MyFiles.Length; i++)
                {
                    if (MyFiles[i].Length >= 4 && MyFiles[i].Substring(MyFiles[i].Length - 4) == "." + Extension)    // if character file
                    {
                        Paths.Add(MyFiles[i]);
                    }
                }
            }
            return Paths;
        }

        public static bool IsEmptyLine(string MyLine)
        {
            bool IsEmpty = true;
            for (int k = 0; k < MyLine.Length; k++)
            {
                if (MyLine[k] != ' ' && MyLine[k] != '\n' && (int)(MyLine[k]) != 13 && MyLine[k] != '\t')
                {
                    return false;
                }
            }
            return IsEmpty;
        }

        public static List<string> SortAlphabetically(List<string> MyList)
        {
            //Debug.LogError("Sorting List: " + MyList.Count);
            MyList.Sort(CompareListByName);
            return MyList;
        }

        private static int CompareListByName(string Input1, string Input2)
        {
            //Debug.Log("Comparing: " + Input1 + " with " + Input2);
            // if they are the same word
            if (Input1 == Input2)
            {
                return 0;
            }
            float LowestLength = Input1.Length;
            if (LowestLength > Input2.Length)
            {
                LowestLength = Input2.Length;
            }
            for (int i = 0; i < LowestLength; i++)
            {
                if (System.Char.IsNumber(Input1[i]) && System.Char.IsNumber(Input2[i]))
                {
                    // if both numbers - check if both numbers until the end
                    bool IsNumbersForTheRest = true;
                    for (int j = i + 1; j < Input1.Length; j++)
                    {
                        if (Input1[j] == '.')
                        {
                            break;
                        }
                        if (System.Char.IsNumber(Input1[j]) == false)
                        {
                            IsNumbersForTheRest = false;
                            break;
                        }
                    }
                    if (IsNumbersForTheRest)
                    {
                        for (int j = i + 1; j < Input2.Length; j++)
                        {
                            if (Input2[j] == '.')
                            {
                                break;
                            }
                            if (System.Char.IsNumber(Input2[j]) == false)
                            {
                                IsNumbersForTheRest = false;
                                break;
                            }
                        }
                    }
                    if (IsNumbersForTheRest)
                    {
                        string NumberOneString = Input1.Substring(i, Input1.Length - i - 4);
                        string NumberTwoString = Input2.Substring(i, Input2.Length - i - 4);
                        int NumberOne = int.Parse(NumberOneString);
                        int NumberTwo = int.Parse(NumberTwoString);
                        /*if (Input1.Contains("VoxelMeta"))
                        {
                            Debug.Log("Comparing " + NumberOneString + " to " + NumberTwoString + "\n[" + Input1 + "], [" + Input2 + "] ");
                        }*/
                        if (NumberOne > NumberTwo)
                        {
                            return 1;
                        }
                        else if (NumberOne < NumberTwo)
                        {
                            return -1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        /*if (Input1.Contains("VoxelMeta"))
                        {
                            Debug.Log("Comparing " + "\n[" + Input1 + "], [" + Input2 + "] ");
                        }*/
                    }
                }
                    // compare normally with characters
                    if (Input1[i] != Input2[i])
                    {
                        if (Input1[i] > Input2[i])
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
            }
            if (Input1.Length > Input2.Length)
            {
                return 1;
            }
            else if (Input1.Length < Input2.Length)
            {
                return -1;
            }
            return 0;
        }
#endregion

#region ImportsExports

        [DllImport("user32.dll")]
        private static extern void SaveFileDialog(); //in your case : OpenFileDialog
        [DllImport("user32.dll")]
        private static extern void OpenFileDialog(); //in your case : OpenFileDialog

        /*public static string Import(string ObjectName, string FunctionName, string FileType)
        {
            Debug.Log("Importing file: " + ObjectName + ":" + FunctionName + " - " + FileType);
#if UNITY_WEBGL
            Application.ExternalCall("Import", ObjectName, FunctionName, FileType);
#elif UNITY_EDITOR
            System.Windows.Forms.OpenFileDialog MyOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            MyOpenFileDialog.Filter = "*." + FileType + "| *." + FileType;
            MyOpenFileDialog.Multiselect = true;
            MyOpenFileDialog.Title = "Import a " + FileType.ToUpper() + " File!";
            if (MyOpenFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string MyFileName in MyOpenFileDialog.FileNames)
                {
                    Debug.Log("Reading: " + MyFileName);
                    string MyText = File.ReadAllText(MyFileName);
                    MyText = MyFileName + "\n" + MyText;
                    Debug.Log(MyText);
                    GameObject.Find(ObjectName).SendMessage(FunctionName, MyText);
                }
            }
#endif
            return "";
        }*/

        /*public static void Export(string Name, string FileExtension, string Data)
        {
            Debug.Log("Exporting file: " + Name);
#if UNITY_WEBGL
            Application.ExternalCall("Export", Name + "." + FileExtension, Data);
#elif UNITY_EDITOR
            System.Windows.Forms.FolderBrowserDialog MyFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            //OpenFileDialog open = new OpenFileDialog();
            //MyFolderDialog.Title = "Export a " + FileExtension.ToUpper() + " File";
            MyFolderDialog.ShowDialog();
            if (MyFolderDialog.SelectedPath != "")
            {
                string MyFileName = MyFolderDialog.SelectedPath + "\\" + Name + "." + FileExtension; // Path.GetDirectoryName(MySaveFileDialog.FileName)
                Debug.Log("Saving as: " + MyFileName);
                File.WriteAllText(MyFileName, Data);
            }
#endif
        }*/

       /* public static void ExportImage(string Name, string FileExtension, string Data)
        {
            Debug.Log("Exporting file: " + Name);
#if UNITY_WEBGL
            Application.ExternalCall("ExportImage", Name + "." + FileExtension, Data);
#elif UNITY_EDITOR
            System.Windows.Forms.SaveFileDialog MySaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            MySaveFileDialog.Filter = "*." + FileExtension + "| *." + FileExtension;
            //OpenFileDialog open = new OpenFileDialog();
            MySaveFileDialog.Title = "Export a Texture";
            MySaveFileDialog.ShowDialog();
            if (MySaveFileDialog.FileName != "")
            {
                byte[] MyBytes = System.Convert.FromBase64String(Data);
                FileUtil.SaveBytes(MySaveFileDialog.FileName, MyBytes);
            }
#endif
        }*/
        #endregion


        public static void OpenPathInWindows(string path)
        {
            Debug.Log("Opening path [" + path + "]");
            bool openInsidesOfFolder = false;

            // try windows
            string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

            if (FileManagement.DirectoryExists(winPath, true, true)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

    }

    /// <summary>
    /// Used to convert scripts to a single string (\n)
    /// </summary>
    public class CoroutineWithData
    {
        public Coroutine coroutine { get; private set; }
        public object result;
        private IEnumerator target;
        public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }
        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }
}