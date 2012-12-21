using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Represents a provider of UnityAutomappedConfig. Implement this interface if you wish to register type-specific mapping guidance instead of using attribute-based configuration on types directly.
	/// </summary>
	public interface IAutomapperConfigProvider
	{
		/// <summary>
		/// Returns an instance of the UnityAutomapperConfig to use for configuration.
		/// </summary>
		/// <returns></returns>
		AutomapperConfig CreateConfiguration();
	}
}
