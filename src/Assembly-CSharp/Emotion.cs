using System.Linq;
using UtilityScripts;

public class Emotion
{
	private string _localizedName;

	private string _localizedResponse;

	protected string _localizedResponseKey;

	public string name { get; private set; }

	public EMOTION emotionType { get; private set; }

	public string[] mutuallyExclusive { get; protected set; }

	public string localizedName => _localizedName;

	public string localizedResponse => _localizedResponse;

	public Emotion(EMOTION emotionType)
	{
		this.emotionType = emotionType;
		name = Utilities.NotNormalizedConversionEnumToString(emotionType.ToString());
		_localizedResponseKey = name + "_Response";
		_localizedName = LocalizationManager.Instance.GetLocalizedValue("Emotions_Table", name);
		_localizedResponse = LocalizationManager.Instance.GetLocalizedValue("Emotions_Table", _localizedResponseKey);
	}

	public virtual string ProcessEmotion(Character p_witness, IPointOfInterest p_target, REACTION_STATUS p_status, ref int p_totalOpinionReduction, ref string p_lastStrawReasonKey, ActualGoapNode p_goapNode = null, string p_reason = "", bool p_triggerOpinionChangesEffect = false)
	{
		return localizedResponse;
	}

	public bool IsEmotionCompatibleWithThis(string emotionName)
	{
		if (mutuallyExclusive != null)
		{
			return !mutuallyExclusive.Contains(emotionName);
		}
		return true;
	}
}
public enum EMOTION
{
	None,
	Fear,
	Approval,
	Embarassment,
	Disgust,
	Anger,
	Betrayal,
	Concern,
	Disappointment,
	Scorn,
	Sadness,
	Threatened,
	Arousal,
	Disinterest,
	Despair,
	Shock,
	Resentment,
	Disapproval,
	Gratefulness,
	Rage,
	Plague_Hysteria,
	Distraught,
	Repulsed
}
