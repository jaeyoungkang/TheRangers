using UnityEngine;
using System.Collections;

namespace Completed
{	
	public class Loader : MonoBehaviour 
	{
		public GameObject gameManager;			//GameManager prefab to instantiate.
		public GameObject soundManager;         //SoundManager prefab to instantiate.
        public Camera camera;
        public int MaxSize = 24;
        public int MinSize = 8;
		void Awake ()
		{
			//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
			if (GameManager.instance == null)
				
				//Instantiate gameManager prefab
				Instantiate(gameManager);
			
			//Check if a SoundManager has already been assigned to static variable GameManager.instance or if it's still null
			if (SoundManager.instance == null)
				
				//Instantiate SoundManager prefab
				Instantiate(soundManager);
		}

        void Start()
        {
            camera.orthographicSize = MinSize;
        }

        // Update is called once per frame
        void Update()
        {
            int speed = 10;

            if (Input.GetKey(KeyCode.Q)) // Change From Q to anyother key you want
            {
                camera.orthographicSize = camera.orthographicSize + speed * Time.deltaTime;
                if (camera.orthographicSize > MaxSize)
                {
                    camera.orthographicSize = MaxSize; // Max size
                }
            }


            if (Input.GetKey(KeyCode.E)) // Also you can change E to anything
            {
                camera.orthographicSize = camera.orthographicSize - speed * Time.deltaTime;
                if (camera.orthographicSize < MinSize)
                {
                    camera.orthographicSize = MinSize; // Min size 
                }
            }

            
            Vector3 newPos = camera.transform.position;
            if (Input.GetKey(KeyCode.I))
            {
                newPos.y += speed * Time.deltaTime;
                camera.transform.position = newPos;
            }
            if (Input.GetKey(KeyCode.J))
            {
                newPos.x -= speed * Time.deltaTime;
                camera.transform.position = newPos;
            }
            if (Input.GetKey(KeyCode.K))
            {
                newPos.y -= speed * Time.deltaTime;
                camera.transform.position = newPos;
            }
            if (Input.GetKey(KeyCode.L))
            {
                newPos.x += speed * Time.deltaTime;
                camera.transform.position = newPos;
            }


        }
    }
}