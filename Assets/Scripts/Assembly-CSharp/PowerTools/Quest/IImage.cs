using System;
using UnityEngine;

namespace PowerTools.Quest
{
	public interface IImage : IGuiControl
	{
		string Anim { get; set; }

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
