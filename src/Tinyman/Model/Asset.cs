using System.Collections.Generic;

namespace Tinyman.Model {

	/// <summary>
	/// An Algorand asset.
	/// </summary>
	public class Asset {

		/// <summary>
		/// Asset identifer (0 for $ALGO).
		/// </summary>
		public virtual ulong Id { get; set; }

		/// <summary>
		/// Asset name.
		/// </summary>
		public virtual string Name { get; set; }

		/// <summary>
		/// Asset unit name.
		/// </summary>
		public virtual string UnitName { get; set; }

		/// <summary>
		/// Asset decimals.
		/// </summary>
		public virtual int Decimals { get; set; }

		/// <summary>
		/// Construct a new instance.
		/// </summary>
		public Asset() { }

		public override string ToString() {
			return $"{Name} ({Id})";
		}

		public override bool Equals(object obj) {
			return obj?.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode() {
			var hashCode = 32852567;
			hashCode = hashCode * -1521134295 + Id.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UnitName);
			hashCode = hashCode * -1521134295 + Decimals.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Asset a, Asset b) {
			return a?.GetHashCode() == b?.GetHashCode();
		}

		public static bool operator !=(Asset a, Asset b) {
			return a?.GetHashCode() != b?.GetHashCode();
		}

	}

}
