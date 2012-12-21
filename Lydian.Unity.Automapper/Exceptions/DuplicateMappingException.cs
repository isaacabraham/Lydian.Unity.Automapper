using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// The exception raised when a concrete type has attempted to be mapped to an interface that is already registered into Unity.
	/// </summary>
	[Serializable]
	public class DuplicateMappingException : Exception
	{
		/// <summary>
		/// The error message to display when two concrete types are mapped to the same interface in a single-mapping environment.
		/// </summary>
		private const string MULTIPLE_CONCRETES_ERROR_MESSAGE = "Attempted to map at least two concrete types ({0} and {1}) to the same interface ({2}).";

		/// <summary>
		/// The error message to display when two concrete types are mapped to the same interface and the same name in a multi-mapping environment.
		/// </summary>
		private const string MULTIPLE_NAMES_ERROR_MESSAGE = "Attempted to map at least two concrete types ({0} and {1}) with the same name ('{2}').";
		/// <summary>
		/// The concrete that failed to map in as either the interface or the mapping name it was mapping to has already been registered into Unity.
		/// </summary>
		public Type DuplicateMappingConcrete { get; private set; }

		/// <summary>
		/// The concrete that has already been mapped into Unity against the interface.
		/// </summary>
		public Type MappedConcrete { get; private set; }

		/// <summary>
		/// The interface that the concrete was attempting to map against and has already been registered into Unity.
		/// </summary>
		public Type MappingInterface { get; private set; }

		/// <summary>
		/// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
		/// </summary>
		/// <param name="info">
		/// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. 
		/// </param>
		/// <param name="context">
		/// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. 
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic). 
		/// </exception>
		/// <filterpriority>2</filterpriority>
		/// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" /><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" /></PermissionSet>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info", "info is null.");

			info.AddValue("DuplicateMappingConcrete", DuplicateMappingConcrete.FullName);
			info.AddValue("MappedConcrete", MappedConcrete.FullName);
			info.AddValue("MappingInterface", MappingInterface.FullName);
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// Creates a new instance of the Duplicate Mapping Exception.
		/// </summary>
		/// <param name="mappingInterface">The source mapping interface.</param>
		/// <param name="mappedConcrete">The concrete that the interface is already mapped to.</param>
		/// <param name="duplicateMappingConcrete">The duplicate interface that attempted to map to it.</param>
		internal DuplicateMappingException(Type mappingInterface, Type mappedConcrete, Type duplicateMappingConcrete)
			: base(String.Format(CultureInfo.CurrentCulture,
									MULTIPLE_CONCRETES_ERROR_MESSAGE, 
									mappedConcrete.FullName, 
									duplicateMappingConcrete.FullName, 
									mappingInterface.FullName))
		{
			MappingInterface = mappingInterface;
			MappedConcrete = mappedConcrete;
			DuplicateMappingConcrete = duplicateMappingConcrete;
		}

		internal DuplicateMappingException(Type mappingInterface, Type mappedConcrete, Type duplicateMappingConcrete, String mappingName)
			: base(String.Format(CultureInfo.CurrentCulture,
									MULTIPLE_NAMES_ERROR_MESSAGE,
									mappedConcrete.FullName,
									duplicateMappingConcrete.FullName,
									mappingName))
		{
			MappingInterface = mappingInterface;
			MappedConcrete = mappedConcrete;
			DuplicateMappingConcrete = duplicateMappingConcrete;
		}
		/// <summary>
		/// Creates a new instance of the Duplicate Mapping Exception.
		/// </summary>
		public DuplicateMappingException() { }
		/// <summary>
		/// Creates a new instance of the Duplicate Mapping Exception.
		/// </summary>
		/// <param name="message">The message to use.</param>
		public DuplicateMappingException(String message) : base(message) { }
		/// <summary>
		/// Creates a new instance of the Duplicate Mapping Exception.
		/// </summary>
		/// <param name="message">The message to use.</param>
		/// <param name="inner">The inner exception details.</param>
		public DuplicateMappingException(String message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// Creates a new instance of the Duplicate Mapping Exception.
		/// </summary>
		/// <param name="info">The serialization info to add exception details to.</param>
		/// <param name="context">The streaming context.</param>
		protected DuplicateMappingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			DuplicateMappingConcrete = Type.GetType(info.GetString("DuplicateMappingConcrete"));
			MappedConcrete = Type.GetType(info.GetString("MappedConcrete"));
			MappingInterface = Type.GetType(info.GetString("MappingInterface"));
		}
	}
}
