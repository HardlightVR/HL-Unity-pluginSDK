/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NullSpace.SDK.Tracking;
using NullSpace.SDK.Enums;
using System;

namespace NullSpace.SDK.Demos
{
    public class IMUTest : MonoBehaviour
    {
    
        private Dictionary<Imu, GameObject> imus;
        public ImuInterface imuInterface;
		private Dictionary<Imu, bool> _registeredImus;
		
		void Start()
        {
            imus = new Dictionary<Imu, GameObject>();
			_registeredImus = new Dictionary<Imu, bool>();
              imus[Imu.Left_Upper_Arm] = GameObject.Find("/Akai_E_Espiritu/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm");
              imus[Imu.Right_Upper_Arm] = GameObject.Find("/Akai_E_Espiritu/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm");
            imus[Imu.Left_Forearm] = GameObject.Find("/Akai_E_Espiritu/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm");
            imus[Imu.Right_Forearm] = GameObject.Find("/Akai_E_Espiritu/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm");

			imus[Imu.Chest] = GameObject.Find("/Akai_E_Espiritu/Hips/Spine/");


			
        }

		

		void OnGUI()
		{
			if (GUI.Button(new Rect(0,100, 100, 80), "Enable Tracking"))
			{
				NSManager.Instance.HapticLoader.SetTrackingEnabled(true);
			}
			if (GUI.Button(new Rect(0, 180, 100, 80), "Disable Tracking"))
			{
				NSManager.Instance.HapticLoader.SetTrackingEnabled(false);
			}

			GUI.Label(new Rect(20, 10, 100, 30), "Online IMUs:");
			int offset = 1;
			foreach (var kvp in _registeredImus)
			{
				if (kvp.Value)
				{
					GUI.Label(new Rect(20, 30 * offset, 120, 30), kvp.Key.ToString());
					
				}
				offset += 1;
			}
		}
        // Update is called once per frame
        void Update()
        {
             foreach (var kvp in imus)
              {
                imus[kvp.Key].transform.rotation = imuInterface.GetOrientation(kvp.Key) ;
             
            }
			 
        
           // imus[Imu.Left_Forearm].transform.localRotation = quat;
        }


       

       
    }
}