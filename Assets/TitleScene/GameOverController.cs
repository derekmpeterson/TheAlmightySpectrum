using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject.Find ("finalScore").GetComponent<Text> ().text = "Final Score: " + GameObject.Find ("DataManager").GetComponent<DataManager> ().m_score;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Return)) {
			Application.LoadLevel ("InstructionScene");
		}
	}
}
