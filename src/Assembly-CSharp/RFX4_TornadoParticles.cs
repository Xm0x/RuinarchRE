using UnityEngine;

public class RFX4_TornadoParticles : MonoBehaviour
{
	public Material TornadoMaterial;

	private ParticleSystem.Particle[] particleArray;

	private ParticleSystem particleSys;

	private Light myLight;

	private Vector4 _twistScale;

	private int materialID = -1;

	private void Start()
	{
		particleSys = GetComponent<ParticleSystem>();
		myLight = GetComponent<Light>();
		if (particleSys != null)
		{
			particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];
		}
		if (TornadoMaterial.HasProperty("_TwistScale"))
		{
			materialID = Shader.PropertyToID("_TwistScale");
		}
		else
		{
			Debug.Log(TornadoMaterial.name + " not have property twist");
		}
		if (materialID != -1)
		{
			_twistScale = TornadoMaterial.GetVector(materialID);
		}
	}

	private void Update()
	{
		if (materialID != -1)
		{
			_twistScale = TornadoMaterial.GetVector(materialID);
		}
		if (particleSys != null)
		{
			int particles = particleSys.GetParticles(particleArray);
			for (int i = 0; i < particles; i++)
			{
				Vector3 position = particleArray[i].position;
				float num = (position.y - base.transform.position.y) * _twistScale.y;
				position.x = Mathf.Sin(Time.time * _twistScale.z + position.y * _twistScale.x) * num;
				position.z = Mathf.Sin(Time.time * _twistScale.z + position.y * _twistScale.x + 1.57075f) * num;
				particleArray[i].position = position;
				particleSys.SetParticles(particleArray, particles);
			}
		}
		if (myLight != null)
		{
			Vector3 localPosition = base.transform.localPosition;
			float num2 = localPosition.y * _twistScale.y;
			localPosition.x = Mathf.Sin(Time.time * _twistScale.z + localPosition.y * _twistScale.x) * num2;
			localPosition.z = Mathf.Sin(Time.time * _twistScale.z + localPosition.y * _twistScale.x + 1.57075f) * num2;
			base.transform.localPosition = localPosition;
		}
	}
}
