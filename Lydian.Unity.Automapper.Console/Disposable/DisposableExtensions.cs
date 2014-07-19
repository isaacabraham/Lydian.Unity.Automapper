using System;

namespace Lydian.Disposable
{
	public static class DisposableExtensions
	{
		public static IDisposable AsDisposable(this ISwitchable switchable)
		{
			return new DisposableAdapter(switchable);
		}
	}
}
