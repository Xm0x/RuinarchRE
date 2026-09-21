using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class ArtifactItem : PooledObject
{
	public TextMeshProUGUI artifactButtonText;

	public Toggle artifactToggle;

	public ARTIFACT_TYPE artifact { get; private set; }

	public void SetArtifact(ARTIFACT_TYPE artifact)
	{
		this.artifact = artifact;
		UpdateData();
		Messenger.AddListener<ARTIFACT_TYPE>(PlayerSignals.PLAYER_NO_ACTIVE_ARTIFACT, OnPlayerNoActiveArtifact);
	}

	private void UpdateData()
	{
		artifactButtonText.text = artifact.ToStringEnumWithSpace();
	}

	private void OnPlayerNoActiveArtifact(ARTIFACT_TYPE artifact)
	{
		if (this.artifact == artifact && artifactToggle.isOn)
		{
			artifactToggle.isOn = false;
		}
	}

	public void OnToggleArtifact(bool state)
	{
		PlayerManager.Instance.player.SetCurrentlyActiveArtifact(ARTIFACT_TYPE.None);
		if (state)
		{
			PlayerManager.Instance.player.SetCurrentlyActiveArtifact(artifact);
		}
	}

	public override void Reset()
	{
		base.Reset();
		artifact = ARTIFACT_TYPE.None;
		Messenger.RemoveListener<ARTIFACT_TYPE>(PlayerSignals.PLAYER_NO_ACTIVE_ARTIFACT, OnPlayerNoActiveArtifact);
	}
}
