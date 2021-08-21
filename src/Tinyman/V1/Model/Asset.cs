using System;

namespace Tinyman.V1.Model {

	public class Asset {

		public long Id { get; internal set; }

		public string Name { get; internal set; }

		public string UnitName { get; internal set; }

		public long Decimals { get; internal set; }
			   		 
		internal Asset() {
		}

	}

}
