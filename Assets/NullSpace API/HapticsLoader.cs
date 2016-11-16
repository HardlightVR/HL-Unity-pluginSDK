/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://nullspacevr.com/?wpdmpro=nullspace-developer-agreement
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/
using System;
using System.Collections.Generic;
using NullSpace.SDK.Editor.Parser;
using NullSpace.API.Enums;
using NullSpace.API.Logger;

namespace NullSpace.SDK.Editor
{
    /// <summary>
    /// This system is responsible for kicking off the loading routines for haptic files,
    /// as well as providing an easy-to-use interface to play haptic files.
    /// </summary>
    public class HapticsLoader 
    {
        private DependencyResolver resolver;
        private HapticFileParser parser;
        private DataModel model;

        /// <summary>
        /// The base path that the loader will search for haptic files. For example, if BasePath is
        /// set to System.IO.Path.Combine(Application.streamingAssetsPath, "Your Haptic Folder",
        /// the loader will be able to find files in the directory StreamingAssets/Your Haptic Folder/. The loader does 
        /// not search recursively. 
        /// </summary>
        public string BasePath
        {
            get { return this.parser.BasePath;  }
            set { this.parser.BasePath = value; }
        }

        /// <summary>
        /// The model used to control execution of haptic effects
        /// </summary>
        public DataModel DataModel
        {
            get { return this.model; }
            set { this.model = value; }
        }

        /// <summary>
        /// Construct a new HapticsLoader
        /// </summary>
        public HapticsLoader()
        {
            this.parser = new HapticFileParser();
           
            this.resolver = new DependencyResolver(this.parser);
        }

        /// <summary>
        /// Loads a haptic file, blocking. For example, if called within Start(), will block until file is completely loaded into 
        /// memory.
        /// </summary>
        /// <param name="fileName">Name of the file, n</param>
        public void LoadBlocking(string fileName)
        {
            try
            {
                this.resolver.Load(fileName);
            } catch (System.IO.FileNotFoundException)
            {
                Log.Warning("Tried to find file {0} at base path {1} and failed.", fileName, BasePath);
            }

        }

        /// <summary>
        /// Loads a haptic file, nonblocking. You may observe the status of the asynchronous load by testing the IAsyncResult
        /// passed back by this method.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="del">Optional callback to handle load completion</param>
        /// <returns></returns>
        public IAsyncResult LoadAsync(string fileName, AsyncCallback del = null)
        {
            AsyncLoad caller = new AsyncLoad(resolver.LoadAsync);
            var fileInfo = new KeyValuePair<string, HapticFileType>(fileName, HapticFileParser.DetermineFileType(fileName));
            return caller.BeginInvoke(fileInfo, del, null);
        }


        private delegate void AsyncLoad(KeyValuePair<string, HapticFileType> fileInfo);

        /// <summary>
        /// Play a pattern on a certain side
        /// </summary>
        /// <param name="name">Name of the pattern</param>
        /// <param name="side">Which side to play pattern on</param>
        public void PlayPattern(string name, Side side = Side.Mirror)
        {
            AsyncCallback playPattern = delegate (IAsyncResult r)
            {
                var patRef = this.resolver.ResolvePattern(name, side);
                model.Play(resolver.GetData(patRef));
            };

            playHapticAsync(name, playPattern, HapticFileType.Pattern);
        }

    

        /// <summary>
        /// Play an experience on a certain side
        /// </summary>
        /// <param name="name">Name of the experience</param>
        /// <param name="side">Which side to play the experience on</param>
        public void PlayExperience(string name, Side side = Side.Mirror)
        {
            AsyncCallback playExperience = delegate (IAsyncResult r)
            {
                var expRef = this.resolver.ResolveExperience(name, side);
                model.Play(resolver.GetData(expRef));
            };


            playHapticAsync(name, playExperience, HapticFileType.Experience);
        }

        /// <summary>
        /// Play a sequence at a certain location
        /// </summary>
        /// <param name="name">Name of the sequence</param>
        /// <param name="loc">Which location to play the sequence at</param>
        public void PlaySequence(string name, Location loc)
        {
            AsyncCallback playSequence = delegate (IAsyncResult r)
            {
                var seqRef = this.resolver.ResolveSequence(name, loc);
                
                model.Play(resolver.GetData(seqRef));
            };

            playHapticAsync(name, playSequence, HapticFileType.Sequence);
        }

        /// <summary>
        /// Check if a haptic file is loaded, and if so, play it. If not, lazy load and then play it.
        /// </summary>
        /// <param name="name">Name of file</param>
        /// <param name="playHapticCallback">The callback which will actually play the haptics</param>
        /// <param name="ftype">Type of haptics to be played</param>

        private void playHapticAsync(string name, AsyncCallback playHapticCallback, HapticFileType ftype)
        {
            AsyncCallback checkLoaded = delegate (IAsyncResult r)
            {
                if (this.resolver.Loaded(name, ftype))
                {
                    playHapticCallback(null);
                }
            };

            if (this.resolver.Loaded(name, ftype))
            {
                playHapticCallback(null);
            }
            else
            {
                AsyncLoad caller = new AsyncLoad(resolver.LoadAsync);
                caller.BeginInvoke(new KeyValuePair<string, HapticFileType>(name, ftype), checkLoaded, null);
            }
        }
        
        
    }
}
