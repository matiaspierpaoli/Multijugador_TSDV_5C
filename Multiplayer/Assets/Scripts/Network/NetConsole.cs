using System;
using System.Collections.Generic;
using System.Text;

public class NetConsole : IMessage<string>
{
    public string consolemessage;

    public NetConsole(byte[] data)
    {
        Deserialize(data);
    }

    public NetConsole(string consoleMessage)
    { 
        this.consolemessage = consoleMessage;
    }

    private void Deserialize(byte[] message) 
    {
        //consolemessage = System.Text.Encoding.UTF8.GetString(message);

        char[] charArray = new char[message.Length - 4];

        for (int i = 0; i < message.Length - 4; i++)
        {
            charArray[i] = (char)message[i + 4];
        }

        consolemessage = new string(charArray);
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

    string IMessage<string>.Deserialize(byte[] message)
    {
        throw new NotImplementedException();
    }
}
