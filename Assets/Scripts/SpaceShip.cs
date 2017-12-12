using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    [System.Serializable]
    public class SpaceShip
    {
        public int shotPower = 0;
        public int[] shotPowerInit = new int[] { 20, 30, 40, 50};
        public int shotPowerLevel = 0;

        public bool canMove = true;
        
        public int indexReload = 0;
                                           // 0    1   2   3   4    5   
                                           //lv1  lv2 lv3 lv4 lv5
        public int[] shieldInits = new int[] { 5, 10, 15, 20, 25, 30};
        public int shieldLevel = 0;
        public int shield;

        public float moveTime;
                                           // 0   1     2    3      4     5       6      7   
        public float[] speed = new float[] { 1f, 0,6f, 0.5f, 0.45f, 0.4f, 0.35f, 0.30f, 0.25f};
        public int speedLevel = 0;

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
        }        

        public void ShotPowerUp()
        {
            shotPowerLevel++;
            if (shotPowerLevel >= shotPowerInit.Length) shotPowerLevel = shotPowerInit.Length - 1;
        }
        
        public void SpeedUp()
        {
            speedLevel++;
            if (speedLevel >= speed.Length - 2) speedLevel = speed.Length - 2;
        }

        public void ShieldUp()
        {
            shieldLevel++;
            if (shieldLevel >= shieldInits.Length - 2) shieldLevel = shieldInits.Length - 2;
        }

        public void RestoreShield(int restore)
        {
            shield += restore;
            if (shield > shieldInits[shieldLevel]) shield = shieldInits[shieldLevel];
        }

        public void AddPower(int addPower)
        {
            shotPower += addPower;
            if (shotPower > shotPowerInit[shotPowerLevel]) shotPower = shotPowerInit[shotPowerLevel];
        }

        public void ReadyToDeparture()
        {
            shotPower = shotPowerInit[shotPowerLevel];
            shield = shieldInits[shieldLevel];
            moveTime = speed[speedLevel];
            scopeRange = scopeRangeInit;
        }

        public void Move()
        {
            canMove = false;
        }

        public bool Shot(int consume)
        {
            if (curWeapon.canShot == false) return false;

            if (shotPower <= 0)
            {
                return false;
            }

            shotPower -= consume;

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