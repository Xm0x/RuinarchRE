namespace UnityEngine.UI.Extensions.Examples;

[RequireComponent(typeof(Image))]
public class CooldownEffect_Image : MonoBehaviour
{
	public CooldownButton cooldown;

	public Text displayText;

	private Image target;

	private string originalText;

	private void Start()
	{
		if (cooldown == null)
		{
			Debug.LogError("Missing Cooldown Button assignment");
		}
		target = GetComponent<Image>();
	}

	private void Update()
	{
		target.fillAmount = Mathf.Lerp(0f, 1f, cooldown.CooldownTimeRemaining / cooldown.CooldownTimeout);
		if ((bool)displayText)
		{
			displayText.text = $"{cooldown.CooldownPercentComplete}%";
		}
	}

	private void OnDisable()
	{
		if ((bool)displayText)
		{
			displayText.text = originalText;
		}
	}

	private void OnEnable()
	{
		if ((bool)displayText)
		{
			originalText = displayText.text;
		}
	}
}
