using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public enum WEAPON { W1, W2, W3, W4, W5, W6 };
    public class LevelEfx : MonoBehaviour
    {
        public GameObject[] bulletMInstances = new GameObject[20];
        public GameObject bulletMTile;
        public GameObject[] bulletPInstances = new GameObject[20];
        public GameObject bulletPTile;
        public GameObject[] bulletSInstances = new GameObject[20];
        public GameObject bulletSTile;

        public GameObject[] shotInstances = new GameObject[8];
        public GameObject shotTile;

        public GameObject explosionInstance;
        public GameObject explosionTile;

        int shotEffectIndex = 0;

        public Weapon GetWeapon(WEAPON index)
        {
            switch(index)
            {
                default:
                case WEAPON.W1: return new Weapon(2, 0.25f, 3, 0.2f, 20, "Speed Gun 1", 0, 0, 2);
                case WEAPON.W2: return new Weapon(4, 0.45f, 2, 0.2f, 10, "Power Gun 1", 0, 0, 3);
                case WEAPON.W3: return new Weapon(3, 0.25f, 3, 0.2f, 20, "Speed Gun A", 0, 1, 3);
                case WEAPON.W4: return new Weapon(5, 0.45f, 2, 0.2f, 10, "Power Gun A", 0, 1, 4);                
                case WEAPON.W5: return new Weapon(4, 0.25f, 3, 0.2f, 20, "Speed Gun S", 1, 2, 4);
                case WEAPON.W6: return new Weapon(6, 0.45f, 2, 0.2f, 10, "Power Gun S", 1, 2, 5);                
            }
        }

        public void Init()
        {
            for (int i = 0; i < shotInstances.Length; i++)
            {
                shotInstances[i] = Instantiate(shotTile, transform.position, Quaternion.identity);
                shotInstances[i].SetActive(false);
            }

            explosionInstance = Instantiate(explosionTile, transform.position, Quaternion.identity);
            explosionInstance.SetActive(false);

            for (int i = 0; i < bulletMInstances.Length; i++)
            {
                bulletMInstances[i] = Instantiate(bulletMTile, transform.position, Quaternion.identity);
                bulletMInstances[i].SetActive(false);
            }
            for (int i = 0; i < bulletSInstances.Length; i++)
            {
                bulletSInstances[i] = Instantiate(bulletSTile, transform.position, Quaternion.identity);
                bulletSInstances[i].SetActive(false);
            }
            for (int i = 0; i < bulletPInstances.Length; i++)
            {
                bulletPInstances[i] = Instantiate(bulletPTile, transform.position, Quaternion.identity);
                bulletPInstances[i].SetActive(false);
            }
        }

        public IEnumerator ShowExplosionEffect(Vector3 targetPos)
        {
            explosionInstance.transform.position = targetPos;
            explosionInstance.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            explosionInstance.SetActive(false);
        }

        int bulletMIndex = 0;
        int bulletSIndex = 0;
        int bulletPIndex = 0;

        public GameObject GetBullet(int type)
        {
            if (type == 0)
            {
                bulletSIndex++;
                if (bulletSIndex >= bulletSInstances.Length) bulletSIndex = 0;

                return bulletSInstances[bulletSIndex];
            }
            else if (type == 1)
            {
                bulletMIndex++;
                if (bulletMIndex >= bulletMInstances.Length) bulletMIndex = 0;

                return bulletMInstances[bulletSIndex];
            }
            else
            {
                bulletPIndex++;
                if (bulletPIndex >= bulletPInstances.Length) bulletPIndex = 0;

                return bulletPInstances[bulletSIndex];
            }

        }
                
        public IEnumerator ShowShotEffect(Vector3 targetPos, Weapon weapon)
        {
            GameObject shotInstance = shotInstances[shotEffectIndex];
            Animator anim = shotInstance.GetComponent<Animator>();
            if (anim)
            {
                anim.speed = weapon.shotAniSpeed;
            }

            shotEffectIndex++;
            if (shotEffectIndex >= shotInstances.Length) shotEffectIndex = 0;

            shotInstance.transform.position = targetPos;
            shotInstance.SetActive(true);
            yield return new WaitForSeconds(weapon.aniDelay);
            shotInstance.SetActive(false);
        }

    }
}