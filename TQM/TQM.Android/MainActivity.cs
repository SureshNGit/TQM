using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using System;
using System.IO;

namespace TQM.Droid
{
    //[Activity(Label = "TQM", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [Activity(Label = "TQM-YCB", Icon = "@mipmap/SasthaLogoYCB", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjU0NjA4QDMyMzAyZTMxMmUzMEUvREZNRzVVcUh1WTgwVUp2K2Evdk9Fb0h0Q1lxWGp2VVhaMUhYSDhoOUk9");
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            if (!(CheckPermissionGranted(Manifest.Permission.ManageExternalStorage) &&
                CheckPermissionGranted(Manifest.Permission.WriteExternalStorage) &&
                    CheckPermissionGranted(Manifest.Permission.ReadExternalStorage) &&
                    CheckPermissionGranted(Manifest.Permission.AccessFineLocation) &&
                    CheckPermissionGranted(Manifest.Permission.AccessCoarseLocation) &&
                    CheckPermissionGranted(Manifest.Permission.Bluetooth) &&
                    CheckPermissionGranted(Manifest.Permission.BluetoothAdmin) &&
                    CheckPermissionGranted(Manifest.Permission.BluetoothScan) &&
                    CheckPermissionGranted(Manifest.Permission.BluetoothConnect) &&
                    CheckPermissionGranted(Manifest.Permission.BluetoothAdvertise) &&
                    CheckPermissionGranted(Manifest.Permission.Internet) &&
                    CheckPermissionGranted(Manifest.Permission.AccessNetworkState)))
            {
                RequestAllPermission();
            }

            string dbName = "tqm_db.sqlite";
            string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string fullPath = Path.Combine(folderPath, dbName);

            //***********To read database file from other than default folder********
            //string testDbName = "tqm_db_test.sqlite";
            //string dataFiles = Android.App.Application.Context.GetExternalFilesDir("").AbsolutePath;
            //if (File.Exists(Path.Combine(dataFiles, testDbName)))
            //{
            //    fullPath = Path.Combine(dataFiles, testDbName);
            //}
            //***********End********

            //***********To copy database file from default folder to downloads folder********
            //string downloadsFolder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
            //if (File.Exists(fullPath))
            //{
            //    File.Copy(fullPath, downloadsFolder, true);
            //}
            //***********End********

            LoadApplication(new App(fullPath));
        }

        public bool CheckPermissionGranted(string Permissions)
        {
            if (ActivityCompat.CheckSelfPermission(this, Permissions) != Permission.Granted)
            {
                return false;
            }
            else
            {
                return true;
            }


        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        private void RequestAllPermission()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ManageExternalStorage) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessCoarseLocation) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Bluetooth) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.BluetoothAdmin) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.BluetoothScan) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.BluetoothConnect) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.BluetoothAdvertise) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Internet) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessNetworkState))
            {
                ActivityCompat.RequestPermissions(this, new String[] {
                    Manifest.Permission.ManageExternalStorage,
                    Manifest.Permission.WriteExternalStorage,
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.BluetoothAdvertise,
                    Manifest.Permission.Internet,
                    Manifest.Permission.AccessNetworkState}, 100);

            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] {
                    Manifest.Permission.ManageExternalStorage,
                    Manifest.Permission.WriteExternalStorage,
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.BluetoothAdvertise,
                    Manifest.Permission.Internet,
                    Manifest.Permission.AccessNetworkState}, 100);
            }
        }
    }
}