using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeltex.Characters;

namespace Zeltex
{
    /// <summary>
    /// Extentions for transforms
    /// </summary>
    public static class TransformExtentions
    {
        public static GameObject FindRootFromBone(this Transform MyTransform)
        {
            if (MyTransform.gameObject.tag == "BonePart")
            {
                Character MyCharacter = MyTransform.FindRootCharacter();
                if (MyCharacter)
                {
                    return MyCharacter.gameObject;
                }
            }
            return null;
        }

        public static Character FindRootCharacter(this Transform MyTransform)
        {
            if (MyTransform != null)
            {
                Character MyCharacter = MyTransform.gameObject.GetComponent<Character>();
                if (MyCharacter)
                {
                    return MyCharacter;
                }
                else
                {
                    return MyTransform.parent.FindRootCharacter();
                }
            }
            return null;
        }
    }
}
