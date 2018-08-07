using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// settings
//
// this script is attached to the SettingsManager GameObject in the settings scene
// and controls the settings behaviour

public class settings : MonoBehaviour {

	// References to the UI Objects
	// The Game state is stored in Playerprefs
	public Toggle sameColor;

	public Toggle color4;
	public Toggle color6;
	public Toggle color8;


	// Use this for initialization
	void Start () {
		// check if Playerprefs are set. If not, set a default value
		// set the slider button's value
		if (!PlayerPrefs.HasKey ("sameColor")) {
			PlayerPrefs.SetInt ("sameColor", 0);
			sameColor.isOn = false;
		} 
		else {
			if (!PlayerPrefs.HasKey("sameColor"))
				sameColor.isOn = false;
			else
				sameColor.isOn = true;
		}

		// check if Playerprefs are set. If not, set a default value
		// set the three toggle's value
		if (!PlayerPrefs.HasKey ("colorCount")) {
			PlayerPrefs.SetInt ("colorCount", 6);
		}

		if (PlayerPrefs.GetInt ("colorCount") == 4)
			color4.isOn = true;
		if (PlayerPrefs.GetInt ("colorCount") == 6)
			color6.isOn = true;
		if (PlayerPrefs.GetInt ("colorCount") == 8)
			color8.isOn = true;

	}
	
	// Update is called once per frame
	void Update () {
		// the escape key is equivalent to android's back button
		if (Input.GetKey(KeyCode.Escape)) { 
			Application.LoadLevel("menu");
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
		Application.LoadLevel ("megaMind");
	}



	//
	// public void menuClicked()
	//
	// This method gets called if the player clicks the menu button
	// The menu level will be loaded
	//

	public void menuClicked() {
		Application.LoadLevel ("menu");
	}



	//
	// public void sameColorClicked()
	//
	// This method controls the state of the slider button (on/off toggle)
	// It gets called if the Player changes the value in the UI
	//

	public void sameColorClicked() {
		if (sameColor.isOn == true)
			PlayerPrefs.SetInt ("sameColor", 1);
		else
			PlayerPrefs.SetInt ("sameColor", 0);
	}



	//
	// public void colorCountClicked(int value)
	//
	// This method controls the state of the three toggle buttons is changed
	// It gets called if the Player changes the value in the UI
	//

	public void colorCountClicked(int value) {
		PlayerPrefs.SetInt ("colorCount", value);
		Debug.Log ("colorCount: " + value);
	}

	#endregion UI Eventhandling

}
