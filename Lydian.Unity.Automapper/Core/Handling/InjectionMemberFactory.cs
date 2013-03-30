using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Linq;
using System.Reflection;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal class InjectionMemberFactory : IInjectionMemberFactory
	{
		private readonly AutomapperConfig configurationDetails;

		public InjectionMemberFactory(AutomapperConfig configurationDetails)
		{
			this.configurationDetails = configurationDetails;
		}

		public InjectionMember[] CreateInjectionMembers(TypeMapping typeMapping)
		{
			var requiresPolicyInjection = configurationDetails.IsMarkedForPolicyInjection(typeMapping.From)
									   || TypeHasHandlerAttribute(typeMapping.From)
									   || TypeHasHandlerAttribute(typeMapping.To);
			
			return requiresPolicyInjection ? new InjectionMember[] { new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior<PolicyInjectionBehavior>() }
									       : new InjectionMember[0];
		}
		
		private static Boolean TypeHasHandlerAttribute(Type type)
		{
			return MemberInfoHasHandlerAttribute(type)
				|| type.GetMethods().Any(MemberInfoHasHandlerAttribute);
		}
		private static Boolean MemberInfoHasHandlerAttribute(MemberInfo mi)
		{
			return mi.GetCustomAttributes(typeof(HandlerAttribute), false).Any();
		}
	}
}
