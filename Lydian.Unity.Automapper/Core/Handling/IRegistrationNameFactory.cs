using System;

namespace Lydian.Unity.Automapper.Core.Handling
{
	internal interface IRegistrationNameFactory
	{
		String GetRegistrationName(TypeMapping typeMapping);
	}
}
