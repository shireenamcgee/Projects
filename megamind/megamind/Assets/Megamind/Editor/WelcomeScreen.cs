using UnityEngine;
using System.Collections;
using UnityEditor;

[InitializeOnLoad]
// https://docs.unity3d.com/ScriptReference/EditorWindow.html
public class WelcomeScreen : EditorWindow {

	static string gameName = "MegaMind";
	static string assetNumber = "http://u3d.as/gsj";
	static string publisherShortlink = "http://u3d.as/aGq";

	[MenuItem("Window/GemMine/MegaMind")]
	public static void ShowWindow() {
		// get an instance to the editor window
		WelcomeScreen wscreen = (WelcomeScreen)EditorWindow.GetWindow(typeof(WelcomeScreen),false);
		// set the window title
		GUIContent titleContent = new GUIContent ("GemMine Media presents: " + gameName);
		wscreen.titleContent = titleContent;
		// contrain the window size
		Vector2 maxSize = new Vector2 (500, 700);
		wscreen.maxSize = maxSize;
		wscreen.minSize = maxSize;
		// show the window
		wscreen.Show ();
	}	

	GUIStyle myBanner;

	bool showToggle;

	// http://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html
	static WelcomeScreen() {
		EditorApplication.update += startUp;
	}


	public static void startUp() {
		EditorApplication.update -= startUp;

		if (!PlayerPrefs.HasKey ("ShowWelcome")) {
			EditorApplication.update += ShowWindow;
		}
	}

	public void OnGUI() {

		myBanner = new GUIStyle ();
		myBanner.normal.background = (Texture2D)Resources.Load ("GemMineBanner");
		GUI.Box (new Rect (0, 0, 500, 60), "", myBanner);
		GUILayoutUtility.GetRect (position.width, 64);
		GUILayout.Space (40);
		GUILayout.BeginVertical ();

		if(createEntry ("documentation", "DOCUMENTATION", "Read the documentation delivered with " + gameName)){
			Application.OpenURL (Application.dataPath + "//" + gameName + "//Documentation//" + gameName + ".pdf");
		}
		if (createEntry ("rating", "RATING & REVIEW", "Help us making this asset better and leave a review and rating")) {
			Application.OpenURL (assetNumber);
		}
		if (createEntry ("assets", "MORE UNITY ASSETS", "Interested in more of our assets? Visit our Asset Store page")) {
			Application.OpenURL (publisherShortlink);
		}
		if (createEntry ("likeus", "SHARING IS CARING", "Follow us on Facebook and like our page")) {
			Application.OpenURL ("https://www.facebook.com/GemMine-312633058899256");
		}
		if (createEntry ("visitus", "THERE IS NO PLACE LIKE HOME", "Visit us on our homepage")) {
			Application.OpenURL ("http://www.gemmine.de");
		}
		if (createEntry ("support", "HELP", "Any questions? Do not hesitate to contact us")) {
			Application.OpenURL ("mailto:office@gemmine.de");
		}
			
		GUILayout.EndVertical (); 

		GUILayout.BeginHorizontal();
		GUILayout.Space (30);
		showToggle = EditorGUILayout.Toggle ("Never show this again", showToggle);
		GUILayout.Space (30);
		GUILayout.EndHorizontal ();

		if (GUI.Button (new Rect (30, 640, 140, 32), "Close this window")) {
			if (showToggle) {
				PlayerPrefs.SetInt ("ShowWelcome", 1);
			}
			EditorApplication.update -= ShowWindow;
			//PlayerPrefs.Save ();
			Close ();
		}
	}


	// 
	// public void createEntry
	//
	// create an entry for the WelcomeScreen

	public bool createEntry(string icon, string title, string description) {
		GUILayout.BeginHorizontal();
		GUILayout.Space (30);
		GUILayout.Box ((Texture)Resources.Load (icon), GUIStyle.none,GUILayout.MaxHeight(64),GUILayout.MaxWidth(64));
		GUILayout.Space (10);

		GUILayout.BeginVertical ();
		GUILayout.Space (10);
		GUILayout.Label (title, EditorStyles.boldLabel);
		GUILayout.Label (description);
		GUILayout.EndVertical ();

		GUILayout.EndHorizontal();

		var entryRect = GUILayoutUtility.GetLastRect ();
		EditorGUIUtility.AddCursorRect (entryRect, MouseCursor.Link);

		GUILayout.Space (20);

		// check for an click inside the entry to fire up an action
		// http://answers.unity3d.com/questions/21261/can-i-place-a-link-such-as-a-href-into-the-guilabe.html
		if (Event.current.type == EventType.MouseUp && entryRect.Contains (Event.current.mousePosition))
			return true;

		return false;
	}
}
