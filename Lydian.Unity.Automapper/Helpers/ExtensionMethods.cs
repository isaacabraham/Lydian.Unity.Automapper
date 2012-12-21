using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
		/// Gets all interfaces from a collection of types.
		/// </summary>
		/// <param name="types">The source collection of types.</param>
		public static Type[] WhereInterface(this IEnumerable<Type> types)
		{
			Contract.Ensures(Contract.Result<Type[]>() != null);
			return types.AsParallel()
							.Where(t => t.IsInterface)
							.ToArray() ?? new Type[0];
		}

		/// <summary>
		/// Gets all concrete (non-interface) types from a collection of types.
		/// </summary>
		/// <param name="types">The source collection of types.</param>
		public static Type[] WhereConcrete(this IEnumerable<Type> types)
		{
			return types.AsParallel()
						.Where(t => !t.IsInterface)
						.ToArray();
		}

		/// <summary>
		/// Determines whether this type is allowed to participate in automapping.
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static Boolean IsMappable(this Type type)
		{
			return !type.HasAttribute<DoNotMapAttribute>();
		}

		/// <summary>
		/// Determines whether this type shoulde be registered into Unity as a Singleton.
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static Boolean IsSingleton(this Type type)
		{
			return type.HasAttribute<SingletonAttribute>();
		}

		/// <summary>
		/// Determines whether this interface can be registered into Unity against multiple named concrete implementations.
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static Boolean IsMultimap(this Type type)
		{
			return HasAttribute<MultimapAttribute>(type);
		}

		/// <summary>
		/// Determines whether this type should be registered with Policy Injection.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Boolean HasPolicyInjection(this Type type)
		{
			return HasAttribute<PolicyInjectionAttribute>(type);
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

		private static Boolean HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			return type.GetCustomAttributes(typeof(TAttribute), false).Any();
		}
	}
}
