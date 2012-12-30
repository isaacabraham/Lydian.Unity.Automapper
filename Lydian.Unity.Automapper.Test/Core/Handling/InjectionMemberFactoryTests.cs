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
		private Mock<IUnityContainer> target;

		[TestInitialize]
		public void Setup()
		{
			target = new Mock<IUnityContainer>();
		}

		[TestMethod]
		public void Ctor_NoPolicyInjectionTypes_DoesNotAddExtension()
		{
			// Act
			new InjectionMemberFactory(AutomapperConfig.Create(), target.Object);

			// Assert
			target.Verify(tar => tar.AddExtension(It.IsAny<UnityContainerExtension>()), Times.Never());
		}

		[TestMethod]
		public void Ctor_SomePolicyInjectionTypes_AddsExtension()
		{
			// Act
			new InjectionMemberFactory(AutomapperConfig.Create().AndUsePolicyInjectionFor(typeof(String)), target.Object);

			// Assert
			target.Verify(tar => tar.AddExtension(It.IsAny<UnityContainerExtension>()), Times.Once());
		}

		[TestMethod]
		public void CreateInjectionMembers_TypeIsPolicyInjected_ReturnsInjectionMembers()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create().AndUsePolicyInjectionFor(typeof(String)), target.Object);

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(String), typeof(String)));

			// Assert
			Assert.IsInstanceOfType(members.First(), typeof(Interceptor<InterfaceInterceptor>));
			Assert.IsInstanceOfType(members.Last(), typeof(InterceptionBehavior<PolicyInjectionBehavior>));
			Assert.AreEqual(2, members.Count());
		}

		[TestMethod]
		public void CreateInjectionMembers_TypeIsNotPolicyInjected_ReturnsEmptyCollection()
		{
			var factory = new InjectionMemberFactory(AutomapperConfig.Create(), target.Object);

			// Act
			var members = factory.CreateInjectionMembers(new TypeMapping(typeof(String), typeof(String)));

			// Assert
			Assert.IsFalse(members.Any());
		}
	}
}
