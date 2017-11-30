using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    [System.Serializable]
    public class SpaceShip
    {
        public int[] totalBullets = new int[3];

        public bool canMove = true;
        
        public bool startChangeWeapon = false;

        public int indexReload = 0;

        public int shield;
        public int[] shieldInits = new int[] { 0, 5, 10, 12, 14, 16, 18, 20, 40 };
        public int shieldLevel = 0;

        public float moveTime;
                                           // 0   1     2    3      4     5       6      7   
        public float[] speed = new float[] { 1f, 0,6f, 0.5f, 0.45f, 0.4f, 0.35f, 0.30f, 0.25f};
        public int speedLevel = 0;

        public float weaponChangeTime;
        public float[] weaponChange = new float[] {1f, 0.8f, 0.6f, 0.4f, 0.2f};
        public int weaponChangeLevel = 0;

        public Weapon curWeapon;

        public int scopeRange;
        public int scopeRangeInit = 2;

        public SpaceShip(int _shieldLevel, int _scopeRangeInit, int _speedLevel)
        {
            shieldLevel = _shieldLevel;
            scopeRangeInit = _scopeRangeInit;
            speedLevel = _speedLevel;
        }

        public void SetWeapon(Weapon weapon)
        {
            curWeapon = weapon;
            startChangeWeapon = true;            
        }

        public void InitWeaponAmmo(int index, int total)
        {
            totalBullets[index] = total;
        }

        public void WeaponChangeSpeedUp()
        {
            weaponChangeLevel++;
            if (weaponChangeLevel >= weaponChange.Length) weaponChangeLevel = weaponChange.Length;
        }

        public void SpeedUp()
        {
            speedLevel++;
            if (speedLevel >= speed.Length) speedLevel = speed.Length;
        }

        public void ShieldUp()
        {
            shieldLevel++;
            if (shieldLevel > shieldInits.Length) shieldLevel = shieldInits.Length;
            RestoreShield(shieldInits[shieldLevel]);
        }

        public void RestoreShield(int restore)
        {
            shield += restore;
            if (shield > shieldInits[shieldLevel]) shield = shieldInits[shieldLevel];
        }

        public void AddAmmo(int index, int ammoNum)
        {
            totalBullets[index] += ammoNum;
        }

        public void ReadyToDeparture(int totalBulletType0, int totalBulletType1, int totalBulletType2)
        {            
            InitWeaponAmmo(0, totalBulletType0);
            InitWeaponAmmo(1, totalBulletType1);
            InitWeaponAmmo(2, totalBulletType2);

            shield = shieldInits[shieldLevel];
            moveTime = speed[speedLevel];
            scopeRange = scopeRangeInit;
            weaponChangeTime = weaponChange[weaponChangeLevel];
        }

        public void Move()
        {
            canMove = false;
        }

        public bool Shot(int input)
        {
            if (curWeapon.canShot == false) return false;

            if (totalBullets[input] <= 0)
            {
                return false;
            }

            totalBullets[input]--;

            curWeapon.canShot = false;
            return true;
        }

		public void UpdateScope(int value)
        {
            if (value != 0 && value != 1) scopeRange = value;
            else scopeRange = scopeRangeInit;
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
                weaponChangeTime = weaponChange[weaponChangeLevel];
                startChangeWeapon = false;
            }
        }

        public void UpdateMoveCoolTime()
        {
            moveTime -= Time.deltaTime;
            if (moveTime <= 0)
            {
                moveTime = speed[speedLevel];
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