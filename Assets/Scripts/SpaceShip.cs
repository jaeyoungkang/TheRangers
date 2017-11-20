using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public class SpaceShip
    {
        public int storageSize = 5;

        public int[] numOfBullets = new int[3];
        public int[] totalBullets = new int[3];

        public bool canMove = true;
        
        public bool startReload = false;
        public bool startChangeWeapon = false;

        public int indexReload = 0;

        List<int> storage = new List<int>();

        public int shield;
        public int shieldInit = 10;
        
        public float moveTime;
        public float moveTimeInit = 0.4f;

        public float weaponChangeTime;
        public float weaponChangeTimeInit = 1f;

        public Weapon curWeapon;

        public int scopeRange;
        public int scopeRangeInit = 2;

        public SpaceShip(int _shieldInit, float _moveTimeInit, int _scopeRangeInit)
        {
            shieldInit = _shieldInit;
            moveTimeInit = _moveTimeInit;
            scopeRangeInit = _scopeRangeInit;
        }

        public void SetWeapon(Weapon weapon)
        {
            if(curWeapon  != null)
            {
                totalBullets[curWeapon.bType] = totalBullets[curWeapon.bType] + numOfBullets[curWeapon.bType];
                numOfBullets[curWeapon.bType] = 0;
            }                

            curWeapon = weapon;
            startChangeWeapon = true;            
        }

        public void InitWeaponAmmo(int index, int total)
        {
            numOfBullets[index] = 0;
            totalBullets[index] = total;
        }

        public void ReadyToDeparture(Weapon weapon, int totalBulletType0, int totalBulletType1, int totalBulletType2)
        {            
            for (int i = 0; i < numOfBullets.Length; i++)
            {
                numOfBullets[i] = 0;
            }

            for (int i = 0; i < totalBullets.Length; i++)
            {
                totalBullets[i] = 0;
            }
            InitWeaponAmmo(0, totalBulletType0);
            InitWeaponAmmo(1, totalBulletType1);
            InitWeaponAmmo(2, totalBulletType2);

            shield = shieldInit;
            moveTime = moveTimeInit;            
            scopeRange = scopeRangeInit;
            weaponChangeTime = weaponChangeTimeInit;
            SetWeapon(weapon);
        }

        public void Move()
        {
            canMove = false;
        }

        public bool Shot(int input)
        {
            if (curWeapon.canShot == false) return false;

            if (numOfBullets[input] <= 0)
            {
                return false;
            }

            numOfBullets[input]--;
            curWeapon.canShot = false;
            return true;
        }

		public void UpdateScope(int value)
        {
            if (value != 0 && value != 1) scopeRange = value;
            else scopeRange = scopeRangeInit;
        }

        public void Reload(int index)
        {
            startReload = true;
            indexReload = index;
        }

        public void UpdateReload()
        {
            curWeapon.UpdateReload();
            
            if (curWeapon.reloadTime <= 0)
            {
                curWeapon.reloadTime = curWeapon.reloadTimeInit;
                startReload = false;
                
                int relaodNum = numOfBullets[indexReload];
                int maxReloadAmmo = curWeapon.capability - numOfBullets[indexReload];

                if (totalBullets[indexReload] >= maxReloadAmmo) relaodNum = maxReloadAmmo;

                totalBullets[indexReload] -= relaodNum;
                numOfBullets[indexReload] += relaodNum;
            }
        }

        public void UpdateWeaponCooling()
        {
            curWeapon.UpdateWeaponCooling();            
        }

        public void UpdateWeaponChangeTime()
        {
            weaponChangeTime -= Time.deltaTime;
            if (weaponChangeTime <= 0)
            {
                weaponChangeTime = weaponChangeTimeInit;
                startChangeWeapon = false;
            }
        }

        public void UpdateMoveCoolTime()
        {
            moveTime -= Time.deltaTime;
            if (moveTime <= 0)
            {
                moveTime = moveTimeInit;
                canMove = true;
            }
        }

        public bool Shield()
        {
            return shield > 0;
        }

        public void Damaged(int damage)
        {
            shield -= damage;
        }
    }
}