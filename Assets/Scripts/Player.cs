    using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };    

    public class Player : MovingObject
    {
        public AudioClip shotSound1;
        public AudioClip itemSound1;
        public AudioClip itemSound2;

        public AudioClip getSound;
        public AudioClip clearSound;
        public AudioClip destroySound;
        public AudioClip gameOverSound;


        public SpaceShip myShip;
        public GameObject sight;

        private GameObject curBullet;

        public Button shotBtn;
        public Button targetChangeBtn;

        public Button lockDirBtn;
        public Button moveUpBtn;
        public Button moveDownBtn;
        public Button moveLeftBtn;
        public Button moveRightBtn;

        public GameObject display;
        public GameObject shield;

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
            SoundManager.instance.PlaySingleBtn();
        }

        public void Init()
        {
            timeLimit = GameManager.instance.curLevel.timeLimit;
            UpdateDirImage();
            money = GameManager.instance.money;
            missionItemCount = 0;
            myShip = GameManager.instance.myShip;

            shield.GetComponent<SpriteRenderer>().color = GetSheildColor();            

            transform.position = Vector3.zero;

            targetChangeBtn.onClick.RemoveAllListeners();
            shotBtn.onClick.RemoveAllListeners();            

            lockDirBtn.onClick.RemoveAllListeners();
            moveUpBtn.onClick.RemoveAllListeners();
            moveDownBtn.onClick.RemoveAllListeners();
            moveRightBtn.onClick.RemoveAllListeners();
            moveLeftBtn.onClick.RemoveAllListeners();

            targetChangeBtn.onClick.AddListener(ChangeTarget);
            shotBtn.onClick.AddListener(AttempAttackAtSight);
            
                        
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

            playerInfo.sheildBar.color = GetSheildColor();

            float width = 200;
            switch(myShip.shieldLevel)
            {
                case 2: width = 280; break;
                case 3: width = 360; break;
                case 4: width = 440; break;
                case 5: width = 520; break;
            }

            float rate = (float)myShip.shield/ (float)myShip.shieldInits[myShip.shieldLevel];
            playerInfo.sheildBar.rectTransform.localScale = new Vector3(rate, 1, 1);
            playerInfo.sheildBar.rectTransform.sizeDelta = new Vector2(width, 50);
            playerInfo.sheildBarBG.rectTransform.sizeDelta = new Vector2(width, 50);

            width = 200;
            switch (myShip.shotPowerLevel)
            {
                case 1: width = 280; break;
                case 2: width = 360; break;
                case 3: width = 440; break;
                case 4: width = 520; break;
            }
            rate = (float)myShip.shotPower / (float)myShip.shotPowerInit[myShip.shotPowerLevel];            
            playerInfo.weaponPowerBar.rectTransform.localScale = new Vector3(rate, 1, 1);
            playerInfo.weaponPowerBar.rectTransform.sizeDelta = new Vector2(width, 50);
            playerInfo.weaponPowerBarBG.rectTransform.sizeDelta = new Vector2(width, 50);

            playerInfo.weaponPowerText.text = myShip.shotPower + "/ " + myShip.shotPowerInit[myShip.shotPowerLevel];
            playerInfo.weaponPowerBar.color = Color.red;


            Color btnColor = Color.white;
            switch (myShip.curWeapon.grade)
            {
                case 1: btnColor = Color.green; break;
                case 2: btnColor = Color.blue; break;
                case 3: btnColor = Color.red; break;
            }
            btnColor.a = 0.6f;
            playerInfo.weaponPanel.GetComponent<Image>().color = btnColor;
            playerInfo.weaponPanel.GetComponentInChildren<Text>().text = "consume [" + myShip.curWeapon.consume + "]";
            


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

        public void UpdateTarget()
        {
            if (targetEnemies.Count > targetIter)
            {
                targetEnemy = targetEnemies[targetIter];
                if (targetEnemy)
                {
                    sight.transform.position = targetEnemy.transform.position;
                    sight.SetActive(true);
                }
            }
            else
            {
                targetEnemy = null;
                sight.SetActive(false);
            }

            if (targetEnemy && GameManager.instance.curLevel.GetMapOfStructures(targetEnemy.transform.position) == 1)
            {
                targetEnemy = null;
                sight.SetActive(false);
                ChangeTarget();
            }
        }

        public List<Enemy> targetEnemies = new List<Enemy>();
        public Enemy targetEnemy = null;
        public int targetIter = 0;
        public void SearchTraget()
        {            
            targetEnemies.Clear();

            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, myShip.scopeRange);

            foreach (Vector3 pos in showRange)
            {
                if (GameManager.instance.curLevel.GetMapOfStructures(pos) == 1) continue;

                int value = GameManager.instance.curLevel.GetMapOfUnits(pos);
                if (value != 0 && value != 1)
                {
                    Enemy en = GameManager.instance.curLevel.GetEnemyByPos(pos);
                    targetEnemies.Add(en);                    
                }
            }

            UpdateTarget();
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
                float length = moveDir.magnitude;
//                float leogth2 = moveDir.sqrMagnitude;
                moveDir.Normalize();                
                bulletPos += (moveDir*Time.deltaTime* myShip.curWeapon.bulletSpeed);
                curBullet.transform.position = bulletPos;

                if (length < 0.2f)
                {
                    shoting = false;
                    curBullet.GetComponent<SpriteRenderer>().color = Color.white;
                    curBullet.SetActive(false);
                    StartCoroutine(EffectManager.instance.ShowShotEffect(enemyPos));
                    GameManager.instance.curLevel.AttackOtherPlayer(enemyPos);
                }
            }

            SearchTraget();

            GameManager.instance.ActivateRootBtn(transform.position);
            
            GameManager.instance.ShowObjs(transform.position, curDir, myShip.scopeRange);
            
            if (myShip.curWeapon.canShot)
            {
                if (Input.GetKeyDown("1")) myShip.SetWeapon(EffectManager.instance.GetWeapon(WEAPON.W1));
                if (Input.GetKeyDown("2")) myShip.SetWeapon(EffectManager.instance.GetWeapon(WEAPON.W2));
                //if (Input.GetKeyDown("3")) myShip.SetWeapon(EffectManager.instance.GetWeapon(WEAPON.W3));
                //if (Input.GetKeyDown("4")) myShip.SetWeapon(EffectManager.instance.GetWeapon(WEAPON.W4));
                //if (Input.GetKeyDown("5")) myShip.SetWeapon(EffectManager.instance.GetWeapon(WEAPON.W5));
                //if (Input.GetKeyDown("6")) myShip.SetWeapon(EffectManager.instance.GetWeapon(WEAPON.W6));

                //if (Input.GetKeyDown("1")) EffectManager.instance.ShowTextEfx(0, 1, transform.position);
                //if (Input.GetKeyDown("2")) EffectManager.instance.ShowTextEfx(1, 10, transform.position);
                if (Input.GetKeyDown("3")) EffectManager.instance.ShowTextEfx(1, 40, transform.position);
                if (Input.GetKeyDown("4")) EffectManager.instance.ShowTextEfx(1, 100, transform.position);
                if (Input.GetKeyDown("5")) EffectManager.instance.ShowTextEfx(2, -10, transform.position);
                if (Input.GetKeyDown("6")) EffectManager.instance.ShowTextEfx(2, -20, transform.position);
                if (Input.GetKeyDown("7")) EffectManager.instance.ShowTextEfx(2, -30, transform.position);
                if (Input.GetKeyDown("8")) EffectManager.instance.ShowTextEfx(2, -40, transform.position);


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
                    myShip.RestoreShield(4);
                }
                if (Input.GetKeyDown("9"))
                {
                    ShieldUp();
                }
                if (Input.GetKeyDown("0"))
                {
                    myShip.ShotPowerUp();
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ChangeTarget();
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
//                LockDir();
            }

            if (horizontal != 0 || vertical != 0)
			{
				AttemptMove<Enemy> (horizontal, vertical);
			}
		}

        public void ChangeTarget()
        {
            SoundManager.instance.PlaySingleBtn();
            targetIter++;
            if (targetIter >= targetEnemies.Count) targetIter = 0;
            UpdateTarget();
        }

        public void AttempAttackAtSight()
        {
            AttempAttack();            
        }

        public void  AttempAttack()
        {
            if (targetEnemy == null) return;            
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
                curBullet = EffectManager.instance.GetBullet(myShip.curWeapon.bType);
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

                SoundManager.instance.PlaySingle(shotSound1);
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

			if (GameManager.instance.curLevel.GetMapOfUnits (nextPos) != 0)
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
                if(missionItemCount == GameManager.instance.curLevel.missionItemCount)
                {
                    GameManager.instance.UpdateGameMssage("다음 우주로 가는 게이트웨이가 활성화 되었습니다.", 3f);
                    GameManager.instance.EnableGateway();
                }
                EffectManager.instance.ShowTextEfx(0, 1, transform.position);
                SoundManager.instance.PlaySingle(itemSound2);
            }
            else if (other.tag == "Resource")
            {
                if(other.name.Contains("gold"))
                {
                    money += 100;
                    EffectManager.instance.ShowTextEfx(1, 100, transform.position);
                }
                else if (other.name.Contains("blue"))
                {
                    money += 40;
                    EffectManager.instance.ShowTextEfx(1, 40, transform.position);
                }
                else if (other.name.Contains("bronze"))
                {
                    money += 10;
                    EffectManager.instance.ShowTextEfx(1, 10, transform.position);
                }
                other.gameObject.SetActive(false);
                SoundManager.instance.PlaySingle(itemSound1);
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
            EffectManager.instance.ShowTextEfx(2, -loss, transform.position);
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
                SoundManager.instance.PlaySingle(gameOverSound);
//                SoundManager.instance.musicSource.Stop();
                GameManager.instance.GameOver();


        }
	}
}

