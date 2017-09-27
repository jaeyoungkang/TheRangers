using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };

    public class Player : MovingObject
	{
        public bool autoMode = true;
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
		public int pointsPerFood = 10;				

		private int HP;
        private int money = 0;

        public float playerTimeInit = 0.5f;
        public float shotTimeInit = 0.5f;
        float playerTime;
        float shotTime;
        bool playersTurn = true;


#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif

        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public GameObject[] dirSprits;

        public int[] numOfBullets = new int[3];
        public int[] maxNumOfBullets = new int[3];
        
        protected override void Start ()
		{
            HP = GameManager.instance.playerFoodPoints;            
			base.Start ();

            UpdateDirImage();
                        
            playerTime = playerTimeInit;
            shotTime = shotTimeInit;

            if(!myPlayer)
            {
                GameManager.instance.AddOtherPlayerToList(this);

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
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			GameManager.instance.playerFoodPoints = HP;
		}
               

        bool canShot = true;
        
        private void UpdateDisplay()
        {
            if (display == null) return;
            PlayerInfo playerInfo = display.GetComponent<PlayerInfo>();

            playerInfo.foodText.text = "HP : " + HP;
            playerInfo.ammoText.text = "[Ammo1 : " + numOfBullets[0] + "/" + maxNumOfBullets[0] + "]\n\n" +
                            "[Ammo2 : " + numOfBullets[1] + "/" + maxNumOfBullets[1] + "]\n\n" +
                            "[Ammo3 : " + numOfBullets[2] + "/" + maxNumOfBullets[2] + "]";
            
            playerInfo.coolTimeText.text = Mathf.FloorToInt(playerTime * 100).ToString();
            playerInfo.shotTimeText.text = Mathf.FloorToInt(shotTime * 100).ToString();
            playerInfo.moneyText.text = "Money : " + money + " $";
        }


        private void Update ()
		{
            if (GameManager.instance.doingSetup) return;
            UpdateDisplay();
            if(myPlayer) GameManager.instance.ShowObjs(transform.position, curDir);

            if (canShot)
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
            if (Input.GetKeyDown("3")) return 2;
            
            return -1;
        }

        private Vector3 targetPos = Vector3.zero; 
        public void AutoMove()
        {
            List<Vector3> showRange = GameManager.instance.GetShowRange(transform.position, curDir);
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
                if (deltaX < Mathf.Epsilon|| deltaY < Mathf.Epsilon)
                {                    
                    if(canShot)
                    {
                        StartCoroutine(GameManager.instance.ShowShotEffect(player.transform.position));
                        player.LoseHP(10);
                        canShot = false;
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

        public void  AttempAttack(int input)
        {
            if (numOfBullets[input] <= 0)
            {
                if(myPlayer) GameManager.instance.UpdateGameMssage("No Ammo !!!", 1f);
                return;
            }

            numOfBullets[input]--;
            Attack(input+1);
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

            GameManager.instance.AttackObj (attackPos);
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

			if (GameManager.instance.GetMapValue (nextPos) == 1)
				return;
					
			base.AttemptMove <T> (xDir, yDir);
			RaycastHit2D hit;
			if (Move (xDir, yDir, out hit)) {
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);

				GameManager.instance.SetMap (transform.position, 0);
				GameManager.instance.SetMap (nextPos, 1);
			}		
			
			CheckIfGameOver ();			
		}
		
		protected override void OnCantMove <T> (T component)
		{

		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
			//if(other.tag == "Exit")
			//{
			//	Invoke ("Restart", restartLevelDelay);
				
			//	enabled = false;
			//}
			//else 
            if(other.tag == "Food")
			{
                HP += pointsPerFood;				
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);				
				other.gameObject.SetActive (false);
			}
			else if(other.tag == "Soda")
			{
                Scroll ammoObj = other.GetComponent<Scroll>();

                int index = ammoObj.type - 1;
                if(index < numOfBullets.Length)
                {
                    int addAmmo = ammoObj.num;
                    if (numOfBullets[index] + ammoObj.num > maxNumOfBullets[index])
                    {
                        addAmmo = maxNumOfBullets[index] - numOfBullets[index];
                        ammoObj.UpdateNumber(ammoObj.num - addAmmo);
                    }
                    else
                    {                        
                        ammoObj.UpdateNumber(0);                        
                    }

                    numOfBullets[index] += addAmmo;

                    if (numOfBullets[index] > maxNumOfBullets[index])
                        numOfBullets[index] = maxNumOfBullets[index];
                }
                
                if(ammoObj.num <= 0)
                    other.gameObject.SetActive(false);
			}
            else if (other.tag == "Money")
            {
                money += 100;
                other.gameObject.SetActive(false);
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
                    GameManager.instance.SetMap(gameObject.transform.position, 0);
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

