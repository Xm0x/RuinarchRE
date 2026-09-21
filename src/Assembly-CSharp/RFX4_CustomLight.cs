using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class RFX4_CustomLight : MonoBehaviour
{
	private static int MaxLightsCount = 4;

	private Texture2D PointLightAttenuation;

	private List<Light> sceneLights;

	private void Awake()
	{
		sceneLights = Object.FindObjectsOfType<Light>().ToList();
		PointLightAttenuation = GeneratePointAttenuationTexture();
		Shader.SetGlobalTexture("RFX4_PointLightAttenuation", PointLightAttenuation);
	}

	private void Update()
	{
		List<Light> allLights = GetAllLights();
		int num = 0;
		List<Vector4> list = new List<Vector4>();
		List<Vector4> list2 = new List<Vector4>();
		num += FillDirectionalLights(allLights, list, list2);
		allLights = SortPointLightsByDistance(allLights);
		num += FillPointLights(allLights, list, list2);
		Shader.SetGlobalInt("RFX4_LightCount", num);
		Shader.SetGlobalVectorArray("RFX4_LightPositions", ListToArrayWithMaxCount(list, MaxLightsCount));
		Shader.SetGlobalVectorArray("RFX4_LightColors", ListToArrayWithMaxCount(list2, MaxLightsCount));
		Color value = SampleLightProbesUp(base.transform.position, 0.5f);
		Shader.SetGlobalColor("RFX4_AmbientColor", value);
	}

	private void OnDisable()
	{
		Shader.SetGlobalInt("RFX4_LightCount", 0);
		Shader.SetGlobalVectorArray("RFX4_LightPositions", new Vector4[1] { Vector4.zero });
		Shader.SetGlobalVectorArray("RFX4_LightColors", new Vector4[1] { Vector4.zero });
		Shader.SetGlobalColor("RFX4_AmbientColor", Color.black);
	}

	private List<Light> GetAllLights()
	{
		List<Light> list = base.transform.root.GetComponentsInChildren<Light>().ToList();
		foreach (Light sceneLight in sceneLights)
		{
			if (sceneLight != null)
			{
				list.Add(sceneLight);
			}
		}
		return list;
	}

	private int FillDirectionalLights(List<Light> lights, List<Vector4> lightPositions, List<Vector4> lightColors)
	{
		int num = 0;
		for (int i = 0; i < lights.Count; i++)
		{
			if (lights[i].isActiveAndEnabled && lights[i].type == LightType.Directional)
			{
				Vector3 vector = -lights[i].transform.forward;
				lightPositions.Add(new Vector4(vector.x, vector.y, vector.z, 0f));
				Color color = lights[i].color * lights[i].intensity;
				lightColors.Add(new Vector4(color.r, color.g, color.b, 0f));
				num++;
			}
		}
		return num;
	}

	private int FillPointLights(List<Light> lights, List<Vector4> lightPositions, List<Vector4> lightColors)
	{
		int num = 0;
		for (int i = 0; i < lights.Count; i++)
		{
			if (lights[i].isActiveAndEnabled && lights[i].type == LightType.Point)
			{
				Vector3 position = lights[i].transform.position;
				lightPositions.Add(new Vector4(position.x, position.y, position.z, lights[i].range));
				Color color = lights[i].color * lights[i].intensity;
				lightColors.Add(new Vector4(color.r, color.g, color.b, 1f));
				num++;
			}
		}
		return num;
	}

	private Vector4[] ListToArrayWithMaxCount(List<Vector4> list, int count)
	{
		Vector4[] array = new Vector4[count];
		for (int i = 0; i < count; i++)
		{
			if (list.Count > i)
			{
				array[i] = list[i];
			}
			else
			{
				array[i] = Vector4.zero;
			}
		}
		return array;
	}

	private List<Light> SortPointLightsByDistance(List<Light> lights)
	{
		Vector3 position = base.transform.position;
		SortedDictionary<float, Light> sortedDictionary = new SortedDictionary<float, Light>();
		foreach (Light light in lights)
		{
			float key = (position - light.transform.position).magnitude + Random.Range(-10000f, 10000f) / 1000000f;
			if (!sortedDictionary.ContainsKey(key))
			{
				sortedDictionary.Add(key, light);
			}
		}
		return sortedDictionary.Values.ToList();
	}

	public Color SampleLightProbesUp(Vector3 pos, float grayScaleFactor)
	{
		LightProbes.GetInterpolatedProbe(pos, null, out var probe);
		Vector4 a = new Vector4(probe[0, 3], probe[0, 1], probe[0, 2], probe[0, 0] - probe[0, 6]);
		Vector4 a2 = new Vector4(probe[1, 3], probe[1, 1], probe[1, 2], probe[1, 0] - probe[1, 6]);
		Vector4 a3 = new Vector4(probe[2, 3], probe[2, 1], probe[2, 2], probe[2, 0] - probe[2, 6]);
		Vector4 a4 = new Vector4(probe[0, 4], probe[0, 6], probe[0, 5] * 3f, probe[0, 7]);
		Vector4 a5 = new Vector4(probe[1, 4], probe[1, 6], probe[1, 5] * 3f, probe[1, 7]);
		Vector4 a6 = new Vector4(probe[2, 4], probe[2, 6], probe[2, 5] * 3f, probe[2, 7]);
		Vector3 vector = new Vector3(probe[0, 8], probe[2, 8], probe[1, 8]);
		Vector4 b = new Vector4(0f, 1f, 0f, 1f);
		Color black = Color.black;
		black.r = Vector4.Dot(a, b);
		black.g = Vector4.Dot(a2, b);
		black.b = Vector4.Dot(a3, b);
		Vector4 b2 = new Vector4(b.x * b.y, b.y * b.z, b.z * b.z, b.z * b.x);
		Color black2 = Color.black;
		black2.r = Vector4.Dot(a4, b2);
		black2.g = Vector4.Dot(a5, b2);
		black2.b = Vector4.Dot(a6, b2);
		float num = b.x * b.x - b.y * b.y;
		Vector3 vector2 = vector * num;
		Color color = new Color(vector2.x, vector2.y, vector2.z);
		Color a7 = black + black2 + color;
		float num2 = a7.r * 0.33f + a7.g * 0.33f + a7.b * 0.33f;
		a7 = Color.Lerp(a7, Color.white * num2, grayScaleFactor);
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			return (black + black2 + color).gamma;
		}
		return a7;
	}

	private Texture2D GeneratePointAttenuationTexture()
	{
		Texture2D texture2D = new Texture2D(256, 1);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < 256; i++)
		{
			float num = (float)i / 256f;
			float num2 = Mathf.Clamp01(1f / (1f + 25f * num * num) * Mathf.Clamp01((1f - num) * 5f));
			texture2D.SetPixel(i, 0, Color.white * num2);
		}
		texture2D.Apply();
		return texture2D;
	}
}
