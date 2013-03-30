using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace Lydian.Unity.Automapper.Test.Core.Handling
{
	[TestClass]
	public class InjectionMemberFactoryTests
	{
		[TestMethod]
		public void CreateInjectionMembers_TypeIsPolicyInjected_ReturnsInjectionMembers()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create().AndUsePolicyInjectionFor(typeof(String)));

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(String), typeof(String)));

			// Assert
			AssertHasInjectionMembers(members);
		}

		[TestMethod]
		public void CreateInjectionMembers_TypeIsNotPolicyInjected_ReturnsEmptyCollection()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create());

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(String), typeof(String)));

			// Assert
			Assert.IsFalse(members.Any());
		}

		[TestMethod]
		public void CreateInjectionMembers_FromTypeHasCallHandlerAttribute_ReturnsInjectionMembers()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create());

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(SampleInterface), typeof(SampleImplementer)));

			// Assert
			AssertHasInjectionMembers(members);
		}

		[TestMethod]
		public void CreateInjectionMembers_FromTypeMethodHasCallHandlerAttribute_ReturnsInjectionMembers()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create());

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(OtherInterface), typeof(SampleImplementer)));

			// Assert
			AssertHasInjectionMembers(members);
		}

		[TestMethod]
		public void CreateInjectionMembers_ToTypeHasCallHandlerAttribute_ReturnsInjectionMembers()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create());

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(EmptyInterface), typeof(EmptyInterfaceAttributeOnType)));

			// Assert
			AssertHasInjectionMembers(members);
		}

		[TestMethod]
		public void CreateInjectionMembers_ToTypeMethodHasCallHandlerAttribute_ReturnsInjectionMembers()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create());

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(EmptyInterface), typeof(EmptyInterfaceAttributeOnMethod)));

			// Assert
			AssertHasInjectionMembers(members);
		}

		private static void AssertHasInjectionMembers(InjectionMember[] members)
		{
			Assert.IsInstanceOfType(members.First(), typeof(Interceptor<InterfaceInterceptor>));
			Assert.IsInstanceOfType(members.Last(), typeof(InterceptionBehavior<PolicyInjectionBehavior>));
			Assert.AreEqual(2, members.Count());
		}

		[SampleHandler]
		public interface SampleInterface
		{
			void Foo();
		}
		public interface OtherInterface
		{
			[SampleHandler]
			void Foo();
		}
		public class SampleImplementer : SampleInterface, OtherInterface
		{
			public void Foo()
			{
				throw new NotImplementedException();
			}
		}
		public class SampleHandlerAttribute : HandlerAttribute
		{
			public override ICallHandler CreateHandler(IUnityContainer container)
			{
				return new Mock<ICallHandler> { DefaultValue = DefaultValue.Mock }.Object;
			}
		}
		public interface EmptyInterface
		{
			void Foo();
		}
		[SampleHandler]
		public class EmptyInterfaceAttributeOnType : EmptyInterface
		{
			public void Foo()
			{
				throw new NotImplementedException();
			}
		}
		public class EmptyInterfaceAttributeOnMethod : EmptyInterface
		{
			[SampleHandler]
			public void Foo()
			{
				throw new NotImplementedException();
			}
		}
	}
}
