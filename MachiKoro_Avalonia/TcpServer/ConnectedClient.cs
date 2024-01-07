using System.Drawing;
using System.Net.Sockets;
using JTProtocol;
using JTProtocol.Serializer;
using JTProtocol.JPackets;

namespace TCPServer;

internal class ConnectedClient
{
    private Socket Client { get; }
    List<Tuple<int, string>> Information = new List<Tuple<int, string>>();

    private readonly Queue<byte[]> _packetSendingQueue = new();
    

    private readonly Random _random = new();

    private string Name { get; set; }

    private int ID { get; set; }

    public ConnectedClient(Socket client)
    {
        Client = client;

        Name = "none";

        Task.Run(ReceivePacketsAsync);
        Task.Run(SendPacketsAsync);
    }

    private async Task ReceivePacketsAsync()
    {
        while (true) // Слушаем пакеты, пока клиент не отключится.
        {
            var buff = new byte[128]; // Максимальный размер пакета - 128 байт - так Шарп сказал...
            await Client.ReceiveAsync(buff);
            var decrBuff = JProtocolEncryptor.Decrypt(buff);

            buff = decrBuff.TakeWhile((b, i) =>
            {
                if (b != 0xFF) return true;
                return decrBuff[i + 1] != 0;
            }).Concat(new byte[] { 0xFF, 0 }).ToArray();

            var parsed = JPacket.Parse(buff);

            if (parsed != null!) ProcessIncomingPacket(parsed);
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
            case JPacketType.DiceThrowAction:
                ProcessDiceThrowAction(packet);
                break;
            case JPacketType.Payment:
                ProcessPayment(packet);
                break;
            case JPacketType.ChangeTurn:
                ProcessChangeTurn(packet);
                break;
            case JPacketType.EndGame:
                ProcessEndGame(packet);
                break;
            default:
                throw new ArgumentException("Получен неизвестный пакет");
        }
    }

    // --- Process -- 
    
    private void ProcessConnection(JPacket packet)
    {
        var connection = JPacketConverter.Deserialize<JPacketConnection>(packet);
        connection.IsSuccessful = true;
        connection.id = JServer._clients.Count - 1;
        ID = connection.id;
        Console.WriteLine(ID);
        Name = connection.PlayerName;
        Console.WriteLine("Handshake successful, количество игроков: " + JServer._clients.Count);
        Thread.Sleep(100);
        QueuePacketSend(JPacketConverter.Serialize(JPacketType.Connection, connection).ToPacket());
    }
    

    private void ProcessDiceThrowAction(JPacket packet)
    {
        var throwResult = JPacketConverter.Deserialize<JPacketDiceThrowAction>(packet);
        Console.WriteLine("Результат броска " + throwResult.Value + "   " + "Id кидавшего " + throwResult.PlayerID);

        foreach (var client in JServer._clients)
        {
            client.QueuePacketSend(JPacketConverter.Serialize(JPacketType.DiceThrowResult,
                new JPacketDiceThrowResult()
                {
                    Result = throwResult.Value,
                    PlayerID = throwResult.PlayerID
                }).ToPacket());
            

            // -место для обработчика события триггреа красных карт на куб.
            // -кидает пакет с вопросом, есть ли красная карта на такое-то значание
            // если да - клиент кидает пакет с данными карты (boool-exist and int-value), если нет - кидает с bool = false 
        }
    }

    private void ProcessPayment(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketPayment>(packet);
        
        
        JServer._clients[data.TakeFromClient].QueuePacketSend(JPacketConverter.Serialize(JPacketType.TakeCoins, 
            new JPacketTakeCoins()
            {
                CoinsToTake = data.CoinsAmountToTake
            } ).ToPacket());
        JServer._clients[data.GiveToClient].QueuePacketSend(JPacketConverter.Serialize(JPacketType.GiveCoins, 
            new JPacketGiveCoins()
            {
                CoinsToGive= data.CoinsAmountToTake
            } ).ToPacket());
        
        Console.WriteLine("Игрок " + data.GiveToClient + "забрал у игрока "+ data.TakeFromClient + " монет " + data.CoinsAmountToTake );
    }

    private void ProcessChangeTurn(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketChangeTurn>(packet);
        int nextPlayerId = 0;

        if (data.PlayerID + 1 <= JServer._clients.Count() - 1)
            nextPlayerId = data.PlayerID + 1; 
        
        JServer._clients[nextPlayerId].QueuePacketSend(JPacketConverter.Serialize(JPacketType.ChangeTurn, 
            new JPacketChangeTurn()
            {
                PlayerID = nextPlayerId
            }).ToPacket());
        Console.WriteLine($"Хрд перешел к игроку номер {nextPlayerId}");

    }

    private void ProcessEndGame(JPacket packet)
    {
        var data = JPacketConverter.Deserialize<JPacketEndGame>(packet);
        foreach (var client in JServer._clients)
        {
            QueuePacketSend(JPacketConverter.Serialize(JPacketType.EndGame, 
                new JPacketEndGame()
                {
                    WinnerID = data.WinnerID
                }).ToPacket());
        }
    }
    
    
    
    private void QueuePacketSend(byte[] packet)
    {
        if (packet.Length > 256)
            throw new Exception("Max packet size is 128 bytes.");

        _packetSendingQueue.Enqueue(packet);
    }

    public void StartGameSession()
    {
        Thread.Sleep(1000);
        foreach (var c in JServer._clients)
        {
            Console.WriteLine("ID добавленного клиента" + c.ID);
            Information.Add(new Tuple<int, string>(c.ID, c.Name));
        }
        
        
        foreach (var client in JServer._clients)
        {
            Thread.Sleep(100);
           client.QueuePacketSend(JPacketConverter.Serialize(JPacketType.PlayersInformation, new JPacketPlayersListInformation()
           {
               PlayerInformationList = Information
           }).ToPacket());
        }

        int PlayerBeginID = _random.Next(0, JServer._clients.Count() - 1);
        JServer._clients[PlayerBeginID].QueuePacketSend(JPacketConverter.Serialize(JPacketType.ChangeTurn, 
                new JPacketChangeTurn()
                {
                    PlayerID = PlayerBeginID
                }).ToPacket());
        Console.WriteLine($"Игру начинает игрок номер {PlayerBeginID}");
        
    }
    
    private async Task SendPacketsAsync()
    {
        while (true)
        {
            if (_packetSendingQueue.Count == 0)
                continue;

            var packet = _packetSendingQueue.Dequeue();
            var encryptedPacket = JProtocolEncryptor.Encrypt(packet);
            await Client.SendAsync(encryptedPacket);

            await Task.Delay(100);
        }
    }
}