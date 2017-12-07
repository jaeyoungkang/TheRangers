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
        public int consume = 1;
        public int grade;
        public int bType;
        public int bulletSpeed = 30;

        public int weaponDamage = 4;

        public bool canShot = true;

        public float shotTime;
        public float shotTimeInit = 1f;

        public int shotAniSpeed = 1;
        public float aniDelay = 0.3f;

        public string name;

        public Weapon(int _damage, float _shotTime, int _shotAniSpeed, float _aniDelay, int _bulletSpeed, string _name, int _bType, int _grade, int _consume)
        {
            weaponDamage = _damage;
            shotTimeInit = _shotTime;
            shotAniSpeed = _shotAniSpeed;
            aniDelay = _aniDelay;
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
        public GameObject shield;

        Vector3 localSightPos = new Vector3(1, 0, 0);
        bool shoting = false;
        Vector3 enemyPos = new Vector3();
        float timeLimit = 60;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif

        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public GameObject[] dirSprits;

        public int money;
        public int missionItemCount;

        public void AddMoney(int addMoney)
        {
            money += addMoney;
        }

        bool bLockDir = false;
        void LockDir()
        {
            bLockDir = !bLockDir;
        }

        public void Init()
        {
            timeLimit = GameManager.instance.curLevel.timeLimit;
            UpdateDirImage();
            money = GameManager.instance.money;
            missionItemCount = 0;
            myShip = GameManager.instance.myShip;

            shield.GetComponent<SpriteRenderer>().color = GetSheildColor();
            UpdateSheildAlpha();

            transform.position = Vector3.zero;

            shotBtn.onClick.RemoveAllListeners();
            sightUpBtn.onClick.RemoveAllListeners();
            sightDownBtn.onClick.RemoveAllListeners();
            sightRightBtn.onClick.RemoveAllListeners();
            sightLeftBtn.onClick.RemoveAllListeners();

            lockDirBtn.onClick.RemoveAllListeners();
            moveUpBtn.onClick.RemoveAllListeners();
            moveDownBtn.onClick.RemoveAllListeners();
            moveRightBtn.onClick.RemoveAllListeners();
            moveLeftBtn.onClick.RemoveAllListeners();
            
            shotBtn.onClick.AddListener(AttempAttackAtSight);
            sightUpBtn.onClick.AddListener(SightMoveUp);
            sightDownBtn.onClick.AddListener(SightMoveDown);
            sightRightBtn.onClick.AddListener(SightMoveRight);
            sightLeftBtn.onClick.AddListener(SightMoveLeft);
                        
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
        void MoveLeft() { AttemptMove<Enemy>(-1, 0); }

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
            localSightPos.y += 1;
            UpdateSightPos();
        }

        void SightMoveRight()
        {
            localSightPos.x += 1;
            UpdateSightPos();
        }

        void SightMoveLeft()
        {
            localSightPos.x -= 1;
            UpdateSightPos();
        }

        void SightMoveDown()
        {
            localSightPos.y -= 1;            
            UpdateSightPos();
        }

        public Color GetSheildColor()
        {
            Color shieldColor = Color.white;
            switch (myShip.shieldLevel)
            {
                case 1: shieldColor = Color.green; break;
                case 2: shieldColor = Color.blue; break;
                case 3: shieldColor = Color.yellow; break;
                case 4: shieldColor = Color.magenta; break;
            }
            shieldColor.a = 0.35f;

            return shieldColor;
        }

        public void ShieldUp()
        {
            myShip.ShieldUp();
            shield.GetComponent<SpriteRenderer>().color = GetSheildColor();
        }

        private void UpdateDisplay()
        {
            if (display == null) return;
            PlayerInfo playerInfo = display.GetComponent<PlayerInfo>();

            playerInfo.missionItemText.text = missionItemCount + "/" + GameManager.instance.curLevel.missionItemCount;
            playerInfo.moneyText.text = money.ToString();
            playerInfo.sheildText.text = myShip.shield + "/" + myShip.shieldInits[myShip.shieldLevel];
            
            playerInfo.sheildText.GetComponentInChildren<Image>().color = GetSheildColor();            

            Color btnColor = Color.white;
            switch (myShip.curWeapon.grade)
            {
                case 1: btnColor = Color.green; break;
                case 2: btnColor = Color.blue; break;
                case 3: btnColor = Color.red; break;
            }
            playerInfo.weaponButton.GetComponent<Image>().color = btnColor;
            playerInfo.weaponButton.GetComponentInChildren<Text>().text = myShip.curWeapon.name + "\n" + "consume [" + myShip.curWeapon.consume + "]";
            playerInfo.weaponPowerText.text = myShip.shotPower.ToString();

//            playerInfo.coolTimeText.text = Mathf.FloorToInt(myShip.moveTime * 100).ToString();
//            shotBtn.GetComponentInChildren<Text>().text = myShip.curWeapon.name + "\n" + "["+ myShip.totalBullets +"]";
//            playerInfo.shotTimeText.text = Mathf.FloorToInt(myShip.curWeapon.shotTime * 100).ToString();

            playerInfo.timeLimitText.text = Mathf.FloorToInt(timeLimit).ToString();
            if(timeLimit <= 10)
            {
                playerInfo.timeLimitText.color = Color.red;
            }
            else
            {
                playerInfo.timeLimitText.color = Color.white;
            }
        }

        private void Update ()
		{
            if (GameManager.instance.doingSetup) return;
            UpdateDisplay();
            int value = GameManager.instance.curLevel.GetMapOfStructures(transform.position);
            myShip.UpdateScope(value);
            timeLimit -= Time.deltaTime;

            if(timeLimit <= 0 )
            {
                LoseHP(2);
                timeLimit = 10;
                GameManager.instance.UpdateGameMssage("시간을 오바했다!", 1f);
            }

            if (shoting)
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
                    curBullet.GetComponent<SpriteRenderer>().color = Color.white;
                    curBullet.SetActive(false);
//                    StartCoroutine(GameManager.instance.lvEfx.ShowShotEffect(enemyPos, myShip.curWeapon));
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
            
            if (myShip.curWeapon.canShot)
            {
                if (Input.GetKeyDown("1")) myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W1));
                if (Input.GetKeyDown("2")) myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W2));
                if (Input.GetKeyDown("3")) myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W3));
                if (Input.GetKeyDown("4")) myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W4));
                if (Input.GetKeyDown("5")) myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W5));
                if (Input.GetKeyDown("6")) myShip.SetWeapon(GameManager.instance.lvEfx.GetWeapon(WEAPON.W6));

                if (Input.GetKeyDown("i"))
                {
                    LoseHP(2);
                }

                if (Input.GetKeyDown("u"))
                {
                    money += 1000;
                }

                if (Input.GetKeyDown("o"))
                {
                    GameManager.instance.DropItem(transform.position, 1);
                }

                if (Input.GetKeyDown("p"))
                {
                    GameManager.instance.LayoutShop(transform.position);
                }

                if (Input.GetKeyDown("7"))
                {
                    myShip.AddPower(10);
                }               
                if (Input.GetKeyDown("8"))
                {
                    myShip.RestoreShield();
                }
                if (Input.GetKeyDown("9"))
                {
                    ShieldUp();
                }
                if (Input.GetKeyDown("0"))
                {
                    myShip.SpeedUp();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AttempAttack();
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

        public void AttempAttackAtSight()
        {
            AttempAttack();            
        }

        public void  AttempAttack()
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

            if (myShip.Shot(myShip.curWeapon.consume)) Attack(sight.transform.position);
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

                Color bulletColor = Color.white;
                switch(myShip.curWeapon.grade)
                {
                    case 1: bulletColor = Color.green; break;
                    case 2: bulletColor = Color.blue; break;
                    case 3: bulletColor = Color.red; break;
                }

                curBullet.GetComponent<SpriteRenderer>().color = bulletColor;
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
                if (missionItemCount >= GameManager.instance.curLevel.missionItemCount)
                {
                    if (GameManager.instance.IsEnd())
                    {
                        GameManager.instance.Win();
                    }
                    else
                    {
                        GameManager.instance.money = money;
                        GameManager.instance.NextUniverse();
                    }
                }
                else
                {
                    GameManager.instance.UpdateGameMssage("크리스탈을 더 수집해야함!", 2f);
                }
                
            }
            else if( other.tag == "MissionItem")
            {
                missionItemCount++;
                other.gameObject.SetActive(false);
            }
            else if (other.tag == "Resource")
            {
                if(other.name.Contains("gold"))
                {
                    money += 100;
                }
                else if (other.name.Contains("blue"))
                {
                    money += 40;
                }
                else if (other.name.Contains("bronze"))
                {
                    money += 10;
                }
                other.gameObject.SetActive(false);
            }
        }

        public void UpdateSheildAlpha()
        {
            Color sColor = shield.GetComponent<SpriteRenderer>().color;
            
            float shiledRate = (float)myShip.shield / (float)myShip.shieldInits[myShip.shieldLevel];
            if (shiledRate > 0.8f) sColor.a = 0.35f;
            else if (shiledRate > 0.5f) sColor.a = 0.25f;
            else if (shiledRate > 0.2f) sColor.a = 0.15f;
            else sColor.a = 0.0f;

            shield.GetComponent<SpriteRenderer>().color = sColor;
        }

        public System.Collections.IEnumerator ShieldEffect(SpaceShip myShip)
        {
            Color sColor = shield.GetComponent<SpriteRenderer>().color;
            sColor.a = 0.8f;
            shield.GetComponent<SpriteRenderer>().color = sColor;
            
            yield return new WaitForSeconds(0.2f);

            UpdateSheildAlpha();
        }

        public void LoseHP (int loss)
		{
            if (myShip.Shield())
            {
                myShip.Damaged(loss);
                StartCoroutine(ShieldEffect(myShip));
            }

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

