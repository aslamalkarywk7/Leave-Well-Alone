using System.Collections;
using UnityEngine;

namespace PowerTools.Quest
{
	public static class Wait
	{
		public static IEnumerator CoroutineWaitForTime(float time)
		{
			while (time > 0f)
			{
				yield return new WaitForEndOfFrame();
				if (!SystemTime.Paused)
				{
					time -= Time.deltaTime;
				}
			}
		}

		public static IEnumerator CoroutineWaitForFixedUpdate()
		{
			yield return new WaitForFixedUpdate();
			while (SystemTime.Paused)
			{
				yield return new WaitForFixedUpdate();
			}
		}

		public static IEnumerator CoroutineWaitForEndOfFrame()
		{
			yield return new WaitForEndOfFrame();
			while (SystemTime.Paused)
			{
				yield return new WaitForEndOfFrame();
			}
		}

		public static IEnumerator CoroutineWaitForTimeNoPause(float time)
		{
			yield return new WaitForSeconds(time);
		}

		public static IEnumerator CoroutineWaitForFixedUpdateNoPause()
		{
			yield return new WaitForFixedUpdate();
		}

		public static IEnumerator CoroutineWaitForEndOfFrameNoPause()
		{
			yield return new WaitForEndOfFrame();
		}

		public static Coroutine ForTime(float time)
		{
			return Singleton<SystemTime>.Get.StartCoroutine(CoroutineWaitForTime(time));
		}

		public static Coroutine ForTimeUnscaled(float time)
		{
			return Singleton<SystemTime>.Get.StartCoroutine(CoroutineWaitForTime(time * Singleton<SystemTime>.Get.GetTimeScale()));
		}

		public static Coroutine ForFixedUpdate()
		{
			return Singleton<SystemTime>.Get.StartCoroutine(CoroutineWaitForFixedUpdate());
		}

		public static Coroutine ForEndOfFrame()
		{
			return Singleton<SystemTime>.Get.StartCoroutine(CoroutineWaitForEndOfFrame());
		}
	}
}
