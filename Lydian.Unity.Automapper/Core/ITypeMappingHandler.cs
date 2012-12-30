using Microsoft.Practices.Unity;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Core
{
	/// <summary>
	/// Carries out registrations on the container.
	/// </summary>
	internal interface ITypeMappingHandler
	{
		/// <summary>
		/// Performs registrations using a supplied set of Mappings and guiding behaviors on a container.
		/// </summary>
		/// <param name="target">The container to use to perform registrations on.</param>
		/// <param name="typeMappings">The mappings to use.</param>
		/// <returns></returns>
		IEnumerable<ContainerRegistration> PerformRegistrations(IUnityContainer target, IEnumerable<TypeMapping> typeMappings);
	}
}
