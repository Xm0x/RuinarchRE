using System;
using UnityEngine;

public class RFX4_PhysicsMotion : MonoBehaviour
{
	public class RFX4_CollisionInfo : EventArgs
	{
		public Vector3 HitPoint;

		public Collider HitCollider;

		public GameObject HitGameObject;
	}

	public bool UseCollisionDetect = true;

	public float MaxDistnace = -1f;

	public float Mass = 1f;

	public float Speed = 10f;

	public float RandomSpeedOffset;

	public float AirDrag = 0.1f;

	public bool UseGravity = true;

	public ForceMode ForceMode = ForceMode.Impulse;

	public Vector3 AddRealtimeForce = Vector3.zero;

	public float MinSpeed;

	public float ColliderRadius = 0.05f;

	public bool FreezeRotation;

	public bool UseTargetPositionAfterCollision;

	public GameObject EffectOnCollision;

	public bool CollisionEffectInWorldSpace = true;

	public bool LookAtNormal = true;

	public float CollisionEffectDestroyAfter = 5f;

	public GameObject[] DeactivateObjectsAfterCollision;

	[HideInInspector]
	public float HUE = -1f;

	private Rigidbody rigid;

	private SphereCollider collid;

	private ContactPoint lastContactPoint;

	private Collider lastCollider;

	private Vector3 offsetColliderPoint;

	private bool isCollided;

	private GameObject targetAnchor;

	private bool isInitializedForce;

	private float currentSpeedOffset;

	private RFX4_EffectSettings effectSettings;

	public event EventHandler<RFX4_CollisionInfo> CollisionEnter;

	private void OnEnable()
	{
		effectSettings = GetComponentInParent<RFX4_EffectSettings>();
		GameObject[] deactivateObjectsAfterCollision = DeactivateObjectsAfterCollision;
		foreach (GameObject gameObject in deactivateObjectsAfterCollision)
		{
			if (gameObject != null)
			{
				if (gameObject.GetComponent<ParticleSystem>() != null)
				{
					gameObject.SetActive(value: false);
				}
				gameObject.SetActive(value: true);
			}
		}
		currentSpeedOffset = UnityEngine.Random.Range((0f - RandomSpeedOffset) * 10000f, RandomSpeedOffset * 10000f) / 10000f;
		InitializeRigid();
	}

	private void InitializeRigid()
	{
		if (effectSettings.UseCollisionDetection)
		{
			collid = base.gameObject.AddComponent<SphereCollider>();
			collid.radius = ColliderRadius;
		}
		isInitializedForce = false;
	}

	private void InitializeForce()
	{
		rigid = base.gameObject.AddComponent<Rigidbody>();
		rigid.mass = effectSettings.Mass;
		rigid.drag = effectSettings.AirDrag;
		rigid.useGravity = effectSettings.UseGravity;
		if (FreezeRotation)
		{
			rigid.constraints = RigidbodyConstraints.FreezeRotation;
		}
		rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		rigid.interpolation = RigidbodyInterpolation.Interpolate;
		rigid.AddForce(base.transform.forward * (effectSettings.Speed + currentSpeedOffset), ForceMode);
		isInitializedForce = true;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isCollided && !effectSettings.UseCollisionDetection)
		{
			return;
		}
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (!isCollided)
			{
				isCollided = true;
				if (UseTargetPositionAfterCollision)
				{
					if (targetAnchor != null)
					{
						UnityEngine.Object.Destroy(targetAnchor);
					}
					targetAnchor = new GameObject();
					targetAnchor.hideFlags = HideFlags.HideAndDontSave;
					targetAnchor.transform.parent = contactPoint.otherCollider.transform;
					targetAnchor.transform.position = contactPoint.point;
					targetAnchor.transform.rotation = base.transform.rotation;
				}
			}
			this.CollisionEnter?.Invoke(this, new RFX4_CollisionInfo
			{
				HitPoint = contactPoint.point,
				HitCollider = contactPoint.otherCollider,
				HitGameObject = contactPoint.otherCollider.gameObject
			});
			if (EffectOnCollision != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(EffectOnCollision, contactPoint.point, default(Quaternion));
				if (HUE > -0.9f)
				{
					RFX4_ColorHelper.ChangeObjectColorByHUE(gameObject, HUE);
				}
				if (LookAtNormal)
				{
					gameObject.transform.LookAt(contactPoint.point + contactPoint.normal);
				}
				else
				{
					gameObject.transform.rotation = base.transform.rotation;
				}
				if (!CollisionEffectInWorldSpace)
				{
					gameObject.transform.parent = contactPoint.otherCollider.transform.parent;
				}
				UnityEngine.Object.Destroy(gameObject, CollisionEffectDestroyAfter);
			}
		}
		GameObject[] deactivateObjectsAfterCollision = DeactivateObjectsAfterCollision;
		foreach (GameObject gameObject2 in deactivateObjectsAfterCollision)
		{
			if (gameObject2 != null)
			{
				ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
				if (component != null)
				{
					component.Stop();
				}
				else
				{
					gameObject2.SetActive(value: false);
				}
			}
		}
		if (rigid != null)
		{
			UnityEngine.Object.Destroy(rigid);
		}
		if (collid != null)
		{
			UnityEngine.Object.Destroy(collid);
		}
	}

	private void FixedUpdate()
	{
		if (!isInitializedForce)
		{
			InitializeForce();
		}
		if (rigid != null && AddRealtimeForce.magnitude > 0.001f)
		{
			rigid.AddForce(AddRealtimeForce);
		}
		if (rigid != null && MinSpeed > 0.001f)
		{
			rigid.AddForce(base.transform.forward * MinSpeed);
		}
		if (rigid != null && effectSettings.MaxDistnace > 0f && base.transform.localPosition.magnitude > effectSettings.MaxDistnace)
		{
			RemoveRigidbody();
		}
		if (UseTargetPositionAfterCollision && isCollided && targetAnchor != null)
		{
			base.transform.position = targetAnchor.transform.position;
			base.transform.rotation = targetAnchor.transform.rotation;
		}
	}

	private void OnDisable()
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = default(Quaternion);
		RemoveRigidbody();
	}

	private void RemoveRigidbody()
	{
		isCollided = false;
		if (rigid != null)
		{
			UnityEngine.Object.Destroy(rigid);
		}
		if (collid != null)
		{
			UnityEngine.Object.Destroy(collid);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			Transform transform = base.transform;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, ColliderRadius);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.position + transform.forward * 100f);
		}
	}
}
