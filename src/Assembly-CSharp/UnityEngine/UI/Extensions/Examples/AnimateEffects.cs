namespace UnityEngine.UI.Extensions.Examples;

public class AnimateEffects : MonoBehaviour
{
	public LetterSpacing letterSpacing;

	private float letterSpacingMax = 10f;

	private float letterSpacingMin = -10f;

	private float letterSpacingModifier = 0.1f;

	public CurvedText curvedText;

	private float curvedTextMax = 0.05f;

	private float curvedTextMin = -0.05f;

	private float curvedTextModifier = 0.001f;

	public Gradient2 gradient2;

	private float gradient2Max = 1f;

	private float gradient2Min = -1f;

	private float gradient2Modifier = 0.01f;

	public CylinderText cylinderText;

	private Transform cylinderTextRT;

	private Vector3 cylinderRotation = new Vector3(0f, 1f, 0f);

	public SoftMaskScript SAUIM;

	private float SAUIMMax = 1f;

	private float SAUIMMin;

	private float SAUIMModifier = 0.01f;

	private void Start()
	{
		cylinderTextRT = cylinderText.GetComponent<Transform>();
	}

	private void Update()
	{
		letterSpacing.spacing += letterSpacingModifier;
		if (letterSpacing.spacing > letterSpacingMax || letterSpacing.spacing < letterSpacingMin)
		{
			letterSpacingModifier = 0f - letterSpacingModifier;
		}
		curvedText.CurveMultiplier += curvedTextModifier;
		if (curvedText.CurveMultiplier > curvedTextMax || curvedText.CurveMultiplier < curvedTextMin)
		{
			curvedTextModifier = 0f - curvedTextModifier;
		}
		gradient2.Offset += gradient2Modifier;
		if (gradient2.Offset > gradient2Max || gradient2.Offset < gradient2Min)
		{
			gradient2Modifier = 0f - gradient2Modifier;
		}
		cylinderTextRT.Rotate(cylinderRotation);
		SAUIM.CutOff += SAUIMModifier;
		if (SAUIM.CutOff > SAUIMMax || SAUIM.CutOff < SAUIMMin)
		{
			SAUIMModifier = 0f - SAUIMModifier;
		}
	}
}
