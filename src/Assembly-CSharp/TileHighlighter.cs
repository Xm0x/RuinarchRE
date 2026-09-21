using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class TileHighlighter : MonoBehaviour
{
	public static TileHighlighter Instance;

	[SerializeField]
	private Transform parentTransform;

	[SerializeField]
	private ParticleSystem[] _particleSystems;

	[SerializeField]
	private SpriteRenderer innerImage;

	[SerializeField]
	private BiomeHighlightColorDictionary _biomeHighlightColor;

	private void Awake()
	{
		Instance = this;
	}

	private void SetupHighlight(int radius, BIOMES biome)
	{
		int num = radius * 2;
		num = ((!Utilities.IsEven(num)) ? (num - 1) : (num + 1));
		if (num == 0)
		{
			num = 1;
		}
		innerImage.gameObject.SetActive(value: false);
		Vector3 scale = new Vector3(num, 1f, num);
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			ParticleSystem obj = _particleSystems[i];
			ParticleSystem.ShapeModule shape = obj.shape;
			shape.scale = scale;
			obj.GetComponent<ParticleSystemRenderer>().material = _biomeHighlightColor[biome];
			int maxParticles;
			int num2;
			if (biome == BIOMES.SNOW)
			{
				maxParticles = num * 80;
				num2 = num * 40;
			}
			else
			{
				maxParticles = num * 25;
				num2 = num * 10;
			}
			ParticleSystem.MainModule main = obj.main;
			main.maxParticles = maxParticles;
			ParticleSystem.EmissionModule emission = obj.emission;
			emission.rateOverTime = num2;
		}
	}

	private void SetupHighlight(int radius, BIOMES biome, Color innerImageColor)
	{
		SetupHighlight(radius, biome);
		innerImage.color = innerImageColor;
		innerImage.gameObject.SetActive(value: true);
	}

	public void PositionHighlight(int radius, LocationGridTile centerTile)
	{
		SetupHighlight(radius, centerTile.mainBiomeType);
		parentTransform.transform.position = centerTile.centeredWorldLocation;
		parentTransform.gameObject.SetActive(value: true);
	}

	public void PositionHighlight(int radius, LocationGridTile centerTile, Color innerImageColor)
	{
		SetupHighlight(radius, centerTile.mainBiomeType);
		parentTransform.transform.position = centerTile.centeredWorldLocation;
		parentTransform.gameObject.SetActive(value: true);
		Color color = innerImageColor;
		color.a = 0.5019608f;
		innerImage.color = color;
		innerImage.gameObject.SetActive(value: true);
	}

	public void PositionHighlight(Area p_area)
	{
		SetupHighlight(InnerMapManager.AreaLocationGridTileSize.x / 2 - 1, p_area.gridTileComponent.centerGridTile.mainBiomeType);
		parentTransform.transform.position = p_area.worldPosition;
		parentTransform.gameObject.SetActive(value: true);
	}

	public void PositionHighlight(Area p_area, Color color)
	{
		SetupHighlight(InnerMapManager.AreaLocationGridTileSize.x / 2 - 1, p_area.gridTileComponent.centerGridTile.mainBiomeType, color);
		parentTransform.transform.position = p_area.worldPosition;
		parentTransform.gameObject.SetActive(value: true);
	}

	public void HideHighlight()
	{
		parentTransform.gameObject.SetActive(value: false);
	}
}
