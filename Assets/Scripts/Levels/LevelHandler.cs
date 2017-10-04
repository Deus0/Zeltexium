using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Voxels;

namespace Zeltex
{
    /// <summary>
    /// Handles levels
    /// </summary>
    [ExecuteInEditMode]
    public class LevelHandler : MonoBehaviour
    {
        public Level MyLevel;
        public EditorAction ActionSpawn = new EditorAction();
        public EditorAction ActionDeSpawn = new EditorAction();
        public EditorAction ActionSaveChunks = new EditorAction();
        public EditorAction ActionSaveCharacters = new EditorAction();
        public bool IsForceSaveAll;

        private void Update()
        {
            if (MyLevel != null)
            {
                if (ActionSaveChunks.IsTriggered())
                {
                    Debug.Log("Saving Level: " + MyLevel.Name);
                    MyLevel.SaveOpenChunks(IsForceSaveAll);
                }
                if (ActionSaveCharacters.IsTriggered())
                {
                    Debug.Log("Saving Level: " + MyLevel.Name);
                    MyLevel.SaveOpenCharacters(IsForceSaveAll);
                }
            }
        }
    }

}