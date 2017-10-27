/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace Hardlight.SDK.Demos
{
	[RequireComponent(typeof(SuitRenderers))]
	[RequireComponent(typeof(SuitColorController))]
	[RequireComponent(typeof(HardlightColliderCollection))]
	public class LibraryHapticControls : MonoBehaviour
	{
		public Camera cam;

		private SuitRenderers suitRenderers;
		private SuitColorController colorController;
		private HardlightColliderCollection collection;

		public HapticRecording recording;

		public SuitDemo[] AllDemos;

		/// <summary>
		/// The demo currently used.
		/// We deactivate the old demo and activate the new one if you call SelectSuitDemo
		/// This lets us configure the UI (based on the ActiveObjects/ActiveIfDisabledObjects set in the Inspector)
		/// </summary>
		public SuitDemo CurrentDemo;
		/// <summary>
		/// This is controlled based on the suit and contents within NSEnums.
		/// This number exists for easier testing of experimental hardware.
		/// </summary>
		void Awake()
		{
			suitRenderers = GetComponent<SuitRenderers>();
			colorController = GetComponent<SuitColorController>();
			collection = GetComponent<HardlightColliderCollection>();
			recording = GetComponent<HapticRecording>();
			collection.Init();
			AllDemos = FindObjectsOfType<SuitDemo>();
			colorController.suitRenderers = suitRenderers;
			for (int i = 0; i < AllDemos.Length; i++)
			{
				AllDemos[i].AssignColliderCollection(collection);
				AllDemos[i].AssignColorController(colorController);
				AllDemos[i].AssignHapticRecorder(recording);
			}
		}

		void Start()
		{
			for (int i = 0; i < AllDemos.Length; i++)
			{
				if (AllDemos[i].AutoDeactivate)
				{
					AllDemos[i].DeactivateDemoOverhead();
				}
			}

			//If we have a demo
			if (CurrentDemo != null)
			{
				var temp = CurrentDemo;
				CurrentDemo = null;
				//Turn it on. (To ensure it's needed elements are on)
				SelectSuitDemo(temp);
			}
		}

		void Update()
		{
			GetInput();
		}

		internal void ChangeViewportMode(bool enabling)
		{
			if (enabling)
			{
				CurrentDemo.ActivateDemo();
			}
			else
			{
				CurrentDemo.DeactivateDemo();
			}
		}

		public MeshRenderer GetRenderer(GameObject suitObject)
		{
			return suitRenderers.GetRenderer(suitObject);
		}

		public void GetInput()
		{
			#region Hotkeys for current demo
			if (CurrentDemo != null)
			{
				CurrentDemo.CheckHotkeys();
			}
			#endregion

			#region [1-9] SuitDemos
			for (int i = 0; i < AllDemos.Length; i++)
			{
				if (AllDemos[i] != null)
				{
					if (AllDemos[i].CheckForActivation())
						SelectSuitDemo(AllDemos[i]);
				}
			}
			#endregion

			#region [7] Massage Toggle

			#endregion

			#region [Space] Clear all effects
			if (Input.GetKeyDown(KeyCode.Space))
			{
				ClearAllEffects();
			}
			#endregion

			#region Clicking on SuitBodyCollider
			if (Input.GetMouseButtonDown(0) && cam != null)
			{
				//Where the mouse is 
				Ray ray = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				//Debug.Log("Camera: " + cam.name + "\n", cam.gameObject);
				//Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 3.5f);

				//Raycast to see if we hit
				if (Physics.Raycast(ray, out hit, 100))
				{
					//Get the clicked SuitBodyCollider
					HardlightCollider clicked = hit.collider.gameObject.GetComponent<HardlightCollider>();

					//Assuming there is one
					if (clicked != null)
					{
						//Debug.Log("Clicked: " + clicked.name + "\n", clicked.gameObject);
						//Do whatever our current demo wants to do with that click info.
						CurrentDemo.OnSuitClicked(clicked, hit);
					}
				}
			}
			#endregion

			#region Clicking on SuitBodyCollider
			if (Input.GetMouseButton(0))
			{
				//Where the mouse is 
				Ray ray = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				//Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 3.5f);

				//Raycast to see if we hit
				if (Physics.Raycast(ray, out hit, 100))
				{
					//Get the clicked SuitBodyCollider
					HardlightCollider clicked = hit.collider.gameObject.GetComponent<HardlightCollider>();

					//Assuming there is one
					if (clicked != null)
					{
						//Do whatever our current demo wants to do with that click info.
						CurrentDemo.OnSuitClicking(clicked, hit);
					}
				}
			}
			else
			{
				if (CurrentDemo != null)
				{
					CurrentDemo.OnSuitNoInput();
				}
			}
			#endregion

			#region [Esc] Application Quit Code
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
			#endregion
		}

		/// <summary>
		/// Library Haptic Controls is set up to take SuitDemos, a simple class for controlling different modes of interaction with the scene
		/// Examples: Impulse Emanation, Impulse Traversal, Region Selection, Tracking, Click to Test
		/// Each SuitDemo enables/disables its critical items (which are set via inspector)
		/// </summary>
		/// <param name="demo"></param>
		public void SelectSuitDemo(SuitDemo demo)
		{
			//We don't cause transitions if we're already that demo.
			if (demo != CurrentDemo)
			{
				if (CurrentDemo != null)
				{
					//Debug.Log("Enabling: " + CurrentDemo.GetType().ToString() + "\t\t" + demo.GetType().ToString() + "\n");
					CurrentDemo.DeactivateDemoOverhead();
					CurrentDemo.enabled = false;
					CurrentDemo.DeselectAllSuitColliders();
				}
				if (demo != null)
				{
					CurrentDemo = demo;
					CurrentDemo.enabled = true;
					CurrentDemo.ActivateDemoOverhead();
				}
			}
		}

		//Hotkey: Spacebar
		public void ClearAllEffects()
		{
			//This stops all haptic effects and clears them out.
			HardlightManager.Instance.ClearAllEffects();
		}
		public void ReloadScene()
		{
			//The goal of this function is to reload the plugin so we can support mid-exploring editing of haptics files
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		//Hotkey: Escape
		public void QuitScene()
		{
			Application.Quit();
			//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}
}
