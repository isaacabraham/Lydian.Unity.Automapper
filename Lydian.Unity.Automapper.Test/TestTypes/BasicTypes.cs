namespace Lydian.Unity.Automapper.Test.Types
{
	[Singleton]			public interface ISingleton { }
	[DoNotMap]			public class IgnoredExplicitConcrete : IInterfaceOne { }
	[DoNotMap]			public interface IIgnoredInterface { }
						public interface IInterfaceOne { }
						public interface IInterfaceTwo { }
	[MapAs("Test")]		public class NamedConcrete : IInterfaceOne { }
						public class ConcreteOne : IInterfaceOne { }
						public class DuplicateConcrete : IInterfaceOne { }
						public class ConcreteTwo : IInterfaceTwo { }
						public class CompoundConcrete : IInterfaceOne, IInterfaceTwo { }
						public class SingletonConcrete : ISingleton { }
						public class IgnoredConcrete : IIgnoredInterface { }
	[PolicyInjection]	public interface IPolicyInterface
						{
							[ExampleCallHandler] void Foo();
						}
						public interface INonPolicyInterface { void Foo(); }
						public class PolicyConcrete : IPolicyInterface, INonPolicyInterface { public void Foo () { } }
}
