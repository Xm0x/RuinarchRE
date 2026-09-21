using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class FactionSuccessionComponent : FactionComponent
{
	private const int SUCCESSOR_LIMIT = 3;

	private WeightedDictionary<Character> _successionWeightedDictionary;

	public Character[] successors { get; private set; }

	public int[] successorWeights { get; private set; }

	public bool hasFirstDayStartedTriggered { get; private set; }

	public FactionSuccessionComponent()
	{
		successors = new Character[3];
		successorWeights = new int[3];
		_successionWeightedDictionary = new WeightedDictionary<Character>();
	}

	public FactionSuccessionComponent(SaveDataFactionSuccessionComponent data)
	{
		successors = new Character[3];
		successorWeights = new int[3];
		_successionWeightedDictionary = new WeightedDictionary<Character>();
		hasFirstDayStartedTriggered = data.hasFirstDayStartedTriggered;
	}

	public void AddListeners(bool shouldLock)
	{
		Messenger.AddListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader, shouldLock);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction, shouldLock);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterLeftFaction, shouldLock);
		Messenger.AddListener<Faction, Character, CrimeData>(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, OnCharacterBecomeWantedCriminalOfFaction, shouldLock);
		Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass, shouldLock);
	}

	public void RemoveListeners()
	{
		Messenger.RemoveListener<Character, ILeader>(CharacterSignals.ON_SET_AS_FACTION_LEADER, OnCharacterSetAsFactionLeader);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterJoinedFaction);
		Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterLeftFaction);
		Messenger.RemoveListener<Faction, Character, CrimeData>(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, OnCharacterBecomeWantedCriminalOfFaction);
		Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
	}

	private void OnCharacterSetAsFactionLeader(Character newLeader, ILeader prevLeader)
	{
		if (newLeader.faction == base.owner)
		{
			UpdateSuccessors();
		}
	}

	private void OnCharacterLeftFaction(Character character, Faction faction)
	{
		if (faction == base.owner)
		{
			UpdateSuccessors();
		}
	}

	private void OnCharacterJoinedFaction(Character character, Faction faction)
	{
		if (faction == base.owner)
		{
			UpdateSuccessors();
		}
	}

	private void OnCharacterBecomeWantedCriminalOfFaction(Faction faction, Character character, CrimeData crimeData)
	{
		if (faction == base.owner)
		{
			UpdateSuccessors();
		}
	}

	private void OnCharacterChangedClass(Character p_character, CharacterClass p_previousClass, CharacterClass p_newClass)
	{
		if (p_character.faction == base.owner && !p_previousClass.IsCombatant() && p_newClass.IsCombatant() && p_previousClass.className != "Werewolf" && p_newClass.className != "Werewolf")
		{
			UpdateSuccessors();
		}
	}

	public void OnCharacterDied(Character character)
	{
		for (int i = 0; i < successors.Length; i++)
		{
			Character character2 = successors[i];
			if (character2 != null && character2 == character)
			{
				UpdateSuccessors();
				break;
			}
		}
	}

	public void OnDayStarted()
	{
		if (!hasFirstDayStartedTriggered)
		{
			hasFirstDayStartedTriggered = true;
		}
		else
		{
			UpdateSuccessors();
		}
	}

	public void UpdateSuccessors()
	{
		_successionWeightedDictionary.Clear();
		List<Character> list = RuinarchListPool<Character>.Claim();
		ResetSuccessors();
		base.owner.factionType.succession.PopulateSuccessorListWeightsInOrder(list, _successionWeightedDictionary, base.owner);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (i < 3)
				{
					Character character = list[i];
					SetSuccessor(character, _successionWeightedDictionary.GetElementWeight(character), i);
				}
			}
		}
		Messenger.Broadcast(FactionSignals.UPDATED_SUCCESSORS, base.owner);
		RuinarchListPool<Character>.Release(list);
	}

	private void ResetSuccessors()
	{
		for (int i = 0; i < successors.Length; i++)
		{
			successors[i] = null;
			successorWeights[i] = 0;
		}
	}

	public void SetSuccessor(Character character, int weight, int index)
	{
		if (index >= 0 && index < successors.Length)
		{
			successors[index] = character;
			successorWeights[index] = weight;
		}
	}

	private bool AreAllSuccessorSlotsFilled()
	{
		for (int i = 0; i < successors.Length; i++)
		{
			if (successors[i] == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSuccessor(Character character)
	{
		for (int i = 0; i < successors.Length; i++)
		{
			Character character2 = successors[i];
			if (character2 != null && character2 == character)
			{
				return true;
			}
		}
		return false;
	}

	public int GetTotalWeightsOfSuccessors()
	{
		return successorWeights.Sum();
	}

	public int GetWeightOfSuccessor(Character character)
	{
		for (int i = 0; i < successors.Length; i++)
		{
			Character character2 = successors[i];
			if (character2 != null && character2 == character)
			{
				return successorWeights[i];
			}
		}
		return 0;
	}

	public Character PickSuccessor()
	{
		return base.owner.factionType.succession.PickSuccessor(successors, successorWeights);
	}

	public void LoadReferences(SaveDataFactionSuccessionComponent data)
	{
		for (int i = 0; i < data.successors.Length; i++)
		{
			string text = data.successors[i];
			if (!string.IsNullOrEmpty(text))
			{
				successors[i] = CharacterManager.Instance.GetCharacterByPersistentID(text);
			}
		}
		for (int j = 0; j < data.successorWeights.Length; j++)
		{
			successorWeights[j] = data.successorWeights[j];
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		successors.Contains(p_character);
	}

	public void OnDisbandFaction()
	{
		if (successors != null)
		{
			for (int i = 0; i < successors.Length; i++)
			{
				successors[i] = null;
			}
		}
	}
}
