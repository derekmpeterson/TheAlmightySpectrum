using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public static GameController s_instance;

	public GameObject m_playerPrefab;
	GameObject m_player;
	public GameObject m_levelPrefab;
	List<GameObject> m_levels = new List<GameObject>();
	int m_currentLevel = 0;
	LevelController m_levelController;
	public AudioClip m_music;
	public AudioClip m_successSong;

	public delegate void StartLevelEvent(GameObject i_level);
	public static event StartLevelEvent DoStartLevelEvent;

	// Use this for initialization
	void Start () {
		if (s_instance == null) {
			s_instance = this;
		} else if (s_instance != this) {
			Destroy (gameObject);
			return;
		}

		Vector3 pTempPos = new Vector3 (-10000.0f, 0.0f, 0.0f);
		m_player = Instantiate (m_playerPrefab, pTempPos, Quaternion.identity) as GameObject;

		ChangeLevel (0);

		EnableHUD ();
	}

	void OnEnable() {
		LevelController.DoCompleteLevelEvent += OnCompleteLevelEvent;
		LevelController.DoFailLevelEvent += OnFailLevelEvent;
	}

	void OnDisable() {
		LevelController.DoCompleteLevelEvent -= OnCompleteLevelEvent;
		LevelController.DoFailLevelEvent -= OnFailLevelEvent;
	}
	
	// Update is called once per frame
	void Update () {

	}

	void EnableHUD() {
//		GameObject.Find ("ComboText").GetComponent<Text>().enabled = true;
//		GameObject.Find ("ScoreText").GetComponent<Text>().enabled = true;
//		GameObject.Find ("LevelText").GetComponent<Text>().enabled = true;
	}

	public void OnCompleteLevelEvent(GameObject i_level) {
		SoundManager.instance.PlayMusic (m_successSong);
		ChangeLevel (m_currentLevel + 1);
	}

	public void OnFailLevelEvent(GameObject i_level) {
		GameObject.Find ("DataManager").GetComponent<DataManager> ().m_score = m_player.GetComponent<PlayerController> ().m_score;
		SoundManager.instance.musicSource.Stop();
		Application.LoadLevel ("GameOverScene");
	}

	void ChangeLevel(int i_level) {
		if (!gameObject) {
			return;
		}
		if (i_level > 0) {
			if (m_player) {
				Animator pAnimator = m_player.GetComponent<Animator> ();
				if (pAnimator) {
					pAnimator.SetTrigger ("teleport");
				}
				CharacterController pCharacterController = m_player.GetComponent<CharacterController> ();
				if (pCharacterController) {
					pCharacterController.m_runDir = Vector2.zero;
				}
			} else {
				bool pBad = true;
			}
		}

		float pRemainingTimer = 0.0f;
		if (m_levelController != null) {
			pRemainingTimer = m_levelController.m_timer;
		}
			
		CreateLevel(i_level);

		m_currentLevel = i_level;
		m_levelController = m_levels [m_levels.Count - 1].GetComponent<LevelController> ();


		m_levelController.m_timer += 0.5f * pRemainingTimer;


		StartCoroutine (MoveCamera ());
	}

	IEnumerator MoveCamera()
	{
		float pCameraLerp = 0.0f;
		Vector3 pStartPos = gameObject.transform.position;
		Vector3 pEndPos = new Vector3 (m_levelController.m_cameraAnchor.transform.position.x, m_levelController.m_cameraAnchor.transform.position.y, pStartPos.z);

		if (m_currentLevel > 0) {
			float pWaitTime = 1.75f;
			while (pWaitTime > 0.0f) {
				pWaitTime -= Time.deltaTime;
				yield return new WaitForSeconds (Time.deltaTime);
			}
		}

		while(pCameraLerp < 1.0f) {
			if (!gameObject) {
				yield break;
			}
			pCameraLerp += Time.deltaTime * 1.5f;

			Vector3 pLerpedPos = Vector3.Lerp (pStartPos, pEndPos, pCameraLerp);
			gameObject.transform.position = pLerpedPos;

			yield return new WaitForSeconds (Time.deltaTime);
		}

		m_player.SetActive (true);
		m_player.transform.SetParent (m_levels [m_levels.Count - 1].transform);
		m_player.transform.position = m_levelController.m_playerSpawner.transform.position;

		Animator pAnimator = m_player.GetComponent<Animator> ();
		if (pAnimator) {
			pAnimator.SetTrigger ("idle");
			pAnimator.SetTrigger ("walk");
		}

		CharacterController pCharacterController = m_player.GetComponent<CharacterController> ();
		if (pCharacterController) {
			pCharacterController.Bounce(Vector2.right);
		}

		while (m_levels.Count > 1) {
			Destroy (m_levels [0]);
			m_levels.RemoveAt (0);
		}

		if (DoStartLevelEvent != null) {
			DoStartLevelEvent(m_levels[0]);
		}

		SoundManager.instance.PlayMusic (m_music);
		GameObject.Find ("levelText").GetComponent<Text> ().text = "Stage "+(m_currentLevel+1);
	}

	void CreateLevel(int i_level) {
		Vector3 pLevelPos = new Vector3 (0.0f, 3.0f * (float)i_level, 0.0f);
		GameObject pLevel = Instantiate (m_levelPrefab, pLevelPos, Quaternion.identity) as GameObject;
		LevelController pLevelController = pLevel.GetComponent<LevelController> ();
		pLevelController.SetLevelSpeed (1.0f + (i_level * 0.25f));

		m_levels.Add (pLevel);
	}
}
