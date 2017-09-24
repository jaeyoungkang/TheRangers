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

        public Count enemiyCount = new Count(1, 5);
        public GameObject exit;											
		public GameObject[] floorTiles;									
		public GameObject[] wallTiles;									
		public GameObject[] foodTiles;
        public GameObject ammo1Tile;
        public GameObject ammo2Tile;
        public GameObject ammo3Tile;

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

            GameManager.instance.MakeGameMap(columns, rows);

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

			GameManager.instance.MakeGameMap (columns, rows);

			for(int x = -1; x < columns + 1; x++)
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

        void LayoutItemsAtRandom(GameObject tile, int minimum, int maximum)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                LayoutItemAtRandom(tile);
            }
        }

        void LayoutItemsAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            int objectCount = Random.Range(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {                
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                LayoutItemAtRandom(tileChoice);
            }
        }

        void LayoutItemAtRandom(GameObject tile)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject obj = Instantiate(tile, randomPosition, Quaternion.identity);
            GameObject objToShow = Instantiate(tile, randomPosition, Quaternion.identity);
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

        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum, bool addList = false)
		{
			int objectCount = Random.Range (minimum, maximum+1);
			
			for(int i = 0; i < objectCount; i++)
			{
				Vector3 randomPosition = RandomPosition();
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				GameObject obj = Instantiate(tileChoice, randomPosition, Quaternion.identity);

                if(addList) GameManager.instance.AddWall (obj);
			}
		}

        public void SetupScene (int level)
		{
			GameManager.instance.ClearFloors ();
			GameManager.instance.ClearWalls ();

			//BoardSetup ();
            BoardSetup2();

            InitialiseList ();

            LayoutItemsAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
            LayoutItemsAtRandom(ammo1Tile, ammo1Count.minimum, ammo1Count.maximum);
            LayoutItemsAtRandom(ammo2Tile, ammo2Count.minimum, ammo2Count.maximum);
            LayoutItemsAtRandom(ammo3Tile, ammo3Count.minimum, ammo3Count.maximum);

            LayoutObjectAtRandom(enemyTiles, enemiyCount.minimum, enemiyCount.maximum);			
		}
	}
}
