using System.IO;
using System.Net;
using UnityEngine;

public class MessageHandler : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
    }

    private void OnDisable()
    {
        NetworkManager.Instance.OnReceiveEvent -= OnReceiveDataEvent;
    }

    private void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {

        if (NetworkManager.Instance.isServer)
            HandleServerMessage(data, ep);
        else
            HandleMessage(data);
    }

    public void HandleMessage(byte[] message)
    {
        MemoryStream memStream = new MemoryStream(message);
        BinaryReader reader = new BinaryReader(memStream);
            
        MessageType temp = (MessageType)reader.ReadInt32();

        switch (temp)
        {
            case MessageType.HandShake:
                NetHandShake nethandShake = new NetHandShake();
                nethandShake.Deserialize(message);
                if (nethandShake.IsClientIdAssigned())
                {
                    NetworkManager.Instance.clientId = nethandShake.GetData();
                    Debug.Log("client ID = " + NetworkManager.Instance.clientId);
                }
                break;
            case MessageType.Console:
                NetConsole con = new NetConsole(message);
                NetConsole.OnDispatch?.Invoke(con.GetData());
                break;
            case MessageType.Position:
                //NetVector3.OnDispatch?.Invoke();
                break;
        }
    }

    public void HandleServerMessage(byte[] data, IPEndPoint ip)
    {
        MemoryStream memStream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(memStream);
            
        MessageType temp = (MessageType)reader.ReadInt32();

        switch (temp)
        {
            case MessageType.HandShake:
                NetHandShake nethandShake = new NetHandShake();
                nethandShake.SetClientID(NetworkManager.Instance.AddClient(ip));
                NetworkManager.Instance.SendToClient(nethandShake.Serialize(), ip);
                break;
            case MessageType.Console:
                NetConsole netConsole = new NetConsole(data);
                NetworkManager.Instance.Broadcast(netConsole.Serialize());
                ChatScreen.Instance.ReceiveConsoleMessage(netConsole.GetData());
                break;
            case MessageType.Position:
                break;
        }
            
        
    }

}
