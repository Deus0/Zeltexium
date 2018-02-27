using UnityEngine;
using UnityEngine.Networking;
using Zeltex.Util;

namespace Zeltex
{
    /// <summary>
    /// Base for a zone
    /// </summary>
    public class Zone : NetworkBehaviour
    {
        protected NetworkIdentity MyNetworkIdentity;
        protected ZoneData Data;

        public ZoneData GetData()
        {
            return Data;
        }

        public void SetData(ZoneData NewData)
        {
            if (Data != NewData)
            {
                Data = NewData;
                // initialize data
            }
        }

        // Use this for initialization
        void Awake()
        {
            //MyMeshRenderer = GetComponent<MeshRenderer>();
            MyNetworkIdentity = GetComponent<NetworkIdentity>();
        }

        protected Vector3 GetRandomPosition()
        {
            Vector3 MySize = transform.lossyScale;
            return transform.position + new Vector3(
                        Random.Range(-MySize.x / 2f, MySize.x / 2f),
                        Random.Range(-MySize.y / 2f, MySize.y / 2f),
                        Random.Range(-MySize.z / 2f, MySize.z / 2f));
            /*Bounds MyBounds = MyMeshRenderer.bounds;
            return transform.position + new Vector3(
                Random.Range(MyBounds.min.x, MyBounds.max.x),
                Random.Range(MyBounds.min.y, MyBounds.max.y),
                Random.Range(MyBounds.min.z, MyBounds.max.z));*/
        }
    }

}