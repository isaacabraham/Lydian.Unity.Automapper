using System.Linq;
using Lydian.Unity.Automapper.Core;
using Lydian.Unity.Automapper.Core.Handling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Lydian.Unity.Automapper.Test.Core.Handling
{
	[TestClass]
	public class RegistrationNameFactoryTests
	{
		[TestMethod]
		public void GetRegistrationName_NoMultimappingAndDefaultName_ReturnsNull()
		{
			var factory = new RegistrationNameFactory(GetFactory(), new TypeMapping[0], MappingBehaviors.None);

			// Act
			var name = factory.GetRegistrationName(new TypeMapping(typeof(Object), typeof(String)));

			// Assert
			Assert.IsNull(name);
		}

		[TestMethod]
		public void GetRegistrationName_NamedItem_ReturnsThatName()
		{
			var factory = new RegistrationNameFactory(GetFactory(namedMapping:"TEST"), new TypeMapping[0], MappingBehaviors.None);

			// Act
			var name = factory.GetRegistrationName(new TypeMapping(typeof(Object), typeof(String)));
			
			// Assert
			Assert.AreEqual("TEST", name);
		}

		[TestMethod]
		public void GetRegistrationName_MultimappingAndDefaultName_ReturnsFullName()
		{
			var factory = new RegistrationNameFactory(GetFactory(true), new TypeMapping[0], MappingBehaviors.None);

			// Act
			var name = factory.GetRegistrationName(new TypeMapping(typeof(Object), typeof(String)));

			// Assert
			Assert.AreEqual("System.String", name);
		}

		[TestMethod]
		public void GetRegistrationName_MultimappingAndNamedItem_ReturnsThatName()
		{
			var factory = new RegistrationNameFactory(GetFactory(true, "TEST"), new TypeMapping[0], MappingBehaviors.None);

			// Act
			var name = factory.GetRegistrationName(new TypeMapping(typeof(Object), typeof(String)));

			// Assert
			Assert.AreEqual("TEST", name);
		}

		[TestMethod]
		public void GetRegistrationName_MultimappingBehaviorButOnlyOneMappingForThatType_ReturnsNull()
		{
			var mappings = new[] { new TypeMapping(typeof(Object), typeof(String)) };
			var factory = new RegistrationNameFactory(GetFactory(), mappings, MappingBehaviors.MultimapByDefault);

			// Act
			var name = factory.GetRegistrationName(mappings.First());

			// Assert
			Assert.IsNull(name);
		}

		[TestMethod]
		public void GetRegistrationName_MultimappingBehaviorManyMappingsForThatType_ReturnsDefaultName()
		{
			var mappings = new[]
			{
                new TypeMapping(typeof(Object), typeof(String)),
				new TypeMapping(typeof(Object), typeof(Exception))
			};

			var factory = new RegistrationNameFactory(GetFactory(), mappings, MappingBehaviors.MultimapByDefault);

			// Act
			var name = factory.GetRegistrationName(mappings.First());

			// Assert
			Assert.AreEqual("System.String", name);
		}

		[TestMethod]
		public void GetRegistrationName_MultimappingBehaviorManyMappingsAcrossManyTypes_ReturnsNull()
		{
			var mappings = new[]
			{
                new TypeMapping(typeof(Object), typeof(String)),
				new TypeMapping(typeof(Exception), typeof(Exception))
			};

			var factory = new RegistrationNameFactory(GetFactory(), mappings, MappingBehaviors.MultimapByDefault);

			// Act
			var name = factory.GetRegistrationName(mappings.First());

			// Assert
			Assert.IsNull(name);
		}
		
		private static AutomapperConfig GetFactory(Boolean useMultiMapping = false, String namedMapping = null)
		{
			var config = AutomapperConfig.Create();
			if (useMultiMapping)
				config = config.AndUseMultimappingFor(typeof(Object));
			if (namedMapping != null)
				config = config.AndUseNamedMappingFor(typeof(String), namedMapping);

			return config;
		}
	}
}
