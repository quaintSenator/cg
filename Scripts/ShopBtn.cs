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
        //ClientNetwork��Ҫinspector������
    }
    public void OnShopBtnClick()
    {
        shopPanel.gameObject.SetActive(true);
        shopPanel.itemListFromNetwork = new List<TCCamp.Item>();
        if (!ClientNetwork) return;
        if (EventModule.Instance.myPlayerID == "")
        {
            return;//��δ��¼

        }
        else
        {
            _getShopContentReq.PlayerID = EventModule.Instance.myPlayerID;
            ClientNetwork.SendMsg((int)TCCamp.CLIENT_CMD.ClientGetShopContentReq, _getShopContentReq);
        }

    }
}
