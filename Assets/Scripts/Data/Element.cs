using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using System;

namespace Zeltex
{

    [Serializable]
    public class ElementEvent : UnityEvent<Element> { }

    /// <summary>
    /// Parent class for all data objects
    /// </summary>
    [Serializable]
    public class Element : System.Object
    {
        [Tooltip("A unique identifier for the Element")]
        public string Name = "Empty";

        // File Management
        [JsonIgnore, HideInInspector]
        [Tooltip("Set to true when the element has been changed from the saved file")]
        public bool HasChanged = false;
        // Hidden
        [JsonIgnore]
        private string OldName = "";
        [JsonIgnore]
        private bool HasMoved = false;

        [Header("Events")]
        [JsonIgnore, HideInInspector]
        [Tooltip("When the file has modified - Update any connected UI to indicate a file change")]
        public ElementEvent ModifiedEvent = new ElementEvent();
        [JsonIgnore, HideInInspector]
        [Tooltip("When the file has saved - Update any connected UI to indicate a file change")]
        public ElementEvent SavedEvent = new ElementEvent();
        [JsonIgnore, HideInInspector]
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change")]
        public ElementEvent RenamedEvent = new ElementEvent();
        [JsonIgnore, HideInInspector]
        [Tooltip("When the file has been renamed - Update any connected UI to indicate a file change")]
        public ElementEvent MovedEvent = new ElementEvent();
        [JsonIgnore, HideInInspector]
        public ElementFolder MyFolder;
        [JsonIgnore]
        private Type DataType = typeof(Element);
        [JsonIgnore, HideInInspector]
        public bool IsDrawGui;
#if UNITY_EDITOR
        [JsonIgnore, HideInInspector]
        public bool IsDefaultGui;
#endif
        [JsonIgnore, HideInInspector]
        public Element ParentElement;
        // A link to the elements position in the database
        [JsonIgnore, HideInInspector]
        public string ElementLink = "";

        public Type GetDataType()
        {
            DataType = DataFolderNames.GetDataType(MyFolder.FolderName);
            return DataType;
        }

        public void ResetName()
        {
            OldName = Name;
        }

        public string GetFolder()
        {
            if (MyFolder != null)
            {
                return MyFolder.FolderName;
            }
            else
            {
                return "";
            }
        }

        public virtual string GetFileExtention()
        {
            if (MyFolder != null)
            {
                return MyFolder.FileExtension;
            }
            else
            {
                return "txt";
            }
        }

        /// <summary>
        /// Gets the element name
        /// </summary>
        public string GetName()
        {
            return Name;
        }
        /// <summary>
        /// Sets a new name for the element
        /// </summary>
        public void SetName(string NewName)
        {
            if (Name != NewName)
            {
                Name = NewName;
                HasMoved = true;
                RenamedEvent.Invoke(this);
                OnModified();
            }
        }

        public void SetNameOfClone(string NewName)
        {
            Name = NewName;
            OldName = NewName;
        }

        /// <summary>
        /// Whether the element needs to save or not
        /// </summary>
        public bool CanSave()
        {
            return HasChanged;
        }

        /// <summary>
        /// Marks the element as dirty
        /// </summary>
        public void OnModified()
        {
            if (ParentElement != null)
            {
                ParentElement.OnModified();
            }
            else
            {
                if (!HasChanged)
                {
                    HasChanged = true;
                    ModifiedEvent.Invoke(this);
                    if (MyFolder != null)
                    {
						MyFolder.OnModified();
                    }
                }
            }
        }

        private string GetFullFilePath()
        {
            return DataManager.GetFolderPath(GetFolder() + "/") +
                       Name + "." + GetFileExtention();
        }

        /// <summary>
        /// Uses JSON to Srialize the object
        /// </summary>
        public string GetSerial()
        {
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            return JsonConvert.SerializeObject(this, MySettings);
        }

        public void Save(bool IsForce = false)
		{
			if (OldName != Name)
			{
				Rename(OldName, Name);
				OldName = Name;
			}
            if (CanSave() || IsForce)
            {
                Util.FileUtil.Save(GetFullFilePath(), GetSerial());
                OnSaved();
            }
        }

        public void Delete()
        {
            if (MyFolder != null)
            {
                if (MyFolder.Remove(Name))
                {
                    Util.FileUtil.Delete(GetFullFilePath());
                }
            }
        }


        /// <summary>
        /// Returns the new name
        ///     - If the old name and new name is the same, return
        ///     - While the name is in the database, incremenet a number on it
        /// </summary>
        public void Rename(string OldName, string NewName)
		{
			if (OldName == NewName)
			{
				Name = NewName;
				return;
			}
			if (MyFolder == null) 
			{
				Name = NewName;
				return;
			}
			string OriginalName = NewName;
			int NameTryCount = 1;
			// Search for a new name
			while (MyFolder.Data.ContainsKey(NewName))
			{
				NameTryCount++;
				NewName = OriginalName + " " + NameTryCount;
				if (NameTryCount >= 100000)
				{
					Name = NewName;
					return;
				}
			}
			// Cannot rename, have to remove and re add!
			if (MyFolder.Data.ContainsKey(NewName) == false)
			{
				string OldFileName = MyFolder.GetFolderPath() + OldName + "." + MyFolder.FileExtension;
				string NewFileName = MyFolder.GetFolderPath() + NewName + "." + MyFolder.FileExtension;
				// Delete file if exists
				if (System.IO.File.Exists(OldFileName))
				{
					Debug.LogError("Moving file: " + OldFileName + " to " + NewFileName);
					System.IO.File.Move(OldFileName, NewFileName);
				}

				//T MyValue = MyFolder.Data[OldName];
				MyFolder.Data.Remove(OldName);
				MyFolder.Data.Add(NewName, this);
				Name = NewName;
				return;
			}
			else
			{
				Name = OldName;
			}
		}

        /// <summary>
        /// Called when element is saved
        /// Make sure to move file here if renamed
        /// </summary>
        public void OnSaved()
        {
            if (HasChanged)
            {
                HasChanged = false;
                OldName = Name;
                if (HasMoved)
                {
                    HasMoved = false;
                    // Move File Here
                    MovedEvent.Invoke(this);
                    /*if (MyFolder != null)
                    {
                        MyFolder.MovedEvent.Invoke(this);
                    }*/
                }
				SavedEvent.Invoke(this);    // finish modifying
				if (MyFolder != null)
				{
					MyFolder.OnSaved();
				}
            }
        }

        public bool CanMove()
        {
            return HasMoved;
        }

        public void OnMoved()
        {
            HasMoved = false;
        }

		#region Loading

		public T Clone<T>() where T : Element
        {
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            T NewElement = JsonConvert.DeserializeObject(GetSerial(), typeof(T), MySettings) as T;
            NewElement.OnLoad();
            return NewElement;
		}

		public Element Clone(System.Type DataType)
        {
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            Element NewElement = JsonConvert.DeserializeObject(GetSerial(), DataType, MySettings) as Element;
            NewElement.OnLoad();
            return NewElement;
		}
		public Element Clone()
        {
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            Element NewElement = JsonConvert.DeserializeObject(GetSerial(), GetType(), MySettings) as Element;   //GetDataType()
            NewElement.OnLoad();
            return NewElement;
		}

		public Element Load()
		{
			string Script = Util.FileUtil.Load(GetFullFilePath());
			return Load(Script);
		}

		public Element Load(string Script, System.Type DataType)
        {
            Element MyElement;
            try
            {
                JsonSerializerSettings MySettings = new JsonSerializerSettings();
                MySettings.Formatting = DataManager.Get().GetFormat();
                MyElement = JsonConvert.DeserializeObject(Script, DataType, MySettings) as Element;
            }
            catch (System.ArgumentException e) 
            {
                Debug.LogError("Element.Load: " + e.ToString());
                MyElement = null;
            }
            if (MyElement != null)
            {
                MyElement.OnLoad();
            }
            return MyElement;
		}


		public Element Load(string Script)  //, bool IsJSONFormat = true
        {
			Element NewElement;
			DataType = DataFolderNames.GetDataType(MyFolder.FolderName);
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            NewElement = JsonConvert.DeserializeObject(Script, DataType, MySettings) as Element;
			NewElement.Name = Name;
			NewElement.MyFolder = MyFolder;
            OnLoad();
            return NewElement;
        }

        public Element Revert()
        {
            if (HasChanged)
            {
                if (OldName != "")
                {
                    Name = OldName;
                }
                string Script = Util.FileUtil.Load(GetFullFilePath());
                Debug.Log("Reverting element " + Name + " with script:\n" + Script);
                DataType = DataFolderNames.GetDataType(MyFolder.FolderName);
                JsonSerializerSettings MySettings = new JsonSerializerSettings();
                MySettings.Formatting = DataManager.Get().GetFormat();
                Element NewElement = JsonConvert.DeserializeObject(Script, DataType, MySettings) as Element;
                NewElement.Name = Name;
                NewElement.MyFolder = MyFolder;
                OnLoad();
                return NewElement;
            }
            else
            {
                return this;
            }
        }

        public static Element Load(string NewName, ElementFolder NewFolder, string Script)
        {
            Element NewElement = new Element();
            if (Script == null)
            {
                Debug.LogError("Error loading " + NewName + " as null script loaded.");
                return NewElement;
            }
            Type DataType = DataFolderNames.GetDataType(NewFolder.FolderName);
            JsonSerializerSettings MySettings = new JsonSerializerSettings();
            MySettings.Formatting = DataManager.Get().GetFormat();
            NewElement = JsonConvert.DeserializeObject(Script, DataType, MySettings) as Element;
            if (NewElement != null)
            {
                NewElement.Name = NewName;
                NewElement.MyFolder = NewFolder;
                NewElement.OldName = NewElement.Name;
                NewElement.OnLoad();
            }
            return NewElement;
        }

        /// <summary>
        /// When element is loaded, initiate variables
        /// </summary>
        public virtual void OnLoad()
        {

        }
        #endregion

		#region WorldSpawning

		public virtual void Spawn()
		{
            Debug.Log(Name + " Has no Spawn implemented.");
		}

		public virtual void DeSpawn()
        {
            Debug.Log(Name + " Has no Spawn implemented.");
        }

		public virtual bool HasSpawned()
		{
			return false;
		}
		#endregion
    }
}