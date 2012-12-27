using System;
using System.Collections.Generic;
using System.Linq;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Contains extension methods that operate over types, or collections of type, relating to automapping.
	/// </summary>
	internal static class ExtensionMethods
	{
		/// <summary>
		/// Generates a tuple containing a comparison interface as well as a "target" interface that is bound to the type of the concrete.
		/// </summary>
		/// <param name="concrete">The concrete type to generate the interface tuple from.</param>
		/// <returns>A tuple containing two types. Item1 = the type to compare with an interface for a match. Item2 = the type to register as the "from" interface.</returns>
		public static IEnumerable<Tuple<Type, Type>> GetGenericallyOpenInterfaces(this Type concrete)
		{
			return from concreteInterface in concrete.GetInterfaces()
				   let comparisonInterface = concreteInterface.IsGenericType ?
										concreteInterface.GetGenericTypeDefinition() :
										concreteInterface
				   let destinationInterface = concrete.IsGenericType ? comparisonInterface : concreteInterface
				   select Tuple.Create(comparisonInterface, destinationInterface);
		}

		/// <summary>
		/// Determines the explicit mapping name that a concrete type should be registered into Unity with.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns>The mapping name. If the type does not have the MapAsAttribute, returns null.</returns>
		public static String GetMapAsName(this Type type)
		{
			var mapAsAttribute = type.GetCustomAttributes(typeof(MapAsAttribute), false).FirstOrDefault() as MapAsAttribute;
			if (mapAsAttribute == null)
				return null;

			return mapAsAttribute.MappingName;
		}

		public static Boolean HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			return type.GetCustomAttributes(typeof(TAttribute), false).Any();
		}
	}
}