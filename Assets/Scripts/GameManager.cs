using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Completed
{
    public enum PAGE { FRONT, GATEWAY, SPACE, READY };
    public class DropInfo
    {
        public bool shop = false;
        public GameObject obj;
        public int[] ids;
    }

    public class GameManager : MonoBehaviour
	{
        public int tryCount = 3;

        public static GameManager instance = null;
        public Level curLevel = new Level();

        public GameObject gateWay;
        private Text gatewaySubText;
        private Text tryText;

        private Text readyDescribeText;

        private Button readyBtn;
        private Button adBtn;
        private Button purchaseBtn;

        private Button purchase10Button;
        private Button purchase30Button;
        private Button purchase50Button;        
        private Button closePurchaseButton;
        private GameObject purchasePanel;

        private Text universeText;
        public Text gameMessage;
        public float msgTimer = 0;

        private BoardManager boardScript;

        public bool doingSetup = true;

        public GameObject lootBox;
        public GameObject shopBox;

        public List<DropInfo> dropItems = new List<DropInfo>();

        public int numberOfUniverse = 1;
        public int numberOfUniverseInit = 1;
        public int numberOfUniverseEnd = 3;

        public int money;
        public SpaceShip myShip;
        public Weapon myWeapon;

        public int[] GenerateDropItemIds(int type)
        {
            int[] dropIds = new int[3];
            List<int> ItemSetA1 = new List<int> { 0, 0, 0, 0, 0 };
            List<int> ItemSetA2 = new List<int> { 1, 1, 1, 1, 1 };
            List<int> ItemSetA3 = new List<int> { 4, 4, 4, 4, 5, 5, 5, 5 };

            switch (type)
            {
                case 0:
                    ItemSetA1.Add(14);
                    ItemSetA3.Add(4); ItemSetA3.Add(6);
                    break;

                case 1:
                    ItemSetA2.Add(15);
                    ItemSetA3.Add(5); ItemSetA3.Add(7);
                    break;

                case 2:
                    ItemSetA2.Add(15); ItemSetA2.Add(15); ItemSetA2.Add(17);
                    ItemSetA3.Add(7); ItemSetA3.Add(7); ItemSetA3.Add(9);
                    break;

                case 3:
                    ItemSetA1.Add(14); ItemSetA1.Add(14); ItemSetA1.Add(16);
                    ItemSetA3.Add(6); ItemSetA3.Add(6); ItemSetA3.Add(8);
                    break;
            }

            dropIds[0] = ItemSetA1[Random.Range(0, ItemSetA1.Count)];
            dropIds[1] = ItemSetA2[Random.Range(0, ItemSetA2.Count)];
            dropIds[2] = ItemSetA3[Random.Range(0, ItemSetA3.Count)];

            return dropIds;
        }

        public void LayoutShop(Vector3 pos)
        {
            GameObject item = Instantiate(shopBox, pos, Quaternion.identity);
            GameManager.instance.curLevel.SetMapOfItems(pos, 1);
            DropInfo dInfo = new DropInfo();
            dInfo.shop = true;
            dInfo.obj = item;
            if(GameManager.instance.curLevel.id <= 3)
            {
                dInfo.ids = new int[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
            else if(GameManager.instance.curLevel.id == 4)
            {
                dInfo.ids = new int[10] { 14, 15, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
            else if (GameManager.instance.curLevel.id == 5)
            {
                dInfo.ids = new int[10] { 16, 17, 2, 3, 4, 5, 6, 7, 8, 9 };
            }

            dropItems.Add(dInfo);
        }

        public void DropItem(Vector3 dropPos, int type)
        {
            GameObject item = Instantiate(lootBox, dropPos, Quaternion.identity);
            GameManager.instance.curLevel.SetMapOfItems(dropPos, 1);
            DropInfo dInfo = new DropInfo();
            dInfo.obj = item;
            dInfo.ids = GenerateDropItemIds(type);

            dropItems.Add(dInfo);
        }
        

        public DropInfo GetDropBox(Vector3 pos)
        {
            for (int i=1; i<dropItems.Count; i++)
            {
                DropInfo dInfo = dropItems[i];
                if (dInfo.obj.transform.position == pos) return dInfo;
            }

            if(dropItems[0].obj.transform.position == pos) return dropItems[0];

            return null;
        }

        public void ReomveDropItem(DropInfo item)
        {            
            dropItems.Remove(item);

            bool anotherItem = false;
            foreach(DropInfo dInfo in dropItems)
            {
                if (dInfo.obj.transform.position == item.obj.transform.position) anotherItem = true;
            }
            if(anotherItem == false)
                GameManager.instance.curLevel.SetMapOfItems(item.obj.transform.position, 0);

            item.obj.SetActive(false);
        }

        public void UpdateGameMssage(string msg, float time)
		{
            gameMessage.gameObject.SetActive(true);

            gameMessage.text = msg;
			msgTimer = time;
		}

        public void DestroyOtherPlayer(GameObject target, int type)
        {
            StartCoroutine(EffectManager.instance.ShowExplosionEffect(target.transform.position));

            curLevel.RemoveOtherPlayer(target);

            DropItem(target.transform.position, type);
        }
                

        public List<Vector3> GetShowRange(Vector3 playerPos, MOVE_DIR dir, int range)
        {
            Vector3 pos = playerPos;
            switch (dir)
            {
                case MOVE_DIR.LEFT: pos.x -= range; break;
                case MOVE_DIR.RIGHT: pos.x += range; break;
                case MOVE_DIR.UP: pos.y += range; break;
                case MOVE_DIR.DOWN: pos.y -= range; break;
            }

            List<Vector3> resultRange = new List<Vector3> { pos, };
            for (int i = 0; i < range; i++)
            {
                resultRange = MakeRange(resultRange, dir, playerPos, range);
            }

            return resultRange;
        }

        public List<Vector3> Get4way(Vector3 pos)
        {
                return new List<Vector3> {pos,
                new Vector3(pos.x+1, pos.y, pos.z),
                new Vector3(pos.x-1, pos.y, pos.z),
                new Vector3(pos.x, pos.y+1, pos.z),
                new Vector3(pos.x, pos.y-1, pos.z),};
        }

        public List<Vector3> MakeRange(List<Vector3> range, MOVE_DIR dir, Vector3 playerPos, int viewRange)
        {
            List<Vector3> showRange = new List<Vector3>();
            showRange.AddRange(range);

            foreach (Vector3 pos in range)
            {
                List<Vector3> ways =  Get4way(pos);

                foreach (Vector3 pos1 in ways)
                {
                    if(showRange.Contains(pos1) == false)
                    {
                        showRange.Add(pos1);
                    }
                }
            }

            List<Vector3> showRangeOneSideView = new List<Vector3>();
            
            foreach (Vector3 pos in showRange)
            {
                switch (dir)
                {
                    case MOVE_DIR.LEFT: if ( pos.x >= playerPos.x - viewRange) showRangeOneSideView.Add(pos); break;
                    case MOVE_DIR.RIGHT: if ( pos.x <= playerPos.x + viewRange) showRangeOneSideView.Add(pos); break;
                    case MOVE_DIR.UP: if (pos.y <= playerPos.y + viewRange) showRangeOneSideView.Add(pos); break;
                    case MOVE_DIR.DOWN: if ( pos.y >= playerPos.y - viewRange) showRangeOneSideView.Add(pos); break;
                }
            }

            return showRangeOneSideView;
        }
        
        public void ShowObjs(Vector3 playerPos, MOVE_DIR dir, int range)
		{
            List<Vector3> showRange = GetShowRange(playerPos, dir, range);

            if(curLevel.GetMapOfStructures(playerPos) == 1)
            {
                Vector3 viewPos = playerPos;
                switch (dir)
                {
                    case MOVE_DIR.RIGHT: viewPos.x += 1; break;
                    case MOVE_DIR.LEFT: viewPos.x -= 1; break;
                    case MOVE_DIR.UP: viewPos.y += 1; break;
                    case MOVE_DIR.DOWN: viewPos.y -= 1; break;
                }
                showRange.Add(viewPos);
            }

            foreach (GameObject obj in curLevel.tiles)
			{
				if (obj == null) continue;
				bool bShow = false;                
                                
                foreach (Vector3 showPos in showRange)
                {
                    if (curLevel.GetMapOfStructures(showPos) == 1) continue;
                    if (showPos == obj.transform.position)
                    {
                        bShow = true;
                        break;
                    }
                }

                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
				if (renderer)
				{
                    if (bShow) renderer.sortingLayerName = "Floor";
                    else renderer.sortingLayerName = "Fog";

                    if(obj.transform.position != playerPos)
                    {
                        Color color = Color.black;
                        if (bShow) color = Color.gray;
                        renderer.color = color;
                    }                                        
				}
			}

            foreach (Vector3 showPos in showRange)
            {               
                if (showPos == gateWay.transform.position)
                {
                    gateWay.GetComponent<SpriteRenderer>().sortingLayerName = "Map";
                    break;
                }

                foreach(GameObject item in curLevel.items)
                {
                    if (showPos == item.transform.position)
                    {
                        item.GetComponent<SpriteRenderer>().sortingLayerName = "Map";
                        break;
                    }
                }

                foreach (DropInfo item in dropItems)
                {
                    if (showPos == item.obj.transform.position)
                    {
                        item.obj.GetComponent<SpriteRenderer>().sortingLayerName = "Map";
                        break;
                    }
                }
            }
        }

        public void ShowEnemyScope(List<Vector3> unitPositions, List<MOVE_DIR> dirs, List<int> ranges)
        {
            List<Vector3> showRange = new List<Vector3>();
            for (int i = 0; i < unitPositions.Count; i++)
            {
                Vector3 unitPos = unitPositions[i];
                MOVE_DIR dir = dirs[i];
                int range = ranges[i];

                showRange.AddRange(GetShowRange(unitPos, dir, range));

                if (curLevel.GetMapOfStructures(unitPos) == 1)
                {
                    Vector3 viewPos = unitPos;
                    switch (dir)
                    {
                        case MOVE_DIR.RIGHT: viewPos.x += 1; break;
                        case MOVE_DIR.LEFT: viewPos.x -= 1; break;
                        case MOVE_DIR.UP: viewPos.y += 1; break;
                        case MOVE_DIR.DOWN: viewPos.y -= 1; break;
                    }
                    showRange.Add(viewPos);
                }
            }

            foreach (GameObject obj in curLevel.tiles)
            {
                if (obj == null) continue;
                bool bShow = false;

                foreach (Vector3 showPos in showRange)
                {
                    if (curLevel.GetMapOfStructures(showPos) == 1) continue;
                    if (showPos == obj.transform.position)
                    {
                        bShow = true;
                        break;
                    }
                }

                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
                if (renderer)
                {
                    Color color = renderer.color;
                    if (bShow)
                    {
                        color = Color.gray;                        
                    }
                    renderer.color = color;
                }
            }

        }

		//Awake is always called before any Start functions
		void Awake()
		{
            //Check if instance already exists
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);	
			
			DontDestroyOnLoad(gameObject);
		    
            boardScript = GetComponent<BoardManager>();            

            InitGame();
        }

        void ResetPlayerInfo()
        {
            money = 0;
            myWeapon = EffectManager.instance.GetWeapon(WEAPON.W1);
            myShip = new SpaceShip(1, 2, 6);
            myShip.ReadyToDeparture();
            myShip.SetWeapon(myWeapon);
        }

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {              
            if (instance.numberOfUniverse == instance.numberOfUniverseInit)
            {
                instance.InitGame();
                instance.ResetPlayerInfo();
                EffectManager.instance.Init();
            }
            else
            {
                instance.InitGame();
                instance.GoIntoGateway();
                EffectManager.instance.Init();
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        void HidePurchasePanel()
        {
            purchasePanel.SetActive(false);
        }

        void ShowPurchasePanel()
        {
            purchasePanel.SetActive(true);
        }

        void InitGame()
		{
            curLevel.Init();
                        
            gameMessage = GameObject.Find("Msg").GetComponent<Text>();
            universeText = GameObject.Find("GatewayText").GetComponent<Text>();
            gatewaySubText = GameObject.Find("GatewaySubText").GetComponent<Text>();
            tryText = GameObject.Find("TryText").GetComponent<Text>();
            readyDescribeText = GameObject.Find("ReadyDescribeText").GetComponent<Text>();
            readyDescribeText.transform.localPosition = new Vector3(0, -500, 0);

            adBtn = GameObject.Find("AdButton").GetComponent<Button>();
            adBtn.onClick.RemoveAllListeners();
            adBtn.onClick.AddListener(()=> AddTry(1));
            purchaseBtn = GameObject.Find("PurchaseButton").GetComponent<Button>();
            purchaseBtn.onClick.RemoveAllListeners();
            purchaseBtn.onClick.AddListener(ShowPurchasePanel);

            readyBtn = GameObject.Find("ReadyButton").GetComponent<Button>();
            readyBtn.onClick.RemoveAllListeners();
            readyBtn.onClick.AddListener(GoIntoGateway);

            
            closePurchaseButton = GameObject.Find("ClosePurchaseButton").GetComponent<Button>();
            closePurchaseButton.onClick.RemoveAllListeners();
            closePurchaseButton.onClick.AddListener(HidePurchasePanel);

            purchase10Button = GameObject.Find("Purchase10Button").GetComponent<Button>();
            purchase10Button.onClick.RemoveAllListeners();
            purchase10Button.onClick.AddListener(() => AddTry(10));

            purchase30Button = GameObject.Find("Purchase30Button").GetComponent<Button>();
            purchase30Button.onClick.RemoveAllListeners();
            purchase30Button.onClick.AddListener(() => AddTry(30));

            purchase50Button = GameObject.Find("Purchase50Button").GetComponent<Button>();
            purchase50Button.onClick.RemoveAllListeners();
            purchase50Button.onClick.AddListener(() => AddTry(50));


            


            purchasePanel = GameObject.Find("PurchasePanel");
            purchasePanel.SetActive(false);

            gameMessage.gameObject.SetActive(false);

            dropItems.Clear();
            InitPages();
        }

        public void ReadyToDeparture()
        {
            instance.ResetPlayerInfo();
            ChangePage(PAGE.READY);
        }

        public void AddTry(int count)
        {
            tryCount += count;
        }

        public void DiscountTry()
        {
            tryCount--;
            if (tryCount < 0) tryCount = 0;
        }

        public void GoIntoGateway()
        {
            DiscountTry();

            SoundManager.instance.PlaySingleBtn();
            InitLevel(numberOfUniverse);
            universeText.text = "제 " + numberOfUniverse + " 우주";

            string universeSizeText = "매우 큼";
            if(curLevel.rows + curLevel.columns < 30)
            {
                universeSizeText = "작음";
            }
            else  if (curLevel.rows + curLevel.columns < 40)
            {
                universeSizeText = "보통";
            }
            else if (curLevel.rows + curLevel.columns < 50)
            {
                universeSizeText = "큼";
            }

            string missiondesc = string.Format("수집해야할 크리스탈 수 : {0}\n\n시간 제한 : {1}\n\n우주 크기 : {2}", curLevel.missionItemCount, curLevel.timeLimit, universeSizeText);
            gatewaySubText.text = missiondesc + "\n\n" + "진입중...";
            ChangePage(PAGE.GATEWAY);
            Invoke("StartMission", 3f);
        }
        
        void InitLevel(int levelId)
        {
            if(numberOfUniverseEnd == 1)
            {
                levelId = 10;                
            }
            doingSetup = true;

            Dictionary<int, int> eInfos = new Dictionary<int, int>();

            switch(levelId)
            {
                case 10: eInfos.Add(0, 10); eInfos.Add(1, 5); eInfos.Add(2, 5); break;
                case 1: eInfos.Add(0, 1); break;
                case 2: eInfos.Add(0, 2); eInfos.Add(1, 1); break;
                case 3: eInfos.Add(0, 5); eInfos.Add(1, 2); eInfos.Add(2, 1); break;
                case 4: eInfos.Add(0, 5); eInfos.Add(1, 4); eInfos.Add(2, 3); break;
                case 5: eInfos.Add(0, 10); eInfos.Add(1, 5); eInfos.Add(2, 5); break;
            }

            Dictionary<int, int> sInfos = new Dictionary<int, int>();

            switch (levelId)
            {
                case 10: sInfos.Add(1, 30); sInfos.Add(3, 10); sInfos.Add(4, 5); break;
                case 1: sInfos.Add(1, 5); sInfos.Add(3, 3); break;
                case 2: sInfos.Add(1, 15); sInfos.Add(3, 6); sInfos.Add(4, 2); break;
                case 3: sInfos.Add(1, 20); sInfos.Add(3, 6); sInfos.Add(4, 2); break;
                case 4: sInfos.Add(1, 30); sInfos.Add(3, 10); sInfos.Add(4, 5); break;
                case 5: sInfos.Add(1, 30); sInfos.Add(3, 10); sInfos.Add(4, 5); break;
            }

            Dictionary<int, int> rInfos = new Dictionary<int, int>();

            switch (levelId)
            {
                case 10: rInfos.Add(2, 10); rInfos.Add(3, 3); rInfos.Add(4, 2); break;
                case 1: rInfos.Add(2, 4); rInfos.Add(3, 0); rInfos.Add(4, 0); break;
                case 2: rInfos.Add(2, 8); rInfos.Add(3, 1); rInfos.Add(4, 0); break;
                case 3: rInfos.Add(2, 8); rInfos.Add(3, 2); rInfos.Add(4, 1); break;
                case 4: rInfos.Add(2, 10); rInfos.Add(3, 3); rInfos.Add(4, 2); break;
                case 5: rInfos.Add(2, 10); rInfos.Add(3, 3); rInfos.Add(4, 2); break;
            }
            int colums = 10;
            int rows = 10;
            switch (levelId)
            {
                case 10: colums = 40; rows = 40; break;
                case 1: colums = 10; rows = 10; break;
                case 2: colums = 20; rows = 10; break;
                case 3: colums = 15; rows = 20; break;
                case 4: colums = 20; rows = 20; break;
                case 5: colums = 25; rows = 25; break;
            }
            
            float timeLimit = 180f;
            if(levelId == 4) timeLimit = 240f;
            else if (levelId > 4) timeLimit = 300f;

            int missionItemCount = levelId + 1;           

            curLevel.Setup(levelId, timeLimit, rows, colums, missionItemCount, eInfos, sInfos, rInfos);
            boardScript.SetupScene(curLevel);
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Init();
        }

        public void StartMission()
		{               
            ChangePage(PAGE.SPACE);			
            doingSetup = false;
        }

        void DescribeRolling()
        {
            Vector3 rolling = new Vector3(0, 30, 0);
            rolling *= Time.deltaTime;
            readyDescribeText.transform.position += rolling;
            if(readyDescribeText.transform.position.y > 1000 )
            {
                readyDescribeText.transform.localPosition = new Vector3(0, -500, 0);
            }
        }


        void Update()
		{
            tryText.text = "도전 가능 횟수 : " + tryCount;
            if(readyPage.activeSelf == true)
            {
                DescribeRolling();
            }

            if (doingSetup) return;
			if (msgTimer >= 0) {
				msgTimer -= Time.deltaTime;
				if (msgTimer <= 0) {
					msgTimer = 0;
					gameMessage.text = "";
                    gameMessage.gameObject.SetActive(false);
                }
			}
            CameraChasePlayer();
            UpdateOtherPlayersScope();
        }

        void CameraChasePlayer()
        {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            playerPos.z = -10;
            Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            camera.transform.position = playerPos;
        }

        void UpdateOtherPlayersScope()
        {
            List<Vector3> positions = new List<Vector3>();
            List<MOVE_DIR> dirs = new List<MOVE_DIR>();
            List<int> ranges = new List<int>();

            foreach(Enemy en in curLevel.enemies)
            {
                if(en.type == 2)
                {
                    positions.Add(en.transform.position);
                    dirs.Add(en.curDir);
                    ranges.Add(en.myShip.scopeRange);
                }                
            }            

            ShowEnemyScope(positions, dirs, ranges);
        }

        public void NextUniverse()
        {
            numberOfUniverse++;
            doingSetup = true;            
            string resultMsg = "다음 우주로 이동합니다!";
            spacePage.GetComponent<SpacePage>().ShowResult("탐색 완료", resultMsg, "Next");
        }

        public bool IsEnd()
        {
            return numberOfUniverse == numberOfUniverseEnd;
        }

        public void Win()
        {
            doingSetup = true;
            numberOfUniverse = numberOfUniverseInit;
            string resultMsg = "우주의 끝에 도달하였습니다. 지구로 귀환합니다.!";
            spacePage.GetComponent<SpacePage>().ShowResult("탐색 임무 완수", resultMsg, "Come Back to Earth");
        }

        public void GameOver()
		{
            doingSetup = true;
            numberOfUniverse = numberOfUniverseInit;
            spacePage.GetComponent<SpacePage>().ShowResult("탐색 실패", "우주의 미아가 되었습니다...", "첫화면으로");
		}

        GameObject frontPage;
        GameObject readyPage;
        GameObject spacePage;
        GameObject gatewayPage;

        public void InitPages()
        {
            frontPage = GameObject.Find("FrontPage");
            readyPage = GameObject.Find("ReadyPage");
            spacePage = GameObject.Find("SpacePage");
            gatewayPage = GameObject.Find("GatewayPage");            

            ChangePage(PAGE.FRONT);
        }

        public void ChangePage(PAGE nextPage)
        {
            bool activeFront = false;
            bool activeGateway = false;
            bool activeSpace = false;
            bool activeReady = false;

            switch (nextPage)
            {
                case PAGE.FRONT: activeFront = true; break;
                case PAGE.GATEWAY: activeGateway = true; break;
                case PAGE.SPACE: activeSpace = true; break;
                case PAGE.READY: activeReady = true; break;
            }

            frontPage.SetActive(activeFront);
            gatewayPage.SetActive(activeGateway);            
            spacePage.SetActive(activeSpace);
            readyPage.SetActive(activeReady);
        }

        public void ActivateRootBtn(Vector3 pos)
        {            
            spacePage.GetComponent<SpacePage>().ActivateSearchBtn(pos);
        }

        public void EnableGateway()
        {
            gateWay.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
}

