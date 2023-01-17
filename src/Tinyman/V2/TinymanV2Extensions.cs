using Org.BouncyCastle.Utilities;
using System;
using Tinyman.Model;

namespace Tinyman.V2 {

	public static class TinymanV2Extensions {

		public static byte[] ToApplicationArgument(this SwapType value) {

			return value switch {
				SwapType.FixedInput => Constant.FixedInputAppArgument,
				SwapType.FixedOutput => Constant.FixedOutputAppArgument,
				_ => throw new ArgumentException($"{nameof(value)} is not valid.")
			};
		}

		public static byte[] ToApplicationNote(this string note) {

			if (String.IsNullOrWhiteSpace(note)) {
				return null;
			}

			return Strings.ToUtf8ByteArray(note);
		}

	}

}
