/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NullSpace.SDK;
using NullSpace.API.Enums;
using NullSpace.SDK.Haptics;

namespace NullSpace.SDK.Demos
{
    public class RaycastHaptics : MonoBehaviour
    {
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
                            NSManager.Instance.DataModel.Play(new List<HapticEffect> {
                            new HapticEffect(Effect.Smooth_Hum_50,
                            haptic.regionID,
                            2.0f, 0.0f, 10)
                        });

                            hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                            //TODO: this is temporary
                            StartCoroutine(ChangeColorDelayed(
                                hit.collider.gameObject,
                                new Color(227 / 255f, 227 / 255f, 227 / 255f, 1f),
                                2.0f));
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
}