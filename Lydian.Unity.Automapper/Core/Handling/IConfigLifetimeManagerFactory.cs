using Microsoft.Practices.Unity;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal interface IConfigLifetimeManagerFactory
	{
		LifetimeManager CreateLifetimeManager(TypeMapping typeMapping);
	}
}
