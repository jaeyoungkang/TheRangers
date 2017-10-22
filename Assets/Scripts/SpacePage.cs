using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class SpacePage : MonoBehaviour
    {
        public GameObject result;
        public Button backBtn;
        public Text resultMsg;

        // Use this for initialization
        void Start()
        {
            result.SetActive(false);
            backBtn.onClick.AddListener(GameManager.instance.GotoMain);
        }

        public void ShowResult(string msg)
        {
            resultMsg.text = msg;
            result.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}