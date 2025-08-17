using System;
using UnityEngine;

namespace PowerTools.Quest
{
	public interface IButton : IGuiControl
	{
		string Description { get; set; }

		string Cursor { get; set; }

		string Text { get; set; }

		string Anim { get; set; }

		string AnimHover { get; set; }

		string AnimClick { get; set; }

		string AnimOff { get; set; }

		Color Color { get; set; }

		Color ColorHover { get; set; }

		Color ColorClick { get; set; }

		Color ColorOff { get; set; }

		bool Clickable { get; set; }

		bool Animating { get; }

		void PauseAnimation();

		void ResumeAnimation();

		void StopAnimation();

		Coroutine PlayAnimation(string animName);

		void PlayAnimationBG(string animName);

		void AddAnimationTrigger(string triggerName, bool removeAfterTriggering, Action action);

		void RemoveAnimationTrigger(string triggerName);

		Coroutine WaitForAnimTrigger(string triggerName);

		Coroutine Fade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth);

		void FadeBG(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth);
	}
}
