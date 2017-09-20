﻿using UnityEngine;
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
        public Count enemiyCount = new Count(1, 5);
        public GameObject exit;											
		public GameObject[] floorTiles;									
		public GameObject[] wallTiles;									
		public GameObject[] foodTiles;									
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
		
		
		void BoardSetup ()
		{
			boardHolder = new GameObject ("Board").transform;
			
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

			BoardSetup ();
			InitialiseList ();
			
			LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum, true);
			LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);			
            LayoutObjectAtRandom(enemyTiles, enemiyCount.minimum, enemiyCount.maximum);			
		}
	}
}
