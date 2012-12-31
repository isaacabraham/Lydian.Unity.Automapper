using System;
namespace Lydian.Unity.Automapper.Test.Types
{
	// Plain interface mappings
	public interface IInterface { void Foo(); }
	public class InterfaceImplementation : IInterface { public void Foo() { } }
	public class InterfaceImplementationTwo : IInterface { public void Foo() { } }
	public class OpenGenericImplementation<T> : IInterface { public void Foo() { } }
	public class ClosedGenericImplementation : OpenGenericImplementation<Boolean> { }

	public interface IOther { }
	public class OtherImplementation : IOther { }
	public class OtherImplementationTwo : IOther { }

	public class CompoundImplementation : IInterface, IOther { public void Foo() { } }


	// Generic interface mappings
	public interface IGenericInterface<T1, T2> { }
	public class OpenGenericConcrete<T1, T2> : IGenericInterface<T1, T2> { }
	public class OpenGenericConcreteTwo<T1, T2> : IGenericInterface<T1, T2> { }
	public class ClosedGenericConcrete : IGenericInterface<String, Boolean> { }

}
