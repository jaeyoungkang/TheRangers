using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed
{
    public class MainPage : MonoBehaviour
    {
        public Button missionBtn;
        public Button garageBtn;
        public GameObject garagePanel;

        public Button moveSetupBtn;
        public Button weapon1SetupBtn;
        public Button weapon2SetupBtn;
        public Button reloadSetupBtn;
        public Button scopeSetupBtn;
        public Button sheildSetupBtn;

        public GameObject setupPanel;
        public Button setupOkBtn;
        public Button setupCancelBtn;

        void Start()
        {
            setupPanel.SetActive(false);
            garagePanel.SetActive(false);
            missionBtn.onClick.AddListener(GameManager.instance.GotoMissionListPage);
            garageBtn.onClick.AddListener(OpenGaragePanel);
            moveSetupBtn.onClick.AddListener(OpenSetupPanel);

            setupOkBtn.onClick.AddListener(ChnageSetup);
            setupCancelBtn.onClick.AddListener(CloseSetupPanel);
        }

        void ChnageSetup()
        {
            CloseSetupPanel();
        }

        void CloseSetupPanel()
        {
            setupPanel.SetActive(false);
        }

        void OpenSetupPanel()
        {
            setupPanel.SetActive(true);
        }

        void OpenGaragePanel()
        {
            garagePanel.SetActive(!garagePanel.activeSelf);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
