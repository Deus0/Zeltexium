using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using System;

namespace Zeltex
{

    [System.Serializable]
    public class ElementEvent : UnityEvent<Element> { }

    /// <summary>
    /// Parent class for all data objects
    /// </summary>
    [System.Serializable]
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

        public Type GetDataType()
        {
            DataType = DataFolderNames.GetDataType(MyFolder.FolderName);
            return DataType;
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

        /// <summary>
        /// Base for getting script
        /// </summary>
        public virtual string GetScript()
        {
            return "";
        }

        /// <summary>
        /// Base for running script
        /// </summary>
        public virtual void RunScript(string Script)
        {

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
            if (!HasChanged)
            {
                HasChanged = true;
                ModifiedEvent.Invoke(this);
                if (MyFolder != null)
                {
                    MyFolder.ModifiedEvent.Invoke(this);
                }
            }
        }

        private string GetFullFilePath()
        {
            return DataManager.GetFolderPath(GetFolder() + "/") +
                       Name + "." + GetFileExtention();
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
                Element NewElement = JsonConvert.DeserializeObject(Script, DataType) as Element;
                NewElement.Name = Name;
                NewElement.MyFolder = MyFolder;
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
            Type DataType = DataFolderNames.GetDataType(NewFolder.FolderName);
            NewElement = JsonConvert.DeserializeObject(Script, DataType) as Element;
            if (NewElement == null)
            {
                //System.Reflection.ConstructorInfo MyConstructor = DataType.GetConstructor(new Type[] { DataType });
                //dynamic NewElement2 = MyConstructor.Invoke(null);
               // NewElement = NewElement2 as Element;
            }
            if (NewElement != null)
            {
                NewElement.Name = NewName;
                NewElement.MyFolder = NewFolder;
            }
			NewElement.OldName = NewElement.Name;
            return NewElement;
        }

        public string GetSerial()
        {
            return JsonConvert.SerializeObject(this);
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
                    if (MyFolder != null)
                    {
                        MyFolder.MovedEvent.Invoke(this);
                    }
                }
                SavedEvent.Invoke(this);    // finish modifying
                if (MyFolder != null)
                {
                    MyFolder.SavedEvent.Invoke(this);
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
			T NewElement = JsonConvert.DeserializeObject(GetSerial(), typeof(T)) as T;
			return NewElement;
		}

		public Element Clone(System.Type DataType)
		{
			Element NewElement = JsonConvert.DeserializeObject(GetSerial(), DataType) as Element;
			return NewElement;
		}
		public Element Clone()
		{
			Element NewElement = JsonConvert.DeserializeObject(GetSerial(), GetDataType()) as Element;
			return NewElement;
		}

		public Element Load(bool IsJSONFormat = true)
		{
			string Script = Util.FileUtil.Load(GetFullFilePath());
			return Load(Script, IsJSONFormat);
		}

		public Element Load(string Script, System.Type DataType)
		{
			return JsonConvert.DeserializeObject(Script, DataType) as Element;
		}
		public Element Load(string Script, bool IsJSONFormat = true)
		{
			Element NewElement;
			DataType = DataFolderNames.GetDataType(MyFolder.FolderName);
			if (IsJSONFormat)
			{
				NewElement = JsonConvert.DeserializeObject(Script, DataType) as Element;
			}
			else
			{
				#if NET_4_6
				System.Reflection.ConstructorInfo MyConstructor = DataType.GetConstructor(Type.EmptyTypes);
				dynamic NewElement2 = MyConstructor.Invoke(null);
				NewElement = NewElement2 as Element;
				NewElement.RunScript(Script);
				#else
				NewElement = JsonConvert.DeserializeObject(Script, DataType) as Element;
				#endif
			}
			NewElement.Name = Name;
			NewElement.MyFolder = MyFolder;
			return NewElement;
		}
		#endregion
    }
}