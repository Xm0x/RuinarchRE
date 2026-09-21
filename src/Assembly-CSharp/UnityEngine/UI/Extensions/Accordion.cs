namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup), typeof(ContentSizeFitter), typeof(ToggleGroup))]
[AddComponentMenu("UI/Extensions/Accordion/Accordion Group")]
public class Accordion : MonoBehaviour
{
	public enum Transition
	{
		Instant,
		Tween
	}

	private bool m_expandVertical = true;

	[SerializeField]
	private Transition m_Transition;

	[SerializeField]
	private float m_TransitionDuration = 0.3f;

	[HideInInspector]
	public bool ExpandVerticval => m_expandVertical;

	public Transition transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public float transitionDuration
	{
		get
		{
			return m_TransitionDuration;
		}
		set
		{
			m_TransitionDuration = value;
		}
	}

	private void Awake()
	{
		m_expandVertical = !GetComponent<HorizontalLayoutGroup>();
		GetComponent<ToggleGroup>();
	}
}
