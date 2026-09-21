namespace UnityEngine.UI.Extensions.Examples;

[RequireComponent(typeof(SoftMaskScript))]
public class CooldownEffect_SAUIM : MonoBehaviour
{
	public CooldownButton cooldown;

	private SoftMaskScript sauim;

	private void Start()
	{
		if (cooldown == null)
		{
			Debug.LogError("Missing Cooldown Button assignment");
		}
		sauim = GetComponent<SoftMaskScript>();
	}

	private void Update()
	{
		sauim.CutOff = Mathf.Lerp(0f, 1f, cooldown.CooldownTimeElapsed / cooldown.CooldownTimeout);
	}
}
