using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Zeltex.AI
{


    [System.Serializable]
    public class BotBehaviour
    {
        public string Name = "Unknown";
        [SerializeField, HideInInspector]
        protected Transform BotTransform;

        public virtual void Initiate(Transform MyTransform)
        {
            BotTransform = MyTransform;
        }

        public virtual void Begin()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void Update(Bot TargetBot)
        {

        }
    }
}
