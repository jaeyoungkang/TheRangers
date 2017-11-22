using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };

    [System.Serializable]
    public class Weapon
    {
        public int bType;
        public int bulletSpeed = 30;

        public int capability = 10;
        public int weaponDamage = 4;

        public bool canShot = true;

        public float shotTime;
        public float shotTimeInit = 1f;

        public float reloadTime;
        public float reloadTimeInit = 1.5f;

        public int shotAniSpeed = 1;
        public float aniDelay = 0.3f;

        public string name;

        public Weapon(int _damage, float _shotTime, float _reloadTime, int _shotAniSpeed, float _aniDelay, int _capability, int _bulletSpeed, string _name, int _bType)
        {
            weaponDamage = _damage;
            shotTimeInit = _shotTime;
            reloadTimeInit = _reloadTime;
            shotAniSpeed = _shotAniSpeed;
            aniDelay = _aniDelay;
            capability = _capability;
            bulletSpeed = _bulletSpeed;
            name = _name;
            bType = _bType;
            Init();
        }

        public void Init()
        {
            shotTime = shotTimeInit;
            reloadTime = reloadTimeInit;
        }

        public void UpdateReload()
        {
            reloadTime -= Time.deltaTime;            
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

    public class Player : MovingObject
	{
        public SpaceShip myShip;
        public GameObject sight;

        private GameObject curBullet;

        public Button reloadBtn;
        public Button shotBtn;
        public Button sightUpBtn;
        public Button sightDownBtn;
        public Button sightLeftBtn;
        public Button sightRightBtn;

        public Button lockDirBtn;
        public Button moveUpBtn;
        public Button moveDownBtn;
        public Button moveLeftBtn;
        public Button moveRightBtn;

        public GameObject display;       

        public float weaponRangeMin = 0;
        public float weaponRangeMax = 3;

        Vector3 localSightPos = new Vector3(1, 0, 0);
        bool shoting = false;
        Vector3 enemyPos = new Vector3();

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif

        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public GameObject[] dirSprits;

        bool bLockDir = false;
        void LockDir()
        {
            bLockDir = !bLockDir;
        }			

        public void Init()
        {            
            UpdateDirImage();

            myShip = GameManager.instance.myShip;

            transform.position = Vector3.zero;

            reloadBtn.onClick.AddListener(ReloadByTouch);
            shotBtn.onClick.AddListener(AttempAttackAtSight);
            sightUpBtn.onClick.AddListener(SightMoveUp);
            sightDownBtn.onClick.AddListener(SightMoveDown);
            sightRightBtn.onClick.AddListener(SightMoveRight);
            sightLeftBtn.onClick.AddListener(SightMoveLeft);

            lockDirBtn.onClick.RemoveAllListeners();
            lockDirBtn.onClick.AddListener(LockDir);
            moveUpBtn.onClick.AddListener(MoveUp);
            moveDownBtn.onClick.AddListener(MoveDown);
            moveRightBtn.onClick.AddListener(MoveRight);
            moveLeftBtn.onClick.AddListener(MoveLeft);
        }        
                
        protected override void Start ()
		{
			base.Start ();
            Init();
        }

        void MoveUp() { AttemptMove<Enemy>(0, 1); }
        void MoveDown() { AttemptMove<Enemy>(0, -1); }
        void MoveRight() { AttemptMove<Enemy>(1, 0); }
        void MoveLeft() { AttemptMove<Enemy>(-1, 1); }

        void UpdateSightPos()
        {
            Vector3 worldSightPos = transform.position + localSightPos;
            if (worldSightPos.x < 0)
            {
                localSightPos.x -= worldSightPos.x;
                worldSightPos.x = 0;
            }

            if (worldSightPos.x >= GameManager.instance.curLevel.columns)
            {
                localSightPos.x -= worldSightPos.x - (GameManager.instance.curLevel.columns - 1);
                worldSightPos.x = GameManager.instance.curLevel.columns - 1;
                
            }

            if (worldSightPos.y < 0)
            {
                localSightPos.y -= worldSightPos.y;
                worldSightPos.y = 0;
            }

            if (worldSightPos.y >= GameManager.instance.curLevel.rows)
            {
                localSightPos.y -= worldSightPos.y - (GameManager.instance.curLevel.rows - 1);
                worldSightPos.y = GameManager.instance.curLevel.rows - 1;
            }

            localSightPos.y = Mathf.Round(localSightPos.y);
            localSightPos.x = Mathf.Round(localSightPos.x);

            worldSightPos.y = Mathf.Round(worldSightPos.y);
            worldSightPos.x = Mathf.Round(worldSightPos.x);

            sight.transform.position = worldSightPos;
        }

        void SightMoveUp()
        {
            if (!myShip.canMove) return;
            myShip.canMove = false;
            localSightPos.y += 1;
            UpdateSightPos();
        }

        void SightMoveRight()
        {
            if (!myShip.canMove) return;
            myShip.canMove = false;
            localSightPos.x += 1;
            UpdateSightPos();
        }

        void SightMoveLeft()
        {
            if (!myShip.canMove) return;
            myShip.canMove = false;
            localSightPos.x -= 1;
            UpdateSightPos();
        }

        void SightMoveDown()
        {
            if (!myShip.canMove) return;
            myShip.canMove = false;
            localSightPos.y -= 1;            
            UpdateSightPos();
        }

        private void UpdateDisplay()
        {
            if (display == null) return;
            PlayerInfo playerInfo = display.GetComponent<PlayerInfo>();

            string weaponChangeTimeText = Mathf.FloorToInt(myShip.weaponChangeTime * 100).ToString();
            playerInfo.curWeaponText.text = "Change Time: " + weaponChangeTimeText + "\n\n" + myShip.curWeapon.name;
            playerInfo.foodText.text = "SHIELD : " + myShip.shield;
            playerInfo.ammoText.text = "[Ammo : " + myShip.numOfBullets[myShip.curWeapon.bType] + "/" + myShip.totalBullets[myShip.curWeapon.bType] + "]\n\n" + "Reload Time :" + Mathf.FloorToInt(myShip.curWeapon.reloadTime * 100).ToString();
            
            playerInfo.coolTimeText.text = Mathf.FloorToInt(myShip.moveTime * 100).ToString();
            playerInfo.shotTimeText.text = Mathf.FloorToInt(myShip.curWeapon.shotTime * 100).ToString();
        }

        private void Update ()
		{
            if (GameManager.instance.doingSetup) return;
            UpdateDisplay();
            int value = GameManager.instance.curLevel.GetMapOfStructures(transform.position);
            myShip.UpdateScope(value);

            if(shoting)
            {
                Vector3 bulletPos = curBullet.transform.position;
                Vector3 moveDir = enemyPos - bulletPos;
                float length = moveDir.sqrMagnitude;
                moveDir.Normalize();
                bulletPos += (moveDir*Time.deltaTime* myShip.curWeapon.bulletSpeed);
                curBullet.transform.position = bulletPos;

                if (length < 0.1f)
                {
                    shoting = false;
                    curBullet.SetActive(false);
                    StartCoroutine(GameManager.instance.lvEfx.ShowShotEffect(enemyPos, myShip.curWeapon));
                    GameManager.instance.curLevel.AttackOtherPlayer(enemyPos);
                }
            }
            
            GameManager.instance.ActivateRootBtn(transform.position);

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                localSightPos.y += 1;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                localSightPos.y -= 1;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                localSightPos.x -= 1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                localSightPos.x += 1;
            }
            UpdateSightPos();
            
            GameManager.instance.ShowObjs(transform.position, curDir, myShip.scopeRange);
            
            if(myShip.startChangeWeapon)
            {
                myShip.UpdateWeaponChangeTime();
            }
            else if (myShip.startReload)
            {
                myShip.UpdateReload();                
            }            
            else if (myShip.curWeapon.canShot)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AttempAttack(myShip.curWeapon.bType);
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    myShip.Reload(myShip.curWeapon.bType);
                }
                else if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    myShip.SetWeapon(GameManager.instance.lvEfx.weaponS);
                }
                else if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    myShip.SetWeapon(GameManager.instance.lvEfx.weaponM);
                }
                else if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    myShip.SetWeapon(GameManager.instance.lvEfx.weaponP);
                }
            }
            else
            {
                myShip.UpdateWeaponCooling();
            }            

            if (myShip.canMove == false)
            {
                myShip.UpdateMoveCoolTime();
            }
            int horizontal = 0;
			int vertical = 0;
			
#if UNITY_STANDALONE || UNITY_WEBPLAYER
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));			
			vertical = (int) (Input.GetAxisRaw ("Vertical"));

			if(horizontal != 0)
			{
				vertical = 0;
			}

#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
#endif

            if (Input.GetKeyDown(KeyCode.W))
            {
                vertical = 1;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                vertical = -1;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                horizontal = -1;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                horizontal = 1;
            }
            else if(Input.GetKeyUp(KeyCode.Tab))
            {
                LockDir();
            }

            if (horizontal != 0 || vertical != 0)
			{
				AttemptMove<Enemy> (horizontal, vertical);
			}
		}

        public void ReloadByTouch()
        {
            myShip.Reload(myShip.curWeapon.bType);
        }

        public void AttempAttackAtSight()
        {
            if (myShip.Shot(myShip.curWeapon.bType)) Attack(sight.transform.position);
        }

        public void  AttempAttack(int input)
        {
            bool enableShot = false;
            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, myShip.scopeRange);
            foreach (Vector3 pos in showRange)
            {
                if (pos == sight.transform.position)
                {
                    enableShot = true;
                    break;
                }
            }
            if (enableShot == false) return;

            if (myShip.Shot(input)) Attack(sight.transform.position);
        }

        public bool CheckDirChanged(int xDir, int yDir)
        {
            if (bLockDir) return false;

            bool change = true;
            switch(curDir)
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
            else if(xDir == -1)
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
            
            if(prevDir == MOVE_DIR.RIGHT)
            {
                if ( curDir == MOVE_DIR.UP)
                {
                    localSightPos = Quaternion.Euler(0, 0, 90) * localSightPos;
                }
                else if (curDir == MOVE_DIR.DOWN)
                {
                    localSightPos = Quaternion.Euler(0, 0, -90) * localSightPos;
                }
                else if (curDir == MOVE_DIR.LEFT)
                {
                    localSightPos = Quaternion.Euler(0, 0, 180) * localSightPos;
                }
            }
            if (prevDir == MOVE_DIR.UP)
            {
                if (curDir == MOVE_DIR.RIGHT)
                {
                    localSightPos = Quaternion.Euler(0, 0, -90) * localSightPos;
                }
                else if (curDir == MOVE_DIR.DOWN)
                {
                    localSightPos = Quaternion.Euler(0, 0, 180) * localSightPos;
                }
                else if (curDir == MOVE_DIR.LEFT)
                {
                    localSightPos = Quaternion.Euler(0, 0, 90) * localSightPos;
                }
            }

            if (prevDir == MOVE_DIR.DOWN)
            {
                if (curDir == MOVE_DIR.UP)
                {
                    localSightPos = Quaternion.Euler(0, 0, 180) * localSightPos;
                }
                else if (curDir == MOVE_DIR.RIGHT)
                {
                    localSightPos = Quaternion.Euler(0, 0, 90) * localSightPos;
                }
                else if (curDir == MOVE_DIR.LEFT)
                {
                    localSightPos = Quaternion.Euler(0, 0, -90) * localSightPos;
                }
            }

            if (prevDir == MOVE_DIR.LEFT)
            {
                if (curDir == MOVE_DIR.UP)
                {
                    localSightPos = Quaternion.Euler(0, 0, -90) * localSightPos;
                }
                else if (curDir == MOVE_DIR.DOWN)
                {
                    localSightPos = Quaternion.Euler(0, 0, 90) * localSightPos;
                }
                else if (curDir == MOVE_DIR.RIGHT)
                {
                    localSightPos = Quaternion.Euler(0, 0, 180) * localSightPos;
                }
            }
            UpdateSightPos();
            
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
        
        public void Attack(Vector3 attackPos)
        {
            if(shoting == false)
            {
                curBullet = GameManager.instance.lvEfx.GetBullet(myShip.curWeapon.bType);
                shoting = true;
                curBullet.transform.position = transform.position;
                enemyPos = attackPos;
                curBullet.SetActive(true);                
            }            
        }
        
        protected override void AttemptMove <T> (int xDir, int yDir)
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

			if (GameManager.instance.curLevel.GetMapOfUnits (nextPos) == 1)
				return;
					
			base.AttemptMove <T> (xDir, yDir);
			RaycastHit2D hit;
			if (Move (xDir, yDir, out hit)) {
//				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);

				GameManager.instance.curLevel.SetMapOfUnits(transform.position, 0);
				GameManager.instance.curLevel.SetMapOfUnits(nextPos, 1);
			}			
		}
		
		protected override void OnCantMove <T> (T component)
		{

		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
            if (other.tag == "Exit")
            {
                if(GameManager.instance.IsEnd())
                {
                    GameManager.instance.Win();
                }
                else
                {
                    GameManager.instance.NextUniverse();
                }                
            }
        }
                
    	public void LoseHP (int loss)
		{
            if (myShip.Shield()) myShip.Damaged(loss);
            else Destoryed();

            if (myShip.shield < 0) Destoryed();
		}
		
		private void Destoryed()
		{
//                SoundManager.instance.PlaySingle(gameOverSound);
                SoundManager.instance.musicSource.Stop();
                GameManager.instance.GameOver();


        }
	}
}

