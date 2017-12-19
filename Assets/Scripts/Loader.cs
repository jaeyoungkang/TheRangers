using UnityEngine;
using System.Collections;

namespace Completed
{	
	public class Loader : MonoBehaviour 
	{
		public GameObject gameManager;
        public GameObject efxManager;
        public GameObject soundManager;

        void Awake ()
		{
			//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
			if (GameManager.instance == null)				
				Instantiate(gameManager);
			
			//Check if a SoundManager has already been assigned to static variable GameManager.instance or if it's still null
			if (SoundManager.instance == null)
				Instantiate(soundManager);

            if (EffectManager.instance == null)
                Instantiate(efxManager);
        }
    }
}