using System;
using System.IO;
using UnityEngine;
using KatLib;
//Built Against KSP 1.4.3
//build id = 02152
//2018-04-26_22-43-47

namespace KXAPI
{
    public class KXAPI
    {
//        internal static string site_url          = "https://kerbalx.com";
//        internal static string site_url          = "http://kerbalx-stage.herokuapp.com";
        internal static string site_url          = "http://mizu.local:3000";
        public static string version = "0.1.3";

        internal static KerbalXLoginUI login_ui = null; //Reference to Login UI
        internal static GUISkin skin = null;            //StyleSheet (initialised on first call to OnGUI)
        internal static GUISkin alt_skin = null;

        internal static void log(string s){            
            Debug.Log("[KerbalXAPI] " + s);
        }
    }


    internal class KXAPIUI : DryUI
    {
        protected override void OnGUI(){
            //Trigger the creation of custom Skin (copy of default skin with various custom styles added to it, see stylesheet.cs)
            if(KXAPI.skin == null){
                KXAPI.skin = new StyleSheet(HighLogic.Skin).skin;
                KXAPI.alt_skin = new StyleSheet(GUI.skin).skin; //works but isn't as clear.
            }
            if(this.skin == null){
                this.skin = KXAPI.skin;
            }
            GUI.skin = skin;
            base.OnGUI();
            GUI.skin = null;
        }
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

    internal class Checksum
    {
        static internal string from_file(string path){
            using(var md5 = System.Security.Cryptography.MD5.Create()){
                using(var stream = File.OpenRead(path)){
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-","").ToLowerInvariant();
                }
            }
        }
    }
}

