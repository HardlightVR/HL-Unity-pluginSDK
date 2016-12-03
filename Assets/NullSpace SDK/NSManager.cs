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
        /// <summary>
        /// Raised when the suit disconnects
        /// </summary>
        public event EventHandler<SuitConnectionArgs> SuitDisconnected;
        /// <summary>
        /// Raised when the suit connects
        /// </summary>
        public event EventHandler<SuitConnectionArgs> SuitConnected;
        /// <summary>
        /// Use Instance to access the NS Manager object
        /// </summary>
        public static NSManager Instance;

		public string StreamingAssetsPath;

        #region Public fields 
    
   
  
        [Header("Suit Options")]
        

        [Header("Experimental")]
        [Tooltip("EXPERIMENTAL: enabling will result in severe performance penalty. Note: currently disabled in firmware.")]
        [SerializeField]
        private bool UseImus = false;

        #endregion

        //in seconds
        private const int AUTO_RECONNECT_INTERVAL = 3;

		private NSVRPluginWrapper loader;
        public NSVRPluginWrapper HapticLoader
        {
            get { return loader; }
        }

		void EnableTracking()
		{
			this.loader.SetTrackingEnabled(true);
		}

		void DisableTracking()
		{
			this.loader.SetTrackingEnabled(false);
		}
		/// <summary>
		/// The synchronization status of the suit with the NullSpace software
		/// </summary>

		private IImuCalibrator imuInterface;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("NSManager Instance is not null\nThere should only be one NSManager.");
            }
			
	


            //Create a new hardware interface, which will allow us to directly communicate with the suit
          //  this.suit = new SuitHardwareInterface();

		

			StreamingAssetsPath = Application.streamingAssetsPath;
			//loader = new NS(StreamingAssetsPath);
			loader = new NSVRPluginWrapper(StreamingAssetsPath);
		}

		public void UseImuCalibrator(IImuCalibrator calibrator)
		{
			imuInterface = calibrator;
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
         
			StartCoroutine(CheckSuitConnection());
			if (UseImus)
			{
				StartCoroutine(UpdateTracking());
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
				if (imuInterface != null)
				{
					imuInterface.ReceiveUpdate(SDK.Enums.Imu.Chest, NSManager.Instance.HapticLoader.GetTracking());
				}
				yield return null;
			}
		}
		IEnumerator CheckSuitConnection()
		{
			int prevStatus = 0;
			bool appRunningSafely = true;
			while (appRunningSafely)
			{
				int newStatus = loader.GetSuitStatus();
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
        
        }

        void OnApplicationPause()
        {
          
        }

        void OnDestroy()
        {
			// suit.Shutdown();
	//dispose
        }


     




      

		public IImuCalibrator GetImuCalibrator()
		{
			return imuInterface;
		}
	}
}
