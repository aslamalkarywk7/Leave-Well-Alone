using System;

namespace PowerTools.Quest
{
	[Serializable]
	public class GuiScript<T> : QuestScript where T : QuestScript
	{
		protected Gui m_gui;

		public Gui Data => m_gui;

		public IGui Gui => m_gui;

		public static T Script => QuestScript.E.GetScript<T>();

		public IGuiControl Control(string name)
		{
			return Data?.GetControl(name) ?? null;
		}

		public IButton Button(string name)
		{
			return (Data?.GetControl(name) as IButton) ?? null;
		}

		public ILabel Label(string name)
		{
			return (Data?.GetControl(name) as ILabel) ?? null;
		}

		public IImage Image(string name)
		{
			return (Data?.GetControl(name) as IImage) ?? null;
		}

		public ISlider Slider(string name)
		{
			return (Data?.GetControl(name) as ISlider) ?? null;
		}

		public ITextField TextField(string name)
		{
			return (Data?.GetControl(name) as ITextField) ?? null;
		}

		public IInventoryPanel InventoryPanel(string name)
		{
			return (Data?.GetControl(name) as IInventoryPanel) ?? null;
		}

		public IContainer Container(string name)
		{
			return (Data?.GetControl(name) as IContainer) ?? null;
		}

		protected void Initialise(Gui gui)
		{
			m_gui = gui;
		}
	}
}
