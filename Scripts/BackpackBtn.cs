using UnityEngine;
using System.Collections.Generic;
using TCCamp;
public class BackpackBtn : MonoBehaviour
{
    public BackpackPanel backpackPanel;
    private GetBackPackContentReq _backpackReq;
    public Network ClientNetwork;
    private void Start()
    {
        _backpackReq = new GetBackPackContentReq();
    }
    public void OnBackpackBtnClick()
    {
        backpackPanel.gameObject.SetActive(true);
        backpackPanel.itemListFromNetwork = new List<Item>();
        if (!ClientNetwork) return;

        if (EventModule.Instance.myPlayerID == "")
        {
            Debug.Log("Please login first");
            return;//ÉÐÎ´µÇÂ¼
        }
        else
        {
            _backpackReq.PlayerID = EventModule.Instance.myPlayerID;
            ClientNetwork.SendMsg((int)TCCamp.CLIENT_CMD.ClientGetBackpackContentReq, _backpackReq);
        }
        /*
message GetBackPackContentReq 
string PlayerID = 1;

message BackPackContentRsp
int32 GetResult = 1;
repeated Item items = 2;
 */



    }
}
