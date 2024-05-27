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
    }

    public void ReceiveConsoleMessage(string consoleMessage, int id)
    {
        if (id != NetworkManager.Instance.serverId)
            messages.text += NetworkManager.Instance.GetPlayer(id).playerName + ": " + consoleMessage + System.Environment.NewLine;
        else
            messages.text += "Server" + ": " + consoleMessage + System.Environment.NewLine;
    }

    void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            NetConsole netConsoleMessage = new NetConsole(inputMessage.text);

            if (NetworkManager.Instance.isServer)
            {
                ReceiveConsoleMessage(inputMessage.text, NetworkManager.Instance.serverId);
                NetworkManager.Instance.Broadcast(netConsoleMessage.Serialize());
            }
            else
            {
                NetworkManager.Instance.SendToServer(netConsoleMessage.Serialize());
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }
    }
}
