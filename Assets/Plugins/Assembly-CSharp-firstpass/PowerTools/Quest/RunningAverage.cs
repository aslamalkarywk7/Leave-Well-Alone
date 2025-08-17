namespace PowerTools.Quest
{
	public class RunningAverage
	{
		private float m_total;

		private float m_num;

		private float m_average;

		public void SetAverage(float average)
		{
			m_total = 0f;
			m_num = 0f;
			m_average = 0f;
			AddValue(average);
		}

		public void AddValue(float value)
		{
			m_total += value;
			m_num += 1f;
			m_average = m_total / m_num;
		}

		public float GetAverage()
		{
			return m_average;
		}
	}
}
