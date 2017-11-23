using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class FrontPage : MonoBehaviour
    {

        public Button startBtn;
        // Use this for initialization
        void Start()
        {
            startBtn.onClick.AddListener(GameManager.instance.GoIntoGateway);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}