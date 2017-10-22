using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MissionPage : MonoBehaviour
    {
        public Button startBtn;
        void Start()
        {
            startBtn.onClick.AddListener(GameManager.instance.StartMission);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}