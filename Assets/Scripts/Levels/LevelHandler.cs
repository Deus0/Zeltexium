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

        public EditorAction ActionSpawn;
        public EditorAction ActionDeSpawn;
        public EditorAction ActionSaveChunks;
        public EditorAction ActionSaveCharacters;

        public bool IsForceSaveAll;

        private void Update()
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