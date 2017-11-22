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

            LayoutStructuresByFile(curLevel.filePath);            
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

        void LayoutItemById(Vector3 pos, int itemId)
        {
            // LayoutItem(itemTiles[itemId], pos);
        }

        void LayoutUnitById(Vector3 pos, int unitId)
        {
            Instantiate(enemyTiles[unitId - 1], pos, Quaternion.identity);
            GameManager.instance.curLevel.SetMapOfUnits(pos, unitId);         
        }

        void LayoutStructuresByFile(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            
            int x = 0;
            int y = lines.Length;
            foreach(string str in lines)
            {
                y--;
                x = 0;
                string[] symbols = str.Split(',');
                foreach(string s in symbols)
                {
                    Vector3 pos = new Vector3(x, y, 0);
                    int value = Int32.Parse(s);
                    int structureId = value % 10;
                    int itemId = (value / 10) % 10;
                    int unitId = value / 100;
                    if (structureId != 0)
                        LayoutStructure(pos, structureId);
                    if(itemId != 0)
                        LayoutItemById(pos, itemId);
                    if (unitId != 0)
                        LayoutUnitById(pos, unitId);
                    x++;                               
                }                
            }

            Vector3 exitPos = new Vector3();            
            while(true)
            {
                exitPos.x = Random.Range(1, columns);
                exitPos.y = Random.Range(1, rows);
                if(GameManager.instance.curLevel.GetMapOfStructures(exitPos) == 0)
                    break;
            }           

            Instantiate(exit, exitPos, Quaternion.identity);
        }

    }
}
