using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MachiKoro_Client.Models;
using MachiKoro_Client.ViewModels;
using DynamicData;
using JTProtocol;
using JTProtocol.Serializer;
using JTProtocol.JPackets;

namespace MachiKoro_Client;

public class JClient : ObservableObject
{
    // ---Client fields
    private readonly Queue<byte[]> _packetSendingQueue = new();

    private Socket? _socket;
    private IPEndPoint? _serverEndPoint;
    
    // ---Player fields


    private int _clientID;

    private bool isGameEnded;

    private string winString;


    private ObservableCollection<string> _playerMessages;
    public ObservableCollection<string> PlayerMessages
    {
        get => _playerMessages;
        set
        {
            _playerMessages = value;
            OnPropertyChanged();
        }

    }
    
    
    public string WinString
    {
        get => winString;
        set
        {
            winString = value;
            OnPropertyChanged();
        }
    }
    
    
    
    

    public bool IsGameEnded
    {
        get => isGameEnded;
        set
        {
            isGameEnded = value;
            OnPropertyChanged();
        }
    }
    
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public int ClientID
    {
        get => _clientID;
        set
        {
            _clientID = value;
            OnPropertyChanged();
        }
    }

    private bool isPlayerListFull = false;

    private CardShop Shop;

    private int _coins;
    public int Coins
    {
        get => _coins;
        set
        {
            _coins = value;
            OnPropertyChanged();
        }
    } // --------------
    

    private bool _isYourTurn;
    public bool IsYourTurn
    {
        get => _isYourTurn;
        set
        {
            _isYourTurn = value;
            OnPropertyChanged();
        }
    }

    private int _diceThrowResult;
    public int DiceThrowResult
    {
        get => _diceThrowResult;
        set
        {
            _diceThrowResult = value;
            OnPropertyChanged();
        }
    }

    
    private ObservableCollection<Tuple<int,string>> _enemies;
    public ObservableCollection<Tuple<int,string>> Enemies
    {
        get => _enemies;
        set
        {
            _enemies = value;
            OnPropertyChanged();
        }

    }

    private ObservableCollection<ICard> _myCards;
    public ObservableCollection<ICard> MyCards
    {
        get => _myCards;
        set
        {
            _myCards = value;
            OnPropertyChanged();
        }

    }
    
    public JClient()
    {
        isGameEnded = false;
        Coins = 2;
        MyCards = new ObservableCollection<ICard>()
        {
            new WheatFieldCard(),
            new BackeryCard()
        };
        Enemies = new ObservableCollection<Tuple<int, string>>();
        PlayerMessages = new ObservableCollection<string>();
    }



    public async Task StartSessionAsync()
    {
        Shop = new CardShop();
        IsYourTurn = false;
        try
        {
            ConnectAsync("127.0.0.1", 4910);
            
            QueuePacketSend(JPacketConverter.Serialize(JPacketType.Connection,
                new JPacketConnection
                {
                    IsSuccessful = false,
                    PlayerName = Name
                }).ToPacket());

            await Task.Delay(100);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private void ConnectAsync(string ip, int port) => ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));

    private async Task ConnectAsync(IPEndPoint? server)
    {
        _serverEndPoint = server;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await _socket.ConnectAsync(_serverEndPoint!);

        Task.Run(ReceivePacketsAsync);
        Task.Run(SendPacketsAsync);
    }

    public void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 256)
        {
            throw new Exception("Max packet size is 256 bytes.");
        }

        _packetSendingQueue.Enqueue(packet);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true)
        {
            var buff = new byte[128];
            await _socket!.ReceiveAsync(buff);
            var decrBuff = JProtocolEncryptor.Decrypt(buff);

            buff = decrBuff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return decrBuff[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();
            var parsed = JPacket.Parse(buff);

            if (parsed != null!)
            {
                ProcessIncomingPacket(parsed);
            }
        }
    }

    private void ProcessIncomingPacket(JPacket packet)
    {
        var type = JPacketTypeManager.GetTypeFromPacket(packet);

        switch (type)
        {
            case JPacketType.Connection:
                ProcessConnection(packet);
                break;
            case JPacketType.Unknown:
                break;
            case JPacketType.DiceThrowResult:
                ProcessThrowResultHandle(packet);
                break;
            case JPacketType.TakeCoins:
                ProcessTakeCoinsFromPayer(packet);
                break;
            case JPacketType.GiveCoins:
                ProcessGiveCoinsToPlayer(packet);
                break;
            case JPacketType.ChangeTurn:
                ProcessChangeTurn(packet);
                break;
            case JPacketType.PlayersInformation:
                ProcessPlayersInformation(packet);
                break;
            case JPacketType.EndGame:
                ProcessEndGame(packet);
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }
    
    
    
    
    
    // --- Process 

    private  void ProcessConnection(JPacket packet)
    {
        var connection = JPacketConverter.Deserialize<JPacketConnection>(packet);

        if (connection.IsSuccessful)
        {
            PlayerMessages.Add("Handshake successful!");
            ClientID = connection.id;
            PlayerMessages.Add("Your id:" + ClientID);
        }
        
    }

    private void ProcessPlayersInformation(JPacket packet)
    {
        PlayerMessages.Add($"Принят игрок ");
        
            var playersList = JPacketConverter.Deserialize<JPacketPlayersListInformation>(packet);
            foreach (var playerData in playersList.PlayerInformationList)
            {
                PlayerMessages.Add($"Принят игрок {playerData.Item1}  {playerData.Item2}"  );
                if (ClientID != playerData.Item1)
                {
                    Enemies.Add(new Tuple<int, string>(playerData.Item1, playerData.Item2));
                }
            }
    }

    private void ProcessEndGame(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketEndGame>(packet);

        isGameEnded = true;
        if (ClientID == data.WinnerID)
        {
            WinString = "Ты победил!";
        }
        else
        {
            WinString = $"Победил игрок с ID {data.WinnerID}";
        }
        
    }

    private void ProcessThrowResultHandle(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketDiceThrowResult>(packet);


        DiceThrowResult = data.Result;
        
        if(data.PlayerID != ClientID) PlayerMessages.Add($"Игроку номер {data.PlayerID} выпало {data.Result}");
        
        RedCardCheck(data.Result, data.PlayerID);
        BlueCardCheck(data.Result, data.PlayerID);
    }
    

    private void ProcessTakeCoinsFromPayer(JPacket packet)
    {
        int takenCoins = 0;
        var data = JPacketConverter.Deserialize<JPacketTakeCoins>(packet);
        if (Coins > data.CoinsToTake)
        {
            takenCoins = Coins - data.CoinsToTake;
            Coins -= data.CoinsToTake;
        }
        else
        {
            takenCoins = Coins;
            Coins = 0;
        }
        
        PlayerMessages.Add("not stonks, монет осталось:" + Coins);
    }

    private void ProcessGiveCoinsToPlayer(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketGiveCoins>(packet);
        Coins += data.CoinsToGive;
        PlayerMessages.Add("STOOOONKS, теперь у тебя монет:" + Coins );
        CheckWinner();
    }

    private void ProcessChangeTurn(JPacket packet)
    {
        try
        {
            var data = JPacketConverter.Deserialize<JPacketChangeTurn>(packet);

            if (ClientID == data.PlayerID )
            {
                IsYourTurn = true;
                PlayerMessages.Add("Твой Ход");
            }
            else
            {
                IsYourTurn = false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // --- 

    private void BlueCardCheck(int diceValue, int throwerId)
    {
        int result = 0;
        foreach (var card in MyCards)
        {
            if (card.Type == CardTypes.Blue)
                foreach (var n in card.TriggerNubmers)
                {
                    if(n == diceValue) result += card.EarnCoinsTriggerNumber;
                }
        }

        Coins += result;
        CheckWinner();
    }
    private void GreenCardCheck(int diceValue, int throwerId)
    {
        int result = 0;
        foreach (var card in MyCards)
        {
            if (card.Type == CardTypes.Green)
                foreach (var n in card.TriggerNubmers)
                {
                    if(n == diceValue) result += card.EarnCoinsTriggerNumber;
                }
        }

        Coins += result;
        CheckWinner();
    }

    

    private void RedCardCheck(int diceValue, int throwerId)
    {
        if (ClientID != throwerId)
        {
            int result = 0;
            foreach (var card in MyCards)
            {
                if (card.Type == CardTypes.Red)
                    foreach (var n in card.TriggerNubmers)
                    {
                        if (n == diceValue) result += card.EarnCoinsTriggerNumber;
                    }
            }
            
            if (result > 0)
            {
                QueuePacketSend(JPacketConverter.Serialize(JPacketType.Payment,
                    new JPacketPayment()
                    {
                        GiveToClient = ClientID,
                        TakeFromClient = throwerId,
                        CoinsAmountToTake = result
                    }).ToPacket());
            }
            
        }
    }
    
    public void DiceThrowPacketCreate() // если игрок кинул кости - послать пакет с информацией об этом на сервер и ждать ответ (мб у кого-то есть красные карты)
    {
        Random r = new Random();
        DiceThrowResult = r.Next(1, 6);
        
        GreenCardCheck(DiceThrowResult, ClientID);
        
        QueuePacketSend(JPacketConverter.Serialize(JPacketType.DiceThrowAction,
            new JPacketDiceThrowAction()
            {
                PlayerID = ClientID,
                Value = DiceThrowResult
            }).ToPacket());
        PlayerMessages.Add("На кубе выпало " + DiceThrowResult);
    }

    public void ChangeTurn()
    {
        IsYourTurn = false;
        QueuePacketSend(JPacketConverter.Serialize(JPacketType.ChangeTurn,
            new JPacketChangeTurn()
            {
                PlayerID = ClientID,
            }).ToPacket());
        PlayerMessages.Add("Ход перешел к другому игроку");
    }

    public void BuyCard(string cardName)
    {
        if (Coins >= Shop.Cards[cardName].Cost)
        {
            MyCards.Add(Shop.Cards[cardName]);
            Coins -= Shop.Cards[cardName].Cost;
        }
        
    }

    public void CheckWinner()
    {
        if (Coins >= 15)
        {
            QueuePacketSend(JPacketConverter.Serialize(JPacketType.EndGame,
                new JPacketEndGame()
                {
                    WinnerID = ClientID,
                }).ToPacket());
        }
    }
    
    // ---
    
    
    private async Task SendPacketsAsync()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
            {
                Thread.Sleep(100);
                continue;
            }
            var packet = _packetSendingQueue.Dequeue();
            var encryptedPacket = JProtocolEncryptor.Encrypt(packet);
            await _socket!.SendAsync(encryptedPacket);

            await Task.Delay(100);
        }
    }
    


}