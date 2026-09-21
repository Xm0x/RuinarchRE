using UnityEngine;
using UnityEngine.UI;

public class TraitEffectButton : MonoBehaviour
{
	public Text buttonText;

	private TraitEffect _traitEffect;

	public TraitEffect traitEffect => _traitEffect;

	public void SetCurrentlySelectedButton()
	{
		TraitPanelUI.Instance.currentSelectedTraitEffectButton = this;
	}

	public void SetTraitEffect(TraitEffect traitEffect)
	{
		_traitEffect = traitEffect;
		if (_traitEffect != null)
		{
			buttonText.text = _traitEffect.description;
		}
	}
}
