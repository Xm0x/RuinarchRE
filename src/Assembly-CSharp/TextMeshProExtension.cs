using TMPro;

public static class TextMeshProExtension
{
	public static void Refresh(this TextMeshProUGUI p_text)
	{
		p_text.gameObject.SetActive(value: false);
		p_text.gameObject.SetActive(value: true);
	}
}
