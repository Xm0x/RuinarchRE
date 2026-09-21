using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BayatGames.SaveGameFree.Encoders;
using BayatGames.SaveGameFree.Serializers;
using UnityEngine;
using UnityEngine.Networking;

namespace BayatGames.SaveGameFree;

public class SaveGameWeb
{
	private static string m_DefaultUsername = "savegamefree";

	private static string m_DefaultPassword = "$@ve#game%free";

	private static string m_DefaultURL = "http://www.example.com";

	private static bool m_DefaultEncode = false;

	private static string m_DefaultEncodePassword = "h@e#ll$o%^";

	private static ISaveGameSerializer m_DefaultSerializer = new SaveGameJsonSerializer();

	private static ISaveGameEncoder m_DefaultEncoder = new SaveGameSimpleEncoder();

	private static Encoding m_DefaultEncoding = Encoding.UTF8;

	protected string m_Username;

	protected string m_Password;

	protected string m_URL;

	protected bool m_Encode;

	protected string m_EncodePassword;

	protected ISaveGameSerializer m_Serializer;

	protected ISaveGameEncoder m_Encoder;

	protected Encoding m_Encoding;

	protected UnityWebRequest m_Request;

	protected bool m_IsError;

	protected string m_Error = "";

	public static string DefaultUsername
	{
		get
		{
			return m_DefaultUsername;
		}
		set
		{
			m_DefaultUsername = value;
		}
	}

	public static string DefaultPassword
	{
		get
		{
			return m_DefaultPassword;
		}
		set
		{
			m_DefaultPassword = value;
		}
	}

	public static string DefaultURL
	{
		get
		{
			return m_DefaultURL;
		}
		set
		{
			m_DefaultURL = value;
		}
	}

	public static bool DefaultEncode
	{
		get
		{
			return m_DefaultEncode;
		}
		set
		{
			m_DefaultEncode = value;
		}
	}

	public static string DefaultEncodePassword
	{
		get
		{
			return m_DefaultEncodePassword;
		}
		set
		{
			m_DefaultEncodePassword = value;
		}
	}

	public static ISaveGameSerializer DefaultSerializer
	{
		get
		{
			if (m_DefaultSerializer == null)
			{
				m_DefaultSerializer = new SaveGameJsonSerializer();
			}
			return m_DefaultSerializer;
		}
		set
		{
			m_DefaultSerializer = value;
		}
	}

	public static ISaveGameEncoder DefaultEncoder
	{
		get
		{
			if (m_DefaultEncoder == null)
			{
				m_DefaultEncoder = new SaveGameSimpleEncoder();
			}
			return m_DefaultEncoder;
		}
		set
		{
			m_DefaultEncoder = value;
		}
	}

	public static Encoding DefaultEncoding
	{
		get
		{
			if (m_DefaultEncoding == null)
			{
				m_DefaultEncoding = Encoding.UTF8;
			}
			return m_DefaultEncoding;
		}
		set
		{
			m_DefaultEncoding = value;
		}
	}

	public virtual string Username
	{
		get
		{
			return m_Username;
		}
		set
		{
			m_Username = value;
		}
	}

	public virtual string Password
	{
		get
		{
			return m_Password;
		}
		set
		{
			m_Password = value;
		}
	}

	public virtual string URL
	{
		get
		{
			return m_URL;
		}
		set
		{
			m_URL = value;
		}
	}

	public virtual bool Encode
	{
		get
		{
			return m_Encode;
		}
		set
		{
			m_Encode = value;
		}
	}

	public virtual string EncodePassword
	{
		get
		{
			return m_EncodePassword;
		}
		set
		{
			m_EncodePassword = value;
		}
	}

	public virtual ISaveGameSerializer Serializer
	{
		get
		{
			if (m_Serializer == null)
			{
				m_Serializer = new SaveGameJsonSerializer();
			}
			return m_Serializer;
		}
		set
		{
			m_Serializer = value;
		}
	}

	public virtual ISaveGameEncoder Encoder
	{
		get
		{
			if (m_Encoder == null)
			{
				m_Encoder = new SaveGameSimpleEncoder();
			}
			return m_Encoder;
		}
		set
		{
			m_Encoder = value;
		}
	}

	public virtual Encoding Encoding
	{
		get
		{
			if (m_Encoding == null)
			{
				m_Encoding = Encoding.UTF8;
			}
			return m_Encoding;
		}
		set
		{
			m_Encoding = value;
		}
	}

	public virtual UnityWebRequest Request => m_Request;

	public virtual bool IsError => m_IsError;

	public virtual string Error => m_Error;

	public SaveGameWeb()
		: this(DefaultUsername)
	{
	}

	public SaveGameWeb(string username)
		: this(username, DefaultPassword)
	{
	}

	public SaveGameWeb(string username, string password)
		: this(username, password, DefaultURL)
	{
	}

	public SaveGameWeb(string username, string password, string url)
		: this(username, password, url, DefaultEncode)
	{
	}

	public SaveGameWeb(string username, string password, string url, bool encode)
		: this(username, password, url, encode, DefaultEncodePassword)
	{
	}

	public SaveGameWeb(string username, string password, string url, bool encode, string encodePassword)
		: this(username, password, url, encode, encodePassword, DefaultSerializer)
	{
	}

	public SaveGameWeb(string username, string password, string url, bool encode, string encodePassword, ISaveGameSerializer serializer)
		: this(username, password, url, encode, encodePassword, serializer, DefaultEncoder)
	{
	}

	public SaveGameWeb(string username, string password, string url, bool encode, string encodePassword, ISaveGameSerializer serializer, ISaveGameEncoder encoder)
		: this(username, password, url, encode, encodePassword, serializer, encoder, DefaultEncoding)
	{
	}

	public SaveGameWeb(string username, string password, string url, bool encode, string encodePassword, ISaveGameSerializer serializer, ISaveGameEncoder encoder, Encoding encoding)
	{
		m_Username = username;
		m_Password = password;
		m_URL = url;
		m_Encode = encode;
		m_EncodePassword = encodePassword;
		m_Serializer = serializer;
		m_Encoder = encoder;
		m_Encoding = encoding;
	}

	public virtual IEnumerator Save<T>(string identifier, T obj)
	{
		MemoryStream memoryStream = new MemoryStream();
		Serializer.Serialize(obj, memoryStream, Encoding);
		string text = Encoding.GetString(memoryStream.ToArray());
		if (Encode)
		{
			text = Encoder.Encode(text, EncodePassword);
		}
		yield return Send(identifier, text, "save");
		if (m_IsError)
		{
			Debug.LogError(m_Error);
		}
		else
		{
			Debug.Log("Data successfully saved.");
		}
	}

	public virtual IEnumerator Download(string identifier)
	{
		yield return Send(identifier, null, "load");
		if (m_IsError)
		{
			Debug.LogError(m_Error);
		}
		else
		{
			Debug.Log("Data successfully downloaded.");
		}
	}

	public virtual T Load<T>(string identifier)
	{
		return Load(identifier, default(T));
	}

	public virtual T Load<T>(string identifier, T defaultValue)
	{
		if (defaultValue == null)
		{
			defaultValue = default(T);
		}
		T val = defaultValue;
		if (!m_IsError && !string.IsNullOrEmpty(m_Request.downloadHandler.text))
		{
			string text = m_Request.downloadHandler.text;
			if (Encode)
			{
				text = Encoder.Decode(text, EncodePassword);
			}
			MemoryStream memoryStream = new MemoryStream(Encoding.GetBytes(text));
			val = Serializer.Deserialize<T>(memoryStream, Encoding);
			memoryStream.Dispose();
			if (val == null)
			{
				val = defaultValue;
			}
		}
		return val;
	}

	public virtual IEnumerator Send(string identifier, string data, string action)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "identifier", identifier },
			{ "action", action },
			{ "username", Username }
		};
		if (!string.IsNullOrEmpty(data))
		{
			dictionary.Add("data", data);
		}
		if (!string.IsNullOrEmpty(Password))
		{
			dictionary.Add("password", Password);
		}
		m_Request = UnityWebRequest.Post(URL, dictionary);
		yield return m_Request.SendWebRequest();
		if (m_Request.isNetworkError || m_Request.isHttpError)
		{
			m_IsError = true;
			m_Error = m_Request.error;
		}
		else if (m_Request.downloadHandler.text.StartsWith("Error"))
		{
			m_IsError = true;
			m_Error = m_Request.downloadHandler.text;
		}
		else
		{
			m_IsError = false;
		}
	}
}
