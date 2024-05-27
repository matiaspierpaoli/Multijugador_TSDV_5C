using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Text;

public enum MessageType
{
    ClientToServerHandshake = -2,
    ServerToClientHandshake = -1,
    Console = 0,
    Position = 1
}

public abstract class BaseMessage<T>
{
    protected MessageType Type;
    protected T Data;
    public static int PlayerID;

    protected BaseMessage(T data)
    {
        Data = data;
    }

    protected BaseMessage()
    {
    }

    public MessageType GetMessageType()
    {
        return Type;
    }

    public static void SetPlayerId(int playerId)
    {
        
    }

    public int GetID()
    {
        return PlayerID;
    }

    public abstract byte[] Serialize();
    public abstract T Deserialize(byte[] message);

    public T GetData()
    {
        return Data;
    }
}

public abstract class OrderableMessage<PayloadType> : BaseMessage<PayloadType>
{
    protected static ulong lastMsgID = 0;
    protected static Dictionary<MessageType, ulong> lastSendMessage = new();
    protected ulong messageId;

    public bool IsTheNewestMessage()
    {
        if (lastSendMessage[Type] < messageId)
        {
            return false;
        }

        lastSendMessage[Type] = messageId;
        return true;
    }
}

public class ClientToServerHS : BaseMessage<string>
{
    public ClientToServerHS(string tag)
    {
        Data = tag;
        Type = MessageType.ClientToServerHandshake;
    }
    
    public ClientToServerHS() => Type = MessageType.ClientToServerHandshake;

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(PlayerID));

        outData.AddRange(BitConverter.GetBytes(Data.Length));
        for (int i = 0; i < Data.Length; i++)
        {
            outData.Add((byte)Data[i]);
        }

        return outData.ToArray();
    }

    public override string Deserialize(byte[] message)
    {
        int maxLength = BitConverter.ToInt32(message, 8);
        return Encoding.UTF8.GetString(message, 12, maxLength);
    }
}

public class ServerToClientHS : BaseMessage<List<Player>>
{
    public ServerToClientHS(List<Player> clients)
    {
        Data = clients;
        Type = MessageType.ServerToClientHandshake;
    }

    public ServerToClientHS() => Type = MessageType.ServerToClientHandshake;

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();
        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(PlayerID));
        outData.AddRange(BitConverter.GetBytes(Data.Count));

        foreach (Player player in Data)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(player.playerName);
            outData.AddRange(BitConverter.GetBytes(player.playerID));
            outData.AddRange(BitConverter.GetBytes(nameBytes.Length));
            outData.AddRange(nameBytes);
        }

        return outData.ToArray();
    }

    public override List<Player> Deserialize(byte[] message)
    {
        List<Player> players = new List<Player>();
        int maxClients = BitConverter.ToInt32(message, 8);
        int offset = 12;

        for (int i = 0; i < maxClients; i++)
        {
            int playerId = BitConverter.ToInt32(message, offset);
            int nameLength = BitConverter.ToInt32(message, offset + 4);
            string playerName = Encoding.UTF8.GetString(message, offset + 8, nameLength);

            players.Add(new Player(playerName, playerId));
            offset += 8 + nameLength;
        }

        return players;
    }
}

public class NetConsole : OrderableMessage<string>
{
    public string consolemessage;

    public NetConsole()
    {
        Type = MessageType.Console;
    }

    public NetConsole(string consoleMessage)
    {
        this.consolemessage = consoleMessage;
        Type = MessageType.Console;
    }

    public override string Deserialize(byte[] message)
    {
        string outData = "";
        int messageLength = BitConverter.ToInt32(message, 16);
        Debug.Log(messageLength);
        for (int i = 0; i < messageLength; i++)
        {
            outData += (char)message[20 + i];
        }

        return outData;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(PlayerID));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(consolemessage.Length));
        outData.AddRange(Encoding.UTF8.GetBytes(consolemessage));

        return outData.ToArray();
    }
}

public class NetVector3 : OrderableMessage<UnityEngine.Vector3>
{
    private static ulong lastMsgID = 0;
    private Vector3 data;

    public NetVector3(Vector3 data)
    {
        this.data = data;
        Type = MessageType.Position;
    }

    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 8);
        outData.y = BitConverter.ToSingle(message, 12);
        outData.z = BitConverter.ToSingle(message, 16);

        return outData;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(PlayerID));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }

    //Dictionary<Client,Dictionary<msgType,int>>
}