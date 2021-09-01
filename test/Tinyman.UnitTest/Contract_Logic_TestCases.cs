using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using Tinyman.V1;

namespace Tinyman.UnitTest {
	   	  
	[TestClass]
	public class Contract_Logic_TestCases {

		public const ulong AppId = 21580889;
		public const long AssetId1 = 0;
		public const long AssetId2 = 21582668;
		public const string PoolLogicAsBase64 = "BCAIAQAAAwTMpqUKBQYhBSQNRDEJMgMSRDEVMgMSRDEgMgMSRDIEIg1EMwEAMQASRDMBEC" +
			"EHEkQzARiB2ZilChJEMwEZIhIzARslEhA3ARoAgAlib290c3RyYXASEEAAXDMBGSMSRDMBG4ECEjcBGgCABHN3YXASEEACEzMBGyISRDcB" +
			"GgCABG1pbnQSQAE5NwEaAIAEYnVybhJAAYM3ARoAgAZyZWRlZW0SQAIzNwEaAIAEZmVlcxJAAlAAIQYhBCQjEk0yBBJENwEaARchBRI3AR" +
			"oCFyQSEEQzAgAxABJEMwIQJRJEMwIhIxJEMwIiIxwSRDMCIyEHEkQzAiQjEkQzAiWAB1RNMVBPT0wSRDMCJlEADYANVGlueW1hbiBQb29s" +
			"IBJEMwIngBNodHRwczovL3RpbnltYW4ub3JnEkQzAikyAxJEMwIqMgMSRDMCKzIDEkQzAiwyAxJEMwMAMQASRDMDECEEEkQzAxEhBRJEMw" +
			"MUMQASRDMDEiMSRCQjE0AAEDMBATMCAQgzAwEINQFCAYkzBAAxABJEMwQQIQQSRDMEESQSRDMEFDEAEkQzBBIjEkQzAQEzAgEIMwMBCDME" +
			"AQg1AUIBVDIEIQYSRDcBHAExABNENwEcATMEFBJEMwIAMQATRDMCFDEAEkQzAwAzAgASRDMDFDMDBzMDECISTTEAEkQzBAAxABJEMwQUMw" +
			"IAEkQzAQEzBAEINQFCAPwyBCEGEkQ3ARwBMQATRDcBHAEzAhQSRDMDFDMDBzMDECISTTcBHAESRDMCADEAEkQzAhQzBAASRDMDADEAEkQz" +
			"AxQzAwczAxAiEk0zBAASRDMEADEAE0QzBBQxABJEMwEBMwIBCDMDAQg1AUIAjjIEIQQSRDcBHAExABNEMwIANwEcARJEMwIAMQATRDMDAD" +
			"EAEkQzAhQzAgczAhAiEk0xABJEMwMUMwMHMwMQIhJNMwIAEkQzAQEzAwEINQFCADwyBCUSRDcBHAExABNEMwIUMwIHMwIQIhJNNwEcARJE" +
			"MwEBMwIBCDUBQgARMgQlEkQzAQEzAgEINQFCAAAzAAAxABNEMwAHMQASRDMACDQBD0M=";

		[TestMethod]
		public void Approval_Program_Is_Not_Null() {

			Assert.IsNotNull(Contract.ValidatorAppApprovalProgramBytes);
		}

		[TestMethod]
		public void Clear_Program_Is_Not_Null() {

			Assert.IsNotNull(Contract.ValidatorAppClearProgramBytes);
		}

		[TestMethod]
		public void Get_Pool_Logic_With_Patch() {

			var logicSig = Contract
				.GetPoolLogicSig(AppId, AssetId1, AssetId2, true);

			var poolLogic = Base64
				.ToBase64String(logicSig.logic);

			Assert.AreEqual(poolLogic, PoolLogicAsBase64);
		}

		[TestMethod]
		public void Get_Pool_Logic_Assets_Reversed_With_Patch() {

			var logicSig = Contract
				.GetPoolLogicSig(AppId, AssetId2, AssetId1, true);

			var poolLogic = Base64
				.ToBase64String(logicSig.logic);

			Assert.AreEqual(poolLogic, PoolLogicAsBase64);
		}
		
		[TestMethod]
		public void Get_Pool_Logic_Without_Patch() {

			Assert.ThrowsException<ArgumentException>(() => {
				var logicSig = Contract
					.GetPoolLogicSig(AppId, AssetId1, AssetId2, false);
			});
		}

		[TestMethod]
		public void Get_Pool_Logic_Assets_Reversed_Without_Patch() {

			Assert.ThrowsException<ArgumentException>(() => {
				var logicSig = Contract
					.GetPoolLogicSig(AppId, AssetId2, AssetId1, false);
			});
		}

	}

}
