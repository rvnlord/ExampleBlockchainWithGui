using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Models;
using BlockchainApp.Source.Models.Wallets;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using static BlockchainApp.Source.Common.Utils.LockUtils;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Servers
{
    public class P2PServer : InformationSender
    {
        private readonly WebSocketServer _wsServer;
        
        public int P2PPort { get; }
        public Blockchain Blockchain { get; }
        public TransactionPool TransactionPool { get; }
        public List<Peer> Peers { get; private set; } // all incoming peer sockets will have destination address, server address, this
        public string IP => _wsServer?.Address.ToString().ToIP();
        public string Address => $"{IP}:{P2PPort}";
        public List<Peer> ActivePeers => Peers.Where(peer => peer.Socket != null && peer.Socket.IsAlive && peer.Socket.ReadyState == WebSocketState.Open).ToList();

        public P2PServer(Blockchain blockchain, TransactionPool transactionPool, int p2pPort, string[] peerAddresses)
        {
            _wsServer = new WebSocketServer(p2pPort);
            Blockchain = blockchain;
            TransactionPool = transactionPool;
            P2PPort = p2pPort;

            Lock (_syncPeers, nameof(_syncPeers), $"{nameof(P2PServer)} ctor", () =>
                CreatePeers(peerAddresses));
        }

        private List<string> FindPeers()
        {
            return Enumerable.Range(5011, P2PPort - 5011).Select(p => $"localhost:{p}").ToList();
        }

        private void CreatePeers(string[] peerAddresses)
        {
            Peers = (peerAddresses?.Any() != true ? FindPeers() : peerAddresses.ToList()).Select(a =>
            {
                var ip = a.AfterFirst("://").BeforeLast(":").ToIP();
                var port = a.AfterLast(":").ToIntN();
                var address = $"{ip}{(port != null ? $":{port}" : "")}";
                return new Peer(address, null);
            }).Where(p => p.Address != Address).ToList();
        }

        public void Listen()
        {
            _wsServer.Log.Disable();
            _wsServer.AddWebSocketService<P2PServerSocketBehavior>("/", bhv =>
            {
                bhv.Connection += ServerSocket_ConnectionReceived;
                bhv.Exception += (s, e) => CloseSocket(e.Socket, e.Message);
                bhv.Disconnect += (s, e) => CloseSocket(e.Socket, e.Reason);
            });

            _wsServer.Start();

            OnInformationSending($"Listening for peer-to-peer connections on port {P2PPort}");

            Lock (_syncPeers, nameof(_syncPeers), $"{nameof(Listen)}", 
                ConnectToPeers);
        }

        private void ConnectToPeers()
        {
            foreach (var peer in Peers)
            {
                if (peer.Socket != null && peer.Socket.IsAlive && peer.Socket.ReadyState == WebSocketState.Open)
                    continue;

                var newSocket = new WebSocket($"ws://{peer.Address}/");
                newSocket.Log.Disable();
                newSocket.OnOpen += ClientSocket_ConnectionEstablished;

                newSocket.Connect();
                if (!newSocket.IsAlive)
                    continue;

                newSocket.OnError += (s, e) => CloseSocket((WebSocket) s, e.Message);
                newSocket.OnClose += (s, e) => CloseSocket((WebSocket) s, e.Reason);

                peer.Socket = newSocket;
                var activePeersNo = ActivePeers.Count;
                OnInformationSending($"Socket connected ({Address} --> {peer.Address}) (peers: {activePeersNo})");
                OnPeerConnectionChanging(peer, activePeersNo, PeerConnectionChangeType.Connect);

                var validTransactions = TransactionPool.ValidTransactions_TS();

                SendChain(peer.Socket);
                SendTransactions(peer.Socket, validTransactions);
            }
        }

        private void ServerSocket_ConnectionReceived(object s, ConnectionEventArgs e)
        {
            var receivedSocket = e.Socket;
            receivedSocket.OnMessage += ClientSocket_MessageReceived;
            SendPeers(receivedSocket);
        }

        private void ClientSocket_ConnectionEstablished(object s, EventArgs e)
        {
            var createdSocket = (WebSocket) s;
            createdSocket.OnMessage += ClientSocket_MessageReceived;
            SendPeers(createdSocket);
        }

        private void ClientSocket_MessageReceived(object sender, MessageEventArgs e)
        {
            var data = e.Data.JsonDeserialize();
            if (data["type"].ToString() == MessageTypes.CHAIN.EnumToString())
            {
                Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(ClientSocket_MessageReceived), () =>
                {
                    var blocks = data["chain"].To<List<Block>>();
                    Blockchain.ReplaceChain(blocks, true, () => TransactionPool.ClearBlockchainTransactions(blocks));
                });
            }
            else if (data["type"].ToString() == MessageTypes.TRANSACTIONS.EnumToString())
            {
                Lock(_syncTransactionPool, nameof(_syncTransactionPool), nameof(ClientSocket_MessageReceived), () =>
                {
                    TransactionPool.MergeTransactions(data["transactions"].To<Dictionary<string, Transaction>>());
                });
            }
            else if (data["type"].ToString() == MessageTypes.PEERS.EnumToString())
            {
                Lock(_syncPeers, nameof(_syncPeers), nameof(ClientSocket_MessageReceived), () =>
                {
                    var receivedPeerAddresses = data["peers"].To<List<string>>();
                    var newPeerAddresses = receivedPeerAddresses
                        .Except(Peers.Select(p => p.Address)) // existing on the list
                        .Except(Address) // this
                        .ToList();
                    Peers.AddRange(newPeerAddresses.Select(a => new Peer(a, null)));
                    //OnInformationSending(
                    //    $"Added {newPeers.Count} new peers (total: {Peers.Count})\n" +
                    //    Peers.JoinAsString("\n"));
                    ConnectToPeers();
                });
            }
        }

        private void SendChain(WebSocket socket)
        {
            Lock(_syncBlockchain, nameof(_syncBlockchain), nameof(SendChain), () =>
                socket.Send(new JObject
                {
                    ["type"] = MessageTypes.CHAIN.EnumToString(),
                    ["chain"] = Blockchain.Chain.ToJToken()
                }.JsonSerialize()));
        }

        private void SendTransactions(WebSocket socket, Dictionary<string, Transaction> transactions)
        {
            socket.Send(new JObject
            {
                ["type"] = MessageTypes.TRANSACTIONS.EnumToString(),
                ["transactions"] = transactions.ToJToken()
            }.JsonSerialize());
        }

        private void SendPeers(WebSocket socket)
        {
            Lock(_syncPeers, nameof(_syncPeers), nameof(SendPeers), () =>
                socket.Send(new JObject
                {
                    ["type"] = MessageTypes.PEERS.EnumToString(),
                    ["peers"] = MoreLinq.MoreEnumerable.Prepend(Peers.Select(p => p.Address), Address).ToJToken()
                }.JsonSerialize()));
            //OnInformationSending(jPeers);
        }

        public void BroadcastChain()
        {
            Lock(_syncPeers, nameof(_syncPeers), nameof(BroadcastChain), () => 
                ActivePeers.Select(p => p.Socket).ForEach(SendChain));
        }

        public void BroadcastTransaction(Transaction transaction)
        {
            BroadcastTransactions(new Dictionary<string, Transaction> { [transaction.Id] = transaction });
        }

        public void BroadcastTransactions(Dictionary<string, Transaction> transactions)
        {
            Lock(_syncPeers, nameof(_syncPeers), nameof(BroadcastTransactions), () => 
                ActivePeers.Select(p => p.Socket).ForEach(socket => SendTransactions(socket, transactions)));
        }

        private void CloseSocket(WebSocket socket, string message)
        {
            Lock(_syncPeers, nameof(_syncPeers), nameof(CloseSocket), () =>
            {
                var peersToRemove = Peers.Where(p => p.Socket != null && p.Socket.ReadyState == WebSocketState.Closed).ToArray();
                Peers.RemoveRange(peersToRemove);
                foreach (var peer in peersToRemove)
                {
                    var activePeersNo = ActivePeers.Count;
                    OnInformationSending($"Socket disconnected ({peer.Address}) (peers: {activePeersNo}) ({message})");
                    OnPeerConnectionChanging(peer, activePeersNo, PeerConnectionChangeType.Disconnect);
                }

                socket.OnMessage -= ClientSocket_MessageReceived;
            });
        }

        public event PeerConnectionChangedEventHandler PeerConnected;

        protected virtual void OnPeerConnectionChanging(PeerConnectionChangedEventArgs e) => PeerConnected?.Invoke(this, e);
        protected virtual void OnPeerConnectionChanging(Peer peer, int peersCount, PeerConnectionChangeType actionType) => OnPeerConnectionChanging(new PeerConnectionChangedEventArgs(peer, peersCount, actionType));
    }

    public class Peer
    {
        public string Address { get; set; }
        public WebSocket Socket { get; set; }

        public Peer(string address, WebSocket socket)
        {
            Address = address;
            Socket = socket;
        }

        public override string ToString()
        {
            return $"--> [{Address}]";
        }
    }

    public class P2PServerSocketBehavior : WebSocketBehavior
    {
        protected override void OnOpen() => OnConnecting(Context.WebSocket, Context.UserEndPoint);
        protected override void OnError(ErrorEventArgs e) => OnExceptionHandling(Context.WebSocket, e.Exception, e.Message);
        protected override void OnClose(CloseEventArgs e) => OnDiconnecting(Context.WebSocket, e.Code, e.Reason, e.WasClean);
        
        public event ConnectionEventHandler Connection;
        public event ExceptionEventHandler Exception;
        public event DisconnectEventHandler Disconnect;
        
        protected virtual void OnConnecting(ConnectionEventArgs e) => Connection?.Invoke(this, e);
        protected virtual void OnConnecting(WebSocket socket, IPEndPoint clientEndpoint) => OnConnecting(new ConnectionEventArgs(socket, clientEndpoint));
        protected virtual void OnExceptionHandling(ExceptionEventArgs e) => Exception?.Invoke(this, e);
        protected virtual void OnExceptionHandling(WebSocket socket, Exception ex, string message) => OnExceptionHandling(new ExceptionEventArgs(socket, ex, message));
        protected virtual void OnDiconnecting(DisconnectEventArgs e) => Disconnect?.Invoke(this, e);
        protected virtual void OnDiconnecting(WebSocket socket, ushort code, string reason, bool wasClean) => OnDiconnecting(new DisconnectEventArgs(socket, code, reason, wasClean));
    }

    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);
    public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs e);
    public delegate void DisconnectEventHandler(object sender, DisconnectEventArgs e);
    public delegate void PeerConnectionChangedEventHandler(object sender, PeerConnectionChangedEventArgs e);

    public class ConnectionEventArgs
    {
        public WebSocket Socket { get; }
        public IPEndPoint ClientEndpoint { get; }

        public ConnectionEventArgs(WebSocket socket, IPEndPoint clientEndpoint)
        {
            Socket = socket;
            ClientEndpoint = clientEndpoint;
        }
    }

    public class ExceptionEventArgs
    {
        public WebSocket Socket { get; }
        public Exception Ex { get; }
        public string Message { get; }

        public ExceptionEventArgs(WebSocket socket, Exception ex, string message)
        {
            Socket = socket;
            Ex = ex;
            Message = message;
        }
    }

    public class DisconnectEventArgs
    {
        public WebSocket Socket { get; }
        public ushort Code { get; }
        public string Reason { get; }
        public bool WasClean { get; }

        public DisconnectEventArgs(WebSocket socket, ushort code, string reason, bool wasClean)
        {
            Socket = socket;
            Code = code;
            Reason = reason;
            WasClean = wasClean;
        }
    }

    public class PeerConnectionChangedEventArgs
    {
        public Peer Peer { get; }
        public int PeerCount { get; }
        public PeerConnectionChangeType ActionType { get; }

        public PeerConnectionChangedEventArgs(Peer peer, int peerCount, PeerConnectionChangeType actionType)
        {
            Peer = peer;
            PeerCount = peerCount;
            ActionType = actionType;
        }
    }

    public enum MessageTypes
    {
        CHAIN,
        TRANSACTIONS,
        PEERS
    }

    public enum PeerConnectionChangeType
    {
        Connect,
        Disconnect
    }
}
