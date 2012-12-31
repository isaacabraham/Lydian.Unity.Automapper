using System;
using System.Collections.Generic;
using System.Linq;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal class RegistrationNameFactory : IRegistrationNameFactory
	{
		private readonly AutomapperConfig configurationDetails;
		private readonly MappingBehaviors mappingBehaviors;
		private readonly IEnumerable<Type> multimapTypes;

		public RegistrationNameFactory(AutomapperConfig configurationDetails, IEnumerable<TypeMapping> typeMappings, MappingBehaviors mappingBehaviors)
		{
			this.mappingBehaviors = mappingBehaviors;
			this.configurationDetails = configurationDetails;
			multimapTypes = typeMappings
								.GroupBy(t => t.From)
								.Where(t => t.Count() > 1)
								.Select(group => group.Key)
								.ToArray();
		}

		public String GetRegistrationName(TypeMapping typeMapping)
		{
			var namedMappingRequested = configurationDetails.IsMultimap(typeMapping.From) || configurationDetails.IsNamedMapping(typeMapping.To);
			var namedMappingRequired = mappingBehaviors.HasFlag(MappingBehaviors.MultimapByDefault) && multimapTypes.Contains(typeMapping.From);
			return (namedMappingRequested || namedMappingRequired) ? configurationDetails.GetNamedMapping(typeMapping)
																   : null;
		}
	}
}
