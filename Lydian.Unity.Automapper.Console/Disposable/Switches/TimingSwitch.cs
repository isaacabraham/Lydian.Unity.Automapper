using System.Diagnostics;

namespace Lydian.Disposable.Switches
{
	public class TimingSwitch : ISwitchable
	{
		public Stopwatch Stopwatch { get; private set; }

		public void On()
		{
			Stopwatch = Stopwatch.StartNew();
		}

		public void Off()
		{
			Stopwatch.Stop();
		}
	}
}