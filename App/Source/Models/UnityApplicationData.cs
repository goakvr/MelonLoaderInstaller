﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using AssetsTools.NET.Extra;
using MelonLoaderInstaller.App.Utilities;
using Stream = System.IO.Stream;
using UnityVersion = AssetRipper.VersionUtilities.UnityVersion;

namespace MelonLoaderInstaller.App.Models
{
    public class UnityApplicationData
    {
        public Drawable Icon { get; private set; }
        public string AppName { get; private set; }
        public string PackageName { get; private set; }
        public string ApkLocation { get; private set; }
        public string LibraryLocation { get; private set; }
        public bool IsPatched { get; private set; }
        public bool IsSupported { get; private set; }
        public bool IsSplit { get; private set; }
        public UnityVersion EngineVersion { get; private set; }

        private ApplicationInfo _applicationInfo;
        private AssetManager _assetManager;

        public UnityApplicationData(PackageManager pm, ApplicationInfo info)
        {
            Icon = info.LoadIcon(pm);
            AppName = info.PackageName;
            PackageName = info.PackageName;
            ApkLocation = info.PublicSourceDir;
            LibraryLocation = info.SplitSourceDirs?.FirstOrDefault(d => d.Contains("arm64"));

            _applicationInfo = info;
            _assetManager = pm.GetResourcesForApplication(info)?.Assets;

            if (_assetManager == null)
                throw new Exception("AssetManager is null for package " + PackageName);

            IsSplit = info.SplitSourceDirs != null;
            IsSupported = info.NativeLibraryDir?.Contains("arm64") ?? false;
            IsPatched = _assetManager.List("melonloader") != null;
            
            TryGetVersion();
        }

        private void TryGetVersion()
        {
            AssetsManager uAssetsManager = new AssetsManager();

            // Try to read directly from file
            try
            {
                Stream stream = _assetManager.Open("bin/Data/globalgamemanagers");

                using MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                stream.Dispose();

                AssetsFileInstance instance = uAssetsManager.LoadAssetsFile(memoryStream, "/bin/Data/globalgamemanagers", true);
                EngineVersion = UnityVersion.Parse(instance.file.Metadata.UnityVersion);
                return;
            }
            catch (Java.IO.FileNotFoundException _) { }

            // If failed before, try to get the data from data.unity3d
            try
            {
                Stream stream = _assetManager.Open("bin/Data/data.unity3d", Access.Buffer);

                using MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                stream.Dispose();

                BundleFileInstance bundle = uAssetsManager.LoadBundleFile(memoryStream, "/bin/Data/data.unity3d");
                AssetsFileInstance instance = uAssetsManager.LoadAssetsFileFromBundle(bundle, "globalgamemanagers");
                EngineVersion = UnityVersion.Parse(instance.file.Metadata.UnityVersion);

                memoryStream.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to get Unity version for package " + PackageName + "\n" + ex.ToString());
            }

            EngineVersion = UnityVersion.MinVersion;
        }
    }
}