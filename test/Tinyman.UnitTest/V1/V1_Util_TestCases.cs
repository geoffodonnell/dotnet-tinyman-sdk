using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tinyman.V1;

namespace Tinyman.UnitTest.V1
{

    [TestClass]
    public class V1_Util_TestCases
    {

        [TestMethod]
        public void Encode_Int_0()
        {

            var value = Util.EncodeInt(0ul);

            Assert.AreEqual(1, value.Length);
            Assert.AreEqual(0x00, value[0]);
        }

        [TestMethod]
        public void Encode_Int_127()
        {

            var value = Util.EncodeInt(127ul);

            Assert.AreEqual(1, value.Length);
            Assert.AreEqual(0x7F, value[0]);
        }

        [TestMethod]
        public void Encode_Int_128()
        {

            var value = Util.EncodeInt(128ul);

            Assert.AreEqual(2, value.Length);
            Assert.AreEqual(0x80, value[0]);
            Assert.AreEqual(0x01, value[1]);
        }

        [TestMethod]
        public void Encode_Int_255()
        {

            var value = Util.EncodeInt(255ul);

            Assert.AreEqual(2, value.Length);
            Assert.AreEqual(0xFF, value[0]);
            Assert.AreEqual(0x01, value[1]);
        }

        [TestMethod]
        public void Encode_Int_256()
        {

            var value = Util.EncodeInt(256ul);

            Assert.AreEqual(2, value.Length);
            Assert.AreEqual(0x80, value[0]);
            Assert.AreEqual(0x02, value[1]);
        }

    }

}
