using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PowerTools.Quest
{
	public class QuestAnimationTriggers : MonoBehaviour
	{
		[Tooltip("AnimShake(int shakeDataIndex)")]
		[SerializeField]
		private CameraShakeData[] m_shakeData;

		private Dictionary<string, Action> m_animCallbacks = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

		private Dictionary<string, Action> m_animCallbacksTemp = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

		private bool m_character;

		private void Awake()
		{
			m_character = GetComponentInParent<CharacterComponent>() != null;
		}

		public void AddTrigger(string triggerName, Action action, bool removeAfterTrigger)
		{
			if (removeAfterTrigger)
			{
				m_animCallbacksTemp[triggerName] = action;
			}
			else
			{
				m_animCallbacks[triggerName] = action;
			}
		}

		public void RemoveTrigger(string triggerName)
		{
			m_animCallbacks.Remove(triggerName);
			m_animCallbacksTemp.Remove(triggerName);
		}

		private void AnimShake(int index)
		{
			if (m_shakeData.IsIndexValid(index))
			{
				Singleton<PowerQuest>.Get.GetCamera().Shake(m_shakeData[index]);
			}
		}

		private bool AnimTrigger(string name)
		{
			Action value = null;
			if (m_animCallbacksTemp.TryGetValue(name, out value) && value != null)
			{
				value();
				m_animCallbacksTemp.Remove(name);
				return true;
			}
			if (m_animCallbacks.TryGetValue(name, out value) && value != null)
			{
				value();
				return true;
			}
			return false;
		}

		private void _Anim(string function)
		{
			bool flag = false;
			string text = function;
			bool flag2 = text.StartsWith("Anim", StringComparison.OrdinalIgnoreCase);
			if (flag2)
			{
				text = text.Substring(4);
			}
			flag = AnimTrigger(text);
			if (!flag)
			{
				QuestScript script = Singleton<PowerQuest>.Get.GetCurrentRoom().GetScript();
				if (script != null)
				{
					MethodInfo method = script.GetType().GetMethod(function, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						method.Invoke(script, null);
						flag = true;
					}
					else if (flag2)
					{
						method = script.GetType().GetMethod(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method != null)
						{
							method.Invoke(script, null);
							flag = true;
						}
					}
				}
			}
			if (!flag && m_character)
			{
				CharacterComponent componentInParent = GetComponentInParent<CharacterComponent>();
				if (componentInParent != null && componentInParent.GetData().GetScript() != null)
				{
					MethodInfo method2 = componentInParent.GetData().GetScript().GetType()
						.GetMethod(function, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method2 != null)
					{
						method2.Invoke(componentInParent.GetData().GetScript(), null);
						flag = true;
					}
					else if (flag2)
					{
						method2 = componentInParent.GetData().GetScript().GetType()
							.GetMethod(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method2 != null)
						{
							method2.Invoke(componentInParent.GetData().GetScript(), null);
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				return;
			}
			QuestScript globalScript = Singleton<PowerQuest>.Get.GetGlobalScript();
			MethodInfo method3 = globalScript.GetType().GetMethod(function, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method3 != null)
			{
				method3.Invoke(globalScript, null);
				flag = true;
			}
			else if (flag2)
			{
				method3 = globalScript.GetType().GetMethod(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method3 != null)
				{
					method3.Invoke(globalScript, null);
					flag = true;
				}
			}
		}
	}
}
