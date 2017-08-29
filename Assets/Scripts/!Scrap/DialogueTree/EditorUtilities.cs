

/*public static class EditorUtilities 
{
	static Vector2 MinimumWindowSize = new Vector2(100f, 100f);
	// Utility Functions
	// extends editor window
	public static void DrawCurves(this EditorWindow MyWindow, Rect wr, Rect wr2,Color color)
	{
		Vector3 startPos = new Vector3(wr.x + wr.width, wr.y + 3 + wr.height / 3, 0);
		Vector3 endPos = new Vector3(wr2.x, wr2.y + wr2.height / 2, 0);
		float mnog = Vector3.Distance(startPos,endPos);
		Vector3 startTangent = startPos + Vector3.right * (mnog / 3f) ;
		Vector3 endTangent = endPos + Vector3.left * (mnog / 3f);
		Handles.BeginGUI();
		Handles.DrawBezier(startPos, endPos, startTangent, endTangent,color, null, 3f);
		Handles.EndGUI();
	}

	public static void DrawTiled (Rect rect, Texture tex)
	{
		GUI.BeginGroup(rect);
		{
			int width = Mathf.RoundToInt(rect.width);
			int height = Mathf.RoundToInt(rect.height);
			
			for (int y = 0; y < height; y += tex.height)
			{
				for (int x = 0; x < width; x += tex.width)
				{
					GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
				}
			}
		}
		GUI.EndGroup();
	}

	
	public static bool ResizeWindow(DialogueDataGui SelectedWindow, Event MyCurrentEvent, Rect MyParentWindow, Vector2 MyScrollPosition) 
	{
		if (SelectedWindow == null)
			return false;
		//	if (IsResizing) 
		{
			//if (MyWindows.Count > 0)
			{
				Vector2 InitialMousePosition = MyCurrentEvent.mousePosition;
				Vector2 InEditorWindowPosition = InitialMousePosition;//-position.position;
				Vector2 SelectedWindowPosition = SelectedWindow.MyRect.position-MyScrollPosition;	// gets the proper position of window
				Vector2 DeltaSize = InEditorWindowPosition - SelectedWindowPosition;
				//DeltaPosition.y += 72.5f;	// for window header? idk but it works lol
				
				//Debug.Log("----------===============----------");
				//Debug.Log("Initial Delta Size: " + DeltaSize.ToString());
				DeltaSize.x = Mathf.Clamp(DeltaSize.x, MinimumWindowSize.x, MyParentWindow.width-SelectedWindowPosition.x);
				DeltaSize.y = Mathf.Clamp(DeltaSize.y, MinimumWindowSize.y, MyParentWindow.height-SelectedWindowPosition.y);
				
				//Minimum Size
				SelectedWindow.MyRect.width = Mathf.Lerp(SelectedWindow.MyRect.width, DeltaSize.x, 1f);
				SelectedWindow.MyRect.height = Mathf.Lerp(SelectedWindow.MyRect.height, DeltaSize.y, 1f);
				
				// debugging
				//Debug.ClearDeveloperConsole();
				return true;
			} 
			//else 
			{
				//IsResizing = false;
			}
		}
	}
}
*/