using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.ViewModels;

namespace BlockchainApp.Source.Common.Converters
{
    public static class BlockConverter
    {
        // List<Block> --> List<BlockGvVM>
        public static List<BlockGvVM> ToBlocksGvVM(this List<Block> blocks)
        {
            return blocks.Select(b => b.ToBlockGvVM()).OrderByDescending(b => b.TimeStamp).ToList();
        }

        // Block --> BlockGvVM
        public static BlockGvVM ToBlockGvVM(this Block block)
        {
            return new BlockGvVM
            {
                TimeStamp = new UnixTimestamp(block.TimeStamp),
                Hash = block.Hash,
                Difficulty = block.Difficulty,
                TransactionsCount = block.Transactions.Count
            };
        }
    }
}
