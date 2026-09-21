using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Maccima_Games.Util;
using Ruinarch;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using UtilityScripts;

public class BaseBuildingManager : MonoBehaviour
{
	public static BaseBuildingManager Instance;

	[SerializeField]
	private Image _buildIconImg;

	[SerializeField]
	private RuinarchText _gridCounterLbl;

	[SerializeField]
	private GameObject _baseBuildingSelectionUI;

	[SerializeField]
	private SpriteRenderer _selectionBoxImg;

	[SerializeField]
	private SpriteRenderer _tileSelectionBoxImg;

	[SerializeField]
	private SpriteRenderer _decorationImg;

	[SerializeField]
	private Transform _gridSelectionParent;

	[SerializeField]
	private Transform _gridSelectionPoolParent;

	[SerializeField]
	private GameObject _gridSelectionPrefab;

	[SerializeField]
	private Color _validColor;

	[SerializeField]
	private Color _invalidColor;

	[SerializeField]
	private Color _neutralColor;

	[SerializeField]
	private Color _textInvalidColor;

	[SerializeField]
	private Color _textNeutralColor;

	[SerializeField]
	private float _selectionUIOffsetX;

	[SerializeField]
	private float _selectionUIOffsetY;

	private List<LocationGridTile> _selectedTiles = new List<LocationGridTile>(100);

	private List<LocationGridTile> _uncorruptedSelectedTiles = new List<LocationGridTile>(100);

	private Queue<SpriteRenderer> _gridSelectionPool = new Queue<SpriteRenderer>(400);

	private List<SpriteRenderer> _activeGridSelections = new List<SpriteRenderer>(400);

	private LocationGridTile _startGridTile;

	private LocationGridTile _endGridTile;

	private LocationGridTile _prevEndGridTile;

	private bool _areSelectedTilesValid;

	private string _noCorruptedNeighbourInvalidString;

	private string _notEnoughChargesInvalidString;

	private string _notEnoughChargesWithParameterInvalidString;

	private string _notEnoughSpiritEnergyInvalidString;

	private string _cannotBuildOnNonCorruptedTileInvalidString;

	private string _cannotBuildOnOccupiedTileInvalidString;

	private string _cannotDemolishNonCorruptedTileInvalidString;

	private string _invalidMessage;

	private string _demolishFailedTitle;

	private string _demolishFailedDescription;

	private int _currentDecorationRotationIndex;

	private bool _isPressingLeftClick;

	private bool _isPressingDemolishKey;

	private SkillData _demolishSkill;

	public bool isPressingLeftClick => _isPressingLeftClick;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_demolishSkill = PlayerSkillManager.Instance.GetBuildSkillData(PLAYER_SKILL_TYPE.DEMOLISH);
		_demolishFailedTitle = LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Demolish_Tiles_Failed_Title");
		_demolishFailedDescription = LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Demolish_Tiles_Failed_Description");
		_notEnoughChargesInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Invalid_Not_Enough_Charges"));
		_notEnoughChargesWithParameterInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "Invalid_Not_Enough_Charges_Power"));
		_notEnoughSpiritEnergyInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Invalid_Not_Enough_Spirit_Energy"));
		_noCorruptedNeighbourInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Invalid_No_Corrupted_Neighbour"));
		_cannotBuildOnNonCorruptedTileInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_not_corrupted"));
		_cannotBuildOnOccupiedTileInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Invalid_Occupied_Tile"));
		_cannotDemolishNonCorruptedTileInvalidString = Utilities.ColorizeInvalidText(LocalizationManager.Instance.GetLocalizedValue("BuildPlayerSkill_Table", "Invalid_Demolish_Not_Corrupted"));
		InitializeObjectPooling();
	}

	private void Update()
	{
		if (!GameManager.Instance.gameHasStarted || _demolishSkill == null)
		{
			return;
		}
		if (!InputManager.Instance.isDemolishKeyKeyDown)
		{
			if (_isPressingDemolishKey)
			{
				if (!CanDoDemolishShortcut())
				{
					return;
				}
				ResetOnMouseRelease();
			}
			_isPressingDemolishKey = false;
			return;
		}
		_isPressingDemolishKey = true;
		if (CanDoDemolishShortcut() && (!UIManager.Instance.IsMouseOnUI() || _isPressingLeftClick))
		{
			LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
			Color p_color = _neutralColor;
			if (GetBaseBuildingCode(tileFromMousePosition, _demolishSkill) == BASE_BUILDING_CODE.Cannot_Target_By_Left_Click)
			{
				p_color = _invalidColor;
			}
			DrawTileSelectionBoxVisual(tileFromMousePosition, p_color);
			Inputs(_demolishSkill);
		}
	}

	public BASE_BUILDING_CODE BaseBuildingUpdate(SkillData p_skill, LocationGridTile p_hoveredTile, ref string p_hoverText)
	{
		if (p_skill is DecorationsData decorationsData)
		{
			if (decorationsData.chosenTileObjectType == TILE_OBJECT_TYPE.NONE)
			{
				return BASE_BUILDING_CODE.Override_Cursor_To_Default;
			}
			if (InputManager.Instance.rotateDecorationAction.phase == InputActionPhase.Performed)
			{
				OnPressRotate();
			}
		}
		Color p_color = _neutralColor;
		BASE_BUILDING_CODE baseBuildingCode = GetBaseBuildingCode(p_hoveredTile, p_skill, ref p_hoverText);
		if (baseBuildingCode == BASE_BUILDING_CODE.Cannot_Target_By_Left_Click)
		{
			p_color = _invalidColor;
		}
		DrawTileSelectionBoxVisual(p_hoveredTile, p_color);
		Inputs(p_skill);
		return baseBuildingCode;
	}

	private void Inputs(SkillData p_skill)
	{
		if (InputManager.Instance.GetMouseButtonDown(0))
		{
			OnLeftClick(p_skill);
			_isPressingLeftClick = true;
		}
		if (InputManager.Instance.GetMouseButton(0) && _isPressingLeftClick)
		{
			OnLeftPress(p_skill);
		}
		if (InputManager.Instance.GetMouseButtonUp(0))
		{
			if (_isPressingLeftClick)
			{
				OnLeftRelease(p_skill);
			}
			_isPressingLeftClick = false;
		}
	}

	private void OnLeftClick(SkillData p_skill)
	{
		ResetSelectedTiles();
		ResetStartAndEndTile();
		_startGridTile = InnerMapManager.Instance.GetTileFromMousePosition();
		UpdateIconImage(p_skill);
	}

	private void OnLeftPress(SkillData p_skill)
	{
		_prevEndGridTile = _endGridTile;
		_endGridTile = InnerMapManager.Instance.GetTileFromMousePosition();
		if (_startGridTile == null)
		{
			_startGridTile = _endGridTile;
		}
		LocationGridTile p_tile = _endGridTile;
		if (_endGridTile == null)
		{
			p_tile = _prevEndGridTile;
		}
		ShowBaseBuildingSelectionUI(p_tile);
		SelectTilesByActiveBuildSkill(p_skill);
	}

	private void OnLeftRelease(SkillData p_skill)
	{
		SelectTilesByActiveBuildSkill(p_skill);
		ProcessBuildSkillOnMouseRelease(p_skill);
		ResetOnMouseRelease();
	}

	private void OnPressRotate()
	{
		_currentDecorationRotationIndex++;
		if (_currentDecorationRotationIndex > 3)
		{
			_currentDecorationRotationIndex = 0;
		}
		RotateDecoration();
	}

	private BASE_BUILDING_CODE GetBaseBuildingCode(LocationGridTile p_hoveredTile, SkillData p_skill)
	{
		if (!_isPressingLeftClick)
		{
			if (CanTarget(p_hoveredTile, p_skill))
			{
				return BASE_BUILDING_CODE.Can_Target_By_Left_Click;
			}
			return BASE_BUILDING_CODE.Cannot_Target_By_Left_Click;
		}
		if (_areSelectedTilesValid)
		{
			return BASE_BUILDING_CODE.Can_Target_By_Left_Click;
		}
		return BASE_BUILDING_CODE.Cannot_Target_By_Left_Click;
	}

	private BASE_BUILDING_CODE GetBaseBuildingCode(LocationGridTile p_hoveredTile, SkillData p_skill, ref string p_hoverText)
	{
		if (!_isPressingLeftClick)
		{
			if (CanTarget(p_hoveredTile, p_skill, ref p_hoverText))
			{
				return BASE_BUILDING_CODE.Can_Target_By_Left_Click;
			}
			return BASE_BUILDING_CODE.Cannot_Target_By_Left_Click;
		}
		if (_areSelectedTilesValid)
		{
			return BASE_BUILDING_CODE.Can_Target_By_Left_Click;
		}
		p_hoverText = _invalidMessage;
		return BASE_BUILDING_CODE.Cannot_Target_By_Left_Click;
	}

	public void SetHasClickedLeft(bool p_state)
	{
		_isPressingLeftClick = p_state;
	}

	private void RotateDecoration()
	{
		Quaternion localRotation = Quaternion.Euler(0f, 0f, (float)_currentDecorationRotationIndex * 90f);
		_buildIconImg.transform.localRotation = localRotation;
	}

	public void ResetDecorationRotation()
	{
		_currentDecorationRotationIndex = 0;
		RotateDecoration();
	}

	private void DrawSelectionBoxVisual()
	{
		if (_startGridTile != null && _endGridTile != null)
		{
			_selectionBoxImg.gameObject.SetActive(value: true);
			_selectionBoxImg.transform.position = _startGridTile.worldLocation;
			int num = _endGridTile.localPlace.x - _startGridTile.localPlace.x;
			int num2 = _endGridTile.localPlace.y - _startGridTile.localPlace.y;
			if (num < 0 && num2 < 0)
			{
				num--;
				num2--;
				_selectionBoxImg.transform.position = new Vector3(_selectionBoxImg.transform.position.x + 1f, _selectionBoxImg.transform.position.y + 1f, _selectionBoxImg.transform.position.z);
			}
			else if (num < 0)
			{
				num--;
				num2++;
				_selectionBoxImg.transform.position = new Vector3(_selectionBoxImg.transform.position.x + 1f, _selectionBoxImg.transform.position.y, _selectionBoxImg.transform.position.z);
			}
			else if (num2 < 0)
			{
				num++;
				num2--;
				_selectionBoxImg.transform.position = new Vector3(_selectionBoxImg.transform.position.x, _selectionBoxImg.transform.position.y + 1f, _selectionBoxImg.transform.position.z);
			}
			else
			{
				num++;
				num2++;
			}
			_selectionBoxImg.size = new Vector2(num, num2);
		}
		else
		{
			_selectionBoxImg.gameObject.SetActive(value: false);
		}
	}

	private void DrawTileSelectionBoxVisual(LocationGridTile p_tile, Color p_color)
	{
		if (p_tile != null)
		{
			_tileSelectionBoxImg.gameObject.SetActive(value: true);
			_tileSelectionBoxImg.transform.position = p_tile.centeredWorldLocation;
		}
		else
		{
			DeactivateTileSelectionBoxVisual();
		}
		_tileSelectionBoxImg.color = p_color;
	}

	public void DeactivateTileSelectionBoxVisual()
	{
		_tileSelectionBoxImg.gameObject.SetActive(value: false);
	}

	private void SelectTilesByActiveBuildSkill(SkillData p_skill)
	{
		if (_startGridTile == null || _endGridTile == null || _endGridTile == _prevEndGridTile)
		{
			return;
		}
		InnerTileMap innerMap = GridMap.Instance.mainRegion.innerMap;
		LocationGridTile startGridTile = _startGridTile;
		LocationGridTile endGridTile = _endGridTile;
		int x = startGridTile.localPlace.x;
		int x2 = endGridTile.localPlace.x;
		int y = startGridTile.localPlace.y;
		int y2 = endGridTile.localPlace.y;
		if (endGridTile.localPlace.x < x)
		{
			x = endGridTile.localPlace.x;
			x2 = startGridTile.localPlace.x;
		}
		if (endGridTile.localPlace.y < y)
		{
			y = endGridTile.localPlace.y;
			y2 = startGridTile.localPlace.y;
		}
		ResetSelectedTiles();
		if (p_skill.type == PLAYER_SKILL_TYPE.DEMONIC_WALL)
		{
			int num = x2 - x;
			int num2 = y2 - y;
			bool flag = false;
			if (num2 > num)
			{
				for (int i = y; i <= y2; i++)
				{
					LocationGridTile tileFromMapCoordinatesRaw = innerMap.GetTileFromMapCoordinatesRaw(startGridTile.localPlace.x, i);
					if (CanTileBeSelectedForMultiSelection(tileFromMapCoordinatesRaw, p_skill))
					{
						_selectedTiles.Add(tileFromMapCoordinatesRaw);
						if (!tileFromMapCoordinatesRaw.corruptionComponent.isCorrupted)
						{
							_uncorruptedSelectedTiles.Add(tileFromMapCoordinatesRaw);
						}
						DrawGridSelectionOnTile(tileFromMapCoordinatesRaw);
						if (!flag && (tileFromMapCoordinatesRaw.corruptionComponent.HasCorruptedNeighbour() || tileFromMapCoordinatesRaw.corruptionComponent.isCorrupted))
						{
							flag = true;
						}
					}
				}
			}
			else
			{
				for (int j = x; j <= x2; j++)
				{
					LocationGridTile tileFromMapCoordinatesRaw2 = innerMap.GetTileFromMapCoordinatesRaw(j, startGridTile.localPlace.y);
					if (CanTileBeSelectedForMultiSelection(tileFromMapCoordinatesRaw2, p_skill))
					{
						_selectedTiles.Add(tileFromMapCoordinatesRaw2);
						if (!tileFromMapCoordinatesRaw2.corruptionComponent.isCorrupted)
						{
							_uncorruptedSelectedTiles.Add(tileFromMapCoordinatesRaw2);
						}
						DrawGridSelectionOnTile(tileFromMapCoordinatesRaw2);
						if (!flag && (tileFromMapCoordinatesRaw2.corruptionComponent.HasCorruptedNeighbour() || tileFromMapCoordinatesRaw2.corruptionComponent.isCorrupted))
						{
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				_selectedTiles.Clear();
				_uncorruptedSelectedTiles.Clear();
			}
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			bool flag2 = false;
			for (int k = x; k <= x2; k++)
			{
				for (int l = y; l <= y2; l++)
				{
					LocationGridTile tileFromMapCoordinatesRaw3 = innerMap.GetTileFromMapCoordinatesRaw(k, l);
					if (CanTileBeSelectedForMultiSelection(tileFromMapCoordinatesRaw3, p_skill))
					{
						_selectedTiles.Add(tileFromMapCoordinatesRaw3);
						if (!tileFromMapCoordinatesRaw3.corruptionComponent.isCorrupted)
						{
							_uncorruptedSelectedTiles.Add(tileFromMapCoordinatesRaw3);
						}
						DrawGridSelectionOnTile(tileFromMapCoordinatesRaw3);
						if (!flag2 && (tileFromMapCoordinatesRaw3.corruptionComponent.HasCorruptedNeighbour() || tileFromMapCoordinatesRaw3.corruptionComponent.isCorrupted))
						{
							flag2 = true;
						}
					}
				}
			}
			if (!flag2)
			{
				_selectedTiles.Clear();
				_uncorruptedSelectedTiles.Clear();
			}
		}
		else
		{
			for (int m = x; m <= x2; m++)
			{
				for (int n = y; n <= y2; n++)
				{
					LocationGridTile tileFromMapCoordinatesRaw4 = innerMap.GetTileFromMapCoordinatesRaw(m, n);
					if (CanTileBeSelectedForMultiSelection(tileFromMapCoordinatesRaw4, p_skill))
					{
						DrawGridSelectionOnTile(tileFromMapCoordinatesRaw4);
						_selectedTiles.Add(tileFromMapCoordinatesRaw4);
					}
				}
			}
		}
		ProcessSelectedTilesValidity(p_skill);
	}

	private void DrawGridSelectionOnTile(LocationGridTile p_tile)
	{
		SpriteRenderer gridSelectionGOFromPool = GetGridSelectionGOFromPool();
		gridSelectionGOFromPool.transform.SetParent(_gridSelectionParent);
		gridSelectionGOFromPool.transform.position = p_tile.centeredWorldLocation;
		gridSelectionGOFromPool.gameObject.SetActive(value: true);
		_activeGridSelections.Add(gridSelectionGOFromPool);
	}

	public bool CanTarget(LocationGridTile p_tile, SkillData p_skill)
	{
		string p_reason = string.Empty;
		return IsTileValidFor(p_tile, p_skill, ref p_reason);
	}

	public bool CanTarget(LocationGridTile p_tile, SkillData p_skill, ref string p_reason)
	{
		return IsTileValidFor(p_tile, p_skill, ref p_reason);
	}

	private bool CanTileBeSelectedForMultiSelection(LocationGridTile p_tile, SkillData p_skill)
	{
		if (p_tile == null)
		{
			return false;
		}
		if (p_skill.type == PLAYER_SKILL_TYPE.CORRUPT_TILE)
		{
			if (p_tile.corruptionComponent.isCorrupted)
			{
				return false;
			}
			if (p_tile.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				return false;
			}
			if (p_tile.HasVillageOrSpecialStructureNeighbour())
			{
				return false;
			}
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DEMONIC_WALL || p_skill.type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			TileObject objHere = p_tile.tileObjectComponent.objHere;
			if (objHere != null && (objHere.traitContainer.HasTrait("Indestructible") || objHere.tileObjectType.IsDemonicStructureTileObject()))
			{
				return false;
			}
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DEMOLISH && !p_tile.corruptionComponent.isCorrupted)
		{
			return false;
		}
		return true;
	}

	private bool IsTileValidFor(LocationGridTile p_tile, SkillData p_skill, ref string p_reason)
	{
		if (!p_skill.HasValidCharges())
		{
			p_reason = _notEnoughChargesInvalidString;
			return false;
		}
		if (!p_skill.HasEnoughSpiritEnergy(1))
		{
			p_reason = _notEnoughSpiritEnergyInvalidString;
			return false;
		}
		if (p_tile == null)
		{
			return false;
		}
		if (p_skill.type == PLAYER_SKILL_TYPE.CORRUPT_TILE)
		{
			if (p_tile.corruptionComponent.isCorrupted)
			{
				return false;
			}
			if (p_tile.structure.structureType != STRUCTURE_TYPE.WILDERNESS)
			{
				return false;
			}
			if (!p_tile.corruptionComponent.HasCorruptedNeighbour())
			{
				p_reason = _noCorruptedNeighbourInvalidString;
				return false;
			}
			if (p_tile.HasVillageOrSpecialStructureNeighbour())
			{
				return false;
			}
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DEMONIC_WALL || p_skill.type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			if (!p_tile.corruptionComponent.isCorrupted && !p_tile.corruptionComponent.HasCorruptedNeighbour())
			{
				p_reason = _cannotBuildOnNonCorruptedTileInvalidString;
				return false;
			}
			TileObject objHere = p_tile.tileObjectComponent.objHere;
			if (objHere != null && (objHere.traitContainer.HasTrait("Indestructible") || objHere.tileObjectType.IsDemonicStructureTileObject()))
			{
				return false;
			}
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DEMOLISH && !p_tile.corruptionComponent.isCorrupted)
		{
			p_reason = _cannotDemolishNonCorruptedTileInvalidString;
			return false;
		}
		return true;
	}

	private void ProcessSelectedTilesValidity(SkillData p_skill)
	{
		_areSelectedTilesValid = false;
		_invalidMessage = null;
		bool flag = p_skill.HasValidCharges(_selectedTiles.Count);
		bool flag2 = p_skill.HasEnoughSpiritEnergy(_selectedTiles.Count);
		string localizedName = p_skill.localizedName;
		if (_selectedTiles.Count > 0)
		{
			if (p_skill.type == PLAYER_SKILL_TYPE.CORRUPT_TILE)
			{
				bool flag3 = false;
				for (int i = 0; i < _selectedTiles.Count; i++)
				{
					if (_selectedTiles[i].corruptionComponent.HasCorruptedNeighbour())
					{
						flag3 = true;
						_areSelectedTilesValid = true;
						break;
					}
				}
				if (!flag3)
				{
					_invalidMessage = _noCorruptedNeighbourInvalidString;
				}
				_areSelectedTilesValid = flag3;
			}
			else if (p_skill.type == PLAYER_SKILL_TYPE.DEMONIC_WALL || p_skill.type == PLAYER_SKILL_TYPE.DECORATIONS)
			{
				_areSelectedTilesValid = true;
				SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CORRUPT_TILE);
				if (flag)
				{
					flag = skillData.HasValidCharges(_uncorruptedSelectedTiles.Count);
					if (!flag)
					{
						localizedName = skillData.localizedName;
					}
				}
				if (flag2)
				{
					flag2 = skillData.HasEnoughSpiritEnergy(_selectedTiles.Count + _uncorruptedSelectedTiles.Count);
				}
			}
			else if (p_skill.type == PLAYER_SKILL_TYPE.DEMOLISH)
			{
				_areSelectedTilesValid = true;
			}
			if (_areSelectedTilesValid)
			{
				if (!flag)
				{
					_areSelectedTilesValid = false;
					Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
					dictionary.Add("playerPowerName", localizedName);
					_invalidMessage = LocalizationSettings.StringDatabase.SmartFormatter.Format(_notEnoughChargesWithParameterInvalidString, dictionary);
					MaccimaDictionaryPool<string, string>.Release(dictionary);
				}
				else if (!flag2)
				{
					_areSelectedTilesValid = false;
					_invalidMessage = _notEnoughSpiritEnergyInvalidString;
				}
			}
		}
		UpdateGridCounter(_selectedTiles.Count, (flag && flag2) ? _textNeutralColor : _textInvalidColor);
		if (_areSelectedTilesValid)
		{
			_selectionBoxImg.color = _validColor;
			return;
		}
		_selectionBoxImg.color = _invalidColor;
		Color invalidColor = _invalidColor;
		invalidColor.a = 0.6f;
		for (int j = 0; j < _activeGridSelections.Count; j++)
		{
			_activeGridSelections[j].color = invalidColor;
		}
	}

	private void ProcessBuildSkillOnMouseRelease(SkillData p_skill)
	{
		if (!_areSelectedTilesValid || _selectedTiles.Count <= 0)
		{
			return;
		}
		if (p_skill.type == PLAYER_SKILL_TYPE.CORRUPT_TILE)
		{
			for (int i = 0; i < _selectedTiles.Count; i++)
			{
				_selectedTiles[i].corruptionComponent.CorruptTileAndDestroyDestructibleObject();
			}
			p_skill.ActivateAbility(_selectedTiles.Count);
			float num = _selectedTiles.Min((LocationGridTile t) => t.worldLocation.x);
			float num2 = _selectedTiles.Max((LocationGridTile t) => t.worldLocation.x);
			float num3 = _selectedTiles.Min((LocationGridTile t) => t.worldLocation.y);
			float num4 = _selectedTiles.Max((LocationGridTile t) => t.worldLocation.y);
			float num5 = num2 - num + 1f;
			float num6 = num4 - num3 + 1f;
			float num7 = num5 / 2f;
			float num8 = num6 / 2f;
			Vector2 vector = new Vector2(num + num7, num3 + num8);
			Bounds bounds = new Bounds(size: new Vector2(num5, num6), center: vector);
			PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(bounds);
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DEMONIC_WALL)
		{
			for (int num9 = 0; num9 < _uncorruptedSelectedTiles.Count; num9++)
			{
				_uncorruptedSelectedTiles[num9].corruptionComponent.CorruptTileAndDestroyDestructibleObject();
			}
			for (int num10 = 0; num10 < _selectedTiles.Count; num10++)
			{
				_selectedTiles[num10].corruptionComponent.BuildDemonicWallBase().SetIsBuiltByPlayerBaseBuilding(p_state: true);
			}
			if (_uncorruptedSelectedTiles.Count > 0)
			{
				PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CORRUPT_TILE).ActivateAbility(_uncorruptedSelectedTiles.Count);
			}
			p_skill.ActivateAbility(_selectedTiles.Count);
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			DecorationsData decorationsData = p_skill as DecorationsData;
			float rotation = (float)_currentDecorationRotationIndex * 90f;
			for (int num11 = 0; num11 < _uncorruptedSelectedTiles.Count; num11++)
			{
				_uncorruptedSelectedTiles[num11].corruptionComponent.CorruptTileAndDestroyDestructibleObject();
			}
			for (int num12 = 0; num12 < _selectedTiles.Count; num12++)
			{
				LocationGridTile locationGridTile = _selectedTiles[num12];
				TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(decorationsData.chosenTileObjectType);
				locationGridTile.structure.AddPOI(tileObject, locationGridTile);
				tileObject.mapVisual.SetVisual(InnerMapManager.Instance.GetCorruptedTileObjectAsset(tileObject.tileObjectType, POI_STATE.ACTIVE, p_getFirstAsset: true));
				tileObject.mapVisual.SetRotation(rotation);
				tileObject.SetIsBuiltByPlayerBaseBuilding(p_state: true);
			}
			if (_uncorruptedSelectedTiles.Count > 0)
			{
				PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.CORRUPT_TILE).ActivateAbility(_uncorruptedSelectedTiles.Count);
			}
			p_skill.ActivateAbility(_selectedTiles.Count);
		}
		else if (p_skill.type == PLAYER_SKILL_TYPE.DEMOLISH)
		{
			PlayerUI.Instance.ShowDemolishConfirmation(OnDemolish);
		}
	}

	private void ResetSelectedTiles()
	{
		_selectedTiles.Clear();
		_uncorruptedSelectedTiles.Clear();
		DestroyAllCurrentGridSelectionGOs();
	}

	private void ResetStartAndEndTile()
	{
		_startGridTile = null;
		_endGridTile = null;
	}

	public void ResetOnMouseRelease()
	{
		ResetStartAndEndTile();
		_selectionBoxImg.size = new Vector2(1f, 1f);
		_selectionBoxImg.gameObject.SetActive(value: false);
		DeactivateTileSelectionBoxVisual();
		DestroyAllCurrentGridSelectionGOs();
		HideBaseBuildingSelectionUI();
	}

	private void UpdateIconImage(SkillData p_skill)
	{
		if (p_skill is DecorationsData { chosenTileObjectType: not TILE_OBJECT_TYPE.NONE } decorationsData)
		{
			_buildIconImg.sprite = InnerMapManager.Instance.GetCorruptedTileObjectAsset(decorationsData.chosenTileObjectType, POI_STATE.ACTIVE, p_getFirstAsset: true);
		}
		else
		{
			_buildIconImg.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type).skillIcon;
		}
		Quaternion localRotation = Quaternion.Euler(0f, 0f, 0f);
		_buildIconImg.transform.localRotation = localRotation;
	}

	private void ShowBaseBuildingSelectionUI(LocationGridTile p_tile)
	{
		_baseBuildingSelectionUI.SetActive(p_tile != null);
		if (p_tile != null)
		{
			RepositionBaseBuildingSelectionUI();
		}
	}

	private void HideBaseBuildingSelectionUI()
	{
		_baseBuildingSelectionUI.SetActive(value: false);
	}

	private void RepositionBaseBuildingSelectionUI()
	{
		Vector3 position = new Vector3(InputManager.Instance.mousePosition.x + _selectionUIOffsetX, InputManager.Instance.mousePosition.y + _selectionUIOffsetY, 0f);
		_baseBuildingSelectionUI.transform.position = position;
	}

	private void UpdateGridCounter(int p_amount, Color p_color)
	{
		_gridCounterLbl.text = p_amount.ToString();
		_gridCounterLbl.color = p_color;
	}

	private void OnDemolish(bool p_includeCorruptedTiles, bool p_includeDemonicWalls, bool p_includeDecorations, bool p_includeStructures)
	{
		bool flag = true;
		if (p_includeCorruptedTiles)
		{
			flag = !PlayerManager.Instance.player.playerSettlement.AreThereMoreThanOneCorruptedIsland(_selectedTiles);
			if (!flag)
			{
				PlayerUI.Instance.ShowGeneralConfirmation(_demolishFailedTitle, _demolishFailedDescription);
			}
		}
		for (int i = 0; i < _selectedTiles.Count; i++)
		{
			_selectedTiles[i].corruptionComponent.BaseBuildingTileDemolition(p_includeStructures, p_includeDemonicWalls, p_includeDecorations, p_includeCorruptedTiles, flag);
		}
		PlayerUI.Instance.buildListUI.UpdateDecorationsInteractability();
	}

	private bool CanDoDemolishShortcut()
	{
		SkillData currentActivePlayerSpell = PlayerManager.Instance.player.currentActivePlayerSpell;
		if (currentActivePlayerSpell != null && currentActivePlayerSpell is DecorationsData { chosenTileObjectType: not TILE_OBJECT_TYPE.NONE })
		{
			return false;
		}
		return true;
	}

	private void DestroyAllCurrentGridSelectionGOs()
	{
		for (int i = 0; i < _activeGridSelections.Count; i++)
		{
			SendBackToPool(_activeGridSelections[i]);
		}
		_activeGridSelections.Clear();
	}

	private void InitializeObjectPooling()
	{
		for (int i = 0; i < 400; i++)
		{
			InstantiateNewGridSelectionGO();
		}
	}

	private void InstantiateNewGridSelectionGO()
	{
		GameObject gameObject = Object.Instantiate(_gridSelectionPrefab, _gridSelectionPoolParent);
		gameObject.transform.position = Vector3.zero;
		gameObject.SetActive(value: false);
		_gridSelectionPool.Enqueue(gameObject.GetComponent<SpriteRenderer>());
	}

	private SpriteRenderer GetGridSelectionGOFromPool()
	{
		if (_gridSelectionPool.Count <= 0)
		{
			InstantiateNewGridSelectionGO();
		}
		return _gridSelectionPool.Dequeue();
	}

	private void SendBackToPool(SpriteRenderer p_go)
	{
		p_go.gameObject.SetActive(value: false);
		p_go.transform.position = Vector3.zero;
		p_go.color = new Color(0.27f, 1f, 0f, 0.6f);
		p_go.transform.SetParent(_gridSelectionPoolParent);
		_gridSelectionPool.Enqueue(p_go);
	}
}
