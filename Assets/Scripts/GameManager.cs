using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Completed
{
    public enum PAGE { FRONT, MAIN, MISSION, MISSION_LIST, SPACE };

    public class GameManager : MonoBehaviour
	{       
        public Level curLevel = new Level();
        public LevelEfx lvEfx = new LevelEfx();
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

        public bool doingSetup = true;

        public GameObject rootBox;
        public List<GameObject> dropItems = new List<GameObject>();

        public void DropItem(Vector3 dropPos)
        {
            GameObject item = Instantiate(rootBox, dropPos, Quaternion.identity);
            GameManager.instance.curLevel.SetMapOfItems(dropPos, 1);
            dropItems.Add(item);
        }

        public GameObject GetDropBox(Vector3 pos)
        {
            foreach (GameObject obj in dropItems)
            {
                if (obj.transform.position == pos) return obj;
            }

            return null;
        }

        public void ReomveDropItem(GameObject item)
        {            
            dropItems.Remove(item);

            bool anotherItem = false;
            foreach(GameObject obj in dropItems)
            {
                if (obj.transform.position == item.transform.position) anotherItem = true;
            }
            if(anotherItem == false)
                GameManager.instance.curLevel.SetMapOfItems(item.transform.position, 0);

            item.SetActive(false);
        }

        public void UpdateGameMssage(string msg, float time)
		{
            gameMessage.gameObject.SetActive(true);

            gameMessage.text = msg;
			msgTimer = time;
		}

        public void DestroyOtherPlayer(GameObject target)
        {
            StartCoroutine(lvEfx.ShowExplosionEffect(target.transform.position));

            curLevel.RemoveOtherPlayer(target);

            DropItem(target.transform.position);            
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
            lvEfx.Init();
            curLevel.Init();

            enemyText = GameObject.Find("EnemyText").GetComponent<Text>();
            gameMessage = GameObject.Find("Msg").GetComponent<Text>();						
            
            InitViewMode();
            InitPages();
        }

        void InitLevel(int levelId)
        {
            doingSetup = true;

            curLevel.Setup(levelId);
            boardScript.SetupScene(curLevel);
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Init();
        }

        public void StartMission()
        {
            HideLevelImage();
        }

        public void GotoMission()
        {
            InitLevel(3);
            ChangePage(PAGE.MISSION);
        }

        void HideLevelImage()
		{
            ChangePage(PAGE.SPACE);
			doingSetup = false;
            SetLocalViewMode();
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

            enemyText.text = "Enemy: " + curLevel.enemies.Count;
            
            if (curViewMode == LOCAL_VIEW)
            {                
                CameraScoll(camera.transform.position);
            }
            UpdateViewMode();

            UpdateOtherPlayersScope();
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

        public void Win()
        {
            curLevel.missionFinish = true;
            string resultMsg = "Winner Winner Chicken Dinner!";            
            spacePage.GetComponent<SpacePage>().ShowResult(resultMsg);
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
        GameObject spacePage;

        public void InitPages()
        {
            frontPage = GameObject.Find("FrontPage");
            mainPage = GameObject.Find("MainPage");
            missionPage = GameObject.Find("MissionPage");            
            spacePage = GameObject.Find("SpacePage");

            ChangePage(PAGE.FRONT);
        }
        
        public void GotoMain()
        {
            ChangePage(PAGE.MAIN);
        }

        
        public void ChangePage(PAGE nextPage)
        {
            bool activeFront = false;
            bool activeMain = false;
            bool activeMission = false;
            bool activeSpace = false;

            switch (nextPage)
            {
                case PAGE.FRONT: activeFront = true; break;
                case PAGE.MAIN: activeMain = true; break;
                case PAGE.MISSION: activeMission = true; break;
                case PAGE.SPACE: activeSpace = true; break;
            }

            frontPage.SetActive(activeFront);
            mainPage.SetActive(activeMain);
            missionPage.SetActive(activeMission);            
            spacePage.SetActive(activeSpace);

            curPage = nextPage;
        }

        public void ActivateRootBtn(Vector3 pos)
        {            
            spacePage.GetComponent<SpacePage>().ActivateSearchBtn(pos);
        }
    }
}

