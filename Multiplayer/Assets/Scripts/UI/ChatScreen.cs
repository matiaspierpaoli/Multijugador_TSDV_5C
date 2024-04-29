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

    public void ReceiveConsoleMessage(string consoleMessage)
    {
        messages.text += consoleMessage + System.Environment.NewLine;
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
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }
    }
}
