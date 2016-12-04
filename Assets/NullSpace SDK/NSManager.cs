/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections;
using System;
using NullSpace.SDK;
using NullSpace.SDK.Tracking;



namespace NullSpace.SDK
{
    /// <summary>
    /// NSManager is one of the main points of entry to the suit 
    /// </summary>
	public sealed class NSManager : MonoBehaviour
    {
		#region Events 
		/// <summary>
		/// Raised when the suit disconnects
		/// </summary>
		public event EventHandler<SuitConnectionArgs> SuitDisconnected;
        /// <summary>
        /// Raised when the suit connects
        /// </summary>
        public event EventHandler<SuitConnectionArgs> SuitConnected;
		#endregion

		/// <summary>
		/// Use the Instance variable to access the NSManager object
		/// </summary>
		public static NSManager Instance;

        #region Suit Options 
   
        [Header("- Suit Options -")]
        [Tooltip("EXPERIMENTAL: may impact performance of haptics on suit, and data refresh rate may be low")]
        [SerializeField]
        private bool EnableSuitTracking = false;
		private bool _lastSuitTrackingEnabledValue = false;
		private bool _isTrackingCoroutineRunning = false;
        #endregion

		private NSVRPluginWrapper _pluginWrapper;
		private IImuCalibrator _imuCalibrator;
		private IEnumerator _trackingUpdateLoop;

		/// <summary>
		/// Enable experimental tracking on the suit. Only the chest is enabled.
		/// </summary>
		public void EnableTracking()
		{
			
			EnableSuitTracking = true;
			if (!_isTrackingCoroutineRunning)
			{
				StartCoroutine(_trackingUpdateLoop);
				_isTrackingCoroutineRunning = true;
			}
			_pluginWrapper.SetTrackingEnabled(true);
			
		}

		/// <summary>
		/// Disable experimental tracking on the suit
		/// </summary>
		public void DisableTracking()
		{
			EnableSuitTracking = false;
			StopCoroutine(_trackingUpdateLoop);
			_isTrackingCoroutineRunning = false;
			_pluginWrapper.SetTrackingEnabled(false);
		}


		/// <summary>
		/// Tell the manager to use a different IMU calibrator
		/// </summary>
		/// <param name="calibrator">A custom calibrator which will receive raw orientation data from the suit in order to calibrate it for your game</param>
		public void SetImuCalibrator(IImuCalibrator calibrator)
		{
			((CalibratorWrapper)_imuCalibrator).SetCalibrator(calibrator);
		}

		void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There should only be one NSManager! Make sure there is only one NSManager prefab in the scene");
            }

			_imuCalibrator = new CalibratorWrapper(new MockImuCalibrator());
			_pluginWrapper = new NSVRPluginWrapper(Application.streamingAssetsPath);
			_trackingUpdateLoop = UpdateTracking();

		}


		private void OnSuitConnected(SuitConnectionArgs a)
        {
            var handler = SuitConnected;
            if (handler != null) { handler(this, a); }
        }

        private void OnSuitDisconnected(SuitConnectionArgs a)
        {
            var handler = SuitDisconnected;
            if (handler != null) { handler(this, a); }
        }

        public void Start()
        {
			//Begin monitoring the status of the suit, so that we can raise relevent events
			StartCoroutine(CheckSuitConnection());
			_lastSuitTrackingEnabledValue = EnableSuitTracking;

			if (EnableSuitTracking)
			{

				StartCoroutine(_trackingUpdateLoop);
				_isTrackingCoroutineRunning = true;
				this.SuitConnected += ActivateImus;
			}
		}

		
		private void ActivateImus(object sender, SuitConnectionArgs e)
		{
			this.EnableTracking();
		}

		IEnumerator UpdateTracking()
		{
			while (true)
			{
				
				_imuCalibrator.ReceiveUpdate(_pluginWrapper.GetTrackingUpdate());

				yield return null;
			}
		}
		IEnumerator CheckSuitConnection()
		{
			int prevStatus = 0;
			while (true)
			{
				int newStatus = _pluginWrapper.GetSuitStatus();
				if (newStatus != prevStatus)
				{
					if (newStatus == 2)
					{
						OnSuitConnected(new SuitConnectionArgs());
					}
					if (newStatus == 0)
					{
						OnSuitDisconnected(new SuitConnectionArgs());
					}
					prevStatus = newStatus;
				}
				
				yield return new WaitForSeconds(0.25f);
			}
		}
     
       
    

        void Update()
        {
			if (_lastSuitTrackingEnabledValue != EnableSuitTracking)
			{
				if (EnableSuitTracking)
				{
					this.EnableTracking();
				}else
				{
					this.DisableTracking();
				}
				_lastSuitTrackingEnabledValue = EnableSuitTracking;
			}
        }

        void OnApplicationPause()
        {
          
        }

        void OnDestroy()
        {
		
        }


     




      

		public IImuCalibrator GetImuCalibrator()
		{
			return _imuCalibrator;
		}
	}
}
