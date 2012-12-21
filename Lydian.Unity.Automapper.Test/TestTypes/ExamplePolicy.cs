using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Lydian.Unity.Automapper.Test.Types
{
	class ExampleCallHandler : HandlerAttribute, ICallHandler
	{
		public override ICallHandler CreateHandler(IUnityContainer container)
		{
			return this;
		}

		public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
		{
			return getNext()(input, getNext);
		}
	}
}
