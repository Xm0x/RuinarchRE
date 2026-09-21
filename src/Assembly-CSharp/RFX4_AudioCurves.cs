using UnityEngine;

public class RFX4_AudioCurves : MonoBehaviour
{
	public AnimationCurve AudioCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public bool IsLoop;

	private bool canUpdate;

	private float startTime;

	private AudioSource audioSource;

	private float startVolume;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		startVolume = audioSource.volume;
		audioSource.volume = AudioCurve.Evaluate(0f);
	}

	private void OnEnable()
	{
		startTime = Time.time;
		canUpdate = true;
	}

	private void Update()
	{
		float num = Time.time - startTime;
		if (canUpdate)
		{
			float volume = AudioCurve.Evaluate(num / GraphTimeMultiplier) * startVolume;
			audioSource.volume = volume;
		}
		if (num >= GraphTimeMultiplier)
		{
			if (IsLoop)
			{
				startTime = Time.time;
			}
			else
			{
				canUpdate = false;
			}
		}
	}
}
