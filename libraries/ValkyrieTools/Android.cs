using UnityEngine;

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
                ValkyrieDebug.Log("Android.GetStorage called");
                AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
                string path = jc.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<string>("getAbsolutePath");
                if (ret != 0)
                    AndroidJNI.DetachCurrentThread();
                if (path != null)
                {
                    ValkyrieDebug.Log("Android.GetStorage: " + path);
                    return path;
                }
            }
            catch (System.Exception e)
            {
                ValkyrieDebug.Log(e.ToString());
            }
            return "";
        }
    }
}
