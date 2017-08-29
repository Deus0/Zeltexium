using UnityEngine;
using UnityEngine.UI;

namespace NobleMuffins.TurboSlicer.Examples.Oranges
{
    public class PseudoGraphic : MaskableGraphic
    {
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return true;
        }

        public override void Rebuild(CanvasUpdate update)
        {
            //Do nothing
        }
    }

}