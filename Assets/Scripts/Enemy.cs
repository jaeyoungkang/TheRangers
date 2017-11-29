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
        public List<Weapon> myWeapons = new List<Weapon>();

        Player player;

        class bulletInfo
        {
            public GameObject bullet;
            public Vector3 targetPos = new Vector3();
        }

        List<bulletInfo> bullets = new List<bulletInfo>();

        protected override void OnCantMove<T>(T component)
        {

        }

        protected override void Start()
        {
            base.Start();
            myWeapons.Clear();
            myWeapons.Add(GameManager.instance.lvEfx.GetWeapon(1));
            myWeapons.Add(GameManager.instance.lvEfx.GetWeapon(2));
            myWeapons.Add(GameManager.instance.lvEfx.GetWeapon(3));

            if (type == 0)
            {
                myShip = new SpaceShip(1, 2, 6);
                myShip.ReadyToDeparture(50, 0 ,0 );
                myShip.SetWeapon(myWeapons[0]);
            }
            else if (type == 1)
            {
                myShip = new SpaceShip(2, 2, 4);
                myShip.ReadyToDeparture(0, 50, 0);
                myShip.SetWeapon(myWeapons[1]);
            }
            else if (type == 2)
            {
                myShip = new SpaceShip(3, 3, 0);
                myShip.ReadyToDeparture(0, 0, 50);
                myShip.SetWeapon(myWeapons[2]);
            }
            else if (type == 3)
            {
                myShip = new SpaceShip(4, 5, 1);
                myShip.ReadyToDeparture(0, 0, 50);
                myShip.SetWeapon(myWeapons[2]);
            }

            GameManager.instance.curLevel.AddEnemyToList(this);
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Update()
        {
            if (GameManager.instance.doingSetup) return;

            int value = GameManager.instance.curLevel.GetMapOfStructures(transform.position);
            myShip.UpdateScope(value);

            List<bulletInfo> deleteBullet = new List<bulletInfo>();
            foreach(bulletInfo bInfo in bullets)
            {
                Vector3 bulletPos = bInfo.bullet.transform.position;
                Vector3 moveDir = bInfo.targetPos - bulletPos;
                float length = moveDir.sqrMagnitude;
                moveDir.Normalize();
                bulletPos += (moveDir * Time.deltaTime * myShip.curWeapon.bulletSpeed);
                bInfo.bullet.transform.position = bulletPos;

                if (length < 0.1f)
                {
                    bInfo.bullet.SetActive(false);
                    if (bInfo.targetPos == player.transform.position)
                    {
                        StartCoroutine(GameManager.instance.lvEfx.ShowShotEffect(bInfo.targetPos, myShip.curWeapon));
                        player.LoseHP(myShip.curWeapon.weaponDamage);
                    }
                    deleteBullet.Add(bInfo);
                }
            }

            foreach (bulletInfo bInfo in deleteBullet)
            {
                bullets.Remove(bInfo);
            }

            if (myShip.canMove == false)
            {
                myShip.UpdateMoveCoolTime();
            }
            else
            {
                AutoMove();
            }

            if (!myShip.curWeapon.canShot)
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
            if (type == 2)
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
                if (myShip.curWeapon.canShot)
                {                    
                    myShip.curWeapon.canShot = false;
                    FireBullet(player.transform.position);                    
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
        
        public void FireBullet(Vector3 targetPos)
        {
            int rValue = Random.Range(0, 3);
            if (rValue == 0)
            {
                bulletInfo bInfo = new bulletInfo();
                bInfo.bullet = GameManager.instance.lvEfx.GetBullet(myShip.curWeapon.bType);
                bInfo.bullet.SetActive(true);
                bInfo.bullet.transform.position = transform.position;
                bInfo.targetPos = targetPos;
                bullets.Add(bInfo);
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
                    myShip.curWeapon.canShot = false;
                    FireBullet(player.transform.position);
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
                    myShip.curWeapon.canShot = false;
                    FireBullet(player.transform.position);
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

                ChangeDir(xDir, yDir);
                UpdateDirImage();
                myShip.Move();
            }
        }

        private void Destoryed()
        {
            GameManager.instance.DestroyOtherPlayer(gameObject, type);
            GameManager.instance.curLevel.SetMapOfUnits(gameObject.transform.position, 0);
        }
        
    }
}