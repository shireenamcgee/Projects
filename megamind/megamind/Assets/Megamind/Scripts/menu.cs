using UnityEngine;
using System.Collections;

// menu
//
// this script is attached to the MenuManager GameObject in the menu scene
// and controls the menu behaviour

public class menu : MonoBehaviour {


	// Use this for initialization
	void Start () {
		if (!PlayerPrefs.HasKey ("sameColor")) {
			PlayerPrefs.SetInt ("sameColor", 0);
		}
		if (!PlayerPrefs.HasKey ("colorCount")) {
			PlayerPrefs.SetInt ("colorCount", 8);
		}

	}
	
	// Update is called once per frame
	void Update () {
		// the escape key is equivalent to android's back button
		if (Input.GetKey(KeyCode.Escape)) { 
			Application.Quit();
		}
	}


    // Here begins the UI Eventhandling

    #region UI Eventhandling

    //
    // public void playClicked()
    //
    // This method gets called if the player clicks the play button
    // The megaMind level will be loaded
    //

    public void playClicked() {
        megaMind.GameProgress.Write(new megaMind.EventRecord() { EventName = "PLAY", });
        
		Application.LoadLevel ("megaMind");
	}



	//
	// public void exitClicked()
	//
	// This method gets called if the player clicks the exit button
	// The the application terminates
	//

	public void exitClicked() {
        
        megaMind.GameProgress.Write(new megaMind.EventRecord() { EventName="Exit"});
		Application.Quit ();
	}



	//
	// public void rulesClicked()
	//
	// This method gets called if the player clicks the rules button
	// The the application loads the rules level
	//

	public void rulesClicked() {
       
       megaMind.GameProgress. Write(new megaMind.EventRecord() { EventName = "Rules" });
        Application.LoadLevel ("rules");
	}



	//
	// public void settingsClicked()
	//
	// This method gets called if the player clicks the settings button
	// The the application loads the settings level
	//


	public void settingsClicked() {
        megaMind.GameProgress.Write(new megaMind.EventRecord() { EventName = "Settings" });
        Application.LoadLevel ("settings");
	}

	#endregion UI Eventhandling
}
