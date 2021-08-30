using System;
using System.Collections.Generic;

namespace Tinyman.V1.Model {

	public class Asset {

		public virtual long Id { get; internal set; }

		public virtual string Name { get; internal set; }

		public virtual string UnitName { get; internal set; }

		public virtual long Decimals { get; internal set; }
			   		 
		internal Asset() {
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
