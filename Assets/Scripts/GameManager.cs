﻿using UnityEngine;
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
		[HideInInspector] public bool playersTurn = true;
                
        private Text levelText;									
		private GameObject levelImage;							
		private BoardManager boardScript;						
		private int level = 1;									
		private List<Enemy> enemies;							
		private bool doingSetup = true;

        private Text enemyText;
        public Text gameMessage;
		public float msgTimer = 0;

		public void UpdateGameMssage(string msg, float time)
		{
            gameMessage.gameObject.SetActive(true);

            gameMessage.text = msg;
			msgTimer = time;
		}

		private List<GameObject> tiles = new List<GameObject>();
		private List<GameObject> walls = new List<GameObject>();

		public void DestroyEnemy(GameObject target)
		{
            Enemy en = target.GetComponent<Enemy> ();
			enemies.Remove (en);
			target.SetActive (false);			

            if(enemies.Count == 0)
            {
                Win();
            }
		}

        public void AttackObj(Vector3 targetPos)
		{
            foreach (GameObject obj in walls)
            {
                if (targetPos == obj.transform.position)
                {
                    obj.GetComponent<Wall>().DamageWall(1);
                    return;
                }
            }

            foreach (Enemy en in enemies)
            {
                if (targetPos == en.transform.position)
                {
                    en.BeDamaged(1);
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

        public List<Vector3> GetShowRange(Vector3 playerPos)
        {
            int type = GetFloorType(playerPos);

            List<Vector3> resultRange = new List<Vector3> { playerPos, };
            for(int i=0; i<type; i++)
            {
                resultRange = MakeRange(resultRange);
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

        public List<Vector3> MakeRange(List<Vector3> range)
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

            return showRange;
        }        

        public void ShowObjs(Vector3 playerPos)
		{
            List<Vector3> showRange = GetShowRange(playerPos);                        

            foreach (GameObject obj in tiles)
			{
				if (obj == null) continue;
				bool bShow = false;

				foreach (Vector3 showPos in showRange) 
				{
                    ExFloor floor = obj.GetComponent<ExFloor>();
                    if (floor && floor.type == 0) continue; // can't see a type 0 floor
                    if (showPos == obj.transform.position) {
						bShow = true;
						break;
					}
				}

				Renderer renderer = obj.GetComponent<SpriteRenderer>();
				if (renderer)
				{
					if (bShow)
					{
						Color color = renderer.material.color;
						color.a = 1f;
						renderer.material.color = color;
					}
					else
					{
						Color color = renderer.material.color;
						color.a = 0.6f;
						renderer.material.color = color;
					}
				}
			}

			foreach (Enemy en in enemies)
			{
				bool bShow = false;

				foreach (Vector3 showPos in showRange) 
				{
                    if (showPos == en.transform.position) {
                        bShow = true;

                        foreach (GameObject floor in tiles)
                        {
                            if(floor.transform.position == showPos)
                            {
                                ExFloor exfloor = floor.GetComponent<ExFloor>();
                                if (exfloor && exfloor.type == 0)
                                {
                                    bShow = false;
                                    break;
                                }
                            }                            
                        }
                                               
						break;
					}
				}

				Renderer renderer = en.GetComponent<SpriteRenderer>();
				if (renderer)
				{
					if (bShow)
					{
						Color color = renderer.material.color;
						color.a = 1f;
						renderer.material.color = color;
					}
					else
					{
						Color color = renderer.material.color;
						color.a = 0.0f;
						renderer.material.color = color;
					}
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

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);	
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
			
			//Assign enemies to a new List of Enemy objects.
			enemies = new List<Enemy>();
			
			//Get a component reference to the attached BoardManager script
			boardScript = GetComponent<BoardManager>();
			
			//Call the InitGame function to initialize the first level 
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
            instance.level++;
            instance.InitGame();
        }

		
		//Initializes the game for each level.
		void InitGame()
		{
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
			doingSetup = true;
			
			//Get a reference to our image LevelImage by finding it by name.
			levelImage = GameObject.Find("LevelImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			levelText = GameObject.Find("LevelText").GetComponent<Text>();

            enemyText = GameObject.Find("EnemyText").GetComponent<Text>();
            gameMessage = GameObject.Find("Msg").GetComponent<Text>();
			
			//Set the text of levelText to the string "Day" and append the current level number.
			levelText.text = "the rangers";
			
			//Set levelImage to active blocking player's view of the game board during setup.
			levelImage.SetActive(true);
			
			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			Invoke("HideLevelImage", levelStartDelay);
			
			//Clear any Enemy objects in our List to prepare for next level.
			enemies.Clear();
			
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			boardScript.SetupScene(level);
			
		}
		
		
		//Hides black image used between levels
		void HideLevelImage()
		{
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);
			
			//Set doingSetup to false allowing player to move again.
			doingSetup = false;
		}
		
		//Update is called every frame.
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
			
			if(doingSetup)				
				return;
                        
            enemyTime -= Time.deltaTime;

            if(!enemiesMoving && enemyTime <= 0 )
            {
                StartCoroutine(MoveEnemies());
                enemyTime = 1.0f;
            }

            enemyText.text = "Enemy: " + enemies.Count;
        }

        float enemyTime = 1.0f;
		
		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddEnemyToList(Enemy script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
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
        //Coroutine to move enemies in sequence.
        IEnumerator MoveEnemies()
        {
            enemiesMoving = true;
            //Loop through List of Enemy objects.
            for (int i = 0; i < enemies.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                enemies[i].MoveEnemy();

                //Wait for Enemy's moveTime before moving next Enemy, 
                yield return new WaitForSeconds(enemies[i].moveTime);
            }
            enemiesMoving = false;
        }
    }
}

