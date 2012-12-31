using System;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Core
{


	/// <summary>
	/// Creates auto-generated mappings from which to perform registrations on.
	/// </summary>
	internal interface ITypeMappingFactory
	{
		/// <summary>
		/// Creates all available mappings from the supplied list of types using the supplied behaviours.
		/// </summary>
		/// <param name="behaviors">The behaviors to use to guide mapping.</param>
		/// <param name="configurationDetails"></param>
		/// <returns>The collection of mappings that have been generated.</returns>
		/// <param name="types">The types from which to generate mappings.</param>
		IEnumerable<TypeMapping> CreateMappings(MappingBehaviors behaviors, AutomapperConfig configurationDetails, params Type[] types);
	}
}
