using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class NetConsole : MonoBehaviour
{
    public string consolemessage;

    public NetConsole()
    {
    }

    public NetConsole(string consoleMessage)
    { 
        this.consolemessage = consoleMessage;
    }

    public string Deserialize(byte[] message)
    {
        string outData;

        outData = BitConverter.ToString(message, 4);

        return outData;
    }

    public MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(Encoding.ASCII.GetBytes(consolemessage));

        return outData.ToArray();
    }
}
