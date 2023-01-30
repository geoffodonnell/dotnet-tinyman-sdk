namespace Tinyman.Model {

    /// <summary>
    /// Tinyman swap type enumeration.
    /// </summary>
    public enum SwapType {

        /// <summary>
        /// Undefined or unset.
        /// </summary>
        Undefined,

        /// <summary>
        /// Represents a swap with a fixed input amount and a variable output amount.
        /// </summary>
        FixedInput,

        /// <summary>
        /// Represents a swap with a fixed output amount and a variable input amount.
        /// </summary>
        FixedOutput
    }

}
