using System;
using UnityEngine;
using UnityEngine.Video;

namespace PowerTools.Quest
{
	public interface IProp : IQuestClickableInterface
	{
		string Description { get; set; }

		string ScriptName { get; }

		MonoBehaviour Instance { get; }

		bool Visible { get; set; }

		bool Clickable { get; set; }

		Vector2 Position { get; set; }

		bool Moving { get; }

		float Baseline { get; set; }

		Vector2 WalkToPoint { get; set; }

		Vector2 LookAtPoint { get; set; }

		string Cursor { get; set; }

		bool FirstUse { get; }

		bool FirstLook { get; }

		int UseCount { get; }

		int LookCount { get; }

		string Animation { get; set; }

		bool Animating { get; }

		VideoPlayer VideoPlayer { get; }

		float Alpha { get; set; }

		Prop Data { get; }

		void SetPosition(float x, float y);

		Coroutine MoveTo(float x, float y, float speed, eEaseCurve curve = eEaseCurve.None);

		Coroutine MoveTo(Vector2 toPos, float speed, eEaseCurve curve = eEaseCurve.None);

		void MoveToBG(Vector2 toPos, float speed, eEaseCurve curve = eEaseCurve.None);

		void Show(bool clickable = true);

		void Hide();

		void Enable(bool clickable = true);

		void Disable();

		Coroutine PlayAnimation(string animName);

		void PlayAnimationBG(string animName);

		void PauseAnimation();

		void ResumeAnimation();

		Coroutine PlayVideo(float skippableAfterTime = -1f);

		void PlayVideoBG();

		void AddAnimationTrigger(string triggerName, bool removeAfterTriggering, Action action);

		void RemoveAnimationTrigger(string triggerName);

		Coroutine WaitForAnimTrigger(string eventName);

		Coroutine Fade(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth);

		void FadeBG(float start, float end, float duration, eEaseCurve curve = eEaseCurve.InOutSmooth);
	}
}
