using UtilityScripts;

namespace Factions.Faction_Types;

public class Bandits : FactionType
{
	public override RESOURCE mainResource => RESOURCE.WOOD;

	public override bool usesBothWoodAndStoneResources => true;

	public Bandits()
		: base(FACTION_TYPE.Bandits)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public Bandits(SaveDataFactionType saveData)
		: base(FACTION_TYPE.Bandits, saveData)
	{
		base.succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
	}

	public override void SetAsDefault(Faction p_faction)
	{
		Warmonger ideology = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
		AddIdeology(ideology, p_faction);
		Raiders ideology2 = FactionManager.Instance.CreateIdeology<Raiders>(FACTION_IDEOLOGY.Raiders);
		AddIdeology(ideology2, p_faction);
		base.hasCrimes = true;
		AddCrime(CRIME_TYPE.Infidelity, CRIME_SEVERITY.Infraction);
		AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Infraction);
		AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Infraction);
		AddCrime(CRIME_TYPE.Disturbances, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Trespassing, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Misdemeanor);
		AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Serious);
		AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
		AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
	}

	public override void SetFixedData()
	{
	}

	public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType)
	{
		return crimeType switch
		{
			CRIME_TYPE.Infidelity => CRIME_SEVERITY.Infraction, 
			CRIME_TYPE.Assault => CRIME_SEVERITY.Infraction, 
			CRIME_TYPE.Theft => CRIME_SEVERITY.Infraction, 
			CRIME_TYPE.Disturbances => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Arson => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Trespassing => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Murder => CRIME_SEVERITY.Misdemeanor, 
			CRIME_TYPE.Cannibalism => CRIME_SEVERITY.Serious, 
			CRIME_TYPE.Werewolf => CRIME_SEVERITY.Heinous, 
			CRIME_TYPE.Vampire => CRIME_SEVERITY.Heinous, 
			_ => CRIME_SEVERITY.None, 
		};
	}

	public override void RecruitProcess(Character p_actor, Character p_target, Character p_factionLeader, out JobQueueItem p_producedJob)
	{
		switch (p_target.raceSetting.category)
		{
		case CHARACTER_CATEGORY.Beast:
			if (GameUtilities.RollChance(50))
			{
				GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUTCHER, INTERACTION_TYPE.BUTCHER, p_target, p_actor);
				goapPlanJob.SetCancelOnDeath(state: false);
				p_producedJob = goapPlanJob;
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			break;
		case CHARACTER_CATEGORY.Humanoid:
			if (ChanceData.RollChance(CHANCE_TYPE.Rarely_Recruits))
			{
				p_actor.jobComponent.TriggerRecruitJob(p_target, out p_producedJob);
			}
			else
			{
				KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			}
			break;
		default:
			KillProcess(p_actor, p_target, p_factionLeader, out p_producedJob);
			break;
		}
	}
}
