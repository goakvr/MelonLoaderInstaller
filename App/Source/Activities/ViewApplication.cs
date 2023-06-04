﻿using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Kotlin;
using MelonLoaderInstaller.App.Models;
using MelonLoaderInstaller.App.Utilities;
using MelonLoaderInstaller.Core;
using System;
using static Android.Resource;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace MelonLoaderInstaller.App.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.MelonLoaderInstaller.NoActionBar", MainLauncher = false)]
    public class ViewApplication : AppCompatActivity, View.IOnClickListener
    {
        private UnityApplicationData _applicationData;
        private PatchLogger _patchLogger;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_view_application);

            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.toolbar1));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            string packageName = Intent.GetStringExtra("target.packageName");

            try
            {
                _applicationData = UnityApplicationFinder.FromPackageName(this, packageName);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to find target package\n" + ex.ToString());
                Finish();
                return;
            }

            ImageView appIcon = FindViewById<ImageView>(Resource.Id.applicationIcon);
            TextView appName = FindViewById<TextView>(Resource.Id.applicationName);
            Button patchButton = FindViewById<Button>(Resource.Id.patchButton);

            patchButton.SetOnClickListener(this);
            patchButton.Text = _applicationData.IsPatched ? "REPATCH" : "PATCH";

            appIcon.SetImageDrawable(_applicationData.Icon);
            appName.Text = _applicationData.AppName;

            _patchLogger = new PatchLogger(this);

            CheckWarnings(packageName);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.patch_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.home:
                    Finish();
                    return true;
                case Resource.Id.action_patch_local_deps:
                    // TODO: patch with local deps
                    // requires patching to work obviously
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void CheckWarnings(string packageName)
        {
            if (PackageWarnings.AvailableWarnings.TryGetValue(packageName, out string warning))
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder
                        .SetTitle("Warning")
                        .SetMessage(warning)
                        .SetIcon(Drawable.IcDialogAlert);

                AlertDialog alert = builder.Create();
                alert.SetCancelable(false);
                alert.Show();

                new WarningCountdown(3500, 1000, builder, alert).Start();
            }
        }

        public void OnClick(View v)
        {
        private class PatchLogger : IPatchLogger
        {
            private Activity _context;
            private TextView _content;
            private ScrollView _scroller;
            private bool _dirty = false;

            public PatchLogger(Activity context)
            {
                _context = context;
                _content = context.FindViewById<TextView>(Resource.Id.loggerBody);
                _scroller = context.FindViewById<ScrollView>(Resource.Id.loggerScroll);
                _content.Text = string.Empty;
            }

            public void Clear()
            {
                _context.RunOnUiThread(() =>
                {
                    _content.Text = string.Empty;
                });
            }

            public void Log(string message)
            {
                Logger.Instance.Info(message);

                _context.RunOnUiThread(() =>
                {
                    if (_dirty)
                        _content.Append("\n");
                    else
                        _dirty = true;

                    _content.Append(message);
                    _scroller.FullScroll(FocusSearchDirection.Down);
                });
            }
        }
    }
}