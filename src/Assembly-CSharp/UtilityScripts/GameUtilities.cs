using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inner_Maps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UtilityScripts;

public static class GameUtilities
{
	public static Stopwatch stopwatch = new Stopwatch();

	private static List<LocationGridTile> listHolder = new List<LocationGridTile>();

	public static string Filtered_Object_Layer = "Filtered Object";

	public static string Unfiltered_Object_Layer = "Non Filtered Object";

	public static string Filtered_Vision_Layer = "Filtered Vision";

	public static string Unfiltered_Vision_Layer = "Non Filtered Vision";

	public static int Line_Of_Sight_Layer_Mask = LayerMask.GetMask("Unpassable", Filtered_Object_Layer, Unfiltered_Object_Layer);

	public static int Unpassable_Layer_Mask = LayerMask.GetMask("Unpassable");

	public static int Filtered_Layer_Mask = LayerMask.GetMask(Filtered_Object_Layer);

	public static string Discord_Link = "http://discord.gg/86C3p7h";

	public static WaitForSeconds waitForTenthOfSecond = new WaitForSeconds(0.1f);

	public static WaitForSeconds waitForQuarterOfSecond = new WaitForSeconds(0.25f);

	public static WaitForSeconds waitForHalfSecond = new WaitForSeconds(0.5f);

	public static WaitForSeconds waitFor1Second = new WaitForSeconds(1f);

	public static WaitForSeconds waitFor2Seconds = new WaitForSeconds(2f);

	public static WaitForSeconds waitFor3Seconds = new WaitForSeconds(3f);

	public static WaitForSeconds waitFor5Seconds = new WaitForSeconds(5f);

	public static WaitForSeconds waitFor1AndHalfSecond = new WaitForSeconds(1.5f);

	public static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	public static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

	public static Color CaveGroundMinimapColor = Color.grey;

	public static Color CaveWallMinimapColor = Color.black;

	public static Color DeepWaterMinimapColor = new Color(0f, 0.23529412f, 0.42745098f);

	public static Color MidWaterMinimapColor = new Color(0f, 0.38431373f, 0.69803923f);

	public static Color ShallowWaterMinimapColor = new Color(0f, 64f / 85f, 0.8392157f);

	public static Color ShoreWaterMinimapColor = new Color(0f, 1f, 1f);

	public static Color CorruptedMinimapColor = new Color(0.32156864f, 0.2627451f, 2f / 3f);

	public static Color StructureMinimapColor = new Color(0.69803923f, 25f / 51f, 0.17254902f);

	private static readonly Color _grayedOutColor = new Color(0.41568628f, 0.41568628f, 0.5019608f);

	private static readonly Color _normalColor = new Color(0.972549f, 0.88235295f, 0.6627451f);

	public static BIOMES[] customWorldBiomeChoices = new BIOMES[4]
	{
		BIOMES.GRASSLAND,
		BIOMES.FOREST,
		BIOMES.DESERT,
		BIOMES.SNOW
	};

	public static FACTION_TYPE[] customWorldFactionTypeChoices = new FACTION_TYPE[2]
	{
		FACTION_TYPE.Human_Empire,
		FACTION_TYPE.Elven_Kingdom
	};

	public static TILE_OBJECT_TYPE[] corruptionTileObjectChoices = new TILE_OBJECT_TYPE[6]
	{
		TILE_OBJECT_TYPE.CORRUPTED_TENDRIL,
		TILE_OBJECT_TYPE.CORRUPTED_SPIKE,
		TILE_OBJECT_TYPE.DEMON_CIRCLE,
		TILE_OBJECT_TYPE.SPAWNING_PIT,
		TILE_OBJECT_TYPE.SIGIL,
		TILE_OBJECT_TYPE.SMALL_TREE_OBJECT
	};

	public static List<STRUCTURE_TYPE> skinnerStructures = new List<STRUCTURE_TYPE>
	{
		STRUCTURE_TYPE.BOAR_DEN,
		STRUCTURE_TYPE.WOLF_DEN,
		STRUCTURE_TYPE.BEAR_DEN,
		STRUCTURE_TYPE.RABBIT_HOLE,
		STRUCTURE_TYPE.MINK_HOLE,
		STRUCTURE_TYPE.MOONCRAWLER_HOLE
	};

	private static readonly HashSet<RACE> _beastRaces = new HashSet<RACE>
	{
		RACE.DRAGON,
		RACE.WOLF,
		RACE.SPIDER,
		RACE.GOLEM,
		RACE.SHEEP,
		RACE.PIG,
		RACE.CHICKEN,
		RACE.BEAST,
		RACE.RAT
	};

	private static List<int> cornersOutside = new List<int>();

	private static Vector3[] corners = new Vector3[4];

	public static string GetNormalizedSingularRace(RACE race)
	{
		return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", race.ToStringEnum() + "_Singular");
	}

	public static string GetNormalizedRaceAdjective(string race)
	{
		race = race.ToUpper();
		return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", race + "_Adjective");
	}

	public static string GetNormalizedRaceAdjective(RACE race)
	{
		return LocalizationManager.Instance.GetLocalizedValue("CharacterGeneric_Table", race.ToStringEnum() + "_Adjective");
	}

	public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
	{
		screenPosition1.y = (float)Screen.height - screenPosition1.y;
		screenPosition2.y = (float)Screen.height - screenPosition2.y;
		Vector3 vector = Vector3.Min(screenPosition1, screenPosition2);
		Vector3 vector2 = Vector3.Max(screenPosition1, screenPosition2);
		return Rect.MinMaxRect(vector.x, vector.y, vector2.x, vector2.y);
	}

	public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
	{
		Vector3 lhs = Camera.main.ScreenToViewportPoint(screenPosition1);
		Vector3 rhs = Camera.main.ScreenToViewportPoint(screenPosition2);
		Vector3 min = Vector3.Min(lhs, rhs);
		Vector3 max = Vector3.Max(lhs, rhs);
		min.z = camera.nearClipPlane;
		max.z = camera.farClipPlane;
		Bounds result = default(Bounds);
		result.SetMinMax(min, max);
		return result;
	}

	public static bool IsVisibleFrom(Renderer renderer, Camera camera)
	{
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), renderer.bounds);
	}

	public static int GetTicksInBetweenDates(GameDate date1, GameDate date2)
	{
		int num = Mathf.Abs(date1.year - date2.year);
		int num2 = Mathf.Abs(date1.month - date2.month);
		int num3 = Mathf.Abs(date1.day - date2.day);
		int num4 = date2.tick - date1.tick;
		return num * 172800 + num2 * 14400 + num3 * 480 + num4;
	}

	public static LocationGridTile GetCenterTile(List<LocationGridTile> tiles, LocationGridTile[,] map)
	{
		int num = tiles.Min((LocationGridTile t) => t.localPlace.x);
		int num2 = tiles.Max((LocationGridTile t) => t.localPlace.x);
		int num3 = tiles.Min((LocationGridTile t) => t.localPlace.y);
		int num4 = tiles.Max((LocationGridTile t) => t.localPlace.y);
		int num5 = num2 - num;
		int num6 = num4 - num3;
		int num7 = num + num5 / 2;
		int num8 = num3 + num6 / 2;
		return map[num7, num8];
	}

	public static LocationGridTile GetCenterTile(HashSet<LocationGridTile> tiles, LocationGridTile[,] map)
	{
		int num = tiles.Min((LocationGridTile t) => t.localPlace.x);
		int num2 = tiles.Max((LocationGridTile t) => t.localPlace.x);
		int num3 = tiles.Min((LocationGridTile t) => t.localPlace.y);
		int num4 = tiles.Max((LocationGridTile t) => t.localPlace.y);
		int num5 = num2 - num;
		int num6 = num4 - num3;
		int num7 = num + num5 / 2;
		int num8 = num3 + num6 / 2;
		return map[num7, num8];
	}

	public static string GetRespectiveBeastClassNameFromByRace(RACE race)
	{
		return race switch
		{
			RACE.GOLEM => "Abomination", 
			RACE.DRAGON => "Dragon", 
			RACE.SPIDER => "Spinner", 
			RACE.WOLF => "Ravager", 
			_ => throw new Exception($"No beast class for {race} Race!"), 
		};
	}

	public static bool IsRaceBeast(RACE race)
	{
		return _beastRaces.Contains(race);
	}

	public static T[] GetComponentsInDirectChildren<T>(GameObject gameObject)
	{
		int num = 0;
		foreach (Transform item in gameObject.transform)
		{
			if (item.GetComponent<T>() != null)
			{
				num++;
			}
		}
		T[] array = new T[num];
		num = 0;
		foreach (Transform item2 in gameObject.transform)
		{
			if (item2.GetComponent<T>() != null)
			{
				array[num++] = item2.GetComponent<T>();
			}
		}
		return array;
	}

	public static int GetOptionIndex(Dropdown dropdown, string option)
	{
		for (int i = 0; i < dropdown.options.Count; i++)
		{
			if (dropdown.options[i].text.Equals(option))
			{
				return i;
			}
		}
		return -1;
	}

	public static int GetOptionIndex(TMP_Dropdown dropdown, string option)
	{
		for (int i = 0; i < dropdown.options.Count; i++)
		{
			if (dropdown.options[i].text.Equals(option))
			{
				return i;
			}
		}
		return -1;
	}

	public static GameObject FindParentWithTag(GameObject childObject, string tag)
	{
		Transform transform = childObject.transform;
		while (transform.parent != null)
		{
			if (transform.parent.tag == tag)
			{
				return transform.parent.gameObject;
			}
			transform = transform.parent.transform;
		}
		return null;
	}

	public static List<LocationGridTile> GetDiamondTilesFromRadius(InnerTileMap map, Vector3Int center, int radius)
	{
		listHolder.Clear();
		int num = center.y - radius;
		int num2 = center.y + radius;
		int num3 = 0;
		for (int i = center.y; i <= num2; i++)
		{
			int num4 = center.x - radius + num3;
			int num5 = center.x + radius - num3;
			for (int j = num4; j <= num5; j++)
			{
				if (Utilities.IsInRange(j, 0, map.width) && Utilities.IsInRange(i, 0, map.height))
				{
					LocationGridTile item = map.map[j, i];
					listHolder.Add(item);
				}
			}
			num3++;
		}
		num3 = 1;
		for (int num6 = center.y - 1; num6 >= num; num6--)
		{
			int num7 = center.x - radius + num3;
			int num8 = center.x + radius - num3;
			for (int k = num7; k <= num8; k++)
			{
				if (Utilities.IsInRange(k, 0, map.width) && Utilities.IsInRange(num6, 0, map.height))
				{
					LocationGridTile item2 = map.map[k, num6];
					listHolder.Add(item2);
				}
			}
			num3++;
		}
		return listHolder;
	}

	public static void HighlightTiles(List<LocationGridTile> tiles, Color color)
	{
		for (int i = 0; i < tiles.Count; i++)
		{
			LocationGridTile locationGridTile = tiles[i];
			locationGridTile.parentMap.groundTilemap.SetColor(locationGridTile.localPlace, color);
		}
	}

	public static Color InvertColor(Color color)
	{
		return new Color(1f - color.r, 1f - color.g, 1f - color.b);
	}

	public static Vector2 VectorSubtraction(Vector2 a, Vector2 b)
	{
		Vector3 vector = a;
		vector.x = a.x - b.x;
		vector.y = a.y - b.y;
		return vector;
	}

	public static int Roll()
	{
		return UnityEngine.Random.Range(0, 100);
	}

	public static float RollFloat()
	{
		return UnityEngine.Random.Range(0f, 100f);
	}

	public static bool RollChance(int chance, ref string log)
	{
		return UnityEngine.Random.Range(0, 100) < chance;
	}

	public static bool RollChanceThreadSafe(int chance, ref string log)
	{
		return Utilities.Rng.Next(0, 101) < chance;
	}

	public static bool RollChance(int chance)
	{
		return UnityEngine.Random.Range(0, 100) < chance;
	}

	public static bool RollChance(float chance)
	{
		chance *= 100f;
		return (float)UnityEngine.Random.Range(0, 10000) < chance;
	}

	public static bool RollChance(float chance, ref string log)
	{
		chance *= 100f;
		return (float)UnityEngine.Random.Range(0, 10000) < chance;
	}

	public static int RandomBetweenTwoNumbers(int p_min, int p_max)
	{
		return UnityEngine.Random.Range(p_min, p_max + 1);
	}

	public static List<int> GetUniqueRandomNumbersInBetween(int p_min, int p_max, int p_count)
	{
		return (from n in Enumerable.Range(p_min, p_max)
			orderby n * n + UnityEngine.Random.Range(p_min, p_max) * new System.Random().Next()
			select n).Distinct().Take(p_count).ToList();
	}

	public static void PopulateAreasGivenCoordinates(List<Area> areas, List<Point> coordinates, Area[,] map)
	{
		for (int i = 0; i < coordinates.Count; i++)
		{
			Point point = coordinates[i];
			Area item = map[point.X, point.Y];
			areas.Add(item);
		}
	}

	public static List<Area> GetHexTilesGivenCoordinates(Point[] coordinates, Area[,] map)
	{
		List<Area> list = new List<Area>();
		for (int i = 0; i < coordinates.Length; i++)
		{
			Point point = coordinates[i];
			Area item = map[point.X, point.Y];
			list.Add(item);
		}
		return list;
	}

	public static Area GetHexTileGivenCoordinates(Point point, Area[,] map)
	{
		return map[point.X, point.Y];
	}

	public static Color GetUpgradeButtonTextColor(bool p_interactable)
	{
		if (!p_interactable)
		{
			return _grayedOutColor;
		}
		return _normalColor;
	}

	public static void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT, Cursor_Type cursorType, RectTransform canvasRT)
	{
		Vector3 position2 = position;
		rtToReposition.pivot = new Vector2(0f, 1f);
		if (cursorType == Cursor_Type.Cross || cursorType == Cursor_Type.Check || cursorType == Cursor_Type.Link)
		{
			position2.x += 100f;
			position2.y -= 32f;
		}
		else
		{
			position2.x += 25f;
			position2.y -= 25f;
		}
		rtToReposition.transform.position = position2;
		cornersOutside.Clear();
		boundsRT.GetWorldCorners(corners);
		for (int i = 0; i < 4; i++)
		{
			Vector3 position3 = corners[i];
			Vector3 point = canvasRT.InverseTransformPoint(position3);
			if (!canvasRT.rect.Contains(point))
			{
				cornersOutside.Add(i);
			}
		}
		if (cornersOutside.Count == 0)
		{
			return;
		}
		if (cornersOutside.Contains(2) && cornersOutside.Contains(3))
		{
			if (cornersOutside.Contains(0))
			{
				rtToReposition.pivot = new Vector2(1f, 0f);
			}
			else
			{
				rtToReposition.pivot = new Vector2(1f, 1f);
			}
		}
		else if (cornersOutside.Contains(0) && cornersOutside.Contains(3))
		{
			rtToReposition.pivot = new Vector2(0f, 0f);
		}
	}

	public static bool IsRectFullyInCanvas(RectTransform boundsRT, RectTransform canvasRT)
	{
		cornersOutside.Clear();
		boundsRT.GetWorldCorners(corners);
		for (int i = 0; i < 4; i++)
		{
			Vector3 position = corners[i];
			Vector3 point = canvasRT.InverseTransformPoint(position);
			if (!canvasRT.rect.Contains(point))
			{
				cornersOutside.Add(i);
			}
		}
		if (cornersOutside.Count != 0)
		{
			if (cornersOutside.Contains(2) && cornersOutside.Contains(3))
			{
				cornersOutside.Contains(0);
				return false;
			}
			if (cornersOutside.Contains(0) && cornersOutside.Contains(3))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsRectFullyInCanvas(RectTransform boundsRT, Rect canvasRT)
	{
		cornersOutside.Clear();
		boundsRT.GetWorldCorners(corners);
		for (int i = 0; i < 4; i++)
		{
			Vector3 point = corners[i];
			if (!canvasRT.Contains(point))
			{
				cornersOutside.Add(i);
			}
		}
		return cornersOutside.Count == 0;
	}

	public static Color GetValidTileHighlightColor()
	{
		Color green = Color.green;
		green.a = 0.3f;
		return green;
	}

	public static Color GetInvalidTileHighlightColor()
	{
		Color red = Color.red;
		red.a = 0.3f;
		return red;
	}

	public static bool IsInLineOfSight(IPointOfInterest p_target, Vector2 p_origin, float p_distance, int p_layerMask, RaycastHit2D[] p_lineOfSightHitObjects)
	{
		if (p_target == null)
		{
			return false;
		}
		if (!(p_target is GenericTileObject) && p_target.mapObjectVisual == null)
		{
			return false;
		}
		Vector3 vector = VectorSubtraction(p_target.worldPosition, p_origin).normalized;
		if (p_target.IsUnpassable())
		{
			p_distance += 1.5f;
		}
		Physics2D.RaycastNonAlloc(p_origin, vector, p_lineOfSightHitObjects, p_distance, p_layerMask);
		if (p_target is GenericTileObject)
		{
			if (p_lineOfSightHitObjects != null)
			{
				for (int i = 0; i < p_lineOfSightHitObjects.Length; i++)
				{
					RaycastHit2D raycastHit2D = p_lineOfSightHitObjects[i];
					if (!p_target.IsUnpassable() && raycastHit2D.collider != null && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Unpassable"))
					{
						return false;
					}
				}
			}
			return true;
		}
		if (p_lineOfSightHitObjects != null)
		{
			for (int j = 0; j < p_lineOfSightHitObjects.Length; j++)
			{
				RaycastHit2D raycastHit2D2 = p_lineOfSightHitObjects[j];
				if (!p_target.IsUnpassable() && raycastHit2D2.collider != null && raycastHit2D2.collider.gameObject.layer == LayerMask.NameToLayer("Unpassable"))
				{
					return false;
				}
				if (raycastHit2D2.transform != null && raycastHit2D2.transform.IsChildOf(p_target.mapObjectVisual.transform))
				{
					return true;
				}
			}
		}
		return false;
	}
}
