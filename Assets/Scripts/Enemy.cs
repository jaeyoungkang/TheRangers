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

        Player player;
        public GameObject shield;


        protected override void OnCantMove<T>(T component)
        {

        }

        public System.Collections.IEnumerator ShieldEffect(SpaceShip myShip)
        {
            Color sColor = shield.GetComponent<SpriteRenderer>().color;
            sColor.a = 0.8f;
            shield.GetComponent<SpriteRenderer>().color = sColor;
            float shiledRate = (float)myShip.shield / (float)myShip.shieldInits[myShip.shieldLevel];
            yield return new WaitForSeconds(0.2f);
            
            if (shiledRate > 0.8f) sColor.a = 0.35f;
            else if (shiledRate > 0.5f) sColor.a = 0.25f;
            else if (shiledRate > 0.2f) sColor.a = 0.15f;
            else sColor.a = 0.0f;

            shield.GetComponent<SpriteRenderer>().color = sColor;

        }

        protected override void Start()
        {
            base.Start();           

            if (type == 0)
            {
                myShip = new SpaceShip(0, 2, 5);
                myShip.ReadyToDeparture();
                myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W1));
            }
            else if (type == 1)
            {
                myShip = new SpaceShip(1, 2, 4);
                myShip.ReadyToDeparture();
                myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W3));
            }
            else if (type == 2)
            {
                myShip = new SpaceShip(2, 3, 0);
                myShip.ReadyToDeparture();
                myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W2));
            }
            else if (type == 3)
            {
                myShip = new SpaceShip(3, 5, 1);
                myShip.ReadyToDeparture();
                myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W4));
            }

            GameManager.instance.curLevel.AddEnemyToList(this);
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        void Update()
        {
            if (GameManager.instance.doingSetup) return;

            int value = GameManager.instance.curLevel.GetMapOfStructures(transform.position);
            myShip.UpdateScope(value);
                       

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
            GameManager.instance.lvEfx.ShowTextEfx(2, -damage, transform.position);
            if (myShip.Shield())
            {
                myShip.Damaged(damage);
                StartCoroutine(ShieldEffect(myShip));
            }
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
                Atttack();
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

            if (GameManager.instance.curLevel.GetMapOfUnits(nextPos) != 0)
                return;

            base.AttemptMove<T>(xDir, yDir);
            RaycastHit2D hit;
            if (Move(xDir, yDir, out hit))
            {
                GameManager.instance.curLevel.SetMapOfUnits(transform.position, 0);
                GameManager.instance.curLevel.SetMapOfUnits(nextPos, 10+type);
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
                    Atttack();                    
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

        public void Atttack()
        {            
            int rValue = Random.Range(0, 2);

            if (myShip.curWeapon.canShot && rValue == 0)
            {
                bulletInfo bInfo = new bulletInfo();
                bInfo.bullet = GameManager.instance.lvEfx.GetBullet(myShip.curWeapon.bType);
                bInfo.bullet.SetActive(true);
                bInfo.bullet.transform.position = transform.position;
                bInfo.targetPos = player.transform.position;
                bInfo.damage = myShip.curWeapon.weaponDamage;
                bInfo.speed = myShip.curWeapon.bulletSpeed;
                GameManager.instance.lvEfx.FireBullet(bInfo);
            }
            myShip.curWeapon.canShot = false;
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
                Atttack();
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