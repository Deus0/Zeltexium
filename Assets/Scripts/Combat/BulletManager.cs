using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zeltex.Combat
{
    /// <summary>
    /// Used to spawn bullets
    /// </summary>
    public class BulletManager : PoolBase<Bullet>
    {


        #region SerializePools
        [SerializeField]
        protected new List<BulletPool> MyPools = new List<BulletPool>();

        public override List<SpawnedPool<Bullet>> Pools
        {
            get
            {
                base.MyPools.Clear();
                for (int i = 0; i < MyPools.Count; i++)
                {
                    base.MyPools.Add(MyPools[i] as SpawnedPool<Bullet>);
                }
                return base.MyPools;
            }
            set
            {
                base.MyPools.Clear();
                for (int i = 0; i < value.Count; i++)
                {
                    base.MyPools.Add(value[i] as BulletPool);
                }
            }
        }

        protected override void CreatePoolObject()
        {
            //Debug.LogError("Creating Pool.");
            MyPools.Add(new BulletPool());
        }

        [System.Serializable]
        public class BulletPool : SpawnedPool<Bullet>
        {

        }

        public new static BulletManager Get() { return MyManager as BulletManager; }
        #endregion
    }
}