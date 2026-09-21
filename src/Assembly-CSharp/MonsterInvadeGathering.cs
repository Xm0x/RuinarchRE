using System;
using Inner_Maps.Location_Structures;

public class MonsterInvadeGathering : Gathering
{
	public LocationStructure targetStructure { get; private set; }

	public Area targetArea { get; private set; }

	public Area areaForJoining { get; private set; }

	public bool isInvading { get; private set; }

	public override IGatheringTarget target => targetStructure;

	public override Type serializedData => typeof(SaveDataMonsterInvadeGathering);

	public MonsterInvadeGathering()
		: base(GATHERING_TYPE.Monster_Invade)
	{
		base.minimumGatheringSize = 3;
		base.waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
		base.relatedBehaviour = typeof(MonsterInvadeBehaviour);
		base.jobQueueOwnerType = JOB_OWNER.FACTION;
	}

	public MonsterInvadeGathering(SaveDataMonsterInvadeGathering data)
		: base(data)
	{
		isInvading = data.isInvading;
	}

	public override bool IsAllowedToJoin(Character character)
	{
		if (character.race == base.host.race && areaForJoining != null)
		{
			return character.areaLocation == areaForJoining;
		}
		return false;
	}

	protected override void OnWaitTimeOver()
	{
		base.OnWaitTimeOver();
		Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
	}

	protected override void OnDisbandGathering()
	{
		base.OnDisbandGathering();
		Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
	}

	protected override void OnSetHost()
	{
		base.OnSetHost();
		if (base.host != null)
		{
			areaForJoining = base.host.areaLocation;
		}
	}

	private void ProcessDisbandment()
	{
		DisbandGathering();
	}

	public void SetTargetStructure(LocationStructure structure)
	{
		if (targetStructure != structure)
		{
			targetStructure = structure;
		}
	}

	public void SetTargetArea(Area p_area)
	{
		if (targetArea != p_area)
		{
			targetArea = p_area;
		}
	}

	private void OnCharacterEnteredArea(Character character, Area p_area)
	{
		bool flag = false;
		if (targetStructure != null)
		{
			flag = targetStructure.settlementLocation != null && p_area.HasSettlementOnArea(targetStructure.settlementLocation);
		}
		else if (targetArea != null)
		{
			flag = p_area == targetArea;
		}
		if (flag && IsAttendee(character))
		{
			StartInvadeTimer();
		}
	}

	private void StartInvadeTimer()
	{
		if (!isInvading)
		{
			isInvading = true;
			GameDate gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
			SchedulingManager.Instance.AddEntry(gameDate, DoneInvadeTimer, this);
		}
	}

	private void DoneInvadeTimer()
	{
		if (isInvading)
		{
			isInvading = false;
			ProcessDisbandment();
		}
	}

	public override void LoadReferences(SaveDataGathering data)
	{
		base.LoadReferences(data);
		if (data is SaveDataMonsterInvadeGathering saveDataMonsterInvadeGathering)
		{
			if (!string.IsNullOrEmpty(saveDataMonsterInvadeGathering.targetStructure))
			{
				targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataMonsterInvadeGathering.targetStructure);
			}
			if (!string.IsNullOrEmpty(saveDataMonsterInvadeGathering.targetHex))
			{
				targetArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataMonsterInvadeGathering.targetHex);
			}
			if (!string.IsNullOrEmpty(saveDataMonsterInvadeGathering.hexForJoining))
			{
				areaForJoining = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataMonsterInvadeGathering.hexForJoining);
			}
			if (base.isWaitTimeOver && !base.isDisbanded)
			{
				Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
			}
		}
	}
}
