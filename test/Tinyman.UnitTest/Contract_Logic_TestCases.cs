using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using Tinyman.V1;

namespace Tinyman.UnitTest {
	   	  
	[TestClass]
	public class Contract_Logic_TestCases {

		public const ulong AppIdV1_0 = TinymanV1Constant.TestnetValidatorAppIdV1_0;
		public const ulong AppIdV1_1 = TinymanV1Constant.TestnetValidatorAppIdV1_1;

		public const ulong AssetId1 = 0;
		public const ulong AssetId2 = 21582668;

		public const string PoolLogicAsBase64V1_0 = "BCAIAQAAAwTMpqUKBQYhBSQNRDEJMgMSRDEVMgMSRDEgMgMSRDIEIg1EMwEAMQASRDMBEC" +
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

		public const string PoolLogicAsBase64V1_1 = "BCAIAQAAzKalCgMEBQYlJA1EMQkyAxJEMRUyAxJEMSAyAxJEMgQiDUQzAQAxABJEMwEQIQ" +
			"cSRDMBGIGs194dEkQzARkiEjMBGyEEEhA3ARoAgAlib290c3RyYXASEEAAXDMBGSMSRDMBG4ECEjcBGgCABHN3YXASEEACOzMBGyISRDcB" +
			"GgCABG1pbnQSQAE7NwEaAIAEYnVybhJAAZg3ARoAgAZyZWRlZW0SQAJbNwEaAIAEZmVlcxJAAnkAIQYhBSQjEk0yBBJENwEaARclEjcBGg" +
			"IXJBIQRDMCADEAEkQzAhAhBBJEMwIhIxJEMwIiIxwSRDMCIyEHEkQzAiQjEkQzAiWACFRNUE9PTDExEkQzAiZRAA+AD1RpbnltYW5Qb29s" +
			"MS4xIBJEMwIngBNodHRwczovL3RpbnltYW4ub3JnEkQzAikyAxJEMwIqMgMSRDMCKzIDEkQzAiwyAxJEMwMAMQASRDMDECEFEkQzAxElEk" +
			"QzAxQxABJEMwMSIxJEJCMTQAAQMwEBMwIBCDMDAQg1AUIBsTMEADEAEkQzBBAhBRJEMwQRJBJEMwQUMQASRDMEEiMSRDMBATMCAQgzAwEI" +
			"MwQBCDUBQgF8MgQhBhJENwEcATEAE0Q3ARwBMwQUEkQzAgAxABNEMwIUMQASRDMDADMCABJEMwIRJRJEMwMUMwMHMwMQIhJNMQASRDMDES" +
			"MzAxAiEk0kEkQzBAAxABJEMwQUMwIAEkQzAQEzBAEINQFCAREyBCEGEkQ3ARwBMQATRDcBHAEzAhQSRDMDFDMDBzMDECISTTcBHAESRDMC" +
			"ADEAEkQzAhQzBAASRDMCESUSRDMDADEAEkQzAxQzAwczAxAiEk0zBAASRDMDESMzAxAiEk0kEkQzBAAxABNEMwQUMQASRDMBATMCAQgzAw" +
			"EINQFCAJAyBCEFEkQ3ARwBMQATRDMCADcBHAESRDMCADEAE0QzAwAxABJEMwIUMwIHMwIQIhJNMQASRDMDFDMDBzMDECISTTMCABJEMwEB" +
			"MwMBCDUBQgA+MgQhBBJENwEcATEAE0QzAhQzAgczAhAiEk03ARwBEkQzAQEzAgEINQFCABIyBCEEEkQzAQEzAgEINQFCAAAzAAAxABNEMw" +
			"AHMQASRDMACDQBD0M=";

		[TestMethod]
		public void Get_Pool_Logic_V1_0() {

			var logicSig = TinymanV1Contract
				.GetPoolLogicsigSignature(AppIdV1_0, AssetId1, AssetId2);

			var poolLogic = Base64
				.ToBase64String(logicSig.Logic);

			Assert.AreEqual(poolLogic, PoolLogicAsBase64V1_0);
		}

		[TestMethod]
		public void Get_Pool_Logic_Assets_Reversed_V1_0() {

			var logicSig = TinymanV1Contract
				.GetPoolLogicsigSignature(AppIdV1_0, AssetId2, AssetId1);

			var poolLogic = Base64
				.ToBase64String(logicSig.Logic);

			Assert.AreEqual(poolLogic, PoolLogicAsBase64V1_0);
		}

		[TestMethod]
		public void Get_Pool_Logic_V1_1() {

			var logicSig = TinymanV1Contract
				.GetPoolLogicsigSignature(AppIdV1_1, AssetId1, AssetId2);

			var poolLogic = Base64
				.ToBase64String(logicSig.Logic);

			Assert.AreEqual(poolLogic, PoolLogicAsBase64V1_1);
		}

		[TestMethod]
		public void Get_Pool_Logic_Assets_Reversed_V1_1() {

			var logicSig = TinymanV1Contract
				.GetPoolLogicsigSignature(AppIdV1_1, AssetId2, AssetId1);

			var poolLogic = Base64
				.ToBase64String(logicSig.Logic);

			Assert.AreEqual(poolLogic, PoolLogicAsBase64V1_1);
		}

	}

}
