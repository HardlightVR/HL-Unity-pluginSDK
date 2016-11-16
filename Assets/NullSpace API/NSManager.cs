/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
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
        private SuitHardwareInterface suit;
        public SuitHardwareInterface Suit
        {
            get { return suit; }
        }
        private PacketDispatcher packetDispatcher;

        #region Public fields 
        [Header("Default Data Model")]
        [Tooltip("The default method of layering haptic effects and providing durations")]
        /// <summary>
        /// The default DataModel exposes methods to play layered haptic effects with variable durations
        /// </summary>
        public DataModel DataModel;
   
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
        [Tooltip("Attempt to automatically reconnect to the suit if USB is unplugged")]
        [SerializeField]
        private bool AutoReconnect = true;
        [Tooltip("Use a mock serial adapter (useful if no suit is present)")]
        [SerializeField]
        private bool Mock = false;
        [Tooltip("Use a specific serial port if the system fails to auto detect")]
        [SerializeField]
        private bool OverridePort = false;
        [Tooltip("The port to use")]
        [SerializeField]
        private string PortName = "";


        #endregion

        //in seconds
        private const int AUTO_RECONNECT_INTERVAL = 3;

        private HapticsLoader loader;
        public HapticsLoader HapticLoader
        {
            get { return loader; }
        }
        /// <summary>
        /// The synchronization status of the suit with the NullSpace software
        /// </summary>
        public string SyncState
        {
            get { return this.packetDispatcher.SyncState; }
        }

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
            this.suit = new SuitHardwareInterface();
          
            this.loader = new HapticsLoader();


        }

		void EnableTracking()
		{

		}

		void DisableTracking()
		{

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
            //If the user checked mock, we should use a mock adapter. Else we should try to use a real
            //one, but it might fail, and fallback to a mock adapter. 
            suit.Adapter = Mock ? this.setupMockAdapter() : this.trySetupSerialAdapter();

            this.loader.DataModel = this.DataModel;
            packetDispatcher = new PacketDispatcher(this.suit.Adapter.suitDataStream);
            packetDispatcher.AddSubscriber(new PingConsumer(), SuitPacket.PacketType.Ping);
            if (!Mock)
            {
                StartCoroutine(SuitKeepAliveRoutine());
                StartCoroutine(AutoReconnectRoutine());
            }
        }

        private MockAdapter setupMockAdapter()
        {
            MockAdapter adapter = new MockAdapter();
            adapter.Connect();
            return adapter;
        }
        private ICommunicationAdapter trySetupSerialAdapter()
        {
        
            ICommunicationAdapter adapter = new SerialAdapter();
            bool didConnect = OverridePort ?
                               ((SerialAdapter)adapter).ConnectWithPort(PortName) :
                               adapter.Connect();
            if (didConnect)
            {
                Log.Message("Suit connected");
                OnSuitConnected(new SuitConnectionArgs());
                PortName = adapter.ToString();
                return adapter;
            } else
            {
                Log.Error("Failed to connect to suit (is it plugged in?)\n Using a mock adapter.");
                return this.setupMockAdapter();
            }


        }
        
        IEnumerator SuitKeepAliveRoutine()
        {
            while (true)
            {
                suit.PingSuit();
                yield return new WaitForSeconds(1);
            }  
        }
        
      

        IEnumerator AutoReconnectRoutine()
        {
            while (true)
            {
                yield return null;
                if (!AutoReconnect || this.suit.Adapter.IsConnected) { continue; }

                //if we make it here, the user wants auto-reconnect and the adapter is disconnected.
                //Raise the event, and try reconnecting
                OnSuitDisconnected(new SuitConnectionArgs());

                Log.Message("Attempting to reconnect to suit..");

                bool connected = OverridePort ? 
                    ((SerialAdapter)this.suit.Adapter).ConnectWithPort(PortName) : 
                    this.suit.Adapter.Connect();

                if (connected)
                {
                    OnSuitConnected(new SuitConnectionArgs());
                    Log.Message("Connection re-established.");
                    continue;
                }
                else
                {
                    yield return new WaitForSeconds(AUTO_RECONNECT_INTERVAL);
                }
            }
        }

        void Update()
        {
            //Update logging uptions during gameplay
            updateSettings();

            //Read raw data from the suit. Since packetDispatcher has it's hand in this stream,
            //we'll need to tell it to process the available data
            this.suit.Adapter.Read();

            packetDispatcher.DispatchAvailable();
        }

        void OnApplicationPause()
        {
          
        }

        void OnDestroy()
        {
            suit.Shutdown();
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
    }
}
