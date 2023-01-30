namespace Tinyman.Model {

	/// <summary>
	/// Implemented by objects that represent a quote for an operation on Tinyman.
	/// </summary>
	public interface IQuote {

		/// <summary>
		/// Validator application ID.
		/// </summary>
		ulong ValidatorApplicationId { get; }
	}

}
