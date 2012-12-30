using System;
namespace Lydian.Unity.Automapper.Test.Types
{
	public interface IInterface { [ExampleCallHandler] void Foo(); }
	public class InterfaceImplementation : IInterface { public void Foo() { } }
	public class InterfaceImplementationTwo : IInterface { public void Foo() { } }
	public class OpenGenericImplementation<T> : IInterface { public void Foo() { } }
	public class ClosedGenericImplementation : OpenGenericImplementation<Boolean> { }


	public interface IOther { }
	public class OtherImplementation : IOther { }
	public class OtherImplementationTwo : IOther { }

	public class CompoundImplementation : IInterface, IOther { public void Foo() { } }
}
