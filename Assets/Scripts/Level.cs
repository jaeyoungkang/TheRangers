using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    [System.Serializable]
    public class Level
    {
        public bool missionFinish;
        public int id;
        public int columns, rows;
        public string filePath;
        public int[,] mapOfUnits;
        public int[,] mapOfStructures;
        public int[,] mapOfItems;

        public List<GameObject> items = new List<GameObject>();
        public List<GameObject> tiles = new List<GameObject>();
        public List<Enemy> enemies = new List<Enemy>();

        public void AddEnemyToList(Enemy script)
        {
            enemies.Add(script);
        }

        public void AttackOtherPlayer(Vector3 targetPos)
        {
            int damage = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().myShip.curWeapon.weaponDamage;
            foreach (Enemy other in enemies)
            {
                if (targetPos == other.transform.position)
                {
                    other.LoseHP(damage);
                    return;
                }
            }
        }

        public void RemoveOtherPlayer(GameObject target)
        {
            Enemy en = target.GetComponent<Enemy>();
            enemies.Remove(en);
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

        public void Setup(int levelId)
        {
            Init();
            id = levelId;
            switch (id)
            {
                case 1:
                    filePath = "map01.txt";
                    break;

                case 2:
                    filePath = "map02.txt";
                    break;

                case 3:
                    filePath = "map03.txt";
                    break;
            }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            string[] symbols = lines[0].Split(',');
            columns = symbols.Length;
            rows = lines.Length;

            MakeGameMapOfUnits(columns, rows);
            MakeGameMapOfStructures(columns, rows);
            MakeGameMapOfItems(columns, rows);
        }

        public void Init()
        {
            missionFinish = false;
            filePath = "";            

            enemies.Clear();
            tiles.Clear();
            items.Clear();
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
            mapOfItems = new int[columns, rows];
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

        public void ReomveItem(Vector3 pos)
        {
            SetMapOfItems(pos, 0);
        }
    }
}