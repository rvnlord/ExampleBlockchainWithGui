using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using BlockchainApp.Source.Servers;
using Newtonsoft.Json.Linq;
using static BlockchainApp.Source.Common.Utils.LockUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Constrollers
{
    public class BlockchainApiController : ApiController
    {
        public static Blockchain Bc { get; set; } 
        public static P2PServer P2PServer_TS { get; set; }
        public static TransactionPool Tp { get; set; }
        public static CustomWallet Wallet { get; set; }
        public static Miner Miner_TS { get; set; }

        [HttpGet]
        public JToken Block(int id)
        {
            return Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(Block), () =>
            {
                if (id >= Bc.Chain.Count)
                    id = Bc.Chain.Count - 1;
                else if (id < 0)
                    id = 0;

                return new JObject
                {
                    ["BlockNo"] = id,
                    ["Block"] = Bc.Chain[id].ToJToken()
                };
            });
        }

        [HttpGet]
        public JToken BlockchainLength()
        {
            return Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(BlockchainLength), () => 
                new JObject { ["BlockchainLength"] = Bc.Chain.Count });
        }

        [HttpGet]
        public JToken Blocks()
        {
            return Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(Blocks), () => 
                Bc.Chain.ToJToken());
        }

        [HttpGet]
        public JToken KnownAddresses()
        {
            var addresses = Lock(
                new[] { _syncBlockchain, _syncTransactionPool },
                new[] { nameof(_syncBlockchain), nameof(_syncTransactionPool) }, 
                nameof(KnownAddresses), () =>
                {
                    return Bc.Chain.SelectMany(b => b.Transactions.Values).SelectMany(t => t.Outputs.Values.Select(o => o.Address))
                        .Concat(Tp.Transactions.Values.SelectMany(t => t.Outputs.Values.Select(o => o.Address)))
                        .Distinct().OrderBy(a => a).ToList();
                });

            return new JObject { ["KnownAddresses"] = addresses.ToJToken() };
        }

        [HttpPost]
        public JToken MineArtificial([FromBody] JToken data)
        {
            try
            {
                var transactions = data["Transactions"].ToString().JsonDeserialize().To<Dictionary<string, Transaction>>().Values;
                foreach (var transaction in transactions)
                {
                    Lock(_syncTransactionPool, nameof(_syncTransactionPool), nameof(MineArtificial), () => 
                        Tp.SetTransaction(transaction));
                    P2PServer_TS.BroadcastTransaction(transaction);
                }

                Miner_TS.Mine();

                return Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(MineArtificial), () =>
                    new JObject
                    {
                        ["Type"] = "Success",
                        ["Message"] = Bc.Chain.ToJToken()
                    });
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    ["Type"] = "Error",
                    ["Message"] = ex.Message
                };
            }
        }

        [HttpGet]
        public JToken MineTransactions()
        {
            try
            {
                Miner_TS.Mine();

                return Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(MineArtificial), () =>
                    new JObject
                    {
                        ["Type"] = "Success",
                        ["Message"] = Bc.Chain.ToJToken()
                    });
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    ["Type"] = "Error",
                    ["Message"] = ex.Message
                };
            }
        }

        [HttpPost]
        public JToken Send([FromBody] JToken data)
        {
            try
            {
                var recipient = data["Recipient"].ToString();
                var amount = data["Amount"].ToDecimal();

                var transaction = Wallet.CreateOrUpdateAndBroadcastTransaction_TS(recipient, amount, Bc, Tp, P2PServer_TS);

                return new JObject
                {
                    ["Type"] = "Success",
                    ["Message"] = transaction.ToJToken()
                };
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    ["Type"] = "Error",
                    ["Message"] = ex.Message
                };
            }
        }

        [HttpGet]
        public JToken Transactions()
        {
            return Lock(_syncTransactionPool, nameof(_syncTransactionPool), nameof(Transactions), () => 
                Tp.Transactions.ToJToken());
        }

        [HttpGet]
        public JToken WalletInfo()
        {
            Wallet.UpdateBalances_TS(Bc, Tp);
            return new JObject
            {
                ["PublicKey"] = Wallet.PublicKey,
                ["Address"] = Wallet.Address,
                ["Balance"] = Wallet.Balance,
                ["SpendableBalance"] = Wallet.SpendableBalance,
            };
        }
    }
}
