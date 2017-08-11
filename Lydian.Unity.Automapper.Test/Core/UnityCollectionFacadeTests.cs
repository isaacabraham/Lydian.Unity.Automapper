using Lydian.Unity.Automapper.Core;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Lydian.Unity.Automapper.Test.Core
{
    [TestClass]
	public class UnityCollectionFacadeTests
	{
		[TestMethod]
		public void GetEnumerator_NoResolvedTypes_GetsNoneBack()
		{
			var facade = GetFacade();

			// Act
			var enumerator = facade.GetEnumerator();
			var result = enumerator.MoveNext();

			// Assert
			Assert.IsFalse(result);
			Assert.IsNull(enumerator.Current);
		}

		[TestMethod]
		public void GetEnumerator_OneResolvedItem_GetsItBack()
		{
			var facade = GetFacade("HELLO");

			// Act
			var enumerator = facade.GetEnumerator();
			enumerator.MoveNext();

			// Assert
			Assert.AreEqual("HELLO", enumerator.Current);
		}

		[TestMethod]
		public void GetEnumerator_ManyResolvedItems_GetsThemBack()
		{
			var facade = GetFacade("HELLO", "GOODBYE");

			// Act
			var enumerator = facade.GetEnumerator();

			// Assert
			enumerator.MoveNext();
			Assert.AreEqual("HELLO", enumerator.Current);
			enumerator.MoveNext();
			Assert.AreEqual("GOODBYE", enumerator.Current);
			var result = enumerator.MoveNext();
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void GetEnumerator_AddedToContainerAfterResolution_StillGetsIt()
		{
			var container = new UnityContainer();
			var facade = new UnityCollectionFacade<String>(container);
			container.RegisterInstance("HELLO", "HELLO");

			// Act		
			var enumerator = facade.GetEnumerator();
			var result = enumerator.MoveNext();

			// Assert
			Assert.AreEqual("HELLO", enumerator.Current);
		}

		private static UnityCollectionFacade<String> GetFacade(params String[] items)
		{
			var container = new UnityContainer();
			foreach (var item in items)
				container.RegisterInstance(item, item);
			return new UnityCollectionFacade<String>(container);
		}
	}
}
