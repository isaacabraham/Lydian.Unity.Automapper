using System;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Specifies the name of the mapping that this concrete should be registered into Unity as. For multimaps, if this attribute is not specified, the full name of the type is used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MapAsAttribute : Attribute
	{
		/// <summary>
		/// The name of the mapping.
		/// </summary>
		public String MappingName { get; private set; }

		/// <summary>
		/// Initializes a new instance of the NamedMappingAttribute class.
		/// </summary>
		/// <param name="mappingName">The name of this mapping.</param>
		public MapAsAttribute(String mappingName)
		{
			MappingName = mappingName;
		}
	}
}
