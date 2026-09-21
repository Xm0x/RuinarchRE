using EZObjectPools;
using UnityEngine;
using UtilityScripts;

public class EyeWardHighlight : PooledObject
{
	[SerializeField]
	private ParticleSystem[] _particleSystems;

	public void SetupHighlight(int radius)
	{
		int num = radius * 2;
		num = ((!Utilities.IsEven(num)) ? (num - 1) : (num + 1));
		if (num == 0)
		{
			num = 1;
		}
		Vector3 scale = new Vector3(num, num, 0f);
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			ParticleSystem.ShapeModule shape = _particleSystems[i].shape;
			shape.scale = scale;
		}
	}

	public void HideHighlight()
	{
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Stop();
		}
		base.gameObject.SetActive(value: false);
	}

	public void ShowHighlight()
	{
		for (int i = 0; i < _particleSystems.Length; i++)
		{
			_particleSystems[i].Play();
		}
		base.gameObject.SetActive(value: true);
	}
}
