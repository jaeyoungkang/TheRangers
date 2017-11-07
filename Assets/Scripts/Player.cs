using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };
    public enum WEAPON_TYPE { FRONT_1, FRONT_2 };    

    public class Player : MovingObject
	{
        public SpaceShip myShip;
        public GameObject sight;

        public Button shotBtn;
        public Button sightUpBtn;
        public Button sightDownBtn;
        public Button sightLeftBtn;
        public Button sightRightBtn;

        public bool autoMode = true;
        public int unitId = 1;
        public bool myPlayer = false;
        public GameObject display;
        public AudioClip moveSound1;
        public AudioClip moveSound2;
        public AudioClip eatSound1;
        public AudioClip eatSound2;
        public AudioClip drinkSound1;
        public AudioClip drinkSound2;
        public AudioClip gameOverSound;        

        public int weaponDamage = 10;
        public float weaponRangeMin = 0;
        public float weaponRangeMax = 3;        

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif

        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public GameObject[] dirSprits;

        public void Init()
        {            
            UpdateDirImage();

            myShip = new SpaceShip();
            myShip.ReadyToDeparture(); 
            
            if (myPlayer)
            {
                transform.position = Vector3.zero;

                shotBtn.onClick.AddListener(AttempAttackAtSight);
                sightUpBtn.onClick.AddListener(SightMoveUp);
                sightDownBtn.onClick.AddListener(SightMoveDown);
                sightRightBtn.onClick.AddListener(SightMoveRight);
                sightLeftBtn.onClick.AddListener(SightMoveLeft);
            }
            else
            {
                myShip.SetupStorage(10, 10, 10);
            }
        }        
                
        protected override void Start ()
		{
			base.Start ();
            Init();

            if (!myPlayer)
            {
                GameManager.instance.curLevel.AddOtherPlayerToList(this);

                Renderer renderer = gameObject.GetComponent<SpriteRenderer>();

                if(unitId == 1) renderer.sortingLayerName = "Player";
                else renderer.sortingLayerName = "Enemy";                
            }
        }

        void UpdateSightPos()
        {
            sight.transform.position = transform.position + localSightPos;
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
            if(myShip.startReload) playerInfo.shotTimeText.text = Mathf.FloorToInt(myShip.reloadTime * 100).ToString();
            else playerInfo.shotTimeText.text = Mathf.FloorToInt(myShip.shotTime * 100).ToString();

            
            string greenTag = "<color=#00ff00>";

            string colorTag = greenTag;
            string HpText = colorTag + string.Format("최대 실드 {0} </color>\n\n", myShip.shieldInit);
            string SpeedText = colorTag + string.Format("이동 대기 시간 {0:N2}초 </color>\n\n", myShip.moveTimeInit);

            playerInfo.infoText.text = HpText + SpeedText + string.Format(
                        @"공격 대기 시간 {0:N2}초

장전 대기 시간 {1:N2}초

시야 {2}칸

1번무기
범위 : 1,2 블럭 공격
장전 탄수: 2개

2번무기
범위 : 2,3 블럭 공격
장전 탄수: 2개", myShip.shotTime, myShip.reloadTime, myShip.scopeRange);

            playerInfo.infoText.text += "\n출력 " + myShip.controlPower + "%";            
        }       

        public void ExtendStorage()
        {
            myShip.storageSize++;
        }

        public void UpdateSightPos(Vector3 localPos)
        {
            Vector3 pos = transform.position;            
            switch (curDir)
            {
                case MOVE_DIR.UP: pos.y += 1; break;
                case MOVE_DIR.DOWN: pos.y -= 1; break;
                case MOVE_DIR.LEFT: pos.x -= 1; break;
                case MOVE_DIR.RIGHT: pos.x += 1; break;
            }

            sight.transform.position = pos + localPos;
        }

        Vector3 localSightPos = new Vector3(1, 0, 0);
        private void Update ()
		{
            if (GameManager.instance.doingSetup) return;
            UpdateDisplay();
            int value = GameManager.instance.curLevel.GetMapOfStructures(transform.position);
            myShip.UpdateScope(value);

            if(myPlayer)
            {
                value = GameManager.instance.curLevel.GetMapOfItems(transform.position);
                GameManager.instance.ActivateRootBtn(value == 1);

                if (Input.GetKeyDown(KeyCode.T))
                {
                    localSightPos.y += 1;
                }
                else if (Input.GetKeyDown(KeyCode.G))
                {
                    localSightPos.y -= 1;
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    localSightPos.x -= 1;
                }
                else if (Input.GetKeyDown(KeyCode.H))
                {
                    localSightPos.x += 1;
                }
                UpdateSightPos();
            }
            

            if (myPlayer)
            {
                GameManager.instance.ShowObjs(transform.position, curDir, myShip.scopeRange);
                myShip.UpdatePowerState();
                if (myShip.IsPowerDown()) Destoryed();
            }            

            if (myShip.startReload)
            {
                myShip.UpdateReload();                
            }
            else if (myShip.canShot)
            {
                if(myPlayer)
                {                    
                    if (Input.GetKeyDown("3"))
                    {
                        myShip.ChargePower();       
                    }
                    else
                    {
                        int input = GetAttackInput();
                        if (input != -1) AttempAttack(input);
                    }
                }
            }
            else
            {
                myShip.UpdateWeaponCooling();
            }            

            if (myShip.canMove == false)
            {
                myShip.UpdateMoveCoolTime();
                return;
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
            if (!myPlayer && autoMode)
            {
                AutoMove();
            }
            else if (horizontal != 0 || vertical != 0)
			{
				AttemptMove<Wall> (horizontal, vertical);
			}

            
		}

        public int GetAttackInput()
        {
            if (Input.GetKeyDown("1")) return 0;
            if (Input.GetKeyDown("2")) return 1;
//            if (Input.GetKeyDown("3")) return 2;
            
            return -1;
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
                if (myShip.Shot(0))
                {
                    StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position));
                    player.LoseHP(weaponDamage);
                }
            }
            else
            {
                if(transform.position.x > 0)
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
                    StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position));
                    player.LoseHP(weaponDamage);
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

        private Vector3 targetPos = Vector3.zero; 
        public void AutoMove()
        {            
            if (unitId == 1)
            {
                AutoMoveUnit01();
                return;
            }
            else if(unitId == 3)
            {
                AutoMoveUnit03();
                return;
            }

            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, myShip.scopeRange);
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            bool found = false;
            foreach(Vector3 pos in showRange)
            {
                if (GameManager.instance.curLevel.GetMapOfStructures(pos) == 1) continue;
                if (player.transform.position == pos)
                {
                    found = true;
                    targetPos = pos;
                    break;
                }
            }

            if(found)
            {
                float deltaX = Mathf.Abs(transform.position.x - player.transform.position.x);
                float deltaY = Mathf.Abs(transform.position.y - player.transform.position.y);

                bool inRange = false;
                if(deltaX + deltaY <= weaponRangeMax && deltaX + deltaY >= weaponRangeMin)
                {
                    inRange = true;
                }

                if( inRange && (deltaX < Mathf.Epsilon || deltaY < Mathf.Epsilon) )
                {
                    if (myShip.Shot(0))
                    {
                        StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position));
                        player.LoseHP(weaponDamage);
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
            else if(targetPos != Vector3.zero)
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

        public void AttempAttackAtSight()
        {
            if (myShip.Shot(0)) Attack(sight.transform.position);
        }


        public void  AttempAttack(int input)
        {
            if (myShip.Shot(input)) Attack(sight.transform.position);
//            if(myShip.Shot(input)) Attack2(input);
            
        }

        public bool CheckDir(int xDir, int yDir)
        {
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
            
            if(myPlayer)
            {
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

        public void Attack2(int type)
        {
            Vector3 attackPos1 = transform.position;
            Vector3 attackPos2 = transform.position;

            int startPos = 1;
            if(type == 1)
            {
                startPos = 2;
            }

            switch (curDir)
            {
                case MOVE_DIR.RIGHT:
                    attackPos1.x += startPos;
                    attackPos2.x += startPos + 1;
                    break;
                case MOVE_DIR.LEFT:
                    attackPos1.x -= startPos;
                    attackPos2.x -= startPos + 1;
                    break;
                case MOVE_DIR.UP:
                    attackPos1.y += startPos;
                    attackPos2.y += startPos + 1;
                    break;
                case MOVE_DIR.DOWN:
                    attackPos1.y -= startPos;
                    attackPos2.y -= startPos + 1;
                    break;
            }

            StartCoroutine(ChainAttack(attackPos1, attackPos2));
        }

        public System.Collections.IEnumerator ChainAttack(Vector3 attackPos1, Vector3 attackPos2)
        {
            StartCoroutine(GameManager.instance.ShowShotEffect(attackPos1));
            GameManager.instance.curLevel.AttackOtherPlayer(attackPos1);
            yield return new WaitForSeconds(0.2f);

            StartCoroutine(GameManager.instance.ShowShotEffect(attackPos2));
            GameManager.instance.curLevel.AttackOtherPlayer(attackPos2);
        }

        public void Attack(Vector3 attackPos)
        {
            StartCoroutine(GameManager.instance.ShowShotEffect(attackPos));

            GameManager.instance.curLevel.AttackOtherPlayer(attackPos);
        }
        
        protected override void AttemptMove <T> (int xDir, int yDir)
		{
            myShip.Move();

            if (CheckDir(xDir, yDir))
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
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);

				GameManager.instance.curLevel.SetMapOfUnits(transform.position, 0);
				GameManager.instance.curLevel.SetMapOfUnits(nextPos, 1);
			}			
		}
		
		protected override void OnCantMove <T> (T component)
		{

		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
            if (!myPlayer) return;

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
		}
		
		private void Destoryed()
		{
            if (myPlayer)
            {
                SoundManager.instance.PlaySingle(gameOverSound);
                SoundManager.instance.musicSource.Stop();
                GameManager.instance.GameOver();
            }
            else
            {
                GameManager.instance.DestroyOtherPlayer(gameObject);
                GameManager.instance.curLevel.SetMapOfUnits(gameObject.transform.position, 0);
            }                       
        }
	}
}

