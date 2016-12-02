using System;

using NullSpace.API.Enums;
using NullSpace.API.Logger;
using UnityEngine;
using NullSpace.SDK.Haptics;
using NullSpace.SDK;

namespace NullSpace.SDK.Editor
{
    public static class Extensions
    {

		
        public static UnityEngine.Quaternion Normalize(this UnityEngine.Quaternion q)
        {
            float d = Mathf.Sqrt(q.w*q.w + q.x*q.x + q.y*q.y + q.z*q.z);
			
            return new UnityEngine.Quaternion(q.x/d, q.y/d, q.z/d, q.w/d);
        }

        public static UnityEngine.Quaternion Clone(this UnityEngine.Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static float Dot(this UnityEngine.Quaternion q, UnityEngine.Quaternion other)
        {
            return q.x * other.x + q.y * other.y + q.z * other.z + q.w * other.w;
        }
    }

    /// <summary>
    /// This system is responsible for kicking off the loading routines for haptic files,
    /// as well as providing an easy-to-use interface to play haptic files.
    /// </summary>
    public partial class NS
    {
		
		private NSVR.NSVR_Plugin _plugin;
        public NS(string path)
        {
			_plugin = new NSVR.NSVR_Plugin(path);
        }
		
		
		
		
		public void SetTrackingEnabled(bool wantTracking)
		{
			_plugin.SetTrackingEnabled(wantTracking);
		}
		public int GetSuitStatus()
		{
			return _plugin.PollStatus();
		}
		public UnityEngine.Quaternion GetTracking()
		{

			var update = _plugin.PollTracking();
			var q = update.chest;
			return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
		}
    
		
       

		
	}
}