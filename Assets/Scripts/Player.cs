using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };

    public class Weapon
    {
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


        public Weapon(int _damage, float _shotTime, float _reloadTime, int _shotAniSpeed, float _aniDelay, int _capability, int _bulletSpeed)
        {
            weaponDamage = _damage;
            shotTimeInit = _shotTime;
            reloadTimeInit = _reloadTime;
            shotAniSpeed = _shotAniSpeed;
            aniDelay = _aniDelay;
            capability = _capability;
            bulletSpeed = _bulletSpeed;
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

            myShip = new SpaceShip(10, 0.4f, 2);
            myShip.ReadyToDeparture(GameManager.instance.weaponM);            
            
            transform.position = Vector3.zero;

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

            myShip.SetupStorage(50, 30, 10);
        }        
                
        protected override void Start ()
		{
			base.Start ();
            Init();
        }

        void MoveUp() { AttemptMove<Wall>(0, 1); }
        void MoveDown() { AttemptMove<Wall>(0, -1); }
        void MoveRight() { AttemptMove<Wall>(1, 0); }
        void MoveLeft() { AttemptMove<Wall>(-1, 1); }

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

            playerInfo.foodText.text = "SHIELD : " + myShip.shield;
            playerInfo.ammoText.text = "[Ammo1 : " + myShip.numOfBullets[0] + "/" + myShip.totalBullets[0] + "]\n\n" +
                            "[Ammo2 : " + myShip.numOfBullets[1] + "/" + myShip.totalBullets[1] + "]";
            
            playerInfo.coolTimeText.text = Mathf.FloorToInt(myShip.moveTime * 100).ToString();
            if(myShip.startReload) playerInfo.shotTimeText.text = Mathf.FloorToInt(myShip.curWeapon.reloadTime * 100).ToString();
            else playerInfo.shotTimeText.text = Mathf.FloorToInt(myShip.curWeapon.shotTime * 100).ToString();

            
            string greenTag = "<color=#00ff00>";

            string colorTag = greenTag;
            string HpText = colorTag + string.Format("최대 실드 {0} </color>\n\n", myShip.shieldInit);
            string SpeedText = colorTag + string.Format("이동 대기 시간 {0:N2}초 </color>\n\n", myShip.moveTimeInit);
        }       

        public void ExtendStorage()
        {
            myShip.storageSize++;
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
                    StartCoroutine(GameManager.instance.ShowShotEffect(enemyPos, myShip.curWeapon));
                    GameManager.instance.curLevel.AttackOtherPlayer(enemyPos);
                }
            }

            value = GameManager.instance.curLevel.GetMapOfItems(transform.position);
            GameManager.instance.ActivateRootBtn(value == 1);

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
            
            if (myShip.startReload)
            {
                myShip.UpdateReload();                
            }
            else if (myShip.curWeapon.canShot)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AttempAttack(0);
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    myShip.Reload(0);
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
			
			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}
				
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;
					
					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
				}
			}

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
				AttemptMove<Wall> (horizontal, vertical);
			}
		}

        public void AttempAttackAtSight()
        {
            if (myShip.Shot(0)) Attack(sight.transform.position);
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
                curBullet = GameManager.instance.GetBullet(1);
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
            if (other.tag == "MissionItem")
            {
                GameManager.instance.collectionCount++;                
                other.gameObject.SetActive(false);
            }
        }
        
		private void Restart ()
		{
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
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

