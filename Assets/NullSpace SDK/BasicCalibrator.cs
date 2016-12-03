using System;
using System.Collections;
using System.Collections.Generic;
using NullSpace.API.Enums;
using NullSpace.API.Logger;
using NullSpace.SDK.Editor;
using UnityEngine;

namespace NullSpace.API.Tracking
{

    public class BasicCalibratorGUI
    {
        public delegate IEnumerator Button();

        private readonly Button _beginCalibration;
        private Texture _backPaneTexture;
        private string _instruction;
        public BasicCalibratorGUI(Button calibrationButton)
        {
            _beginCalibration = calibrationButton;
            _backPaneTexture = Resources.Load("calibration_gui_backpane") as Texture;
            
           
        }
        public void OnDraw(int counter)
        {
            int xOffset = Screen.width/2 - 128;
            int yOffset = Screen.height - 164;
            GUI.DrawTexture(new Rect(xOffset, yOffset, 256,128),_backPaneTexture );
            if (GUI.Button(new Rect(xOffset + 20, yOffset + 20, 100, 50), "Calibrate"))
            {
                ImuInterface.Instance.StartCoroutine(_beginCalibration());
            }
            GUI.Label(new Rect(xOffset + 130, yOffset + 10, 120, 70), _instruction);
            if (counter > -1)
            {
                GUI.Label(new Rect(xOffset + 30, yOffset + 84, 30, 30), counter.ToString());
            }
        }

        public void HandleStateChange(object sender, CalibrationArgs args)
        {
            
            switch (args.State)
            {
                case CalibrationState.Calibrated:
                    _instruction = "Calibrated.";
                    break;
                case CalibrationState.MeasuringAttentionPose:
                    _instruction = "Relax your arms beside your body, facing forward.";
                    break;
                case CalibrationState.MeasuringOffset:
                    _instruction = "Put your arms back down beside your body, facing forward.";
                    break;
                case CalibrationState.MeasuringTPose:
                    _instruction = "Put your arms in a T-Pose, with your palms facing down.";
                    break;
                case CalibrationState.Uncalibrated:
                    _instruction = "Get ready for calibration!";
                    break;
                default:
                    _instruction = "Idle";
                    break;
            }
        }
    }

    public class CalibratedIMU
    {

        public Quaternion ReferenceOrientation;
        private RawIMU _imu;
        public Func<Quaternion, Quaternion> QCal;
        public Func<Quaternion, Quaternion> Transformer;
        public Quaternion AttentionPoseReading;
        public Quaternion TPoseReading;
        public Quaternion InitialOffset;
        public Imu Id;
        public CalibratedIMU(Imu id, RawIMU imu, Quaternion refQ)
        {
            Transformer = q => q;
            QCal = q => q;
            _imu = imu;
            Id = id;
            ReferenceOrientation = refQ;
        }

        public Quaternion Measure()
        {

            return _imu.Orientation.Clone();
        }
        /// <summary>
        /// Run the raw orientation through a transformation before returning it.
        /// This transform does all the coordinate transforms, etc.
        /// </summary>
        public Quaternion Orientation
        {
            get { return this.Transformer(_imu.Orientation); }
        }
        /// <summary>
        /// Remaps the quaternion as recommended by paper
        /// </summary>
        public static Func<Quaternion, Quaternion> Remap
        {
            get { return q => new Quaternion(-q.x, -q.z, -q.y, q.w); }
        }



    }

    public interface ICalibrator
    {
        IEnumerator Calibrate();
        void OnDraw();

    }

    public enum CalibrationState
    {

        Uncalibrated,
        MeasuringAttentionPose,
        MeasuringTPose,
        MeasuringOffset,
        Calibrated

    }

    public class CalibrationArgs : EventArgs
    {
        public CalibrationState State;
        public CalibrationArgs(CalibrationState state)
        {
            this.State = state;
        }
    }
    public class BasicCalibrator : ICalibrator
    {
        /// <summary>
        /// Raised when the calibrator reaches a new stage, such as "calibrated"
        /// </summary>
        private readonly EventHandler<CalibrationArgs> CalibrationEvent;

        /// <summary>
        /// Gui used to display the calibration steps
        /// </summary>
        private readonly BasicCalibratorGUI _gui;
        private int _countdown;

        /// <summary>
        /// How long to wait between measurments of the imu
        /// </summary>
        private int _measureDelay = 4;
        private IDictionary<Imu, CalibratedIMU> _imus;
        private CalibrationState _state;
        public void OnDraw()
        {
            if (_state != CalibrationState.Calibrated)
            {
                _gui.OnDraw(_countdown);
            }
        }

        public Quaternion GetOrientation(Imu imu)
        {
            return _imus.ContainsKey(imu) ? _imus[imu].Orientation : Quaternion.identity;
        }

        public BasicCalibrator(IDictionary<Imu, CalibratedIMU> imus)
        {
            _imus = imus;
            _gui = new BasicCalibratorGUI(Calibrate);

            CalibrationEvent += _gui.HandleStateChange;

        }


        private void RaiseCalibrationEvent(CalibrationState state)
        {
            _state = state;
            if (CalibrationEvent != null)
            {
                CalibrationEvent(this, new CalibrationArgs(state));
            }
        }

        /// <summary>
        /// Main routine for kicking off the calibration.
        /// First we measure the attention pose, and then wait.
        /// Then we measure the Tpose, and wait.
        /// After this step we calculate the rotation axes for the upper arm IMUs. this would need
        /// to be extended for the forearms.
        /// Finally we measure the attention pose again, and calculate the 
        /// initial offsets. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator Calibrate()
        {
            RaiseCalibrationEvent(CalibrationState.MeasuringAttentionPose);
            yield return CountDownSecondsFrom(_measureDelay);
            measureAttentionPose();

            RaiseCalibrationEvent(CalibrationState.MeasuringTPose);
            yield return CountDownSecondsFrom(_measureDelay);
            measureTPose();

            if (_imus.ContainsKey(Imu.Left_Upper_Arm))
            {
                calculateRotationAxes(_imus[Imu.Left_Upper_Arm]);
            }

            if (_imus.ContainsKey(Imu.Right_Upper_Arm))
            {
                calculateRotationAxes(_imus[Imu.Right_Upper_Arm]);
            }


            RaiseCalibrationEvent(CalibrationState.MeasuringOffset);
            yield return CountDownSecondsFrom(_measureDelay);

            if (_imus.ContainsKey(Imu.Left_Upper_Arm))
            {
                measureInitialOffset(_imus[Imu.Left_Upper_Arm], _imus[Imu.Left_Upper_Arm].ReferenceOrientation);
            }
            if (_imus.ContainsKey(Imu.Right_Upper_Arm))
            {
                measureInitialOffset(_imus[Imu.Right_Upper_Arm], _imus[Imu.Right_Upper_Arm].ReferenceOrientation);
            }

            RaiseCalibrationEvent(CalibrationState.Calibrated);
        }


        private void measureAttentionPose()
        {
            foreach (var imu in _imus)
            {
                imu.Value.AttentionPoseReading = imu.Value.Measure();
            }


        }

        private void measureTPose()
        {
            foreach (var imu in _imus)
            {
                imu.Value.TPoseReading = imu.Value.Measure();
            }
        }


        private void calculateRotationAxes(CalibratedIMU imu)
        {
            //this math is straight from the paper
            var qT = CalibratedIMU.Remap(imu.TPoseReading).Normalize();
            var qA = CalibratedIMU.Remap(imu.AttentionPoseReading).Normalize();

            Quaternion _v0 = new Quaternion(0, -1, 0, 0);

            Quaternion _v1 = qT * Quaternion.Inverse(qA) * _v0 * qA * Quaternion.Inverse(qT);

            //yes, we are putting quaternion parts into vectors
            Vector3 v1 = new Vector3(_v1.x, _v1.y, _v1.z);
            Vector3 v0 = new Vector3(_v0.x, _v0.y, _v0.z);
            Vector3 v3 = Vector3.Cross(v1, v0);

            float drift = Mathf.Atan(v3.x / v3.z);
            imu.QCal = inQ => new Quaternion(-(inQ.x * Mathf.Cos(-drift) + inQ.y * Mathf.Sin(-drift)),
              -inQ.z,
              -(-inQ.x * Mathf.Sin(-drift) + inQ.y * Mathf.Cos(-drift)),
             inQ.w);
        }

        private IEnumerator CountDownSecondsFrom(int x)
        {
            _countdown = x;
            while (_countdown > 0)
            {
                yield return new WaitForSeconds(1);
                _countdown--;
            }
        }
        private void measureInitialOffset(CalibratedIMU imu, Quaternion reference)
        {
            //equation 2: qt' = q1'q0'
            //equation 3: q1' = qt'q0'*
            //equation 4: q1' = qt' q0'* qI


            imu.InitialOffset = imu.QCal(imu.Measure());
            imu.Transformer = delegate (Quaternion current)
            {
                Quaternion q1 = imu.QCal(current) * Quaternion.Inverse(imu.InitialOffset) * reference;
                return q1.Normalize();
            };

        }


    }
}