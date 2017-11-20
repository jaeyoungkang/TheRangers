using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public class Enemy : MovingObject
    {
        public int type = 0;
        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public SpaceShip myShip;
        public float weaponRangeMin = 0;
        public float weaponRangeMax = 3;
        private GameObject curBullet;
        public Weapon weaponS = new Weapon(2, 0.2f, 1.5f, 3, 0.2f, 10);
        bool shoting = false;
        Vector3 enemyPos = new Vector3();
        float bulletSpeed = 30f;
        public GameObject bulletS;

        protected override void OnCantMove<T>(T component)
        {

        }

        protected override void Start()
        {
            base.Start();

            myShip = new SpaceShip();
            myShip.ReadyToDeparture(weaponS);
            curBullet = bulletS;

            GameManager.instance.curLevel.AddEnemyToList(this);

            Renderer renderer = gameObject.GetComponent<SpriteRenderer>();

            if (type == 1) renderer.sortingLayerName = "Player";
            else renderer.sortingLayerName = "Enemy";

        }

        void Update()
        {
            if (GameManager.instance.doingSetup) return;

            if (shoting)
            {
                Vector3 bulletPos = curBullet.transform.position;
                Vector3 moveDir = enemyPos - bulletPos;
                float length = moveDir.sqrMagnitude;
                moveDir.Normalize();
                bulletPos += (moveDir * Time.deltaTime * bulletSpeed);
                curBullet.transform.position = bulletPos;

                if (length < 0.1f)
                {
                    shoting = false;
                    curBullet.SetActive(false);
                }
            }

            if (myShip.canMove == false)
            {
                myShip.UpdateMoveCoolTime();
            }
            else
            {
                AutoMove();
            }

            if (myShip.startReload)
            {
                myShip.UpdateReload();
            }
            else if (!myShip.curWeapon.canShot)
            {
                myShip.UpdateWeaponCooling();
            }           
            
        }

        public void LoseHP(int damage)
        {
            if (myShip.Shield()) myShip.Damaged(damage);
            else Destoryed();

            if (myShip.shield < 0) Destoryed();
        }

        private Vector3 targetPos = Vector3.zero;
        public void AutoMove()
        {
            if (type == 1)
            {
                AutoMoveUnit01();
                return;
            }
            else if (type == 3)
            {
                AutoMoveUnit03();
                return;
            }

            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, myShip.scopeRange);
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            bool found = false;
            foreach (Vector3 pos in showRange)
            {
                if (GameManager.instance.curLevel.GetMapOfStructures(pos) == 1) continue;
                if (player.transform.position == pos)
                {
                    found = true;
                    targetPos = pos;
                    break;
                }
            }

            if (found)
            {
                float deltaX = Mathf.Abs(transform.position.x - player.transform.position.x);
                float deltaY = Mathf.Abs(transform.position.y - player.transform.position.y);

                bool inRange = false;
                if (deltaX + deltaY <= weaponRangeMax && deltaX + deltaY >= weaponRangeMin)
                {
                    inRange = true;
                }

                if (inRange)
                {
                    if (myShip.curWeapon.canShot)
                    {
                        StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position, myShip.curWeapon));
                        player.LoseHP(myShip.curWeapon.weaponDamage);
                        myShip.curWeapon.canShot = false;
                        if (shoting == false)
                        {
                            curBullet = GameManager.instance.GetBullet();
                            curBullet.SetActive(true);
                            curBullet.transform.position = transform.position;
                            shoting = true;
                            enemyPos = player.transform.position;
                            bulletSpeed = 30f;
                        }
                    }
                }
                else
                {
                    int xDir = 0;
                    int yDir = 0;
                    if (deltaX < deltaY)
                    {
                        xDir = player.transform.position.x - transform.position.x < Mathf.Epsilon ? -1 : 1;
                    }
                    else
                    {
                        yDir = player.transform.position.y - transform.position.y < Mathf.Epsilon ? -1 : 1;
                    }

                    AttemptMove<Player>(xDir, yDir);
                }
            }
            else if (targetPos != Vector3.zero)
            {
                float deltaX = Mathf.Abs(transform.position.x - targetPos.x);
                float deltaY = Mathf.Abs(transform.position.y - targetPos.y);
                int xDir = 0;
                int yDir = 0;
                if (deltaX < deltaY)
                {
                    xDir = targetPos.x - transform.position.x < Mathf.Epsilon ? -1 : 1;
                }
                else
                {
                    yDir = targetPos.y - transform.position.y < Mathf.Epsilon ? -1 : 1;
                }

                AttemptMove<Player>(xDir, yDir);
                if (deltaX + deltaY <= 2 + Mathf.Epsilon) targetPos = Vector3.zero;
            }
            else
            {
                int xDir = 0;
                int yDir = 0;

                int rand = Random.Range(0, 3);

                if (rand == 0)
                {
                    xDir = Random.Range(0, 2) == 0 ? 1 : -1;
                }
                else if (rand == 1)
                {
                    yDir = Random.Range(0, 2) == 0 ? 1 : -1;
                }
                AttemptMove<Player>(xDir, yDir);
            }
        }

        public bool CheckDirChanged(int xDir, int yDir)
        {
            bool change = true;
            switch (curDir)
            {
                case MOVE_DIR.LEFT:
                    if (xDir == -1) change = false;
                    break;

                case MOVE_DIR.RIGHT:
                    if (xDir == 1) change = false;
                    break;

                case MOVE_DIR.UP:
                    if (yDir == 1) change = false;
                    break;

                case MOVE_DIR.DOWN:
                    if (yDir == -1) change = false;
                    break;

            }

            return change;
        }

        public void ChangeDir(int xDir, int yDir)
        {
            MOVE_DIR prevDir = curDir;
            if (xDir == 1)
            {
                curDir = MOVE_DIR.RIGHT;

            }
            else if (xDir == -1)
            {
                curDir = MOVE_DIR.LEFT;
            }

            if (yDir == 1)
            {
                curDir = MOVE_DIR.UP;

            }
            else if (yDir == -1)
            {
                curDir = MOVE_DIR.DOWN;
            }

        }

        public void UpdateDirImage()
        {
            Vector3 rotation = new Vector3();
            switch (curDir)
            {
                case MOVE_DIR.RIGHT: rotation.z = -90; break;
                case MOVE_DIR.LEFT: rotation.z = 90; break;
                case MOVE_DIR.UP: rotation.z = 0; break;
                case MOVE_DIR.DOWN: rotation.z = 180; break;
            }
            transform.localEulerAngles = rotation;
        }


        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            if (myShip.canMove == false) return;

            myShip.Move();

            if (CheckDirChanged(xDir, yDir))
            {
                ChangeDir(xDir, yDir);
                UpdateDirImage();
                return;
            }

            Vector3 nextPos = transform.position;
            nextPos.x += xDir;
            nextPos.y += yDir;

            if (GameManager.instance.curLevel.GetMapOfUnits(nextPos) == 1)
                return;

            base.AttemptMove<T>(xDir, yDir);
            RaycastHit2D hit;
            if (Move(xDir, yDir, out hit))
            {
                GameManager.instance.curLevel.SetMapOfUnits(transform.position, 0);
                GameManager.instance.curLevel.SetMapOfUnits(nextPos, 1);
            }
        }


        public void AutoMoveUnit03()
        {
            List<Vector3> showRange = new List<Vector3>();
            Vector3 range = transform.position;
            range.x -= 1;
            showRange.Add(range);
            range.x -= 1;
            showRange.Add(range);
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            bool found = false;
            foreach (Vector3 pos in showRange)
            {
                if (GameManager.instance.curLevel.GetMapOfStructures(pos) == 1) continue;
                if (player.transform.position == pos)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (myShip.curWeapon.canShot)
                {
                    StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position, myShip.curWeapon));
                    player.LoseHP(myShip.curWeapon.weaponDamage);
                    myShip.curWeapon.canShot = false;
                }
            }
            else
            {
                if (transform.position.x > 0)
                {
                    int xDir = Random.Range(-1, 1);
                    int yDir = 0;

                    AttemptMove<Player>(xDir, yDir);
                }
                else
                {
                    Vector3 pos = transform.position;
                    pos.x = 19;
                    transform.position = pos;
                }

            }
        }

        public void AutoMoveUnit01()
        {
            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, myShip.scopeRange);
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            bool found = false;
            foreach (Vector3 pos in showRange)
            {
                if (GameManager.instance.curLevel.GetMapOfStructures(pos) == 1) continue;
                if (player.transform.position == pos)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (myShip.Shot(0))
                {
                    StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position, myShip.curWeapon));
                    player.LoseHP(myShip.curWeapon.weaponDamage);
                }
            }
            else
            {
                int xDir = 0;
                int yDir = 0;
                switch (curDir)
                {
                    case MOVE_DIR.LEFT: yDir = 1; break;
                    case MOVE_DIR.RIGHT: yDir = -1; break;
                    case MOVE_DIR.UP: xDir = 1; break;
                    case MOVE_DIR.DOWN: xDir = -1; break;
                }

//                ChangeDir(xDir, yDir);
//                UpdateDirImage();
                myShip.Move();
            }
        }

        private void Destoryed()
        {
            GameManager.instance.DestroyOtherPlayer(gameObject);
            GameManager.instance.curLevel.SetMapOfUnits(gameObject.transform.position, 0);
        }
        
    }
}