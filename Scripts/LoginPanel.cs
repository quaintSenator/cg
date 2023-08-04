using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using TCCamp;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour {

    public Text       Tip;
    public InputField InputName;
    public InputField InputPWD;
    public Button     BtnLogin;
    public Button     BtnRegister;
    public Button     BtnChangeToRegister;
    
    public GameObject InputCompContent;
    public GameObject WaitLoginComp;

    [Space(10)] public Network ClientNetwork;

    private bool m_isRegisterMode = false;
    
    void Start() {
        Tip.text = "";
        Tip.gameObject.SetActive(true);
        ChangeLoginViewMode(false);
        BtnLogin.onClick.AddListener(OnBtnLoginClick);
        BtnRegister.onClick.AddListener(OnBtnRegisterClick);
    }

    private void OnEnable() {
        EventModule.Instance.RemoveNetEvent((int)SERVER_CMD.ServerLoginRsp, OnLoginRsp);
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.ServerLoginRsp, OnLoginRsp);
        EventModule.Instance.RemoveNetEvent((int)SERVER_CMD.ServerCreateRsp, OnCreateRsp);
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.ServerCreateRsp, OnCreateRsp);
    }

    private void OnDisable() {
        EventModule.Instance.RemoveNetEvent((int)SERVER_CMD.ServerLoginRsp,  OnLoginRsp);
        EventModule.Instance.RemoveNetEvent((int)SERVER_CMD.ServerCreateRsp, OnCreateRsp);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnBtnRegisterClick() {
        if (!ClientNetwork) return;
        
        string playerID  = InputName.text;
        string playerPWD = InputPWD.text;

        if (string.IsNullOrWhiteSpace(playerID) || string.IsNullOrWhiteSpace(playerPWD)) {
            Debug.LogError("Player Account Or Player Password is error!");
            return;
        }
        
        PlayerCreateReq createReq = new PlayerCreateReq();
        createReq.PlayerID = InputName.text;
        createReq.Password = InputPWD.text;
        ClientNetwork.SendMsg((int)CLIENT_CMD.ClientCreateReq, createReq);
        
        ChangeLoginViewMode(true);
    }
    
    private void OnBtnLoginClick() {
        if (!ClientNetwork) return;

        string playerID = InputName.text;
        string playerPWD = InputPWD.text;

        if (string.IsNullOrWhiteSpace(playerID) || string.IsNullOrWhiteSpace(playerPWD)) {
            Debug.LogError("Player Account Or Player Password is error!");
            return;
        }
        EventModule.Instance.myPlayerID = playerID;
        PlayerLoginReq loginReq = new PlayerLoginReq();
        loginReq.PlayerID = InputName.text;
        loginReq.Password = InputPWD.text;
        ClientNetwork.SendMsg((int)CLIENT_CMD.ClientLoginReq, loginReq);
        
        ChangeLoginViewMode(true);
    }

    private void OnLoginRsp(int cmd, IMessage msg) {
        PlayerLoginRsp rsp = msg as PlayerLoginRsp;
        switch (rsp.Result) {
            case 0:
                Debug.Log("Login success！");
                LoginSuccess();
                break;
            case -1:
                ChangeLoginViewMode(false);
                Tip.text = "Account is run in server! Login failed!";
                break;
            case -2:
                ChangeLoginViewMode(false);
                Tip.text = "Account is not exist!";
                break;
            case -3:
                ChangeLoginViewMode(false);
                Tip.text = "Account or password error!";
                break;
        }
    }
    
    private void OnCreateRsp(int cmd, IMessage msg) {
        PlayerCreateRsp rsp = msg as PlayerCreateRsp;
        switch (rsp.Result) {
            case 0:
                Debug.Log("Register success! And Login success！");
                LoginSuccess();
                break;
            default:
                ChangeLoginViewMode(false);
                Tip.text = "Register failed!";
                break;
        }
    }

    private void LoginSuccess() {
        this.gameObject.SetActive(false);
        Player.Instance.IsLogin = true;
        
    }

    private void ChangeLoginViewMode(bool isWaiting) {
        InputCompContent.SetActive(!isWaiting);
        WaitLoginComp.SetActive(isWaiting);
    }
}
