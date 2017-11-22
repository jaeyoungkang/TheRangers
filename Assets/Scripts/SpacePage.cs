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
    }
    public class SpacePage : MonoBehaviour
    {
        public GameObject searchPanel;
        public Button[] getItemBtns;
        public Image[] itemIcons;
        public Text[] itemDescriptions;
        public Button searchBtn;

        public GameObject result;
        public Button backBtn;
        public Text resultMsg;
        
        public ItemInfo[] itemInfos = new ItemInfo[6];
        public ItemInfo[] curItems = new ItemInfo[3];

        void Start()
        {            
            backBtn.onClick.AddListener(GameManager.instance.Restart);
            searchBtn.onClick.AddListener(OpenSearchPanel);
            getItemBtns[0].onClick.AddListener(() => GetItem(0));
            getItemBtns[1].onClick.AddListener(() => GetItem(1));
            getItemBtns[2].onClick.AddListener(() => GetItem(2));            
        }

        GameObject searchBox;

        public void ActivateSearchBtn(Vector3 pos)
        {
            searchBox = GameManager.instance.GetDropBox(pos);
            if (searchBox == null)
            {
                searchBtn.enabled = false;
                CloseSearchPanel();
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
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            int itemId = curItems[index].id;
            switch (itemId)
            {
                case 0: player.myShip.AddAmmo(0, 8); break;
                case 1: player.myShip.AddAmmo(1, 4); break;
                case 2: player.myShip.AddAmmo(2, 2); break;
                case 3: player.myShip.RestoreShield(); break;
                case 4: player.myShip.AddShield(1); break;
                case 5: player.myShip.SpeedUp(0.01f); break;
            }

            GameManager.instance.ReomveDropItem(searchBox);
            CloseSearchPanel();
        }

        public void CloseSearchPanel()
        {
            searchPanel.SetActive(false);
        }

        public void OpenSearchPanel()
        {
            searchPanel.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
                int rValue = Random.Range(0, itemInfos.Length);
                curItems[i] = itemInfos[rValue];
                itemIcons[i].sprite = curItems[i].image;
                itemDescriptions[i].text = curItems[i].desc;
            }
        }

        public void ShowResult(string msg)
        {
            resultMsg.text = msg;
            result.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}