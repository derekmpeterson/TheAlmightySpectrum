using UnityEngine;
using System.Collections;

public class HealthController : MonoBehaviour {

	public delegate void DamageEvent(GameObject i_victim, GameObject i_attacker, float i_damage);
	public static event DamageEvent DoDamageEvent;
	public delegate void DeathEvent(GameObject i_gameObject);
	public static event DeathEvent DoDeathEvent;
	public delegate void KillEvent(GameObject i_attacker);
	public static event KillEvent DoKillEvent;
	public delegate void RespawnEvent(GameObject i_gameObject);
	public static event RespawnEvent DoRespawnEvent;
	public bool m_colored = false;

	public float m_healthRegenRate = 0.5f;
	public float m_maxHealth = 1000.0f;
	public float m_startHealth = 1000.0f;
	float m_health;

	// Use this for initialization
	void Start() {
		m_health = m_startHealth;
	}

	void OnEnable() {
		HealthController.DoDamageEvent += OnDamageEvent;
	}

	void OnDisable () {
		HealthController.DoDamageEvent -= OnDamageEvent;
	}

	void Update()
	{
		ChangeHealth (m_healthRegenRate * Time.deltaTime, null);
	}

	public void ChangeHealth(float i_change, GameObject i_attacker) {
		m_health += i_change;

		if (m_health <= 0.0f) {
			Kill (i_attacker);
		}
		if (m_health >= m_maxHealth) {
			m_health = m_maxHealth;
			SetColor (true);
		} else {
			SetColor (false);
		}
	}

	public float GetHealth() {
		return m_health;
	}

	public void Kill(GameObject i_attacker) {
		if (DoDeathEvent != null) {
			DoDeathEvent(gameObject);
		}

		if (DoKillEvent != null) {
			DoKillEvent(i_attacker);
		}

		m_health = m_maxHealth;

		if (DoRespawnEvent != null) {
			DoRespawnEvent(gameObject);
		}
	}

	public void SetColor(bool i_color) {
		if (m_colored != i_color) {
			m_colored = i_color;
			Animator pAnimator = GetComponent<Animator> ();
			if (pAnimator) {
				if (i_color) {
					pAnimator.SetTrigger ("color");
				} else {
					pAnimator.SetTrigger ("uncolor");
				}
			}
		}
	}

	public void OnDamageEvent(GameObject i_victim, GameObject i_attacker, float i_damage) {
		if (i_victim == gameObject) {
			ChangeHealth (-i_damage, i_attacker);
			Animator pAnimator = GetComponent<Animator> ();
			if (pAnimator) {
				pAnimator.SetTrigger ("squish");
			}
			if (m_colored) {
				StartCoroutine (Grow ());
			}
		}
	}

	public static void SendDamageEvent(GameObject i_victim, GameObject i_attacker, float i_damage) {
		if (DoDamageEvent != null) {
			DoDamageEvent(i_victim, i_attacker, i_damage);
		}
	}

	IEnumerator Grow()
	{
		float pGrowDelay = 0.1f;
		while (pGrowDelay > 0.0f) {
			if (!gameObject) {
				yield break;
			}
			pGrowDelay -= Time.deltaTime;
			yield return new WaitForSeconds (Time.deltaTime);
		}

		Vector2 pScale = transform.localScale;
		pScale.x *= 1.1f;
		pScale.y *= 1.1f;
		transform.localScale = pScale;
	}
}
