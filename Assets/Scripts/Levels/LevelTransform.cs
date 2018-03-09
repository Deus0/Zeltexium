using UnityEngine;
using Newtonsoft.Json;
using Zeltex.Voxels;

namespace Zeltex
{
    /// <summary>
    /// Used for data to remember their transform data inside a level and world
    /// </summary>
    [System.Serializable]
    public class LevelTransform : Element
    {
        [Header("Stored")]
        [JsonProperty]
        public string LevelInsideOf = "";
        [JsonProperty]
        public string WorldInsideOf = "";
        [JsonProperty]
        public Int3 InChunkPosition;
        [JsonProperty]
        public Vector3 LevelPosition;
        [JsonProperty]
        public Vector3 LevelRotation;
        [Header("Instantiated")]
        [JsonIgnore]
        public Chunk InChunk;
        [JsonIgnore]
        public World InWorld;
        [JsonIgnore]
        private Transform TargetTransform;
        [JsonIgnore, SerializeField]
        public Level InLevel;

        public void SetLevel(Level NewInLevel)
        {
            InLevel = NewInLevel;
            if (InLevel != null)
            {
                InWorld = InLevel.GetWorld();
            }
        }

        public void AttachComponent(Component MyComponent)
        {
            if (MyComponent == null)
            {
                AttachTransform(null);
            }
            else
            {
                AttachTransform(MyComponent.transform);
            }
        }

        public void AttachTransform(Transform NewTransform)
        {
            if (TargetTransform != NewTransform)
            {
                TargetTransform = NewTransform;
                if (TargetTransform != null)
                {
                    TargetTransform.position = LevelPosition;
                    TargetTransform.eulerAngles = LevelRotation;
                }
                CheckInChunk();
            }
        }

        /// <summary>
        /// Compares transform to cached transform data
        /// </summary>
        public void CheckTransformUpdated()
        {
            if (TargetTransform != null)
            {
                if (LevelPosition != TargetTransform.position)
                {
                    LevelPosition = TargetTransform.position;
                    OnModified();
                }
                if (LevelRotation != TargetTransform.eulerAngles)
                {
                    LevelRotation = TargetTransform.eulerAngles;
                    OnModified();
                }
                CheckInChunk();
            }
        }

        private void CheckInChunk()
        {
            // If chunk position changes
            if (InWorld && TargetTransform)
            {
                Int3 NewChunkPosition = InWorld.GetChunkPosition(TargetTransform);
                if (InChunkPosition != NewChunkPosition)
                {
                    InChunkPosition = NewChunkPosition;
                    Chunk NewChunk = InWorld.GetChunk(InChunkPosition);
                    SetInChunk(NewChunk);
                    OnModified();
                }
            }
            else if (TargetTransform)
            {
                Debug.LogWarning("Character [" + Name + "] spawned without a world.");
            }
        }

        public void SetInChunk(Chunk NewInChunk)
        {
            if (NewInChunk != InChunk)
            {
                // First remove character from old chunk
                if (InChunk)
                {
                    InChunk.RemoveTransform(TargetTransform);
                }
                InChunk = NewInChunk;
                if (NewInChunk)
                {
                    NewInChunk.AddTransform(TargetTransform);
                }
            }
        }

        public Chunk GetInChunk()
        {
            return InChunk;
        }

        public void SetWorld(World NewWorld)
        {
            if (InWorld != NewWorld)
            {
                InWorld = NewWorld;
            }
        }

        public World GetInWorld()
        {
            return InWorld;
        }

        public Int3 GetChunkPosition()
        {
            return InChunkPosition;
        }
    }
}