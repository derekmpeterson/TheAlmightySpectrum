using UnityEngine;
using System.Collections;

public class SpellController : MonoBehaviour {

	public float m_lifetime = 0.2f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		m_lifetime -= Time.deltaTime;
		if (m_lifetime < 0.0f) {
			Destroy (gameObject);
		}
	}
}
