using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class FrontPage : MonoBehaviour
    {

        public Button startBtn;
        public Button interUniverseBtn;
        public Button outerUniverseBtn;

        void Start()
        {
            startBtn.onClick.AddListener(ShowSelectButton);
            interUniverseBtn.onClick.AddListener(GoInterUnivers);
            outerUniverseBtn.onClick.AddListener(GoOuterUnivers);

            startBtn.gameObject.SetActive(true);
            interUniverseBtn.gameObject.SetActive(false);
            outerUniverseBtn.gameObject.SetActive(false);
        }

        void GoOuterUnivers()
        {
            SoundManager.instance.PlaySingleBtn();
            GameManager.instance.numberOfUniverseEnd = 1;
            GameManager.instance.ReadyToDeparture();
        }

        void GoInterUnivers()
        {
            SoundManager.instance.PlaySingleBtn();
            GameManager.instance.numberOfUniverseEnd = 5;
            GameManager.instance.ReadyToDeparture();
        }

        void ShowSelectButton()
        {
            SoundManager.instance.PlaySingleBtn();
            startBtn.gameObject.SetActive(false);
            interUniverseBtn.gameObject.SetActive(true);
            outerUniverseBtn.gameObject.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}