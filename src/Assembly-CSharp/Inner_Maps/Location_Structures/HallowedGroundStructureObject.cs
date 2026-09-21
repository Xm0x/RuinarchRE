using UnityEngine;
using UnityEngine.Tilemaps;

namespace Inner_Maps.Location_Structures;

public class HallowedGroundStructureObject : LocationStructureObject
{
	[SerializeField]
	private ParticleSystem[] _particles;

	[SerializeField]
	private TilemapRenderer demonGround;

	[SerializeField]
	private TilemapRenderer divineGround;

	[SerializeField]
	private TilemapRenderer natureGround;

	protected override void Awake()
	{
		base.Awake();
		int sortingOrder = 11;
		demonGround.sortingOrder = sortingOrder;
		divineGround.sortingOrder = sortingOrder;
		natureGround.sortingOrder = sortingOrder;
	}

	public void SetParticlesColor(Color color)
	{
		for (int i = 0; i < _particles.Length; i++)
		{
			ParticleSystem.MainModule main = _particles[i].main;
			main.startColor = color;
		}
	}

	public void UpdateGroundVisualBasedOnReligion(RELIGION p_religion)
	{
		switch (p_religion)
		{
		case RELIGION.Demon_Worship:
			demonGround.gameObject.SetActive(value: true);
			divineGround.gameObject.SetActive(value: false);
			natureGround.gameObject.SetActive(value: false);
			break;
		case RELIGION.Divine_Worship:
			demonGround.gameObject.SetActive(value: false);
			divineGround.gameObject.SetActive(value: true);
			natureGround.gameObject.SetActive(value: false);
			break;
		case RELIGION.Nature_Worship:
			demonGround.gameObject.SetActive(value: false);
			divineGround.gameObject.SetActive(value: false);
			natureGround.gameObject.SetActive(value: true);
			break;
		default:
			demonGround.gameObject.SetActive(value: false);
			divineGround.gameObject.SetActive(value: false);
			natureGround.gameObject.SetActive(value: false);
			break;
		}
	}
}
