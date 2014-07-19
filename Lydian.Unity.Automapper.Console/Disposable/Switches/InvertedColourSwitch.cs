using System;

namespace Lydian.Disposable.Switches
{
	public class InvertedColourSwitch : ISwitchable
	{
		public void On()
		{
			ToggleColours();
		}

		private static void ToggleColours()
		{
			var toBackground = Console.ForegroundColor;
			Console.ForegroundColor = Console.BackgroundColor;
			Console.BackgroundColor = toBackground;
		}

		public void Off()
		{
			ToggleColours();
		}
	}
}