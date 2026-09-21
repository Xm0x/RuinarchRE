using System.Collections;
using UtilityScripts;

public class RainParticleEffect : BaseParticleEffect
{
	protected override IEnumerator PlayParticleCoroutine()
	{
		PlayParticle();
		yield return GameUtilities.waitForQuarterOfSecond;
		if (pauseOnGamePaused && GameManager.Instance.isPaused)
		{
			PauseParticle();
		}
	}
}
