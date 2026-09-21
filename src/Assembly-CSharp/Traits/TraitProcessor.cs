using UtilityScripts;

namespace Traits;

public abstract class TraitProcessor
{
	public abstract void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration);

	public abstract void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy = null);

	public abstract void OnStatusStacked(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration);

	public abstract void OnStatusUnstack(ITraitable traitable, Status status, Character removedBy = null, bool bySchedule = false);

	protected void DefaultProcessOnAddTrait(ITraitable traitable, Trait trait, Character characterResponsible, int overrideDuration)
	{
		trait.AddCharacterResponsibleForTrait(characterResponsible);
		ApplyPOITraitInteractions(traitable, trait);
		trait.OnAddTrait(traitable);
		int num = overrideDuration;
		if (num == -1)
		{
			num = trait.ticksDuration;
		}
		GameDate gameDate = default(GameDate);
		if (num > 0)
		{
			gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(num);
			string ticket = SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				traitable.traitContainer.RemoveTraitOnSchedule(traitable, trait);
			}, traitable);
			traitable.traitContainer.AddScheduleTicket(trait.name, ticket, gameDate);
		}
		trait.ApplyMoodEffects(traitable, gameDate, characterResponsible);
		if (trait.traitOverrideFunctionIdentifiers != null && trait.traitOverrideFunctionIdentifiers.Count > 0)
		{
			for (int num2 = 0; num2 < trait.traitOverrideFunctionIdentifiers.Count; num2++)
			{
				string identifier = trait.traitOverrideFunctionIdentifiers[num2];
				traitable.traitContainer.AddTraitOverrideFunction(identifier, trait);
			}
		}
		if (traitable is Character character)
		{
			character.eventDispatcher.ExecuteCharacterGainedTrait(character, trait);
		}
		else if (traitable is TileObject tileObject)
		{
			tileObject.eventDispatcher.ExecuteTileObjectGainedTrait(tileObject, trait);
		}
		Messenger.Broadcast(TraitSignals.TRAITABLE_GAINED_TRAIT, traitable, trait);
	}

	protected void DefaultProcessOnRemoveTrait(ITraitable traitable, Trait trait, Character removedBy)
	{
		UnapplyPOITraitInteractions(traitable, trait);
		trait.OnRemoveTrait(traitable, removedBy);
		trait.UnapplyMoodEffects(traitable);
		if (trait.traitOverrideFunctionIdentifiers != null && trait.traitOverrideFunctionIdentifiers.Count > 0)
		{
			for (int i = 0; i < trait.traitOverrideFunctionIdentifiers.Count; i++)
			{
				string identifier = trait.traitOverrideFunctionIdentifiers[i];
				traitable.traitContainer.RemoveTraitOverrideFunction(identifier, trait);
			}
		}
		if (traitable is Character character)
		{
			character.eventDispatcher?.ExecuteCharacterLostTrait(character, trait, removedBy);
		}
		else if (traitable is TileObject tileObject)
		{
			tileObject.eventDispatcher?.ExecuteTileObjectLostTrait(tileObject, trait);
		}
		Messenger.Broadcast(TraitSignals.TRAITABLE_LOST_TRAIT, traitable, trait, removedBy);
	}

	protected bool DefaultProcessOnStackStatus(ITraitable traitable, Status status, Character characterResponsible, int overrideDuration)
	{
		int num = overrideDuration;
		if (num == -1)
		{
			num = status.ticksDuration;
		}
		GameDate gameDate = default(GameDate);
		if (num > 0)
		{
			gameDate = GameManager.Instance.Today();
			gameDate.AddTicks(num);
			string ticket = SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				traitable.traitContainer.RemoveTraitOnSchedule(traitable, status);
			}, traitable);
			traitable.traitContainer.AddScheduleTicket(status.name, ticket, gameDate);
		}
		if (traitable.traitContainer.stacks[status.name] <= status.stackLimit)
		{
			status.AddCharacterResponsibleForTrait(characterResponsible);
			status.OnStackStatus(traitable);
			status.ApplyStackedMoodEffect(traitable, gameDate, characterResponsible);
			return true;
		}
		status.OnStackStatusAddedButStackIsAtLimit(traitable);
		return false;
	}

	protected void DefaultProcessOnUnstackStatus(ITraitable traitable, Status status, Character removedBy, bool bySchedule)
	{
		if (traitable.traitContainer.stacks[status.name] < status.stackLimit)
		{
			status.OnUnstackStatus(traitable, bySchedule);
			status.UnapplyStackedMoodEffect(traitable);
		}
	}

	private void ApplyPOITraitInteractions(ITraitable traitable, Trait trait)
	{
		if (trait.advertisedInteractions != null)
		{
			for (int i = 0; i < trait.advertisedInteractions.Count; i++)
			{
				traitable.AddAdvertisedAction(trait.advertisedInteractions[i], allowDuplicates: true);
			}
		}
		if (traitable.advertisedActions != null && traitable.advertisedActions.Count > 0 && traitable is GenericTileObject poi)
		{
			LocationAwarenessUtility.AddToAwarenessList(poi, traitable.gridTileLocation);
		}
	}

	private void UnapplyPOITraitInteractions(ITraitable traitable, Trait trait)
	{
		if (trait.advertisedInteractions != null)
		{
			for (int i = 0; i < trait.advertisedInteractions.Count; i++)
			{
				traitable.RemoveAdvertisedAction(trait.advertisedInteractions[i]);
			}
		}
		if ((traitable.advertisedActions == null || traitable.advertisedActions.Count <= 0) && traitable is GenericTileObject poi)
		{
			LocationAwarenessUtility.RemoveFromAwarenessList(poi);
		}
	}
}
