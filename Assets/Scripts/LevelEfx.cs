using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public enum WEAPON { WS1, WS2, WS3, WS4, WN1, WN2, WN3, WN4, WP1, WP2, WP3, WP4 };
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
                case WEAPON.WS1: return new Weapon(2, 0.3f, 3, 0.2f, 20, "Speed Gun 1", 0);
                case WEAPON.WS2: return new Weapon(3, 0.35f, 3, 0.2f, 20, "Speed Gun 2", 0);
                case WEAPON.WS3: return new Weapon(3, 0.3f, 3, 0.2f, 20, "Speed Gun A", 0);
                case WEAPON.WS4: return new Weapon(3, 0.25f, 3, 0.2f, 25, "Speed Gun S", 0);
                default:
                case WEAPON.WN1: return new Weapon(4, 0.6f, 2, 0.3f, 10, "Normal Gun 1", 1);
                case WEAPON.WN2: return new Weapon(3, 0.45f, 2, 0.3f, 15, "Normal Gun 2", 1);
                case WEAPON.WN3: return new Weapon(5, 0.6f, 2, 0.3f, 10, "Normal Gun A", 1);
                case WEAPON.WN4: return new Weapon(5, 0.5f, 2, 0.3f, 15, "Normal Gun S", 1);

                case WEAPON.WP1: return new Weapon(12, 1f, 1, 0.5f, 6, "Power Gun 1", 2);
                case WEAPON.WP2: return new Weapon(10, 0.8f, 1, 0.5f, 8, "Power Gun 2", 2);
                case WEAPON.WP3: return new Weapon(12, 0.8f, 1, 0.5f, 8, "Power Gun A", 2);
                case WEAPON.WP4: return new Weapon(15, 0.7f, 1, 0.5f, 10, "Power Gun S", 2);
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