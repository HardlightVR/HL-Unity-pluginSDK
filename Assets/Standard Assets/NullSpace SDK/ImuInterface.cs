using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NullSpace.API.Enums;
using NullSpace.API.Logger;
using NullSpace.SDK.Editor;

using Quaternion = UnityEngine.Quaternion;
using NullSpace.SDK;
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
        public GameObject LeftUpperArm;
        public GameObject RightUpperArm;
        public GameObject LeftForearm;
        public GameObject RightForearm;
		public GameObject Chest;
        private IDictionary<Imu, RawIMU> _rawImuQuaternions;
        private IDictionary<Imu, Quaternion> _processedQuaternions;
        public IDictionary<Imu, Quaternion> _referenceObjects;

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

            _referenceObjects = getImuReferences();

         //   StartCoroutine(MonitorIMUSetup());
        }
		public void ReceiveUpdate(Imu key, Quaternion q)
		{
			_rawImuQuaternions[key].Orientation = q;
		}
        public void Update()
        {
            if (_calibrator != null)
            {
                _processedQuaternions[Imu.Left_Upper_Arm] = _calibrator.GetOrientation(Imu.Left_Upper_Arm);
                _processedQuaternions[Imu.Right_Upper_Arm] = _calibrator.GetOrientation(Imu.Right_Upper_Arm);
                _processedQuaternions[Imu.Left_Forearm] = _calibrator.GetOrientation(Imu.Left_Forearm);
                _processedQuaternions[Imu.Right_Forearm] = _calibrator.GetOrientation(Imu.Right_Forearm);
				_processedQuaternions[Imu.Chest] = _calibrator.GetOrientation(Imu.Chest);
            }
            else
            {
             //   _processedQuaternions[Imu.Left_Forearm] = _rawImuQuaternions//[Imu.Left_Forearm].Orientation;
              //  _processedQuaternions[Imu.Right_Forearm] = _rawImuQuaternions[Imu.Right_Forearm].Orientation;
				_processedQuaternions[Imu.Chest] = _rawImuQuaternions[Imu.Chest].Orientation;
			}



        }
        private class MutTuple<TKey, TValue>
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public MutTuple(TKey key, TValue val)
            {
                Key = key;
                Value = val;
            }
        }

        private IEnumerator MonitorIMUSetup()
        {

            Dictionary<Imu, MutTuple<int, Quaternion>> calibrationData =
                new Dictionary<Imu, MutTuple<int, Quaternion>>();


            bool calibrated = false;
            Log.Message("Beginning IMU calibration");
            while (!calibrated)
            {

                foreach (KeyValuePair<Imu, RawIMU> pair in _rawImuQuaternions)
                {
                    var newMeasure = pair.Value.Orientation;
                    if (!calibrationData.ContainsKey(pair.Key))
                    {
                        calibrationData[pair.Key] = new MutTuple<int, Quaternion>(0, newMeasure);
                    }

                    var dif = QuatAngleDifference(calibrationData[pair.Key].Value, newMeasure);
                    if (Mathf.Abs(dif) < 0.0003f)
                    {
                        calibrationData[pair.Key].Key++;
                    }
                    else
                    {
                        calibrationData[pair.Key].Key = 0;
                    }
                    calibrationData[pair.Key].Value = newMeasure;
                }
                yield return new WaitForSeconds(1.0f);

                calibrated = calibrationData.Keys.Count > 1 && calibrationData.All(pair => pair.Value.Key >= 5);
            }
            Log.Message("Calibration complete. Initializing human calibrator.");


            var validImus = (
                from imu in calibrationData
                where _rawImuQuaternions.ContainsKey(imu.Key) && _referenceObjects.ContainsKey(imu.Key)
                select new CalibratedIMU(imu.Key, _rawImuQuaternions[imu.Key], _referenceObjects[imu.Key]))
                .ToDictionary(imu => imu.Id);


            _calibrator = new BasicCalibrator(validImus);
        }

        private IDictionary<Imu, Quaternion> getImuReferences()
        {
            var refs = new Dictionary<Imu, Quaternion>();
            if (LeftUpperArm != null)
            {
                refs[Imu.Left_Upper_Arm] = LeftUpperArm.transform.rotation.Clone();
            }
            if (RightUpperArm != null)
            {
                refs[Imu.Right_Upper_Arm] = RightUpperArm.transform.rotation.Clone();
            }
            if (LeftForearm != null)
            {
                refs[Imu.Left_Forearm] = LeftForearm.transform.rotation.Clone();
            }
            if (RightForearm != null)
            {
                refs[Imu.Right_Forearm] = RightForearm.transform.rotation.Clone();
            }

			if (Chest != null)
			{
				refs[Imu.Chest] = Chest.transform.rotation.Clone();
			}
            return refs;
        }


        private float QuatAngleDifference(Quaternion q1, Quaternion q2)
        {
            return 2.0f * (1.0f - Mathf.Abs(q1.Dot(q2)));

        }
        public void OnGUI()
        {
            if (_calibrator != null) { 
            _calibrator.OnDraw();
               }

        }

        

    }
}