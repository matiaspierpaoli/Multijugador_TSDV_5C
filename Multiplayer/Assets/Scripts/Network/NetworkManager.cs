using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    [NonSerialized] public int serverId = -10;

    public IPAddress ipAddress
    {
        get; private set;
    }

    public int port
    {
        get; private set;
    }

    public bool isServer
    {
        get; private set;
    }

    public int TimeOut = 30;

    public Action<byte[], IPEndPoint, int> OnReceiveEvent;

    private UdpConnection connection;

    private readonly Dictionary<int, Client> connectedClients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    public List<Player> PlayerList = new();
    public string tagName = "";

    public int clientId = 0;

    public void InitializedServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
        NetConsole.PlayerID = -10;
        NetVector3.PlayerID = -10;
    }

    public void InitializeClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, tagName, this);
    }

    public void RegisterClient(IPEndPoint ipEndPoint, out int clientId, string playerTag)
    {
        if (!ipToId.ContainsKey(ipEndPoint))
        {
            Debug.Log($"Registering client: {ipEndPoint.Address}");
            clientId = GenerateClientId();
            ipToId[ipEndPoint] = clientId;
            connectedClients[clientId] = new Client(ipEndPoint, clientId, Time.realtimeSinceStartup, playerTag);
            PlayerList.Add(new Player(playerTag, clientId));
        }
        else
        {
            clientId = ipToId[ipEndPoint];
        }
    }

    private int GenerateClientId()
    {
        return clientId++;
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            connectedClients.Remove(ipToId[ip]);
        }
    }

    public void SetPlayer(List<Player> updatedPlayers)
    {
        PlayerList = updatedPlayers;
        Player currentPlayer = PlayerList.Find(p => p.playerName == tagName);

        if (currentPlayer != null)
        {
            clientId = currentPlayer.playerID;
            NetConsole.PlayerID = clientId;
            NetVector3.PlayerID = clientId;
        }
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip, int id, string tag)
    {
        if (OnReceiveEvent != null)
            OnReceiveEvent.Invoke(data, ip, id);
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void SendToClient(byte[] data, IPEndPoint ip)
    {
        connection.Send(data, ip);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = connectedClients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }

    public Player GetPlayer(int id)
    {
        return PlayerList.Find(player => player.playerID == id) ?? new Player("Not Found", -999);
    }
}
