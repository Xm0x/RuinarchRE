using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(Selectable))]
public class StepperSide : UIBehaviour, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	private Selectable button => GetComponent<Selectable>();

	private Stepper stepper => GetComponentInParent<Stepper>();

	private bool leftmost => button == stepper.sides[0];

	protected StepperSide()
	{
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Press();
		}
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		Press();
	}

	private void Press()
	{
		if (button.IsActive() && button.IsInteractable())
		{
			if (leftmost)
			{
				stepper.StepDown();
			}
			else
			{
				stepper.StepUp();
			}
		}
	}
}
