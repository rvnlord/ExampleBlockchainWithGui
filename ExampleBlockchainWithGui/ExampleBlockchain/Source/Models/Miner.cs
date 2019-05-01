using BlockchainApp.Source.Models.Wallets;
using BlockchainApp.Source.Servers;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Models
{
    public class Miner
    {
        public Blockchain Blockchain { get; }
        public TransactionPool TransactionPool { get; }
        public CustomWallet Wallet { get; }
        public P2PServer P2PServer_TS { get; }

        public Miner(Blockchain blockchain, TransactionPool transactionPool, CustomWallet wallet, P2PServer p2pServer)
        {
            Blockchain = blockchain;
            TransactionPool = transactionPool;
            Wallet = wallet;
            P2PServer_TS = p2pServer;
        }

        public Block Mine()
        {
            var validTransactions = TransactionPool.ValidTransactions_TS();
            var reward = Transaction.Reward(Wallet);
            validTransactions.Add(reward.Id, reward);
            var block = Blockchain.MineBlock_TS(validTransactions);
            P2PServer_TS.BroadcastChain();
            TransactionPool.Clear_TS(); // clear local transaction pool

            return block;
        }
    }
}
