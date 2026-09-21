using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class AreaMapTextPopup : PooledObject
{
	[SerializeField]
	private TextMeshPro text;

	private TMP_FontAsset _defaultFontAsset;

	private float _defaultFontSize;

	private void Awake()
	{
		_defaultFontAsset = text.font;
		_defaultFontSize = text.fontSize;
	}

	private void OnEnable()
	{
		if (LocalizationManager.Instance != null)
		{
			if (LocalizationManager.Instance.HasSpecifiedFontForLocale(LocalizationSettings.SelectedLocale))
			{
				LocalizedFontSettings specifiedFontSettingsForLocale = LocalizationManager.Instance.GetSpecifiedFontSettingsForLocale(LocalizationSettings.SelectedLocale);
				text.font = specifiedFontSettingsForLocale.fontAsset;
				text.fontSize = _defaultFontSize + (float)specifiedFontSettingsForLocale.fontSizeModifier;
				if (text.fontSize < (float)specifiedFontSettingsForLocale.minimumFontSize)
				{
					text.fontSize = specifiedFontSettingsForLocale.minimumFontSize;
				}
			}
			else if (text.font != _defaultFontAsset)
			{
				text.font = _defaultFontAsset;
				text.fontSize = _defaultFontSize;
			}
		}
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
	}

	private void OnDisable()
	{
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
	}

	public void Show(string p_text, Vector3 p_startPos, Color p_textColor)
	{
		text.text = p_text;
		text.color = p_textColor;
		base.transform.position = p_startPos;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOLocalMoveY(base.transform.localPosition.y + 1f, 1f));
		sequence.Join(text.DOFade(0f, 1.2f).SetEase(Ease.InQuint));
		sequence.OnComplete(OnCompleteTween);
		sequence.Play();
	}

	private void OnCompleteTween()
	{
		ObjectPoolManager.Instance.DestroyObject(this);
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
			text.font = specifiedFontSettingsForLocale.fontAsset;
			text.fontSize = _defaultFontSize + (float)specifiedFontSettingsForLocale.fontSizeModifier;
			if (text.fontSize < (float)specifiedFontSettingsForLocale.minimumFontSize)
			{
				text.fontSize = specifiedFontSettingsForLocale.minimumFontSize;
			}
		}
		else if (text.font != _defaultFontAsset)
		{
			text.font = _defaultFontAsset;
			text.fontSize = _defaultFontSize;
		}
	}
}
