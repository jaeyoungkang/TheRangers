using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class SpacePage : MonoBehaviour
    {        
        public GameObject rootPanel;
        public Button[] rootItemBtns;
        public Button[] myItemBtns;
        public Button closeRootPanelBtn;
        public Button openRootPanelBtn;

        public GameObject result;
        public Button backBtn;
        public Text resultMsg;

        // Use this for initialization
        void Start()
        {            
            backBtn.onClick.AddListener(GameManager.instance.GotoMain);
            openRootPanelBtn.onClick.AddListener(ShowRootPanel);
            closeRootPanelBtn.onClick.AddListener(HideRootPanel);
        }
        
        void OnEnable()
        {               
            result.SetActive(false);
            HideRootPanel();
        }

        public void HideRootPanel()
        {
            rootPanel.SetActive(false);
        }

        public void ShowRootPanel()
        {
            rootPanel.SetActive(true);
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