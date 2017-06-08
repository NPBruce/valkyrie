using UnityEngine;

namespace ValkyrieTools
{
    public class Android
    {
        public static string GetStorage()
        {
            try
            {
                AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
                string path = jc.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<string>("getAbsolutePath");
                if (path != null)
                {
                    ValkyrieDebug.Log(path);
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
