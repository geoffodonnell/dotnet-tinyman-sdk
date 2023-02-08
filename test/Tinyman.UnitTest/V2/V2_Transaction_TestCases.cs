using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using Tinyman.Model;
using Tinyman.V2;
using Account = Algorand.Algod.Model.Account;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.UnitTest.V2 {

	[TestClass]
	public class V2_Transaction_TestCases {

		public const ulong AppId = TinymanV2Constant.TestnetValidatorAppIdV2_0;
		public const string Note = "tinyman/v2:j{{\"origin\":\"Tinyman.UnitTest\"}}";

		public static readonly Asset Asset1 = new Asset {
			Id = 21582981,
			Name = "Tiny Gold",
			UnitName = "TINYAU",
			Decimals = 5
		};

		public static readonly Asset Asset2 = new Asset {
			Id = 0,
			Name = "Algo",
			UnitName = "ALGO",
			Decimals = 6
		};

		public static readonly Asset AssetLiquidity = new Asset {
			Id = 152331250,
			Name = "TinymanPool2.0 TINYAU-ALGO",
			UnitName = "TMPOOL2",
			Decimals = 6
		};

		public static readonly StringComparison Cmp = StringComparison.InvariantCulture;

		public static readonly TransactionParametersResponse TxParams = new TransactionParametersResponse {
			ConsensusVersion = "https://github.com/algorandfoundation/specs/tree/abc54f79f9ad679d2d22f0fb9909fb005c16f8a1",
			Fee = 0,
			GenesisHash = Base64.Decode(Strings.ToUtf8ByteArray("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=")),
			GenesisId = "testnet-v1.0",
			LastRound = 10000,
			MinFee = 1000
		};

		public static readonly Account Account = new Account(
			"autumn coach siege genius key " +
			"usual helmet wood stairs spatial " +
			"ridge holiday turn chief embody " +
			"exotic hotel arctic morning " +
			"boring beef such march absent update");

		public static readonly string BurnAsBase64 = "gqNzaWfEQINJtuo0OAjc2eFthRO1Z8dyZmBnTimlX4ZoCNhBF5jE0FGkv+hzaboNZM+0kZ49FMrcDq71/8xQi/XyYHmTjw2jdHhuiqRhYW10zScQpGFyY3bEIBtfU7lywIIweoAO2RJlvrcZnNeOXrXw9AEAYTOJJWLHo2ZlZc0D6KJmds0nEKJnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKjZ3JwxCCFNhFIaOMqp0Ww7Ordi8griQsbZQhOkrcJC/S73pKSe6Jsds0q+KNzbmTEIIQt5jYd29p+/+UV0aXPq/ng1dUpgnqMaj9rFr8m9alBpHR5cGWlYXhmZXKkeGFpZM4JFGPygqNzaWfEQP0iLdv+cO68dpZDQ24VWEqJlsYDhdO76UzfYWJSAheyj0bAnu36DLDs86EVg2eBuQC/E319PnALyIkXz93o5wmjdHhujaRhcGFhk8QQcmVtb3ZlX2xpcXVpZGl0ecQIAAAAAAAB4kDECAAAAAAACfvxpGFwYXOSzgFJVIUApGFwYXSRxCAbX1O5csCCMHqADtkSZb63GZzXjl618PQBAGEziSVix6RhcGlkzgjbkBijZmVlzQu4omZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQghTYRSGjjKqdFsOzq3YvIK4kLG2UITpK3CQv0u96SknuibHbNKvikbm90ZcQrdGlueW1hbi92Mjpqe3sib3JpZ2luIjoiVGlueW1hbi5Vbml0VGVzdCJ9faNzbmTEIIQt5jYd29p+/+UV0aXPq/ng1dUpgnqMaj9rFr8m9alBpHR5cGWkYXBwbA==";
		public static readonly string MintAsBase64 = "gqNzaWfEQBgnmGtH7e1Ljgy2Ql4F7QYsXRItvehK65RgV/nETbABFfeo7ARA6QoZo7HNBRN2wgGpDlY/cgO2NXINKrEyTAGjdHhuiqRhYW10zgAB4kCkYXJjdsQgG19TuXLAgjB6gA7ZEmW+txmc145etfD0AQBhM4klYsejZmVlzQPoomZ2zScQomdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqNncnDEIPGuK5s2K/ciSDxyyt0xamz+BKraFvlwPHhAIuralsPNomx2zSr4o3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaVheGZlcqR4YWlkzgFJVIWCo3NpZ8RAk3fuiQ9CgNHQelsE6sBZ7skOyAsazK3BjWgWVABnmBPrIuUodNHHEOjV9UwE+Z3v/4LEDA5aHMnRY44x3XMcC6N0eG6Ko2FtdM4ACfvxo2ZlZc0D6KJmds0nEKNnZW6sdGVzdG5ldC12MS4womdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqNncnDEIPGuK5s2K/ciSDxyyt0xamz+BKraFvlwPHhAIuralsPNomx2zSr4o3JjdsQgG19TuXLAgjB6gA7ZEmW+txmc145etfD0AQBhM4klYsejc25kxCCELeY2Hdvafv/lFdGlz6v54NXVKYJ6jGo/axa/JvWpQaR0eXBlo3BheYKjc2lnxEAe6nSCAFweorvmA6JUkb2LzRXWhk28It2VCmYQv6Vb3Cfuz1QvYe/R63NYr9L3ZT+Fan7wSFi8fBUkR566AjEIo3R4bo2kYXBhYZPEDWFkZF9saXF1aWRpdHnECGZsZXhpYmxlxAgAAAAAAAAnEKRhcGFzkc4JFGPypGFwYXSRxCAbX1O5csCCMHqADtkSZb63GZzXjl618PQBAGEziSVix6RhcGlkzgjbkBijZmVlzQu4omZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQg8a4rmzYr9yJIPHLK3TFqbP4EqtoW+XA8eEAi6tqWw82ibHbNKvikbm90ZcQrdGlueW1hbi92Mjpqe3sib3JpZ2luIjoiVGlueW1hbi5Vbml0VGVzdCJ9faNzbmTEIIQt5jYd29p+/+UV0aXPq/ng1dUpgnqMaj9rFr8m9alBpHR5cGWkYXBwbA==";
		public static readonly string SwapFixedInput01AsBase64 = "gqNzaWfEQLVfexR+KvY0ZbVJlLRrhIs0PqA+/7muyDhNepSTHg++gwBQrzcA/E64vlS8EIXtrr4rl+hXD8yATCPx+AuvlAWjdHhuiqRhYW10zgAB4kCkYXJjdsQgG19TuXLAgjB6gA7ZEmW+txmc145etfD0AQBhM4klYsejZmVlzQPoomZ2zScQomdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqNncnDEIAPx58BbHqNaH+YeqTKtFel21l2EMuS2bVq3dpQXhrW7omx2zSr4o3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaVheGZlcqR4YWlkzgFJVIWCo3NpZ8RAY0LyrRMXsSMagjKArM0XsAEH8VekoGZQ1XT/MMMhU7U83nEXvC6gZzIYINMrZqZRf7q0exj+WB5TO7EHzd+nCqN0eG6NpGFwYWGTxARzd2FwxAtmaXhlZC1pbnB1dMQIAAAAAAAJ+/GkYXBhc5LOAUlUhQCkYXBhdJHEIBtfU7lywIIweoAO2RJlvrcZnNeOXrXw9AEAYTOJJWLHpGFwaWTOCNuQGKNmZWXNB9CiZnbNJxCjZ2VurHRlc3RuZXQtdjEuMKJnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKjZ3JwxCAD8efAWx6jWh/mHqkyrRXpdtZdhDLktm1at3aUF4a1u6Jsds0q+KRub3RlxCt0aW55bWFuL3YyOmp7eyJvcmlnaW4iOiJUaW55bWFuLlVuaXRUZXN0In19o3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaRhcHBs";
		public static readonly string SwapFixedInput02AsBase64 = "gqNzaWfEQG5CLXu7e5RyXbQLGXGf7LRkRILtKm1biDkvsO8MIq224SHUzi7o1vAtUwJ5OcvKIthR2236/0kG9Yn/JsgTxwmjdHhuiqNhbXTOAAHiQKNmZWXNA+iiZnbNJxCjZ2VurHRlc3RuZXQtdjEuMKJnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKjZ3JwxCDLI/6M2/0vsUootZLwX0Z+ZSaibkCHSVPhMWg0wLGzuaJsds0q+KNyY3bEIBtfU7lywIIweoAO2RJlvrcZnNeOXrXw9AEAYTOJJWLHo3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaNwYXmCo3NpZ8RAPXLvYXUGoq2Zh+v2KQMj9p89KIYANWKJmHY5b59aSj3L3cnfr51/eXHYW0Rm6aT+7FMRx150PWI0yqL4DowSCaN0eG6NpGFwYWGTxARzd2FwxAtmaXhlZC1pbnB1dMQIAAAAAAAJ+/GkYXBhc5IAzgFJVIWkYXBhdJHEIBtfU7lywIIweoAO2RJlvrcZnNeOXrXw9AEAYTOJJWLHpGFwaWTOCNuQGKNmZWXNB9CiZnbNJxCjZ2VurHRlc3RuZXQtdjEuMKJnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKjZ3JwxCDLI/6M2/0vsUootZLwX0Z+ZSaibkCHSVPhMWg0wLGzuaJsds0q+KRub3RlxCt0aW55bWFuL3YyOmp7eyJvcmlnaW4iOiJUaW55bWFuLlVuaXRUZXN0In19o3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaRhcHBs";
		public static readonly string SwapFixedOutput01AsBase64 = "gqNzaWfEQPwdkQc8ID6DPy2xlMZyD6RD2jmvC+rUXE3cqc0tjc8HgmJ+JYghCd0/4m3l6ab6PQWjcChnF+JUng8DxnqbWgyjdHhuiqRhYW10zgAB4kCkYXJjdsQgG19TuXLAgjB6gA7ZEmW+txmc145etfD0AQBhM4klYsejZmVlzQPoomZ2zScQomdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqNncnDEIBSO3Trx4R8At7UvkUj4mQkQmIoxrB/3hSC7UJZl2DDkomx2zSr4o3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaVheGZlcqR4YWlkzgFJVIWCo3NpZ8RANp3MDoplWwzmAFH2elhbTUI7jDo74EgGvTLtrte/nCGr3pf32F4nSYb8MNkowJzpph4+P0VhQwpeDTZGXTCKBaN0eG6NpGFwYWGTxARzd2FwxAxmaXhlZC1vdXRwdXTECAAAAAAACfvxpGFwYXOSzgFJVIUApGFwYXSRxCAbX1O5csCCMHqADtkSZb63GZzXjl618PQBAGEziSVix6RhcGlkzgjbkBijZmVlzQu4omZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQgFI7dOvHhHwC3tS+RSPiZCRCYijGsH/eFILtQlmXYMOSibHbNKvikbm90ZcQrdGlueW1hbi92Mjpqe3sib3JpZ2luIjoiVGlueW1hbi5Vbml0VGVzdCJ9faNzbmTEIIQt5jYd29p+/+UV0aXPq/ng1dUpgnqMaj9rFr8m9alBpHR5cGWkYXBwbA==";
		public static readonly string SwapFixedOutput02AsBase64 = "gqNzaWfEQKju7ueCfV55v/5G/fnRMq97HnlyoG1e9XaET0Lsm9eD7zR/P/+VmJ9AaHmWmLC8h084AxpxX3aYoUYRt2rO7A6jdHhuiqNhbXTOAAHiQKNmZWXNA+iiZnbNJxCjZ2VurHRlc3RuZXQtdjEuMKJnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKjZ3JwxCB9aCxb1rWHVr+DEZDmlv68Aa1+vhDw63Sc+FtdI7WEz6Jsds0q+KNyY3bEIBtfU7lywIIweoAO2RJlvrcZnNeOXrXw9AEAYTOJJWLHo3NuZMQghC3mNh3b2n7/5RXRpc+r+eDV1SmCeoxqP2sWvyb1qUGkdHlwZaNwYXmCo3NpZ8RA8z5wQC+KCk2Rhk91J3WpVN8vgtk4Kkq0Hux6+NpHAIr/RLjm2ph1dYStSJVWHwrh7n1xr3UaIfjr9htX3RtbDaN0eG6NpGFwYWGTxARzd2FwxAxmaXhlZC1vdXRwdXTECAAAAAAACfvxpGFwYXOSAM4BSVSFpGFwYXSRxCAbX1O5csCCMHqADtkSZb63GZzXjl618PQBAGEziSVix6RhcGlkzgjbkBijZmVlzQu4omZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQgfWgsW9a1h1a/gxGQ5pb+vAGtfr4Q8Ot0nPhbXSO1hM+ibHbNKvikbm90ZcQrdGlueW1hbi92Mjpqe3sib3JpZ2luIjoiVGlueW1hbi5Vbml0VGVzdCJ9faNzbmTEIIQt5jYd29p+/+UV0aXPq/ng1dUpgnqMaj9rFr8m9alBpHR5cGWkYXBwbA==";

		[TestMethod]
		public void Verify_PrepareBootstrapTransactions() {

			var group = TinymanV2Transaction.PrepareBootstrapTransactions(AppId, Asset1, Asset2, Account.Address, 3000, TxParams, 40000, Note);

			Assert.IsTrue(group.Transactions.Length == 2);
			Assert.IsTrue(group.SignedTransactions.Length == 2);
			Assert.IsTrue(group.IsSigned == false);

			Assert.IsNotNull(group.Transactions[0]);
			Assert.IsNotNull(group.Transactions[1]);

			Assert.IsTrue(group.Transactions[0] is PaymentTransaction);
			Assert.IsTrue(group.Transactions[1] is ApplicationCallTransaction);

			Assert.IsNull(group.SignedTransactions[0]);
			Assert.IsNotNull(group.SignedTransactions[1]);

			group.Sign(Account);

			Assert.IsTrue(group.IsSigned);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			// The transaction group bytes are non-deterministic; the asset create txn
			// contains a random 32 byte array.
		}

		[TestMethod]
		public void Verify_PrepareBurnTransactions() {

			var group = TinymanV2Transaction.PrepareBurnTransactions(
				AppId,
				new AssetAmount(Asset1, 123456),
				new AssetAmount(Asset2, 654321),
				new AssetAmount(AssetLiquidity, 10000),
				Account.Address,
				TxParams,
				Note);

			group.Sign(Account);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			Assert.IsTrue(string.Equals(bytesAsBase64, BurnAsBase64, Cmp));
		}

		[TestMethod]
		public void Verify_PrepareMintTransactions() {

			var group = TinymanV2Transaction.PrepareMintTransactions(
				AppId,
				new AssetAmount(Asset1, 123456),
				new AssetAmount(Asset2, 654321),
				new AssetAmount(AssetLiquidity, 10000),
				Account.Address,
				TxParams,
				isInitialLiquidity: false,
				appCallNote: Note);

			group.Sign(Account);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			Assert.IsTrue(string.Equals(bytesAsBase64, MintAsBase64, Cmp));
		}

		[TestMethod]
		public void Verify_PrepareSwapTransactions_FixedInput_01() {

			var group = TinymanV2Transaction.PrepareSwapTransactions(
				AppId,
				new AssetAmount(Asset1, 123456),
				new AssetAmount(Asset2, 654321),
				SwapType.FixedInput,
				Account.Address,
				TxParams,
				Note);

			group.Sign(Account);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedInput01AsBase64, Cmp));
		}

		[TestMethod]
		public void Verify_PrepareSwapTransactions_FixedInput_02() {

			var group = TinymanV2Transaction.PrepareSwapTransactions(
				AppId,
				new AssetAmount(Asset2, 123456),
				new AssetAmount(Asset1, 654321),
				SwapType.FixedInput,
				Account.Address,
				TxParams,
				Note);

			group.Sign(Account);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedInput02AsBase64, Cmp));
		}

		[TestMethod]
		public void Verify_PrepareSwapTransactions_FixedOutput_01() {

			var group = TinymanV2Transaction.PrepareSwapTransactions(
				AppId,
				new AssetAmount(Asset1, 123456),
				new AssetAmount(Asset2, 654321),
				SwapType.FixedOutput,
				Account.Address,
				TxParams,
				Note);

			group.Sign(Account);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedOutput01AsBase64, Cmp));
		}

		[TestMethod]
		public void Verify_PrepareSwapTransactions_FixedOutput_02() {

			var group = TinymanV2Transaction.PrepareSwapTransactions(
				AppId,
				new AssetAmount(Asset2, 123456),
				new AssetAmount(Asset1, 654321),
				SwapType.FixedOutput,
				Account.Address,
				TxParams,
				Note);

			group.Sign(Account);

			var bytes = group.EncodeToMsgPack();
			var bytesAsBase64 = Base64.ToBase64String(bytes);

			Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedOutput02AsBase64, Cmp));
		}

	}

}
