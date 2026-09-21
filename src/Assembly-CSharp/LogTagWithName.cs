using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogTagWithName : PooledObject
{
	[SerializeField]
	private Image tagImg;

	[SerializeField]
	private TextMeshProUGUI tagName;

	public void SetTag(LOG_TAG tag)
	{
		tagImg.sprite = UIManager.Instance.GetLogTagSprite(tag);
		string key = tag.ToStringEnumWithSpace();
		tagName.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", key);
	}
}
