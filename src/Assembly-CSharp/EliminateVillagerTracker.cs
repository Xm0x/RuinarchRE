using System;
using System.Collections.Generic;

public class EliminateVillagerTracker
{
	public interface IListener
	{
		void OnCharacterEliminated(Character p_character);

		void OnCharacterAddedAsTarget(Character p_character);
	}

	private Action<Character> _characterEliminatedAction;

	private Action<Character> _characterAddedAsTargetAction;

	public int totalCharactersToEliminate { get; private set; }

	public List<Character> villagersToEliminate { get; private set; }

	public EliminateVillagerTracker()
	{
		villagersToEliminate = new List<Character>();
	}

	public void Initialize(List<Character> p_allCharacters)
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
		Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_DEMON_CULTIST, CheckIfCharacterIsEliminated);
		Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
		List<Character> allCharactersToBeEliminated = GetAllCharactersToBeEliminated(p_allCharacters);
		villagersToEliminate.Clear();
		for (int i = 0; i < allCharactersToBeEliminated.Count; i++)
		{
			AddVillagerToEliminate(allCharactersToBeEliminated[i]);
		}
		totalCharactersToEliminate = villagersToEliminate.Count;
		for (int j = 0; j < allCharactersToBeEliminated.Count; j++)
		{
			Character character = allCharactersToBeEliminated[j];
			if (ShouldConsiderCharacterAsEliminated(character))
			{
				EliminateVillager(character);
			}
		}
	}

	private List<Character> GetAllCharactersToBeEliminated(List<Character> p_allCharacters)
	{
		List<Character> list = new List<Character>();
		for (int i = 0; i < p_allCharacters.Count; i++)
		{
			Character character = p_allCharacters[i];
			if (!character.isDead && character.isNormalCharacter && character.race.IsSapient())
			{
				list.Add(character);
			}
		}
		return list;
	}

	public void Subscribe(IListener p_listener)
	{
		_characterEliminatedAction = (Action<Character>)Delegate.Combine(_characterEliminatedAction, new Action<Character>(p_listener.OnCharacterEliminated));
		_characterAddedAsTargetAction = (Action<Character>)Delegate.Combine(_characterAddedAsTargetAction, new Action<Character>(p_listener.OnCharacterAddedAsTarget));
	}

	public void Unsubscribe(IListener p_listener)
	{
		_characterEliminatedAction = (Action<Character>)Delegate.Remove(_characterEliminatedAction, new Action<Character>(p_listener.OnCharacterEliminated));
		_characterAddedAsTargetAction = (Action<Character>)Delegate.Remove(_characterAddedAsTargetAction, new Action<Character>(p_listener.OnCharacterAddedAsTarget));
	}

	private void CheckIfCharacterIsEliminated(Character p_character)
	{
		if (ShouldConsiderCharacterAsEliminated(p_character))
		{
			EliminateVillager(p_character);
		}
	}

	private void OnNewVillagerArrived(Character newVillager)
	{
		AddVillagerToEliminate(newVillager);
	}

	private void OnCharacterNoLongerCultist(Character p_character)
	{
		AddVillagerToEliminate(p_character);
	}

	private void EliminateVillager(Character p_character)
	{
		if (villagersToEliminate.Remove(p_character))
		{
			totalCharactersToEliminate--;
			_characterEliminatedAction?.Invoke(p_character);
		}
	}

	private void AddVillagerToEliminate(Character p_character)
	{
		if (!villagersToEliminate.Contains(p_character))
		{
			villagersToEliminate.Add(p_character);
			totalCharactersToEliminate++;
			_characterAddedAsTargetAction?.Invoke(p_character);
		}
	}

	public static bool ShouldConsiderCharacterAsEliminated(Character character)
	{
		if (character.isDead)
		{
			return true;
		}
		if (character.traitContainer.HasTrait("Demon Cultist"))
		{
			return true;
		}
		if (character.faction != null && !character.faction.isMajorNonPlayerOrVagrant && character.faction.factionType.type != FACTION_TYPE.Ratmen)
		{
			return true;
		}
		return false;
	}
}
