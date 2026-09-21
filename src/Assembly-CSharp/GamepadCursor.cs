using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class GamepadCursor : MonoBehaviour
{
	[SerializeField]
	private PlayerInput playerInput;

	[SerializeField]
	private RectTransform gamePadCursorTransform;

	[SerializeField]
	private float cursorSpeed;

	[SerializeField]
	private RectTransform _canvasRectTransform;

	[SerializeField]
	private Image gamepadImg;

	[SerializeField]
	private Sprite gamepadDefaultSprite;

	[SerializeField]
	private Sprite gamepadValidSprite;

	[SerializeField]
	private Sprite gamepadInvalidSprite;

	private bool _previousMouseState;

	private Mouse _virtualMouse;

	public Vector2 position => _virtualMouse?.position.ReadValue() ?? Vector2.zero;

	public bool isEnabled => gamePadCursorTransform.gameObject.activeSelf;

	public Mouse virtualMouse => _virtualMouse;

	private void OnEnable()
	{
		if (_virtualMouse == null)
		{
			_virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
		}
		else if (!_virtualMouse.added)
		{
			InputSystem.AddDevice(_virtualMouse);
		}
		SetCursor(Cursor_Type.Default);
		InputUser.PerformPairingWithDevice(_virtualMouse, playerInput.user);
		if (gamePadCursorTransform != null)
		{
			Vector2 anchoredPosition = gamePadCursorTransform.anchoredPosition;
			InputState.Change(_virtualMouse.position, anchoredPosition);
		}
		InputSystem.onAfterUpdate += UpdateMotion;
	}

	private void OnDisable()
	{
		if (_virtualMouse != null && _virtualMouse.added)
		{
			if (playerInput.user.valid)
			{
				playerInput.user.UnpairDevice(_virtualMouse);
			}
			InputSystem.RemoveDevice(_virtualMouse);
			_virtualMouse = null;
		}
		InputSystem.onAfterUpdate -= UpdateMotion;
	}

	private void UpdateMotion()
	{
		if (_virtualMouse != null && Gamepad.current != null && gamePadCursorTransform.gameObject.activeSelf)
		{
			Vector2 vector = Gamepad.current.leftStick.ReadValue();
			vector *= cursorSpeed * Time.deltaTime;
			Vector2 vector2 = _virtualMouse.position.ReadValue() + vector;
			vector2.x = Mathf.Clamp(vector2.x, 0f, Screen.width);
			vector2.y = Mathf.Clamp(vector2.y, 0f, Screen.height);
			InputState.Change(_virtualMouse.position, vector2);
			InputState.Change(_virtualMouse.delta, vector);
			AnchorCursor(vector2);
		}
	}

	public void OnLeftClick(bool p_state)
	{
		if (isEnabled)
		{
			_virtualMouse.CopyState<MouseState>(out var state);
			state.WithButton(MouseButton.Left, p_state);
			InputState.Change(_virtualMouse, state);
		}
	}

	private void AnchorCursor(Vector2 newPos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, newPos, null, out var localPoint);
		gamePadCursorTransform.anchoredPosition = localPoint;
	}

	public void Enable(Vector2 mousePos)
	{
		if (!gamePadCursorTransform.gameObject.activeSelf)
		{
			gamePadCursorTransform.gameObject.SetActive(value: true);
			InputState.Change(_virtualMouse.position, mousePos);
			AnchorCursor(mousePos);
		}
	}

	public void Enable()
	{
		if (!gamePadCursorTransform.gameObject.activeSelf)
		{
			gamePadCursorTransform.gameObject.SetActive(value: true);
		}
	}

	public void Disable()
	{
		if (gamePadCursorTransform.gameObject.activeSelf)
		{
			gamePadCursorTransform.gameObject.SetActive(value: false);
		}
	}

	public void SetCursor(Cursor_Type p_type)
	{
		switch (p_type)
		{
		case Cursor_Type.Check:
			gamepadImg.sprite = gamepadValidSprite;
			break;
		case Cursor_Type.Cross:
			gamepadImg.sprite = gamepadInvalidSprite;
			break;
		default:
			gamepadImg.sprite = gamepadDefaultSprite;
			break;
		}
	}
}
