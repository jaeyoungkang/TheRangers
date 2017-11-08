using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;
    using UnityEngine.UI;
    public enum PAGE { FRONT, MAIN, MISSION, MISSION_LIST, SPACE };  

    public class Level
    {
        public bool missionFinish;
        public List<int> rewardItems = new List<int>();
        public int rewardMoney;

        public int collectMission;        
        public int id;
        public int columns, rows;
        public string filePath;
        public int[,] mapOfUnits;
        public int[,] mapOfStructures;
        public int[,] mapOfItems;

        public List<GameObject> items = new List<GameObject>();
        public List<GameObject> tiles = new List<GameObject>();
        public List<Player> otherPlayers = new List<Player>();

        public void AddOtherPlayerToList(Player script)
        {
            otherPlayers.Add(script);
        }

        public void AttackOtherPlayer(Vector3 targetPos)
        {
            foreach (Player other in otherPlayers)
            {
                if (targetPos == other.transform.position)
                {
                    other.LoseHP(10);
                    return;
                }
            }
        }

        public void RemoveOtherPlayer(GameObject target)
        {
            Player otherPlayer = target.GetComponent<Player>();
            otherPlayers.Remove(otherPlayer);
            target.SetActive(false);
        }

        public void AddItem(GameObject obj)
        {
            items.Add(obj);
        }

        public void AddFloor(GameObject obj)
        {
            tiles.Add(obj);
        }

        public void Init(int levelId)
        {
            missionFinish = false;
            filePath = "";
            rewardItems.Clear();
            rewardMoney = 0;
            collectMission = 0;

            id = levelId;
            switch(id)
            {
                case 1:
                    filePath = "map01.txt";
                    rewardItems.Add(10);
                    rewardMoney = 10;
                    break;

                case 2:
                    filePath = "map02.txt";
                    rewardItems.Add(20);
                    rewardMoney = 10;
                    break;

                case 3:
                    filePath = "map03.txt";
                    collectMission = 3;
                    rewardItems.Add(30);
                    rewardMoney = 10;
                    break;
            }            
            
            foreach (Player other in otherPlayers)
            {
                UnityEngine.GameObject.Destroy(other.gameObject);
            }
            otherPlayers.Clear();

            foreach(GameObject tile in tiles)
            {
                UnityEngine.GameObject.Destroy(tile);
            }
            tiles.Clear();

            foreach(GameObject item in items)
            {
                UnityEngine.GameObject.Destroy(item);
            }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            string[] symbols = lines[0].Split(',');
            columns = symbols.Length;
            rows = lines.Length;

            MakeGameMapOfUnits(columns, rows);
            MakeGameMapOfStructures(columns, rows);
            MakeGameMapOfItems(columns, rows);
        }

        public void MakeGameMapOfUnits(int columns, int rows)
        {
            mapOfUnits = new int[columns, rows];
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    mapOfUnits[i, j] = 0;
                }
            }
        }

        public void SetMapOfUnits(Vector3 pos, int value)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || mapOfUnits.GetUpperBound(0) < x)
                return;
            if (y < 0 || mapOfUnits.GetUpperBound(1) < y)
                return;

            mapOfUnits[x, y] = value;
        }

        public int GetMapOfUnits(Vector3 pos)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || mapOfUnits.GetUpperBound(0) < x)
                return 1;
            if (y < 0 || mapOfUnits.GetUpperBound(1) < y)
                return 1;

            return mapOfUnits[x, y];
        }
        public void MakeGameMapOfItems(int columns, int rows)
        {
            mapOfItems= new int[columns, rows];
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    mapOfItems[i, j] = 0;
                }
            }
        }

        public void MakeGameMapOfStructures(int columns, int rows)
        {
            mapOfStructures = new int[columns, rows];
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    mapOfStructures[i, j] = 0;
                }
            }
        }

        public void SetMapOfStructures(Vector3 pos, int value)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || mapOfStructures.GetUpperBound(0) < x)
                return;
            if (y < 0 || mapOfStructures.GetUpperBound(1) < y)
                return;

            mapOfStructures[x, y] = value;
        }

        public int GetMapOfStructures(Vector3 pos)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || mapOfStructures.GetUpperBound(0) < x)
                return 1;
            if (y < 0 || mapOfStructures.GetUpperBound(1) < y)
                return 1;

            return mapOfStructures[x, y];
        }

        public void SetMapOfItems(Vector3 pos, int value)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || mapOfItems.GetUpperBound(0) < x)
                return;
            if (y < 0 || mapOfItems.GetUpperBound(1) < y)
                return;

            mapOfItems[x, y] = value;
        }

        public int GetMapOfItems(Vector3 pos)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            if (x < 0 || mapOfItems.GetUpperBound(0) < x)
                return 1;
            if (y < 0 || mapOfItems.GetUpperBound(1) < y)
                return 1;

            return mapOfItems[x, y];
        }
    }


    public class GameManager : MonoBehaviour
	{
        public int collectionCount = 0;
        public Level curLevel = new Level();
		public float levelStartDelay = 2f;						
		public float turnDelay = 0.1f;							
		public int playerFoodPoints = 100;						
		public static GameManager instance = null;				
                
        private Text enemyText;
        public Text gameMessage;
        public float msgTimer = 0;

        private BoardManager boardScript;

        public int storageAmmo1 = 0;
        public int storageAmmo2 = 0;
        public int powerSupply = 0;

        public List<int> inven = new List<int>();
        public int money = 10;
        public string storageText ="";

        public bool doingSetup = true;

        void BuyItem(int itemId)
        {
            switch (itemId)
            {
                case 1: storageAmmo1++; money -= 1; break;
                case 2: storageAmmo2++; money -= 1; break;
                case 3: powerSupply++; money -= 1; break;
            }
        }

        void SellItem(int itemId)
        {
            switch (itemId)
            {
                case 1: storageAmmo1--; money += 10; break;
                case 2: storageAmmo2--; money += 10; break;
                case 3: powerSupply--; money += 10; break;
            }
        }

        public int gold = 0;
        public int silver = 0;
        public int copper = 0;

        void ExtendStorage()
        {
            if (money < 10) return;           

            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().ExtendStorage();
            money -= 10;            
        }

        // 미션 설명 및 보상 표시 - 보상을 보고 미션을 선택한다.
        // 미션 완료시 보상 획득
        // 미션수행과 끝난후 보상으로 업그레이드 미션에서 확실하고 큰 보상은 하나 있어야한다.

        void UpdateStorage()
        {
            Player myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            if (storageAmmo1 + storageAmmo2 + powerSupply < myPlayer.myShip.storageSize)
            {
                if (Input.GetKeyDown("1")) BuyItem(1);
                else if (Input.GetKeyDown("2")) BuyItem(2);
                else if (Input.GetKeyDown("3")) BuyItem(3);
            }

            if (Input.GetKeyDown("4") && storageAmmo1 > 0) SellItem(1);
            else if (Input.GetKeyDown("5") && storageAmmo2 > 0) SellItem(2);
            else if (Input.GetKeyDown("6") && powerSupply > 0) SellItem(3);

            if (Input.GetKeyDown("0"))
            {
                ExtendStorage();
            }

            storageText = string.Format(@"
Money : {0}
Storage : {1} / {2}
Ammo1 : {3}
Ammo2 : {4}
powerSupply : {5}
                ", money, storageAmmo1 + storageAmmo2 + powerSupply, myPlayer.myShip.storageSize, storageAmmo1, storageAmmo2, powerSupply);

            string itemName = GetItemName(curLevel.rewardItems[0]);
            storageText += string.Format("\n\n 미션 보상 \n {0} $\n {1}", curLevel.rewardMoney, itemName);
        }

        public string GetItemName(int itemId)
        {
            string itemName = "";
            switch (itemId)
            {
                case 10: itemName = "쿠퍼 광석"; break;
                case 20: itemName = "실버 광석"; break;
                case 30: itemName = "골드 광석"; break;
            }
            return itemName;
        }

        public void UpdateGameMssage(string msg, float time)
		{
            gameMessage.gameObject.SetActive(true);

            gameMessage.text = msg;
			msgTimer = time;
		}
        
        public GameObject[] shotInstances = new GameObject[8];
        public GameObject shotTile;        

        public GameObject explosionInstance;
		public GameObject explosionTile;

		public IEnumerator ShowExplosionEffect(Vector3 targetPos)
		{
            explosionInstance.transform.position = targetPos;
            explosionInstance.SetActive(true);
			yield return new WaitForSeconds(0.5f);
            explosionInstance.SetActive(false);
		}

        int shotEffectIndex = 0;
        public IEnumerator ShowShotEffect(Vector3 targetPos)
        {
            GameObject shotInstance = shotInstances[shotEffectIndex];
            shotEffectIndex++;
            if (shotEffectIndex >= shotInstances.Length) shotEffectIndex = 0;

            shotInstance.transform.position = targetPos;
            shotInstance.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            shotInstance.SetActive(false);            
        }

        public void DestroyOtherPlayer(GameObject target)
        {
            StartCoroutine(ShowExplosionEffect(target.transform.position));

            curLevel.RemoveOtherPlayer(target);

            boardScript.DropItem(target.transform.position);
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
//            return showRange;
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
                    if (bShow) color = Color.red;
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
//            instance.level++;
            instance.InitGame();
        }

		
		void InitGame()
		{
            for(int i=0; i< shotInstances.Length; i++)
            {
                shotInstances[i] = Instantiate(shotTile, transform.position, Quaternion.identity);
                shotInstances[i].SetActive(false);
            }
                        
            explosionInstance = Instantiate(explosionTile, transform.position, Quaternion.identity);
            explosionInstance.SetActive(false);
            			
            enemyText = GameObject.Find("EnemyText").GetComponent<Text>();
            gameMessage = GameObject.Find("Msg").GetComponent<Text>();
						
            
            InitViewMode();
            InitPages();
        }

        void InitLevel(int levelId)
        {
            doingSetup = true;

            collectionCount = 0;
            curLevel.Init(levelId);
            boardScript.SetupScene(curLevel);
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Init();
        }

        public void StartMission()
        {
            HideLevelImage();
        }

        public void GotoMission(int id)
        {
            InitLevel(id);
            ChangePage(PAGE.MISSION);
        }

        public void GotoMissionListPage()
        {
            ChangePage(PAGE.MISSION_LIST);
        }

        void HideLevelImage()
		{
            ChangePage(PAGE.SPACE);
			doingSetup = false;
            SetLocalViewMode();
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().myShip.SetupStorage(storageAmmo1 * 2, storageAmmo2 * 2, powerSupply);
        }

        
		void Update()
		{
			if (msgTimer >= 0) {
				msgTimer -= Time.deltaTime;
				if (msgTimer <= 0) {
					msgTimer = 0;
					gameMessage.text = "";
                    gameMessage.gameObject.SetActive(false);
                }
			}

            enemyText.text = "Enemy: " + curLevel.otherPlayers.Count;
            
            if (curViewMode == LOCAL_VIEW)
            {                
                CameraScoll(camera.transform.position);
            }
            UpdateViewMode();

            UpdateOtherPlayersScope();

            if(curPage == PAGE.MISSION) UpdateStorage();

            if(curPage == PAGE.SPACE && curLevel.missionFinish == false)
            {
                switch(curLevel.id)
                {
                    case 1:
                    case 2:
                        if (curLevel.otherPlayers.Count == 0)
                        {
                            Win();
                        }
                        break;

                    case 3:
                        if(curLevel.collectMission == collectionCount)
                        {
                            Win();
                        }
                        break;
                }
                
            }
        }

        void UpdateOtherPlayersScope()
        {
            List<Vector3> positions = new List<Vector3>();
            List<MOVE_DIR> dirs = new List<MOVE_DIR>();
            List<int> ranges = new List<int>();

            foreach(Player other in curLevel.otherPlayers)
            {
                if(other.unitId == 1)
                {
                    positions.Add(other.transform.position);
                    dirs.Add(other.curDir);
                    ranges.Add(other.myShip.scopeRange);
                }                
            }            

            ShowEnemyScope(positions, dirs, ranges);
        }        

        public void Win()
        {
            curLevel.missionFinish = true;
            string resultMsg = "Winner Winner Chicken Dinner!";
            string itemName = GetItemName(curLevel.rewardItems[0]);
            resultMsg += string.Format("\n\n 미션 보상 \n {0} $\n {1}", curLevel.rewardMoney, itemName);
            spacePage.GetComponent<SpacePage>().ShowResult(resultMsg);

            money += curLevel.rewardMoney;
            inven.AddRange(curLevel.rewardItems);
        }

        public void GameOver()
		{
            spacePage.GetComponent<SpacePage>().ShowResult("You died.");
		}

        public Camera camera;
        public int MaxSize = 16;
        public int MinSize = 6;
        public Vector3 WorldPos = new Vector3(9.5f, 6f, -10f);
        public Vector3 LocalPos = new Vector3(4.5f, 4.5f, -10f);
        public GameObject TopFrame, BottomFrame, LeftFrame, RightFrame;

        const int WORLD_VIEW = 1;
        const int LOCAL_VIEW = 2;
        int curViewMode = LOCAL_VIEW;
        int preViewMode = LOCAL_VIEW;
        
        void SetLocalViewMode()
        {
            camera.orthographicSize = MinSize;
            preViewMode = curViewMode;
            TopFrame.SetActive(true);
            BottomFrame.SetActive(true);
            RightFrame.SetActive(true);
            LeftFrame.SetActive(true);

        }

        void UpdateViewMode()
        {            
            int speed = 10;
            if (curViewMode != preViewMode)
            {
                if (curViewMode == WORLD_VIEW)
                {
                    camera.orthographicSize = camera.orthographicSize + speed * Time.deltaTime;

                    if (camera.orthographicSize > MaxSize)
                    {
                        camera.orthographicSize = MaxSize;
                        preViewMode = curViewMode;
                    }
                }

                if (curViewMode == LOCAL_VIEW)
                {
                    CameraScoll(LocalPos);
                    camera.orthographicSize = camera.orthographicSize - speed * Time.deltaTime;
                    if (camera.orthographicSize < MinSize)
                    {
                        camera.orthographicSize = MinSize;
                        preViewMode = curViewMode;
                        TopFrame.SetActive(true);
                        BottomFrame.SetActive(true);
                        RightFrame.SetActive(true);
                        LeftFrame.SetActive(true);
                    }
                }
            }            
        }

        void InitViewMode()
        {
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            TopFrame = GameObject.Find("FrameTop");
            BottomFrame = GameObject.Find("FrameBottom");
            RightFrame = GameObject.Find("FrameRight");
            LeftFrame = GameObject.Find("FrameLeft");

            camera.orthographicSize = MinSize;
            camera.transform.position = LocalPos;
            TopFrame.SetActive(false);
            BottomFrame.SetActive(false);
            RightFrame.SetActive(false);
            LeftFrame.SetActive(false);
        }

        void CameraScoll(Vector3 MoveCamera)
        {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;            
            Vector3 MoveTopFrame = TopFrame.transform.position;
            Vector3 MoveBottomFrame = BottomFrame.transform.position;
            Vector3 MoveRightFrame = RightFrame.transform.position;
            Vector3 MoveLeftFrame = LeftFrame.transform.position;

            if (boardScript.columns > 10)
            {
                if (playerPos.x > 5 && playerPos.x < 15)
                {
                    MoveCamera.x = LocalPos.x + playerPos.x - 5;
                    MoveRightFrame.x = 14.5f + playerPos.x - 5;
                    MoveLeftFrame.x = -5.5f + playerPos.x - 5;
                }
                if (playerPos.x >= 15)
                {
                    MoveCamera.x = 14.5f;
                    MoveRightFrame.x = 25.5f;
                    MoveLeftFrame.x = 4.5f;
                }
            }

            if (boardScript.rows > 10)
            {
                if (playerPos.y > 5 && playerPos.y < 15)
                {
                    MoveCamera.y = LocalPos.y + playerPos.y - 5;
                    MoveTopFrame.y = 14.5f + playerPos.y - 5;
                    MoveBottomFrame.y = -5.5f + playerPos.y - 5;
                }
                if (playerPos.y >= 15)
                {
                    MoveCamera.y = 13.5f;
                    MoveTopFrame.y = 24.5f;
                    MoveBottomFrame.y = 4.5f;
                }
            }

            camera.transform.position = MoveCamera;
            TopFrame.transform.position = MoveTopFrame;
            BottomFrame.transform.position = MoveBottomFrame;
            RightFrame.transform.position = MoveRightFrame;
            LeftFrame.transform.position = MoveLeftFrame;
        }

        PAGE curPage = PAGE.FRONT;
        GameObject frontPage;
        GameObject mainPage;
        GameObject missionPage;
        GameObject missionListPage;
        GameObject spacePage;

        public void InitPages()
        {
            frontPage = GameObject.Find("FrontPage");
            mainPage = GameObject.Find("MainPage");
            missionPage = GameObject.Find("MissionPage");
            missionListPage = GameObject.Find("MissionListPage");
            spacePage = GameObject.Find("SpacePage");

            ChangePage(PAGE.FRONT);
        }
        
        public void GotoMain()
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            ChangePage(PAGE.MAIN);
        }

        
        public void ChangePage(PAGE nextPage)
        {
            bool activeFront = false;
            bool activeMain = false;
            bool activeMission = false;
            bool activeMissionList = false;
            bool activeSpace = false;

            switch (nextPage)
            {
                case PAGE.FRONT: activeFront = true; break;
                case PAGE.MAIN: activeMain = true; break;
                case PAGE.MISSION: activeMission = true; break;
                case PAGE.MISSION_LIST: activeMissionList = true; break;
                case PAGE.SPACE: activeSpace = true; break;
            }

            frontPage.SetActive(activeFront);
            mainPage.SetActive(activeMain);
            missionPage.SetActive(activeMission);
            missionListPage.SetActive(activeMissionList);
            spacePage.SetActive(activeSpace);

            curPage = nextPage;
        }

        public void ActivateRootBtn(bool bActive)
        {
            spacePage.GetComponent<SpacePage>().ActivateRootBtn(bActive);
        }
    }
}

