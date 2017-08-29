using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex;
using Zeltex.Voxels;

namespace Zeltex.Tests
{
    public class SerializeableDictionaryTest : MonoBehaviour
    {
        [SerializeField()]
        private ChunkDictionary TestChunkDictionary = new ChunkDictionary();

        [SerializeField()]
        private ElementDictionary TestElementDictionary = new ElementDictionary();

        [SerializeField()]
        private StringDictionary TestDictionary = new StringDictionary();

        [System.Serializable()]
        public class StringDictionary : SerializableDictionaryBase<string, string>
        {

        }
    }

}