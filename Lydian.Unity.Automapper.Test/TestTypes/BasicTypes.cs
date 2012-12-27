namespace Lydian.Unity.Automapper.Test.Types
{
	public interface IInterface { }
	public class InterfaceImplementation : IInterface { }
	public class InterfaceImplementationTwo : IInterface { }

	public interface IOther { }
	public class OtherImplementation : IOther { }
	public class OtherImplementationTwo : IOther { }

	public class CompoundImplementation : IInterface, IOther { }



						public interface IInterfaceOne { }
						public interface IInterfaceTwo { }
	[MapAs("Test")]		public class NamedConcrete : IInterfaceOne { }
						public class ConcreteOne : IInterfaceOne { }
						public class DuplicateConcrete : IInterfaceOne { }
	[PolicyInjection]	public interface IPolicyInterface
						{
							[ExampleCallHandler] void Foo();
						}
						public interface INonPolicyInterface { void Foo(); }
						public class PolicyConcrete : IPolicyInterface, INonPolicyInterface { public void Foo () { } }
}
