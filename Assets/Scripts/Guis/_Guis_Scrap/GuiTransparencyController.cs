using UnityEngine;
using UnityEngine.UI;
using Zeltex.Util;

namespace GuiSystem
{
    /// <summary>
    /// Maximizes a gui window to the entire screen
    /// Puts to front when doing so.
    /// </summary>
    public class GuiTransparencyController : MonoBehaviour
    {
        public bool IsTransparency = true;
        public RawImage MyImage;
        public int DecreaseRate = 40;

        public void IncreaseTransparency()
        {
            if (IsTransparency)
            {
                int Transparency = Mathf.RoundToInt(255 * MyImage.color.a) - DecreaseRate;
                if (Transparency < DecreaseRate)
                {
                    Transparency = 255;
                }
                SetTransparency(Transparency);
            }
            else
            {
                // Flip Fog
                RenderSettings.fog = !RenderSettings.fog;
            }
        }

        private void SetTransparency(int Transparency)
        {
            Color32 MyColor = MyImage.color;
            MyImage.color = new Color32(
                (byte) (MyColor.r),
                (byte) (MyColor.g),
                (byte) (MyColor.b),
                (byte) (Transparency));
        }
    }
}