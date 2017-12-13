using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Util;

namespace Zeltex
{
    /// <summary>
    /// Manages games layers
    /// Various scripts connect to this
    /// </summary>
    public class LayerManager : ManagerBase<LayerManager>
    {
        [Header("Layers")]

        [SerializeField]
        private LayerMask CharacterLayer;
        [SerializeField]
        private LayerMask CharacterInteractLayer;

        [SerializeField]
        private LayerMask SkeletonLayer;
        [SerializeField]
        private LayerMask ItemsLayer;
        // raycasts in characters movement uses this
        [SerializeField]
        public LayerMask WorldsLayer;
        [SerializeField]
        private LayerMask BulletsLayer;
        public LayerMask ViewerLayer;
        [SerializeField]
        private LayerMask WaypointsLayer;
        [SerializeField]
        private LayerMask GuiLayer;
        [SerializeField]
        private LayerMask MainCameraMask;

        public static bool AreLayersEqual(LayerMask MyLayer, int GameObjectLayer)
        {
            return (MyLayer.value == 1 << GameObjectLayer);
        }

        public static LayerManager Get()
        {
            if (MyManager == null)
            {
                GameObject LayerManagerObject = GameObject.Find("LayerManager");
                if (LayerManagerObject)
                {
                    MyManager = LayerManagerObject.GetComponent<LayerManager>();
                }
                else
                {
                    Debug.LogError("Could not find [LayerManager].");
                }
            }
            return MyManager;
        }

        // Use this for initialization
        void Start()
        {
            Camera.main.cullingMask = MainCameraMask;
        }

        public LayerMask GetInteractLayer()
        {
            return CharacterInteractLayer;
        }

        public LayerMask GetWorldsLayer()
        {
            return WorldsLayer;
        }

        public LayerMask GetWaypointLayer()
        {
            return WaypointsLayer;
        }

        public LayerMask GetSkeletonLayer()
        {
            return SkeletonLayer;
        }

        public LayerMask GetItemsLayer()
        {
            return ItemsLayer;
        }

        public void SetLayerWorld(GameObject MyObject)
        {
            MyObject.SetLayerRecursive(WorldsLayer);
        }

        public void SetLayerGui(GameObject MyObject)
        {
            MyObject.SetLayerRecursive(GuiLayer);
        }

        public void SetLayerCharacter(GameObject MyObject)
        {
            MyObject.SetLayerRecursive(CharacterLayer);
        }

        public void SetLayerSkeleton(GameObject MyObject)
        {
            MyObject.SetLayerRecursive(SkeletonLayer);
        }

        public void SetLayerBullet(GameObject MyObject)
        {
            MyObject.SetLayerRecursive(BulletsLayer);
        }
    }
}
