using System;
using System.Collections.Generic;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using NLog;

namespace BlockchainApp.Source
{
    public static class Config
    {
        public static int MINE_RATE = 1000; // Milliseconds
        public static int INITIAL_DIFFICULTY = 3;
        public static Block GENESIS_DATA = new Block(UnixTimestamp.First(), "-----", "hash-one", new Dictionary<string, Transaction>(), 0, INITIAL_DIFFICULTY);
        public static int STARTING_BALANCE = 1000;
        public static string REWARD_INPUT_ADDRESS = "*authorized-reward*";
        public static int MINING_REWARD = 50;

        public static DdlItem ddlRequests_DUMMY_REQUEST = new DdlItem(-1, "(Select request)");
        public static DdlItem ddlPeers_DUMMY_PEER = new DdlItem(-1, "(Select peer)");

        public static readonly Logger _logger = LoggerUtils.Create();

        public static readonly object _syncPeers = new object();
        public static readonly object _syncBlockchain = new object();
        public static readonly object _syncTransactionPool = new object();

        public static readonly Random _r = new Random();
    }
}
