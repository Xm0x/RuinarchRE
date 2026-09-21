using System.Collections;
using UtilityScripts;

public class BlizzardParticleEffect : BaseParticleEffect
{
	private uint _sfxID;

	protected override IEnumerator PlayParticleCoroutine()
	{
		PlayParticle();
		yield return GameUtilities.waitForQuarterOfSecond;
		if (pauseOnGamePaused && GameManager.Instance.isPaused)
		{
			PauseParticle();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Blizzard", base.gameObject);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		AkSoundEngine.StopPlayingID(_sfxID);
	}
}
