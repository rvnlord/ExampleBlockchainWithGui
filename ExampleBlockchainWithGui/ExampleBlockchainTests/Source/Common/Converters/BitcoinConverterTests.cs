using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockchainTests.Source.Common.Converters
{
    [TestClass]
    public class BitcoinConverterTests
    {
        private BitcoinWallet _validBitcoibWallet;

        [TestInitialize]
        public void InitTestWallets()
        {
            _validBitcoibWallet = new BitcoinWallet().Create();
        }

        [TestMethod]
        [Description("converts bitcoin private key to valid EC private key")]
        public void CryptoUtilsTest_ConvertBitcoinPrivateKeyToECPrivateKey()
        {
            var ecPriv = _validBitcoibWallet.PrivateKey.ToBase58ByteArray().BitcoinCompressedPrivateKeyToECPrivateKey();
            Assert.AreEqual(ecPriv, _validBitcoibWallet.ECKeyPair.Private);
        }

        [TestMethod]
        [Description("converts bitcoin compressed public key to valid EC public key")]
        public void CryptoUtilsTest_ConvertBitcoinCompressedPublicKeyToECPublicKey()
        {
            var ecPub = _validBitcoibWallet.PublicKey.ToBase58ByteArray().BitcoinCompressedPublicKeyToECPublicKey();
            Assert.AreEqual(ecPub, _validBitcoibWallet.ECKeyPair.Public);
        }
    }
}
