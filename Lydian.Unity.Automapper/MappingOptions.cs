namespace Lydian.Unity.Automapper
{
	/// <summary>
	/// Contains any options that can be used to help guide the auto-mapping process.
	/// </summary>
	public sealed class MappingOptions
	{
		/// <summary>
		/// Any custom behaviors to use when mapping.
		/// </summary>
		public MappingBehaviors Behaviors { get; set; }

		/// <summary>
		/// Initializes a new instance of the MappingOptions class.
		/// </summary>
		public MappingOptions() : this(MappingBehaviors.None) { }

		/// <summary>
		/// Initializes a new instance of the MappingOptions class.
		/// <param name="behaviors">Any custom behaviours to use when mapping.</param>
		/// </summary>
		public MappingOptions(MappingBehaviors behaviors)
		{
			Behaviors = behaviors;
		}
	}
}
