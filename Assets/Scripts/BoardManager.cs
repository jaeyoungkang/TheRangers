using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed
	
{
	
	public class BoardManager : MonoBehaviour
	{
		// Using Serializable allows us to embed a class with sub properties in the inspector.
		[Serializable]
		public class Count
		{
			public int minimum; 			//Minimum value for our Count class.
			public int maximum; 			//Maximum value for our Count class.
			
			
			//Assignment constructor.
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}		
		
		public int columns = 5; 										
		public int rows = 5;											
		public Count wallCount = new Count (5, 9);						
		public Count foodCount = new Count (1, 5);
        public Count ammo1Count = new Count(1, 5);
        public Count ammo2Count = new Count(1, 5);
        public Count ammo3Count = new Count(1, 5);
        public Count goldCount = new Count(1, 5);
        public Count gemCount = new Count(1, 5);

        public Count shelterCount = new Count(1, 5);
        public Count radarCount = new Count(1, 5);

        public Count enemiyCount = new Count(1, 5);
        public GameObject exit;											
		public GameObject[] floorTiles;									
		public GameObject[] wallTiles;									
		public GameObject[] foodTiles;
        public GameObject ammo1Tile;
        public GameObject ammo2Tile;
        public GameObject ammo3Tile;
        public GameObject goldTile;
        public GameObject gemTile;

        public GameObject shelterTile;
        public GameObject radarTile;

        public GameObject[] enemyTiles;
		public GameObject[] outerWallTiles;								
		
		private Transform boardHolder;									
		private List <Vector3> gridPositions = new List <Vector3> ();	                          
		
		
		void InitialiseList ()
		{
			gridPositions.Clear ();
			
			for(int x = 1; x < columns-1; x++)
			{
				for(int y = 1; y < rows-1; y++)
				{
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}

        void BoardSetup2()
        {
            boardHolder = new GameObject("Board").transform;

            GameManager.instance.MakeGameMapOfUnits(columns, rows);

            int[][] mapInfo = new int[][] {
                new int[]{ 1, 2, 2, 2, 3, 2, 0, 0, 1, 1 },
                new int[]{ 1, 2, 3, 2, 2, 1, 2, 2, 2, 2 },
                new int[]{ 1, 0, 3, 2, 1, 1, 2, 3, 3, 2 },
                new int[]{ 2, 0, 2, 3, 2, 1, 2, 2, 2, 2 },
                new int[]{ 2, 1, 2, 2, 2, 2, 1, 1, 0, 3 },

                new int[]{ 3, 1, 2, 3, 0, 3, 1, 2, 2, 2 },
                new int[]{ 2, 2, 2, 2, 0, 2, 1, 2, 3, 2 },
                new int[]{ 2, 3, 3, 2, 2, 1, 1, 2, 3, 2 },
                new int[]{ 2, 2, 2, 2, 1, 2, 1, 2, 2, 2 },
                new int[]{ 3, 2, 1, 0, 2, 3, 2, 1, 1, 0 },                
                
            };

            for (int x = -1; x < columns + 1; x++)
            {
                for (int y = -1; y < rows + 1; y++)
                {
                    GameObject toInstantiate = null;
                    if (x == -1 || x == columns || y == -1 || y == rows)
                    {
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                    }
                    else
                    {
                        toInstantiate = floorTiles[mapInfo[y][x]];
                        GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(boardHolder);
                        GameManager.instance.AddFloor(instance);
                    }                    
                }
            }
        }


        void BoardSetup ()
		{
			boardHolder = new GameObject ("Board").transform;

			GameManager.instance.MakeGameMapOfUnits(columns, rows);
            GameManager.instance.MakeGameMapOfStructures(columns, rows);

            for (int x = -1; x < columns + 1; x++)
			{
				for(int y = -1; y < rows + 1; y++)
				{
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
					if(x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];

					GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					
					instance.transform.SetParent (boardHolder);

                    if (x == -1 || x == columns || y == -1 || y == rows) continue;                        
                    GameManager.instance.AddFloor (instance);
				}
			}
		}
		
		Vector3 RandomPosition ()
		{
			int randomIndex = Random.Range (0, gridPositions.Count);
			Vector3 randomPosition = gridPositions[randomIndex];
			gridPositions.RemoveAt (randomIndex);
			return randomPosition;
		}

        void LayoutItemsAtRandom(GameObject tile, int minimum, int maximum, bool writeToMapTable = false, int mapValue = 0)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = RandomPosition();
                if (writeToMapTable) GameManager.instance.SetMapOfStructures(randomPosition, mapValue);
                LayoutItem(tile, randomPosition);
            }
        }

        void LayoutItemsByEnemy(GameObject tile, int minimum, int maximum, List<GameObject> enemies)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = RandomPosition();
                bool nearEnemy = false;
                foreach(GameObject en in enemies)
                {
                    if((en.transform.position - randomPosition).magnitude < 5f)
                    {
                        nearEnemy = true;
                        break;
                    }
                }
                if (nearEnemy == false) continue;

                LayoutItem(tile, randomPosition);
            }
        }

        void LayoutItemsAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {                
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                Vector3 randomPosition = RandomPosition();
                LayoutItem(tileChoice, randomPosition);

            }
        }

        void LayoutItem(GameObject tile, Vector3 pos)
        {
            GameObject obj = Instantiate(tile, pos, Quaternion.identity);
            GameObject objToShow = Instantiate(tile, pos, Quaternion.identity);
            Renderer renderer = objToShow.GetComponent<SpriteRenderer>();
            if (renderer)
            {
                renderer.sortingLayerName = "Map";
                objToShow.transform.SetParent(obj.transform);
                BoxCollider2D boxCol = objToShow.GetComponent<BoxCollider2D>();
                boxCol.enabled = false;
            }
            Scroll scrollItem = obj.GetComponent<Scroll>();
            if (scrollItem)
            {
                scrollItem.GenerateNumber();
            }
        }
        
        List<GameObject> LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
		{
            List<GameObject> instances = new List<GameObject>();
			int objectCount = Random.Range (minimum, maximum+1);
			
			for(int i = 0; i < objectCount; i++)
			{
				Vector3 randomPosition = RandomPosition();
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
                instances.Add(Instantiate(tileChoice, randomPosition, Quaternion.identity));
            }

            return instances;
        }

        public void DropItem(Vector3 dropPos)
        {
            int randomValue = Random.Range(0, 8);
            if(randomValue < 3)
            {
                GameObject tileChoice = foodTiles[Random.Range(0, foodTiles.Length)];
                Instantiate(tileChoice, dropPos, Quaternion.identity);
            }
            else if(randomValue == 4)
            {
                LayoutItem(ammo1Tile, dropPos);
            }
            else if (randomValue == 5)
            {
                LayoutItem(ammo2Tile, dropPos);
            }
        }

        public void SetupScene ()
		{
			GameManager.instance.ClearFloors ();
			GameManager.instance.ClearWalls ();

			BoardSetup ();
            //BoardSetup2();

            InitialiseList ();

            List<GameObject> enemies =  LayoutObjectAtRandom(enemyTiles, enemiyCount.minimum, enemiyCount.maximum);

            LayoutItemsByEnemy(ammo1Tile, ammo1Count.minimum, ammo1Count.maximum, enemies);
            LayoutItemsByEnemy(ammo2Tile, ammo2Count.minimum, ammo2Count.maximum, enemies);

 //           LayoutItemsAtRandom(ammo1Tile, ammo1Count.minimum, ammo1Count.maximum);
//            LayoutItemsAtRandom(ammo2Tile, ammo2Count.minimum, ammo2Count.maximum);

            LayoutItemsAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
//            LayoutItemsAtRandom(ammo3Tile, ammo3Count.minimum, ammo3Count.maximum);

//            LayoutItemsAtRandom(radarTile, radarCount.minimum, radarCount.maximum, true, 4);
//            LayoutItemsAtRandom(shelterTile, shelterCount.minimum, shelterCount.maximum, true, 1);
            LayoutStructuresByFile();
            
		}

        void LayoutStructure(Vector3 pos, int range)
        {
            GameObject tileChoice = radarTile;
            if (range == 1) tileChoice = shelterTile;
            GameObject obj = Instantiate(tileChoice, pos, Quaternion.identity);
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer)
            {
                renderer.sortingLayerName = "Map";
                if(range == 5)
                {
                    renderer.color = Color.yellow;
                }
            }

            GameManager.instance.SetMapOfStructures(pos, range);
        }

        void LayoutStructuresByFile()
        {
            string[] lines = System.IO.File.ReadAllLines(@"E:\TheRangers\map01.txt");
            
            int x = 0;
            int y = 0;
            foreach(string str in lines)
            {
                x = 0;
                string[] symbols = str.Split(',');
                foreach(string s in symbols)
                {
                    Vector3 pos = new Vector3(x, y, 0);
                    int value = Int32.Parse(s);
                    if (value != 0)
                        LayoutStructure(pos, value);
                    x++;                               
                }
                y++;                
            }
        }

    }
}
