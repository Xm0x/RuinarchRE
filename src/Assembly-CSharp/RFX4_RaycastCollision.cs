using System;
using System.Collections.Generic;
using UnityEngine;

public class RFX4_RaycastCollision : MonoBehaviour
{
	public float RaycastDistance = 100f;

	public GameObject[] Effects;

	public float Offset;

	public float EnableTimeDelay;

	public float DestroyTime = 3f;

	public bool UsePivotPosition;

	public bool UseNormalRotation = true;

	public bool IsWorldSpace = true;

	public bool RealTimeUpdateRaycast;

	public bool DestroyAfterDisabling;

	[HideInInspector]
	public float HUE = -1f;

	[HideInInspector]
	public List<GameObject> CollidedInstances = new List<GameObject>();

	private const string particlesAdditionalName = "Distance";

	private ParticleSystem[] distanceParticles;

	private bool canUpdate;

	public event EventHandler<RFX4_PhysicsMotion.RFX4_CollisionInfo> CollisionEnter;

	private void Awake()
	{
		distanceParticles = base.transform.root.GetComponentsInChildren<ParticleSystem>();
	}

	private void OnEnable()
	{
		CollidedInstances.Clear();
		if ((double)EnableTimeDelay > 0.001)
		{
			Invoke("UpdateRaycast", EnableTimeDelay);
		}
		else
		{
			UpdateRaycast();
		}
	}

	private void OnDisable()
	{
		if (!DestroyAfterDisabling)
		{
			return;
		}
		foreach (GameObject collidedInstance in CollidedInstances)
		{
			UnityEngine.Object.Destroy(collidedInstance);
		}
	}

	private void Update()
	{
		if (canUpdate)
		{
			UpdateRaycast();
		}
	}

	private void UpdateRaycast()
	{
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, RaycastDistance))
		{
			Vector3 position = ((!UsePivotPosition) ? (hitInfo.point + hitInfo.normal * Offset) : hitInfo.transform.position);
			this.CollisionEnter?.Invoke(this, new RFX4_PhysicsMotion.RFX4_CollisionInfo
			{
				HitPoint = hitInfo.point,
				HitCollider = hitInfo.collider,
				HitGameObject = hitInfo.transform.gameObject
			});
			if (distanceParticles != null)
			{
				ParticleSystem[] array = distanceParticles;
				foreach (ParticleSystem particleSystem in array)
				{
					if (particleSystem != null && particleSystem.name.Contains("Distance"))
					{
						particleSystem.GetComponent<ParticleSystemRenderer>().lengthScale = (base.transform.position - hitInfo.point).magnitude / particleSystem.main.startSize.constantMax;
					}
				}
			}
			if (CollidedInstances.Count == 0)
			{
				GameObject[] effects = Effects;
				foreach (GameObject gameObject in effects)
				{
					if (gameObject != null)
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, position, default(Quaternion));
						RFX4_EffectSettings component = gameObject2.GetComponent<RFX4_EffectSettings>();
						RFX4_EffectSettings componentInParent = GetComponentInParent<RFX4_EffectSettings>();
						if (component != null)
						{
							_ = componentInParent != null;
						}
						CollidedInstances.Add(gameObject2);
						if (HUE > -0.9f)
						{
							RFX4_ColorHelper.ChangeObjectColorByHUE(gameObject2, HUE);
						}
						if (!IsWorldSpace)
						{
							gameObject2.transform.parent = base.transform;
						}
						if (UseNormalRotation)
						{
							gameObject2.transform.LookAt(hitInfo.point + hitInfo.normal);
						}
						if (DestroyTime > 0.0001f)
						{
							UnityEngine.Object.Destroy(gameObject2, DestroyTime);
						}
					}
				}
			}
			else
			{
				foreach (GameObject collidedInstance in CollidedInstances)
				{
					if (!(collidedInstance == null))
					{
						collidedInstance.transform.position = position;
						if (UseNormalRotation)
						{
							collidedInstance.transform.LookAt(hitInfo.point + hitInfo.normal);
						}
					}
				}
			}
		}
		if (RealTimeUpdateRaycast)
		{
			canUpdate = true;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward * RaycastDistance);
	}
}
