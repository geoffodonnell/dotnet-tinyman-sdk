using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.V1;

namespace Tinyman.UnitTest {
	   	  
	[TestClass]
	public class Contract_Logic_TestCases {

		[TestMethod]
		public void Approval_Program_Is_Not_Null() {

			Assert.IsNotNull(Contract.ValidatorAppApprovalProgramBytes);
		}

		[TestMethod]
		public void Clear_Program_Is_Not_Null() {

			Assert.IsNotNull(Contract.ValidatorAppClearProgramBytes);
		}
	}
}
