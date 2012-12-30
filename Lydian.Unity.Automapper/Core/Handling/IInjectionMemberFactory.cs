using Microsoft.Practices.Unity;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal interface IInjectionMemberFactory
	{
		InjectionMember[] CreateInjectionMembers(TypeMapping typeMapping);
	}
}
