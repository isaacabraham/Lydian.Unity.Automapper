using Microsoft.Practices.Unity;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Carries out registrations on the container.
	/// </summary>
	internal interface ITypeMappingHandler
	{
		/// <summary>
		/// Performs registrations using a supplied set of Mappings and guiding behaviors on a container.
		/// </summary>
		/// <param name="container">The cotainer to use to perform registrations.</param>
		/// <param name="typeMappings">The mappings to use.</param>
		/// <param name="mappingBehaviors">The behaviours to help guide the registration process.</param>
		/// <param name="configurationDetails">Details of the mappings, such as which types to ignore or to use policy injection with.</param>
		/// <returns></returns>
		IEnumerable<ContainerRegistration> PerformRegistrations(IUnityContainer container, IEnumerable<TypeMapping> typeMappings, MappingBehaviors mappingBehaviors, AutomapperConfig configurationDetails);
	}
}
