using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;
using Algorand;

namespace Tinyman.V1.Teal4 {

    /// <summary>
    /// Copied from: https://github.com/RileyGe/dotnet-algorand-sdk/blob/master/dotnet-algorand-sdk/Signature.cs
    /// to support TEAL4 contracts in the Tinyman SDK until the Algorand SDK is updated, at which time this class
    /// will be removed.
    /// 
    /// Serializable logicsig class. 
    /// LogicsigSignature is constructed from a program and optional arguments. 
    /// Signature sig and MultisigSignature msig property are available for modification by it's clients.
    /// </summary>
    [JsonObject]
    public class LogicsigSignature4 {
        [JsonIgnore]
        private static byte[] LOGIC_PREFIX = Encoding.UTF8.GetBytes("Program");//.getBytes(StandardCharsets.UTF_8);

        [JsonProperty(PropertyName = "l")]
        public byte[] logic;

        [JsonProperty(PropertyName = "arg")]
        public List<byte[]> args;

        [JsonProperty(PropertyName = "sig")]
        public Signature sig;

        [JsonProperty(PropertyName = "msig")]
        public MultisigSignature msig;

        /// <summary>
        /// LogicsigSignature
        /// </summary>
        /// <param name="logic">Unsigned logicsig object</param>
        /// <param name="args">Unsigned logicsig object's arguments</param>
        /// <param name="sig"></param>
        /// <param name="msig"></param>
        [JsonConstructor]
        public LogicsigSignature4(
            [JsonProperty("l")] byte[] logic,
            [JsonProperty("arg")] List<byte[]> args = null,
            [JsonProperty("sig")] byte[] sig = null,
            [JsonProperty("msig")] MultisigSignature msig = null) {
            this.logic = JavaHelper<byte[]>.RequireNotNull(logic, "program must not be null");
            this.args = args;

            if (!Logic.CheckProgram(this.logic, this.args))
                throw new Exception("program verified failed!");

            if (sig != null) {
                this.sig = new Signature(sig);
            }
            this.msig = msig;
        }
        /// <summary>
        /// Uninitialized object used for serializer to ignore default values.
        /// </summary>
        public LogicsigSignature4() {
            this.logic = null;
            this.args = null;
        }
        /// <summary>
        /// alculate escrow address from logic sig program
        /// DEPRECATED
        /// Please use Address property.
        /// The address of the LogicsigSignature
        /// </summary>
        /// <returns>The address of the LogicsigSignature</returns>
        public Address ToAddress() {
            return Address;
        }
        /// <summary>
        /// The address of the LogicsigSignature
        /// </summary>
        [JsonIgnore]
        public Address Address {
            get {
                return new Address(Digester.Digest(BytesToSign()));
            }
        }
        /// <summary>
        /// Return prefixed program as byte array that is ready to sign
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] BytesToSign() {
            List<byte> prefixedEncoded = new List<byte>(LOGIC_PREFIX);
            prefixedEncoded.AddRange(this.logic);
            return prefixedEncoded.ToArray();
        }
        /// <summary>
        /// Perform signature verification against the sender address
        /// </summary>
        /// <param name="address">Address to verify</param>
        /// <returns>bool</returns>
        public bool Verify(Address address) {
            if (this.logic == null) {
                return false;
            } else if (this.sig != null && this.msig != null) {
                return false;
            } else {
                try {
                    Logic.CheckProgram(this.logic, this.args);
                } catch (Exception) {
                    return false;
                }

                if (this.sig == null && this.msig == null) {
                    try {
                        return address.Equals(this.ToAddress());
                    } catch (Exception) {
                        return false;
                    }
                }

                if (this.sig != null) {
                    try {
                        var pk = new Ed25519PublicKeyParameters(address.Bytes, 0);
                        var signer = new Ed25519Signer();
                        signer.Init(false, pk); //false代表用于VerifySignature
                        signer.BlockUpdate(this.BytesToSign(), 0, this.BytesToSign().Length);
                        return signer.VerifySignature(this.sig.Bytes);
                    } catch (Exception err) {
                        Console.WriteLine("Message = " + err.Message);
                        return false;
                    }
                } else {
                    return this.msig.Verify(this.BytesToSign());
                }

            }
        }

        private static bool NullCheck(object o1, object o2) {
            if (o1 == null && o2 == null) {
                return true;
            } else if (o1 == null && o2 != null) {
                return false;
            } else {
                return o1 == null || o2 != null;
            }
        }

        public override bool Equals(object obj) {
            if (obj is LogicsigSignature actual)
                if ((this.logic is null && actual.logic is null) || (!(this.logic is null || actual.logic is null) && Enumerable.SequenceEqual(this.logic, actual.logic)))
                    if ((this.sig is null && actual.sig is null) || this.sig.Equals(actual.sig))
                        if ((this.msig is null && actual.msig is null) || this.msig.Equals(actual.msig))
                            if ((this.args is null && actual.args is null) || ArgsEqual(this.args, actual.args))
                                return true;
            return false;
        }

        public override int GetHashCode() {
            return this.logic.GetHashCode() + this.args.GetHashCode() + this.sig.GetHashCode() + this.msig.GetHashCode();
        }

        private static bool ArgsEqual(List<byte[]> args1, List<byte[]> args2) {
            bool flag = true;
            if (args1.Count == args2.Count) {
                for (int i = 0; i < args1.Count; i++) {
                    if (!Enumerable.SequenceEqual(args1[i], args2[i])) {
                        flag = false;
                        break;
                    }
                }
            } else
                flag = false;
            return flag;
        }
    }

}
