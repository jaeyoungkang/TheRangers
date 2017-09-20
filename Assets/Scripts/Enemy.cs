﻿using UnityEngine;
using System.Collections;

namespace Completed
{
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Enemy : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.
		
		
		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private Transform target;							//Transform to attempt to move toward each turn.

		public int hp = 2;
		public float dodgeRate = 0.3f;

		public bool BeDamaged(int point)
		{
			int randomIndex = Random.Range (0, 5);
			if (randomIndex == 0) {
				GameManager.instance.UpdateGameMssage ("Dodged!", 1f);
				return false;
			} else if (randomIndex == 1) {
				GameManager.instance.UpdateGameMssage ("Critical Damaged! " + point*2, 1f);
				UpdateHp (point*2);
			}
			else {
				GameManager.instance.UpdateGameMssage ("Damaged! " + point, 1f);
				UpdateHp (point);
			}

			return true;
		}

		public void UpdateHp(int point)
		{
			hp -= point;
			if (hp <= 0)
				GameManager.instance.DestroyEnemy (gameObject);
		}
			

		protected override void Start ()
		{
			GameManager.instance.AddEnemyToList (this);
			
			animator = GetComponent<Animator> ();
			
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			
			//Call the start function of our base class MovingObject.
			base.Start ();
		}
		
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			base.AttemptMove <T> (xDir, yDir);			
		}
		

        public bool SearchEnemy(float deltaX, float deltaY, float range)
        {
            if (deltaX + deltaY  <= range + float.Epsilon)
                return true;

            return false;
        }
				
		public void MoveEnemy ()
		{
			int xDir = 0;
			int yDir = 0;
            float deltaX = Mathf.Abs(target.position.x - transform.position.x);
            float deltaY = Mathf.Abs(target.position.y - transform.position.y);

            int type = GameManager.instance.GetFloorType(transform.position);
            if (type == 0) type = 1;

            if (SearchEnemy(deltaX, deltaY, type))
            {
                int rand = Random.Range(0, 3);
                if (rand == 0)
                {
                    if (deltaX < float.Epsilon)
                        yDir = target.position.y > transform.position.y ? 1 : -1;
                    else
                        xDir = target.position.x > transform.position.x ? 1 : -1;
                }
                else if (rand == 1)
                {
                    if (deltaY < float.Epsilon)
                        xDir = target.position.x > transform.position.x ? 1 : -1;
                    else
                        yDir = target.position.y > transform.position.y ? 1 : -1;                    
                }                
            }
            else
            {
                int rand = Random.Range(0, 3);

                if (rand == 0)
                {
                    xDir = Random.Range(0, 2) == 0 ? 1 : -1;
                }
                else if (rand == 1)
                {
                    yDir = Random.Range(0, 2) == 0 ? 1 : -1;
                }
            }			

			AttemptMove <Player> (xDir, yDir);
		}
		
		
		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;
			
			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);
			
			//Set the attack trigger of animator to trigger Enemy attack animation.
			animator.SetTrigger ("enemyAttack");
			
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}
	}
}
