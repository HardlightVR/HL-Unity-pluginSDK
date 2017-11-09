using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// A component that stores the tooltip information.
	/// For the act of tooltip display look at ExplorerTooltip.cs
	/// </summary>
	[RequireComponent(typeof(EventTrigger))]
	public class TooltipDescriptor : MonoBehaviour, IScrollHandler
	{
		public string TooltipName;
		[Multiline]
		public string DetailedTooltip;
		[SerializeField]
		private Color32 backgroundColor = new Color32(255, 255, 255, 200);
		public Color32 BackgroundColor
		{
			get { return backgroundColor; }
			set { backgroundColor = value; }
		}
		private ScrollRect myScrollRect;

		EventTrigger et;

		void Start()
		{
			et = GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				//Debug.Log("Show " + TooltipName + "\n");
				ExplorerTooltip.Tooltip.ShowTooltip(this);
			}
			);
			et.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				//Debug.Log("Left " + TooltipName + "\n");

				ExplorerTooltip.Tooltip.HideTooltip();
			}
			);
			et.triggers.Add(entry);
		}

		/// <summary>
		/// Implementing IScrollHandle
		/// (finds the likely nearby ScrollRect parent(up 2 layers) and then accesses it's scrollbar)
		/// </summary>
		/// <param name="eventData"></param>
		public void OnScroll(PointerEventData eventData)
		{
			//Debug.Log("Scrolling: " + eventData.scrollDelta + "\n");

			if (gameObject.transform.parent != null)
			{
				if (myScrollRect == null)
				{
					myScrollRect = RecursiveFindParentWithScrollRect(transform);
				}

				if (myScrollRect != null)
				{
					myScrollRect.verticalScrollbar.value += eventData.scrollDelta.y / 10;
				}
			}
		}

		private ScrollRect RecursiveFindParentWithScrollRect(Transform currentTransform)
		{
			if (currentTransform == null)
			{
				return null;
			}
			ScrollRect scrollR = currentTransform.GetComponent<ScrollRect>();
			if (scrollR != null)
			{
				return scrollR;
			}
			if (currentTransform.parent == null)
			{
				return null;
			}

			return RecursiveFindParentWithScrollRect(currentTransform.parent);
		}

		/// <summary>
		/// Adds a tooltip to a UI object (tooltips don't work on non-canvas objects)
		/// </summary>
		public static TooltipDescriptor AddDescriptor(GameObject go, string name, string description)
		{
			TooltipDescriptor desc = go.AddComponent<TooltipDescriptor>();
			desc.TooltipName = name;
			desc.DetailedTooltip = description;
			return desc;
		}

		/// <summary>
		/// Adds a tooltip to a UI object (tooltips don't work on non-canvas objects)
		/// Default color is a slightly transparent white.
		/// </summary>
		public static TooltipDescriptor AddDescriptor(GameObject go, string name, string description, Color color)
		{
			TooltipDescriptor desc = AddDescriptor(go, name, description);
			desc.backgroundColor = color;
			return desc;
		}
	}
}