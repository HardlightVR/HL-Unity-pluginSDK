/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using System.Collections;
using System;
using NullSpace.SDK.Tracking;

namespace NullSpace.SDK
{
	
	/// <summary>
	/// NSManager provides access to a essential suit functions, 
	/// including enabling/disabling tracking, monitoring suit connection status, 
	/// globally pausing and playing effects, and clearing all playing effects.
	/// 
	/// If you prefer to interact directly with the plugin, you may instantiate and destroy your own
	/// NSVR_Plugin and remove NSManager.
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
		/// Use the Instance variable to access the NSManager object. There should only be one NSManager
		/// in the scene. 
		/// </summary>
		private static NSManager instance;
		public static NSManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<NSManager>();

					if (FindObjectsOfType<NSManager>().Length > 1)
					{
						Debug.LogError("[NSManager] There is more than one NSManager Singleton\n" +
							"There shouldn't be multiple NSManager objects");
						return instance;
					}

					if (instance == null)
					{
						GameObject singleton = new GameObject();
						instance = singleton.AddComponent<NSManager>();
						singleton.name = "NSManager [Runtime Singleton]";
					}
					else
					{
						//Debug.Log("[Singleton] Using instance already created: " +
						//	_instance.gameObject.name + "\n");
					}
				}
				return instance;
			}
			set { instance = value; }
		}

		#region Suit Options 
		[Header("- Suit Options -")]
		[Tooltip("EXPERIMENTAL: may impact performance of haptics on suit, and data refresh rate may be low")]
		[SerializeField]
		private bool EnableSuitTracking = false;
		//[Tooltip("Creates a suit connection indicator on runtime.")]
		//[SerializeField]
		//private bool CreateDebugDisplay = false;
		#endregion

		private bool _lastSuitTrackingEnabledValue = false;
		private bool _isTrackingCoroutineRunning = false;
		private bool _isFrozen = false;

		private IImuCalibrator _imuCalibrator;
		private IEnumerator _trackingUpdateLoop;
		private IEnumerator _suitStatusLoop;

		private SuitStatus _suitStatus;


		private NSVR.NSVR_Plugin _plugin;

		/// <summary>
		/// Enable experimental tracking on the suit. Only the chest sensor is enabled.
		/// </summary>
		public void EnableTracking()
		{
			EnableSuitTracking = true;
			if (!_isTrackingCoroutineRunning)
			{
				StartCoroutine(_trackingUpdateLoop);
				_isTrackingCoroutineRunning = true;
			}
			_plugin.EnableTracking();
		}

		/// <summary>
		/// Disable experimental tracking on the suit
		/// </summary>
		public void DisableTracking()
		{
			EnableSuitTracking = false;
			StopCoroutine(_trackingUpdateLoop);
			_isTrackingCoroutineRunning = false;
			_plugin.DisableTracking();
		}


		/// <summary>
		/// Tell the manager to use a different IMU calibrator
		/// </summary>
		/// <param name="calibrator">A custom calibrator which will receive raw orientation data from the suit and calibrate it for your game. Create a class that implements IImuCalibrator and pass it to this method to receive data.</param>
		public void SetImuCalibrator(IImuCalibrator calibrator)
		{
			((CalibratorWrapper)_imuCalibrator).SetCalibrator(calibrator);
		}

		private SuitStatus ChangeSuitStatus(SuitStatus newStatus)
		{
			if (newStatus == SuitStatus.Connected)
			{
				OnSuitConnected(new SuitConnectionArgs());
			}
			else
			{
				OnSuitDisconnected(new SuitConnectionArgs());
			}
			return newStatus;
		}

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if(Instance != this)
			{
				Debug.LogError("There should only be one NSManager! Make sure there is only one NSManager prefab in the scene\n" +
					"If there is no NSManager, one will be created for you!");
			}

			_imuCalibrator = new CalibratorWrapper(new MockImuCalibrator());

			//The plugin needs to load resources from your app's Streaming Assets folder
			_plugin = new NSVR.NSVR_Plugin();

			_trackingUpdateLoop = UpdateTracking();
			_suitStatusLoop = CheckSuitConnection();

			DoDelayedAction(1.0f, delegate ()
			{
				_suitStatus = ChangeSuitStatus(_plugin.PollStatus());
				StartCoroutine(_suitStatusLoop);
			});

		}
		private void DoDelayedAction(float delay, Action action)
		{
			StartCoroutine(DoDelayedActionHelper(delay, action));
		}
		private IEnumerator DoDelayedActionHelper(float delay, Action action)
		{
			yield return new WaitForSeconds(delay);
			action();
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
			//Begin monitoring the status of the suit
			StartCoroutine(_suitStatusLoop);
			_lastSuitTrackingEnabledValue = EnableSuitTracking;

			if (EnableSuitTracking)
			{
				StartCoroutine(_trackingUpdateLoop);
				_isTrackingCoroutineRunning = true;
				this.SuitConnected += ActivateImus;
			}
		}

		/// <summary>
		/// For use in application pause routine. Pauses currently executing haptic effects and is a no-op if called more than once. 
		/// </summary>
		public void FreezeActiveEffects()
		{
			if (_isFrozen)
			{
				Debug.LogWarning("FreezeActiveEffects() and UnfreezeActiveEffects() are intended for use in an application's play/pause routines: FreezeActiveEffects() should be followed by UnfreezeActiveEffects().");
				return;
			}
			_plugin.PauseAll();
			_isFrozen = true;
		}

		/// <summary>
		/// For use in an application unpause routine. Resumes effects that were paused by FreezeActiveEffects(). If the effects were paused by you, i.e. mySequence.Pause(), they will remain paused.
		/// </summary>
		public void UnfreezeActiveEffects()
		{
			_plugin.ResumeAll();
			_isFrozen = false;

		}

		/// <summary>
		/// Cancels and destroys all effects immediately, invalidating any HapticHandles
		/// </summary>
		public void ClearAllEffects()
		{
			_plugin.ClearAll();
		}


		private void ActivateImus(object sender, SuitConnectionArgs e)
		{
			this.EnableTracking();
		}


		IEnumerator UpdateTracking()
		{
			while (true)
			{
				_imuCalibrator.ReceiveUpdate(_plugin.PollTracking());
				yield return null;
			}
		}

		IEnumerator CheckSuitConnection()
		{
			while (true)
			{
				var status = _plugin.PollStatus();
				if (status != _suitStatus)
				{
					_suitStatus = ChangeSuitStatus(status);
				}
				yield return new WaitForSeconds(0.15f);
			}
		}

		void Update()
		{
			if (_lastSuitTrackingEnabledValue != EnableSuitTracking)
			{
				_plugin.SetTrackingEnabled(EnableSuitTracking);
				_lastSuitTrackingEnabledValue = EnableSuitTracking;
			}
		}



		void OnApplicationQuit()
		{
			_plugin.DisableTracking();
			ClearAllEffects();
			System.Threading.Thread.Sleep(100);
		}

		/// <summary>
		/// Retrieve the current IMU calibration utility
		/// </summary>
		/// <returns></returns>
		public IImuCalibrator GetImuCalibrator()
		{
			return _imuCalibrator;
		}

	}
}
