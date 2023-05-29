﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MelonLoaderInstaller.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MelonLoaderInstaller.App.Utilities
{
    public static class UnityApplicationFinder
    {
        public static List<UnityApplicationData> Find(Activity context)
        {
            PackageManager pm = context.PackageManager;
            if (pm == null)
                throw new Exception("PackageManager is null, how does this happen?");

            IList<ApplicationInfo> allPackages = pm.GetInstalledApplications(PackageInfoFlags.MetaData);

            List<UnityApplicationData> unityApps = new List<UnityApplicationData>();

            foreach (ApplicationInfo package in allPackages)
            {
                if (package.NativeLibraryDir == null)
                    continue;

                try
                {
                    bool isUnity = Directory.GetFiles(package.NativeLibraryDir).Any(f => f.Contains("libunity.so"));
                    if (!isUnity)
                        continue;
                }
                catch (IOException _)
                {
                    continue;
                }

                unityApps.Add(new UnityApplicationData(pm, package));
            }

            return unityApps;
        }
    }
}