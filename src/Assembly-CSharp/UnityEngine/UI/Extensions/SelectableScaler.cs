using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Selectable Scalar")]
[RequireComponent(typeof(Button))]
public class SelectableScaler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public AnimationCurve animCurve;

	[Tooltip("Animation speed multiplier")]
	public float speed = 1f;

	private Vector3 initScale;

	public Transform target;

	private Selectable selectable;

	public Selectable Target
	{
		get
		{
			if (selectable == null)
			{
				selectable = GetComponent<Selectable>();
			}
			return selectable;
		}
	}

	private void Awake()
	{
		if (target == null)
		{
			target = base.transform;
		}
		initScale = target.localScale;
	}

	private void OnEnable()
	{
		target.localScale = initScale;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!(Target != null) || Target.interactable)
		{
			StopCoroutine("ScaleOUT");
			StartCoroutine("ScaleIN");
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!(Target != null) || Target.interactable)
		{
			StopCoroutine("ScaleIN");
			StartCoroutine("ScaleOUT");
		}
	}

	private IEnumerator ScaleIN()
	{
		if (animCurve.keys.Length != 0)
		{
			target.localScale = initScale;
			float t = 0f;
			float maxT = animCurve.keys[animCurve.length - 1].time;
			while (t < maxT)
			{
				t += speed * Time.unscaledDeltaTime;
				target.localScale = Vector3.one * animCurve.Evaluate(t);
				yield return null;
			}
		}
	}

	private IEnumerator ScaleOUT()
	{
		if (animCurve.keys.Length != 0)
		{
			float t = 0f;
			float maxT = animCurve.keys[animCurve.length - 1].time;
			while (t < maxT)
			{
				t += speed * Time.unscaledDeltaTime;
				target.localScale = Vector3.one * animCurve.Evaluate(maxT - t);
				yield return null;
			}
			base.transform.localScale = initScale;
		}
	}
}
