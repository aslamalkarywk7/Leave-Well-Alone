using System;
using PowerScript;
using UnityEngine;

namespace PowerTools.Quest
{
	[Serializable]
	public class DialogTreeScript<T> : QuestScript where T : QuestScript
	{
		protected IDialogTree m_data;

		public bool FirstTimeShown => D.Current.FirstTimeShown;

		public int TimesShown => D.Current.TimesShown;

		public static T Script => QuestScript.E.GetScript<T>();

		public IDialogOption Option(int id)
		{
			return m_data.GetOption(id);
		}

		public IDialogOption Option(string id)
		{
			if (m_data == null)
			{
				Debug.LogError("Data not set up yet in Dialog. Can't retrieve option");
			}
			return m_data.GetOption(id);
		}

		public void OptionOn(params int[] id)
		{
			m_data.OptionOn(id);
		}

		public void OptionOff(params int[] id)
		{
			m_data.OptionOff(id);
		}

		public void OptionOffForever(params int[] id)
		{
			m_data.OptionOffForever(id);
		}

		public void OptionOn(params string[] id)
		{
			m_data.OptionOn(id);
		}

		public void OptionOff(params string[] id)
		{
			m_data.OptionOff(id);
		}

		public void OptionOffForever(params string[] id)
		{
			m_data.OptionOffForever(id);
		}

		public void Goto(IDialogTree dialog)
		{
			dialog?.Start();
		}

		public void GotoPrevious()
		{
			if (Singleton<PowerQuest>.Get.GetPreviousDialog() != null)
			{
				Singleton<PowerQuest>.Get.GetPreviousDialog().Start();
			}
		}

		public void Stop()
		{
			Singleton<PowerQuest>.Get.StopDialog();
		}
	}
}
