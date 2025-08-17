using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace PowerTools.Quest
{
	public class QuestSaveManager
	{
		private class CustomSaveData
		{
			public string m_name;

			public object m_data;

			public Action CallbackOnPostRestore;
		}

		public sealed class VersionDeserializationBinder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
				{
					assemblyName = Assembly.GetExecutingAssembly().FullName;
					return Type.GetType($"{typeName}, {assemblyName}");
				}
				return null;
			}
		}

		private static readonly byte[] NOTHING_TO_SEE_HERE = new byte[8] { 221, 42, 220, 88, 166, 196, 202, 16 };

		private static readonly byte[] JUST_A_REGULAR_VARIABLE = new byte[8] { 71, 161, 109, 193, 198, 103, 217, 237 };

		private static readonly string FILE_NAME_START = "Save";

		private static readonly string FILE_NAME_EXTENTION = ".sav";

		private static readonly string FILE_NAME_WILDCARD = FILE_NAME_START + "*" + FILE_NAME_EXTENTION;

		private static readonly int VERSION_CURRENT = 4;

		private static readonly int VERSION_REQUIRED = 4;

		private List<QuestSaveSlotData> m_saveSlots = new List<QuestSaveSlotData>();

		private string m_log = string.Empty;

		private bool m_loadedSaveSlots;

		private List<CustomSaveData> m_customSaveData = new List<CustomSaveData>();

		private Dictionary<string, byte[]> m_cachedSaveData = new Dictionary<string, byte[]>();

		private static readonly BindingFlags BINDING_FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly Type TYPE_QUESTSAVE = typeof(QuestSaveAttribute);

		private static readonly Type TYPE_QUESTDONTSAVE = typeof(QuestDontSaveAttribute);

		public void AddSaveData(string name, object data, Action OnPostRestore = null)
		{
			if (Debug.isDebugBuild && data.GetType().IsValueType)
			{
				Debug.LogError("Error in AddSaveData( \"" + name + "\", ... ): Value types cannot be used for custom save data. You need to save the containing class, or put them in one to be saved");
			}
			else if (Debug.isDebugBuild && QuestSaveSurrogateSelector.IsIgnoredType(data.GetType()) && !Attribute.IsDefined(data.GetType(), TYPE_QUESTSAVE))
			{
				Debug.LogError("Error in AddSaveData( \"" + name + "\", ... ): When saving a component, use the [QuestSave] attribute on the class, and any variables you wish to save");
			}
			if (m_customSaveData.Exists((CustomSaveData customSaveData) => string.Equals(customSaveData.m_name, name)))
			{
				Debug.LogWarning("Save data already exists for " + name + ", Call UnregisterSaveData first for safety. Item will be overwritten");
				m_customSaveData.RemoveAll((CustomSaveData customSaveData) => string.Equals(customSaveData.m_name, name));
			}
			CustomSaveData item = new CustomSaveData
			{
				m_name = name,
				m_data = data,
				CallbackOnPostRestore = OnPostRestore
			};
			m_customSaveData.Add(item);
		}

		public void RemoveSaveData(string name)
		{
			m_customSaveData.RemoveAll((CustomSaveData item) => string.Equals(item.m_name, name));
		}

		public List<QuestSaveSlotData> GetSaveSlotData()
		{
			if (!m_loadedSaveSlots)
			{
				LoadSaveSlotData();
			}
			return m_saveSlots;
		}

		public QuestSaveSlotData GetSaveSlot(int id)
		{
			if (!m_loadedSaveSlots)
			{
				LoadSaveSlotData();
			}
			return m_saveSlots.Find((QuestSaveSlotData slot) => slot.m_slotId == id);
		}

		public bool Save(int slot, string displayName, int version, Dictionary<string, object> data, Texture2D image = null)
		{
			bool result = Save(FILE_NAME_START + slot + FILE_NAME_EXTENTION, displayName, version, data, image);
			ReloadSaveSlotData(slot);
			return result;
		}

		public bool Save(string fileName, string displayName, int version, Dictionary<string, object> data, Texture2D image = null)
		{
			bool result = false;
			foreach (CustomSaveData customSaveDatum in m_customSaveData)
			{
				data.Add(customSaveDatum.m_name + "%", customSaveDatum.m_data);
			}
			Stream stream = null;
			Stream stream2 = null;
			QuestSaveSurrogateSelector.StartLogSave();
			try
			{
				stream = File.Open(GetSaveDirectory() + fileName, FileMode.Create);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Binder = new VersionDeserializationBinder();
				binaryFormatter.Serialize(stream, VERSION_CURRENT);
				binaryFormatter.Serialize(stream, version);
				binaryFormatter.Serialize(stream, displayName);
				binaryFormatter.Serialize(stream, Utils.GetUnixTimestamp());
				if (image == null)
				{
					binaryFormatter.Serialize(stream, false);
				}
				else
				{
					binaryFormatter.Serialize(stream, true);
					byte[] graph = image.EncodeToPNG();
					binaryFormatter.Serialize(stream, graph);
				}
				DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
				dESCryptoServiceProvider.Key = NOTHING_TO_SEE_HERE;
				dESCryptoServiceProvider.IV = JUST_A_REGULAR_VARIABLE;
				stream2 = new CryptoStream(stream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
				SurrogateSelector surrogateSelector = new SurrogateSelector();
				surrogateSelector.ChainSelector(new QuestSaveSurrogateSelector());
				binaryFormatter.SurrogateSelector = surrogateSelector;
				using (MemoryStream memoryStream = new MemoryStream(128))
				{
					binaryFormatter.Serialize(stream2, data.Count);
					foreach (KeyValuePair<string, object> datum in data)
					{
						binaryFormatter.Serialize(stream2, datum.Key);
						byte[] array = null;
						if (datum.Value is IQuestSaveCachable)
						{
							IQuestSaveCachable questSaveCachable = datum.Value as IQuestSaveCachable;
							if (questSaveCachable.SaveDirty || !m_cachedSaveData.ContainsKey(datum.Key))
							{
								binaryFormatter.Serialize(memoryStream, datum.Value);
								array = memoryStream.ToArray();
								m_cachedSaveData[datum.Key] = array;
								questSaveCachable.SaveDirty = false;
								memoryStream.SetLength(0L);
							}
							else
							{
								array = m_cachedSaveData[datum.Key];
							}
						}
						else
						{
							binaryFormatter.Serialize(memoryStream, datum.Value);
							array = memoryStream.ToArray();
							memoryStream.SetLength(0L);
						}
						binaryFormatter.Serialize(stream2, array);
					}
				}
				stream2.Close();
				result = true;
			}
			catch (Exception ex)
			{
				m_log = "Save failed: " + ex.ToString();
				result = false;
			}
			finally
			{
				stream2?.Close();
				stream?.Close();
			}
			TempPrintLog();
			return result;
		}

		public bool RestoreSave(int slot, int versionRequired, out int version, out Dictionary<string, object> data)
		{
			return RestoreSave(FILE_NAME_START + slot + FILE_NAME_EXTENTION, versionRequired, out version, out data, slot);
		}

		public bool RestoreSave(string fileName, int versionRequired, out int version, out Dictionary<string, object> data)
		{
			return RestoreSave(fileName, versionRequired, out version, out data, -1);
		}

		private bool RestoreSave(string fileName, int versionRequired, out int version, out Dictionary<string, object> data, int slot)
		{
			bool result = false;
			data = null;
			version = -1;
			int num = -1;
			QuestSaveSurrogateSelector.StartLogLoad();
			QuestSaveSlotData questSaveSlotData = new QuestSaveSlotData();
			if (slot >= 0)
			{
				questSaveSlotData = GetSaveSlot(slot);
				if (questSaveSlotData == null)
				{
					questSaveSlotData = new QuestSaveSlotData
					{
						m_slotId = slot
					};
				}
			}
			Stream stream = null;
			Stream stream2 = null;
			try
			{
				stream = File.Open(GetSaveDirectory() + fileName, FileMode.Open);
				DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
				dESCryptoServiceProvider.Key = NOTHING_TO_SEE_HERE;
				dESCryptoServiceProvider.IV = JUST_A_REGULAR_VARIABLE;
				stream2 = new CryptoStream(stream, dESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Read);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Binder = new VersionDeserializationBinder();
				num = (int)binaryFormatter.Deserialize(stream);
				if (num < VERSION_REQUIRED)
				{
					throw new Exception("Incompatible save version. Required: " + VERSION_REQUIRED + ", Found: " + num);
				}
				DeserializeSlotData(questSaveSlotData, binaryFormatter, stream, num);
				version = questSaveSlotData.m_version;
				if (version < versionRequired)
				{
					throw new Exception("Incompatible game save version. Required: " + versionRequired + ", Found: " + version);
				}
				SurrogateSelector surrogateSelector = new SurrogateSelector();
				surrogateSelector.ChainSelector(new QuestSaveSurrogateSelector());
				binaryFormatter.SurrogateSelector = surrogateSelector;
				if (num < 3)
				{
					data = binaryFormatter.Deserialize(stream2) as Dictionary<string, object>;
				}
				else
				{
					int num2 = (int)binaryFormatter.Deserialize(stream2);
					data = new Dictionary<string, object>(num2);
					for (int i = 0; i < num2; i++)
					{
						string key = binaryFormatter.Deserialize(stream2) as string;
						byte[] array = binaryFormatter.Deserialize(stream2) as byte[];
						using MemoryStream serializationStream = new MemoryStream(array);
						object obj = binaryFormatter.Deserialize(serializationStream);
						data.Add(key, obj);
						if (obj is IQuestSaveCachable)
						{
							(obj as IQuestSaveCachable).SaveDirty = false;
							m_cachedSaveData[key] = array;
						}
					}
				}
				foreach (CustomSaveData customSaveDatum in m_customSaveData)
				{
					if (data.TryGetValue(customSaveDatum.m_name + "%", out var value))
					{
						CopyCustomSaveDataFields(customSaveDatum.m_data, value);
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				if (!(ex is FileNotFoundException))
				{
					m_log = "Load failed: " + ex.ToString();
				}
				result = false;
			}
			finally
			{
				try
				{
					stream2?.Close();
				}
				catch (Exception ex2)
				{
					m_log = m_log + "\nLoad failed: " + ex2.ToString();
					result = false;
				}
				stream?.Close();
			}
			TempPrintLog();
			return result;
		}

		private void DeserializeSlotData(QuestSaveSlotData slotData, BinaryFormatter bformatter, Stream stream, int saveVersion)
		{
			if (slotData == null)
			{
				return;
			}
			slotData.m_version = (int)bformatter.Deserialize(stream);
			slotData.m_description = (string)bformatter.Deserialize(stream);
			slotData.m_timestamp = (int)bformatter.Deserialize(stream);
			if (saveVersion < 2 || !(bool)bformatter.Deserialize(stream))
			{
				return;
			}
			byte[] array = (byte[])bformatter.Deserialize(stream);
			if (array != null && array.Length != 0)
			{
				if (slotData.m_image == null)
				{
					slotData.m_image = new Texture2D(2, 2);
				}
				slotData.m_image.LoadImage(array, markNonReadable: false);
			}
		}

		public void OnPostRestore()
		{
			foreach (CustomSaveData customSaveDatum in m_customSaveData)
			{
				if (customSaveDatum.CallbackOnPostRestore != null)
				{
					customSaveDatum.CallbackOnPostRestore();
				}
			}
		}

		public static void CopyCustomSaveDataFields<T>(T to, T from)
		{
			Type type = to.GetType();
			if (type != from.GetType())
			{
				return;
			}
			FieldInfo[] fields = type.GetFields(BINDING_FLAGS);
			bool flag = Attribute.IsDefined(type, TYPE_QUESTSAVE);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (!flag || Attribute.IsDefined(fieldInfo, TYPE_QUESTSAVE))
				{
					fieldInfo.SetValue(to, fieldInfo.GetValue(from));
				}
			}
		}

		public bool DeleteSave(int slot)
		{
			bool result = true;
			try
			{
				File.Delete(GetSaveDirectory() + FILE_NAME_START + slot + FILE_NAME_EXTENTION);
			}
			catch (Exception ex)
			{
				m_log = "Delete failed: " + ex.ToString();
				result = false;
			}
			m_saveSlots.RemoveAll((QuestSaveSlotData item) => item.m_slotId == slot);
			TempPrintLog();
			return result;
		}

		private bool LoadHeader(QuestSaveSlotData slotData)
		{
			bool result = false;
			if (slotData == null)
			{
				return false;
			}
			int slotId = slotData.m_slotId;
			string path = GetSaveDirectory() + FILE_NAME_START + slotId + FILE_NAME_EXTENTION;
			Stream stream = null;
			try
			{
				stream = File.Open(path, FileMode.Open);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Binder = new VersionDeserializationBinder();
				int num = (int)binaryFormatter.Deserialize(stream);
				if (num >= VERSION_REQUIRED)
				{
					DeserializeSlotData(slotData, binaryFormatter, stream, num);
					result = true;
				}
				else
				{
					m_log = "Incompatible save version. Required: " + VERSION_REQUIRED + ", Found: " + num;
				}
			}
			catch (Exception ex)
			{
				m_log = "Load failed: " + ex.ToString();
			}
			finally
			{
				stream?.Close();
			}
			return result;
		}

		private void ReloadSaveSlotData(int slotId)
		{
			if (!m_loadedSaveSlots)
			{
				LoadSaveSlotData();
				return;
			}
			QuestSaveSlotData questSaveSlotData = GetSaveSlot(slotId);
			bool num = questSaveSlotData == null;
			if (num)
			{
				questSaveSlotData = new QuestSaveSlotData
				{
					m_slotId = slotId
				};
			}
			bool flag = LoadHeader(questSaveSlotData);
			if (num && flag)
			{
				m_saveSlots.Add(questSaveSlotData);
			}
			if (!num && !flag)
			{
				m_saveSlots.Remove(questSaveSlotData);
			}
		}

		private void LoadSaveSlotData()
		{
			if (m_loadedSaveSlots)
			{
				Debug.LogWarning("Save slots should only be loaded once. Use ReloadSaveSlotData()");
			}
			string[] files = Directory.GetFiles(Path.GetFullPath(GetSaveDirectory()), FILE_NAME_WILDCARD);
			foreach (string text in files)
			{
				QuestSaveSlotData questSaveSlotData = new QuestSaveSlotData();
				if (!int.TryParse(Path.GetFileNameWithoutExtension(text).Substring(4), out questSaveSlotData.m_slotId))
				{
					m_log = "Couldn't parse id from path: " + text;
				}
				else if (LoadHeader(questSaveSlotData))
				{
					m_saveSlots.Add(questSaveSlotData);
				}
			}
			m_loadedSaveSlots = true;
		}

		private string GetSaveDirectory()
		{
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				return Application.persistentDataPath + "/";
			}
			return "./";
		}

		private void TempPrintLog()
		{
			if (!string.IsNullOrEmpty(m_log))
			{
				Debug.Log(m_log);
				m_log = null;
			}
			QuestSaveSurrogateSelector.PrintLog();
		}
	}
}
