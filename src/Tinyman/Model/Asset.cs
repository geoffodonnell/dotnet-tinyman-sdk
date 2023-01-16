using System.Collections.Generic;

namespace Tinyman.Model {

	public class Asset {

		public virtual ulong Id { get; set; }

		public virtual string Name { get; set; }

		public virtual string UnitName { get; set; }

		public virtual int Decimals { get; set; }

		public Asset() {
		}

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
