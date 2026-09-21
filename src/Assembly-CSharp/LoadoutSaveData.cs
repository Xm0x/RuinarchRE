using System;
using System.Collections.Generic;

[Serializable]
public class LoadoutSaveData
{
	public List<PLAYER_SKILL_TYPE> extraSpells;

	public List<PLAYER_SKILL_TYPE> extraAfflictions;

	public List<PLAYER_SKILL_TYPE> extraMinions;

	public List<PLAYER_SKILL_TYPE> extraStructures;

	public List<PLAYER_SKILL_TYPE> extraMiscs;

	public LoadoutSaveData()
	{
		extraSpells = new List<PLAYER_SKILL_TYPE>();
		extraAfflictions = new List<PLAYER_SKILL_TYPE>();
		extraMinions = new List<PLAYER_SKILL_TYPE>();
		extraStructures = new List<PLAYER_SKILL_TYPE>();
		extraMiscs = new List<PLAYER_SKILL_TYPE>();
	}

	public void AddExtraSpell(PLAYER_SKILL_TYPE skillType)
	{
		extraSpells.Add(skillType);
	}

	public void AddExtraAffliction(PLAYER_SKILL_TYPE skillType)
	{
		extraAfflictions.Add(skillType);
	}

	public void AddExtraMinion(PLAYER_SKILL_TYPE skillType)
	{
		extraMinions.Add(skillType);
	}

	public void AddExtraStructure(PLAYER_SKILL_TYPE skillType)
	{
		extraStructures.Add(skillType);
	}

	public void AddExtraMisc(PLAYER_SKILL_TYPE skillType)
	{
		extraMiscs.Add(skillType);
	}

	public void ClearExtraSpells()
	{
		extraSpells.Clear();
	}

	public void ClearExtraAfflictions()
	{
		extraAfflictions.Clear();
	}

	public void ClearExtraMinions()
	{
		extraMinions.Clear();
	}

	public void ClearExtraStructures()
	{
		extraStructures.Clear();
	}

	public void ClearExtraMiscs()
	{
		extraMiscs.Clear();
	}
}
