﻿using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using Tinyman.Model;
using Tinyman.V1;
using Account = Algorand.Algod.Model.Account;
using Asset = Tinyman.Model.Asset;

namespace Tinyman.UnitTest.V1
{

    [TestClass]
    public class V1_TinymanTransaction_TestCases
    {

        public static ulong AppId = TinymanV1Constant.MainnetValidatorAppIdV1_1;

        public static Asset Asset1 = new Asset
        {
            Id = 31566704,
            Name = "USDC",
            UnitName = "USDC",
            Decimals = 6
        };

        public static Asset Asset2 = new Asset
        {
            Id = 0,
            Name = "Algo",
            UnitName = "ALGO",
            Decimals = 6
        };

        public static Asset AssetLiquidity = new Asset
        {
            Id = 552647097,
            Name = "TinymanPool1.1 USDC-ALGO",
            UnitName = "TMPOOL11",
            Decimals = 6
        };

        public static StringComparison Cmp = StringComparison.InvariantCulture;

        public static TransactionParametersResponse TxParams = new TransactionParametersResponse
        {
            ConsensusVersion = "https://github.com/algorandfoundation/specs/tree/abc54f79f9ad679d2d22f0fb9909fb005c16f8a1",
            Fee = 0,
            GenesisHash = Base64.Decode(Strings.ToUtf8ByteArray("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=")),
            GenesisId = "testnet-v1.0",
            LastRound = 10000,
            MinFee = 1000
        };

        public static Account Account = new Account(
            "autumn coach siege genius key " +
            "usual helmet wood stairs spatial " +
            "ridge holiday turn chief embody " +
            "exotic hotel arctic morning " +
            "boring beef such march absent update");

        public static string AppOptInAsBase64 = "gqNzaWfEQJDUlzojK9BsYBfiVO5GWQ69Klq8iwn4805jSWv8mlq6SwnTmz/IAC/M8yrY2Ju88JZ5QYWpahBX399748a9BAajdHhuiqRhcGFuAaRhcGlkziDwjlijZmVlzQPoomZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQgUYDPrv0nYKmbMZ5FSMFEuJ4y+zl1NCREVUs2KyiMlg+ibHbNKvijc25kxCCELeY2Hdvafv/lFdGlz6v54NXVKYJ6jGo/axa/JvWpQaR0eXBlpGFwcGw=";
        public static string AppOptOutAsBase64 = "gqNzaWfEQO8TxcIkhvB/CEJzRL7M9kuiTpq7SXkaIwuBogbWWUBxr1vXlAIDE6DR4nEeMgQ1tRJzdRGJvuJ2308TonymCwSjdHhuiqRhcGFuA6RhcGlkziDwjlijZmVlzQPoomZ2zScQo2dlbqx0ZXN0bmV0LXYxLjCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQgphH66IgA7corc37Yvtktr+EjLicslr4IFppoqDsonUuibHbNKvijc25kxCCELeY2Hdvafv/lFdGlz6v54NXVKYJ6jGo/axa/JvWpQaR0eXBlpGFwcGw=";
        public static string AssetOptInAsBase64 = "gqNzaWfEQNQP0VBybEFKLRjiBv989TMJwkof2x2YFZIuBwb2ZYdoud7aHLUpRL4foF3vxXT0yvjMsxS/kilFmnQN/CKLIAWjdHhuiaRhcmN2xCCELeY2Hdvafv/lFdGlz6v54NXVKYJ6jGo/axa/JvWpQaNmZWXNA+iiZnbNJxCiZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToio2dycMQgAlEtOivhwPXIbDVANDERxb2507FZnYOwpdtn+EO+3jyibHbNKvijc25kxCCELeY2Hdvafv/lFdGlz6v54NXVKYJ6jGo/axa/JvWpQaR0eXBlpWF4ZmVypHhhaWTOAeGrcA==";
        public static string BurnAsBase64 = "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";
        public static string MintAsBase64 = "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";
        public static string RedeemAsBase64 = "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";
        public static string SwapFixedInput01AsBase64 = "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";
        public static string SwapFixedInput02AsBase64 = "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";
        public static string SwapFixedOutput01AsBase64 = "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";
        public static string SwapFixedOutput02AsBase64 = "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";

        [TestMethod]
        public void Verify_PrepareAppOptinTransactions()
        {

            var group = TinymanV1Transaction.PrepareAppOptinTransactions(AppId, Account.Address, TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, AppOptInAsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareAppOptoutTransactions()
        {

            var group = TinymanV1Transaction.PrepareAppOptoutTransactions(AppId, Account.Address, TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, AppOptOutAsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareAssetOptinTransactions()
        {

            var group = TinymanV1Transaction.PrepareAssetOptinTransactions(Asset1.Id, Account.Address, TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, AssetOptInAsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareBootstrapTransactions()
        {

            var group = TinymanV1Transaction.PrepareBootstrapTransactions(AppId, Asset1, Asset2, Account.Address, TxParams);

            Assert.IsTrue(group.Transactions.Length == 4);
            Assert.IsTrue(group.SignedTransactions.Length == 4);
            Assert.IsTrue(group.IsSigned == false);

            Assert.IsNotNull(group.Transactions[0]);
            Assert.IsNotNull(group.Transactions[1]);
            Assert.IsNotNull(group.Transactions[2]);
            Assert.IsNotNull(group.Transactions[3]);

            Assert.IsTrue(group.Transactions[0] is PaymentTransaction);
            Assert.IsTrue(group.Transactions[1] is ApplicationCallTransaction);
            Assert.IsTrue(group.Transactions[2] is AssetCreateTransaction);
            Assert.IsTrue(group.Transactions[3] is AssetAcceptTransaction);

            Assert.IsNull(group.SignedTransactions[0]);
            Assert.IsNotNull(group.SignedTransactions[1]);
            Assert.IsNotNull(group.SignedTransactions[2]);
            Assert.IsNotNull(group.SignedTransactions[3]);

            group.Sign(Account);

            Assert.IsTrue(group.IsSigned);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            // The transaction group bytes are non-deterministic; the asset create txn
            // contains a random 32 byte array.
        }

        [TestMethod]
        public void Verify_PrepareBurnTransactions()
        {

            var group = TinymanV1Transaction.PrepareBurnTransactions(
                AppId,
                new AssetAmount(Asset1, 1000000),
                new AssetAmount(Asset2, 1000000),
                new AssetAmount(AssetLiquidity, 10000),
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, BurnAsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareMintTransactions()
        {

            var group = TinymanV1Transaction.PrepareMintTransactions(
                AppId,
                new AssetAmount(Asset1, 1000000),
                new AssetAmount(Asset2, 1000000),
                new AssetAmount(AssetLiquidity, 10000),
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, MintAsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareRedeemTransactions()
        {

            var group = TinymanV1Transaction.PrepareRedeemTransactions(
                AppId,
                Asset1,
                Asset2,
                AssetLiquidity,
                new AssetAmount(Asset1, 1000000),
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, RedeemAsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareSwapTransactions_FixedInput_01()
        {

            var group = TinymanV1Transaction.PrepareSwapTransactions(
                AppId,
                new AssetAmount(Asset1, 1000000),
                new AssetAmount(Asset2, 1000000),
                AssetLiquidity,
                SwapType.FixedInput,
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedInput01AsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareSwapTransactions_FixedInput_02()
        {

            var group = TinymanV1Transaction.PrepareSwapTransactions(
                AppId,
                new AssetAmount(Asset2, 1000000),
                new AssetAmount(Asset1, 1000000),
                AssetLiquidity,
                SwapType.FixedInput,
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedInput02AsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareSwapTransactions_FixedOutput_01()
        {

            var group = TinymanV1Transaction.PrepareSwapTransactions(
                AppId,
                new AssetAmount(Asset1, 1000000),
                new AssetAmount(Asset2, 1000000),
                AssetLiquidity,
                SwapType.FixedOutput,
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedOutput01AsBase64, Cmp));
        }

        [TestMethod]
        public void Verify_PrepareSwapTransactions_FixedOutput_02()
        {

            var group = TinymanV1Transaction.PrepareSwapTransactions(
                AppId,
                new AssetAmount(Asset2, 1000000),
                new AssetAmount(Asset1, 1000000),
                AssetLiquidity,
                SwapType.FixedOutput,
                Account.Address,
                TxParams);

            group.Sign(Account);

            var bytes = group.EncodeToMsgPack();
            var bytesAsBase64 = Base64.ToBase64String(bytes);

            Assert.IsTrue(string.Equals(bytesAsBase64, SwapFixedOutput02AsBase64, Cmp));
        }

    }

}
