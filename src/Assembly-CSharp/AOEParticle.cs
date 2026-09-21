using EZObjectPools;
using Inner_Maps;
using UnityEngine;

public class AOEParticle : PooledObject
{
	[SerializeField]
	private ParticleSystem[] particles;

	public void PlaceParticleEffect(LocationGridTile tile, int range, bool isAutoDestroy)
	{
		base.transform.SetParent(tile.parentMap.objectsParent);
		base.transform.localPosition = tile.centeredLocalLocation;
		for (int i = 0; i < particles.Length; i++)
		{
			ParticleSystem particleSystem = particles[i];
			if (!isAutoDestroy)
			{
				ParticleSystem.MainModule main = particleSystem.main;
				main.startLifetime = new ParticleSystem.MinMaxCurve(range + 2, range + 4);
			}
			float num = 1 + 2 * range;
			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.scale = new Vector3(num, num, num);
		}
	}
}
