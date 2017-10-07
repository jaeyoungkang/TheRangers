using UnityEngine;
using System.Collections;

namespace Completed
{	
	public class Loader : MonoBehaviour 
	{
		public GameObject gameManager;			//GameManager prefab to instantiate.
		public GameObject soundManager;         //SoundManager prefab to instantiate.
        public Camera camera;
        public int MaxSize = 16;
        public int MinSize = 8;
        public Vector3 WorldPos = new Vector3(9.5f, 6f, -10f);
        public Vector3 LocalPos = new Vector3(4.5f, 3.5f, -10f);
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
            camera.orthographicSize = MaxSize;
            camera.transform.position = WorldPos;
        }

        const int WORLD_VIEW = 1;
        const int LOCAL_VIEW = 2;
        int curViewMode = 1;
        int preViewMode = 1;

        // Update is called once per frame
        void Update()
        {
            int speed = 10;
            if (curViewMode != preViewMode)
            {
                if(curViewMode == WORLD_VIEW)
                {
                    Vector3 dir = WorldPos - LocalPos;
                    dir.Normalize();
                    dir *= speed * Time.deltaTime;
                    camera.transform.position = camera.transform.position + dir;
                    if (camera.transform.position.x > WorldPos.x) camera.transform.position = WorldPos;

                    camera.orthographicSize = camera.orthographicSize + speed * Time.deltaTime;
                    
                    if (camera.orthographicSize > MaxSize)
                    {
                        camera.orthographicSize = MaxSize;
                        preViewMode = curViewMode;
                    }
                }

                if (curViewMode == LOCAL_VIEW)
                {
                    Vector3 dir = WorldPos - LocalPos;
                    dir.Normalize();
                    dir *= speed * Time.deltaTime;
                    camera.transform.position = camera.transform.position - dir;
                    if (camera.transform.position.x < LocalPos.x) camera.transform.position = LocalPos;

                    camera.orthographicSize = camera.orthographicSize - speed * Time.deltaTime;
                    if (camera.orthographicSize < MinSize)
                    {
                        camera.orthographicSize = MinSize;
                        preViewMode = curViewMode;
                    }
                }                    
            }

            if (curViewMode == preViewMode && Input.GetKey(KeyCode.Q))
            {
                if (curViewMode == WORLD_VIEW) curViewMode = LOCAL_VIEW;
                else curViewMode = WORLD_VIEW;
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