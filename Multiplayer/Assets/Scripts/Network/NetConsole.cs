using System;
using System.Collections.Generic;
using System.Text;

public class NetConsole : Message<string>
{
    public string consolemessage;

    public NetConsole(byte[] data)
    {
        consolemessage = Deserialize(data);
    }

    public NetConsole(string consoleMessage)
    { 
        this.consolemessage = consoleMessage;
    }

    public override string Deserialize(byte[] message) 
    {
        int stringlenght = BitConverter.ToInt32(message, 4);
        return Encoding.UTF8.GetString(message, 8, stringlenght);
    }

    public override string GetData()
    {
        return consolemessage;
    }

    public override MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public override byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(consolemessage.Length));
        outData.AddRange(Encoding.UTF8.GetBytes(consolemessage));

        return outData.ToArray();
    }
}
