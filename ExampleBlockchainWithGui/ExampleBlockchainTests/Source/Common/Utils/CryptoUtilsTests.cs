using BlockchainApp.Source.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static BlockchainApp.Source.Common.Utils.CryptoUtils;

namespace BlockchainTests.Source.Common.Utils
{
    [TestClass]
    public class CryptoUtilsTests
    {
        [TestMethod]
        [Description("Creates and Verifies ECDSA Signature")]
        public void CreateAndVerifyECDSASignature()
        {
            var keys = GenerateECKeyPair();
            var testData = "There are blue foxes!".ToUTF8ByteArray();
            Assert.IsTrue(VerifyECDSA(keys.Public, SignECDSA(keys.Private, testData), testData));
        }
    }
}
