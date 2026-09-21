using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Maccima_Games.Util;
using Ruinarch.Custom_UI;
using Traits;
using UnityEngine;
using UtilityScripts;

public class StoreTargetButton : MonoBehaviour
{
	[SerializeField]
	private RuinarchButton btn;

	[SerializeField]
	private HoverHandler hoverHandler;

	[SerializeField]
	private GameObject effectPrefab;

	private IStoredTarget _target;

	private void Awake()
	{
		btn.onClick.AddListener(OnClick);
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
		Messenger.AddListener(Signals.GAME_STARTED, OnGameStarted);
	}

	private void OnGameStarted()
	{
		Messenger.RemoveListener(Signals.GAME_STARTED, OnGameStarted);
		UpdateInteractableState();
	}

	private void OnEnable()
	{
		UpdateInteractableState();
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterRemovedTrait);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
		Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterRemovedTrait);
	}

	public void SetTarget(IStoredTarget p_target)
	{
		_target = p_target;
		btn.name = ((p_target == null) ? "BtnStoreTarget" : ("BtnStoreTarget-" + p_target.storedTargetType.ToStringEnum()));
		UpdateInteractableState();
	}

	private void OnClick()
	{
		AudioManager.Instance.TryPlayUISFX("Play_Add_Target");
		PlayStoreAnimation();
		PlayerManager.Instance.player.storedTargetsComponent.Store(_target);
		UpdateInteractableState();
	}

	private void PlayStoreAnimation()
	{
		Vector3 position = btn.transform.position;
		position.z = 0f;
		GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(effectPrefab.name, position, Quaternion.identity, UIManager.Instance.transform);
		effectGO.transform.position = position;
		Vector3 position2 = PlayerUI.Instance.targetsToggle.transform.position;
		Vector3 position3 = effectGO.transform.position;
		position3.x += 500f;
		position3.z = 0f;
		Vector3 vector = position2;
		vector.y -= 5f;
		vector.z = 0f;
		effectGO.transform.DOPath(new Vector3[3] { position2, position3, vector }, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(delegate
		{
			OnEffectCompleted(effectGO);
		});
	}

	private void OnEffectCompleted(GameObject p_effect)
	{
		PlayerUI.Instance.DoTargetTabPunchEffect();
		StoreTargetEffect component = p_effect.GetComponent<StoreTargetEffect>();
		component.trailParticles.Stop();
		component.SetImageState(p_state: false);
	}

	public void UpdateInteractableState()
	{
		if (_target != null)
		{
			if (!_target.IsValidForStoreTarget() || !GameManager.Instance.gameHasStarted)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			base.gameObject.SetActive(value: true);
			bool flag = PlayerManager.Instance.player.storedTargetsComponent.IsAlreadyStored(_target);
			bool flag2 = _target.CanBeStoredAsTarget();
			btn.interactable = !flag && flag2;
		}
	}

	private void OnHoverOver()
	{
		if (!btn.interactable)
		{
			if (_target is IPointOfInterest pointOfInterest)
			{
				if (pointOfInterest.traitContainer.HasTrait("Slick"))
				{
					UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Store_Target_Slick"), "", autoReplaceText: false);
				}
				else if (pointOfInterest.traitContainer.HasTrait("Temporal"))
				{
					UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Store_Target_Temporal"), "", autoReplaceText: false);
				}
			}
			else
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("targetName", _target.iconRichText + " " + _target.name);
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Store_Target_Already", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
				UIManager.Instance.ShowSmallInfo(localizedValue, "", autoReplaceText: false);
			}
		}
		else if (PlayerManager.Instance.player.storedTargetsComponent.HasStoredMaxCapacity())
		{
			IStoredTarget storedTarget = PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.First();
			Dictionary<string, string> dictionary2 = MaccimaDictionaryPool<string, string>.Claim();
			dictionary2.Add("targetName", storedTarget.iconRichText + " " + storedTarget.name);
			string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Store_Target_Max", dictionary2);
			MaccimaDictionaryPool<string, string>.Release(dictionary2);
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Store_Target") + ". \n" + Utilities.ColorizeInvalidText(localizedValue2), "", autoReplaceText: false);
		}
		else
		{
			UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Store_Target"), "", autoReplaceText: false);
		}
	}

	private void OnHoverOut()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void OnDestroy()
	{
		_target = null;
	}

	private void OnCharacterDied(Character p_character)
	{
		if (_target == p_character)
		{
			UpdateInteractableState();
		}
	}

	private void OnPlayerStoredTarget(IStoredTarget p_target)
	{
		if (_target == p_target)
		{
			UpdateInteractableState();
		}
	}

	private void OnCharacterRemovedTrait(Character p_character, Trait p_trait)
	{
		if (p_character == _target)
		{
			UpdateInteractableState();
		}
	}

	private void OnCharacterGainedTrait(Character p_character, Trait p_trait)
	{
		if (p_character == _target)
		{
			UpdateInteractableState();
		}
	}
}
