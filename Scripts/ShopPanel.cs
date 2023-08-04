using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TCCamp;
using Google.Protobuf;

[Serializable]
public class ItemInfo
{
    public string name;
    public string introduce;
    public int price;
    public string iconName;
    public int itemID;
}

[Serializable]
public class ItemInfos
{
    public List<ItemInfo> items;
}

public class ItemPrefab
{
    public GameObject go;
    public Image itemImage;
}

public class ShopPanel : MonoBehaviour
{
    #region members
    public Button buyButton;

    public Text moneyNumText;

    public GameObject itemPrefab;

    public GameObject itemContent;

    public GameObject itemInfoPanel;

    public Text itemInfoText;

    public Image itemInfoImage;

    public MsgBox msgBox;

    public Button closeBtn;

    public Network ClientNetwork;

    private Dictionary<string, Sprite> itemSpriteDict;

    private int moneyNum;

    private ItemInfo currentSelect;//当前选中的物品
    private BuyItemReq _buyItemReq;
    public List<Item> itemListFromNetwork;
    #endregion
    private void Start()
    {
        InitItemAtlas();
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.ShopContentRsp, OnShopContentRsp);
        _buyItemReq = new BuyItemReq();
    }

    private void OnEnable()
    {
        moneyNum = 10000;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClick);
        
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(delegate { gameObject.SetActive(false); });
        moneyNumText.text = moneyNum.ToString();

        currentSelect = null;
        itemInfoPanel.gameObject.SetActive(false);
    }

    private void OnBuyClick()
    {
        if (currentSelect == null)
        {
            Debug.Log("请选中需要购买的物品");
            return;
        }

        if (currentSelect.price > moneyNum)
        {
            Debug.Log("钱不够");
            return;
        }
        
        msgBox.gameObject.SetActive(true);
        string msg = $"是否花费{currentSelect.price}金币购买{currentSelect.name}";
        msgBox.SetData(msg,delegate { 
            Debug.Log("购买成功"); 
            moneyNum -= currentSelect.price;
            moneyNumText.text = moneyNum.ToString();
            currentSelect = null;
            itemInfoPanel.gameObject.SetActive(false);
            SendBuyItemRequest(currentSelect.itemID);
        });
    }
    private void SendBuyItemRequest(int itemID)
    {
        _buyItemReq.ItemID = itemID;
        ClientNetwork.SendMsg((int)TCCamp.CLIENT_CMD.ClientBuyItemReq, _buyItemReq);
    }
    private void InitItemListFromLocal()
    {
        TextAsset itemConfig = Resources.Load<TextAsset>("ItemConfig");
        if (itemConfig != null)
        {
            ItemInfos itemInfos = JsonUtility.FromJson<ItemInfos>(itemConfig.text);
            foreach (var itemInfo in itemInfos.items)
            {
                ItemPrefab item = new ItemPrefab();
                item.go = Instantiate<GameObject>(itemPrefab,itemContent.transform);
                Sprite sp;
                itemSpriteDict.TryGetValue(itemInfo.iconName, out sp);
                item.itemImage = item.go.transform.Find("ItemImage").GetComponent<Image>();
                item.itemImage.sprite = sp;
                item.go.SetActive(true);
                item.go.transform.GetComponent<Button>().onClick.AddListener(delegate
                {
                    currentSelect = itemInfo;
                    itemInfoPanel.gameObject.SetActive(true);
                    itemInfoText.text = itemInfo.introduce;
                    itemInfoImage.sprite = sp;
                });
            }
        }
    }
    public void loadLocalIConfigandCompare()
    {
        if (itemListFromNetwork.Count < 1)
        {
            Debug.Log("No network data found!");
            return;
        }
        TextAsset itemConfig = Resources.Load<TextAsset>("ItemConfig");
        if (itemConfig != null)
        {
            ItemInfos itemInfos = JsonUtility.FromJson<ItemInfos>(itemConfig.text);
            int contentChildCount = itemContent.transform.childCount;
            Debug.Log("content childcount = " + itemContent.transform.childCount);
            
            if(contentChildCount < 1)
            {
                foreach (var networkItem in itemListFromNetwork)
                {
                    foreach (var localItem in itemInfos.items)
                    {
                        if (networkItem.ItemID == localItem.itemID)
                        {
                            ItemPrefab item = new ItemPrefab();
                            item.go = Instantiate<GameObject>(itemPrefab, itemContent.transform);
                            Sprite sp;
                            itemSpriteDict.TryGetValue(localItem.iconName, out sp);
                            item.itemImage = item.go.transform.Find("ItemImage").GetComponent<Image>();
                            item.itemImage.sprite = sp;
                            item.go.SetActive(true);
                            item.go.transform.GetComponent<Button>().onClick.AddListener(delegate
                            {
                                currentSelect = localItem;
                                itemInfoPanel.gameObject.SetActive(true);
                                itemInfoText.text = localItem.introduce;
                                itemInfoImage.sprite = sp;
                            });
                        }
                    }
                }
            }
            
        }
        else
        {
            Debug.Log("itemConfig == null!");
        }
        Debug.Log("loadLocalItemConfig out");
    }


    private void InitItemAtlas()
    {
        UnityEngine.Object[] objs = Resources.LoadAll("itemIcon");
        itemSpriteDict = new Dictionary<string, Sprite>();
        foreach (var obj in objs)
        {
            if (obj.GetType() == typeof(Sprite))
            {
                if(!itemSpriteDict.ContainsKey(obj.name))
                    itemSpriteDict.Add(obj.name,(Sprite)obj);
            }
            else
            {
                Debug.Log($"转换失败{obj.name},{obj.GetType().Name}");
            }
            
        }
    }

    public void OnShopContentRsp(int cmd, IMessage msg)
    {
        ShopContentRsp rsp = msg as ShopContentRsp;
        if(rsp.GetResult == (int)PROTO_RESULT_CODE.ServerGetShopContentSuccess)
        {
            this.gameObject.SetActive(true);
            Debug.Log("ShopContent Fetch success, items are:");
            Debug.Log(rsp.Items.Count);
            foreach (var item in rsp.Items)
            {
                Item generatedNetworkItem = new Item();
                generatedNetworkItem.HeapSize = item.HeapSize;
                generatedNetworkItem.ItemID = item.ItemID;
                generatedNetworkItem.Price = item.Price;
                itemListFromNetwork.Add(generatedNetworkItem);
            }
            Debug.Log("Foreach out");
            loadLocalIConfigandCompare();
        }
        //只有收到了回报，才能把面板显示出来
        else
        {
            Debug.Log("error, ShopContent Fetch error");
        }

    }

}