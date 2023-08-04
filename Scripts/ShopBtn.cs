using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopBtn : MonoBehaviour
{
    public ShopPanel shopPanel;
    public Network ClientNetwork;
    private TCCamp.GetShopContentReq _getShopContentReq;
    public void Start()
    {
        _getShopContentReq = new TCCamp.GetShopContentReq();
        //ClientNetwork需要inspector传引用
    }
    public void OnShopBtnClick()
    {
        shopPanel.gameObject.SetActive(true);
        shopPanel.itemListFromNetwork = new List<TCCamp.Item>();
        if (!ClientNetwork) return;
        if (EventModule.Instance.myPlayerID == "")
        {
            return;//尚未登录

        }
        else
        {
            _getShopContentReq.PlayerID = EventModule.Instance.myPlayerID;
            ClientNetwork.SendMsg((int)TCCamp.CLIENT_CMD.ClientGetShopContentReq, _getShopContentReq);
        }

    }
}
