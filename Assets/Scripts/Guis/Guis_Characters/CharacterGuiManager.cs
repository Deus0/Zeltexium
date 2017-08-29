using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;
using UnityEngine.Networking;

namespace Zeltex.Guis.Characters
{
	/*[System.Serializable]
	public class CharacterGuiPool
	{
		public string Name;
		private List<ZelGui> PooledCharacterGuis = new List<ZelGui>();
        public GameObject MyObject;

		public CharacterGuiPool(GameObject MyObject2)
		{
            MyObject = MyObject2;
		}

		public void Add(ZelGui MyZelGui)
		{
			if (MyZelGui != null)
			{
				MyZelGui.name = "ZelGui_" + Random.Range(1, 100000);
				MyZelGui.transform.SetParent(MyObject.transform);
                MyZelGui.transform.localPosition = Vector3.zero;
				MyZelGui.gameObject.SetActive(false);
				PooledCharacterGuis.Add(MyZelGui);
			}
		}

		public ZelGui Get()
		{
			if (PooledCharacterGuis.Count > 0)
			{
				ZelGui MyZelGui = PooledCharacterGuis[0];
				PooledCharacterGuis.RemoveAt(0);
				MyZelGui.gameObject.SetActive(true);
				return MyZelGui;
			}
			else
			{
				//Debug.LogError("No Character Guis Left.");
				return null;
			}
		}
	}*/

	/// <summary>
	/// Pools the guis character
	/// </summary>
	public class CharacterGuiManager : PoolBase<ZelGui>
    {

        public new static CharacterGuiManager Get()
        {
            if (MyManager == null)
            {
                GameObject ManagerObject = GameObject.Find("GuiPool");
                if (ManagerObject)
                {
                    MyManager = ManagerObject.GetComponent<CharacterGuiManager>();
                }
                else
                {
                    Debug.LogError("Could not find CharacterGuiManager [GuiPool].");
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
                PoolObject.gameObject.SetActive(false);
                if (Data.ExtraData != null)
                {
                    Data.ExtraData.gameObject.GetComponent<Character>().MyGuis.AttachGui(PoolObject);
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