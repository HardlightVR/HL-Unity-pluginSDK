/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using NullSpace.API.Enums;
using NullSpace.SDK.Haptics;
using NullSpace.SDK.Editor;


namespace NullSpace.SDK.Demos
{
    public class TestHaptics : MonoBehaviour
    {
        Rigidbody myRB;

        private bool massage = false;


        private HapticsLoader haptics;
        private DataModel model;

        void Awake()
        {
        }
        void Start()
        {
            myRB = GameObject.Find("Haptic Trigger").GetComponent<Rigidbody>();

            haptics = NSManager.Instance.HapticLoader;
            haptics.BasePath = System.IO.Path.Combine(Application.streamingAssetsPath, "NS Haptics");

            model = NSManager.Instance.DataModel;
        }


        IEnumerator MoveFromTo(Vector3 pointA, Vector3 pointB, float time)
        {
            while (massage)
            {

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / time; // sweeps from 0 to 1 in time seconds
                    myRB.transform.position = Vector3.Lerp(pointA, pointB, t); // set position proportional to t
                    yield return 0; // leave the routine and return here in the next frame
                }
                t = 0f;

                while (t < 1f)
                {
                    t += Time.deltaTime / time; // sweeps from 0 to 1 in time seconds
                    myRB.transform.position = Vector3.Lerp(pointB, pointA, t); // set position proportional to t
                    yield return 0; // leave the routine and return here in the next frame
                }


            }
        }

        void Update()
        {
            bool moving = false;
            float velVal = 350;

            #region Direction Controls
            if (Input.GetKey(KeyCode.LeftArrow) && myRB.transform.position.x > -8)
            {
                myRB.AddForce(Vector3.left * velVal);
            }
            if (Input.GetKey(KeyCode.RightArrow) && myRB.transform.position.x < 8)
            {
                myRB.AddForce(Vector3.right * velVal);
            }
            if (Input.GetKey(KeyCode.UpArrow) && myRB.transform.position.y < 8)
            {
                myRB.AddForce(Vector3.up * velVal);
            }
            if (Input.GetKey(KeyCode.DownArrow) && myRB.transform.position.y > -8)
            {
                myRB.AddForce(Vector3.down * velVal);
            }

            if (!moving)
            {
                myRB.velocity = Vector3.zero;
            }
            #endregion



            #region Application Quit Code
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            #endregion
        }


        public void OnGUI()
        {

            if (GUI.Button(new Rect(20, 20, 100, 40), "Massage"))
            {
                massage = !massage;
                StartCoroutine(MoveFromTo(new Vector3(0, -3.5f, 0), new Vector3(0, 4.5f, 0), .8f));
            }

            if (GUI.Button(new Rect(20, 80, 120, 40), "Continuous Play"))
            {
                model.Play(new List<HapticEffect>() {
                        new HapticEffect(Effect.Smooth_Hum_50, Location.Chest_Left, 5.0f, 0.0f, 10)
                });
            }
            if (GUI.Button(new Rect(160, 80, 150, 40), "Continuous Play All"))
            {
                List<HapticEffect> effects = new List<HapticEffect>();
                foreach (Location loc in Enum.GetValues(typeof(Location)))
                {
                    if (loc == Location.Error) { continue; }
                    effects.Add(new HapticEffect(Effect.Smooth_Hum_50, loc, 5.0f, 0.0f, 10));
                }
                model.Play(effects);
            }
            if (GUI.Button(new Rect(20, 140, 100, 40), "Halt All"))
            {
                model.ClearAllPlaying();
            }

            if (GUI.Button(new Rect(20, 200, 100, 40), "Jolt Left Body"))
            {
                haptics.PlayPattern("body_jolt", Side.Left);

			}
			if (GUI.Button(new Rect(140, 200, 100, 40), "Jolt Full Body"))
            {
                haptics.PlayPattern("body_jolt", Side.Mirror);
            }
            if (GUI.Button(new Rect(260, 200, 100, 40), "Jolt Right Body"))
            {
                haptics.PlayPattern("body_jolt", Side.Right);
            }

            if (GUI.Button(new Rect(20, 260, 130, 40), "TapTapTap"))
            {
                haptics.PlaySequence("taptaptap", Location.Chest_Right);

            }

        }
    }
}
