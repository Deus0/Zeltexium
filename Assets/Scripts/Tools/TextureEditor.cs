using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ZeltexTools;

namespace Zeltex.Guis.Maker
{

    /// <summary>
    /// Texture Editing. To use, add onto a raw image component.
    /// To Do:
    ///     - add in chunking so updates are more efficient for larger sizes
    /// </summary>
    public class TextureEditor : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        public GameObject MySelectObject;
        public ColourPicker PrimaryColorPicker;
        public int BrushType = 0;
        [Header("Settings")]
        public Vector2 MyBrushSize = new Vector2(1, 1);
        public Color32 ClearColor = new Color32(255, 255, 255, 0);
        private FilterMode MyFilterMode = FilterMode.Point;
        // colours
        private Color32 MyColor = new Color32(255, 255, 255, 255);
        private Color32 MySecondaryColor = new Color32(255, 255, 255, 255);
        // Inputs
        private bool CanDragPaint = true;
        private bool IsButtonDown = false;
        // positioning
        public Vector2 MyScale = new Vector2(1, 1); // used to resize brush too
        private Vector2 LastPaintedPosition;
        public Vector2 CurrentPaintPosition;
        // used for undo
        private int MaxInstructins = 20;
        private List<PaintInstruction> PreviousPaintInstructions = new List<PaintInstruction>();

        public Dropdown MyBrushTypeDropdown;
        bool IsLineBegin = false;
        Vector2 LineBeginPosition;
        public GameObject LineBeginObject;
        bool IsDragging = false;
        #endregion

        private void RefreshScale()
        {
            Vector2 TextureSize = new Vector2(GetTexture().width, GetTexture().height);
            Vector2 MySize = GetComponent<RectTransform>().GetSize();
            MyScale = new Vector2(TextureSize.x / MySize.x, TextureSize.y / MySize.y);
        }

        #region Monobehaviour
        void Update()
        {
            if (GetTexture() != null)
            {
                UpdatePaintPosition();
                HandlePaintInput();
            }
        }

        public void Disable()
        {
            GetComponent<RawImage>().color = new Color32(255, 255, 255, 0);
            GetComponent<RawImage>().texture = null;
        }
        public void Enable(Texture2D MyTexture)
        {
            GetComponent<RawImage>().color = Color.white;
            GetComponent<RawImage>().texture = MyTexture;
        }
        #endregion

        #region GettersAndSetters
        /// <summary>
        /// Gets the texture used by the editor
        /// </summary>
        public Texture2D GetTexture()
        {
            return GetComponent<RawImage>().texture as Texture2D;
        }
        public void UpdatePrimaryColor(Color32 NewColor)
        {
            MyColor = NewColor;
            if (MySelectObject)
            {
                MySelectObject.GetComponent<RawImage>().color = MyColor;
                LineBeginObject.GetComponent<RawImage>().color = MyColor;
            }
            if (PrimaryColorPicker != null)
            {
                PrimaryColorPicker.SetColor(MyColor);
            }
        }

        public void UpdateSecondaryColor(Color32 NewColor)
        {
            MySecondaryColor = NewColor;
        }

        public Color32 GetMainColor() { return MyColor; }

        public Color32 GetSecondaryColor() { return MySecondaryColor; }

        public Color32[] GetPixelColors()
        {
            return GetTexture().GetPixels32(0);
        }
        public int GetWidth()
        {
            return GetTexture().width;
        }
        public int GetHeight()
        {
            return GetTexture().height;
        }
        #endregion

        #region Input
        /// <summary>
        /// Handles all the texture editor input
        /// </summary>
        private void HandlePaintInput()
        {
            if (MySelectObject.activeSelf)
            {
                if (CanDragPaint)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        IsDragging = true;
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        IsDragging = false;
                    }
                }

                if (BrushType == 0)
                {
                    if ((!CanDragPaint && Input.GetMouseButtonDown(0)) || (CanDragPaint && IsDragging))
                    {
                        NewInstruction("Paint");
                        PaintOnNetwork();
                    }
                }
                else if (BrushType == 1)
                {
                    if ((!CanDragPaint && Input.GetMouseButtonDown(0)) || (CanDragPaint && Input.GetMouseButton(0)))
                    {
                        NewInstruction("Erase");
                        EraseOnNetwork();
                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if (BrushType == 2)
                    {
                        DrawLine(CurrentPaintPosition);
                    }
                    else if (BrushType == 3)
                    {
                        Fill(CurrentPaintPosition);
                    }
                    else if (BrushType == 4)
                    {
                        PickColor(CurrentPaintPosition);
                        BrushType = 0;
                        MyBrushTypeDropdown.value = BrushType;
                        IsDragging = false;
                    }
                }
            }
            else
            {
                IsDragging = false;
            }
        }

        /// <summary>
        /// When maximized, update the brush size
        /// </summary>
        public void OnMaximized()
        {
            RefreshScale();
            if (MySelectObject)
            {
                MySelectObject.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(MyBrushSize.x / MyScale.x, MyBrushSize.y / MyScale.y);
            }
        }

        /// <summary>
        /// Changes the size of the brush
        /// </summary>
	    public void ChangeSize(Vector2 NewSize)
        {
            MyBrushSize = NewSize;
            if (MySelectObject)
            {
                MySelectObject.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(MyBrushSize.x / MyScale.x, MyBrushSize.y / MyScale.y);
            }
        }
        /// <summary>
        /// increase paint brush size
        /// </summary>
        public void IncreaseSize(int SizeAddition)
        {
            IncreaseSize(new Vector2(SizeAddition, SizeAddition));
        }
        /// <summary>
        /// decrease paint brush size
        /// </summary>
        public void IncreaseSize(Vector2 SizeAddition)
        {
            ChangeSize(MyBrushSize + SizeAddition);
        }
        #endregion

        #region PaintInstructions

        /// <summary>
        /// Clears the textures pixels
        /// </summary>
        public void Clear()
        {
            if (GetTexture() != null && IsButtonDown == false)
            {
                NewInstruction("Clear");
                Color[] MyColorData = GetTexture().GetPixels(0);
                for (int i = 0; i < MyColorData.Length; i++)
                {

                    MyColorData[i] = ClearColor;
                }
                GetTexture().SetPixels(MyColorData, 0);
                GetTexture().Apply(false);
            }
        }
        #endregion

        #region Positioning
        private void SetBrushPosition(GameObject MyBrush)
        {
            if (MyBrush)
            {
                MyBrush.SetActive(true);
                Vector2 NewPosition = CurrentPaintPosition;//GetScaledDrawPosition();  // get texture position
                NewPosition.x /= MyScale.x; // convert it back to the rect
                NewPosition.y /= MyScale.y; // convert it back to the rect
                MyBrush.GetComponent<RectTransform>().anchoredPosition = NewPosition;// the paint brush
            }
        }
        /// <summary>
        ///  gets the texture position rather then the rect position
        /// </summary>
        /// <returns></returns>
        public Vector2 GetScaledDrawPosition()
        {
            Vector2 DrawPosition = CurrentPaintPosition;
		    DrawPosition.x = Mathf.FloorToInt (DrawPosition.x * MyScale.x);
		    DrawPosition.y = Mathf.FloorToInt (DrawPosition.y * MyScale.y);
		    return DrawPosition;
	    }

        bool IsMouseInRect()
        {
            return IsMouseInRect(Input.mousePosition);
        }

        bool IsMouseInRect(Vector2 MousePosition)
        {
            //return IsPositionInRect(gameObject, MousePosition);
            return RectTransformUtility.RectangleContainsScreenPoint(gameObject.GetComponent<RectTransform>(), MousePosition, Camera.main);
        }

        public bool IsPositionInRect(Vector2 Position) 
	    {
            Vector2 RectSize = GetComponent<RectTransform>().GetSize();
		    return ((Position.x >= 0 &&
			    Position.x < (RectSize.x) &&
		    Position.y >= 0 &&
			    Position.y < (RectSize.y)));
        }
        #endregion


        #region TextureIndexing
        public static int GetIndex(Texture2D MyTexture2, Vector2 TexturePosition)
        {
            return GetPixelIndex(Mathf.RoundToInt(TexturePosition.x), Mathf.RoundToInt(TexturePosition.y), MyTexture2.width);
        }
        public static int GetPixelIndex(float i, float j, int Width)
        {
            if (i < 0 || i >= Width || j < 0 || j >= Width)
                return -1;
            return Mathf.FloorToInt(i + j * Width);
        }
        public static bool IsPointWithinBounds(Texture2D MyTexture2, Vector2 TexturePosition)
        {
            return (TexturePosition.x >= 0 && TexturePosition.x < MyTexture2.width && TexturePosition.y >= 0 && TexturePosition.y < MyTexture2.height);
        }
        #endregion

        #region Moved

        /// <summary>
        /// Instructions are used to undo actions
        /// </summary>
        /// <param name="InstructionType"></param>
        public void NewInstruction(string InstructionType)
        {
            //MyTexture = GetComponent<RawImage>().texture as Texture2D;
            if (GetTexture() != null)
            {
                PaintInstruction NewInstruction = new PaintInstruction();
                NewInstruction.SetColors(GetTexture().GetPixels32(0));
                NewInstruction.InstructionType = InstructionType;
                PreviousPaintInstructions.Add(NewInstruction);
                if (PreviousPaintInstructions.Count > MaxInstructins)
                {
                    PreviousPaintInstructions.RemoveAt(0);
                }
            }
        }
        /// <summary>
        /// Undos' previous action
        /// </summary>
        public void Undo()
        {
            if (IsButtonDown)
                return;
            if (PreviousPaintInstructions.Count == 0)
                return;
            // grab previous stuff
            PaintInstruction MyInstruction = PreviousPaintInstructions[PreviousPaintInstructions.Count - 1];
            //PaintPixels(MyInstruction.PreviousColors);
            PreviousPaintInstructions.RemoveAt(PreviousPaintInstructions.Count - 1);
        }

        /// <summary>
        /// Picks a colour
        /// </summary>
        /// <param name="PaintPosition"></param>
        public void PickColor(Vector2 PaintPosition)
        {
            NewInstruction("PickColor");
            //Debug.LogError("Filling position: " + PaintPosition.ToString());
            //PaintPosition = FixPosition(PaintPosition);
            Color[] MyColorData = GetTexture().GetPixels(0);
            int Index = GetPixelIndex(PaintPosition.x, PaintPosition.y, GetTexture().width);
            UpdatePrimaryColor(MyColorData[Index]);
        }
        /// <summary>
        /// Draws a line
        /// </summary>
        public void DrawLine(Vector2 PaintPosition)
        {
            if (!IsLineBegin)
            {
                IsLineBegin = true;
                LineBeginPosition = PaintPosition;
                SetBrushPosition(LineBeginObject);
            }
            else
            {
                LineBeginObject.SetActive(false);
                IsLineBegin = false;
                NewInstruction("DrawLine");
                // Debug.LogError("Filling position: " + PaintPosition.ToString());
                //PaintPosition = FixPosition(PaintPosition);
                Color[] MyColorData = GetTexture().GetPixels(0);
                int Index = GetPixelIndex(PaintPosition.x, PaintPosition.y, GetTexture().width);
                Vector2 Direction = (LineBeginPosition - PaintPosition).normalized;
                for (int i = 0; i <= Vector2.Distance(PaintPosition, LineBeginPosition); i++)
                {
                    int NewIndex = GetPixelIndex(Mathf.RoundToInt(PaintPosition.x + Direction.x * i),
                                                     Mathf.RoundToInt(PaintPosition.y + Direction.y * i),
                                                     GetTexture().width);
                    if (NewIndex >= 0 && NewIndex < MyColorData.Length)
                    {
                        MyColorData[NewIndex] = MyColor;
                    }
                }

                GetTexture().SetPixels(MyColorData, 0);
                GetTexture().Apply(false);
            }
        }

        /// <summary>
        /// Creates a new instruction to fill a selected pixel
        /// </summary>
        public void Fill(Vector2 PaintPosition)
        {
            NewInstruction("Fill");
            //Debug.LogError("Filling position: " + PaintPosition.ToString());
            //PaintPosition = FixPosition(PaintPosition);
            Color[] MyColorData = GetTexture().GetPixels(0);
            int Index = GetPixelIndex(PaintPosition.x, PaintPosition.y, GetTexture().width);
            Color32 ColorToReplace = MyColorData[Index];
            MyColorData = FloodFillPixels(MyColorData, PaintPosition, ColorToReplace, MyColor);
            GetTexture().SetPixels(MyColorData, 0);
            GetTexture().Apply(false);
        }

        /// <summary>
        /// Fills the colour
        /// </summary>
        private Color[] FloodFillPixels(Color[] MyColorData, Vector2 Position, Color32 ColorToReplace, Color32 NewColor)
        {
            int Index = GetPixelIndex(Position.x, Position.y, GetTexture().width);
            if (Index >= 0 && Index < MyColorData.Length)
            {
                if (MyColorData[Index] == ColorToReplace)
                {
                    MyColorData[Index] = NewColor;
                    Vector2 PositionAbove = new Vector2(Position.x, Position.y + 1);
                    Vector2 PositionBelow = new Vector2(Position.x, Position.y - 1);
                    Vector2 PositionLeft = new Vector2(Position.x - 1, Position.y);
                    Vector2 PositionRight = new Vector2(Position.x + 1, Position.y);
                    int IndexAbove = GetPixelIndex(PositionAbove.x, PositionAbove.y, GetTexture().width);
                    int IndexBelow = GetPixelIndex(PositionBelow.x, PositionBelow.y, GetTexture().width);
                    int IndexLeft = GetPixelIndex(PositionLeft.x, PositionLeft.y, GetTexture().width);
                    int IndexRight = GetPixelIndex(PositionRight.x, PositionRight.y, GetTexture().width);
                    MyColorData = FloodFillPixels(MyColorData, PositionAbove, ColorToReplace, MyColor);
                    MyColorData = FloodFillPixels(MyColorData, PositionBelow, ColorToReplace, MyColor);
                    MyColorData = FloodFillPixels(MyColorData, PositionLeft, ColorToReplace, MyColor);
                    MyColorData = FloodFillPixels(MyColorData, PositionRight, ColorToReplace, MyColor);
                }
            }
            return MyColorData;
        }

        public void UpdatePaintPosition()
        {
            // refresh the scale -  scale position by texture size
            Vector2 TextureSize = new Vector2(GetTexture().width, GetTexture().height);
            RefreshScale();
            // Vector2 MySize = GetComponent<RectTransform>().GetSize();
            //MyScale = new Vector2(TextureSize.x / MySize.x, TextureSize.y / MySize.y);
            Vector2 MyInRectPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main,
                out MyInRectPosition))
            {
                //Debug.Log("Mouse Inside rect! " + MyInRectPosition.ToString() +"- scale - " + MyScale.ToString());
                CurrentPaintPosition = MyInRectPosition;
                CurrentPaintPosition.x *= MyScale.x;
                CurrentPaintPosition.y *= MyScale.y;
                CurrentPaintPosition = new Vector2(
                    Mathf.FloorToInt(CurrentPaintPosition.x + TextureSize.x / 2),
                    Mathf.FloorToInt(CurrentPaintPosition.y + TextureSize.y / 2)
                    );
                // Set selected object to true
                if (CurrentPaintPosition.x >= 0 && CurrentPaintPosition.x < TextureSize.x &&
                    CurrentPaintPosition.y >= 0 && CurrentPaintPosition.y < TextureSize.y)
                {
                    SetBrushPosition(MySelectObject);
                }
                else
                {
                    MySelectObject.SetActive(false);
                }
            }
            else
            {
                CurrentPaintPosition = new Vector2(-1, -1);
                MySelectObject.SetActive(false);
            }
        }
        /// <summary>
        /// erases a spot on the network
        /// </summary>
        public void EraseOnNetwork()
        {
            Vector2 TempSize = new Vector2(MyBrushSize.x / transform.localScale.x,
                                           MyBrushSize.y / transform.localScale.y);
            Paint(CurrentPaintPosition,
                         TempSize,
                         ClearColor);
        }
        /// <summary>
        /// Paints a spot on the network
        /// </summary>
        public void PaintOnNetwork()
        {
            //Debug.Log("Painting on network.");
            Vector2 TempSize = new Vector2(MyBrushSize.x / transform.localScale.x,
                                           MyBrushSize.y / transform.localScale.y);
            Paint(CurrentPaintPosition,
                         TempSize,
                         MyColor);
        }
        
        private void Paint(Vector2 Position, Vector2 Size, byte PaintColorR, byte PaintColorG, byte PaintColorB, byte PaintColorA)
        {
            Paint(Position, Size, new Color32(PaintColorR, PaintColorG, PaintColorB, PaintColorA));
        }
        /// <summary>
        /// initializes texture on the raw image
        /// </summary>
        private void CreateTexture(Vector2 TextureSize)
        {
            if (GetTexture() != null)
            {
                GetComponent<RawImage>().texture = new Texture2D(Mathf.FloorToInt(TextureSize.x), Mathf.FloorToInt(TextureSize.y), TextureFormat.RGBA32, false);
                Color[] MyColorData2 = GetTexture().GetPixels(0);
                for (int i = 0; i < MyColorData2.Length; i++)
                {
                    MyColorData2[i] = ClearColor;
                }
                GetTexture().SetPixels(MyColorData2, 0);
                GetTexture().Apply(false);
                GetTexture().filterMode = MyFilterMode;
                GetTexture().wrapMode = TextureWrapMode.Clamp;
                Clear();
            }
        }
        private void Paint(Vector2 Position, Vector2 Size, Color32 PaintColor)
        {
            bool IsBlend = false;
            Vector2 PositionMin = new Vector2(Position.x - Mathf.FloorToInt(Size.x / 2f), Position.y - Mathf.FloorToInt(Size.y / 2f));
            Vector2 PositionMax = new Vector2(Position.x + Mathf.CeilToInt(Size.x / 2f), Position.y + Mathf.CeilToInt(Size.y / 2f));
            Color32[] MyColorData = GetTexture().GetPixels32(0);
            for (float i = PositionMin.x; i < PositionMax.x; i++)
                for (float j = PositionMin.y; j < PositionMax.y; j++)
                {
                    if (i >= 0 && i < GetTexture().width && j >= 0 && j <= GetTexture().height)
                    {
                        float DistanceToMid = Vector2.Distance(new Vector2(i, j), Position);
                        if (DistanceToMid <= Size.x / 2f) // if is within bounds of texture size
                        {
                            //PaintColor.a = (byte)(DistanceToMid/(Size.x/2f));
                            //PaintColor.a = (byte)(255);
                            int Index = GetPixelIndex(i, j, GetTexture().width);
                            if (Index < MyColorData.Length && Index >= 0)
                            {
                                Color32 NewColor = MyColorData[Index];
                                if (IsBlend)
                                {
                                    NewColor.r = (byte)((NewColor.r + PaintColor.r * PaintColor.a) / 2f);
                                    NewColor.g = (byte)((NewColor.g + PaintColor.g * PaintColor.a) / 2f);
                                    NewColor.b = (byte)((NewColor.b + PaintColor.b * PaintColor.a) / 2f);
                                }
                                else
                                {
                                    NewColor = PaintColor;
                                }
                                MyColorData[Index] = NewColor;
                            }
                        }
                    }
                }
            GetTexture().SetPixels32(MyColorData, 0);
            GetTexture().Apply(true);
        }
        #endregion

    }
}




/*public void Paint(Vector2 Position, Vector2 Size, Color32 PaintColor)
{
    Paint(gameObject, Position, Size, PaintColor);
}*/

// Updates the raw imageg texture file based on mouse input
/*public void Paint(GameObject MyPaintObject, Vector2 Position, Vector2 Size, Color32 PaintColor)
{
    // scale position by texture size
    Position = FixPosition(Position);
    LastPaintedPosition = Position;
    PaintPixelPosition(MyPaintObject, Position, Size, PaintColor);
}*/
/*void OnGUI()
{
    if (DebugMode)
    {
        float WIDTH = 1920/2f;
        float HEIGHT = 1080/2f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / WIDTH, Screen.height / HEIGHT, 1));
        if (MyTexture)
        {
            GUILayout.Label("Texture Size [" + MyTexture.width + "," + MyTexture.height + "]");
            GUILayout.Label("Texture Ansio [" + MyTexture.anisoLevel + "]");
            GUILayout.Label("Texture filterMode [" + MyTexture.filterMode.ToString() + "]");
            GUILayout.Label("Texture mipmapCount [" + MyTexture.mipmapCount + "]");
        }
        else
            GUILayout.Label("Texture is null.");

        GUILayout.Label("Texture Scale [" + MyScale.ToString() + "]");

        GUILayout.Label("Brush Size [" + MyBrushSize.ToString() + "]");
        if (MySelectObject)
            GUILayout.Label("Paint Icon Size [" + 
                MySelectObject.GetComponent<RectTransform>().GetWidth() + "," +
                MySelectObject.GetComponent<RectTransform>().GetHeight() + "]");
        else
            GUILayout.Label("Paint Icon is null.");

        for (int i = 0; i < TextureColors.Count; i += 16)
            {
            string MyReds = (i + 1) + " R ";
            for (int j = i; j < i+16; j++)
            {
                MyReds += "]-[" + TextureColors[j].r.ToString();
            }
            GUILayout.Label(MyReds);
        }
        for (int i = 0; i < PaintPositions.Count; i++)
        {
            GUILayout.Label((i + 1) + " Pos [" + PaintPositions[i].ToString() + "]" + " - Size [" + PaintSizes[i].ToString() + "]");
        }
    }
}*/
/* scraps
 * 
		//Debug.LogError("ScreenHeight: " + Screen.height);
		//PositionToDraw.y = Mathf.Abs(PositionToDraw.y-Screen.height);
		//PositionToDraw.x = Mathf.Abs(PositionToDraw.x-Screen.width);

		//PositionToDraw = transform.InverseTransformPoint (PositionToDraw);

		//PositionToDraw -= new Vector2(gameObject.GetComponent<RectTransform>().position.x, gameObject.GetComponent<RectTransform>().position.y);
		
		// now get the scaled size
		Vector3[] MyCorners = new Vector3[4];
		gameObject.GetComponent<RectTransform>().GetWorldCorners (MyCorners);
		Vector2 OldSize = gameObject.GetComponent<RectTransform>().sizeDelta;
		Vector2 ScaledSize = new Vector2(MyCorners[0].x,MyCorners[0].y);
		for (int i = 1; i < 4; i++) {
			if (MyCorners[i].x != ScaledSize.x) {
				if (MyCorners[i].x > ScaledSize.x)
					ScaledSize.x = MyCorners[i].x-ScaledSize.x;
				else
					ScaledSize.x = ScaledSize.x-MyCorners[i].x;
				break;
			}
		}
		for (int i = 1; i < 4; i++) {
			if (MyCorners[i].y != ScaledSize.y) {
				if (MyCorners[i].y > ScaledSize.y)
					ScaledSize.y = MyCorners[i].y-ScaledSize.y;
				else
					ScaledSize.y = ScaledSize.y-MyCorners[i].y;
				break;
			}
		}
		Debug.Log("ScaledSize: " +  ScaledSize.ToString() + " -OldSize: " + OldSize.ToString());

		PositionToDraw += ScaledSize/2f;

		PositionToDraw.y = ScaledSize.y-PositionToDraw.y;	// flip y
		PositionToDraw.x = ScaledSize.x-PositionToDraw.x;	// flip y

		PositionToDraw.x = Mathf.RoundToInt ((PositionToDraw.x/ScaledSize.x)*OldSize.x);
		PositionToDraw.y = Mathf.RoundToInt ((PositionToDraw.y/ScaledSize.y)*OldSize.y);
		//Debug.LogError(PositionToDraw.ToString());
*/

/*if (Input.GetAxis("Mouse ScrollWheel") > 0)
{
    IncreaseSize(new Vector2(5,5));
}
else if (Input.GetAxis("Mouse ScrollWheel") < 0)
{
    IncreaseSize(new Vector2(-5,-5));
}*/

/*if (Input.GetKey(KeyCode.D))
{
    IncreaseColor(1,1,1,0);
} 
else if (Input.GetKey(KeyCode.A))
{
    IncreaseColor(-1,-1,-1,0);
}*/
