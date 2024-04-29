using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;
    public string name;

    private List<Player> players = new List<Player>();

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.name = "";

        NetConsole.OnDispatch += DispatchNetConsole;
        NetVector3.OnDispatch += DispatchNetVec3;
        NetHandShake.OnDispatch += DispatchNetHandShake;
        NetServerToClientHS.OnDispatch += DispatchNetServerToClient;
    }

    private void DispatchNetConsole(string consoleMessage)
    {
        ChatScreen.Instance.ReceiveConsoleMessage(consoleMessage);
    }

    private void DispatchNetVec3(Vector3 vec3)
    {
        throw new NotImplementedException();
    }

    private void DispatchNetHandShake(int handshake)
    {
        id = handshake;
    }

    private void DispatchNetServerToClient(List<Player> players)
    {
        this.players = players;
    }

}
