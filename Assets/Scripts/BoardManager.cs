using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed	
{
    public class BoardManager : MonoBehaviour
	{
		public int columns = 5;
		public int rows = 5;        
        public GameObject exit;
        public GameObject shop;
        public GameObject[] floorTiles;									
		public GameObject[] itemTiles;
        
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

        void BoardSetup (Level curLevel)
		{
			boardHolder = new GameObject ("Board").transform;
            rows = curLevel.rows;
            columns = curLevel.columns;

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
                    GameManager.instance.curLevel.AddFloor (instance);
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

        void LayoutItem(GameObject tile, Vector3 pos)
        {
            GameObject obj = Instantiate(tile, pos, Quaternion.identity);
            GameManager.instance.curLevel.AddItem(obj);
        }       
 
        public void SetupScene (Level curLevel)
		{
			BoardSetup (curLevel);

            InitialiseList ();

            LayoutStructuresRandomly(curLevel.structureInfo);
            LayoutResourceItemsRandomly(curLevel.missionItemCount+1, curLevel.resourceInfo);
            LayoutEnemiesRandomly(curLevel.enemyInfo);

            Vector3 randomPos = GetRandomPosRefMap();
            Instantiate(exit, randomPos, Quaternion.identity);

            int shopNum = 1;
            if (curLevel.id == 10) shopNum = 4;
            for (int i =0;i< shopNum; i++)
            {
                randomPos = GetRandomPosRefMap();
                GameManager.instance.LayoutShop(randomPos);
            }

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

            GameManager.instance.curLevel.SetMapOfStructures(pos, range);
        }

        void LayoutStructuresRandomly(Dictionary<int, int> sInfos)
        {
            foreach (KeyValuePair<int, int> sInfo in sInfos)
            {
                for (int i = 0; i < sInfo.Value; i++)
                {
                    Vector3 rPos = GetRandomPosRefMap();
                    LayoutStructure(rPos, sInfo.Key);
                }
            }
        }

        void LayoutEnemiesRandomly(Dictionary<int, int> eInfos)
        {
            foreach (KeyValuePair<int,int> eInfo in eInfos)
            {                                
                int count = eInfo.Value + Random.Range(0, 2);
                for (int i = 0; i < count; i++)
                {
                    Vector3 rPos = GetRandomPosRefMap();
                    if (rPos.x < 3 && rPos.y < 3) continue;
                    LayoutUnitById(rPos, eInfo.Key);
                }
            }
        }

        void LayoutResourceItemsRandomly(int missionItemCount, Dictionary<int, int> rInfos)
        {
            foreach (KeyValuePair<int, int> rInfo in rInfos)
            {
                int maxCount = 0;
                if(rInfo.Key == 0) maxCount = 4;
                else if (rInfo.Key == 1) maxCount = 2;
                else if (rInfo.Key == 2) maxCount = 1;

                int count = rInfo.Value + Random.Range(0, maxCount);
                for (int i = 0; i < count; i++)
                {
                    Vector3 rPos = GetRandomPosRefMap();
                    LayoutItemById(rPos, rInfo.Key);
                }                
            }

            for (int i = 0; i < missionItemCount; i++)
            {
                Vector3 rPos = GetRandomPosRefMap();
                LayoutItemById(rPos, 1);
            }
        }

        void LayoutItemById(Vector3 pos, int itemId)
        {
            LayoutItem(itemTiles[itemId-1], pos);
        }

        void LayoutUnitById(Vector3 pos, int unitId)
        {
            Instantiate(enemyTiles[unitId], pos, Quaternion.identity);
            GameManager.instance.curLevel.SetMapOfUnits(pos, unitId+10);         
        }

        Vector3 GetRandomPosRefMap()
        {
            Vector3 rPos = new Vector3();
            while (true)
            {
                rPos = RandomPosition();
                if (GameManager.instance.curLevel.GetMapOfStructures(rPos) == 0)
                    break;
            }            

            return rPos;
        }

    }
}
