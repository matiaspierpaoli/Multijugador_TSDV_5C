using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class MessageHandler : MonoBehaviourSingleton<MessageHandler>
{
    public ChatScreen chat;

    protected override void Initialize()
    {
        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
    }

    private void OnReceiveDataEvent(byte[] data, IPEndPoint ep, int id)
    {
        if (NetworkManager.Instance.isServer)
            HandleServerMessage(data, ep, id);
        else
            HandleMessage(data);
    }

    public void HandleMessage(byte[] data)
    {
        MemoryStream memStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memStream);
            
        MessageType temp = (MessageType)reader.ReadInt32();

        switch (temp)
        {
            case MessageType.ServerToClientHandshake:
                ServerToClientHS serverToClientHS = new ServerToClientHS();
                List<Player> players = serverToClientHS.Deserialize(data);
                NetworkManager.Instance.SetPlayer(players);
                Debug.Log("IP: " + NetworkManager.Instance.clientId);
                break;
            case MessageType.ClientToServerHandshake:
                break;
            case MessageType.Console:
                NetConsole consoleMessage = new();
                int playerId = BitConverter.ToInt32(data, 4);
                Debug.Log(playerId);
                chat.ReceiveConsoleMessage(consoleMessage.Deserialize(data), playerId);
                break;
            case MessageType.Position:
                //NetVector3.OnDispatch?.Invoke();
                break;
        }
    }

    public void HandleServerMessage(byte[] data, IPEndPoint ep, int id)
    {
        MemoryStream memStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memStream);
            
        MessageType temp = (MessageType)reader.ReadInt32();

        switch (temp)
        {
            case MessageType.ClientToServerHandshake:
                ClientToServerHS clientToServerHS = new ClientToServerHS();
                string gameTag = clientToServerHS.Deserialize(data);
                Debug.Log("Client ip: " + ep.Address + " and nametag: " + gameTag);
                NetworkManager.Instance.RegisterClient(ep, out id, gameTag);
                ServerToClientHS handOK = new ServerToClientHS(NetworkManager.Instance.PlayerList);
                NetworkManager.Instance.Broadcast(handOK.Serialize());
                break;
            case MessageType.ServerToClientHandshake:
                break;
            case MessageType.Console:
                NetConsole consoleMessage = new();
                chat.ReceiveConsoleMessage(consoleMessage.Deserialize(data), BitConverter.ToInt32(data, 4));
                NetworkManager.Instance.Broadcast(data);
                break;
            case MessageType.Position:
                break;
        }
    }
}
