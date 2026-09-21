using System.Collections.Generic;
using UtilityScripts;

public class InfuseData : PlayerAction
{
	private List<InfuseChoice> _infuseChoices;

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.INFUSE;

	public override string name => "Infuse";

	public InfuseData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
		_infuseChoices = new List<InfuseChoice>
		{
			new BloatedChoice(),
			new FoodComaChoice(),
			new RabidChoice()
		};
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		TileObject tileObject = targetPOI as TileObject;
		if (tileObject == null)
		{
			return;
		}
		List<string> list = RuinarchListPool<string>.Claim();
		for (int i = 0; i < _infuseChoices.Count; i++)
		{
			InfuseChoice infuseChoice = _infuseChoices[i];
			if (infuseChoice.IsValid())
			{
				list.Add(infuseChoice.localizedName);
			}
		}
		UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
		{
			OnChooseInfuseChoice(o, tileObject);
		}, null, (string t) => CanPickInfuse(t, tileObject), "", delegate(string t)
		{
			OnHoverEnter(tileObject, t);
		}, OnHoverExit, "", showCover: true, 25);
		RuinarchListPool<string>.Release(list);
	}

	protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems)
	{
		if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is FoodPile)
		{
			p_contextMenuItems.Clear();
			for (int i = 0; i < _infuseChoices.Count; i++)
			{
				InfuseChoice infuseChoice = _infuseChoices[i];
				if (infuseChoice.IsValid())
				{
					p_contextMenuItems.Add(infuseChoice);
				}
			}
			return p_contextMenuItems;
		}
		return null;
	}

	public override bool IsValid(IPlayerActionTarget p_target)
	{
		if (base.IsValid(p_target))
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.MANIFEST_FOOD);
			if (!spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Bloated) && !spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Bloated_FoodComa))
			{
				return spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Unlock_Bloated_FoodComa_Rabid);
			}
			return true;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		bool flag = base.CanPerformAbilityTowards(tileObject);
		if (flag && tileObject is FoodPile foodPile)
		{
			return foodPile.infusedType == FOOD_INFUSE_TYPE.None;
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
		if (targetTileObject is FoodPile { infusedType: not FOOD_INFUSE_TYPE.None })
		{
			text = text + LocalizationManager.Instance.GetLocalizedValue("PlayerPowerReasons_Table", "Already_Infused") + "|";
		}
		return text;
	}

	private void OnHoverEnter(TileObject p_tileObject, string target)
	{
		if (!(p_tileObject is FoodPile p_foodPile))
		{
			return;
		}
		InfuseChoice infuseChoiceByLocalizedName = GetInfuseChoiceByLocalizedName(target);
		if (infuseChoiceByLocalizedName != null)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Traits_Table", infuseChoiceByLocalizedName.name + "_Description");
			string additionalText = string.Empty;
			if (!infuseChoiceByLocalizedName.CanInfuse(p_foodPile))
			{
				additionalText = Utilities.ColorizeInvalidText(infuseChoiceByLocalizedName.GetReasonsWhyCannotPerformAbilityTowards(p_tileObject));
			}
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(infuseChoiceByLocalizedName.localizedName, localizedValue, null, autoReplaceText: true, additionalText);
		}
	}

	private void OnHoverExit(string target)
	{
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}

	private void OnChooseInfuseChoice(object obj, TileObject targetObject)
	{
		if (obj is string p_name)
		{
			UIManager.Instance.HideObjectPicker();
			InfuseChoice infuseChoiceByLocalizedName = GetInfuseChoiceByLocalizedName(p_name);
			if (infuseChoiceByLocalizedName != null && targetObject is FoodPile p_foodPile)
			{
				infuseChoiceByLocalizedName.Infuse(p_foodPile);
			}
			base.ActivateAbility((IPointOfInterest)targetObject);
		}
	}

	private InfuseChoice GetInfuseChoiceByLocalizedName(string p_name)
	{
		for (int i = 0; i < _infuseChoices.Count; i++)
		{
			InfuseChoice infuseChoice = _infuseChoices[i];
			if (infuseChoice.localizedName == p_name)
			{
				return infuseChoice;
			}
		}
		return null;
	}

	private bool CanPickInfuse(string p_infuse, TileObject p_tileObject)
	{
		if (p_tileObject is FoodPile p_foodPile)
		{
			return GetInfuseChoiceByLocalizedName(p_infuse).CanInfuse(p_foodPile);
		}
		return false;
	}
}
