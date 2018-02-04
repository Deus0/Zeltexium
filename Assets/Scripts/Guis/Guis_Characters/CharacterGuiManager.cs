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
                GameObject ManagerObject = GameObject.Find(ManagerNames.CharacterGuiManager);
                if (ManagerObject)
                {
                    MyManager = ManagerObject.GetComponent<CharacterGuiManager>();
                }
                else
                {
                    Debug.LogError("Could not find CharacterGuiManager [" + ManagerNames.CharacterGuiManager + "].");
                }
            }
            return MyManager as CharacterGuiManager;
        }

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
                //PoolObject.gameObject.SetActive(false);
                if (Data.ExtraData != null)
                {
                    Data.ExtraData.gameObject.GetComponent<Character>().GetGuis().AttachGui(PoolObject);
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
        #endregion
    }
}