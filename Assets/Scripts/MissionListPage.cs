using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MissionListPage : MonoBehaviour
    {
        public Button mission1;

        // Use this for initialization
        void Start()
        {
            mission1.onClick.AddListener(GameManager.instance.GotoMission);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}