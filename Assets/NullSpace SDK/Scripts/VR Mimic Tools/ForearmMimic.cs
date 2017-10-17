using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class ForearmMimic : MonoBehaviour
	{
		public GameObject ForearmBody;
		public GameObject ForearmVisual;
		public HardlightCollider ForearmCollider;

		public void AssignSide(ArmSide side)
		{
			if (ForearmCollider != null)
			{
				if (side == ArmSide.Left)
				{
					if (ForearmCollider.regionID.ContainsArea(AreaFlag.Forearm_Right))
					{
						ForearmCollider.MyLocation.Where = ForearmCollider.MyLocation.Where.AddArea(AreaFlag.Forearm_Left);
						ForearmCollider.MyLocation.Where = ForearmCollider.MyLocation.Where.RemoveArea(AreaFlag.Forearm_Right);
					}
				}
				else if (side == ArmSide.Right)
				{
					if (ForearmCollider.regionID.ContainsArea(AreaFlag.Forearm_Left))
					{
						ForearmCollider.MyLocation.Where = ForearmCollider.MyLocation.Where.AddArea(AreaFlag.Forearm_Right);
						ForearmCollider.MyLocation.Where = ForearmCollider.MyLocation.Where.RemoveArea(AreaFlag.Forearm_Left);
					}
				}
			}
		}
	}
}