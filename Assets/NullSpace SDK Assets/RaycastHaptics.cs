using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using NullSpace.SDK;
using NullSpace.API.Enums;
using NullSpace.SDK.Haptics;

public class RaycastHaptics : MonoBehaviour
{
    Rigidbody myRB;

    /// <summary>
    /// The Haptic JEffect ID that will play. Check the reference sheet included in the NullSpace API.
    /// </summary>
    public Effect selectedHapticEffectID = Effect.Pulsing_Medium_100;

    /// <summary>
    /// This is controlled based on the suit and contents within NSEnums.
    /// This number exists for easier testing of experimental hardware.
    /// </summary>
    public int numRegions = 14;
    public float CodeHapticDuration = 5.5f;

    int[] playingIDs;

	Sequence five_second_hum;

    //List<List<HapticFrame>> frames;
    void Start()
    {
        playingIDs = new int[numRegions];
        for (int i = 0; i < playingIDs.Length; i++)
        {
            playingIDs[i] = -1;
        }

		five_second_hum = new Sequence("ns.demos.five_second_hum");


    }


    public void OnGUI()
    {
        /*if (GUI.Button(new Rect(10, 10, 100, 40), "Halt All"))
		{
			NSManager.Instance.SuitHardwareInterface.HaltAllEffects();
		}

		//TODO: Need halt all for data model. Like clear all.
		*/

    }

    public IEnumerator ChangeColorDelayed(GameObject g, Color c, float timeout)
    {
        yield return new WaitForSeconds(timeout);
        g.GetComponent<MeshRenderer>().material.color = c;

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 3.5f);

            if (Physics.Raycast(ray, out hit, 100))
            {

                if (hit.collider.gameObject.tag == "Haptic Region")
                {
                    HapticCollider haptic = hit.collider.gameObject.GetComponent<HapticCollider>();

                    if (haptic != null)
                    {
                        Debug.LogFormat("Starting Haptic: Region ID {0}\n", haptic.regionID);
						five_second_hum.CreateHandle(haptic.regionID).Play();
                      
               
                        hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                        //TODO: this is temporary
                        StartCoroutine(ChangeColorDelayed(
                            hit.collider.gameObject, 
                            new Color(227/255f, 227/255f, 227/255f,1f), 
                            5.0f));
                    }
                }
            }

        }

      
		
		
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		
	}
}
