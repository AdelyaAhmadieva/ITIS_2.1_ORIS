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
    
    public int clientID;

    public List<Player> OtherPlayers;

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
        Coins = 2;
        MyCards = new ObservableCollection<ICard>()
        {
            new WheatFieldCard()
        };
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
                    IsSuccessful = false
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
            Console.WriteLine("Handshake successful!");
            clientID = connection.id;
            Console.WriteLine("Your id:" + clientID);
        }
        
    }

    private void ProcessPlayersInformation(JPacket packet)
    {
        var playersList = JPacketConverter.Deserialize<JPacketConnection>(packet);
        
        
    }

    private void ProcessThrowResultHandle(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketDiceThrowResult>(packet);


        DiceThrowResult = data.Result;
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
        
        Console.WriteLine("not stonks, монет осталось:" + Coins);
    }

    private void ProcessGiveCoinsToPlayer(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketGiveCoins>(packet);
        Coins += data.CoinsToGive;
        Console.WriteLine("STOOOONKS, теперь у тебя монет:" + Coins );
    }

    private void ProcessChangeTurn(JPacket packet)
    {
        try
        {
            var data = JPacketConverter.Deserialize<JPacketChangeTurn>(packet);
            Console.WriteLine($"Мне пришел найди {data.PlayerID}, а мой {clientID}");

            if (clientID == data.PlayerID )
            {
                IsYourTurn = true;
                Console.WriteLine("Ваш ход");
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
                    if(n == diceValue) result += card.Cost;
                }
        }

        Coins += result;
    }

    private void RedCardCheck(int diceValue, int throwerId)
    {
        Console.WriteLine("fd");
    }
    
    public void DiceThrowPacketCreate() // если игрок кинул кости - послать пакет с информацией об этом на сервер и ждать ответ (мб у кого-то есть красные карты)
    {
        Random r = new Random();
        DiceThrowResult = r.Next(1, 6);
        
        QueuePacketSend(JPacketConverter.Serialize(JPacketType.DiceThrowAction,
            new JPacketDiceThrowAction()
            {
                PlayerID = clientID,
                Value = DiceThrowResult
            }).ToPacket());
        Console.WriteLine("На кубе выпало " + DiceThrowResult);
        
        
        
    }

    public void ChangeTurn()
    {
        IsYourTurn = false;
        QueuePacketSend(JPacketConverter.Serialize(JPacketType.ChangeTurn,
            new JPacketChangeTurn()
            {
                PlayerID = clientID,
            }).ToPacket());
        Console.WriteLine("Ход перешел");
    }

    public void BuyCard(string cardName)
    {
        if (Coins >= Shop.Cards[cardName].Cost)
        {
            MyCards.Add(Shop.Cards[cardName]);
            Coins -= Shop.Cards[cardName].Cost;
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