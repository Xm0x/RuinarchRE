using UnityEngine;

public class ThreatParticleEffect : MonoBehaviour
{
	public ParticleSystem[] threatParticleSystems;

	public GameObject leftParticleGO;

	public GameObject rightParticleGO;

	public GameObject bottomParticleGO;

	private bool _isPlaying;

	private void Start()
	{
		Messenger.AddListener(PlayerSignals.START_THREAT_EFFECT, OnStartThreatEffect);
		Messenger.AddListener(PlayerSignals.STOP_THREAT_EFFECT, OnStopThreatEffect);
		Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
		Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener(PlayerSignals.START_THREAT_EFFECT, OnStartThreatEffect);
		Messenger.RemoveListener(PlayerSignals.STOP_THREAT_EFFECT, OnStopThreatEffect);
	}

	private void OnInnerMapOpened(Region region)
	{
		base.gameObject.transform.SetParent(InnerMapCameraMove.Instance.transform);
		base.gameObject.transform.localPosition = Vector3.zero;
		UpdatePosition(InnerMapCameraMove.Instance.camera);
		if (_isPlaying)
		{
			StopEffect();
			PlayEffect();
		}
	}

	private void OnInnerMapClosed(Region region)
	{
		base.gameObject.transform.localPosition = Vector3.zero;
		if (_isPlaying)
		{
			StopEffect();
			PlayEffect();
		}
	}

	public void OnZoomCamera(Camera camera)
	{
		UpdatePosition(camera);
	}

	private void UpdatePosition(Camera camera)
	{
		Vector3 vector = camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
		Vector3 vector2 = camera.ViewportToWorldPoint(new Vector3(1f, 0f, 0f));
		Vector3 vector3 = camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f));
		leftParticleGO.transform.position = new Vector3(vector.x - 1f, leftParticleGO.transform.position.y, leftParticleGO.transform.position.z);
		rightParticleGO.transform.position = new Vector3(vector2.x + 1f, rightParticleGO.transform.position.y, rightParticleGO.transform.position.z);
		bottomParticleGO.transform.position = new Vector3(bottomParticleGO.transform.position.x, vector3.y - 1f, bottomParticleGO.transform.position.z);
	}

	private void OnStartThreatEffect()
	{
		CheckEffectState();
	}

	private void OnStopThreatEffect()
	{
		CheckEffectState();
	}

	private void CheckEffectState()
	{
		if (PlayerManager.Instance.player.retaliationComponent.isRetaliating)
		{
			PlayEffect();
		}
		else
		{
			StopEffect();
		}
	}

	private void PlayEffect()
	{
		if (_isPlaying)
		{
			return;
		}
		_isPlaying = true;
		for (int i = 0; i < threatParticleSystems.Length; i++)
		{
			ParticleSystem particleSystem = threatParticleSystems[i];
			if (!particleSystem.isPlaying)
			{
				particleSystem.Play();
			}
		}
	}

	private void StopEffect()
	{
		if (!_isPlaying)
		{
			return;
		}
		_isPlaying = false;
		for (int i = 0; i < threatParticleSystems.Length; i++)
		{
			ParticleSystem particleSystem = threatParticleSystems[i];
			if (particleSystem.isPlaying)
			{
				particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
		}
	}
}
