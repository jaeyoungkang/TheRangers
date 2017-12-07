using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    [System.Serializable]
    public class ItemInfo
    {
        public int id;
        public int grade;
        public Sprite image;
        public string desc;
        public int price;
    }
    public class SpacePage : MonoBehaviour
    {
        public GameObject shopPanel;
        public GameObject searchPanel;
        public Button closeShopBtn;
        public Button[] getItemBtns;
        public Image[] itemBgs;
        public Image[] itemIcons;
        public Text[] itemDescriptions;
        public Button searchBtn;

        public GameObject[] shopItemPanels;
        public Button[] shopBuyItemBtns;
        public Image[] shopItemIcons;
        public Text[] shopItemDescriptions;
        public Text moneyText;
        

        public GameObject result;
        public Button backBtn;
        public Text resultMsg;
        public Text resultTitle;

        public ItemInfo[] itemInfos;

        void Start()
        {
            closeShopBtn.onClick.AddListener(CloseShopPanel);
            backBtn.onClick.AddListener(GameManager.instance.Restart);
            searchBtn.onClick.AddListener(OpenSearchPanel);
            getItemBtns[0].onClick.AddListener(() => GetItem(0));
            getItemBtns[1].onClick.AddListener(() => GetItem(1));
            getItemBtns[2].onClick.AddListener(() => GetItem(2));

            shopBuyItemBtns[0].onClick.AddListener(() => BuyItem(0));
            shopBuyItemBtns[1].onClick.AddListener(() => BuyItem(1));
            shopBuyItemBtns[2].onClick.AddListener(() => BuyItem(2));
            shopBuyItemBtns[3].onClick.AddListener(() => BuyItem(3));
            shopBuyItemBtns[4].onClick.AddListener(() => BuyItem(4));
            shopBuyItemBtns[5].onClick.AddListener(() => BuyItem(5));
            shopBuyItemBtns[6].onClick.AddListener(() => BuyItem(6));
            shopBuyItemBtns[7].onClick.AddListener(() => BuyItem(7));
            shopBuyItemBtns[8].onClick.AddListener(() => BuyItem(8));
            shopBuyItemBtns[9].onClick.AddListener(() => BuyItem(9));
        }

        void BuyItem(int index)
        {
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            int price = itemInfos[searchBox.ids[index]].price;
            if (player.money >= price)
            {
                ActivateItem(index);
                player.money -= price;
                OpenShopPanel();
            }
            else
            {
                GameManager.instance.UpdateGameMssage("돈이 없다!", 2f);
            }            
        }

        DropInfo searchBox;

        public void ActivateSearchBtn(Vector3 pos)
        {
            searchBox = GameManager.instance.GetDropBox(pos);
            if (searchBox == null)
            {
                searchBtn.enabled = false;
                searchBtn.GetComponent<Image>().color = Color.white;
                CloseSearchPanel();
                CloseShopPanel();
            }
            else
            {
                searchBtn.enabled = true;
                searchBtn.GetComponent<Image>().color = Color.green;
            }
        }

        void OnEnable()
        {
            searchBtn.enabled = false;
            result.SetActive(false);
            CloseSearchPanel();
        }

        public void GetItem(int index)
        {
            ActivateItem(index);
            GameManager.instance.ReomveDropItem(searchBox);
            CloseSearchPanel();
        }

        public void ActivateItem(int index)
        {
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            switch (searchBox.ids[index])
            {
                case 0: player.myShip.AddPower(10); break;
                case 1: player.myShip.RestoreShield(4); break;
                case 2: player.ShieldUp(); break;
                case 3: player.myShip.SpeedUp(); break;
                case 4: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W1)); break;
                case 5: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W2)); break;
                case 6: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W3)); break;
                case 7: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W4)); break;
                case 8: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W5)); break;
                case 9: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W6)); break;
                case 14: player.myShip.AddPower(15); break;
                case 15: player.myShip.RestoreShield(6); break;
                case 16: player.myShip.AddPower(20); break;
                case 17: player.myShip.RestoreShield(8); break;
            }            
        }

        public void OpenShopPanel()
        {
            Color blue = new Color(0, 0f, 0.8f);
            Color green = new Color(0, 0.8f, 0.0f);
            Color gold = new Color(0.9f, 0.9f, 0.0f);

            shopPanel.SetActive(true);
            for (int i = 0; i < 10; i++)
            {
                ItemInfo item = itemInfos[searchBox.ids[i]];

                if(item.id == 2)
                {
                    item = GetSheildUpgradeItem();
                }
                if (item.id == 3)
                {
                    item = GetSpeedUpgradeItem();
                }

                Color panelColor = Color.white;
                if(item.grade == 1) panelColor = green;
                else if (item.grade == 2) panelColor = blue;
                else if (item.grade == 3) panelColor = gold;
                shopItemPanels[i].GetComponent<Image>().color = panelColor;
                shopItemIcons[i].sprite = item.image;
                shopItemDescriptions[i].text = item.desc +  "\n 가격[" + item.price + "]";
            }
        }

        public ItemInfo GetSheildUpgradeItem()
        {
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            switch(player.myShip.shieldLevel)
            {
                case 0: return itemInfos[2];
                case 1: return itemInfos[10];
                case 2: return itemInfos[11];
                case 3: return itemInfos[12];
                default:
                    return itemInfos[13];
            }
        }

        public ItemInfo GetSpeedUpgradeItem()
        {
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            switch (player.myShip.speedLevel)
            {
                case 4: return itemInfos[3];
                case 5: return itemInfos[18];
                case 6: return itemInfos[19];
                default:
                    return itemInfos[3];
            }
        }

        public void CloseShopPanel()
        {
            shopPanel.SetActive(false);
        }

        public void CloseSearchPanel()
        {
            searchPanel.SetActive(false);
        }

        public void OpenDropItemPanel()
        {
            Color blue = new Color(0, 0f, 0.8f);
            Color green = new Color(0, 0.8f, 0.0f);
            Color gold = new Color(0.9f, 0.9f, 0.0f);

            searchPanel.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
                Color panelColor = Color.white;
                if (itemInfos[searchBox.ids[i]].grade == 1) panelColor = green;
                else if (itemInfos[searchBox.ids[i]].grade == 2) panelColor = blue;
                else if (itemInfos[searchBox.ids[i]].grade == 3) panelColor = gold;
                itemBgs[i].GetComponent<Image>().color = panelColor;                
                itemIcons[i].sprite = itemInfos[searchBox.ids[i]].image;
                itemDescriptions[i].text = itemInfos[searchBox.ids[i]].desc;
            }
        }

        public void OpenSearchPanel()
        {
            if (searchBox.shop)
            {
                OpenShopPanel();
            }
            else
            {
                OpenDropItemPanel();
            }            
        }

        public void ShowResult(string titleMsg, string msg, string btnString)
        {
            backBtn.GetComponentInChildren<Text>().text = btnString;
            resultTitle.text = titleMsg;
            resultMsg.text = msg;
            result.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            moneyText.text = "MONEY : " + GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().money;            
        }
    }
}