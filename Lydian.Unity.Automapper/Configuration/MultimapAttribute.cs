using System;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Marks an interface as a multi-map i.e. many concrete types can be mapped to this interface. Each registration into Unity will be named based on the full name of the concrete type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	public sealed class MultimapAttribute : Attribute { }
}
