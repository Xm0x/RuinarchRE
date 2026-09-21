using System;
using System.IO;
using System.Text;
using BayatGames.SaveGameFree.Encoders;
using BayatGames.SaveGameFree.Serializers;
using UnityEngine;

namespace BayatGames.SaveGameFree;

public static class SaveGame
{
	public delegate void SaveHandler(object obj, string identifier, bool encode, string password, ISaveGameSerializer serializer, ISaveGameEncoder encoder, Encoding encoding, SaveGamePath path);

	public delegate void LoadHandler(object loadedObj, string identifier, bool encode, string password, ISaveGameSerializer serializer, ISaveGameEncoder encoder, Encoding encoding, SaveGamePath path);

	public static SaveHandler SaveCallback;

	public static LoadHandler LoadCallback;

	private static ISaveGameSerializer m_Serializer = new SaveGameJsonSerializer();

	private static ISaveGameEncoder m_Encoder = new SaveGameSimpleEncoder();

	private static Encoding m_Encoding = Encoding.UTF8;

	private static bool m_Encode = false;

	private static SaveGamePath m_SavePath = SaveGamePath.PersistentDataPath;

	private static string m_EncodePassword = "h@e#ll$o%^";

	private static bool m_LogError = false;

	public static ISaveGameSerializer Serializer
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

	public static ISaveGameEncoder Encoder
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

	public static Encoding DefaultEncoding
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

	public static bool Encode
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

	public static SaveGamePath SavePath
	{
		get
		{
			return m_SavePath;
		}
		set
		{
			m_SavePath = value;
		}
	}

	public static string EncodePassword
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

	public static bool LogError
	{
		get
		{
			return m_LogError;
		}
		set
		{
			m_LogError = value;
		}
	}

	public static event SaveHandler OnSaved;

	public static event LoadHandler OnLoaded;

	public static void Save<T>(string identifier, T obj)
	{
		Save(identifier, obj, Encode, EncodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static void Save<T>(string identifier, T obj, bool encode)
	{
		Save(identifier, obj, encode, EncodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static void Save<T>(string identifier, T obj, string encodePassword)
	{
		Save(identifier, obj, Encode, encodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static void Save<T>(string identifier, T obj, ISaveGameSerializer serializer)
	{
		Save(identifier, obj, Encode, EncodePassword, serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static void Save<T>(string identifier, T obj, ISaveGameEncoder encoder)
	{
		Save(identifier, obj, Encode, EncodePassword, Serializer, encoder, DefaultEncoding, SavePath);
	}

	public static void Save<T>(string identifier, T obj, Encoding encoding)
	{
		Save(identifier, obj, Encode, EncodePassword, Serializer, Encoder, encoding, SavePath);
	}

	public static void Save<T>(string identifier, T obj, SaveGamePath savePath)
	{
		Save(identifier, obj, Encode, EncodePassword, Serializer, Encoder, DefaultEncoding, savePath);
	}

	public static void Save<T>(string identifier, T obj, bool encode, string password, ISaveGameSerializer serializer, ISaveGameEncoder encoder, Encoding encoding, SaveGamePath path)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new ArgumentNullException("identifier");
		}
		if (serializer == null)
		{
			serializer = Serializer;
		}
		if (encoding == null)
		{
			encoding = DefaultEncoding;
		}
		string text = "";
		text = (IsFilePath(identifier) ? identifier : ((path != SaveGamePath.PersistentDataPath && path == SaveGamePath.DataPath) ? $"{Application.dataPath}/{identifier}" : $"{Application.persistentDataPath}/{identifier}"));
		if (obj == null)
		{
			obj = default(T);
		}
		Stream stream = null;
		Directory.CreateDirectory(Path.GetDirectoryName(text));
		stream = (encode ? new MemoryStream() : ((!IOSupported()) ? ((Stream)new MemoryStream()) : ((Stream)File.Create(text))));
		serializer.Serialize(obj, stream, encoding);
		if (encode)
		{
			string input = encoding.GetString(((MemoryStream)stream).ToArray());
			string value = encoder.Encode(input, password);
			if (IOSupported())
			{
				using StreamWriter streamWriter = new StreamWriter(text, append: false, Encoding.UTF8, 65536);
				streamWriter.WriteLine(value);
			}
			else
			{
				PlayerPrefs.SetString(text, value);
				PlayerPrefs.Save();
			}
		}
		else if (!IOSupported())
		{
			string value2 = encoding.GetString(((MemoryStream)stream).ToArray());
			PlayerPrefs.SetString(text, value2);
			PlayerPrefs.Save();
		}
		stream.Dispose();
		if (SaveCallback != null)
		{
			SaveCallback(obj, identifier, encode, password, serializer, encoder, encoding, path);
		}
		if (SaveGame.OnSaved != null)
		{
			SaveGame.OnSaved(obj, identifier, encode, password, serializer, encoder, encoding, path);
		}
	}

	public static T Load<T>(string identifier)
	{
		return Load(identifier, default(T), Encode, EncodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, T defaultValue)
	{
		return Load(identifier, defaultValue, Encode, EncodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, bool encode, string encodePassword)
	{
		return Load(identifier, default(T), encode, encodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, ISaveGameSerializer serializer)
	{
		return Load(identifier, default(T), Encode, EncodePassword, serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, ISaveGameEncoder encoder)
	{
		return Load(identifier, default(T), Encode, EncodePassword, Serializer, encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, Encoding encoding)
	{
		return Load(identifier, default(T), Encode, EncodePassword, Serializer, Encoder, encoding, SavePath);
	}

	public static T Load<T>(string identifier, SaveGamePath savePath)
	{
		return Load(identifier, default(T), Encode, EncodePassword, Serializer, Encoder, DefaultEncoding, savePath);
	}

	public static T Load<T>(string identifier, T defaultValue, bool encode)
	{
		return Load(identifier, defaultValue, encode, EncodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, T defaultValue, string encodePassword)
	{
		return Load(identifier, defaultValue, Encode, encodePassword, Serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, T defaultValue, ISaveGameSerializer serializer)
	{
		return Load(identifier, defaultValue, Encode, EncodePassword, serializer, Encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, T defaultValue, ISaveGameEncoder encoder)
	{
		return Load(identifier, defaultValue, Encode, EncodePassword, Serializer, encoder, DefaultEncoding, SavePath);
	}

	public static T Load<T>(string identifier, T defaultValue, Encoding encoding)
	{
		return Load(identifier, defaultValue, Encode, EncodePassword, Serializer, Encoder, encoding, SavePath);
	}

	public static T Load<T>(string identifier, T defaultValue, SaveGamePath savePath)
	{
		return Load(identifier, defaultValue, Encode, EncodePassword, Serializer, Encoder, DefaultEncoding, savePath);
	}

	public static T Load<T>(string identifier, T defaultValue, bool encode, string password, ISaveGameSerializer serializer, ISaveGameEncoder encoder, Encoding encoding, SaveGamePath path)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new ArgumentNullException("identifier");
		}
		if (serializer == null)
		{
			serializer = Serializer;
		}
		if (encoding == null)
		{
			encoding = DefaultEncoding;
		}
		if (defaultValue == null)
		{
			defaultValue = default(T);
		}
		T result = defaultValue;
		string text = "";
		text = (IsFilePath(identifier) ? identifier : ((path != SaveGamePath.PersistentDataPath && path == SaveGamePath.DataPath) ? $"{Application.dataPath}/{identifier}" : $"{Application.persistentDataPath}/{identifier}"));
		if (!Exists(text, path))
		{
			Debug.LogWarningFormat("The specified identifier ({1}) does not exists. please use Exists () to check for existent before calling Load.\nreturning the default(T) instance.", text, identifier);
			return result;
		}
		Stream stream = null;
		if (encode)
		{
			string text2 = "";
			text2 = ((!IOSupported()) ? PlayerPrefs.GetString(text) : File.ReadAllText(text, encoding));
			string s = encoder.Decode(text2, password);
			stream = new MemoryStream(encoding.GetBytes(s), writable: true);
		}
		else if (IOSupported())
		{
			stream = File.OpenRead(text);
		}
		else
		{
			string s2 = PlayerPrefs.GetString(text);
			stream = new MemoryStream(encoding.GetBytes(s2));
		}
		result = serializer.Deserialize<T>(stream, encoding);
		stream.Dispose();
		if (result == null)
		{
			result = defaultValue;
		}
		if (LoadCallback != null)
		{
			LoadCallback(result, identifier, encode, password, serializer, encoder, encoding, path);
		}
		if (SaveGame.OnLoaded != null)
		{
			SaveGame.OnLoaded(result, identifier, encode, password, serializer, encoder, encoding, path);
		}
		return result;
	}

	public static bool Exists(string identifier)
	{
		return Exists(identifier, SavePath);
	}

	public static bool Exists(string identifier, SaveGamePath path)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new ArgumentNullException("identifier");
		}
		string text = "";
		text = (IsFilePath(identifier) ? identifier : ((path != SaveGamePath.PersistentDataPath && path == SaveGamePath.DataPath) ? $"{Application.dataPath}/{identifier}" : $"{Application.persistentDataPath}/{identifier}"));
		if (IOSupported())
		{
			bool flag = false;
			flag = Directory.Exists(text);
			if (!flag)
			{
				flag = File.Exists(text);
			}
			return flag;
		}
		return PlayerPrefs.HasKey(text);
	}

	public static void Delete(string identifier)
	{
		Delete(identifier, SavePath);
	}

	public static void Delete(string identifier, SaveGamePath path)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			throw new ArgumentNullException("identifier");
		}
		string text = "";
		text = (IsFilePath(identifier) ? identifier : ((path != SaveGamePath.PersistentDataPath && path == SaveGamePath.DataPath) ? $"{Application.dataPath}/{identifier}" : $"{Application.persistentDataPath}/{identifier}"));
		if (Exists(text, path))
		{
			if (IOSupported())
			{
				File.Delete(text);
			}
			else
			{
				PlayerPrefs.DeleteKey(text);
			}
		}
	}

	public static void Clear()
	{
		DeleteAll(SavePath);
	}

	public static void Clear(SaveGamePath path)
	{
		DeleteAll(path);
	}

	public static void DeleteAll()
	{
		DeleteAll(SavePath);
	}

	public static void DeleteAll(SaveGamePath path)
	{
		string path2 = "";
		switch (path)
		{
		case SaveGamePath.PersistentDataPath:
			path2 = Application.persistentDataPath;
			break;
		case SaveGamePath.DataPath:
			path2 = Application.dataPath;
			break;
		}
		if (IOSupported())
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(path2);
			FileInfo[] files = directoryInfo.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				files[i].Delete();
			}
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			for (int j = 0; j < directories.Length; j++)
			{
				directories[j].Delete(recursive: true);
			}
		}
		else
		{
			PlayerPrefs.DeleteAll();
		}
	}

	public static FileInfo[] GetFiles()
	{
		return GetFiles(string.Empty, SavePath);
	}

	public static FileInfo[] GetFiles(string identifier)
	{
		return GetFiles(identifier, SavePath);
	}

	public static FileInfo[] GetFiles(string identifier, SaveGamePath path)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			identifier = string.Empty;
		}
		string text = "";
		text = (IsFilePath(identifier) ? identifier : ((path != SaveGamePath.PersistentDataPath && path == SaveGamePath.DataPath) ? $"{Application.dataPath}/{identifier}" : $"{Application.persistentDataPath}/{identifier}"));
		FileInfo[] result = new FileInfo[0];
		if (!Exists(text, path))
		{
			return result;
		}
		if (Directory.Exists(text))
		{
			result = new DirectoryInfo(text).GetFiles();
		}
		return result;
	}

	public static DirectoryInfo[] GetDirectories()
	{
		return GetDirectories(string.Empty, SavePath);
	}

	public static DirectoryInfo[] GetDirectories(string identifier)
	{
		return GetDirectories(identifier, SavePath);
	}

	public static DirectoryInfo[] GetDirectories(string identifier, SaveGamePath path)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			identifier = string.Empty;
		}
		string text = "";
		text = (IsFilePath(identifier) ? identifier : ((path != SaveGamePath.PersistentDataPath && path == SaveGamePath.DataPath) ? $"{Application.dataPath}/{identifier}" : $"{Application.persistentDataPath}/{identifier}"));
		DirectoryInfo[] result = new DirectoryInfo[0];
		if (!Exists(text, path))
		{
			return result;
		}
		if (Directory.Exists(text))
		{
			result = new DirectoryInfo(text).GetDirectories();
		}
		return result;
	}

	public static bool IOSupported()
	{
		if (Application.platform != RuntimePlatform.WebGLPlayer && Application.platform != RuntimePlatform.MetroPlayerARM && Application.platform != RuntimePlatform.MetroPlayerX64 && Application.platform != RuntimePlatform.MetroPlayerX86 && Application.platform != RuntimePlatform.tvOS)
		{
			return Application.platform != RuntimePlatform.PS4;
		}
		return false;
	}

	public static bool IsFilePath(string str)
	{
		bool result = false;
		if (Path.IsPathRooted(str))
		{
			try
			{
				Path.GetFullPath(str);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}
}
