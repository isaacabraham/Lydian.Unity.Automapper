using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal class InjectionMemberFactory : IInjectionMemberFactory
	{
		private readonly AutomapperConfig configurationDetails;

		public InjectionMemberFactory(AutomapperConfig configurationDetails, IUnityContainer target)
		{
			this.configurationDetails = configurationDetails;

			if (configurationDetails.PolicyInjectionRequired())
				target.AddNewExtension<Interception>();
		}

		public InjectionMember[] CreateInjectionMembers(TypeMapping typeMapping)
		{
			return configurationDetails.IsPolicyInjection(typeMapping.From) ? new InjectionMember[] { new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<PolicyInjectionBehavior>() }
																			: new InjectionMember[0];
		}
	}
}
