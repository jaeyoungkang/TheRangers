using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MainPage : MonoBehaviour
    {
        public Button missionBtn;

        void Start()
        {
            missionBtn.onClick.AddListener(GameManager.instance.GoIntoGateway);
        }
    }
}
