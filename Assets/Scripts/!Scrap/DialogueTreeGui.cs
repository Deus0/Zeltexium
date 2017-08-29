

	
/*public class DialogueTreeGui : EditorWindow 
{
	//public static GameObject LinkedGameObject;
	private static string FileName = "";
	private static List<DialogueDataGui> MyWindows = new List<DialogueDataGui>();
	// input handling
	private static bool HasSelectedWindow = false;	// window selection
	private static bool IsResizing = false;		// window resizing
	private static int SelectedWindowIndex = 0;
	// states
	public static bool IsDrawBackground = false;
	public static bool IsRenderLinks = false;

	// positions
	private static Vector2 MyScrollPosition = Vector2.zero;
	private static Texture BackgroundTexture = null;

	// dimensions
	public static Vector2 NewWindowSize =  new Vector2 (160f, 120f);
	private static Rect MyScrollArea = new Rect (0, 0, 3000, 3000);
	private static Vector2 CloseButtonSize = new Vector2 (15, 15);

	// colours
	public static Color MyBackgroundColor =  new Color(25, 85, 200);
	public static Color HeaderButtonColour = new Color (0.75f, 0.75f, 0.75f);
	public static Color HeaderTextColour = new Color (0, 0, 0.05f);
	public static Color WindowBackgroundColour = new Color (0.6f, 0.6f, 0.85f, 1f);
	public static Color WindowLabelColour =  new Color (0.75f, 0.75f, 0.75f);
	public static Color DialogueTextColour = new Color (1f, 1f, 1f);

	[MenuItem ("Marz/DialogueTree")]
	public static void  ShowWindow () {
		//EditorWindow.GetWindow(typeof(MyWindow));
		DialogueTreeGui window  = (DialogueTreeGui)EditorWindow.GetWindow(typeof(DialogueTreeGui), false, "DialogueTree");
	}
	public void OnGUI () 
	{
		HandleInput ();
		//MyCurrentEvent = Event.current;
		if (IsResizing)
			EditorUtilities.ResizeWindow (GetSelectedWindow(), Event.current, position, MyScrollPosition);
		CheckResizeEnd ();
		RenderBackground();
		RenderHeaderButtons ();
		
		MyScrollPosition = EditorGUILayout.BeginScrollView (MyScrollPosition, true, true, GUILayout.Width(position.width),  GUILayout.Height(position.height));
		GUILayout.Label("", GUILayout.Width(MyScrollArea.width), GUILayout.Height(MyScrollArea.height));	// used to stretch the scroll area
		RenderWindows ();
		
		if (IsRenderLinks) 
		{
			RenderLinks();
		}
		EditorGUILayout.EndScrollView();
	}

	public void HandleInput() {
		HandleDeleteWindow ();
	}
	public void CheckResizeEnd() {
		if (Event.current.rawType == EventType.mouseUp) {
			EndResize ();
			//Debug.LogError ("Ending Resize");
		}
	}
	void OnSelectionChange() {
		//selectionIDs = Selection.instanceIDs;
		Selection.activeGameObject.name = 
			FileName = Selection.activeGameObject.name;
	}

	private void HandleDeleteWindow() {
		if ((Event.current.keyCode == KeyCode.Delete) && (Event.current.type == EventType.KeyDown)) {
			if (SelectedWindowIndex != -1) {
				MyWindows.RemoveAt (SelectedWindowIndex);
				SelectedWindowIndex = -1;
			}
		}
	}

	DialogueDataGui GetSelectedWindow() {
		if (SelectedWindowIndex >= 0 && SelectedWindowIndex < MyWindows.Count)
			return MyWindows [SelectedWindowIndex];
		return null;
	}
	public void InitializeResize() {
		if (!IsResizing) {
			IsResizing = true;
		}
	}
	public void EndResize() 
	{
		IsResizing = false;
	}

// ----- RENDERING CODES -----
	public void RenderBackground() {
		// First render a background image
		if (IsDrawBackground) 
		{
			if (BackgroundTexture == null) 
			{
				BackgroundTexture = Resources.Load ("Background") as Texture;
			}
			if (BackgroundTexture != null) {
				Rect MyWindowRect = new Rect (0, 0, position.width, position.height);
				EditorUtilities.DrawTiled (MyWindowRect, BackgroundTexture);
			}
		} else {
			GUI.contentColor = new Color(0,0,0);
			GUI.Label(position, "", "color");
		}
	}
	public void RenderHeaderButtons() {
		GUI.backgroundColor = HeaderButtonColour;
		GUI.contentColor = HeaderTextColour;
		if (GUI.Button (new Rect (0, 0, 100, 20), "SpawnWindow")) {
			DialogueDataGui MyWindow = SpawnNewWindow ();
			MyWindow.MyDialogueLine = new Zeltex.Dialogue.DialogueData();
		}
		//if (GUI.Button (new Rect (100, 0, 100, 20), "Save")) {
			//SpawnNewWindow(new Vector2(Screen.width/2f, Screen.height/2f), MyWindowSize);
		//}
		if (GUI.Button (new Rect (200, 0, 100, 20), "Open")) {
			//SpawnNewWindow(new Vector2(Screen.width/2f, Screen.height/2f), MyWindowSize);
			GenerateWindowsFromSpeech ();
		}
		if (GUI.Button (new Rect (300f, 0, 100, 20), "ClearWindows")) 
		{
			MyWindows.Clear ();
		}
		string MyIsBackgroundLabel = "Background";
		if (!IsDrawBackground)
			MyIsBackgroundLabel = "NoBackground";
		if (GUI.Button (new Rect (500f, 0, 100, 20), MyIsBackgroundLabel)) {
			IsDrawBackground = !IsDrawBackground;
		}
		
		string MyRenderLinks = "Is Links";
		if (!IsRenderLinks)
			MyRenderLinks = "No Links";
		if (GUI.Button (new Rect (600f, 0, 100, 20), MyRenderLinks)) {
			IsRenderLinks = !IsRenderLinks;
		}
		GUI.Label (new Rect(700f, 0, 100, 20), "Resizing: " + IsResizing.ToString());
		FileName = GUI.TextField (new Rect (400f, 0, 100, 20), FileName);
	}

	void RunWindow(int WindowIndex) {
		//GUI.color = Color.white;
		if (!HasSelectedWindow && (Event.current.button == 0) && (Event.current.type == EventType.MouseDown))
		{
			//Event.current.Use();
			SelectedWindowIndex = WindowIndex;
			HasSelectedWindow = true;
			//Debug.Log("Clicking with colour: " + GUI.color.ToString());
		}
		
		GUI.contentColor = DialogueTextColour;
		//GUI.color = DialogueTextColour;
		if (SelectedWindowIndex == WindowIndex) {
			GUI.backgroundColor = new Color(1f,0.8f,0.8f);
		} 
		else 
		{
			GUI.backgroundColor = WindowBackgroundColour;
			GUI.color = WindowBackgroundColour;
		}
		//Debug.LogError("Clicking with colour: " + GUI.color.ToString());
		MyWindows [WindowIndex].Render ();

		Rect MyResizeRect = new Rect (MyWindows [WindowIndex].MyRect.width - CloseButtonSize.x, 
		                              MyWindows [WindowIndex].MyRect.height - CloseButtonSize.y, 
		                              CloseButtonSize.x, 
		                              CloseButtonSize.y);

		GUI.backgroundColor = Color.white;
		GUI.Box (MyResizeRect, "∆", "color");
		if (Event.current.type == EventType.mouseDown && MyResizeRect.Contains(Event.current.mousePosition))  
		{
			InitializeResize ();
		}
		
		Rect MyShrinkButtonRect = new Rect (MyWindows [WindowIndex].MyRect.width - CloseButtonSize.x, 
		                              CloseButtonSize.y, 
		                              CloseButtonSize.x, 
		                                    CloseButtonSize.y);
		if (GUI.Button (MyShrinkButtonRect, "X", "color")) {
			MyWindows[WindowIndex].Shrink();
		}
		GUI.DragWindow(MyScrollArea);
	}

	public bool IsVisible(Rect MyRenderRect) {
		Rect MyVisibleRect = new Rect (MyScrollPosition.x, MyScrollPosition.y, position.width, position.height);
		Vector2 TopLeftCorner = MyRenderRect.position;
		Vector2 BottomRightCorner = MyRenderRect.position+MyRenderRect.size;
		Vector2 TopRightCorner = MyRenderRect.position+new Vector2(MyRenderRect.width, 0);
		Vector2 BottomLeftCorner = MyRenderRect.position+new Vector2(0, MyRenderRect.height);
		return (MyVisibleRect.Contains (TopLeftCorner) || MyVisibleRect.Contains (BottomRightCorner)
			|| MyVisibleRect.Contains (TopRightCorner) || MyVisibleRect.Contains (BottomLeftCorner));
	}
	public void RenderWindows() 
	{
		HasSelectedWindow = false;
		BeginWindows();
		//GUI.color = WindowBackgroundColour;
		GUI.backgroundColor = WindowBackgroundColour;
		GUI.color = WindowLabelColour;
		for (int i = 0; i < MyWindows.Count; i++) 
		{ 
			if (IsVisible(MyWindows[i].MyRect)) {
				if (!IsResizing)
					MyWindows[i].MyRect = GUI.Window (i, MyWindows[i].MyRect, RunWindow,  MyWindows[i].Name);
				else
					GUI.Window (i, MyWindows[i].MyRect, RunWindow,  MyWindows[i].Name);	// don't move it
			}
		}
		EndWindows();
	}

	public void RenderLinks() 
	{
		int NextIndex;
		for (int i = 0; i < MyWindows.Count; i++) 
		{
			//NextIndex = MyWindows [i].MyDialogueLine.IsFirstMyNext;
		}
	}

	// Generation stuff
	public DialogueDataGui SpawnNewWindow() {
		return SpawnNewWindow( "Window " + (MyWindows.Count + 1));
	}
	public DialogueDataGui SpawnNewWindow(string NewTitle) 
	{
		return SpawnNewWindow (NewTitle, new Vector2 (Screen.width / 2f, Screen.height / 2f), NewWindowSize);
	}
	public DialogueDataGui SpawnNewWindow(string NewTitle, Vector2 Position, Vector2 Size) {
		DialogueDataGui NewWindow = new DialogueDataGui ();
		NewWindow.MyRect = new Rect (Position.x-Size.x/2f, Position.y-Size.y/2f, Size.x, Size.y);
		NewWindow.Name = NewTitle;
		MyWindows.Add (NewWindow);
		return NewWindow;
	}
	public void GenerateWindowsFromSpeech() {
		int MaxCountX = 4;
		float MarginX = 1.25f;
		float MarginY = 1.25f;
		GameObject MyTarget = GameObject.Find (FileName);
		if (MyTarget == null) {
			Debug.LogError("Could not find: " + FileName);
			return;
		}
    }
    // arranges the dialogue into a tree
    // then it offsets the x by halfway to position it in horizontal middle (depending on widest tree branches)
    public void AutoArrangeTree() {

	}
}
*/
// scrap code

/*
	void OnEnable ()
	{
		EditorApplication.update += Update;
		Debug.Log ("Loaded DialogueTreeGui");
	}
 GUI.BeginGroup(MyDialogueRect);
		//GUI.Button(new Rect(0,25,100,20),"I am a button");
		GUI.Label (new Rect (Position.x, Position.y+50, 100, 20), "I'm a Label!");
		GUI.EndGroup ();


		//toggleTxt = GUI.Toggle(new Rect(0, 75, 200, 30), toggleTxt, "I am a Toggle button");
		//toolbarInt = GUI.Toolbar (new Rect (0, 110, 250, 25), toolbarInt, toolbarStrings);
		//selGridInt = GUI.SelectionGrid (new Rect (0, 160, 200, 40), selGridInt, selStrings, 2);
		//hSliderValue = GUI.HorizontalSlider (new Rect (0, 210, 100, 30), hSliderValue, 0.0f, 1.0f);
		//hSbarValue = GUI.HorizontalScrollbar (new Rect (0, 230, 100, 30), hSbarValue, 1.0f, 0.0f, 10.0f);
		
 		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
		myString = EditorGUILayout.TextField ("Text Field", myString);
		
		groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
		myBool = EditorGUILayout.Toggle ("Toggle", myBool);
		myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
		EditorGUILayout.EndToggleGroup ();*/