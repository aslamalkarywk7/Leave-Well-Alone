using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace PowerTools.Quest
{
	internal sealed class QuestSaveSurrogateSelector : ISerializationSurrogate, ISurrogateSelector
	{
		private static readonly BindingFlags BINDING_FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly Type TYPE_QUESTSAVE = typeof(QuestSaveAttribute);

		private static readonly Type TYPE_QUESTDONTSAVE = typeof(QuestDontSaveAttribute);

		private static readonly Type TYPE_COMPILERGENERATED = typeof(CompilerGeneratedAttribute);

		public static StringBuilder s_log = new StringBuilder();

		private ISurrogateSelector m_nextSelector;

		private static readonly Type STRING_TYPE = typeof(string);

		public static void StartLogSave()
		{
		}

		public static void StartLogLoad()
		{
		}

		public static void PrintLog()
		{
		}

		public void ChainSelector(ISurrogateSelector selector)
		{
			m_nextSelector = selector;
		}

		public ISurrogateSelector GetNextSelector()
		{
			return m_nextSelector;
		}

		public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
		{
			if (IsIgnoredType(type))
			{
				selector = this;
				return this;
			}
			if (IsKnownType(type))
			{
				selector = null;
				return null;
			}
			if (type.IsClass)
			{
				selector = this;
				return this;
			}
			if (type.IsValueType)
			{
				selector = this;
				return this;
			}
			selector = null;
			return null;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			try
			{
				Type type = obj.GetType();
				FieldInfo[] fields;
				if (type == typeof(Vector2) || type == typeof(Color))
				{
					fields = type.GetFields(BINDING_FLAGS);
					foreach (FieldInfo fieldInfo in fields)
					{
						info.AddValue(fieldInfo.Name, fieldInfo.GetValue(obj));
					}
					return;
				}
				bool flag = Attribute.IsDefined(type, TYPE_QUESTSAVE);
				if (IsIgnoredType(type) && !flag)
				{
					return;
				}
				fields = type.GetFields(BINDING_FLAGS);
				foreach (FieldInfo fieldInfo2 in fields)
				{
					if ((flag && !Attribute.IsDefined(fieldInfo2, TYPE_QUESTSAVE)) || IsIgnoredType(fieldInfo2.FieldType))
					{
						continue;
					}
					if (IsKnownType(fieldInfo2.FieldType))
					{
						if (fieldInfo2.Name.Length > 0 && fieldInfo2.Name[0] == '$')
						{
							break;
						}
						info.AddValue(fieldInfo2.Name, fieldInfo2.GetValue(obj));
					}
					else if (fieldInfo2.FieldType.IsClass || fieldInfo2.FieldType.IsValueType)
					{
						info.AddValue(fieldInfo2.Name, fieldInfo2.GetValue(obj));
					}
				}
			}
			catch
			{
			}
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			try
			{
				Type type = obj.GetType();
				FieldInfo[] fields;
				if (type == typeof(Vector2) || type == typeof(Color))
				{
					fields = type.GetFields(BINDING_FLAGS);
					foreach (FieldInfo fieldInfo in fields)
					{
						fieldInfo.SetValue(obj, info.GetValue(fieldInfo.Name, fieldInfo.FieldType));
					}
					return obj;
				}
				bool flag = Attribute.IsDefined(type, TYPE_QUESTSAVE);
				if (IsIgnoredType(type) && !flag)
				{
					return obj;
				}
				fields = type.GetFields(BINDING_FLAGS);
				foreach (FieldInfo fieldInfo2 in fields)
				{
					if ((flag && !Attribute.IsDefined(fieldInfo2, TYPE_QUESTSAVE)) || IsIgnoredType(fieldInfo2.FieldType))
					{
						continue;
					}
					if (IsKnownType(fieldInfo2.FieldType))
					{
						if (IsNullableType(fieldInfo2.FieldType))
						{
							Type firstArgumentOfGenericType = GetFirstArgumentOfGenericType(fieldInfo2.FieldType);
							fieldInfo2.SetValue(obj, info.GetValue(fieldInfo2.Name, firstArgumentOfGenericType));
						}
						else
						{
							fieldInfo2.SetValue(obj, info.GetValue(fieldInfo2.Name, fieldInfo2.FieldType));
						}
					}
					else if (fieldInfo2.FieldType.IsClass || fieldInfo2.FieldType.IsValueType)
					{
						fieldInfo2.SetValue(obj, info.GetValue(fieldInfo2.Name, fieldInfo2.FieldType));
					}
				}
			}
			catch
			{
			}
			return obj;
		}

		public static bool IsIgnoredType(Type type)
		{
			if (!(type == typeof(IEnumerator)))
			{
				if (type != STRING_TYPE && type.IsClass)
				{
					if (!(type == typeof(GameObject)) && !(type == typeof(Coroutine)) && !(type == typeof(AudioHandle)) && !type.IsSubclassOf(typeof(Component)) && !type.IsSubclassOf(typeof(Texture)) && !type.IsSubclassOf(typeof(MulticastDelegate)))
					{
						return Attribute.IsDefined(type, TYPE_COMPILERGENERATED);
					}
					return true;
				}
				return false;
			}
			return true;
		}

		private bool IsKnownType(Type type)
		{
			if (!(type == STRING_TYPE) && !type.IsPrimitive)
			{
				return type.IsSerializable;
			}
			return true;
		}

		private bool IsNullableType(Type type)
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return false;
		}

		private Type GetFirstArgumentOfGenericType(Type type)
		{
			return type.GetGenericArguments()[0];
		}
	}
}
