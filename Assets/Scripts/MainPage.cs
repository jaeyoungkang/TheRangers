using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MainPage : MonoBehaviour
    {
        public Button missionBtn;

        // Use this for initialization
        void Start()
        {
            missionBtn.onClick.AddListener(GameManager.instance.GotoMissionPage);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
