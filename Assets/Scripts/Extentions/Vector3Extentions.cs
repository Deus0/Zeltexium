using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeltex
{

    public static class Vector3Extentions
    {
        public static Vector3 ApplyRotationMagic(this Vector3 Input)
        {
            Vector3 Output = new Vector3(Input.x, Input.y, Input.z);
            if (Output.x > 180) // if 210, 180-210 = -30! 210 - 360, -150!
            {
                Output.x = Output.x - 360;
            }
            if (Output.y > 180)
            {
                Output.y = Output.y - 360;
            }
            if (Output.z > 180)
            {
                Output.z = Output.z - 360;
            }
            return Output;
        }
    }
}