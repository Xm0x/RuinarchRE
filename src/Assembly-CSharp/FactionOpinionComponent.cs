using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class FactionOpinionComponent : FactionComponent, SharedOpinionModifierEventDispatcher.IExpiryListener
{
	public Dictionary<Character, List<SharedOpinionModifier>> factionOpinions { get; }

	public FactionOpinionComponent()
	{
		factionOpinions = new Dictionary<Character, List<SharedOpinionModifier>>();
	}

	public void SubscribeListeners(bool shouldLock)
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_REMOVED, OnCharacterRemovedFromDatabase, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter, shouldLock);
	}

	public void UnsubscribeListeners()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_REMOVED, OnCharacterRemovedFromDatabase);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
	}

	private void OnCharacterRemovedFromDatabase(Character p_character)
	{
		RemoveOpinionModifiersForCharacter(p_character);
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		RemoveOpinionModifiersForCharacter(p_character);
	}

	public void LoadReferences(SaveDataFactionOpinionComponent data)
	{
		if (data.factionOpinions == null)
		{
			return;
		}
		foreach (KeyValuePair<string, List<string>> factionOpinion in data.factionOpinions)
		{
			Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(factionOpinion.Key);
			if (characterByPersistentID != null)
			{
				List<SharedOpinionModifier> list = new List<SharedOpinionModifier>();
				for (int i = 0; i < factionOpinion.Value.Count; i++)
				{
					string id = factionOpinion.Value[i];
					SharedOpinionModifier opinionModifierByPersistentID = DatabaseManager.Instance.sharedOpinionDatabase.GetOpinionModifierByPersistentID(id);
					list.Add(opinionModifierByPersistentID);
					opinionModifierByPersistentID.eventDispatcher.SubscribeToExpiryEvent(this);
				}
				factionOpinions.Add(characterByPersistentID, list);
			}
		}
	}

	public void AddOpinionModifier(Character p_target, SharedOpinionModifier p_modifier)
	{
		if (!factionOpinions.ContainsKey(p_target))
		{
			factionOpinions.Add(p_target, RuinarchListPool<SharedOpinionModifier>.Claim());
		}
		factionOpinions[p_target].Add(p_modifier);
		TryApplyModiferToCharacter(base.owner.characters, p_target, p_modifier);
		p_modifier.eventDispatcher.SubscribeToExpiryEvent(this);
	}

	private void RemoveOpinionModifier(SharedOpinionModifier p_modifier)
	{
		foreach (KeyValuePair<Character, List<SharedOpinionModifier>> factionOpinion in factionOpinions)
		{
			if (factionOpinion.Value.Contains(p_modifier))
			{
				factionOpinion.Value.Remove(p_modifier);
				p_modifier.eventDispatcher.UnsubscribeToExpiryEvent(this);
				break;
			}
		}
	}

	private void RemoveOpinionModifiersForCharacter(Character p_character)
	{
		if (factionOpinions.ContainsKey(p_character))
		{
			List<SharedOpinionModifier> list = RuinarchListPool<SharedOpinionModifier>.Claim(factionOpinions[p_character].Count);
			list.AddRange(factionOpinions[p_character]);
			for (int i = 0; i < list.Count; i++)
			{
				SharedOpinionModifier sharedOpinionModifier = list[i];
				sharedOpinionModifier.eventDispatcher.ExecuteModifierExpired(sharedOpinionModifier);
			}
			factionOpinions.Remove(p_character);
			RuinarchListPool<SharedOpinionModifier>.Release(list);
		}
	}

	public void OnFactionMemberAdded(Character p_character)
	{
		foreach (KeyValuePair<Character, List<SharedOpinionModifier>> factionOpinion in factionOpinions)
		{
			for (int i = 0; i < factionOpinion.Value.Count; i++)
			{
				TryApplyModiferToCharacter(p_character, factionOpinion.Key, factionOpinion.Value[i]);
			}
		}
	}

	private void TryApplyModiferToCharacter(Character p_applyTo, Character p_targetOfModifier, SharedOpinionModifier p_modifier)
	{
		if (p_modifier.CanAddModifierToCharacter(p_applyTo))
		{
			p_applyTo.relationshipContainer.AddSharedOpinionModifier(p_applyTo, p_targetOfModifier, p_modifier);
		}
	}

	private void TryApplyModiferToCharacter(List<Character> p_characters, Character p_targetOfModifier, SharedOpinionModifier p_modifier)
	{
		for (int i = 0; i < p_characters.Count; i++)
		{
			TryApplyModiferToCharacter(p_characters[i], p_targetOfModifier, p_modifier);
		}
	}

	public void OnSharedOpinionModifierExpired(SharedOpinionModifier p_modifier)
	{
		RemoveOpinionModifier(p_modifier);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		factionOpinions.ContainsKey(p_character);
	}

	public void OnDisbandFaction()
	{
		if (factionOpinions == null || factionOpinions.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<Character, List<SharedOpinionModifier>> item in new Dictionary<Character, List<SharedOpinionModifier>>(factionOpinions))
		{
			List<SharedOpinionModifier> list = RuinarchListPool<SharedOpinionModifier>.Claim(item.Value.Count);
			list.AddRange(item.Value);
			for (int i = 0; i < list.Count; i++)
			{
				SharedOpinionModifier sharedOpinionModifier = list[i];
				sharedOpinionModifier.eventDispatcher.ExecuteModifierExpired(sharedOpinionModifier);
			}
			RuinarchListPool<SharedOpinionModifier>.Release(list);
		}
		factionOpinions?.Clear();
	}
}
