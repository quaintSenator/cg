using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TCCamp;
using Google.Protobuf;

public class BackpackPanel : MonoBehaviour
{
    public Button     closeBtn;
    public GameObject itemPrefab;
    public GameObject itemContent;
    public List<Item> itemListFromNetwork;
    private Dictionary<string, Sprite> itemSpriteDict;

    private ItemInfo currentSelect;
    public GameObject itemInfoPanel;
    public Text itemInfoText;
    public Image itemInfoImage;


    private void Start()
    {
        InitItemAtlas();
        //InitItemList();
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.BackpackContentRsp, OnBackpackContentRsp);
    }
    
    private void OnEnable() {
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(delegate { gameObject.SetActive(false); });
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
                    itemSpriteDict.Add(obj.name, (Sprite)obj);
            }
            else
            {
                Debug.Log($"转换失败{obj.name},{obj.GetType().Name}");
            }
            
        }
    }
    
    private void InitItemList()
    {
        TextAsset itemConfig = Resources.Load<TextAsset>("ItemConfig");
        if (itemConfig != null)
        {
            ItemInfos itemInfos = JsonUtility.FromJson<ItemInfos>(itemConfig.text);
            foreach (var itemInfo in itemInfos.items)
            {
                ItemPrefab item = new ItemPrefab();
                item.go = Instantiate<GameObject>(itemPrefab, itemContent.transform);
                Sprite sp;
                itemSpriteDict.TryGetValue(itemInfo.iconName, out sp);
                item.itemImage        = item.go.transform.Find("ItemImage").GetComponent<Image>();
                item.itemImage.sprite = sp;
                item.go.SetActive(true);
                item.go.transform.GetComponent<Button>().onClick.AddListener(delegate
                {
                    //currentSelect = itemInfo;
                    //itemInfoPanel.gameObject.SetActive(true);
                    //itemInfoText.text    = itemInfo.introduce;
                    //itemInfoImage.sprite = sp;
                });
            }
        }
    }

    public void OnBackpackContentRsp(int cmd, IMessage msg)
    {
        BackPackContentRsp rsp = msg as BackPackContentRsp;
        if (rsp.GetResult == (int)PROTO_RESULT_CODE.ServerGetBackpackContentSuccess)
        {
            this.gameObject.SetActive(true);
            Debug.Log(rsp.Items[0].ItemID);


            foreach (var item in rsp.Items)
            {
                Item generatedNetworkItem = new Item();
                generatedNetworkItem.HeapSize = item.HeapSize;
                generatedNetworkItem.ItemID = item.ItemID;
                Debug.Log("read from respond,itemID = " + item.ItemID);
                //这里的rsp.Items的内容是脏的
                generatedNetworkItem.Price = item.Price;
                itemListFromNetwork.Add(generatedNetworkItem);
            }
            Debug.Log("Foreach out");
            loadLocalIConfigandCompare();
        }
        //只有收到了回报，才能把面板显示出来
        else
        {
            Debug.Log("error, Backpack Content Fetch error");
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
            
            Debug.Log("itemListFromNetwork.Count = " + itemListFromNetwork.Count);
            Debug.Log("Local Count = " + itemInfos.items.Count);
            if (contentChildCount < 1)
            {
                foreach (var networkItem in itemListFromNetwork)
                {
                    foreach (var localItem in itemInfos.items)
                    {
                        Debug.Log("localItemid = " + localItem.itemID + ", networkItemid = " + networkItem.ItemID);
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
                Debug.Log("content childcount = " + itemContent.transform.childCount);
            }

        }
        else
        {
            Debug.Log("itemConfig == null!");
        }
        Debug.Log("loadLocalItemConfig out");
    }
    
}
