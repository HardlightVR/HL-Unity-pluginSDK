/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using NullSpace.API.Logger;
namespace NullSpace.SDK.Editor
{
    public enum HapticFileType
    {
        Pattern,
        Sequence, 
        Experience,
        Unrecognized
    }
    public static class HapticResourceLoader
    {
        public static KeyValuePair<HapticFileType, string> Load(string path)
        {
            string[] pp = path.Split('.');
            string newPath = Application.streamingAssetsPath + "/"+ path;
            try
            {
                string text = System.IO.File.ReadAllText(newPath);
                switch (pp[1])
                {
                    case "sequence":
                        return new KeyValuePair<HapticFileType, string>(HapticFileType.Sequence, text);
                    case "experience":
                        return new KeyValuePair<HapticFileType, string>(HapticFileType.Experience, text);
                    case "pattern":
                        return new KeyValuePair<HapticFileType, string>(HapticFileType.Pattern, text);
                    default:
                        return new KeyValuePair<HapticFileType, string>(HapticFileType.Unrecognized, null);
                }
            } catch { throw;  }

        }
    }
}
