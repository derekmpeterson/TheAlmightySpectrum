using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour {

	public GameObject m_playerSpawner;
	public GameObject[] m_enemyTypes;
	public GameObject[] m_enemySpawners;
	public int m_enemyCount = 8;
	public int m_simultaneousEnemies = 4;
	int m_spawnedEnemies = 0;
	public GameObject m_cameraAnchor;
	public GameObject m_background;
	bool m_running = false;
	public float m_timer = 20.0f;
	float m_levelSpeed = 1.0f;

	private List <GameObject> m_enemies = new List<GameObject>();

	private bool m_spawningEnemies;

	public delegate void CompleteLevelEvent(GameObject i_level);
	public static event CompleteLevelEvent DoCompleteLevelEvent;

	public delegate void FailLevelEvent(GameObject i_level);
	public static event FailLevelEvent DoFailLevelEvent;
	
	void OnEnable () {
		HealthController.DoDeathEvent += OnDeathEvent;
		GameController.DoStartLevelEvent += OnStartLevelEvent;
	}

	void OnDisable () {
		HealthController.DoDeathEvent -= OnDeathEvent;
		GameController.DoStartLevelEvent -= OnStartLevelEvent;
	}
	
	// Update is called once per frame
	void Update () {
		if (!m_spawningEnemies && m_enemies.Count < m_simultaneousEnemies && m_spawnedEnemies < m_enemyCount && m_enemyTypes.Length > 0 && m_enemySpawners.Length > 0) {
			StartCoroutine (SpawnEnemies ());
		}
		if (m_running && GetBaddyCount() == 0 && m_spawnedEnemies >= m_enemyCount) {
			// Level completed
			CompleteLevel();
		}

		if (m_running) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0.0f) {
				m_timer = 0.0f;
				FailLevel ();
			}
			int pMinutes = (int)(m_timer / 60.0f);
			int pSeconds = ((int)m_timer) % 60;
			GameObject.Find ("timer").GetComponent<Text> ().text = string.Format ("{0:00}:{1:00}", pMinutes, pSeconds);
		}
	}

	int GetBaddyCount() {
		int pBaddyCount = 0;
		for (int i = 0; i < m_enemies.Count; i++) {
			if (!m_enemies [i].GetComponent<HealthController> ().m_colored){
				pBaddyCount++;
			}
		}
		return pBaddyCount;
	}

	void CompleteLevel() {
		Animator pAnimator = m_background.GetComponent<Animator> ();
		if (pAnimator) {
			pAnimator.SetTrigger ("color");
		}

		for (int i = 0; i < m_enemies.Count; i++) {
			CharacterController pCharacterController = m_enemies [i].GetComponent<CharacterController> ();
			if (pCharacterController) {
				pCharacterController.m_runDir = Vector2.zero;
			}
		}
		
		m_running = false;
		if (gameObject && DoCompleteLevelEvent != null) {
			DoCompleteLevelEvent (gameObject);
		}
	}

	void FailLevel() {
		m_running = false;
		if (gameObject && DoFailLevelEvent != null) {
			DoFailLevelEvent(gameObject);
		}
	}

	public void OnStartLevelEvent(GameObject i_level) {
		if (i_level == gameObject) {
			m_running = true;

			for (int i = 0; i < m_enemies.Count; i++) {
				m_enemies[i].GetComponent<EnemyController>().SetAwake (true);
			}
		}
	}

	public void OnDeathEvent(GameObject i_gameObject) {
		if (m_enemies.Contains (i_gameObject)) {
			Destroy (i_gameObject);
			RemoveEnemy (i_gameObject);
		}
	}

	public void RemoveEnemy(GameObject i_enemy) {
		m_enemies.Remove(i_enemy);
	}

	IEnumerator SpawnEnemies()
	{
		m_spawningEnemies = true;
		while (m_enemies.Count < m_simultaneousEnemies && m_spawnedEnemies < m_enemyCount && m_enemyTypes.Length > 0 && m_enemySpawners.Length > 0) {
			yield return new WaitForSeconds (Random.Range (0.25f, 1.5f));

 			GameObject pEnemyType = m_enemyTypes[Random.Range (0, m_enemyTypes.Length)];
			GameObject pSpawner = m_enemySpawners[Random.Range (0, m_enemySpawners.Length)];
			GameObject pEnemy = Instantiate (pEnemyType, pSpawner.transform.position, Quaternion.identity) as GameObject;
			pEnemy.GetComponent<CharacterController>().SetSpeedMultiplier(m_levelSpeed);
			pEnemy.transform.SetParent(gameObject.transform);

			if (m_running) {
				pEnemy.GetComponent<EnemyController>().SetAwake (true);
			}
			m_enemies.Add(pEnemy);
			m_spawnedEnemies++;
		}
		m_spawningEnemies = false;
	}

	public void SetLevelSpeed(float i_levelSpeed) {
		m_levelSpeed = i_levelSpeed;

		for (int i = 0; i < m_enemies.Count; i++) {
			m_enemies[i].GetComponent<CharacterController>().SetSpeedMultiplier(m_levelSpeed);
		}
	}
}
