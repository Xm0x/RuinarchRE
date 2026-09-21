using System;
using System.Collections;
using EZObjectPools;
using Inner_Maps;
using UnityEngine;

public class BaseParticleEffect : PooledObject
{
	public static Action<BaseParticleEffect> particleEffectActivated;

	public static Action<BaseParticleEffect> particleEffectDeactivated;

	public ParticleSystem[] particleSystems;

	private ParticleSystemRenderer[] _particleSystemRenderers;

	public bool pauseOnGamePaused;

	public LocationGridTile targetTile { get; protected set; }

	public void SetTargetTile(LocationGridTile tile)
	{
		targetTile = tile;
	}

	protected virtual void OnEnable()
	{
		particleEffectActivated?.Invoke(this);
	}

	protected virtual void OnDisable()
	{
		particleEffectDeactivated?.Invoke(this);
	}

	private void TryConstructParticleSystemRenderers()
	{
		if (_particleSystemRenderers == null || _particleSystemRenderers.Length == 0)
		{
			_particleSystemRenderers = new ParticleSystemRenderer[particleSystems.Length];
			for (int i = 0; i < particleSystems.Length; i++)
			{
				_particleSystemRenderers[i] = particleSystems[i].GetComponent<ParticleSystemRenderer>();
			}
		}
	}

	public void PlayParticleEffect()
	{
		StartCoroutine(PlayParticleCoroutine());
	}

	public void StopParticleEffect()
	{
		StopParticle();
	}

	public void ResetParticleEffect()
	{
		ResetParticle();
	}

	public void SetSortingOrder(int amount)
	{
		TryConstructParticleSystemRenderers();
		for (int i = 0; i < _particleSystemRenderers.Length; i++)
		{
			_particleSystemRenderers[i].sortingOrder = amount;
		}
	}

	protected virtual IEnumerator PlayParticleCoroutine()
	{
		PlayParticle();
		yield return null;
		if (pauseOnGamePaused && GameManager.Instance.isPaused)
		{
			PauseParticle();
		}
	}

	protected virtual void PlayParticle()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Play();
		}
	}

	protected virtual void PauseParticle()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Pause();
		}
	}

	protected virtual void StopParticle()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].Stop();
		}
	}

	protected virtual void ResetParticle()
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem obj = particleSystems[i];
			obj.Clear();
			obj.Stop();
		}
	}

	public void OnGamePaused(bool state)
	{
		if (state)
		{
			for (int i = 0; i < particleSystems.Length; i++)
			{
				if (particleSystems[i].isPlaying)
				{
					particleSystems[i].Pause();
				}
			}
			return;
		}
		for (int j = 0; j < particleSystems.Length; j++)
		{
			if (particleSystems[j].isPaused)
			{
				particleSystems[j].Play();
			}
		}
	}

	public virtual void SetSize(Vector2Int p_size)
	{
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem.ShapeModule shape = particleSystems[i].shape;
			shape.scale = new Vector3(p_size.x, p_size.y, 1f);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ResetParticleEffect();
		targetTile = null;
	}
}
