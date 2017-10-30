using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MissionPage : MonoBehaviour
    {
        public Button startBtn;
        public Text subText;

        void Start()
        {
            startBtn.onClick.AddListener(GameManager.instance.StartMission);
        }        

        void Update()
        {            
            subText.text = GameManager.instance.storageText;
        }
    }
}