using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f;						
		public float turnDelay = 0.1f;							
		public int playerFoodPoints = 100;						
		public static GameManager instance = null;				
                
        private Text levelText;									
		private GameObject levelImage;
        private Text enemyText;
        public Text gameMessage;
        public float msgTimer = 0;

        private BoardManager boardScript;

		private List<Enemy> enemies;
        private List<Player> otherPlayers;

        public bool doingSetup = true;        
		
		public int[,] map;

		public void MakeGameMap (int columns, int rows)
		{
			map = new int[columns,rows];
			for (int i = 0; i < columns; i++) {
				for (int j = 0; j < rows; j++) {
					map [i, j] = 0;
				}
			}
		}

		public void SetMap(Vector3 pos, int value)
		{
			int x = (int)pos.x;
			int y = (int)pos.y;

			if (x<0 || map.GetUpperBound (0) < x)
				return;
			if (y<0 || map.GetUpperBound (1) < y)
				return;

			map [x,y] = value;
		}

		public int GetMapValue(Vector3 pos)
		{
			int x = (int)pos.x;
			int y = (int)pos.y;

			if (x<0 || map.GetUpperBound (0) < x)
				return 1;
			if (y<0 || map.GetUpperBound (1) < y)
				return 1;
			
			return map [x,y];
		}

		public void UpdateGameMssage(string msg, float time)
		{
            gameMessage.gameObject.SetActive(true);

            gameMessage.text = msg;
			msgTimer = time;
		}

		private List<GameObject> tiles = new List<GameObject>();
		private List<GameObject> walls = new List<GameObject>();

		public GameObject explosioinInstance;
		public GameObject exploreEffect;

		public IEnumerator ExploreTarget(Vector3 targetPos)
		{
			explosioinInstance.transform.position = targetPos;
			explosioinInstance.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			explosioinInstance.SetActive(false);
		}

        public void DestroyEnemy(GameObject target)
        {
            Enemy en = target.GetComponent<Enemy>();
            enemies.Remove(en);
            target.SetActive(false);

            if (enemies.Count == 0)
            {
                Win();
            }
        }

        public void DestroyOtherPlayer(GameObject target)
        {
            Player otherPlayer = target.GetComponent<Player>();
            otherPlayers.Remove(otherPlayer);
            target.SetActive(false);

            if(otherPlayers.Count == 0 )
            {
                Win();
            }
        }


        public void AttackObj(Vector3 targetPos)
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

        public int GetFloorType(Vector3 playerPos)
        {
            foreach (GameObject obj in tiles)
            {
                if (obj.transform.position == playerPos)
                {
                    return obj.GetComponent<ExFloor>().type;                    
                }
            }

            return 0;
        }

        public List<Vector3> GetShowRange(Vector3 playerPos, MOVE_DIR dir)
        {
            int type = GetFloorType(playerPos);            
            Vector3 pos = playerPos;
            switch(dir)
            {
                case MOVE_DIR.LEFT: pos.x -= type; break;
                case MOVE_DIR.RIGHT: pos.x += type; break;
                case MOVE_DIR.UP: pos.y += type; break;
                case MOVE_DIR.DOWN: pos.y -= type; break;
            }

            List<Vector3> resultRange = new List<Vector3> { pos, };
            for(int i=0; i<type; i++)
            {
                resultRange = MakeRange(resultRange, dir, playerPos);
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

        public List<Vector3> MakeRange(List<Vector3> range, MOVE_DIR dir, Vector3 playerPos)
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
            
            int type = GetFloorType(playerPos);

            foreach (Vector3 pos in showRange)
            {
                switch (dir)
                {
                    case MOVE_DIR.LEFT: if ( pos.x >= playerPos.x - type) showRangeOneSideView.Add(pos); break;
                    case MOVE_DIR.RIGHT: if ( pos.x <= playerPos.x + type) showRangeOneSideView.Add(pos); break;
                    case MOVE_DIR.UP: if (pos.y <= playerPos.y + type) showRangeOneSideView.Add(pos); break;
                    case MOVE_DIR.DOWN: if ( pos.y >= playerPos.y - type) showRangeOneSideView.Add(pos); break;
                }
            }

            return showRangeOneSideView;
//            return showRange;
        }        

        public void ShowObjs(Vector3 playerPos, MOVE_DIR dir)
		{
            List<Vector3> showRange = GetShowRange(playerPos, dir);

            if(GetFloorType(playerPos) == 0)
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

            foreach (GameObject obj in tiles)
			{
				if (obj == null) continue;
				bool bShow = false;                
                ExFloor floor = obj.GetComponent<ExFloor>();
                                
                foreach (Vector3 showPos in showRange)
                {
                    if (floor && floor.type == 0) continue;
                    if (showPos == obj.transform.position)
                    {
                        bShow = true;
                        break;
                    }
                }

                Renderer renderer = obj.GetComponent<SpriteRenderer>();
				if (renderer)
				{
                    if (bShow) renderer.sortingLayerName = "Floor";
                    else renderer.sortingLayerName = "Fog";

                    Color color = renderer.material.color;
                    if (bShow) color = Color.white;                    
                    else color = Color.gray;
                    renderer.material.color = color;

                    switch (floor.type)
                    {
                        case 0:
                            if (playerPos == obj.transform.position) renderer.sortingLayerName = "Floor";
                            else renderer.sortingLayerName = "Fog";
                            break;
                    }                    
				}
			}

            foreach (Enemy en in enemies)
            {
                bool bShow = false;
                foreach (Vector3 showPos in showRange)
                {
                    if (showPos == en.transform.position)
                    {
                        bShow = true;
                        break;
                    }
                }

                foreach (GameObject obj in tiles)
                {
                    if (obj == null) continue;
                    if(obj.transform.position == en.transform.position)
                    {
                        ExFloor floor = obj.GetComponent<ExFloor>();
                        if (floor && floor.type == 0) bShow = false;
                    }                    
                }

                 Renderer renderer = en.GetComponent<SpriteRenderer>();
                if (renderer)
                {
                    if (bShow) renderer.sortingLayerName = "Player";
                    else renderer.sortingLayerName = "Enemy";                    
                }
            }
        }

		public void ClearFloors()
		{
			tiles.Clear ();
		}

		public void AddFloor(GameObject obj)
		{
			tiles.Add (obj);
		}


		public void ClearWalls()
		{
			walls.Clear ();
		}

		public void AddWall(GameObject obj)
		{
			walls.Add (obj);
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
			
			enemies = new List<Enemy>();
            otherPlayers = new List<Player>();
            
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
			explosioinInstance = Instantiate(exploreEffect, transform.position, Quaternion.identity);
			explosioinInstance.SetActive(false);

			doingSetup = true;
			
			levelImage = GameObject.Find("LevelImage");			
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
            enemyText = GameObject.Find("EnemyText").GetComponent<Text>();
            gameMessage = GameObject.Find("Msg").GetComponent<Text>();
			levelText.text = "the rangers";
			levelImage.SetActive(true);
			Invoke("HideLevelImage", levelStartDelay);
			
			enemies.Clear();
            otherPlayers.Clear();
			
			boardScript.SetupScene();			
		}

		void HideLevelImage()
		{
			levelImage.SetActive(false);
			
			doingSetup = false;
		}
		
		void UpdateOthers()
        {
            if (doingSetup)
                return;

            enemyTime -= Time.deltaTime;

            if (!enemiesMoving && enemyTime <= 0)
            {                
//                StartCoroutine(MoveEnemies());
                enemyTime = 2.0f;
            }

//            enemyText.text = "Enemy: " + enemies.Count;
            
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

            enemyText.text = "Enemy: " + otherPlayers.Count;
        }

        float enemyTime = 2.0f;
		
		public void AddEnemyToList(Enemy script)
		{
			enemies.Add(script);
		}

        public void AddOtherPlayerToList(Player script)
        {
            otherPlayers.Add(script);
        }

        public void Win()
        {
            levelText.text = "You win!";
            levelImage.SetActive(true);
            enabled = false;
        }

        public void GameOver()
		{
			levelText.text = "You died.";
			levelImage.SetActive(true);
			enabled = false;
		}

        bool enemiesMoving = false;
        IEnumerator MoveEnemies()
        {
            enemiesMoving = true;
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].MoveEnemy();
                yield return new WaitForSeconds(enemies[i].moveTime);
            }
            enemiesMoving = false;
        }
    }
}

