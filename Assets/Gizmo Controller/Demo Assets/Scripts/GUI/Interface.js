public var SelectedObject : Transform;

function OnGUI(){
	GUILayout.BeginArea(Rect(10, Screen.height-45, Screen.width-20, 45));
		GUILayout.BeginHorizontal();
			GUILayout.Label("LMB: Camera Rotate");
			GUILayout.Label("RMB: Camera Pan");
			GUILayout.Label("1: Translate Mode");
			GUILayout.Label("2: Rotate Mode");
			GUILayout.Label("3: Scale Mode");
			GUILayout.Label("S: Enable/Disable Snapping");
		GUILayout.EndHorizontal();
	GUILayout.EndArea();
}//OnGUI