using System;

namespace Lydian.Disposable
{
	public class DisposableAdapter : IDisposable
	{
		private readonly ISwitchable switchableItem;

		public DisposableAdapter(ISwitchable switchableItem)
		{
			this.switchableItem = switchableItem;
			switchableItem.On();
		}

		public void Dispose()
		{
			switchableItem.Off();
		}
	}

	public sealed class DisposableAdapter<TSwitchable> : DisposableAdapter where TSwitchable : ISwitchable, new()
	{
		public DisposableAdapter() : base(new TSwitchable()) { }
	}
}