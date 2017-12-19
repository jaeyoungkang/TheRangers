using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public enum WEAPON { W1, W2, W3, W4, W5, W6 };
    public class bulletInfo
    {
        public int damage;
        public float speed;
        public GameObject bullet;
        public Vector3 targetPos = new Vector3();
    }


    [System.Serializable]
    public class Weapon
    {
        public int consume = 1;
        public int grade;
        public int bType;
        public int bulletSpeed = 30;

        public int weaponDamage = 4;

        public bool canShot = true;

        public float shotTime;
        public float shotTimeInit = 1f;

        public string name;

        public Weapon(int _damage, float _shotTime, int _bulletSpeed, string _name, int _bType, int _grade, int _consume)
        {
            weaponDamage = _damage;
            shotTimeInit = _shotTime;
            bulletSpeed = _bulletSpeed;
            name = _name;
            bType = _bType;
            grade = _grade;
            consume = _consume;
            Init();
        }

        public void Init()
        {
            shotTime = shotTimeInit;
        }

        public void ShotTimeUp(float addSpeed)
        {
            shotTimeInit -= addSpeed;
            Init();
        }

        public void UpdateWeaponCooling()
        {
            shotTime -= Time.deltaTime;
            if (shotTime <= 0)
            {
                shotTime = shotTimeInit;
                canShot = true;
            }
        }
    }


    public class EffectManager : MonoBehaviour
    {
        public static EffectManager instance = null;

        public class TEXT_EFX_INFO
        {
            public GameObject obj;
            public Vector3 startPos;
        }
        public List<TEXT_EFX_INFO> textEfxs = new List<TEXT_EFX_INFO>();

        public GameObject textEfxA;
        public GameObject textEfxB;
        public GameObject textEfxC;
        Dictionary<int, List<GameObject>> textEfxPool = new Dictionary<int, List<GameObject>>();

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
        
        List<bulletInfo> bullets = new List<bulletInfo>();

        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            Init();
        }

        public Weapon GetWeapon(WEAPON index)
        {
            switch(index)
            {
                default:
                case WEAPON.W1: return new Weapon(10, 0.25f, 9, "Speed Gun 1", 0, 0, 2);
                case WEAPON.W2: return new Weapon(20, 0.45f, 6, "Power Gun 1", 1, 0, 3);
                case WEAPON.W3: return new Weapon(20, 0.25f, 9, "Speed Gun A", 0, 1, 3);
                case WEAPON.W4: return new Weapon(30, 0.45f, 6, "Power Gun A", 1, 1, 4);                
                case WEAPON.W5: return new Weapon(30, 0.25f, 9, "Speed Gun S", 0, 2, 4);
                case WEAPON.W6: return new Weapon(40, 0.45f, 6, "Power Gun S", 1, 2, 5);                
            }
        }

        public void FireBullet(bulletInfo  bInfo)
        {
            bullets.Add(bInfo);            
        }

        public void Update()
        {
            if (GameManager.instance.doingSetup) return;

            foreach (bulletInfo bInfo in bullets)
            {
                Vector3 bulletPos = bInfo.bullet.transform.position;
                Vector3 moveDir = bInfo.targetPos - bulletPos;
                float length = moveDir.magnitude;
                moveDir.Normalize();                
                bulletPos += (moveDir * Time.deltaTime * bInfo.speed);
                bInfo.bullet.transform.position = bulletPos;

                if (length < 0.2f)
                {
                    Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
                    
                    if (bInfo.targetPos == player.transform.position)
                    {
                        StartCoroutine(ShowShotEffect(bInfo.targetPos));
                        player.LoseHP(bInfo.damage);
                    }

                    bInfo.bullet.SetActive(false);
                }
            }

            bullets.RemoveAll(item => item.bullet.activeSelf == false);

            UpdateTextEfxs();
        }

        public void UpdateTextEfxs()
        {
            Vector3 up = new Vector3(0, 2, 0);
            up *= Time.deltaTime;
            foreach (TEXT_EFX_INFO info in textEfxs)
            {
                float acc = (info.startPos.y+1.5f) - info.obj.transform.position.y;
                up *= acc;

                info.obj.transform.position += up;
                if (info.obj.transform.position.y - info.startPos.y >= 1.0f)
                {
                    info.obj.SetActive(false);
                }
            }

            textEfxs.RemoveAll(item => item.obj.activeSelf == false);
        }

        int[] efxIterators = new int[3] { 0, 0, 0 };
        public Sprite[] efxIcons;
        public Sprite[] efxTexts;

        GameObject GetTextEfx(int type)
        {
            efxIterators[type]++;
            if (efxIterators[type] == 10) efxIterators[type] = 0;

            return textEfxPool[type][efxIterators[type]];
        }

        public void MakeTextEfxPool()
        {
            textEfxA.SetActive(false);
            textEfxB.SetActive(false);
            textEfxC.SetActive(false);

            List<GameObject> textEfxAList = new List<GameObject>();
            List<GameObject> textEfxBList = new List<GameObject>();
            List<GameObject> textEfxCList = new List<GameObject>();
            for (int i = 0; i < 10; i++)
            {
                textEfxAList.Add(Instantiate(textEfxA));
                textEfxBList.Add(Instantiate(textEfxB));
                textEfxCList.Add(Instantiate(textEfxC));
            }

            textEfxPool.Clear();
            textEfxPool.Add(0, textEfxAList);
            textEfxPool.Add(1, textEfxBList);
            textEfxPool.Add(2, textEfxCList);
        }

        public void ShowTextEfx(int type, int number, Vector3 targetPos)
        {
            GameObject textEfx = GetTextEfx(type);
            textEfx.transform.position = targetPos + new Vector3(0, 0.3f, 0);
            textEfx.SetActive(true);

            GameObject efxText = textEfx.transform.Find("TextImg").gameObject;
            GameObject efxIcon = textEfx.transform.Find("Icon").gameObject;

            if (type == 0 || type == 1)
            {
                Sprite icon = efxIcons[type];
                efxIcon.GetComponent<SpriteRenderer>().sprite = icon;
            }

            if (type == 1 || type == 2)
            {
                Sprite numberImg = null;
                switch (number)
                {
                    case 1: numberImg = efxTexts[0]; break;
                    case 10: numberImg = efxTexts[1]; break;
                    case 40: numberImg = efxTexts[2]; break;
                    case 100: numberImg = efxTexts[3]; break;
                    case -10: numberImg = efxTexts[4]; break;
                    case -20: numberImg = efxTexts[5]; break;
                    case -30: numberImg = efxTexts[6]; break;
                    case -40: numberImg = efxTexts[7]; break;

                }
                efxText.GetComponent<SpriteRenderer>().sprite = numberImg;
            }

            TEXT_EFX_INFO info = new TEXT_EFX_INFO();
            info.obj = textEfx;
            info.startPos = targetPos;
            textEfxs.Add(info);
        }

        public void Init()
        {
            for (int i = 0; i < shotInstances.Length; i++)
            {
                shotInstances[i] = Instantiate(shotTile);
                shotInstances[i].SetActive(false);
            }

            explosionInstance = Instantiate(explosionTile);
            explosionInstance.SetActive(false);

            for (int i = 0; i < bulletMInstances.Length; i++)
            {
                bulletMInstances[i] = Instantiate(bulletMTile);
                bulletMInstances[i].SetActive(false);
            }
            for (int i = 0; i < bulletSInstances.Length; i++)
            {
                bulletSInstances[i] = Instantiate(bulletSTile);
                bulletSInstances[i].SetActive(false);
            }
            for (int i = 0; i < bulletPInstances.Length; i++)
            {
                bulletPInstances[i] = Instantiate(bulletPTile);
                bulletPInstances[i].SetActive(false);
            }

            MakeTextEfxPool();
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
                
        public IEnumerator ShowShotEffect(Vector3 targetPos)
        {
            shotEffectIndex++;
            if (shotEffectIndex >= shotInstances.Length) shotEffectIndex = 0;
            GameObject shotInstance = shotInstances[shotEffectIndex];

            shotInstance.transform.position = targetPos;
            shotInstance.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
}