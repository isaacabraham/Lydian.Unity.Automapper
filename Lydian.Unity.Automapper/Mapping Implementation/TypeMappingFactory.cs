using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Creates auto-generated mappings from which to perform registrations on.
	/// </summary>
	internal sealed class TypeMappingFactory
	{
		private TypeMappingFactory() { }

		/// <summary>
		/// Creates all available mappings from the supplied list of types using the supplied behaviours.
		/// </summary>
		/// <param name="types">The types from which to generate mappings.</param>
		/// <param name="behaviors">The behaviors to use to guide mapping.</param>
		/// <returns>The collection of mappings that have been generated.</returns>
		/// <param name="configurationDetails"></param>
		public static IEnumerable<TypeMapping> CreateMappings(IEnumerable<Type> types, MappingBehaviors behaviors, AutomapperConfig configurationDetails)
		{

			var results = (from availableInterface in types.AsParallel().WhereInterface().Where(availableInterface => configurationDetails.IsMappable(availableInterface))
			               from concrete in types.WhereConcrete().Where(concrete => configurationDetails.IsMappable(concrete))
			               from concreteInterface in concrete.GetGenericallyOpenInterfaces().Where(ci => ci.Item1 == availableInterface)
			               let matchingPair = new { concrete, concreteInterface }
			               group matchingPair by matchingPair.concreteInterface.Item2 into mappingsForAnInterface
			               from mapping in mappingsForAnInterface
			               select new TypeMapping(mapping.concreteInterface.Item2, mapping.concrete)
						  ).ToArray();

			if (!behaviors.HasFlag(MappingBehaviors.CollectionRegistration))
				return results;

			return CreateMultimapCollections(results)
							.Union(results)
							.ToArray();
		}

		private static TypeMapping[] CreateMultimapCollections(IEnumerable<TypeMapping> mappings)
		{
			Contract.Requires(mappings != null);
			return mappings
					.GroupBy(m => m.From)
					.Where(g => g.Count() > 1)
					.Select(g => new TypeMapping(GetGenericTypeSafely(typeof(IEnumerable<>), g.Key), GetGenericTypeSafely(typeof(UnityCollectionFacade<>), g.Key)))
					.ToArray();
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
