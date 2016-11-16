/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using UnityEngine;
using System;
using NullSpace.API.Enums;

namespace NullSpace.API.Tracking
{

    /// <summary>
    /// The IMU class stores quaternion data and a hardware Imu id 
    /// </summary>
    public class IMU
    {
       
        private Imu id;
        private Quaternion offset;
        private Quaternion quaternion;

        public IMU(Imu id, string name)
        {
            this.quaternion = new Quaternion();
            this.offset = Quaternion.identity;
            this.id = id;
        }

        public Imu Id
        {
            get
            {
                return id;
            }
        }

        

        public Quaternion Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
            }
        }

        public Quaternion Orientation
        {
            get
            {
                return quaternion;
            }
            set
            {
                quaternion = value;
            }
        }



    }
}

