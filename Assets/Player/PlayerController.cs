using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {

	CharacterController m_characterController;
	public int m_score = 0;
	public int m_multiplier = 1;
	public float m_changeDirTimeout = 2.0f;
	bool m_canChangeDir = true;
	public AudioClip m_jumpSound;
	public AudioClip m_healSound;
	public AudioClip m_hurtSound;
	public GameObject m_spellObject;

	private int m_playerID;

	void Start() {
		m_characterController = GetComponent<CharacterController> ();
	}

	void OnEnable() {
		HealthController.DoDamageEvent += OnDamageEvent;
		CollisionController.DoCollisionEvent += OnCollisionEvent;
	}

	void OnDisable() {
		HealthController.DoDamageEvent -= OnDamageEvent;
		CollisionController.DoCollisionEvent -= OnCollisionEvent;
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			SoundManager.instance.PlaySingle (m_jumpSound);
			m_characterController.Jump();
		}
		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
			if (m_canChangeDir) {
				m_canChangeDir = false;
				m_characterController.Bounce ();
			}
		}
		HealthController pHealthController = GetComponent <HealthController> ();
		if (pHealthController) {
			float pDiff = pHealthController.m_maxHealth - pHealthController.GetHealth();
			if (pDiff > 6.0f) {
				float pChange = 6.0f - pDiff;
				pHealthController.ChangeHealth(pChange, null);
			}
		}
	}

	public void ChangeScore(int i_amount, bool i_useMultiplier = true) {
		int pAmount = (i_useMultiplier) ? i_amount * m_multiplier : i_amount;
		m_score += pAmount;
		GameObject.Find ("score").GetComponent<Text> ().text = "Score: "+ m_score;
	}

	public void SetMultiplier(int i_multiplier) {
		m_multiplier = i_multiplier;
	}

	public void OnDamageEvent(GameObject i_victim, GameObject i_attacker, float i_damage) {
		if (i_attacker == gameObject && i_damage < 0.0f) {
			ChangeScore ((int) (1.0f * Mathf.Abs(i_victim.transform.localScale.x)));
			if (!i_victim.GetComponent<HealthController>().m_colored) {
				if (m_multiplier == 1) {
					SetMultiplier(2);
				} else {
					SetMultiplier(m_multiplier + 2);
				}
				SoundManager.instance.PlaySingle (m_healSound);
				Animator pAnimator = GetComponent<Animator> ();
				if (pAnimator) {
					pAnimator.SetTrigger ("spell");
				}
				Vector2 pSpellPos = transform.position;
				pSpellPos.y -= 0.16f;
				Instantiate (m_spellObject, pSpellPos, Quaternion.identity);
			}
			CharacterController pCharacterController = GetComponent<CharacterController> ();
			if (pCharacterController && pCharacterController.m_runDir != Vector2.zero) {
				ScreenShakeController pScreenShakeController = Camera.main.GetComponent<ScreenShakeController> ();
				if (pScreenShakeController) {
					float pBaseShake = 0.005f;
					float pShake = pBaseShake * Mathf.Abs (i_victim.transform.localScale.x);
					pScreenShakeController.StartShake (pShake);
				}
			}
		}
		if (i_victim == gameObject) {
			SoundManager.instance.PlaySingle (m_hurtSound);
		}
	}

	public void OnCollisionEvent(GameObject i_gameObject, GameObject i_initiator, GameObject i_target, RaycastHit2D i_hit) {
		if (i_gameObject == gameObject) {
			LayerMask pFloorMask = LayerMask.GetMask("Floor");
			if (pFloorMask.IsInLayerMask (i_target)) {
				SetMultiplier(1);
			}
			m_canChangeDir = true;
		}
	}

}
