using System;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Specifies that this interface should be registered to take part in policy injection. This is the same as manually applying a Unity type registration with both the PolicyInjection InjectionBehaviour and the Interface Interceptor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	public sealed class PolicyInjectionAttribute : Attribute
	{
	}
}
