using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PowerTools.Quest
{
	public static class QuestUtils
	{
		private static readonly BindingFlags BINDING_FLAGS = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static float Ease(float ratio, eEaseCurve curve = eEaseCurve.InOutSmooth)
		{
			if (ratio <= 0f)
			{
				return 0f;
			}
			if (ratio >= 1f)
			{
				return 1f;
			}
			float num = ratio;
			switch (curve)
			{
			case eEaseCurve.InSmooth:
				ratio *= 0.5f;
				return (-2f * ratio * ratio * ratio + 3f * ratio * ratio) * 2f;
			case eEaseCurve.OutSmooth:
				ratio = ratio * 0.5f + 0.5f;
				return (-2f * ratio * ratio * ratio + 3f * ratio * ratio) * 2f - 1f;
			case eEaseCurve.InOutSmooth:
				return -2f * ratio * ratio * ratio + 3f * ratio * ratio;
			case eEaseCurve.InSine:
				return 1f - Mathf.Cos(ratio * (float)Math.PI * 0.5f);
			case eEaseCurve.OutSine:
				return Mathf.Sin(num * (float)Math.PI * 0.5f);
			case eEaseCurve.InOutSine:
				return (0f - (Mathf.Cos(num * (float)Math.PI) - 1f)) * 0.5f;
			case eEaseCurve.InQuad:
			case eEaseCurve.OutQuad:
			case eEaseCurve.InOutQuad:
			case eEaseCurve.InCubic:
			case eEaseCurve.OutCubic:
			case eEaseCurve.InOutCubic:
			case eEaseCurve.InQuart:
			case eEaseCurve.OutQuart:
			case eEaseCurve.InOutQuart:
			case eEaseCurve.InQuint:
			case eEaseCurve.OutQuint:
			case eEaseCurve.InOutQuint:
			{
				float num2 = (int)(curve - 7) / 3 + 2;
				switch ((int)(curve - 7) % 3)
				{
				case 0:
					return Mathf.Pow(ratio, num2);
				case 1:
					return 1f - Mathf.Pow(1f - ratio, num2);
				default:
					if (!(num < 0.5f))
					{
						return 1f - Mathf.Pow(-2f * num + 2f, num2) * 0.5f;
					}
					return Mathf.Pow(2f, num2 - 1f) * Mathf.Pow(num, num2);
				}
			}
			case eEaseCurve.InExp:
				return Mathf.Pow(2f, 10f * num - 10f);
			case eEaseCurve.OutExp:
				return 1f - Mathf.Pow(2f, -10f * num);
			case eEaseCurve.InOutExp:
				if (!(num < 0.5f))
				{
					return (2f - Mathf.Pow(2f, -20f * num + 10f)) * 0.5f;
				}
				return Mathf.Pow(2f, 20f * num - 10f) * 0.5f;
			case eEaseCurve.InElastic:
				return (0f - Mathf.Pow(2f, 10f * num - 10f)) * Mathf.Sin((num * 10f - 10.75f) * 1.4670551f);
			case eEaseCurve.OutElastic:
				return Mathf.Pow(2f, -10f * num) * Mathf.Sin((num * 10f - 0.75f) * 1.4670551f) + 1f;
			case eEaseCurve.InOutElastic:
				throw new NotImplementedException();
			default:
				return ratio;
			}
		}

		public static void CopyFields<T>(T to, T from)
		{
			Type type = to.GetType();
			if (!(type != from.GetType()))
			{
				FieldInfo[] fields = type.GetFields(BINDING_FLAGS);
				foreach (FieldInfo fieldInfo in fields)
				{
					fieldInfo.SetValue(to, fieldInfo.GetValue(from));
				}
			}
		}

		public static List<T> CopyListFields<T>(List<T> from) where T : new()
		{
			List<T> list = new List<T>(from.Count);
			foreach (T item in from)
			{
				T val = new T();
				CopyFields(val, item);
				list.Add(val);
			}
			return list;
		}

		public static void CopyHotLoadFields<T>(T to, T from)
		{
			FieldInfo[] fields = to.GetType().GetFields(BINDING_FLAGS);
			FieldInfo[] fields2 = from.GetType().GetFields(BINDING_FLAGS);
			FieldInfo[] array = fields;
			foreach (FieldInfo finfo in array)
			{
				FieldInfo fieldInfo = Array.Find(fields2, (FieldInfo item) => item.Name == finfo.Name);
				if (!(fieldInfo != null))
				{
					continue;
				}
				_ = finfo.ReflectedType;
				try
				{
					object value = fieldInfo.GetValue(from);
					if (value is Enum)
					{
						finfo.SetValue(to, (int)value);
					}
					else
					{
						finfo.SetValue(to, value);
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Hotloading script warning: " + ex.ToString());
				}
			}
		}

		public static void InitWithDefaults<T>(T toInit) where T : class
		{
			if (Activator.CreateInstance(toInit.GetType()) is T val)
			{
				CopyFields(toInit, val);
			}
		}

		public static void HotSwapScript<T>(ref T toSwap, string name, Assembly assembly) where T : class
		{
			if (toSwap != null)
			{
				T val = toSwap;
				toSwap = ConstructByName<T>(name, assembly);
				CopyHotLoadFields(toSwap, val);
			}
		}

		public static T ConstructByName<T>(string name) where T : class
		{
			T result = null;
			try
			{
				result = Type.GetType($"{name}, {typeof(PowerQuest).Assembly.FullName}").GetConstructor(new Type[0]).Invoke(new object[0]) as T;
				return result;
			}
			catch
			{
			}
			return result;
		}

		public static T ConstructByName<T>(string name, Assembly assembly) where T : class
		{
			T result = null;
			try
			{
				result = Type.GetType($"{name}, {assembly.FullName}").GetConstructor(new Type[0]).Invoke(new object[0]) as T;
				return result;
			}
			catch
			{
			}
			return result;
		}

		public static void StopwatchStart()
		{
		}

		public static void StopwatchStop(string logTxt)
		{
		}

		public static T FindScriptable<T>(List<T> scriptables, string scriptName) where T : class, IQuestScriptable
		{
			foreach (T scriptable in scriptables)
			{
				if (scriptable != null && string.Equals(scriptable.GetScriptName(), scriptName, StringComparison.OrdinalIgnoreCase))
				{
					return scriptable;
				}
			}
			return null;
		}

		public static T FindScriptableMono<T>(List<T> scriptables, string scriptName) where T : MonoBehaviour, IQuestScriptable
		{
			foreach (T scriptable in scriptables)
			{
				if (scriptable != null && string.Equals(scriptable.GetScriptName(), scriptName, StringComparison.OrdinalIgnoreCase))
				{
					return scriptable;
				}
			}
			return null;
		}

		public static T FindByName<T>(List<T> objects, string name) where T : UnityEngine.Object
		{
			foreach (T @object in objects)
			{
				if (@object != null && string.Equals(@object.name, name, StringComparison.OrdinalIgnoreCase))
				{
					return @object;
				}
			}
			return null;
		}
	}
}
