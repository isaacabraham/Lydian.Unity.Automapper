using System;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Marks an interface or concrete class to explicitly be ignored by the auto-mapper.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class DoNotMapAttribute : Attribute { }
}
