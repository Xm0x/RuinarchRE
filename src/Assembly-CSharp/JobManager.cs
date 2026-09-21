using System;
using System.Collections.Generic;
using Goap.Job_Checkers;

public class JobManager : BaseMonoBehaviour
{
	public static JobManager Instance;

	private Dictionary<string, CanTakeJobChecker> _canTakeJobCheckers;

	private Dictionary<string, JobApplicabilityChecker> _applicabilityCheckers;

	public const string Can_Take_Search_For_Demonic_Area = "CanTakeSearchForDemonicArea";

	public const string Can_Take_Bury_Job = "CanTakeBury";

	public const string Can_Take_Join_Gathering = "CanTakeJoinGathering";

	public const string Can_Take_Remove_Status = "CanTakeRemoveStatus";

	public const string Can_Take_Counterattack = "CanTakeCounterattack";

	public const string Can_Take_Raid = "CanTakeRaid";

	public const string Can_Take_Hunt_Heirloom = "CanTakeHuntHeirloom";

	public const string Can_Take_Repair = "CanTakeRepair";

	public const string Can_Take_Haul = "CanTakeHaul";

	public const string Can_Take_Judgement = "CanTakeJudgement";

	public const string Can_Take_Apprehend = "CanTakeApprehend";

	public const string Can_Take_Obtain_Personal_Food = "CanTakeObtainPersonalFood";

	public const string Can_Take_Restrain = "CanTakeRestrain";

	public const string Can_Take_Remove_Fire = "CanTakeRemoveFire";

	public const string Can_Take_Exterminate = "CanTakeExterminate";

	public const string Can_Brew_Potion = "CanBrewPotion";

	public const string Can_Craft_Tool = "CanCraftTool";

	public const string Can_Brew_Antidote = "CanBrewAntidote";

	public const string Can_Craft_Well = "CanCraftWell";

	public const string Can_Craft_Phylactery = "CanCraftPhylactery";

	public const string Can_Steal_Corpse = "CanStealCorpse";

	public const string Can_Summon_Bone_Golem = "CanSummonBoneGolem";

	public const string Can_Take_Change_Class = "CanTakeChangeClass";

	public const string Can_Take_Extra_Change_Class = "CanTakeExtraChangeClass";

	public const string Can_Take_Snatch_Job = "CanTakeSnatchJob";

	public const string Can_Take_Steal_Job = "CanTakeStealJob";

	public const string Can_Take_Tend_Wyvern_Coop = "CanTakeTendWyvernCoop";

	public const string Can_Take_Build_Job = "CanTakeBuildJob";

	public const string Can_Take_Purify_Job = "CanTakePurifyJob";

	public const string Can_Take_Place_Blueprint_Job = "CanTakePlaceBlueprintJob";

	public const string Can_Take_Faction_Ideology_Job = "CanTakeFactionIdeologyJob";

	public const string Destroy_Applicability = "IsDestroyApplicable";

	public const string Remove_Status_Applicability = "IsRemoveStatusApplicable";

	public const string Remove_Status_Self_Applicability = "IsRemoveStatusSelfApplicable";

	public const string Remove_Status_Target_Applicability = "IsRemoveStatusTargetApplicable";

	public const string Heal_Self_Applicability = "IsHealSelfApplicable";

	public const string Bury_Settlement_Applicability = "IsBurySettlementApplicable";

	public const string Bury_Applicability = "IsBuryApplicable";

	public const string Apprehend_Applicability = "IsApprehendApplicable";

	public const string Produce_Resource_Applicability = "IsProduceResourceApplicable";

	public const string Repair_Applicability = "IsRepairApplicable";

	public const string Haul_Applicability = "IsHaulApplicable";

	public const string Judge_Applicability = "IsJudgeApplicable";

	public const string Apprehend_Settlement_Applicability = "IsApprehendSettlementApplicable";

	public const string Obtain_Personal_Food_Applicability = "IsObtainPersonalFoodApplicable";

	public const string Combine_Stockpile_Applicability = "IsCombineStockpileApplicable";

	public const string Restrain_Applicability = "IsRestrainApplicable";

	public const string Magic_Academy_Craft_Applicability = "IsMagicAcademyCraftStillApplicable";

	public const string Barracks_Craft_Applicability = "IsBarracksCraftStillApplicable";

	public const string FeedApplicability = "IsFeedStillApplicable";

	public const string Remove_Trap_Applicability = "IsRemoveTrapApplicable";

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void Initialize()
	{
		_canTakeJobCheckers = new Dictionary<string, CanTakeJobChecker>
		{
			{
				"CanTakeJoinGathering",
				new CanTakeJoinGathering()
			},
			{
				"CanTakeRemoveStatus",
				new CanTakeRemoveStatus()
			},
			{
				"CanTakeBury",
				new CanTakeBuryJob()
			},
			{
				"CanTakeCounterattack",
				new CanTakeCounterattackJob()
			},
			{
				"CanTakeRaid",
				new CanTakeRaidJob()
			},
			{
				"CanTakeHuntHeirloom",
				new CanTakeHuntHeirloomJob()
			},
			{
				"CanTakeRepair",
				new CanTakeRepairJob()
			},
			{
				"CanTakeHaul",
				new CanTakeHaulJob()
			},
			{
				"CanTakeJudgement",
				new CanTakeJudgement()
			},
			{
				"CanTakeApprehend",
				new CanTakeApprehend()
			},
			{
				"CanTakeObtainPersonalFood",
				new CanTakeObtainPersonalFood()
			},
			{
				"CanTakeRestrain",
				new CanTakeRestrainJob()
			},
			{
				"CanTakeRemoveFire",
				new CanTakeRemoveFire()
			},
			{
				"CanTakeExterminate",
				new CanTakeExterminateJob()
			},
			{
				"CanBrewPotion",
				new CanBrewPotion()
			},
			{
				"CanCraftTool",
				new CanCraftTool()
			},
			{
				"CanBrewAntidote",
				new CanBrewAntidote()
			},
			{
				"CanCraftWell",
				new CanCraftWell()
			},
			{
				"CanCraftPhylactery",
				new CanCraftPhylactery()
			},
			{
				"CanStealCorpse",
				new CanStealCorpse()
			},
			{
				"CanSummonBoneGolem",
				new CanSummonBoneGolem()
			},
			{
				"CanTakeChangeClass",
				new CanTakeChangeClass()
			},
			{
				"CanTakeExtraChangeClass",
				new CanTakeExtraChangeClass()
			},
			{
				"CanTakeSnatchJob",
				new CanTakeSnatchJob()
			},
			{
				"CanTakeStealJob",
				new CanTakeStealJob()
			},
			{
				"CanTakeTendWyvernCoop",
				new CanTakeTendWyvernCoopJob()
			},
			{
				"CanTakeSearchForDemonicArea",
				new CanTakeSearchForDemonicArea()
			},
			{
				"CanTakeBuildJob",
				new CanTakeBuildJob()
			},
			{
				"CanTakePurifyJob",
				new CanTakePurifyJob()
			},
			{
				"CanTakePlaceBlueprintJob",
				new CanTakePlaceBlueprintJob()
			},
			{
				"CanTakeFactionIdeologyJob",
				new CanTakeFactionIdeologyJob()
			}
		};
		_applicabilityCheckers = new Dictionary<string, JobApplicabilityChecker>
		{
			{
				"IsDestroyApplicable",
				new DestroyJobApplicabilityChecker()
			},
			{
				"IsRemoveStatusApplicable",
				new RemoveStatusApplicabilityChecker()
			},
			{
				"IsRemoveStatusSelfApplicable",
				new RemoveStatusSelfApplicabilityChecker()
			},
			{
				"IsRemoveStatusTargetApplicable",
				new RemoveStatusTargetApplicabilityChecker()
			},
			{
				"IsHealSelfApplicable",
				new HealSelfApplicabilityChecker()
			},
			{
				"IsBurySettlementApplicable",
				new BurySettlementApplicabilityChecker()
			},
			{
				"IsBuryApplicable",
				new BuryApplicabilityChecker()
			},
			{
				"IsApprehendApplicable",
				new ApprehendApplicabilityChecker()
			},
			{
				"IsProduceResourceApplicable",
				new ProduceResourceApplicabilityChecker()
			},
			{
				"IsRepairApplicable",
				new RepairApplicabilityChecker()
			},
			{
				"IsHaulApplicable",
				new HaulApplicabilityChecker()
			},
			{
				"IsJudgeApplicable",
				new JudgeApplicabilityChecker()
			},
			{
				"IsApprehendSettlementApplicable",
				new ApprehendSettlementApplicabilityChecker()
			},
			{
				"IsObtainPersonalFoodApplicable",
				new ObtainPersonalFoodApplicabilityChecker()
			},
			{
				"IsCombineStockpileApplicable",
				new CombineStockpileApplicabilityChecker()
			},
			{
				"IsRestrainApplicable",
				new RestrainApplicabilityChecker()
			},
			{
				"IsMagicAcademyCraftStillApplicable",
				new MagicAcademyCraftApplicabilityChecker()
			},
			{
				"IsBarracksCraftStillApplicable",
				new BarracksCraftApplicabilityChecker()
			},
			{
				"IsFeedStillApplicable",
				new FeedApplicabilityChecker()
			},
			{
				"IsRemoveTrapApplicable",
				new RemoveTrapApplicabilityChecker()
			}
		};
	}

	public void ReleaseJob(JobQueueItem job)
	{
		if (job is GoapPlanJob goapPlanJob)
		{
			if (goapPlanJob.isInMultithread)
			{
				goapPlanJob.SetShouldForceCancelJobUponReceiving(state: true);
			}
			else
			{
				ObjectPoolManager.Instance.ReturnGoapPlanJobToPool(goapPlanJob);
			}
		}
		else if (job is CharacterStateJob job2)
		{
			ObjectPoolManager.Instance.ReturnCharacterStateJobToPool(job2);
		}
	}

	public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, IJobOwner owner)
	{
		GoapPlanJob goapPlanJob = ObjectPoolManager.Instance.CreateNewGoapPlanJob();
		goapPlanJob.Initialize(jobType, goal, targetPOI, owner);
		return goapPlanJob;
	}

	public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, IJobOwner owner)
	{
		GoapPlanJob goapPlanJob = ObjectPoolManager.Instance.CreateNewGoapPlanJob();
		goapPlanJob.Initialize(jobType, targetInteractionType, targetPOI, owner);
		return goapPlanJob;
	}

	public GoapPlanJob CreateNewGoapPlanJob(SaveDataGoapPlanJob data)
	{
		GoapPlanJob goapPlanJob = ObjectPoolManager.Instance.CreateNewGoapPlanJob();
		goapPlanJob.Initialize(data);
		return goapPlanJob;
	}

	public CharacterStateJob CreateNewCharacterStateJob(JOB_TYPE jobType, CHARACTER_STATE state, IPointOfInterest targetPOI, IJobOwner owner)
	{
		CharacterStateJob characterStateJob = ObjectPoolManager.Instance.CreateNewCharacterStateJob();
		characterStateJob.Initialize(jobType, state, targetPOI, owner);
		return characterStateJob;
	}

	public CharacterStateJob CreateNewCharacterStateJob(JOB_TYPE jobType, CHARACTER_STATE state, IJobOwner owner)
	{
		CharacterStateJob characterStateJob = ObjectPoolManager.Instance.CreateNewCharacterStateJob();
		characterStateJob.Initialize(jobType, state, owner);
		return characterStateJob;
	}

	public CharacterStateJob CreateNewCharacterStateJob(SaveDataCharacterStateJob data)
	{
		CharacterStateJob characterStateJob = ObjectPoolManager.Instance.CreateNewCharacterStateJob();
		characterStateJob.Initialize(data);
		return characterStateJob;
	}

	public CanTakeJobChecker GetJobChecker(string key)
	{
		if (_canTakeJobCheckers.ContainsKey(key))
		{
			return _canTakeJobCheckers[key];
		}
		throw new Exception("Could not find job checker with key " + key);
	}

	public JobApplicabilityChecker GetApplicabilityChecker(string key)
	{
		if (_applicabilityCheckers.ContainsKey(key))
		{
			return _applicabilityCheckers[key];
		}
		throw new Exception("Could not find applicability checker with key " + key);
	}
}
