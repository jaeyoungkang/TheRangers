﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };

    public class Player : MovingObject
	{
        bool canShot = true;
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

        public float restartLevelDelay = 1f;		

		public int HP;
        public int HP_MAX = 20;
        public int HP_INIT = 10;
        private int money = 0;

        public float playerTimeInit = 0.5f;
        public float shotTimeInit = 0.5f;
        public float reloadTimeInit = 3.0f;
        float playerTime;
        float shotTime;
        float reloadTime;
        bool playersTurn = true;

        public int weaponDamage = 10;
        public float weaponRangeMin = 0;
        public float weaponRangeMax = 3;

        public int scopeRangeInit = 2;
        public int scopeRange = 2;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif

        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public GameObject[] dirSprits;

        public int[] numOfBullets = new int[3];
        public int[] totalBullets = new int[3];

        public void Init()
        {
            UpdateDirImage();
            playerTime = playerTimeInit;
            shotTime = shotTimeInit;
            reloadTime = reloadTimeInit;
            scopeRange = scopeRangeInit;
            HP = HP_INIT;
            if (myPlayer)  transform.position = Vector3.zero;
        }
        
        protected override void Start ()
		{
			base.Start ();
            Init();

            if (!myPlayer)
            {
                GameManager.instance.curLevel.AddOtherPlayerToList(this);

                Renderer renderer = gameObject.GetComponent<SpriteRenderer>();
                renderer.sortingLayerName = "Enemy";

                if(autoMode)
                {
                    for (int i = 0; i < numOfBullets.Length; i++)
                    {
                        numOfBullets[i] = 2;
                    }
                }                
            }
        }

        private void UpdateDisplay()
        {
            if (display == null) return;
            PlayerInfo playerInfo = display.GetComponent<PlayerInfo>();

            playerInfo.foodText.text = "HP : " + HP;
            playerInfo.ammoText.text = "[Ammo1 : " + numOfBullets[0] + "/" + totalBullets[0] + "]\n\n" +
                            "[Ammo2 : " + numOfBullets[1] + "/" + totalBullets[1] + "]";
            
            playerInfo.coolTimeText.text = Mathf.FloorToInt(playerTime * 100).ToString();
            if(startReload) playerInfo.shotTimeText.text = Mathf.FloorToInt(reloadTime * 100).ToString();
            else playerInfo.shotTimeText.text = Mathf.FloorToInt(shotTime * 100).ToString();

//            playerInfo.moneyText.text = "Money : " + money + " $";
        }       

        private void Update ()
		{
            if (GameManager.instance.doingSetup) return;
            UpdateDisplay();
            int value = GameManager.instance.curLevel.GetMapOfStructures(transform.position);
            if (value != 0) scopeRange = value;
            else scopeRange = scopeRangeInit;
            if (myPlayer) GameManager.instance.ShowObjs(transform.position, curDir, scopeRange);
            

            if (startReload)
            {
                reloadTime -= Time.deltaTime;
                if (reloadTime <= 0)
                {
                    reloadTime = reloadTimeInit;
                    startReload = false;

                    int relaodNum = 1;
                    if(totalBullets[indexReload]>= 2) relaodNum = 2;

                    totalBullets[indexReload] -= relaodNum;
                    numOfBullets[indexReload] += relaodNum;
                }
            }
            else if (canShot)
            {
                if(myPlayer)
                {
                    int input = GetAttackInput();
                    if (input != -1) AttempAttack(input);
                }
            }
            else
            {
                shotTime -= Time.deltaTime;
                if(shotTime <=0)
                {
                    shotTime = shotTimeInit;
                    canShot = true;
                }                
            }            

            if (!playersTurn)
            {
                playerTime -= Time.deltaTime;
                if(playerTime <= 0)
                {
                    playersTurn = true;
                    playerTime = playerTimeInit;
                }
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
			if(horizontal != 0 || vertical != 0)
			{
				AttemptMove<Wall> (horizontal, vertical);
			}

            if(!myPlayer && autoMode)
            {
                AutoMove();
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
                if (canShot)
                {
                    canShot = false;
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
            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, scopeRange);
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
                if (canShot)
                {
                    canShot = false;
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

                AttemptMove<Player>(xDir, yDir);
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

            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir, scopeRange);
            Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

            bool found = false;
            foreach(Vector3 pos in showRange)
            {
                if(player.transform.position == pos)
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
                    if (canShot)
                    {
                        canShot = false;
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

        bool startReload = false;
        int indexReload = 0;
        void StartReload(int index)
        {
            startReload = true;
            indexReload = index;
        }

        public void  AttempAttack(int input)
        {
            if (numOfBullets[input] <= 0)
            {
                if(totalBullets[input] > 0)
                {
                    StartReload(input);
                }
                else
                {
                    if (myPlayer) GameManager.instance.UpdateGameMssage("No Ammo !!!", 1f);
                }
                
                return;
            }

            numOfBullets[input]--;
            //Attack(input+1);
            Attack2(input);
            canShot = false;
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
            if(xDir == 1)
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

        public void Attack(int distance)
        {
            Vector3 attackPos = transform.position;
            switch (curDir)
            {
                case MOVE_DIR.RIGHT: attackPos.x += distance; break;
                case MOVE_DIR.LEFT: attackPos.x -= distance; break;
                case MOVE_DIR.UP: attackPos.y += distance; break;
                case MOVE_DIR.DOWN: attackPos.y -= distance; break;
            }

            StartCoroutine(GameManager.instance.ShowShotEffect(attackPos));

            GameManager.instance.curLevel.AttackOtherPlayer(attackPos);
        }        


       
        protected override void AttemptMove <T> (int xDir, int yDir)
		{
            playersTurn = false;

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
			
			CheckIfGameOver ();			
		}
		
		protected override void OnCantMove <T> (T component)
		{

		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
            if (!myPlayer) return;
            if(other.tag == "Food")
			{
                if(HP < HP_MAX)
                {
                    HP += 10;
                    SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
                    other.gameObject.SetActive(false);
                }                
			}
			else if(other.tag == "Soda")
			{
                Scroll ammoObj = other.GetComponent<Scroll>();

                int index = ammoObj.type - 1;
                if(index < numOfBullets.Length)
                {
                    int maxAmmo = 4;
                    int addAmmo = ammoObj.num;
                    if (totalBullets[index] + ammoObj.num > maxAmmo)
                    {
                        addAmmo = maxAmmo - totalBullets[index];
                        ammoObj.UpdateNumber(ammoObj.num - addAmmo);
                    }
                    else
                    {
                        ammoObj.UpdateNumber(0);
                    }

                    totalBullets[index] += addAmmo;
                }
                
                if(ammoObj.num <= 0)
                    other.gameObject.SetActive(false);
			}
            else if (other.tag == "Money")
            {
                //money += 100;
                //other.gameObject.SetActive(false);
            }

        }


		private void Restart ()
		{
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		
		
    	public void LoseHP (int loss)
		{
            HP -= loss;

            if(myPlayer)
            {
                GameManager.instance.UpdateGameMssage("공격 받았다!!!!", 0.5f);
                CheckIfGameOver();
            }
            else
            {
                if(HP<=0)
                {
                    GameManager.instance.DestroyOtherPlayer(gameObject);
                    GameManager.instance.curLevel.SetMapOfUnits(gameObject.transform.position, 0);
                }
            }
		}
		
		private void CheckIfGameOver ()
		{
			if (HP <= 0) 
			{
				SoundManager.instance.PlaySingle (gameOverSound);
				SoundManager.instance.musicSource.Stop();
				GameManager.instance.GameOver ();
			}
		}
	}
}

