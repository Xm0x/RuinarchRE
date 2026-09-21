using System;
using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class Defiler : DemonicStructure
{
	public bool hasVampirismBefore { get; private set; }

	public bool hasSpawnNecronomiconBefore { get; private set; }

	public override Type serializedData => typeof(SaveDataDefiler);

	public Defiler(Region location)
		: base(STRUCTURE_TYPE.DEFILER, location)
	{
		SetMaxHPAndReset(3000);
	}

	public Defiler(Region location, SaveDataDefiler data)
		: base(location, data)
	{
		hasVampirismBefore = data.hasVampirismBefore;
		hasSpawnNecronomiconBefore = data.hasSpawnNecronomiconBefore;
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON);
		SkillData afflictionData = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM);
		hasVampirismBefore = afflictionData.isInUse;
		hasSpawnNecronomiconBefore = skillData.isInUse;
		PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(skillData);
		PlayerManager.Instance.player.playerSkillComponent.AddAndCategorizePlayerSkill(afflictionData);
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		base.AfterStructureDestruction(p_responsibleCharacter);
		if (!PlayerManager.Instance.player.playerSettlement.HasStructure(base.structureType))
		{
			if (!hasVampirismBefore)
			{
				PlayerManager.Instance.player.playerSkillComponent.RemovePlayerSkill(PLAYER_SKILL_TYPE.VAMPIRISM);
			}
			if (!hasSpawnNecronomiconBefore)
			{
				PlayerManager.Instance.player.playerSkillComponent.RemovePlayerSkill(PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON);
			}
		}
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		worldPosition = position;
	}
}
