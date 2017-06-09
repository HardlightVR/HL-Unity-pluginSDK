/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//Effect tooltip?

namespace NullSpace.SDK.Demos
{
	abstract public class SuitDemo : MonoBehaviour
	{
		//Turn on my needed things
		abstract public void ActivateDemo();

		//Turn off my needed things
		abstract public void DeactivateDemo();

		abstract public void OnSuitClicked(HardlightCollider suit, RaycastHit hit);
		abstract public void OnSuitClicking(HardlightCollider suit, RaycastHit hit);
		abstract public void OnSuitNoInput();

		public Button MyEnableButton;
		public Button MyDisableButton;

		public Sprite EnabledSprite;
		public Sprite DisabledSprite;

		public KeyCode ActivateHotkey = KeyCode.None;

		public List<HardlightCollider> suitObjects;

		public List<GameObject> ActiveObjects;
		public List<GameObject> ActiveIfDisabledObjects;

		protected Color buttonSelected = new Color(150 / 255f, 150 / 255f, 150 / 255f, 1f);
		//protected Color buttonSelected = new Color(30 / 255f, 167 / 255f, 210 / 255f, 1f);
		protected Color buttonUnselected = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1f);

		/// <summary>
		/// An overhead method that calls ActivateDemo.
		/// Enforce abstract 'need implementation' while still calling certain things without overriding.
		/// </summary>
		public void ActivateDemoOverhead()
		{
			SetEnableButtonBackgroundColor(buttonSelected);
			ActivateDemo();
			HandleRequiredObjects(true);
		}

		/// <summary>
		/// An overhead method that calls DeactivateDemo.
		/// Enforce abstract 'need implementation' while still calling certain things without overriding.
		/// </summary>
		public void DeactivateDemoOverhead()
		{
			SetEnableButtonBackgroundColor(buttonUnselected);
			DeactivateDemo();
			HandleRequiredObjects(false);
		}

		public virtual void SetupButtons()
		{
			if (MyEnableButton != null)
			{
				MyEnableButton.transform.FindChild("Icon").GetComponent<Image>().sprite = EnabledSprite;
			}
			if (MyDisableButton != null)
			{
				MyDisableButton.transform.FindChild("Icon").GetComponent<Image>().sprite = DisabledSprite;
			}
		}

		public virtual void Start()
		{
			suitObjects = new List<HardlightCollider>();
			suitObjects = FindObjectsOfType<HardlightCollider>().ToList();
			SetupButtons();
			SetEnableButtonBackgroundColor(buttonUnselected);
			DeactivateDemo();
			enabled = false;
		}

		public void HandleRequiredObjects(bool Activating)
		{
			for (int i = 0; i < ActiveObjects.Count; i++)
			{
				if (ActiveObjects[i])
				{
					//Debug.Log("Setting Active : " + ActiveObjects[i].name + "\n to " + Activating);
					ActiveObjects[i].SetActive(Activating);
				}
			}
			for (int i = 0; i < ActiveIfDisabledObjects.Count; i++)
			{
				if (ActiveIfDisabledObjects[i])
				{
					//Debug.Log("Setting Anti-Active : " + ActiveIfDisabledObjects[i].name + "\n to " + Activating);
					ActiveIfDisabledObjects[i].SetActive(!Activating);
				}
			}
		}

		public virtual void SetEnableButtonBackgroundColor(Color col)
		{
			if (MyEnableButton != null)
			{
				MyEnableButton.GetComponent<Image>().color = col;
			}
		}

		public IEnumerator ChangeColorDelayed(GameObject targetObject, Color setColor, float delay, float minDuration = .15f)
		{
			yield return new WaitForSeconds(Mathf.Clamp(delay, minDuration, float.MaxValue));
			ColorSuitCollider(targetObject, setColor);
		}

		public void ColorSuitCollider(GameObject suitCollider, Color setColor)
		{
			MeshRenderer rend = suitCollider.GetComponent<MeshRenderer>();
			if (rend != null)
			{
				rend.material.color = setColor;
			}
		}
		public Color SuitColliderCurrentColor(GameObject suitCollider)
		{
			MeshRenderer rend = suitCollider.GetComponent<MeshRenderer>();
			if (rend != null)
			{
				return rend.material.color;
			}
			return Color.white;
		}
		public void ColorSuitCollider(HardlightCollider suitCollider, Color setColor)
		{
			ColorSuitCollider(suitCollider.gameObject, setColor);
		}

		/// <summary>
		/// Colors a particular suit visual to the labeled color.
		/// Performs a null check on suit first.
		/// </summary>
		/// <param name="suit"></param>
		/// <param name="col"></param>
		protected void ColorSuit(HardlightCollider suit, Color col)
		{
			//This is just sanitization and to make the code more robust.
			if (suit != null)
			{
				//We could easily be more efficient than getting the MeshRenderer each time (like having SuitBodyCollider hold onto a ref to it's MeshRenderer)
				//However this isn't a VR application, so ease of programming/readability is the priority here.
				suit.GetComponent<MeshRenderer>().material.color = col;
			}
		}

		public IEnumerator ColorPadForXDuration(HardlightCollider suit, Color targetColor, Color revertColor, float MinDuration = 0.0f)
		{
			//I don't think we need to save this local reference. Just in case.
			HardlightCollider current = suit;

			//You could do a fancy color lerp functionality here...
			ColorSuit(current, targetColor);

			//var duration = Mathf.Clamp(MinDuration, .1f, 100.0f);
			//I clamp this to a min of .1 for user visibility.
			yield return new WaitForSeconds(MinDuration);
			ColorSuit(current, revertColor);
		}
	}

	public class SuitClickDemo : SuitDemo
	{
		//Turn on my needed things
		public override void ActivateDemo()
		{
			HandleRequiredObjects(true);

			//I need nothing
		}

		//Turn off my needed things
		public override void DeactivateDemo()
		{
			HandleRequiredObjects(false);

			//I need nothing
		}

		public override void OnSuitClicked(HardlightCollider clicked, RaycastHit hit)
		{
			Debug.Log("Clicked on " + clicked.name + " with a regionID value of: " + (int)clicked.regionID + "\n");
		}

		public override void OnSuitClicking(HardlightCollider clicked, RaycastHit hit)
		{
			Debug.Log("Clicked on " + clicked.name + " with a regionID value of: " + (int)clicked.regionID + "\n");
		}

		public override void OnSuitNoInput()
		{
		}
	}
}