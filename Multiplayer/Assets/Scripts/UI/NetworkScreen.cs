﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;
    public InputField nameInputField;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        int port = System.Convert.ToInt32(portInputField.text);
        NetworkManager.Instance.tagName = nameInputField.text;
        NetworkManager.Instance.InitializeClient(ipAddress, port);
        
        SwitchToChatScreen();
    }

    void OnStartServerBtnClick()
    {
        int port = System.Convert.ToInt32(portInputField.text);
        NetworkManager.Instance.InitializedServer(port);
        SwitchToChatScreen();
    }

    void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
