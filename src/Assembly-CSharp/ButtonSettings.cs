using Ruinarch.Custom_UI;
using UnityEngine.UI;

public class ButtonSettings
{
	private readonly bool _interactable;

	private readonly Selectable.Transition _transition;

	private readonly Graphic _targetGraphic;

	private SpriteState _spriteState;

	private ColorBlock _colorBlock;

	private Navigation _navigation;

	private readonly Button.ButtonClickedEvent _onClickAction;

	public ButtonSettings(Button button)
	{
		_interactable = button.interactable;
		_transition = button.transition;
		_targetGraphic = button.targetGraphic;
		_spriteState = button.spriteState;
		_colorBlock = button.colors;
		_navigation = button.navigation;
		_onClickAction = button.onClick;
	}

	public void ApplySettings(RuinarchButton ruinarchButton)
	{
		ruinarchButton.interactable = _interactable;
		ruinarchButton.transition = _transition;
		ruinarchButton.targetGraphic = _targetGraphic;
		ruinarchButton.spriteState = _spriteState;
		ruinarchButton.colors = _colorBlock;
		ruinarchButton.navigation = _navigation;
		ruinarchButton.onClick = _onClickAction;
	}
}
