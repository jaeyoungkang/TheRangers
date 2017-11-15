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
        public int powerSupply = 0;

        public int reloadPower = 1;
        public int scopePower = 1;
        public int shotPower = 1;
        public int movePower = 1;

        public bool canMove = true;
        
        public bool startReload = false;
        public int indexReload = 0;

        List<int> storage = new List<int>();

        public int controlPower;
        public int controlPowerInit = 50;

        public int shield;
        public int shieldInit = 10;
        
        public float moveTime;
        public float moveTimeInit = 0.45f;

        public Weapon curWeapon;        

        public int scopeRange;
        public int scopeRangeInit = 2;

        public void ReadyToDeparture(Weapon weapon)
        {
            curWeapon = weapon;
            powerSupply = 0;
            for (int i = 0; i < numOfBullets.Length; i++)
            {
                numOfBullets[i] = 0;
            }

            for (int i = 0; i < totalBullets.Length; i++)
            {
                totalBullets[i] = 0;
            }

            shield = shieldInit;
            moveTime = moveTimeInit;
            controlPower = controlPowerInit;
            curWeapon.Init();            
            scopeRange = scopeRangeInit;
        }

        public void SetupStorage(int Ammo1, int Ammo2, int supply)
        {
            totalBullets[0] = Ammo1;
            totalBullets[1] = Ammo2;
            powerSupply = supply;
        }

        public void Move()
        {
            canMove = false;
            ConsumePower(movePower + scopePower);
        }

        public bool Shot(int input)
        {
            if (curWeapon.canShot == false) return false;

            if (numOfBullets[input] <= 0)
            {
                return false;
            }

            numOfBullets[input]--;
            ConsumePower(shotPower);
            curWeapon.canShot = false;
            return true;

        }

        public bool IsPowerDown()
        {
            return controlPower < 0;
        }

        public void UpdatePowerState()
        {
            if (controlPower < 0)
                ConsumePower(controlPower);
        }

        public void UpdateScope(int value)
        {
            if (value != 0) scopeRange = value;
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
                                
                ConsumePower(reloadPower);
            }
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
                moveTime = moveTimeInit;
                canMove = true;
            }
        }

        public void ChargePower()
        {
            if (powerSupply > 0)
            {
                powerSupply--;
                controlPower += 10;
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

        public void ConsumePower(int consume)
        {
//            controlPower -= consume;
        }

        public bool LowerPower()
        {
            return controlPower < controlPowerInit / 5;
        }        
    }
}