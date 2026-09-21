using System.Collections.Generic;
using UnityEngine;

namespace Necromancy.UI;

[RequireComponent(typeof(ParticleSystem))]
public class TextRendererParticleSystem : MonoBehaviour
{
	public SymbolsTextureData textureData;

	private ParticleSystemRenderer particleSystemRenderer;

	private ParticleSystem particleSystem;

	[ContextMenu("TestText")]
	public void TestText()
	{
		SpawnParticle(base.transform.position, "+600", Color.red, 10f);
	}

	public void Stop()
	{
		if (particleSystem != null)
		{
			particleSystem.Stop(withChildren: true);
			particleSystem.Clear();
		}
	}

	public void SpawnParticle(Vector3 position, float amount, Color color, float? startSize = null)
	{
		int num = Mathf.RoundToInt(amount);
		if (num != 0)
		{
			string text = num.ToString();
			if (num > 0)
			{
				text = "+" + text;
			}
			SpawnParticle(position, text, color, startSize);
		}
	}

	public void SpawnParticle(Vector3 position, int amount, Color color, float? startSize = null)
	{
		if (amount != 0)
		{
			string text = amount.ToString();
			if (amount > 0)
			{
				text = "+" + text;
			}
			SpawnParticle(position, text, color, startSize);
		}
	}

	public void SpawnParticle(Vector3 position, string message, Color color, float? startSize = null)
	{
		Vector2[] array = new Vector2[24];
		int num = Mathf.Min(23, message.Length);
		array[array.Length - 1] = new Vector2(0f, num);
		for (int i = 0; i < array.Length && i < num; i++)
		{
			array[i] = textureData.GetTextureCoordinates(message[i]);
		}
		Vector4 value = CreateCustomData(array);
		Vector4 value2 = CreateCustomData(array, 12);
		if (particleSystem == null)
		{
			particleSystem = GetComponent<ParticleSystem>();
		}
		if (particleSystemRenderer == null)
		{
			particleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
			List<ParticleSystemVertexStream> list = new List<ParticleSystemVertexStream>();
			particleSystemRenderer.GetActiveVertexStreams(list);
			if (!list.Contains(ParticleSystemVertexStream.UV2))
			{
				list.Add(ParticleSystemVertexStream.UV2);
			}
			if (!list.Contains(ParticleSystemVertexStream.Custom1XYZW))
			{
				list.Add(ParticleSystemVertexStream.Custom1XYZW);
			}
			if (!list.Contains(ParticleSystemVertexStream.Custom2XYZW))
			{
				list.Add(ParticleSystemVertexStream.Custom2XYZW);
			}
			particleSystemRenderer.SetActiveVertexStreams(list);
		}
		ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
		{
			startColor = color,
			position = position,
			applyShapeToPosition = true,
			startSize3D = new Vector3(num, 1f, 1f)
		};
		if (startSize.HasValue)
		{
			emitParams.startSize3D *= startSize.Value * particleSystem.main.startSizeMultiplier;
		}
		particleSystem.Emit(emitParams, 1);
		List<Vector4> list2 = new List<Vector4>();
		particleSystem.GetCustomParticleData(list2, ParticleSystemCustomData.Custom1);
		list2[list2.Count - 1] = value;
		particleSystem.SetCustomParticleData(list2, ParticleSystemCustomData.Custom1);
		particleSystem.GetCustomParticleData(list2, ParticleSystemCustomData.Custom2);
		list2[list2.Count - 1] = value2;
		particleSystem.SetCustomParticleData(list2, ParticleSystemCustomData.Custom2);
	}

	public float PackFloat(Vector2[] vecs)
	{
		if (vecs == null || vecs.Length == 0)
		{
			return 0f;
		}
		float num = vecs[0].y * 10000f + vecs[0].x * 100000f;
		if (vecs.Length > 1)
		{
			num += vecs[1].y * 100f + vecs[1].x * 1000f;
		}
		if (vecs.Length > 2)
		{
			num += vecs[2].y + vecs[2].x * 10f;
		}
		return num;
	}

	private Vector4 CreateCustomData(Vector2[] texCoords, int offset = 0)
	{
		Vector4 zero = Vector4.zero;
		for (int i = 0; i < 4; i++)
		{
			Vector2[] array = new Vector2[3];
			for (int j = 0; j < 3; j++)
			{
				int num = i * 3 + j + offset;
				if (texCoords.Length > num)
				{
					array[j] = texCoords[num];
					continue;
				}
				zero[i] = PackFloat(array);
				i = 5;
				break;
			}
			if (i < 4)
			{
				zero[i] = PackFloat(array);
			}
		}
		return zero;
	}
}
