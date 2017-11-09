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
using System;

//Effect tooltip?

namespace Hardlight.SDK.Demos
{
	abstract public class SuitDemo : MonoBehaviour
	{
		public HapticRecording recording;
		public SuitColorController colorController;
		//Turn on my needed things
		abstract public void ActivateDemo();

		//Turn off my needed things
		abstract public void DeactivateDemo();

		abstract public void OnSuitClicked(HardlightCollider suit, RaycastHit hit);
		abstract public void OnSuitClicking(HardlightCollider suit, RaycastHit hit);
		abstract public void OnSuitNoInput();

		public Light MyLight;
		public Light MainLight;
		private float lightIntensity;
		public Color lightColor;

		public bool AutoDeactivate = true;
		public Button MyEnableButton;
		public Button MyDisableButton;

		public Sprite EnabledSprite;
		public Sprite DisabledSprite;

		public KeyCode ActivateHotkey = KeyCode.None;

		private HardlightColliderCollection ColliderCollection;
		public List<HardlightCollider> SuitObjects
		{
			get
			{
				if (ColliderCollection != null)
				{
					return ColliderCollection.SuitObjects;
				}
				throw new System.Exception("Collider Collection is null.\n\tCheck to ensure there is one in the scene, ideally on [Demo Controls and Selector] object.");
			}
		}

		public List<GameObject> ActiveObjects;
		public List<GameObject> ActiveIfDisabledObjects;

		protected Color buttonSelected = new Color(150 / 255f, 150 / 255f, 150 / 255f, 1f);
		protected Color buttonUnselected = new Color(255 / 255f, 255 / 255f, 255 / 255f, 1f);
		protected Color unselectedColor
		{
			get
			{
				if (colorController)
				{
					return colorController.unselectedColor;
				}
				return Color.magenta;
			}
		}


		/// <summary>
		/// An overhead method that calls ActivateDemo.
		/// Enforce abstract 'need implementation' while still calling certain things without overriding.
		/// </summary>
		public void ActivateDemoOverhead()
		{
			SetEnableButtonBackgroundColor(buttonSelected);
			ActivateDemo();
			HandleRequiredObjects(true);
			StartCoroutine(LightTransition(MainLight, lightColor));
			MyLight.gameObject.SetActive(false);
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

		public void AssignColliderCollection(HardlightColliderCollection coll)
		{
			ColliderCollection = coll;
		}
		public void AssignColorController(SuitColorController colorCont)
		{
			colorController = colorCont;
		}
		public void AssignHapticRecorder(HapticRecording newRecording)
		{
			recording = newRecording;
		}
		public virtual void Start()
		{
			lightIntensity = MyLight.intensity;
			SetupButtons();
			SetEnableButtonBackgroundColor(buttonUnselected);
			if (AutoDeactivate)
			{
				DeactivateDemo();
				enabled = false;
			}
		}

		public virtual bool CheckForActivation()
		{
			return Input.GetKeyDown(ActivateHotkey);
		}
		public virtual void CheckHotkeys()
		{

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

		public void ChangeColorDelayed(GameObject targetObject, Color setColor, float delay, float minDuration = .15f)
		{
			//yield return new WaitForSeconds(Mathf.Clamp(delay, minDuration, float.MaxValue));
			//ColorSuitCollider(targetObject, setColor);
		}

		public Color GetObjectCurrentColor(GameObject suitCollider)
		{
			if (suitCollider != null)
			{
				return colorController.GetObjectCurrentColor(suitCollider);
			}
			else
			{
				Debug.Log("Suit Demo attempted to color a null object\n\t" + name, this);
			}
			return Color.magenta;
		}
		public void ColorSuitObject(HardlightCollider suitCollider, Color setColor)
		{
			colorController.ColorSuitObject(suitCollider, setColor);
		}
		public void ColorSuitObject(GameObject suitCollider, Color setColor)
		{
			if (suitCollider != null)
			{
				var hardlightCol = suitCollider.GetComponent<HardlightCollider>();

				if (hardlightCol != null)
				{
					ColorSuitObject(hardlightCol, setColor);
				}
				else
				{
					Debug.Log("Suit Demo attempted to color an object without a hardlight collider\n\t" + name, this);
				}
			}
			else
			{
				Debug.Log("Suit Demo attempted to color a null object\n\t" + name, this);
			}
		}
		public void ColorSuitObject(int index, Color setColor)
		{
			var providedIndex = index;
			index = Mathf.Clamp(index, 0, SuitObjects.Count - 1);
			if (index != providedIndex)
			{
				Debug.LogError("Attempted to color a suit object by index for the provided index of [" + providedIndex + "] when only [" + SuitObjects.Count + "] valid indices exist\n", this);
			}
			if (SuitObjects.Count > index)
			{
				ColorSuitObject(SuitObjects[index], setColor);
			}
		}

		public void ColorSuitObject(GameObject suitCollider, Color setColor, float lerpDuration, float sustainDuration)
		{
			colorController.ColorSuitObject(suitCollider, setColor, lerpDuration, sustainDuration);
		}
		public void ColorSuitObject(HardlightCollider suitCollider, Color setColor, float lerpDuration, float sustainDuration)
		{
			if (suitCollider != null)
			{
				ColorSuitObject(suitCollider.gameObject, setColor, lerpDuration, sustainDuration);
			}
			else
			{
				Debug.Log("Suit Demo attempted to color a null object\n\t" + name, this);
			}
		}
		public void ColorSuitObject(int index, Color setColor, float lerpDuration, float sustainDuration)
		{
			ColorSuitObject(SuitObjects[index].gameObject, setColor, lerpDuration, sustainDuration);
		}

		public void ColorSuitObjectOverTime(GameObject suitCollider, Color setColor, float lerpDuration, float sustainDuration)
		{
			colorController.ColorSuitObject(suitCollider, setColor, lerpDuration, sustainDuration);
		}
		public void ColorSuitObjectOverTime(HardlightCollider suitCollider, Color setColor, float lerpDuration, float sustainDuration)
		{
			colorController.ColorSuitObject(suitCollider.gameObject, setColor, lerpDuration, sustainDuration);
		}

		public virtual void DeselectAllSuitColliders()
		{
			UncolorAllSuitColliders();
		}

		protected void UncolorAllSuitColliders()
		{
			colorController.UncolorAllSuitColliders();
		}
		IEnumerator LightTransition(Light light, Color newColor)
		{
			float step = .15f;
			var wait = new WaitForSeconds(.01f);
			var oldColor = light.color;
			var oldIntensity = light.intensity;
			for (float i = 0; i < step; i += .01f)
			{
				light.color = Color.Lerp(oldColor, newColor, i / step);
				light.intensity = Mathf.Lerp(oldIntensity, lightIntensity, i / step);
				yield return wait;
			}
			light.gameObject.SetActive(true);
		}

		public virtual void DisplayHandleHaptic(HardlightCollider hit, HapticHandle handle)
		{
			ColorSuitObject(hit, buttonSelected, .05f, .15f);
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