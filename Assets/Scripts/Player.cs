using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
    public enum MOVE_DIR { UP, DOWN, LEFT, RIGHT };

    public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;		
		public int pointsPerFood = 10;				
		public int pointsPerSoda = 20;				
		public int wallDamage = 1;					
		public Text foodText;						
		public Text ammoText;
        public Text coolTimeText;
        public Text shotTimeText;
        

        public AudioClip moveSound1;				
		public AudioClip moveSound2;				
		public AudioClip eatSound1;					
		public AudioClip eatSound2;					
		public AudioClip drinkSound1;				
		public AudioClip drinkSound2;				
		public AudioClip gameOverSound;				
		
		private Animator animator;					                      
		private int food;


#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif
                
        public MOVE_DIR curDir = MOVE_DIR.RIGHT;
        public GameObject explosioinInstance;
        public GameObject exploreEffect;

        public int[] numOfBullets = new int[3];
        public int[] maxNumOfBullets = new int[3];
        
        protected override void Start ()
		{
			animator = GetComponent<Animator>();
			
			food = GameManager.instance.playerFoodPoints;            
			base.Start ();

            UpdateDirImage();

            explosioinInstance = Instantiate(exploreEffect, transform.position, Quaternion.identity);
            explosioinInstance.SetActive(false);

            playerTime = playerTimeInit;
            shotTime = shotTimeInit;
        }
		
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			GameManager.instance.playerFoodPoints = food;
		}

        public float playerTimeInit = 0.5f ;
        public float shotTimeInit = 0.5f;
        float playerTime;
        float shotTime;

        bool canShot = true;

        private void Update ()
		{
            foodText.text = "HP : " + food;
            ammoText.text = "[Ammo1 : " + numOfBullets[0] + "/" + maxNumOfBullets[0] + "]\n\n" +
                            "[Ammo2 : " + numOfBullets[1] + "/" + maxNumOfBullets[1] + "]\n\n" +
                            "[Ammo3 : " + numOfBullets[2] + "/" + maxNumOfBullets[2] + "]";

            GameManager.instance.ShowObjs(transform.position);
            coolTimeText.text = Mathf.FloorToInt(playerTime*100).ToString();
            shotTimeText.text = Mathf.FloorToInt(shotTime * 100).ToString();

            if (canShot)
            {
                if(AttempAttack())
                    canShot = false;
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

            

            if (!GameManager.instance.playersTurn)
            {
                playerTime -= Time.deltaTime;
                if(playerTime <= 0)
                {
                    GameManager.instance.playersTurn = true;
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
		}

        public bool AttempAttack()
        {
            if (Input.GetKeyDown("1"))
            {
                if (numOfBullets[0] <= 0)
                {
                    GameManager.instance.UpdateGameMssage("No Ammo1 !!!", 1f);
                    return false;
                }
                else
                {
                    numOfBullets[0]--;
                    Attack(1);
                    return true;
                }

            }
            else if (Input.GetKeyDown("2"))
            {
                if (numOfBullets[1] <= 0)
                {
                    GameManager.instance.UpdateGameMssage("No Ammo2 !!!", 1f);
                    return false;
                }
                else
                {
                    numOfBullets[1]--;
                    Attack(2);
                    return true;
                }
            }
            else if (Input.GetKeyDown("3"))
            {
                if (numOfBullets[2] <= 0)
                {
                    GameManager.instance.UpdateGameMssage("No Ammo3 !!!", 1f);
                    return false;
                }
                else
                {
                    numOfBullets[2]--;
                    Attack(3);
                    return true;
                }
            }

            return false;
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

        public Sprite[] dirSprits;
        public void UpdateDirImage()
        {
            SpriteRenderer sRenderer = gameObject.GetComponent<SpriteRenderer>();
            if (sRenderer == null) return;
                        
            switch (curDir)
            {
                case MOVE_DIR.RIGHT: sRenderer.sprite = dirSprits[0]; break;
                case MOVE_DIR.LEFT: sRenderer.sprite = dirSprits[1]; break;
                case MOVE_DIR.UP: sRenderer.sprite = dirSprits[2]; break;
                case MOVE_DIR.DOWN: sRenderer.sprite = dirSprits[3]; break;
            }
        }

        public void Attack(int distance)
        {
            Vector3 targetPos = transform.position;
            switch (curDir)
            {
                case MOVE_DIR.RIGHT: targetPos.x += distance; break;
                case MOVE_DIR.LEFT: targetPos.x -= distance; break;
                case MOVE_DIR.UP: targetPos.y += distance; break;
                case MOVE_DIR.DOWN: targetPos.y -= distance; break;
            }

            StartCoroutine(ExploreTarget(targetPos));

			GameManager.instance.AttackObj (targetPos);			
        }        

        IEnumerator ExploreTarget(Vector3 targetPos)
        {
            explosioinInstance.transform.position = targetPos;
            explosioinInstance.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            explosioinInstance.SetActive(false);
        }
        protected override void AttemptMove <T> (int xDir, int yDir)
		{
            GameManager.instance.playersTurn = false;

            if (CheckDir(xDir, yDir))
            {
                ChangeDir(xDir, yDir);
                UpdateDirImage();
                return;
            }

			Vector3 nextPos = transform.position;
            			
			base.AttemptMove <T> (xDir, yDir);
			RaycastHit2D hit;
			if (Move (xDir, yDir, out hit)) {
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);

				nextPos.x += xDir;
				nextPos.y += yDir;
			}		
			
			CheckIfGameOver ();			
		}
		
		protected override void OnCantMove <T> (T component)
		{
			//Wall hitWall = component as Wall;
			
		//animator.SetTrigger ("playerChop");
		}
		
		
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit")
			{
				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);
				
				//Disable the player object since level is over.
				enabled = false;
			}
			
			//Check if the tag of the trigger collided with is Food.
			else if(other.tag == "Food")
			{
				food += pointsPerFood;				
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);				
				other.gameObject.SetActive (false);
			}
			
			//Check if the tag of the trigger collided with is Soda.
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
		}


		private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		
		
    	public void LoseFood (int loss)
		{
			food -= loss;
			CheckIfGameOver ();
		}
		
		
		private void CheckIfGameOver ()
		{
			if (food <= 0) 
			{
				SoundManager.instance.PlaySingle (gameOverSound);
				SoundManager.instance.musicSource.Stop();
				GameManager.instance.GameOver ();
			}
		}
	}
}

