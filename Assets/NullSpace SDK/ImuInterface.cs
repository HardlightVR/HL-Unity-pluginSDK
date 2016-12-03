/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections.Generic;
using NullSpace.API.Enums;
using NullSpace.API.Logger;

using Quaternion = UnityEngine.Quaternion;
namespace NullSpace.API.Tracking
{

	public class RawIMU
    {
        public Quaternion Orientation;

        public RawIMU(Quaternion q)
        {
            Orientation = q;
        }
    }
    public class ImuInterface : MonoBehaviour
    {
     
        private IDictionary<Imu, RawIMU> _rawImuQuaternions;
        private IDictionary<Imu, Quaternion> _processedQuaternions;

        public IDictionary<Imu, Quaternion> Orientations
        {
            get { return _processedQuaternions; }
        }

        private static ImuInterface _instance = null;
        public static ImuInterface Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                Log.Error("There should only be one ImuInterface instantiated!");
                return null;
            }
        }

        private BasicCalibrator _calibrator;

       
        public void Awake()
        {
            _instance = this;
            _rawImuQuaternions = new Dictionary<Imu, RawIMU>();
            _processedQuaternions = new Dictionary<Imu, Quaternion>();
			_rawImuQuaternions[Imu.Chest] = new RawIMU(new Quaternion());

        }

        public Quaternion GetOrientation(Imu imu)
        {
            if (_processedQuaternions.ContainsKey(imu))
            {
                return _processedQuaternions[imu];
            }
            else
            {
                return Quaternion.identity;
            }
        }
        public void Start()
        {


        }
		public void ReceiveUpdate(Imu key, Quaternion q)
		{
			_rawImuQuaternions[key].Orientation = q;
		}
        public void Update()
        {
           
				_processedQuaternions[Imu.Chest] = _rawImuQuaternions[Imu.Chest].Orientation;
			


        }
  

      



        

    }
}