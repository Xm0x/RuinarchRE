using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class RuinarchText : TextMeshProUGUI
{
	public bool changeFontSizeBasedOnLocalization = true;

	private TMP_FontAsset _defaultFontAsset;

	private float _defaultFontSize;

	protected override void Awake()
	{
		base.Awake();
		_defaultFontAsset = base.font;
		_defaultFontSize = base.fontSize;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (LocalizationManager.Instance != null)
		{
			if (LocalizationManager.Instance.HasSpecifiedFontForLocale(LocalizationSettings.SelectedLocale))
			{
				LocalizedFontSettings specifiedFontSettingsForLocale = LocalizationManager.Instance.GetSpecifiedFontSettingsForLocale(LocalizationSettings.SelectedLocale);
				base.font = specifiedFontSettingsForLocale.fontAsset;
				if (changeFontSizeBasedOnLocalization)
				{
					base.fontSize = _defaultFontSize + (float)specifiedFontSettingsForLocale.fontSizeModifier;
					if (base.fontSize < (float)specifiedFontSettingsForLocale.minimumFontSize)
					{
						base.fontSize = specifiedFontSettingsForLocale.minimumFontSize;
					}
				}
			}
			else if (base.font != _defaultFontAsset)
			{
				base.font = _defaultFontAsset;
				base.fontSize = _defaultFontSize;
			}
		}
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
	}

	private void OnLocaleChanged(Locale p_locale)
	{
		TryUpdateFontAssetGivenLocale(p_locale);
	}

	private void TryUpdateFontAssetGivenLocale(Locale p_locale)
	{
		if (!(LocalizationManager.Instance != null))
		{
			return;
		}
		if (LocalizationManager.Instance.HasSpecifiedFontForLocale(LocalizationSettings.SelectedLocale))
		{
			LocalizedFontSettings specifiedFontSettingsForLocale = LocalizationManager.Instance.GetSpecifiedFontSettingsForLocale(LocalizationSettings.SelectedLocale);
			base.font = specifiedFontSettingsForLocale.fontAsset;
			if (changeFontSizeBasedOnLocalization)
			{
				base.fontSize = _defaultFontSize + (float)specifiedFontSettingsForLocale.fontSizeModifier;
				if (base.fontSize < (float)specifiedFontSettingsForLocale.minimumFontSize)
				{
					base.fontSize = specifiedFontSettingsForLocale.minimumFontSize;
				}
			}
		}
		else if (base.font != _defaultFontAsset)
		{
			base.font = _defaultFontAsset;
			base.fontSize = _defaultFontSize;
		}
	}

	public void SetTextAndReplaceWithIcons(string text)
	{
		SetText(text);
	}

	private void ElementIconHoverInfo()
	{
		if (base.gameObject.GetComponent(typeof(ElementIconHoverEventLabel)) == null)
		{
			base.gameObject.AddComponent(typeof(ElementIconHoverEventLabel));
			raycastTarget = true;
		}
	}
}
