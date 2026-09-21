using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Object_Pools;
using UtilityScripts;

namespace Interrupts;

public class InterruptHolder : IRumorable, ICrimeable, IReactable, ISavable
{
	public string persistentID { get; private set; }

	public Interrupt interrupt { get; private set; }

	public Character actor { get; private set; }

	public IPointOfInterest target { get; private set; }

	public Character disguisedActor { get; private set; }

	public Character disguisedTarget { get; private set; }

	public Log effectLog { get; private set; }

	public string identifier { get; private set; }

	public Rumor rumor { get; private set; }

	public List<Character> awareCharacters { get; private set; }

	public string reason { get; private set; }

	public CRIME_TYPE crimeType { get; private set; }

	public bool shouldNotBeObjectPooled { get; private set; }

	public List<LOG_TAG> logTags { get; private set; }

	public int reactionProcessCounter { get; private set; }

	public bool isSupposedToBeInPool { get; private set; }

	public bool isIntel { get; private set; }

	public string name => interrupt.name;

	public string classificationName => "News";

	public Log informationLog => effectLog;

	public bool isStealth => false;

	public bool isRumor => rumor != null;

	public RUMOR_TYPE rumorType => RUMOR_TYPE.Interrupt;

	public CRIMABLE_TYPE crimableType => CRIMABLE_TYPE.Interrupt;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Interrupt;

	public Type serializedData => typeof(SaveDataInterruptHolder);

	public InterruptHolder()
	{
		persistentID = Utilities.GetNewUniqueID();
		identifier = string.Empty;
		awareCharacters = new List<Character>();
		logTags = new List<LOG_TAG>();
	}

	public InterruptHolder(SaveDataInterruptHolder data)
	{
		logTags = data.logTags;
		if (logTags == null)
		{
			logTags = new List<LOG_TAG>();
		}
		awareCharacters = new List<Character>();
		persistentID = data.persistentID;
		interrupt = InteractionManager.Instance.GetInterruptData(data.interruptType);
		identifier = data.identifier;
		reason = data.reason;
		crimeType = data.crimeType;
		shouldNotBeObjectPooled = data.shouldNotBeObjectPooled;
		isSupposedToBeInPool = data.isSupposedToBeInPool;
		isIntel = data.isIntel;
	}

	public void SetEffectLog(Log p_effectLog)
	{
		if (effectLog != null)
		{
			LogPool.Release(p_effectLog);
		}
		effectLog = p_effectLog;
	}

	public void SetIdentifier(string identifier)
	{
		this.identifier = identifier;
	}

	public void SetDisguisedActor(Character disguised)
	{
		disguisedActor = disguised;
	}

	public void SetDisguisedTarget(Character disguised)
	{
		disguisedTarget = disguised;
	}

	public void SetReason(string reason)
	{
		this.reason = reason;
	}

	public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return interrupt.ReactionToActor(actor, target, witness, this, status);
	}

	public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return interrupt.ReactionToTarget(actor, target, witness, this, status);
	}

	public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		return interrupt.ReactionOfTarget(actor, target, this, status);
	}

	public void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		interrupt.PopulateReactionsToActor(reactions, actor, target, witness, this, status);
	}

	public void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		interrupt.PopulateReactionsToTarget(reactions, actor, target, witness, this, status);
	}

	public void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		interrupt.PopulateReactionsOfTarget(reactions, actor, target, this, status);
	}

	public REACTABLE_EFFECT GetReactableEffect(Character witness)
	{
		return REACTABLE_EFFECT.Neutral;
	}

	public void AddAwareCharacter(Character character)
	{
		awareCharacters.Add(character);
	}

	public void SetAsRumor(Rumor newRumor)
	{
		if (rumor != newRumor)
		{
			rumor = newRumor;
			if (rumor != null)
			{
				rumor.SetRumorable(this);
			}
		}
	}

	public bool IsCharacterReferenced(Character p_character)
	{
		if (actor == p_character)
		{
			return true;
		}
		if (target == p_character)
		{
			return true;
		}
		if (disguisedActor == p_character)
		{
			return true;
		}
		if (disguisedTarget == p_character)
		{
			return true;
		}
		if (rumor != null && rumor.IsCharacterReferenced(p_character))
		{
			return true;
		}
		if (awareCharacters.Contains(p_character))
		{
			return true;
		}
		return false;
	}

	public bool IsStructureReferenced(LocationStructure p_structure)
	{
		if (rumor != null && rumor.IsStructureReferenced(p_structure))
		{
			return true;
		}
		return false;
	}

	public void SetIsIntel(bool p_state)
	{
		isIntel = p_state;
	}

	public void SetCrimeType()
	{
		if (crimeType == CRIME_TYPE.Unset)
		{
			UpdateCrimeType();
		}
	}

	private void UpdateCrimeType()
	{
		Character disguisedCharacter = actor;
		IPointOfInterest disguisedCharacter2 = target;
		if (actor.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter = actor.reactionComponent.disguisedCharacter;
		}
		if (target is Character character && character.reactionComponent.disguisedCharacter != null)
		{
			disguisedCharacter2 = character.reactionComponent.disguisedCharacter;
		}
		crimeType = interrupt.GetCrimeType(disguisedCharacter, disguisedCharacter2, this);
	}

	public void Initialize(Interrupt interrupt, Character actor, IPointOfInterest target, string identifier, string reason)
	{
		this.interrupt = interrupt;
		this.actor = actor;
		this.target = target;
		disguisedActor = actor.reactionComponent.disguisedCharacter;
		if (target is Character character)
		{
			disguisedTarget = character.reactionComponent.disguisedCharacter;
		}
		SetIdentifier(identifier);
		SetReason(reason);
		SetDefaultLogTags();
	}

	public void Reset()
	{
		interrupt = null;
		actor = null;
		target = null;
		disguisedActor = null;
		disguisedTarget = null;
		if (effectLog != null)
		{
			LogPool.Release(effectLog);
		}
		effectLog = null;
		rumor = null;
		identifier = string.Empty;
		crimeType = CRIME_TYPE.Unset;
		awareCharacters.Clear();
		logTags.Clear();
		isSupposedToBeInPool = false;
		reactionProcessCounter = 0;
	}

	public void SetShouldNotBeObjectPooled(bool state)
	{
		shouldNotBeObjectPooled = state;
	}

	public void SetIsSupposedToBeInPool(bool p_state)
	{
		isSupposedToBeInPool = p_state;
	}

	public void IncreaseReactionCounter()
	{
		reactionProcessCounter++;
	}

	public void DecreaseReactionCounter()
	{
		reactionProcessCounter--;
		if (isSupposedToBeInPool)
		{
			ObjectPoolManager.Instance.TryReturnInterruptToPool(this);
		}
	}

	public override string ToString()
	{
		return "Interrupt: " + (interrupt?.type.ToString() ?? "None") + ". Actor: " + (actor?.name ?? "None");
	}

	private void SetDefaultLogTags()
	{
		logTags.Clear();
		if (interrupt.logTags != null)
		{
			for (int i = 0; i < interrupt.logTags.Length; i++)
			{
				logTags.Add(interrupt.logTags[i]);
			}
		}
	}

	public bool LoadReferences(SaveDataInterruptHolder data)
	{
		bool result = true;
		actor = CharacterManager.Instance.GetCharacterByPersistentID(data.actorID);
		if (actor == null)
		{
			result = false;
		}
		if (!string.IsNullOrEmpty(data.targetID))
		{
			if (data.targetPOIType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				target = CharacterManager.Instance.GetCharacterByPersistentID(data.targetID);
			}
			else if (data.targetPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				target = InnerMapManager.Instance.GetTileObjectByPersistentID(data.targetID);
			}
			if (target == null)
			{
				result = false;
			}
		}
		disguisedActor = null;
		disguisedTarget = null;
		if (!string.IsNullOrEmpty(data.disguisedActorID))
		{
			disguisedActor = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedActorID);
		}
		if (!string.IsNullOrEmpty(data.disguisedTargetID))
		{
			disguisedTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedTargetID);
		}
		effectLog = null;
		if (data.effectLog != null)
		{
			effectLog = LogPool.Claim();
			effectLog.Copy(data.effectLog);
		}
		if (data.awareCharacterIDs != null && data.awareCharacterIDs.Count > 0)
		{
			for (int i = 0; i < data.awareCharacterIDs.Count; i++)
			{
				Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(data.awareCharacterIDs[i]);
				if (characterByPersistentID != null)
				{
					awareCharacters.Add(characterByPersistentID);
				}
			}
		}
		if (data.hasRumor)
		{
			rumor = data.rumor.Load();
			rumor.SetRumorable(this);
		}
		return result;
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = actor;
		_ = target;
		_ = disguisedActor;
		_ = disguisedTarget;
		awareCharacters.Contains(p_character);
		if (rumor != null)
		{
			rumor.CheckIfCharacterIsStillReferenced(p_character);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		awareCharacters.Remove(p_character);
		rumor?.DisconnectFromCharacter(p_character);
	}

	public bool IsImportantDataNull()
	{
		if (interrupt == null)
		{
			return true;
		}
		if (actor == null)
		{
			return true;
		}
		if (target == null)
		{
			return true;
		}
		if (rumor != null && (rumor.characterThatCreatedRumor == null || rumor.targetCharacter == null))
		{
			return true;
		}
		return false;
	}
}
