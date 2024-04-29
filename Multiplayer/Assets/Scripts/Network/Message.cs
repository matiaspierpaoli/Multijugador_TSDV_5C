using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Text;

public enum MessageType
{
    ClientToServerHS = -3,
    ServerToClientHS = -2,
    HandShake = -1,
    Console = 0,
    Position = 1
}

public abstract class Message<T>
{
    public T data;

    public static Action<T> OnDispatch;

    public abstract MessageType GetMessageType();
    public abstract byte[] Serialize();
    public abstract T Deserialize(byte[] message);
    public abstract T GetData();
}

public class NetServerToClientHS : Message<Player>
{
    new public static Action<List<Player>> OnDispatch;

    private Player data;

    public override MessageType GetMessageType()
    {
        return MessageType.ServerToClientHS;
    }

    public override Player Deserialize(byte[] message)
    {
        using MemoryStream memStream = new MemoryStream(message);
        using BinaryReader reader = new BinaryReader(memStream);
        reader.ReadInt32();

        string playerName = reader.ReadString();
        int playerID = reader.ReadInt32();

        return new Player(playerName, playerID);
    }

    public override Player GetData()
    {
        return data;
    }

    public override byte[] Serialize()
    {
        using MemoryStream memStream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(memStream);

        writer.Write((int)GetMessageType());
        writer.Write(data.playerName);
        writer.Write(data.playerID);

        return memStream.ToArray();
    }
}

public class ClientToServerHS : Message<string>
{
    new private string data;

    public override string Deserialize(byte[] message)
    {
        int stringlenght = BitConverter.ToInt32(message, 4);
        return Encoding.UTF8.GetString(message, 8, stringlenght);
    }

    public override string GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.ClientToServerHS;
    }

    public override byte[] Serialize()
    {
        using MemoryStream memStream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(memStream);

        writer.Write((int)GetMessageType());
        writer.Write(data.Length);
        writer.Write(data);

        return memStream.ToArray();
    }
}

public class NetHandShake : Message<int>
{
    new int data;
    bool isClientIdAssigned = false;

    public NetHandShake()
    {
        data = -1;
    }

    public NetHandShake(byte[] dataToDeserialize)
    {
        data = Deserialize(dataToDeserialize);
    }

    public void SetClientID(int clientID)
    {
        data = clientID;
        isClientIdAssigned = true;
    }

    public bool IsClientIdAssigned()
    {
        return isClientIdAssigned;
    }

    public override int Deserialize(byte[] message)
    {
        return BitConverter.ToInt32(message, 4);
    }

    public override int GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
       return MessageType.HandShake;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
}

public class NetVector3 : Message<UnityEngine.Vector3>
{
    private static ulong lastMsgID = 0;
    private Vector3 data;

    public NetVector3(Vector3 data)
    {
        this.data = data;
    }

    public override Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 8);
        outData.y = BitConverter.ToSingle(message, 12);
        outData.z = BitConverter.ToSingle(message, 16);

        return outData;
    }

    public override Vector3 GetData()
    {
        return data;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Position;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));
        outData.AddRange(BitConverter.GetBytes(data.z));

        return outData.ToArray();
    }

    //Dictionary<Client,Dictionary<msgType,int>>
}

public struct Player
{
    public string playerName;
    public int playerID;

    public Player(string playerName, int playerID)
    {
        this.playerName = playerName;
        this.playerID = playerID;
    }
}