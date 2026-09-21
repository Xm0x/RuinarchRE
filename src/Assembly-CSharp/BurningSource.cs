using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class BurningSource
{
	private int _poisOnFireCount;

	public string persistentID { get; }

	public int id { get; }

	public List<ITraitable> objectsOnFire { get; private set; }

	public bool hasBeenCleanedUp { get; private set; }

	public BurningSource(string overrideID = "")
	{
		persistentID = (string.IsNullOrEmpty(overrideID) ? Utilities.GetNewUniqueID() : overrideID);
		id = Utilities.SetID(this);
		objectsOnFire = new List<ITraitable>();
		_poisOnFireCount = 0;
		Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		DatabaseManager.Instance.burningSourceDatabase.Register(this);
	}

	public void AddObjectOnFire(ITraitable traitable)
	{
		if (!hasBeenCleanedUp && !objectsOnFire.Contains(traitable))
		{
			objectsOnFire.Add(traitable);
			if (traitable is IPointOfInterest)
			{
				_poisOnFireCount++;
			}
		}
	}

	private void RemoveObjectOnFire(ITraitable traitable, Burning p_burning)
	{
		if (!objectsOnFire.Remove(traitable))
		{
			return;
		}
		p_burning.SetSourceOfBurning(null, traitable);
		if (objectsOnFire.Count == 0)
		{
			SetAsInactive();
		}
		else
		{
			if (!(traitable is IPointOfInterest) || _poisOnFireCount <= 0)
			{
				return;
			}
			_poisOnFireCount--;
			if (_poisOnFireCount == 0)
			{
				List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
				list.AddRange(objectsOnFire);
				for (int i = 0; i < list.Count; i++)
				{
					ITraitable traitable2 = list[i];
					traitable2.traitContainer.RemoveTrait(traitable2, "Burning");
				}
				RuinarchListPool<ITraitable>.Release(list);
			}
		}
	}

	private void SetAsInactive()
	{
		Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.RemoveListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		Messenger.Broadcast(InnerMapSignals.BURNING_SOURCE_INACTIVE, this);
		DatabaseManager.Instance.burningSourceDatabase.UnRegister(this);
	}

	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy)
	{
		if (trait is Burning p_burning)
		{
			RemoveObjectOnFire(traitable, p_burning);
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		objectsOnFire.Remove(p_character);
	}

	public override string ToString()
	{
		return "Burning Source " + id + ". Objects: " + objectsOnFire.Count;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		objectsOnFire.Contains(p_character);
	}

	public void CleanUp()
	{
		objectsOnFire?.Clear();
		objectsOnFire = null;
		hasBeenCleanedUp = true;
	}
}
