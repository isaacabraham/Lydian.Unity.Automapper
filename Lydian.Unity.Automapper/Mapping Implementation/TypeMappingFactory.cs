using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Lydian.Unity.Automapper
{
	internal sealed class TypeMappingFactory : ITypeMappingFactory
	{
		public IEnumerable<TypeMapping> CreateMappings(IEnumerable<Type> types, MappingBehaviors behaviors, AutomapperConfig configurationDetails)
		{
			var results = from availableInterface in types.AsParallel()
														  .Where(type => type.IsInterface)
														  .Where(configurationDetails.IsMappable)
						  from concrete in types.Where(type => !type.IsInterface)
												.Where(configurationDetails.IsMappable)
						  from concreteInterface in concrete.GetGenericallyOpenInterfaces()
															.Where(ci => ci.Item1 == availableInterface)
						  let matchingPair = new { concrete, concreteInterface }
						  group matchingPair by matchingPair.concreteInterface.Item2 into mappingsForAnInterface
						  from mapping in mappingsForAnInterface
						  select new TypeMapping(mapping.concreteInterface.Item2, mapping.concrete);

			if (!behaviors.HasFlag(MappingBehaviors.CollectionRegistration))
				return results.ToArray();

			return CreateMultimapCollections(results)
							.Union(results)
							.ToArray();
		}

		private static IEnumerable<TypeMapping> CreateMultimapCollections(IEnumerable<TypeMapping> mappings)
		{
			Contract.Requires(mappings != null);
			return mappings
					.GroupBy(m => m.From)
					.Where(g => g.Count() > 1)
					.Select(g => new TypeMapping(GetGenericTypeSafely(typeof(IEnumerable<>), g.Key), GetGenericTypeSafely(typeof(UnityCollectionFacade<>), g.Key)));
		}

		private static Type GetGenericTypeSafely(Type source, params Type[] args)
		{
			if (!source.IsGenericTypeDefinition)
				throw new ArgumentException("source must be a generic type definition", "source");
			if (source.GetGenericArguments().Length != args.Length)
				throw new ArgumentException("incorrect number of arguments", "args");
			return source.MakeGenericType(args);
		}
	}
}
