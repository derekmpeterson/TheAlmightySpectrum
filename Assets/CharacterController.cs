using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent (typeof (CollisionController))]
public class CharacterController : MonoBehaviour {
	public float m_jumpHeight = 9.0f;
	public float m_doubleJumpPercent = 0.5f;
	public float m_timeToJumpApex = 0.32f;
	float m_airAcceleration = 0.25f;
	float m_groundAcceleration = 0.05f;
	public float m_moveSpeed = 8.0f;
	public float m_bounceSpeed = 4.0f;

	float m_speedMultiplier = 1.0f;
	float m_moveSpeedMultiplier = 1.0f;

	float m_jumpTime = 0.0f;
	float m_changeDirTime = 0.0f;

	float m_gravity;
	float m_jumpVelocity;
	public Vector2 m_velocity;
	float m_velocityXSmoothing;

	bool m_jumping = false;
	bool m_running = true;
	public Vector2 m_runDir = Vector2.right;
	
	public CollisionController m_collisionController;
	LevelController m_levelController;
	Animator m_animator;
	public LayerMask m_enemyMask;

	public bool m_heals = false;
	public bool m_frontWeapon = false;
	public bool m_footWeapon = false;
	public bool m_topWeapon = false;
	bool m_allowJumpAction = false;
	public bool m_changesDir = false;

	// Use this for initialization
	void Start() {
		m_collisionController = GetComponent<CollisionController> ();
		m_levelController = GetComponentInParent<LevelController> ();
		m_animator = GetComponent<Animator> ();
		
		m_gravity = -(2.0f * m_jumpHeight) / Mathf.Pow (m_timeToJumpApex, 2.0f);
		m_jumpVelocity = Mathf.Abs (m_gravity) * m_timeToJumpApex;
		m_velocity = new Vector2 (0.0f, 0.0f);
		m_runDir = Vector2.right;

		if (m_animator != null) {
			m_animator.SetTrigger ("walk");
		}
	}

	void OnEnable() {
		CollisionController.DoCollisionEvent += OnCollisionEvent;
	}

	void OnDisable() {
		CollisionController.DoCollisionEvent -= OnCollisionEvent;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_collisionController.collisions.above || m_collisionController.collisions.below) {
			m_velocity.y = 0.0f;
		}

		if (m_jumping) {
			m_jumpTime += Time.deltaTime;
		}

		if ((m_jumping) && m_collisionController.collisions.below) {
			Land ();
		}

		if (m_changesDir && m_changeDirTime <= 0.0f) {
			Bounce ();
			m_changeDirTime = Random.Range (1.0f, 5.0f);
		} else if (m_changesDir) {
			m_changeDirTime -= Time.deltaTime;
		}
				
		if (m_running) {
			float targetVelocityX = m_runDir.x * (m_moveSpeed * m_speedMultiplier * m_moveSpeedMultiplier * (1.0f / Mathf.Abs(transform.localScale.x)));
			m_velocity.x = Mathf.SmoothDamp (m_velocity.x, targetVelocityX, ref m_velocityXSmoothing, (m_collisionController.collisions.below) ? m_groundAcceleration : m_airAcceleration);
		} else {
			m_velocity.x = 0.0f;
		}

		// Slow gravity near the apex of a jump to make it feel floaty
		if (Mathf.Abs (m_velocity.y) > 5.0f || !m_allowJumpAction) {
			m_velocity.y += m_gravity * Time.deltaTime;
		} else {
			m_velocity.y += 0.5f * m_gravity * Time.deltaTime;
		}
		m_collisionController.Move (m_velocity * Time.deltaTime);
	}

	public void SetSpeedMultiplier(float i_speedMultiplier) {
		m_speedMultiplier = i_speedMultiplier;
	}

	public void SetMoveSpeedMultiplier(float i_moveSpeedMultiplier) {
		m_moveSpeedMultiplier = i_moveSpeedMultiplier;
	}
	
	public void Bounce(Vector2 i_forceRunDir=default(Vector2)) {
		Vector2 pOldRunDir = m_runDir;
		if (i_forceRunDir == Vector2.zero) {
			m_runDir.x *= -1.0f;
		} else {
			m_runDir = i_forceRunDir;
		}

		if (Vector2.Angle (pOldRunDir, m_runDir) >= 45.0f) {
			if (Mathf.Abs (m_velocity.x) >= m_bounceSpeed * m_speedMultiplier * (1.0f / Mathf.Abs(transform.localScale.x))) {
				m_velocity.x *= -1.0f;
			} else {
				m_velocity.x = m_runDir.x * m_bounceSpeed * m_speedMultiplier * (1.0f / Mathf.Abs(transform.localScale.x));
			}
				
			if (m_runDir != Vector2.zero) {
				Vector3 pScale = transform.localScale;
				pScale.x = Mathf.Abs (pScale.x) * m_runDir.x;
				transform.localScale = pScale;
			}

			m_allowJumpAction = true;
		}

		m_changeDirTime = Random.Range (1.0f, 5.0f);
	}

	void Land() {
		m_allowJumpAction = false;
		m_jumping = false;
		m_jumpTime = 0.0f;

		if (m_animator != null) {
			m_animator.SetTrigger ("walk");
		}
	}
	
	public void Jump(bool force=false) {
		if (force || m_collisionController.collisions.below) {
			m_velocity.y = m_jumpVelocity;
			m_collisionController.collisions.below = false;
			m_allowJumpAction = true;
			m_jumping = true;

			if (m_animator != null) {
				m_animator.SetTrigger ("jump");
			}

		} else if (m_allowJumpAction) {
			// Pressing jump while in the air
			m_velocity.y = m_doubleJumpPercent * m_jumpVelocity;
			m_collisionController.collisions.below = false;
			m_allowJumpAction = false;
		}
	}
	


	public void OnCollisionEvent(GameObject i_gameObject, GameObject i_initiator, GameObject i_target, RaycastHit2D i_hit) {
		if (i_gameObject == gameObject) {
			if (i_hit) {
				GameObject pObject = i_initiator;
				GameObject pOtherObject = i_target;
				float pHitSideAngle = Mathf.Abs (Vector2.Angle (i_hit.normal, Vector2.right));
				float pHitTopAngle = Mathf.Abs (Vector2.Angle (i_hit.normal, Vector2.up));
				if (pOtherObject == i_gameObject) {
					pObject = i_target;
					pOtherObject = i_initiator;
					pHitSideAngle = Mathf.Abs (Vector2.Angle (i_hit.normal, Vector2.left));
					pHitTopAngle = Mathf.Abs (Vector2.Angle (i_hit.normal, Vector2.down));
				}


				if (pHitSideAngle < 22.5f || pHitSideAngle > 157.5f) {
					// The character hit something from the side
					Bounce ();

					if (m_frontWeapon && m_collisionController.collisions.below && m_enemyMask.IsInLayerMask (pOtherObject)) {
						HealthController pHealthController = GetComponent<HealthController>();
						if (pHealthController && !pHealthController.m_colored) {
							HealthController.SendDamageEvent(pOtherObject, pObject, 1.0f);
						}
					}
				}

				if (pHitTopAngle < 45.0f) {
					if (m_enemyMask.IsInLayerMask (pOtherObject)) {
						if (m_footWeapon) {
							HealthController pHealthController = GetComponent<HealthController>();
							if (pHealthController && pHealthController.m_colored) {
								float pDamageAmount = 1.0f;
								if (m_heals) {
									pDamageAmount = -1000.0f;
								}
								HealthController.SendDamageEvent(pOtherObject, pObject, pDamageAmount);
							}
						}
					}
					LayerMask pFloorMask = LayerMask.GetMask("Floor");
					if (!pFloorMask.IsInLayerMask (pOtherObject)) {
						Jump (true);
						m_collisionController.collisions.below = false;
					}
				} else if (pHitTopAngle > 157.5 && m_topWeapon) {
//					if (m_enemyMask.IsInLayerMask (pOtherObject)) {
//						HealthController pHealthController = GetComponent<HealthController>();
//						if (pHealthController && pHealthController.m_colored) {
//							float pDamageAmount = 1.0f;
//							if (m_heals) {
//								pDamageAmount = -1000.0f;
//							}
//							HealthController.SendDamageEvent(pOtherObject, pObject, pDamageAmount);
//						}
//					}
				}
			}
		}
	}

	public bool GetJumping() {
		return m_jumping;
	}

	public float GetJumpTime() {
		return m_jumpTime;
	}
}
