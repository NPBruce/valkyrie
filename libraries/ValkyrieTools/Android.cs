using UnityEngine;

using System.IO;
using System.Threading;

namespace ValkyrieTools
{
    public class Android
    {
        public static string GetStorage()
        {
            try
            {
                // we import in a thread, we have to attach JNI, otherwise we would crash
                int ret = AndroidJNI.AttachCurrentThread();
                AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
                string path = jc.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<string>("getAbsolutePath");
                if (ret != 0)
                    AndroidJNI.DetachCurrentThread();
                if (path != null)
                {
                    return path;
                }
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log(e.ToString());
            }
            return "";
        }

        public static bool CopyOfficialAppData(string packageName)
        {
             
            try
            {

                if (!Directory.Exists(GetStorage() + "/Valkyrie"))
                {
                    Directory.CreateDirectory(GetStorage() + "/Valkyrie");
                }

                if (Directory.Exists(GetStorage() + "/Valkyrie/" + packageName))
                {
                    Directory.Delete(GetStorage() + "/Valkyrie/" + packageName, true);
                }

                Directory.CreateDirectory(GetStorage() + "/Valkyrie/" + packageName);
                
                // we import in a thread, we have to attach JNI, otherwise we would crash
                string andriodDataDir = "data";
                int ret = AndroidJNI.AttachCurrentThread();
                var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                var appContext = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaClass jc = new AndroidJavaClass("com.android.accessmomdata.AccessActivity");

                if(packageName.Equals("com.fantasyflightgames.rtl"))
                {
                    andriodDataDir = "obb";
                }

                jc.CallStatic("makeActivity", activity, appContext, packageName, andriodDataDir);
                if (ret != 0)
                    AndroidJNI.DetachCurrentThread();

                //Block until android MoM data copy completed.
                string doneIndicatorFilePath = GetStorage() + "/Valkyrie/" + packageName + "/done";
                string failedIndicatorFilePath = GetStorage() + "/Valkyrie/" + packageName + "/failed";
                while (!File.Exists(doneIndicatorFilePath) && !File.Exists(failedIndicatorFilePath))
                {
                    Thread.Sleep(1000);
                }
                if(File.Exists(failedIndicatorFilePath))
                {
                    ValkyrieDebug.Log("Failed copy with indicator");
                    return false;
                }

                if (File.Exists(doneIndicatorFilePath))
                {
                    ValkyrieDebug.Log("Done copy with indicator");
                }
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log(e.ToString());
                ValkyrieDebug.Log("Exception in copy");
                return false;
            }
            return true;
        }

        public static int GetSDKLevel()
        {
            var clazz = AndroidJNI.FindClass("android/os/Build$VERSION");
            var fieldID = AndroidJNI.GetStaticFieldID(clazz, "SDK_INT", "I");
            var sdkLevel = AndroidJNI.GetStaticIntField(clazz, fieldID);
            return sdkLevel;
        }

        public static string GetAndroidAPKPath(string packageName)
        {
            int ret = AndroidJNI.AttachCurrentThread();
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

            string apkPath = activity.Call<AndroidJavaObject>("getApplicationContext").Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo", packageName, 0).Get<AndroidJavaObject>("applicationInfo").Get<string>("sourceDir");
            if (ret != 0)
                AndroidJNI.DetachCurrentThread();
            if (apkPath != null)
            {
                return apkPath;
            }

            return "";
        }

        public static string GetAndroidPackageVersion(string packageName)
        {
            int ret = AndroidJNI.AttachCurrentThread();
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

            string version = activity.Call<AndroidJavaObject>("getApplicationContext").Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo", packageName, 0).Get<string>("versionName");
            if (ret != 0)
                AndroidJNI.DetachCurrentThread();
            if (version != null)
            {
                return version;
            }

            return "";
        }


    }


}
