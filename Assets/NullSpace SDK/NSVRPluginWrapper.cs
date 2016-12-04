﻿/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;

namespace NullSpace.SDK
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
	/// A simple wrapper to interface with the NullSpace plugin
	/// </summary>
    public class NSVRPluginWrapper
    {
		
		private NSVR.NSVR_Plugin _plugin;

        public NSVRPluginWrapper(string path)
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

		public void PauseAllEffects()
		{
			_plugin.PauseAll();
		}

		public void ResumeAllEffects()
		{
			_plugin.ResumeAll();
		}

		public void ClearAllEffects()
		{
			_plugin.ClearAll();
		}
		public TrackingUpdate GetTrackingUpdate()
		{
			return _plugin.PollTracking();
		}
    
	}
}