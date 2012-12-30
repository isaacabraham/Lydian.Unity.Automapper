using System;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Test
{
	internal class TestableUnityConfigProvider : IAutomapperConfigProvider
	{
		private static List<Type> doNotMaps;
		private static List<Type> singletons;
		private static List<Type> multimaps;
		private static List<Type> policyInjections;
		private static List<Tuple<Type, String>> namedMappings;

		public static void Reset()
		{
			singletons = new List<Type>();
			multimaps = new List<Type>();
			namedMappings = new List<Tuple<Type, String>>();
			doNotMaps = new List<Type>();
			policyInjections = new List<Type>();
		}

		public static void AddDoNotMaps(params Type[] types)
		{
			doNotMaps.AddRange(types);
		}

		public static void AddNamedMapping(Type type, String name)
		{
			namedMappings.Add(Tuple.Create(type, name));
		}

		public static void AddMultimaps(params Type[] types)
		{
			multimaps.AddRange(types);
		}

		public static void AddSingletons(params Type[] types)
		{
			singletons.AddRange(types);
		}

		public static void AddPolicyInjection(params Type[] types)
		{
			policyInjections.AddRange(types);
		}

		public AutomapperConfig CreateConfiguration()
		{
			var config = AutomapperConfig.Create()
								   .AndMapAsSingleton(singletons.ToArray())
								   .AndUsePolicyInjectionFor(policyInjections.ToArray())
								   .AndUseMultimappingFor(multimaps.ToArray())
								   .AndDoNotMapFor(doNotMaps.ToArray());

			foreach (var mapping in namedMappings)
				config = config.AndUseNamedMappingFor(mapping.Item1, mapping.Item2);

			return config;
		}
	}
}
