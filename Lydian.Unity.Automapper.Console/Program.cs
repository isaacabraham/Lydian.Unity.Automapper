using ConfigProviderAssembly;
using Lydian.Disposable;
using Lydian.Disposable.Switches;
using Lydian.Unity.Automapper;
using Lydian.Unity.Automapper.Test.TestAssembly;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ConsoleApplication1
{
	#region Smoke test types
	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IMyGenericClass<TFirst, TSecond> { }
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed class MyGenericClass<TFirst, TSecond> : IMyGenericClass<TFirst, TSecond>, IEnumerable<TFirst>
	{
		IEnumerator<TFirst> IEnumerable<TFirst>.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
	public class ClosedGenericClass : IMyGenericClass<String, Int32> { }
	public class AnotherClosedGenericClass : IMyGenericClass<Boolean, Object> { }
	public interface ISimpleInterface { }
	public class SimpleClass : ISimpleInterface { }

	[Multimap]
	public interface IMultimap { }
	public class MultimapClass : IMultimap { }
	public class MultimapClassTwo : IMultimap { }

	public interface INamedInterface { }
	[MapAs("Test")]
	public class NamedConcrete : INamedInterface { }

	[PolicyInjection]
	public interface IPolicyMapping { void Foo(String testValue = null); }
	public class PolicyMappingConcrete : IPolicyMapping
	{
		[ApplySmokeTestPolicy]
		public void Foo(String testValue = null) { }
	}
	public class ApplySmokeTestPolicyAttribute : HandlerAttribute, ICallHandler
	{
		public override ICallHandler CreateHandler(IUnityContainer container)
		{
			return this;
		}
		public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext) { return getNext()(input, getNext); }
	}
	#endregion

	class Program
	{
		static void Main()
		{
			using (new ColorSwitch(ConsoleColor.Yellow).AsDisposable())
			using (new DisposableAdapter<InvertedColourSwitch>())
				Console.WriteLine("Starting Unity Automapper Smoke Test...");
			Console.WriteLine();

			PerformSmokeTest(MappingBehaviors.None);
			PerformSmokeTest(MappingBehaviors.MultimapByDefault);
			PerformSmokeTest(MappingBehaviors.CollectionRegistration);
			PerformSmokeTest(MappingBehaviors.MultimapByDefault | MappingBehaviors.CollectionRegistration);

			using (new ColorSwitch(ConsoleColor.Yellow).AsDisposable())
			using (new DisposableAdapter<InvertedColourSwitch>())
				Console.WriteLine("All smoke tests completed!");
			Console.ReadLine();
		}

		private static void PerformSmokeTest(MappingBehaviors behaviors)
		{
			Console.WriteLine();
			using (var container = new UnityContainer())
			{
				using (new ColorSwitch(ConsoleColor.Yellow).AsDisposable())
				using (new DisposableAdapter<InvertedColourSwitch>())
					Console.WriteLine("Smoke testing using the following behaviors: {0}.", behaviors.ToString());

				try
				{
					container.AutomapAssemblies(new MappingOptions { Behaviors = behaviors }, Assembly.GetExecutingAssembly().FullName, "ConfigProviderAssembly", "Lydian.Unity.Automapper.Test.TestAssembly", "Lydian.Unity.Automapper.Test.TestAssemblyTwo");
				}
				catch (Exception ex)
				{
					using (new ColorSwitch(ConsoleColor.Red).AsDisposable())
					using (new DisposableAdapter<InvertedColourSwitch>())
					{
						Console.WriteLine("Failed during automapping phase: {0}.", ex.Message);
						Console.WriteLine();
					}
				}

				TestRegistration<ISimpleInterface>(container, "Non generic mapping");
				TestRegistration<IMyGenericClass<Boolean, String>>(container, "Open generic mapping");
				TestRegistration<IMyGenericClass<String, Int32>>(container, "First closed generic mapping");
				TestRegistration<IMyGenericClass<Boolean, Object>>(container, "Second closed generic mapping");
				TestRegistration<IMultimap>(container, "Multiple mappings", false);
				TestRegistration<INamedInterface>(container, "Named mapping", mappingName: "Test");

				TestRegistration<SingletonInterface>(container, "Provider-based singleton mapping");
				TestRegistration<MultimappingInterface>(container, "Provider-based multi-mapping", singleMapping: false);

				TestRegistration<IDependencyInversionPrinciple>(container, "Decoupled DIP mapping");

				TestPolicyRegistration(container);
			}
		}
		private static void TestRegistration<TInterface>(IUnityContainer container, String message, Boolean singleMapping = true, String mappingName = null)
		{
			try
			{
				using (new DisposableAdapter<InvertedColourSwitch>())
					Console.WriteLine("{0}...", message);

				if (singleMapping)
				{
					var concrete = container.Resolve<TInterface>(mappingName);
					Console.WriteLine("\tResolved from {0} to {1}", typeof(TInterface).ToString(), concrete.GetType().ToString());
				}
				else
				{
					var concretes = container.ResolveAll<TInterface>();
					foreach (var concrete in concretes)
						Console.WriteLine("\tResolved from {0} to {1}", typeof(TInterface).ToString(), concrete.GetType().ToString());
				}

				using (new DisposableAdapter<InvertedColourSwitch>())
					Console.WriteLine("Done!");
			}
			catch (Exception ex)
			{
				using (new ColorSwitch(ConsoleColor.Red).AsDisposable())
				using (new DisposableAdapter<InvertedColourSwitch>())
					Console.WriteLine("Failed during mapping: {0}", ex.Message);
			}
			Console.WriteLine();
		}
		private static void TestPolicyRegistration(UnityContainer container)
		{
			using (new DisposableAdapter<InvertedColourSwitch>())
				Console.WriteLine("{0}...", "Testing automatic policy injection.");

			if (container.Resolve<IPolicyMapping>() is PolicyMappingConcrete)
			{
				using (new ColorSwitch(ConsoleColor.Red).AsDisposable())
				using (new DisposableAdapter<InvertedColourSwitch>())
					Console.WriteLine("Failed during mapping: policy injection did not take place");
			}

			using (new DisposableAdapter<InvertedColourSwitch>())
				Console.WriteLine("Done!");
		}
	}
}
