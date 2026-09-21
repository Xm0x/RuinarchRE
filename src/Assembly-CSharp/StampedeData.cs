using System;
using System.Collections.Generic;
using Inner_Maps;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class StampedeData : SkillData
{
	private string _bonusUIText = string.Empty;

	private string _bonusLevelUpUIText = string.Empty;

	private LocationGridTile m_targetTile;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.STAMPEDE;

	public override string name => "Stampede";

	public override string description => "This Spell will summons a small herd of beasts on the target ground. The number and type of Beasts spawned increases as you upgrade this Spell.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public StampedeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		m_targetTile = targetTile;
		Vector3 position = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
		position.z = 0f;
		PointerDirection component = ObjectPoolManager.Instance.InstantiateObjectFromPool("PointerDirection", position, Quaternion.identity).GetComponent<PointerDirection>();
		component.OnClick = (Action<PointerDirection, Vector3>)Delegate.Combine(component.OnClick, new Action<PointerDirection, Vector3>(OnPointerClick));
		component.OnCancel = (Action<PointerDirection>)Delegate.Combine(component.OnCancel, new Action<PointerDirection>(OnCancel));
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason))
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			targetTile.PopulateTilesInRadius(list, radius, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			bool result = true;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].elevationType == ELEVATION.WATER)
				{
					result = false;
					o_cannotPerformReason = "Cannot spawn stampede on an area with water.";
					break;
				}
			}
			RuinarchListPool<LocationGridTile>.Release(list);
			return result;
		}
		return false;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}

	protected override void OnLevelUp()
	{
		base.OnLevelUp();
		ResetBonusUIText();
		ResetBonusLevelUpUIText();
	}

	private void ResetBonusUIText()
	{
		_bonusUIText = string.Empty;
	}

	private void ResetBonusLevelUpUIText()
	{
		_bonusLevelUpUIText = string.Empty;
	}

	public override string GetBonusUIText()
	{
		if (string.IsNullOrEmpty(_bonusUIText))
		{
			int widthByCurrentLevel = GetWidthByCurrentLevel();
			_bonusUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Width") + ":"), widthByCurrentLevel);
		}
		return _bonusUIText;
	}

	public override string GetBonusLevelUpUIText()
	{
		if (string.IsNullOrEmpty(_bonusLevelUpUIText))
		{
			int widthByCurrentLevel = GetWidthByCurrentLevel();
			if (base.isMaxLevel)
			{
				_bonusLevelUpUIText = string.Format("{0} {1}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Width") + ":"), widthByCurrentLevel);
			}
			else
			{
				int widthByLevel = GetWidthByLevel(base.currentLevel + 1);
				_bonusLevelUpUIText = string.Format("{0} {1} {2} {3}", Utilities.ColorizeSpellTitle(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Width") + ":"), widthByCurrentLevel, Utilities.UpgradeArrowIcon(), Utilities.ColorizeUpgradeText($"{widthByLevel}"));
			}
		}
		return _bonusLevelUpUIText;
	}

	private void OnPointerClick(PointerDirection p_pointer, Vector3 p_position)
	{
		Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
		if (Vector2.Distance(m_targetTile.centeredWorldLocation, vector) > 1f)
		{
			p_pointer.OnClick = (Action<PointerDirection, Vector3>)Delegate.Remove(p_pointer.OnClick, new Action<PointerDirection, Vector3>(OnPointerClick));
			p_pointer.OnCancel = (Action<PointerDirection>)Delegate.Remove(p_pointer.OnCancel, new Action<PointerDirection>(OnCancel));
			Vector3 vector2 = vector - m_targetTile.centeredWorldLocation;
			vector2.Normalize();
			float num = Mathf.Atan2(vector2.y, vector2.x);
			LocationGridTile targetTile = m_targetTile;
			Vector3 vector3 = new Vector3(Mathf.Cos(num), Mathf.Sin(num), 0f) * 20f;
			Vector3 p_direction = targetTile.centeredWorldLocation + vector3;
			targetTile.tileObjectComponent.EnableStampede(p_direction, num * 57.29578f, GetWidthByCurrentLevel());
			ObjectPoolManager.Instance.DestroyObject(p_pointer.gameObject);
			base.ActivateAbility(m_targetTile);
		}
	}

	private void OnCancel(PointerDirection p_pointer)
	{
		p_pointer.OnClick = (Action<PointerDirection, Vector3>)Delegate.Remove(p_pointer.OnClick, new Action<PointerDirection, Vector3>(OnPointerClick));
		p_pointer.OnCancel = (Action<PointerDirection>)Delegate.Remove(p_pointer.OnCancel, new Action<PointerDirection>(OnCancel));
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
		ObjectPoolManager.Instance.DestroyObject(p_pointer.gameObject);
	}

	public int GetWidthByLevel(int p_level)
	{
		int result = 6;
		if (p_level >= 3)
		{
			result = 12;
		}
		else
		{
			switch (p_level)
			{
			case 2:
				result = 10;
				break;
			case 1:
				result = 8;
				break;
			}
		}
		return result;
	}

	public int GetLevelByWidth(int p_width)
	{
		int result = 0;
		switch (p_width)
		{
		case 6:
			result = 0;
			break;
		case 8:
			result = 1;
			break;
		case 10:
			result = 2;
			break;
		case 12:
			result = 3;
			break;
		}
		return result;
	}

	public int GetWidthByCurrentLevel()
	{
		return GetWidthByLevel(base.currentLevel);
	}
}
