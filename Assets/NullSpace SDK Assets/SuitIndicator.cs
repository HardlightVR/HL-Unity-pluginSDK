using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NullSpace.SDK
{
    public class SuitIndicator : MonoBehaviour
    {
        private Sprite suitDisconnectedSprite;
        private Sprite suitConnectedSprite;
        
        public void Awake()
        {
            suitConnectedSprite = Resources.Load<Sprite>("suit_on");
            suitDisconnectedSprite = Resources.Load<Sprite>("suit_off");
           
        }
		public void Start()
		{
			NSManager.Instance.SuitConnected += HandleSuitConnect;
			NSManager.Instance.SuitDisconnected += HandleSuitDisconnect;
		}
        void HandleSuitConnect(object sender, SuitConnectionArgs s)
        {
            this.GetComponent<Image>().sprite = suitConnectedSprite;
        }

        void HandleSuitDisconnect(object sender, SuitConnectionArgs s)
        {
            this.GetComponent<Image>().sprite = suitDisconnectedSprite;
        }
    }
}
