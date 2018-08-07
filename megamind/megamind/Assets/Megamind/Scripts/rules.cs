using UnityEngine;
using System.Collections;

// rules
//
// this script is attached to the RulesManager GameObject in the rules scene
// and controls the rules behaviour

public class rules : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
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

	#endregion UI Eventhandling

}
