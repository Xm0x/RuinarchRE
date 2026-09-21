using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class CharacterThoughtTooltip : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private TextMeshProUGUI thoughtLbl;

	public Character activeCharacter { get; private set; }

	public void Show(Character character)
	{
		activeCharacter = character;
		base.gameObject.SetActive(value: true);
		UpdateText(character);
		rectTransform.SetAsLastSibling();
		Reposition(character);
	}

	private void Update()
	{
		if (activeCharacter != null)
		{
			UpdateText(activeCharacter);
			Reposition(activeCharacter);
		}
	}

	public void Hide()
	{
		activeCharacter = null;
		base.gameObject.SetActive(value: false);
	}

	private void Reposition([NotNull] Character character)
	{
		Vector3 position = InnerMapCameraMove.Instance.camera.WorldToScreenPoint(character.marker.transform.position);
		float num = (InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV) / 3.5f;
		position.y -= 125f - num * 12f;
		rectTransform.position = position;
	}

	private void UpdateText([NotNull] Character character)
	{
		thoughtLbl.text = character.visuals.GetThoughtBubble();
	}
}
