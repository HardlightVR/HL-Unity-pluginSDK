using UnityEngine;
using System.Collections;
using System;

namespace Hardlight.SDK.Internal
{
	[Serializable]
	[CreateAssetMenu(menuName = "Hardlight/HardlightPlugin")]
	public class HardlightPlugin : ScriptableObject
	{
		private HLVR.HLVR_Plugin _plugin = null;

		public HLVR.HLVR_Plugin Plugin
		{
			get
			{
				if (_plugin == null)
				{

					throw new MemberAccessException("[HardlightPlugin] The plugin was null!");
				}

				return _plugin;
			}
		}

		public void Awake()
		{

			//Debug.Log("[HardlightPluginScrob] Awoken");
			Debug.Assert(_plugin == null);
			_plugin = new HLVR.HLVR_Plugin();
			this.hideFlags = HideFlags.HideAndDontSave;
		}

		public void OnDestroy()
		{
		//	Debug.Log("[HardlightPluginScrob] Destroyed");
			if (_plugin != null)
			{
				_plugin.ClearAll();
				System.Threading.Thread.Sleep(25);
				_plugin.Dispose();
				_plugin = null;
			}
		}

		protected void EnsurePluginIsValid()
		{
			HardlightManager.Instance.InstantiateNativePlugin();
		}

	}
}