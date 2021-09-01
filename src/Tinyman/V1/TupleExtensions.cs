using System;

namespace Tinyman.V1 {

	public static class TupleExtensions {

		public static Tuple<T, T> Select<T>(
			this Tuple<T, T> tuple, Func<T, T> selector) {

			return new Tuple<T, T>(
				selector(tuple.Item1),
				selector(tuple.Item2));
		}

	}

}
