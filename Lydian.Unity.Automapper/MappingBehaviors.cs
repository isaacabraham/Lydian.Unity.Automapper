using System;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Specifies mapping behaviours to guide the automapping process.
	/// </summary>
	[Flags]
	public enum MappingBehaviors
	{
		/// <summary>
		/// No custom behaviours are specified.
		/// </summary>
		None = 0,
		/// <summary>
		/// If two types are mapped to the same interface, even if you do not specify the MultimapAttribute on the interface, multimap behaviour will be used.
		/// </summary>
		MultimapByDefault = 1,
		/// <summary>
		/// If an interface is multimapped, an extra registration will be made of the generic IEnumerable of T, allowing you to easily retrieve all registrations for the interface.
		/// </summary>
		CollectionRegistration = 2,
	}
}