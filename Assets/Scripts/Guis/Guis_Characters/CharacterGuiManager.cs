using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;
using UnityEngine.Networking;

namespace Zeltex.Guis.Characters
{
	/// <summary>
	/// Pools the guis character
	/// </summary>
	public class CharacterGuiManager : PoolBase<ZelGui>
    {

        public new static CharacterGuiManager Get()
        {
            if (MyManager == null)
            {
                MyManager = ManagersManager.Get().GetManager<CharacterGuiManager>(ManagerNames.CharacterGuiManager);
            }
            return MyManager as CharacterGuiManager;
        }

        /// <summary>
        /// When readying the guis for characters, make sure to attach it to the characters
        /// </summary>
        public override void ReadyObject(ReadyMessageData Data)
        {
            base.ReadyObject(Data);
            ZelGui PoolObject = Data.SpawnedObject.gameObject.GetComponent<ZelGui>();
            if (LogManager.Get())
            {
                LogManager.Get().Log("ReadyObjectFinal: " + PoolObject.name + " -to -" + Data.ExtraData.name, "ReadyObject");
            }
            if (PoolObject)
            {
                if (Data.ExtraData != null)
                {
                    Debug.LogError(Data.ExtraData.name + " is attaching Gui after spawning.");
                    Data.ExtraData.gameObject.GetComponent<Character>().GetGuis().AttachGui(PoolObject);
                }
                else
                {
                    Debug.LogError("Gui has no character.");
                }
            }
        }

        #region SerializePools
        [SerializeField]
        protected new List<GuiPool> MyPools = new List<GuiPool>();

        public override List<SpawnedPool<ZelGui>> Pools
        {
            get
            {
                base.MyPools.Clear();
                for (int i = 0; i < MyPools.Count; i++)
                {
                    base.MyPools.Add(MyPools[i] as SpawnedPool<ZelGui>);
                }
                return base.MyPools;
            }
            set
            {
                base.MyPools.Clear();
                for (int i = 0; i < value.Count; i++)
                {
                    base.MyPools.Add(value[i] as GuiPool);
                }
            }
        }

        protected override void CreatePoolObject()
        {
            //Debug.LogError("Creating Pool.");
            MyPools.Add(new GuiPool());
        }

        [System.Serializable]
        public class GuiPool : SpawnedPool<ZelGui>
        {

        }

        public override void ClearPools()
        {
            Debug.Log("Clearing pools in " + name);
            for (int i = 0; i < MyPools.Count; i++)
            {
                MyPools[i].ClearPool();
            }
            MyPools.Clear();
            IsPoolsSpawned = false;
        }
        #endregion
    }
}