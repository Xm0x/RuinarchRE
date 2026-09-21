using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataPlagueDisease : SaveData<PlagueDisease>
{
	public SaveDataPlagueLifespan lifespan;

	public List<PLAGUE_FATALITY> activeFatalities;

	public List<PLAGUE_SYMPTOM> activeSymptoms;

	public bool hasDeathEffect;

	public PLAGUE_DEATH_EFFECT activeDeathEffect;

	public int activeDeathEffectLevel;

	public int activeCases;

	public int deaths;

	public int recoveries;

	public Dictionary<PLAGUE_TRANSMISSION, int> transmissionLevels;

	public override void Save()
	{
		base.Save();
		PlagueDisease instance = PlagueDisease.Instance;
		lifespan = new SaveDataPlagueLifespan();
		lifespan.Save(instance.lifespan);
		if (instance.activeFatalities.Count > 0)
		{
			activeFatalities = new List<PLAGUE_FATALITY>();
			for (int i = 0; i < instance.activeFatalities.Count; i++)
			{
				activeFatalities.Add(instance.activeFatalities[i].fatalityType);
			}
		}
		if (instance.activeSymptoms.Count > 0)
		{
			activeSymptoms = new List<PLAGUE_SYMPTOM>();
			for (int j = 0; j < instance.activeSymptoms.Count; j++)
			{
				activeSymptoms.Add(instance.activeSymptoms[j].symptomType);
			}
		}
		hasDeathEffect = instance.activeDeathEffect != null;
		if (hasDeathEffect)
		{
			activeDeathEffect = instance.activeDeathEffect.deathEffectType;
			activeDeathEffectLevel = instance.activeDeathEffect.level;
		}
		activeCases = instance.activeCases;
		deaths = instance.deaths;
		recoveries = instance.recoveries;
		transmissionLevels = new Dictionary<PLAGUE_TRANSMISSION, int>(instance.transmissionLevels);
	}

	public override PlagueDisease Load()
	{
		return new PlagueDisease(this);
	}

	public override void CleanUp()
	{
		lifespan = null;
		activeFatalities?.Clear();
		activeFatalities = null;
		activeSymptoms?.Clear();
		activeSymptoms = null;
		transmissionLevels?.Clear();
		transmissionLevels = null;
	}
}
