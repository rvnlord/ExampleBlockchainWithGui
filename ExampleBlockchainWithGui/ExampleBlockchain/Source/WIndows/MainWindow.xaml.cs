using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BlockchainApp.Source.Clients.PeersClient;
using BlockchainApp.Source.Common.Converters;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Constrollers;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.ViewModels;
using BlockchainApp.Source.Models.Wallets;
using BlockchainApp.Source.Servers;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using MoreLinq;
using static BlockchainApp.Source.Common.Utils.AsyncUtils;
using static BlockchainApp.Source.Common.Utils.DeferredControlsUpdateUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.WIndows
{
    public partial class MainWindow : MetroWindow
    {
        private CustomWallet _wallet;
        private BitcoinWallet _btcWallet;
        private Blockchain _bc;
        private TransactionPool _tp;
        private P2PServer _p2pServer;
        private HttpServer _httpServer;
        private Miner _miner_TS;
        private string _walletBalanceStr => $"{_wallet?.Balance:0.########} EXC (Spendable: {_wallet?.SpendableBalance:0.########} EXC) ({_btcWallet?.Balance:0.########} BTC)";

        private List<object> _controlsToDisable => this.LogicalDescendants<Button, Tile>().Cast<object>().ToList();

        private async void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailure = BaseCompatibilityPreferences.HandleDispatcherRequestProcessingFailureOptions.Throw;

            SelectDefaultMainTab();

            SetupWindow();
            SetupTiles();
            SetupNotifyIcon();
            SetupTextBoxes();
            SetupDataGrids();
            SetupDdls();

            var transactionsVM = new List<TransactionGvVM>();

            if (await AsyncWithLoader(gridMain, _controlsToDisable, () =>
            {
                Host();
                _btcWallet.UpdateBalance();
                transactionsVM = CreateTransactionsVM();
                _wallet.UpdateBalance(_bc);
                _wallet.UpdateSpendableBalance(_tp);
            }) != null) return;

            lblWindowTitle.Content += $" - http://{_httpServer?.Address}/ | ws://{_p2pServer?.Address}/";
            EnqueueSetValue(txtAddress.ClearValue(true), _wallet.Address).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
            EnqueueSetValue(txtPublicKey.ClearValue(true), _wallet.PublicKey).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
            EnqueueSetValue(txtBalance.ClearValue(true), _walletBalanceStr).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
            EnqueueSetValue(_ocBlocks, _bc.Chain.ToBlocksGvVM()).OrSetDirectlyIfGridVisible(gridBlockchainExplorerMainTab);
            EnqueueSetValue(_ocTransactions, transactionsVM).OrSetDirectlyIfGridVisible(gridDashboardMainTab);

            const string cloud = "ioeirapsnrpe srkaieis eercbulla daaersuteols deinlpsstleaec xaiofflteubu e02vb0a0r. taosngurmd gomnaniotm ta";
            EnqueueSetValue(txtPuzzleCloud.ClearValue(true), cloud).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
        }

        private void TransactionPool_TransactionChanged(object sender, TransactionChangedEventArgs e)
        {
            var transactionsVM = CreateTransactionsVM(); // not async we don't want to block user interation and Async with loader would cancel existing loaders
            _wallet.UpdateBalance(_bc);
            _wallet.UpdateSpendableBalance(_tp);

            Dispatcher.Invoke(() =>
            {
                EnqueueSetValue(_ocTransactions, transactionsVM).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                EnqueueSetValue(txtBalance, _walletBalanceStr).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
            });
        }

        private void Blockchain_ChainChanged(object sender, ChainChangedEventArgs e)
        {
            var transactionsVM = CreateTransactionsVM();
            _wallet.UpdateBalance(_bc);
            _wallet.UpdateSpendableBalance(_tp);
            var blocksVM = _bc.Chain.ToBlocksGvVM();

            Dispatcher.Invoke(() =>
            {
                EnqueueSetValue(_ocTransactions, transactionsVM).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                EnqueueSetValue(txtBalance, _walletBalanceStr).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                EnqueueSetValue(_ocBlocks, blocksVM).OrSetDirectlyIfGridVisible(gridBlockchainExplorerMainTab);
            });
        }

        private void Blockchain_GeneratedBlockHash(object sender, GeneratedhashEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lblMineTransactionsStatus.Content = $"Generated Hash: {e.GeneratedHash}";
            });
        }

        private void P2PServer_PeerConnected(object sender, PeerConnectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                xrgPeers.Value = Math.Min(e.PeerCount, xrgPeers.Maximum);
                //lblPeersCount.Content = e.PeerCount;

                var peerAddresses = _p2pServer.ActivePeers.Select(p => p.Address.WsAddressToHttpApiAddress()).OrderBy(a => a).ToArray();
                var myAddress = _p2pServer.Address.WsAddressToHttpApiAddress();
                EnqueueSetValue(ddlPeers, peerAddresses.PrependEl(myAddress).ToArray()).OrSetDirectlyIfGridVisible(gridHttpApiTestMainTab);
            });
        }

        private void Log_MessageReceived(object sender, InformationSentEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var oldLogConteent = txtLog.NullifyIfTag().Text;
                txtLog.ClearValue(true).Text = $"{ExtendedTime.LocalNow.ToDateTimeString()}: {e.Information}\n{oldLogConteent}";
            });
        }

        private void btnCopyAddress_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtAddress.Text);
        private void btnCopyPublicKey_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtPublicKey.Text);
        private void btnCopyPrivateKey_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtPrivateKey.IsNullWhiteSpaceOrTag() ? _wallet.PrivateKey : txtPrivateKey.Text);
        private void btnCopyBalance_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtBalance.Text.BeforeFirst(" EXC"));

        private void btnDonateBTC_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtDonateBTC.Text);
        private void btnDonateETH_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtDonateETH.Text);
        private void btnDonateXRP_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtDonateXRP.Text);
        private void btnDonateNEO_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtDonateNEO.Text);
        private void btnDonateXMR_Click(object sender, RoutedEventArgs e) => ClipboardUtils.TrySetText(txtDonateXMR.Text);

        private void btnSHA256_Click(object sender, RoutedEventArgs e)
        {
            txtSHA256.ClearValue().Text = CryptoUtils.Sha256(txtSHA256.NullifyIfTag().Text.ToUTF8ByteArray()).ToBase58String();
        }

        private void btnSHA256_3_Click(object sender, RoutedEventArgs e)
        {
            txtSHA3_256.ClearValue().Text = CryptoUtils.Sha3(txtSHA3_256.NullifyIfTag().Text.ToUTF8ByteArray()).ToBase58String();
        }

        private void btnRevealPrivateKey_Click(object sender, RoutedEventArgs e)
        {
            var iconRevealKey = (PackIconModern) btnRevealPrivateKey.Content;
            var iconChangekey = (PackIconModern)btnChangePrivateKey.Content;
            var revealKey = iconRevealKey.Kind == PackIconModernKind.EyeHide;
            var cancelEditing = iconRevealKey.Kind == PackIconModernKind.Close;

            if (revealKey)
            {
                txtPrivateKey.ClearValue(true).Text = _wallet.PrivateKey;
                iconRevealKey.Kind = PackIconModernKind.Eye;
            }
            else if (cancelEditing)
            {
                iconRevealKey.Kind = PackIconModernKind.Eye;
                iconChangekey.Kind = PackIconModernKind.DrawMarkerReflection;
                txtPrivateKey.Text = _wallet.PrivateKey;
                txtPrivateKey.IsReadOnly = true;
            }
            else
            {
                txtPrivateKey.ResetValue(true);
                iconRevealKey.Kind = PackIconModernKind.EyeHide;
            }
        }

        private async void btnChangePrivateKey_Click(object sender, RoutedEventArgs e)
        {
            var currentPrivateKey = _wallet.PrivateKey;
            var iconChangeKey = (PackIconModern) btnChangePrivateKey.Content;
            var iconRevealKey = (PackIconModern) btnRevealPrivateKey.Content;
            var startEditing = iconChangeKey.Kind == PackIconModernKind.DrawMarkerReflection;
            var save = iconChangeKey.Kind == PackIconModernKind.Save;

            if (startEditing)
            {
                txtPrivateKey.ClearValue(true).Text = currentPrivateKey;
                iconRevealKey.Kind = PackIconModernKind.Close;
                iconChangeKey.Kind = PackIconModernKind.Save;
                txtPrivateKey.IsReadOnly = false;
            }
            else if (save)
            {
                var newPrivateKey = txtPrivateKey.Text;
                if (currentPrivateKey == newPrivateKey)
                {
                    iconRevealKey.Kind = PackIconModernKind.Eye;
                    iconChangeKey.Kind = PackIconModernKind.DrawMarkerReflection;
                    txtPrivateKey.IsReadOnly = true;
                }
                else if (CustomWallet.IsPrivateKeyCorrect(newPrivateKey))
                {
                    iconRevealKey.Kind = PackIconModernKind.Eye;
                    iconChangeKey.Kind = PackIconModernKind.DrawMarkerReflection;
                    txtPrivateKey.IsReadOnly = true;

                    var transactionsVM = new List<TransactionGvVM>();
                    await AsyncWithLoader(gridMain, _controlsToDisable, () =>
                    {
                        _btcWallet = new BitcoinWallet().CreateWithECPrivateKeyByteArray(newPrivateKey.ToBase58ByteArray().BitcoinCompressedPrivateKeyToECPrivateKeyByteArray());                       
                        _wallet = new CustomWallet().CreateWithPrivateKeyCompressed(newPrivateKey.ToBase58ByteArray());

                        transactionsVM = CreateTransactionsVM();
                        _wallet.UpdateBalance(_bc);
                        _wallet.UpdateSpendableBalance(_tp);
                        _btcWallet.UpdateBalance();
                    });
                    txtPrivateKey.ClearValue(true);
                    EnqueueSetValue(_ocTransactions, transactionsVM).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                    EnqueueSetValue(txtAddress.ClearValue(true), _wallet.Address).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                    EnqueueSetValue(txtPublicKey.ClearValue(true), _wallet.PublicKey).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                    EnqueueSetValue(txtBalance.ClearValue(true), _walletBalanceStr).OrSetDirectlyIfGridVisible(gridDashboardMainTab);
                }
                else
                    await this.ShowMessageAsync("Error", "Incorrect Private Key");
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();

            var recipient = txtRecipientAddress.NullifyIfTag().Text;
            var amountToSend = xceAmountToSend.Text.BeforeFirst(" EXC").ToDecimalN() ?? 0;
            Transaction transaction = null;

            await AsyncWithLoader(gridMain, _controlsToDisable, () =>
            {
                this.SetLoaderStatus("Sending transactions...");

                transaction = _wallet.CreateOrUpdateAndBroadcastTransaction_TS(recipient, amountToSend, _bc, _tp, _p2pServer);
            });

            lblTransactionStatus.Visibility = Visibility.Visible;

            if (transaction != null)
            {
                lblTransactionStatus.Background = Brushes.Green;
                lblTransactionStatus.Content =
                    $"{amountToSend} EXC has been sent to {recipient},\n" +
                    $"(Id: {transaction.Id})";              
            }
            else
            {
                lblTransactionStatus.Background = Brushes.Red;
                lblTransactionStatus.Content = $"No transaction has been sent";
            }
        }

        private async void btnMineTransactions_Click(object sender, RoutedEventArgs e)
        {
            lblMineTransactionsStatus.Visibility = Visibility.Visible;
            lblMineTransactionsStatus.Background = Brushes.Red;

            Block minedBlock = null;

            await AsyncWithLoader(gridMineTransactionsLoadingScreen, _controlsToDisable, () =>
            {
                this.SetLoaderStatus("Mining transactions...");
                minedBlock = _miner_TS.Mine();
            });

            if (minedBlock != null)
            {
                lblMineTransactionsStatus.Background = Brushes.Green;
                lblMineTransactionsStatus.Content = $"Mined Block: {minedBlock.Hash}";
            }
            else
            {
                lblMineTransactionsStatus.Background = Brushes.Red;
                lblMineTransactionsStatus.Content = "No block has been mined";
            }
        }

        private async void btnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();

            txtApiTestResponse.ResetValue(true);

            if (ddlPeers.SelectedId() == -1 || ddlRequests.SelectedId() == -1)
            {
                await this.ShowMessageAsync("Error", "Peer and request must be selected");
                return;
            }

            _controlsToDisable.DisableControls();
            gridMain.ShowLoader();

            try
            {
                var requestName = ddlRequests.SelectedItem().Text;
                var peer = ddlPeers.SelectedItem().Text;
                var client = new PeersClient(peer);

                if (requestName == nameof(BlockchainApiController.Block))
                {
                    var id = xneQueryId.Text.ToIntN() ?? 0;

                    var blockResponse = await client.BlockAsync(id);

                    txtApiTestResponse.ClearValue(true).Text = blockResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.Blocks))
                {
                    var blocksResponse = await client.BlocksAsync();

                    txtApiTestResponse.ClearValue(true).Text = blocksResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.BlockchainLength))
                {
                    var blocksLengthResponse = await client.BlocksLengthAsync();

                    txtApiTestResponse.ClearValue(true).Text = blocksLengthResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.KnownAddresses))
                {
                    var knownAddressesResponse = await client.KnownAddressesAsync();

                    txtApiTestResponse.ClearValue(true).Text = knownAddressesResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.MineArtificial))
                {
                    _logger.Debug($"{nameof(btnSendRequest)} | {nameof(BlockchainApiController.MineArtificial)}");

                    var transactionInputControls = GetMineArtificialAPiTestInputControls();
                    var transactions = new Dictionary<string, Transaction>();

                    foreach (var (xce, txt) in transactionInputControls)
                    {
                        var amount = xce.Text.BeforeFirst(" EXC").ToDecimalN() ?? 0;
                        var recipient = txt.NullifyIfTag().Text;

                        if (amount <= 0 || recipient.IsNullOrWhiteSpace())
                            continue;
                        var transaction = new Transaction().CreateAsValid(new CustomWallet().Create(), recipient, amount); // these are requests, real transactions are created by peer/server // OLD: async to prevent 4.7 bug with freezing and "not enough quota" exception on quick button clicking | lock should bee ussed in async context at all times
                        transactions[transaction.Id] = transaction;
                    }

                    var mineArtificialResponse = await client.MineArtificialAsync(transactions);

                    txtApiTestResponse.ClearValue(true).Text = mineArtificialResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.MineTransactions))
                {
                    var mineTransactionsResponse = await client.MineTransactionsAsync();

                    txtApiTestResponse.ClearValue(true).Text = mineTransactionsResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.Send))
                {
                    var amount = xceAmount_Send_ApiTest.Text.BeforeFirst(" EXC").ToDecimalN() ?? 0;
                    var recipient = txtRecipient_Send_ApiTest.NullifyIfTag().Text;

                    var transactionResponse = await client.SendAsync(recipient, amount);

                    txtApiTestResponse.ClearValue(true).Text = transactionResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.Transactions))
                {
                    var transactionsResponse = await client.TransactionsAsync();

                    txtApiTestResponse.ClearValue(true).Text = transactionsResponse.JsonSerialize();
                }
                else if (requestName == nameof(BlockchainApiController.WalletInfo))
                {
                    var walletInfoResponse = await client.WalletInfoAsync();

                    txtApiTestResponse.ClearValue(true).Text = walletInfoResponse.JsonSerialize();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                await this.ShowMessageAsync("Error", ex.Message);
            }
            finally
            {
                gridMain.HideLoader();
                _controlsToDisable.EnableControls();
            }
        }

        private void btnRecipientGenerateAddress_Click(object sender, RoutedEventArgs e)
        {
            xceAmountToSend.Text = RandomUtils.RandomDecimalBetween(1, _wallet.UpdateBalances_TS(_bc, _tp).SpendableBalance) + " EXC";
            txtRecipientAddress.ClearValue(true).Text = new CustomWallet().Create().Address;
        }

        private void btnRecipientGenerateAddress_Send_ApiTest_Click(object sender, RoutedEventArgs e)
        {
            xceAmount_Send_ApiTest.Text = RandomUtils.RandomDecimalBetween(1, _wallet.UpdateBalances_TS(_bc, _tp).SpendableBalance) + " EXC";
            txtRecipient_Send_ApiTest.ClearValue(true).Text = new CustomWallet().Create().Address;
        }

        private void ddlRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xneQueryId.Text = string.Empty;

            var selectedItem = ddlRequests.SelectedItem();
            var gridsApiTest = gridHttpApiTestMainTab.LogicalDescendants<Grid>().Where(g => g.Name.EndsWith("_ApiTest")).ToArray();
            var selectedGrid = gridsApiTest.SingleOrDefault(g => g.Name.Between("grid", "_ApiTest").Equals(selectedItem?.Text));
            var notSelectedGrids = gridsApiTest.Except(selectedGrid).ToArray();

            notSelectedGrids.ForEach(g => g.Visibility = Visibility.Collapsed);
            if (selectedGrid != null)
            {
                selectedGrid.Visibility = Visibility.Visible;

                if (selectedGrid.Name == nameof(gridMineArtificial_ApiTest))
                {
                    var transactionInputControls = GetMineArtificialAPiTestInputControls();

                    foreach (var (xce, txt) in transactionInputControls)
                    {
                        xce.Text = RandomUtils.RandomDecimalBetween(1, 1000).Round(8) + " EXC";
                        txt.ClearValue(true).Text = new CustomWallet().Create().Address;
                    }
                }
                else if (selectedGrid.Name == nameof(gridSend_ApiTest))
                {
                    xceAmount_Send_ApiTest.Text = RandomUtils.RandomDecimalBetween(1, 1000).Round(8) + " EXC";
                    txtRecipient_Send_ApiTest.ClearValue(true).Text = new CustomWallet().Create().Address;
                }
            }

            if (selectedItem != null)
            {
                if (selectedItem.Text == nameof(BlockchainApiController.Block))
                {
                    xneQueryId.Text = RandomUtils.RandomIntBetween(0, 10).ToString();
                }
            }


        }

        private Tuple<TextBox, TextBox>[] GetMineArtificialAPiTestInputControls()
        {
            return gridHttpApiTestMainTab.LogicalDescendants<TextBox>()
                .Where(xce => xce.Name.StartsWith("xceTransaction") && xce.Name.EndsWith("Amount_MineArtificial_ApiTest"))
                .Select(xce => new Tuple<TextBox, TextBox>(xce,
                    gridHttpApiTestMainTab.LogicalDescendants<TextBox>().Single(txt =>
                    {
                        var xceId = xce.Name.Between("xceTransaction", "Amount_MineArtificial_ApiTest").ToInt();
                        return txt.Name == $"txtTransaction{xceId}Recipient_MineArtificial_ApiTest";
                    })))
                .ToArray();
        }

        private async void gvTransactions_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            await CtrlCCopySelectedDataGridContent(_ocTransactions, e);
        }

        private async void gvBlockchainExplorer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            await CtrlCCopySelectedDataGridContent(_ocBlocks, e);
        }

        private async Task CtrlCCopySelectedDataGridContent<T>(ObservableCollection<T> oc, KeyEventArgs e)
        {
            if (e.Key != Key.C || !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) // if not CTRL + C then return
                return;
            
            var selectedItems = oc.ToArray();
            if (selectedItems.Length <= 0)
                return;

            var copyToCB = ClipboardUtils.TrySetText(selectedItems.JoinAsString("\n\n"));
            if (copyToCB.IsFailure)
                await this.ShowMessageAsync("Error", copyToCB.Message);
        }

        private void HandleMainTabChange(Grid gridToShow)
        {
            UpdateControlValuesWithGridVisibility();
            RepaintDataGrids(gridToShow);
        }

        private void RepaintDataGrids(Grid grid) => grid.LogicalDescendants<DataGrid>().ForEach(dg => dg.Refresh()); // if I won't repaint the control, it won't respect "*"s for dg's with datateemplated columns for releease builds

        private void Host()
        {
            var ports = Enumerable.Range(3011, 100000).Select(n => new[] { n, n + 2000 }).First(p => !p.Intersect(Portutils.PortsInUse()).Any());
            var httpPort = ports[0];
            var p2pPort = ports[1];

            _wallet = BlockchainApiController.Wallet = new CustomWallet().Create();
            _btcWallet = new BitcoinWallet().CreateWithECPrivateKeyByteArray(_wallet.RawECPrivateKey.ToHexByteArray());
            _bc = BlockchainApiController.Bc = new Blockchain();
            _tp = BlockchainApiController.Tp = new TransactionPool();
            _p2pServer = BlockchainApiController.P2PServer_TS = new P2PServer(_bc, _tp, p2pPort, null);
            _httpServer = new HttpServer(httpPort);
            _miner_TS = BlockchainApiController.Miner_TS = new Miner(_bc, _tp, _wallet, _p2pServer);

            _bc.ChainChanged += Blockchain_ChainChanged;
            _bc.GeneratedBlockHash += Blockchain_GeneratedBlockHash;
            _tp.TransactionChanged += TransactionPool_TransactionChanged;
            _p2pServer.PeerConnected += P2PServer_PeerConnected;

            _bc.ReceiveInfoWith<Blockchain>(Log_MessageReceived);
            _tp.ReceiveInfoWith<TransactionPool>(Log_MessageReceived);
            _p2pServer.ReceiveInfoWith<P2PServer>(Log_MessageReceived);
            _httpServer.ReceiveInfoWith<HttpServer>(Log_MessageReceived);

            EnqueueSetValue(ddlPeers, _p2pServer.Address.WsAddressToHttpApiAddress().ToArrayOfOne()).OrSetDirectlyIfGridVisible(gridHttpApiTestMainTab);

            _httpServer.Listen();
            _p2pServer.Listen();
        }

        private List<TransactionGvVM> CreateTransactionsVM()
        {
            var confirmedTransactionsVM = _bc.GetTransactions().ToTransactionsGvVM(_wallet.Address, TransactionGvVMState.Confirmed);
            var unconfirmedTransactionsVM = _tp.GetTransactions().ToTransactionsGvVM(_wallet.Address, TransactionGvVMState.Unconfirmed);
            return confirmedTransactionsVM.Concat(unconfirmedTransactionsVM).OrderByDescending(t => t.TimeStamp).ToList();
        }
    }
}
