using UnityEngine;
using System.Collections;

public class DataManager : MonoBehaviour {

	public float m_score = 0.0f;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
