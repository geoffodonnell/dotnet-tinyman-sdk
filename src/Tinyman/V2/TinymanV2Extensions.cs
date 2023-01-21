using Org.BouncyCastle.Utilities;
using System;
using Tinyman.Model;

namespace Tinyman.V2 {

	public static class TinymanV2Extensions {

		public static byte[] ToApplicationArgument(this SwapType value) {

			if (value == SwapType.FixedInput) {
				return TinymanV2Constant.FixedInputAppArgument;
			}

			if (value == SwapType.FixedOutput) {
				return TinymanV2Constant.FixedOutputAppArgument;
			}

			throw new ArgumentException($"{nameof(value)} is not valid.");
		}

		public static byte[] ToApplicationNote(this string note) {

			if (String.IsNullOrWhiteSpace(note)) {
				return null;
			}

			return Strings.ToUtf8ByteArray(note);
		}

	}

}
