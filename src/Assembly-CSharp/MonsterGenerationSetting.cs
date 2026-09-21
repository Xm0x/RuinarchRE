using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Generation Setting", menuName = "Scriptable Objects/Monster Generation")]
public class MonsterGenerationSetting : ScriptableObject
{
	[SerializeField]
	private IntRange _iterations;

	public IntRange iterations => _iterations;
}
