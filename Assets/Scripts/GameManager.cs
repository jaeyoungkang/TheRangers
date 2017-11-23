﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Completed
{
    public enum PAGE { FRONT, GATEWAY, SPACE };

    public class GameManager : MonoBehaviour
	{
        public static GameManager instance = null;
        public Level curLevel = new Level();
        public LevelEfx lvEfx = new LevelEfx();

        public TextAsset[] maps;

        private GameObject gatewayPage;
        private Text universeText;
        public Text gameMessage;
        public float msgTimer = 0;

        private BoardManager boardScript;

        public bool doingSetup = true;

        public GameObject rootBox;
        public List<GameObject> dropItems = new List<GameObject>();

        public int numberOfUniverse = 1;
        public int numberOfUniverseInit = 1;
        public int numberOfUniverseEnd = 3;

        public SpaceShip myShip;
        public List<Weapon> myWeapons = new List<Weapon>();

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
            ResetPlayerInfo();

        }

        void ResetPlayerInfo()
        {
            myShip = new SpaceShip(10, 0.4f, 2);
            myShip.ReadyToDeparture(GameManager.instance.lvEfx.weaponM, 8, 4, 2);
            myWeapons.Clear();
            myWeapons.Add(GameManager.instance.lvEfx.GetWeapon(1));
            myWeapons.Add(GameManager.instance.lvEfx.GetWeapon(2));
            myWeapons.Add(GameManager.instance.lvEfx.GetWeapon(3));
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
            }
            else
            {
                instance.InitGame();
                instance.GoIntoGateway();
            }
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        void InitGame()
		{            
            lvEfx.Init();
            curLevel.Init();
                        
            gameMessage = GameObject.Find("Msg").GetComponent<Text>();
            universeText = GameObject.Find("GatewayText").GetComponent<Text>();
            gameMessage.gameObject.SetActive(false);

            dropItems.Clear();
            InitPages();
        }

        public void GoIntoGateway()
        {
            universeText.text = "제 " + numberOfUniverse + " 우주";
            ChangePage(PAGE.GATEWAY);            
            Invoke("StartMission", 2f);
        }

        void InitLevel(int levelId)
        {
            doingSetup = true;
                        
            curLevel.Setup(levelId, maps[levelId-1].text);
            boardScript.SetupScene(curLevel);
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Init();
        }

        public void StartMission()
		{
            InitLevel(numberOfUniverse);
            ChangePage(PAGE.SPACE);			
            doingSetup = false;
        }
        
		void Update()
		{
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
            spacePage.GetComponent<SpacePage>().ShowResult(resultMsg);                        
        }

        public bool IsEnd()
        {
            return numberOfUniverse == numberOfUniverseEnd;
        }

        public void Win()
        {
            doingSetup = true;
            numberOfUniverse = numberOfUniverseInit;
            string resultMsg = "탐색 임무 완수!";
            spacePage.GetComponent<SpacePage>().ShowResult(resultMsg);
        }

        public void GameOver()
		{
            doingSetup = true;
            numberOfUniverse = numberOfUniverseInit;
            spacePage.GetComponent<SpacePage>().ShowResult("You died.");
		}

        GameObject frontPage;
        GameObject spacePage;

        public void InitPages()
        {
            frontPage = GameObject.Find("FrontPage");
            spacePage = GameObject.Find("SpacePage");
            gatewayPage = GameObject.Find("GatewayPage");

            ChangePage(PAGE.FRONT);
        }

        public void ChangePage(PAGE nextPage)
        {
            bool activeFront = false;
            bool activeGateway = false;
            bool activeSpace = false;

            switch (nextPage)
            {
                case PAGE.FRONT: activeFront = true; break;
                case PAGE.GATEWAY: activeGateway = true; break;
                case PAGE.SPACE: activeSpace = true; break;
            }

            frontPage.SetActive(activeFront);
            gatewayPage.SetActive(activeGateway);            
            spacePage.SetActive(activeSpace);
            
        }

        public void ActivateRootBtn(Vector3 pos)
        {            
            spacePage.GetComponent<SpacePage>().ActivateSearchBtn(pos);
        }
    }
}

