using TMPro;
using UnityEngine;

public class DialogItem : MonoBehaviour
{
	public enum Position
	{
		Left,
		Right
	}

	[SerializeField]
	private RectTransform portraitRT;

	[SerializeField]
	private CharacterPortrait portrait;

	public RectTransform characterDialogParent;

	[Header("Left Dialog")]
	[SerializeField]
	private GameObject leftGO;

	[SerializeField]
	private TextMeshProUGUI leftText;

	[Header("Right Dialog")]
	[SerializeField]
	private GameObject rightGO;

	[SerializeField]
	private TextMeshProUGUI rightText;

	[SerializeField]
	private Vector2 leftPortraitPos;

	[SerializeField]
	private Vector2 rightPortraitPos;

	public Position positionTest;

	public void SetData(Character character, string text, Position position = Position.Left)
	{
		if (character != null)
		{
			portrait.enabled = true;
			portrait.GeneratePortrait(character);
			portrait.ignoreInteractions = true;
		}
		else
		{
			portrait.SetAsDefaultMinion();
			portrait.enabled = false;
		}
		if (position == Position.Left)
		{
			leftText.text = text;
		}
		else
		{
			rightText.text = text;
		}
		AlignToPosition(position);
	}

	private void AlignToPosition(Position position)
	{
		switch (position)
		{
		case Position.Left:
			rightGO.SetActive(value: false);
			leftGO.SetActive(value: true);
			portraitRT.anchoredPosition = leftPortraitPos;
			break;
		case Position.Right:
			rightGO.SetActive(value: true);
			leftGO.SetActive(value: false);
			portraitRT.anchoredPosition = rightPortraitPos;
			break;
		}
	}

	[ContextMenu("Alignment test")]
	public void AlignmentTest()
	{
		AlignToPosition(positionTest);
	}
}
