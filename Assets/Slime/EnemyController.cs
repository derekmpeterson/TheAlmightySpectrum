using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (CharacterController))]
public class EnemyController : MonoBehaviour {
	
	CharacterController m_characterController;
	float m_nextJump;
	public bool m_jumps = false;
	public bool m_runs = false;
	float m_nextRun;
	public AudioClip m_jumpSound;

	bool m_awake = false;

	void Start() {
		m_characterController = GetComponent<CharacterController> ();
		m_characterController.SetMoveSpeedMultiplier (Random.Range (0.5f, 2.0f));

		if (m_jumps) {
			m_nextJump = Random.Range (0.1f, 5.0f);
		}

		if (m_runs) {
			m_nextRun = Random.Range (2.0f, 40.0f);
		}
	}

	void OnEnable() {

	}
	
	void OnDisable() {
	}
	
	// Update is called once per frame
	void Update() {

		if (m_jumps) {
			m_nextJump -= Time.deltaTime;

			if (m_nextJump <= 0.0f) {
				SoundManager.instance.PlaySingle (m_jumpSound);
				m_characterController.Jump ();
				m_nextJump = Random.Range (0.1f, 5.0f);
			}
		}

		if (m_runs && m_awake) {
			m_nextRun -= Time.deltaTime;

			if (m_nextRun <= 0.0f) {
				m_runs = false;
				StartCoroutine(BecomeRunner());
			}
		}
	}

	public void SetAwake(bool i_awake) {
		m_awake = i_awake;
	}

	IEnumerator BecomeRunner() {
		m_characterController.SetMoveSpeedMultiplier(0.0f);
		yield return new WaitForSeconds (1.0f);
		m_characterController.SetMoveSpeedMultiplier(3.5f);
	}
}
