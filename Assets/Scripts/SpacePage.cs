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
        public Image[] itemIcons;
        public Text[] itemDescriptions;
        public Button searchBtn;

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
                CloseSearchPanel();
                CloseShopPanel();
            }
            else searchBtn.enabled = true;                        
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
                case 0: player.myShip.AddAmmo(0, 4); break;
                case 1: player.myShip.AddAmmo(1, 2); break;
                case 2: player.myShip.AddAmmo(2, 1); break;
                case 3: player.myShip.RestoreShield(4); break;
                case 4: player.myShip.ShieldUp(); break;
                case 5: player.myShip.SpeedUp(); break;
                case 6: player.myShip.WeaponChangeSpeedUp(); break;
                case 7: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WS1)); break;
                case 8: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WS2)); break;
                case 9: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WS3)); break;
                case 10: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WN1)); break;
                case 11: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WN2)); break;
                case 12: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WN3)); break;
                case 13: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WP1)); break;
                case 14: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WP2)); break;
                case 15: player.myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.WP3)); break;
                case 16: player.AddMoney(10); break;
                case 17: player.AddMoney(40); break;
            }

            
        }

        public void OpenShopPanel()
        {
            shopPanel.SetActive(true);
            for (int i = 0; i < 10; i++)
            {
                shopItemIcons[i].sprite = itemInfos[searchBox.ids[i]].image;
                shopItemDescriptions[i].text = itemInfos[searchBox.ids[i]].desc +  "\n 가격[" + itemInfos[searchBox.ids[i]].price + "]";
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
            searchPanel.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
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