using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZeltexTools
{
    /// <summary>
    /// One is added to record each action by the user
    /// </summary>
    [System.Serializable]
    public class PaintInstruction
    {
        public string InstructionType = "None";
        public Texture2D MyTexture; // which texture was updated
        public Vector2 Position;
        public Color32 MyColor;
        public Color32[] PreviousColors;
        public void SetColors(Color32[] NewColors)
        {
            PreviousColors = new Color32[NewColors.Length];
            for (int i = 0; i < NewColors.Length; i++)
            {
                PreviousColors[i] = NewColors[i];
            }
        }
    }
}