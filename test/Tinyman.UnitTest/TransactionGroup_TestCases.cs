using Algorand.V2.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System.Linq;
using Tinyman.V1;
using Tinyman.V1.Model;
using Account = Algorand.Account;
using Asset = Tinyman.V1.Model.Asset;

namespace Tinyman.UnitTest {

	[TestClass]
	public class TransactionGroup_TestCases {

		public static TransactionParametersResponse TxParams = new TransactionParametersResponse(
				"https://github.com/algorandfoundation/specs/tree/abc54f79f9ad679d2d22f0fb9909fb005c16f8a1",
				0,
				Base64.Decode(Strings.ToUtf8ByteArray("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=")),
				"testnet-v1.0",
				10000,
				1000);

		private static Account Account = new Account(
			"autumn coach siege genius key " +
			"usual helmet wood stairs spatial " +
			"ridge holiday turn chief embody " +
			"exotic hotel arctic morning " +
			"boring beef such march absent update");

		[TestMethod]
		public void Sign_And_Pack_Transaction() {

			var txGroup = TinymanTransaction.PrepareAppOptinTransactions(
				Constant.TestnetValidatorAppId,
				Account.Address,
				TxParams);

			Assert.AreEqual(1, txGroup.Transactions.Length);
			Assert.AreEqual(1, txGroup.SignedTransactions.Length);
			
			Assert.AreEqual(false, txGroup.IsSigned);

			txGroup.Sign(Account);

			Assert.AreEqual(true, txGroup.IsSigned);

			var txAsBytes = txGroup
				.SignedTransactions
				.SelectMany(s => Algorand.Encoder.EncodeToMsgPack(s))
				.ToArray();

			var txAsBase64 = Base64.ToBase64String(txAsBytes);
			var expectedAsBase64 = "gqNzaWfEQEB9LAOg09BPGvo41vGnGn3luFIG8ZBCIZotbHc7HN" +
				"kknSjILLHb2OlCd9Km4dVYSGO4uvOrZGQyzvntfPYykgKjdHhuiqRhcGFuAaRhcGlkzgF" +
				"JTFmjZmVlzQPoomZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8Q" +
				"gcsPcfBZp6wg3sYvf3DlCToio2dycMQgWni1rAlHpXwvBF14lqR8go96g8LI8BOcHGWfe" +
				"t8rz6CibHbNKvijc25kxCCELeY2Hdvafv/lFdGlz6v54NXVKYJ6jGo/axa/JvWpQaR0eX" +
				"BlpGFwcGw=";

			Assert.AreEqual(expectedAsBase64, txAsBase64);
		}

	}

}
