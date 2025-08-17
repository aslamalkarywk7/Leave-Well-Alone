namespace PowerTools.Quest
{
	public interface ISpeechGui
	{
		void StartSay(Character character, string text, int currLineId, bool backgroundSpeech);

		void EndSay(Character character);
	}
}
