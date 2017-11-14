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
    }
}
