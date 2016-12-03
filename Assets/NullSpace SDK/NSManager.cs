using UnityEngine;
using System.Collections;
using System;
using NullSpace.API;
using NullSpace.API.Tracking;
using NullSpace.API.Logger;
using NullSpace.SDK.Editor;

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
    
   
        [Header("Logging")]
        [Tooltip("Log to the Unity Console")]
        [SerializeField]
        private bool EnableLogging = true;
        [Tooltip("Record a timestamp")]
        [SerializeField]
        private bool EnableTimestamps = false;
        [Tooltip("Toggle between real system time and elapsed game time")]
        [SerializeField]
        private bool GameTimestamp = false;

        [Header("Suit Options")]
        

        [Header("Experimental")]
        [Tooltip("EXPERIMENTAL: enabling will result in severe performance penalty. Note: currently disabled in firmware.")]
        [SerializeField]
        private bool UseImus = false;

        #endregion

        //in seconds
        private const int AUTO_RECONNECT_INTERVAL = 3;

		private NS loader;
        public NS HapticLoader
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

		private ImuInterface imuInterface;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Log.Error("NSManager Instance is not null\nThere should only be one NSManager.");
            }
			
	
            //Update logger settings upon awakening from a deep slumber
            updateSettings();

            //Create a new hardware interface, which will allow us to directly communicate with the suit
          //  this.suit = new SuitHardwareInterface();

            imuInterface = GetComponent<ImuInterface>();
		

			StreamingAssetsPath = Application.streamingAssetsPath;
			//loader = new NS(StreamingAssetsPath);
			loader = new NS(StreamingAssetsPath);
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
				imuInterface.ReceiveUpdate(API.Enums.Imu.Chest, NSManager.Instance.HapticLoader.GetTracking());
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
            //Update logging uptions during gameplay
            updateSettings();
			
            //Read raw data from the suit. Since packetDispatcher has it's hand in this stream,
            //we'll need to tell it to process what is available
          //  this.suit.Adapter.Read();

          //  packetDispatcher.DispatchAvailable();
            
        }

        void OnApplicationPause()
        {
          
        }

        void OnDestroy()
        {
			// suit.Shutdown();
	//dispose
        }


     




        private void updateSettings()
        {
            Log.Enable = EnableLogging;

            if (EnableTimestamps)
            {
                if (GameTimestamp)
                {
                    Log.SetTimestampGametime();
                }
                else
                {
                    Log.SetTimestampRealtime();
                }
            }
            else
            {
                Log.DisableTimestamps();
            }
        }

		public ImuInterface GetImuInterface()
		{
			return imuInterface;
		}
	}
}
