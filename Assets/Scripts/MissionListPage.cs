using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MissionListPage : MonoBehaviour
    {
        public Button mission1;
        public Button mission2;

        // Use this for initialization
        void Start()
        {
            mission1.onClick.AddListener(()=>GameManager.instance.GotoMission(1));
            mission2.onClick.AddListener(() => GameManager.instance.GotoMission(2));
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}