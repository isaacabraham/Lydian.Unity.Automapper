using System;
using System.Collections.Generic;

namespace Lydian.Unity.Automapper.Test
{
	internal class TestableUnityConfigProvider : IAutomapperConfigProvider
	{
		private static List<Type> doNotMaps { get; set; }
		static public Type[] Singletons { get; set; }
		private static List<Type> multimaps { get; set; }
		private static List<Tuple<Type, String>> namedMappings { get; set; }

		public static void Reset()
		{
			Singletons = new Type[0];
			multimaps = new List<Type>();
			namedMappings = new List<Tuple<Type, String>>();
			doNotMaps = new List<Type>();
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

		public AutomapperConfig CreateConfiguration()
		{
			var config = AutomapperConfig.Create()
								   .AndMapAsSingleton(Singletons)
								   .AndUseMultimappingFor(multimaps.ToArray())
								   .AndDoNotMapFor(doNotMaps.ToArray());

			foreach (var mapping in namedMappings)
				config = config.AndUseNamedMappingFor(mapping.Item1, mapping.Item2);

			return config;
		}
	}
}
