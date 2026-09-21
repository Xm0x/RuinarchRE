using System.Text;
using BayatGames.SaveGameFree.Encoders;
using BayatGames.SaveGameFree.Serializers;
using BayatGames.SaveGameFree.Types;
using UnityEngine;

namespace BayatGames.SaveGameFree;

[AddComponentMenu("Save Game Free/Auto Save")]
public class SaveGameAuto : MonoBehaviour
{
	public enum SaveFormat
	{
		XML,
		JSON,
		Binary
	}

	[Header("Settings")]
	[Space]
	[Tooltip("You must specify a value for this to be able to save it.")]
	public string positionIdentifier = "enter the position identifier";

	[Tooltip("You must specify a value for this to be able to save it.")]
	public string rotationIdentifier = "enter the rotation identifier";

	[Tooltip("You must specify a value for this to be able to save it.")]
	public string scaleIdentifier = "enter the scale identifier";

	[Tooltip("Encode the data?")]
	public bool encode;

	[Tooltip("If you leave it blank this will reset to it's default value.")]
	public string encodePassword = "";

	[Tooltip("Which serialization format?")]
	public SaveFormat format = SaveFormat.JSON;

	[Tooltip("If you leave it blank this will reset to it's default value.")]
	public ISaveGameSerializer serializer;

	[Tooltip("If you leave it blank this will reset to it's default value.")]
	public ISaveGameEncoder encoder;

	[Tooltip("If you leave it blank this will reset to it's default value.")]
	public Encoding encoding;

	[Tooltip("Where to save? (PersistentDataPath highly recommended).")]
	public SaveGamePath savePath;

	[Tooltip("Reset the empty fields to their default value.")]
	public bool resetBlanks = true;

	[Header("What to Save?")]
	[Space]
	[Tooltip("Save Position?")]
	public bool savePosition = true;

	[Tooltip("Save Rotation?")]
	public bool saveRotation = true;

	[Tooltip("Save Scale?")]
	public bool saveScale = true;

	[Header("Defaults")]
	[Space]
	[Tooltip("Default Position Value")]
	public Vector3 defaultPosition = Vector3.zero;

	[Tooltip("Default Rotation Value")]
	public Vector3 defaultRotation = Quaternion.identity.eulerAngles;

	[Tooltip("Default Scale Value")]
	public Vector3 defaultScale = Vector3.one;

	[Header("Save Events")]
	[Space]
	[Tooltip("Save on Awake()")]
	public bool saveOnAwake;

	[Tooltip("Save on Start()")]
	public bool saveOnStart;

	[Tooltip("Save on OnEnable()")]
	public bool saveOnEnable;

	[Tooltip("Save on OnDisable()")]
	public bool saveOnDisable = true;

	[Tooltip("Save on OnApplicationQuit()")]
	public bool saveOnApplicationQuit = true;

	[Tooltip("Save on OnApplicationPause()")]
	public bool saveOnApplicationPause;

	[Header("Load Events")]
	[Space]
	[Tooltip("Load on Awake()")]
	public bool loadOnAwake;

	[Tooltip("Load on Start()")]
	public bool loadOnStart = true;

	[Tooltip("Load on OnEnable()")]
	public bool loadOnEnable;

	protected virtual void Awake()
	{
		if (resetBlanks)
		{
			if (string.IsNullOrEmpty(encodePassword))
			{
				encodePassword = SaveGame.EncodePassword;
			}
			if (serializer == null)
			{
				serializer = SaveGame.Serializer;
			}
			if (encoder == null)
			{
				encoder = SaveGame.Encoder;
			}
			if (encoding == null)
			{
				encoding = SaveGame.DefaultEncoding;
			}
		}
		switch (format)
		{
		case SaveFormat.Binary:
			serializer = new SaveGameBinarySerializer();
			break;
		case SaveFormat.JSON:
			serializer = new SaveGameJsonSerializer();
			break;
		case SaveFormat.XML:
			serializer = new SaveGameXmlSerializer();
			break;
		}
		if (loadOnAwake)
		{
			Load();
		}
		if (saveOnAwake)
		{
			Save();
		}
	}

	protected virtual void Start()
	{
		if (loadOnStart)
		{
			Load();
		}
		if (saveOnStart)
		{
			Save();
		}
	}

	protected virtual void OnEnable()
	{
		if (loadOnEnable)
		{
			Load();
		}
		if (saveOnEnable)
		{
			Save();
		}
	}

	protected virtual void OnDisable()
	{
		if (saveOnDisable)
		{
			Save();
		}
	}

	protected virtual void OnApplicationQuit()
	{
		if (saveOnApplicationQuit)
		{
			Save();
		}
	}

	protected virtual void OnApplicationPause()
	{
		if (saveOnApplicationPause)
		{
			Save();
		}
	}

	public virtual void Save()
	{
		if (savePosition)
		{
			SaveGame.Save(positionIdentifier, (Vector3Save)base.transform.position, encode, encodePassword, serializer, encoder, encoding, savePath);
		}
		if (saveRotation)
		{
			SaveGame.Save(rotationIdentifier, (QuaternionSave)base.transform.rotation, encode, encodePassword, serializer, encoder, encoding, savePath);
		}
		if (saveScale)
		{
			SaveGame.Save(scaleIdentifier, (Vector3Save)base.transform.localScale, encode, encodePassword, serializer, encoder, encoding, savePath);
		}
	}

	public virtual void Load()
	{
		if (savePosition)
		{
			base.transform.position = SaveGame.Load(positionIdentifier, (Vector3Save)defaultPosition, encode, encodePassword, serializer, encoder, encoding, savePath);
		}
		if (saveRotation)
		{
			base.transform.rotation = SaveGame.Load(rotationIdentifier, (QuaternionSave)Quaternion.Euler(defaultRotation), encode, encodePassword, serializer, encoder, encoding, savePath);
		}
		if (saveScale)
		{
			base.transform.localScale = SaveGame.Load(scaleIdentifier, (Vector3Save)defaultScale, encode, encodePassword, serializer, encoder, encoding, savePath);
		}
	}
}
