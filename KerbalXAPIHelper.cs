using System;
using UnityEngine;

namespace KXAPI
{
    //The KerbalXAPIHelper is a lightweight class that is created in every scene.  It provides an instance that inherits MonoBehaviour
    //so can be used to start other classes that inherit MonoBehaviour.
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    internal class KerbalXAPIHelper : MonoBehaviour
    {
        internal static KerbalXAPIHelper instance = null;

        //On Awake any other instance of this class is destroyed and this instance is set on the static 'instance' variable
        //Automatically opens the login UI if on the main KSP menu and the API is already logged in (to provide a route to logout).
        private void Awake(){            
            if(instance != null){
                GameObject.Destroy(instance);
            }
            instance = this;

            //start the login UI if the API token exists ie API is logged in (to allow the user to logout).
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if(scene == "kspMainMenu" && KerbalXAPI.token != null){
                start_login_ui();
            }
        }

        //Start the Login UI
        internal void start_login_ui(){
            KXAPI.login_ui = gameObject.AddOrGetComponent<KerbalXLoginUI>();
        }

        //Start the Request Handler
        internal void start_request_handler(){
            if(RequestHandler.instance == null){
                KerbalXAPI.log("starting web request handler");
                RequestHandler request_handler = gameObject.AddOrGetComponent<RequestHandler>();
                RequestHandler.instance = request_handler;
            }
        }
    }
}

