using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using MoreLinq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace BlockchainApp.Source.Common.Utils
{
    public static class CryptoUtils
    {
        public static string UniqueId()
        {
            return Guid.NewGuid().ToString();
        }

        public static AsymmetricCipherKeyPair GenerateECKeyPair()
        {
            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            var sr = new SecureRandom();
            var keyParams = new ECKeyGenerationParameters(domainParams, sr);
            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(keyParams);
            return generator.GenerateKeyPair();
        }

        public static AsymmetricCipherKeyPair CreateECKeyPair(byte[] privKey)
        {
            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
            var d = privKey.ToBigIntU();
            var ecPrivKey = new ECPrivateKeyParameters(d, domainParams);
            var q = domainParams.G.Multiply(d);
            var ecPubKey = new ECPublicKeyParameters(q, domainParams);
            return new AsymmetricCipherKeyPair(ecPubKey, ecPrivKey);
        }

        public static byte[] SignECDSA_BitcoinPrivateKey(byte[] bitcoinCompresseedPrivateKey, byte[] data)
        {
            return SignECDSA(bitcoinCompresseedPrivateKey.BitcoinCompressedPrivateKeyToECPrivateKey(), data);
        }

        public static byte[] SignECDSA(AsymmetricKeyParameter privKey, byte[] data)
        {
            var signer = SignerUtilities.GetSigner("SHA256withECDSA");
            signer.Init(true, privKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }

        public static bool VerifyECDSA_BitcoinCompressedPublicKey(byte[] pubKey, byte[] signature, byte[] dataHash)
        {
            return VerifyECDSA(pubKey.BitcoinCompressedPublicKeyToECPublicKey(), signature, dataHash);
        }

        public static bool VerifyECDSA(AsymmetricKeyParameter pubKey, byte[] signature, byte[] dataHash)
        {
            var ecdsaVerify = SignerUtilities.GetSigner("SHA256withECDSA");
            ecdsaVerify.Init(false, pubKey);
            ecdsaVerify.BlockUpdate(dataHash, 0, dataHash.Length);
            return ecdsaVerify.VerifySignature(signature);
        }

        public static byte[] Ripemd160(byte[] data)
        {
            var digest = new RipeMD160Digest();
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] SignHMACSha512(byte[] key, byte[] data)
        {
            return new HMACSHA512(key).ComputeHash(data);
        }

        public static byte[] SignHMACSha256(byte[] key, byte[] data)
        {
            return new HMACSHA256(key).ComputeHash(data);
        }

        public static byte[] Sha256(byte[] value)
        {
            return SHA256.Create().ComputeHash(value);
        }

        public static string Sha3(string value) => Sha3(value.ToUTF8ByteArray()).ToHexString();

        public static byte[] Sha3(byte[] value)
        {
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Sha3(List<byte> value)
        {
            return Sha3(value.ToArray());
        }
    }
}
