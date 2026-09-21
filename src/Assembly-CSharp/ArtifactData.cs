using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact Data", menuName = "Scriptable Objects/Artifact Data")]
public class ArtifactData : ScriptableObject
{
	[SerializeField]
	private ARTIFACT_TYPE _type;

	[SerializeField]
	private string _name;

	[SerializeField]
	private Sprite _sprite;

	[SerializeField]
	private Sprite _portrait;

	[SerializeField]
	private ArtifactUnlockable[] _unlocks;

	[SerializeField]
	private TileObjectScriptableObject _tileObjectScriptableObject;

	public ARTIFACT_TYPE type => _type;

	public string internalName => _name;

	public Sprite sprite => _sprite;

	public ArtifactUnlockable[] unlocks => _unlocks;

	public Sprite portrait => _portrait;

	public TileObjectScriptableObject tileObjectScriptableObject => _tileObjectScriptableObject;
}
