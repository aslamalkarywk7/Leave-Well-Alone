using UnityEngine;

namespace PowerTools.Quest
{
	public interface ILabel : IGuiControl
	{
		string Text { get; set; }

		Color Color { get; set; }

		QuestText TextComponent { get; }

		Coroutine Fade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth);

		void FadeBG(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth);
	}
}
