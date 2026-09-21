using System.Collections.Generic;
using UtilityScripts;

public class Fang : WeaponItem
{
	public Fang()
	{
		Initialize(TILE_OBJECT_TYPE.FANG, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public Fang(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public override void ApplyWeaponEffectsOnHit(IPointOfInterest p_targetPOI)
	{
		base.ApplyWeaponEffectsOnHit(p_targetPOI);
		if (!(p_targetPOI is Character character) || !ChanceData.RollChance(CHANCE_TYPE.Fang_Affliction) || !character.limiterComponent.canBeAfflicted)
		{
			return;
		}
		List<AfflictData> list = RuinarchListPool<AfflictData>.Claim();
		for (int i = 0; i < PlayerSkillManager.Instance.allAfflictions.Length; i++)
		{
			PLAYER_SKILL_TYPE type = PlayerSkillManager.Instance.allAfflictions[i];
			AfflictData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(type);
			if (afflictionData.IsValid(character) && afflictionData.CanPerformAbilityTowards(character))
			{
				list.Add(afflictionData);
			}
		}
		if (list.Count > 0)
		{
			CollectionUtilities.GetRandomElement(list).ApplyAfflictionEffects(character);
		}
		RuinarchListPool<AfflictData>.Release(list);
	}
}
