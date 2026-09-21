using System.Linq;
using EZObjectPools;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class StoredTargetPortrait : PooledObject
{
	[SerializeField]
	private CharacterPortrait characterPortrait;

	[SerializeField]
	private LocationPortrait locationPortrait;

	[SerializeField]
	private TileObjectPortrait tileObjectPortrait;

	[SerializeField]
	private TextMeshProUGUI lblTargetName;

	[SerializeField]
	private RuinarchButton btnRemoveTarget;

	private IStoredTarget _storedTarget;

	public IStoredTarget storedTarget => _storedTarget;

	private void OnEnable()
	{
		btnRemoveTarget.onClick.AddListener(OnClickRemoveTarget);
	}

	private void OnDisable()
	{
		btnRemoveTarget.onClick.RemoveListener(OnClickRemoveTarget);
	}

	public void SetStoredTarget(IStoredTarget p_target)
	{
		_storedTarget = p_target;
		characterPortrait.gameObject.SetActive(value: false);
		locationPortrait.gameObject.SetActive(value: false);
		tileObjectPortrait.gameObject.SetActive(value: false);
		if (p_target is Character character)
		{
			characterPortrait.GeneratePortrait(character);
			characterPortrait.gameObject.SetActive(value: true);
		}
		else if (p_target is LocationStructure locationStructure)
		{
			locationPortrait.SetPortrait(locationStructure.structureType);
			locationPortrait.gameObject.SetActive(value: true);
		}
		else if (p_target is BaseSettlement baseSettlement)
		{
			locationPortrait.SetLocation(baseSettlement);
			LocationStructure locationStructure2 = baseSettlement.allStructures.FirstOrDefault();
			locationPortrait.SetPortrait(locationStructure2?.structureType ?? STRUCTURE_TYPE.CITY_CENTER);
			locationPortrait.gameObject.SetActive(value: true);
		}
		else if (p_target is TileObject tileObject)
		{
			tileObjectPortrait.SetTileObject(tileObject);
			tileObjectPortrait.gameObject.SetActive(value: true);
		}
		lblTargetName.text = p_target.name;
	}

	public void UpdateName()
	{
		lblTargetName.text = _storedTarget?.name;
	}

	private void OnClickRemoveTarget()
	{
		PlayerManager.Instance.player.storedTargetsComponent.Remove(_storedTarget);
	}

	public override void Reset()
	{
		base.Reset();
		_storedTarget = null;
	}
}
