using UnityEngine;
using System.Collections;

public class TitleSceneController : MonoBehaviour {

	public float m_displayTime = 3.0f;
	float m_timer;
	// Use this for initialization
	void Start () {
		m_timer = m_displayTime;
	}
	
	// Update is called once per frame
	void Update () {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0.0f) {
			Application.LoadLevel("InstructionScene");
		}
	}
}
