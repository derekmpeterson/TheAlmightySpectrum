using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (BoxCollider2D))]
public class CollisionController : MonoBehaviour {

	public LayerMask collisionMask;
	public List <RaycastHit2D> m_unhandledCollisions = new List<RaycastHit2D>();

	const float skinWidth = .015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float maxClimbAngle = 80.0f;
	float maxDescendAngle = 80.0f;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	BoxCollider2D collider;
	RaycastOrigins raycastOrigins;
	public CollisionInfo collisions;

	public delegate void CollisionEvent(GameObject i_gameObject, GameObject i_initiator, GameObject i_target, RaycastHit2D i_hit);
	public static event CollisionEvent DoCollisionEvent;

	void OnEnable() {
		collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();
	}

	public void Move(Vector2 velocity) {
		UpdateRaycastOrigins ();
		collisions.Reset ();
		collisions.velocityOld = velocity;
		
		if (velocity.y < 0) {
			DescendSlope (ref velocity);
		}
		if (velocity.x != 0.0f) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0.0f) {
			VerticalCollisions (ref velocity);
		}

		HandleCollisions ();
		
		transform.Translate (velocity);
	}

	void HorizontalCollisions(ref Vector2 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;
		
		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1.0f) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			collider.enabled = false;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			collider.enabled = true;
			
			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);
			
			if (hit) {
				
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}
					float distanceToSlopeStart = 0.0f;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}
				
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;
					
					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}
					
					collisions.left = directionX == -1.0f;
					collisions.right = directionX == 1.0f;
				}

				QueueCollision(hit);
			}
		}
	}
	
	void VerticalCollisions(ref Vector2 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;
		
		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1.0f) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			collider.enabled = false;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			collider.enabled = true;
			
			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);
			
			if (hit) {
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;
				
				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}
				
				collisions.above = directionY == 1.0f;
				collisions.below = directionY == -1.0f;

				QueueCollision(hit);
			}
		}
		
		// Check if we encounter a new slope while already climbing a slope
		if (collisions.climbingSlope) {
			float directionX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1.0f) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			collider.enabled = false;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			collider.enabled = true;
			
			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					// We have collided with a new slope
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}
	
	void ClimbSlope(ref Vector2 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
		
		if (velocity.y <= climbVelocityY) {
			// not jumping
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			velocity.y = climbVelocityY;
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}
	
	void DescendSlope(ref Vector2 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = directionX == -1.0f ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		collider.enabled = false;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
		collider.enabled = true;
		
		if (hit) {
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if (slopeAngle != 0.0f && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign (hit.normal.x) == directionX) {
					// We are moving down the slope
					if (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) {
						// Distance to the slope is less than how far we have to move on the y axis, we are close enough to descend it
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;
						
						collisions.slopeAngle = slopeAngle;
						collisions.descendingSlope = true;
						collisions.below = true;
					}
				}
			}
		}
	}

	void QueueCollision(RaycastHit2D hit) {
		if (hit.distance > skinWidth + 0.01f) {
			return;
		}

		bool found = false;
		for (int i = 0; i < m_unhandledCollisions.Count; i++) {
			if (hit == m_unhandledCollisions[i]) {
				found = true;
				break;
			}
		}
		if (!found) {
			m_unhandledCollisions.Add (hit);
		}
	}

	void HandleCollisions() {
		for (int i = 0; i < m_unhandledCollisions.Count; i++) {
			if (DoCollisionEvent != null) {
				GameObject pOtherGameObject = m_unhandledCollisions[i].collider.gameObject;
				DoCollisionEvent(gameObject, gameObject, pOtherGameObject, m_unhandledCollisions[i]);

				CollisionController pOtherCollisionController = pOtherGameObject.GetComponent<CollisionController>();
				if (pOtherCollisionController) {
					DoCollisionEvent(pOtherGameObject, gameObject, pOtherGameObject, m_unhandledCollisions[i]);
				}
			}
		}
		m_unhandledCollisions.Clear ();
	}
	
	void UpdateRaycastOrigins() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2.0f);
		
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
	}
	
	void CalculateRaySpacing() {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2.0f);
		
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);
		
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
	
	struct RaycastOrigins {
		public Vector2 topLeft, topRight, bottomLeft, bottomRight;
	}
	
	public struct CollisionInfo {
		public bool left, right, above, below, climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector2 velocityOld;
		
		public void Reset() {
			above = below = left = right = climbingSlope = descendingSlope = false;
			
			slopeAngleOld = slopeAngle;
			slopeAngle = 0.0f;
		}
	}
}
