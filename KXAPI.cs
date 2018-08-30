using System;
using System.IO;
using UnityEngine;

//Built Against KSP 1.4.3
//build id = 02152
//2018-04-26_22-43-47

namespace KXAPI
{
    public class KXAPI
    {
        public static string version = "0.1.1";
        internal static KerbalXLoginUI login_ui = null;

        //StyleSheet (initialised on first call to OnGUI)
        internal static GUISkin skin = null;
        internal static GUISkin alt_skin = null;
    }



    internal class Paths
    {
        //takes any number of strings and returns them joined together with Linux specific path divider, ie:
        //Paths.joined("follow", "the", "yellow", "brick", "road") -> "follow/the/yellow/brick/road 
        static public string joined(params string[] paths){
            return String.Join("/", paths).Replace("\\", "/");
        }

        //takes any number of strings and returns them joined together with OS specific path divider, ie:
        //Paths.joined("follow", "the", "yellow", "brick", "road") -> "follow/the/yellow/brick/road or follow\the\yellow\brick\road (I mean, what kinda os uses \ anyway, madness).
        static public string os_joined(params string[] paths){
            return String.Join(Path.DirectorySeparatorChar.ToString(), paths);
        }

        static public string os_safe(string path_string){
            path_string = path_string.Replace('\\', '/');
            return path_string.Replace('/', Path.DirectorySeparatorChar);
        }

    }
}

