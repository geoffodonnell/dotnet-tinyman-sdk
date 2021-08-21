//using Org.BouncyCastle.Crypto.Digests;

//namespace Tinyman.V1.Teal4 {

//    /// <summary>
//    /// Copied from: https://github.com/RileyGe/dotnet-algorand-sdk/blob/master/dotnet-algorand-sdk/Util/Digester.cs
//    /// to support TEAL4 contracts in the Tinyman SDK until the Algorand SDK is updated, at which time this class
//    /// will be removed.
//    /// </summary>
//    internal class Digester {

//        internal static byte[] Digest(byte[] data) {

//            Sha512tDigest digest = new Sha512tDigest(256);
//            digest.BlockUpdate(data, 0, data.Length);
//            byte[] output = new byte[32];
//            digest.DoFinal(output, 0);

//            return output;
//        }

//    }

//}
