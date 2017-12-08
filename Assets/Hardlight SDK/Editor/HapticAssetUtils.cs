using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Hardlight.SDK.FileUtilities
{
	public class HapticAssetEditorUtils : MonoBehaviour
	{
		public static void SaveSequence(string name, HapticSequence sequence)
		{
			if (sequence != null)
			{
				name = HapticResources.CleanName(name);
				CodeHapticFactory.EnsureSequenceIsRemembered(name, sequence);
				if (CodeHapticFactory.SequenceExists(name))
				{
					var seqData = CodeHapticFactory.GetRememberedSequence(name);
					if (!seqData.saved)
					{
						string assetPath = GetDefaultSavePath();
						string finalizedPath = TryToFindAvailableFileName(assetPath + name, GetAssetExtension());

						//Debug.Log("Seq  " + name + " - Finalized Path: " + finalizedPath + "\n");

						if (!AssetDatabase.Contains(sequence))
						{
							seqData.saved = true;
							//Debug.Log("We are saving sequence: " + sequence.name + "\nAttempting: [" + finalizedPath + "]  " + Exists("", finalizedPath) + "\n");
							AssetDatabase.CreateAsset(sequence, finalizedPath);
							//Debug.Log("Asset was created - " + File.Exists(finalizedPath) + " does the lower case return true: " + File.Exists(finalizedPath.ToLower()) + "\n");
						}
						else
						{
							//Debug.LogError("Not saving: " + name + " sequence because it already exists?");
						}
						AssetDatabase.SetLabels(sequence, new string[] { "Haptics", "Sequence" });
						Selection.activeObject = sequence;
					}
					//else
					//	Debug.Log("Not saving: " + name + " sequence because we think it has been saved already");
				}
				//else
				//	Debug.LogError("Not saving: " + name + " sequence because it isn't in our sequence memory dictionary");
			}
			else
				Debug.LogError("Attempted to save a null sequence asset [" + name + "]\n");
		}
		public static void SavePattern(string name, HapticPattern pattern)
		{
			if (pattern != null)
			{
				//Debug.Log(name + "  " + pattern.name);
				name = CleanName(name);
				CodeHapticFactory.EnsurePatternIsRemembered(name, pattern);
				//Debug.Log("Looking for: " + CleanName(pattern.name) + "\n");
				if (CodeHapticFactory.PatternExists(name))
				{
					var patData = CodeHapticFactory.GetRememberedPattern(name);
					if (!patData.saved)
					{
						#region Save Required Elements
						for (int i = 0; i < pattern.Sequences.Count; i++)
						{
							if (pattern.Sequences[i] != null)
							{
								string key = pattern.Sequences[i].Sequence.name;
								SaveSequence(key, pattern.Sequences[i].Sequence);
							}
							else
							{
								Debug.Log(name + " has null sequences at " + i + "\n");
							}
						}
						#endregion

						#region Save Self
						string assetPath = GetDefaultSavePath();
						string finalizedPath = TryToFindAvailableFileName(assetPath + name, GetAssetExtension());

						//Debug.Log("Pat  " + name + " - " + finalizedPath + "\n");

						if (!AssetDatabase.Contains(pattern))
						{
							patData.saved = true;
							AssetDatabase.CreateAsset(pattern, finalizedPath);
							//Debug.Log("Asset was created - " + finalizedPath + "  " + File.Exists(finalizedPath) + " does the lower case return true: " + File.Exists(finalizedPath.ToLower()) + "\n");
						}
						AssetDatabase.SetLabels(pattern, new string[] { "Haptics", "Pattern" });
						Selection.activeObject = pattern;
						#endregion
					}
				}
			}
			else
				Debug.LogError("Attempted to save a null pattern asset [" + name + "]\n");
		}
		public static void SaveExperience(string name, HapticExperience experience)
		{
			if (experience != null)
			{
				#region Save Required Elements
				for (int i = 0; i < experience.Patterns.Count; i++)
				{
					string key = experience.Patterns[i].Pattern.name;
					SavePattern(key, experience.Patterns[i].Pattern);
				}
				#endregion

				#region Save Self
				string assetPath = GetDefaultSavePath();
				name = CleanName(name);

				string finalizedPath = TryToFindAvailableFileName(assetPath + name, GetAssetExtension());

				if (!AssetDatabase.Contains(experience))
				{
					AssetDatabase.CreateAsset(experience, finalizedPath);
				}
				AssetDatabase.SetLabels(experience, new string[] { "Haptics", "Experience" });
				Selection.activeObject = experience;
				#endregion
			}
			else
				Debug.LogError("Attempted to save a null experience asset [" + name + "]\n");
		}

		private static bool Exists(string enginePath, string desiredPathAndName)
		{
			bool normalExists = File.Exists((enginePath + desiredPathAndName));
			//bool lowerExists = File.Exists((enginePath + desiredPathAndName).ToLower());
			//Debug.Log("looking at " + desiredPathAndName + "\tNormalExists: " + normalExists + "\n\tLowerExists:" + lowerExists + "\n");
			return normalExists;
		}
		private static string GetEnginePath()
		{
			string enginePath = Application.dataPath;
			//this removes the Path/[Assets] from the Application.dataPath;
			enginePath = Application.dataPath.Remove(enginePath.Length - 6, 6);
			return enginePath;
		}
		private static string TryToFindAvailableFileName(string desiredPathAndName, string extension = ".asset")
		{
			string enginePath = GetEnginePath();
			bool exists = Exists(enginePath, desiredPathAndName + extension);

			desiredPathAndName = RemoveExtension(desiredPathAndName, extension);
			//Debug.Log("Desired Path: " + desiredPathAndName + " " + exists + "\n" + enginePath + "\n" + enginePath + desiredPathAndName);

			bool succeeded = true;
			int extraChecks = 2;
			if (exists)
			{
				succeeded = false;
				for (int i = 0; i < extraChecks; i++)
				{
					exists = File.Exists(enginePath + desiredPathAndName + " " + i + extension);
					//Debug.Log("Checking for: " + enginePath + desiredPathAndName + " " + i + extension + "\n" + exists);
					if (!exists)
					{
						desiredPathAndName = desiredPathAndName + " " + i;
						succeeded = true;
						break;
					}
				}
			}

			if (!succeeded)
			{
				throw new HapticsAssetException("Could not create asset for sequence [" + desiredPathAndName + "]\nToo many similarly named files (checked " + extraChecks + " files)...\n\tSuggestion: clean up some excessive names and try again.");
			}

			//Debug.Log("We have found a valid name: " + desiredPathAndName + extension + "\n");

			return desiredPathAndName + extension;
		}

		public static string CleanName(string name)
		{
			return StripNamespace(RemoveExtension(name));
		}
		private static string RemoveExtension(string filePath, string extension = ".asset")
		{
			string[] ext = new string[1];
			ext[0] = extension;
			if (filePath.Contains(extension))
			{
				return filePath.Split(ext, StringSplitOptions.RemoveEmptyEntries)[0];
			}
			return filePath;
		}
		private static string StripNamespace(string key)
		{
			//string output = "Dealing with key [" + key + "]\n";
			if (key.Contains("."))
			{
				var split = key.Split('.');
				//Debug.Log("split count: " + split.Length + "\n");
				//output += "Returning [" + split[split.Length - 1] + "]";
				//Debug.Log(output + "\n");
				return split[split.Length - 1];
			}
			return key;
		}
		private static string GetDefaultSavePath()
		{
			return "Assets/Resources/Haptics/";
		}
		private static string GetAssetExtension()
		{
			return ".asset";
		}
	}
}