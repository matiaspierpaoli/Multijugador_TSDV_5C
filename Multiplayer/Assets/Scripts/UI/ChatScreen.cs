using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);

        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
    }

    void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(data);
        }
        
        //messages.text += System.Text.ASCIIEncoding.UTF8.GetString(data) + System.Environment.NewLine;

        if (data.Length < sizeof(int))
        {
            Debug.LogError("Received data is too short to determine message type.");
            return;
        }


        //long newData = BitConverter.ToInt64(data, 0);

        int messageType = BitConverter.ToInt32(data, 0);

        switch ((MessageType)messageType)
        {
            case MessageType.HandShake:
                break;
            case MessageType.Console:
                NetConsole netConsoleMessage = new NetConsole(data);
                messages.text += netConsoleMessage.consolemessage + System.Environment.NewLine;
                break;
            case MessageType.Position:
                break;
            default:
                Debug.LogError("Unknown message type received: " + messageType);
                break;
        }
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            NetConsole netConsoleMessage = new NetConsole(inputMessage.text);

            if (NetworkManager.Instance.isServer)
            {
                NetworkManager.Instance.Broadcast(netConsoleMessage.Serialize());
                messages.text += inputMessage.text + System.Environment.NewLine;
            }
            else
            {
                NetworkManager.Instance.SendToServer(netConsoleMessage.Serialize());

                //NetworkManager.Instance.SendToServer(System.Text.ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }

    }

}
