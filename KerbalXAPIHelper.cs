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
        internal static MessageHandler message_handler_instance = null;
        private static bool opening_login_ui = false;

        //On Awake any other instance of this class is destroyed and this instance is set on the static 'instance' variable
        //Automatically opens the login UI if on the main KSP menu and the API is already logged in (to provide a route to logout).
        private void Awake(){            
            if(instance != null){
                GameObject.Destroy(instance);
            }
            instance = this;

            //start the login UI if the API token exists ie API is logged in (to allow the user to logout).
            if(on_main_menu && KerbalXAPI.token != null){
                start_login_ui();
            }
        }

        //Start the Login UI
        internal void start_login_ui(){            
            if(KXAPI.login_ui == null && !opening_login_ui){
                opening_login_ui = true;
                gameObject.AddOrGetComponent<KerbalXLoginUI>();
            }
            opening_login_ui = false;
        }

        //Start the Request Handler
        internal void start_request_handler(){
            if(RequestHandler.instance == null){
                KXAPI.log("starting web request handler");
                RequestHandler request_handler = gameObject.AddOrGetComponent<RequestHandler>();
                RequestHandler.instance = request_handler;
            }
        }
            
        internal bool on_main_menu{
            get{ 
                return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "kspMainMenu";
            }
        }


        internal void show_error_messages_for(KerbalXAPI api){           
            if(message_handler_instance != null){
                GameObject.Destroy(message_handler_instance);
            }
            message_handler_instance = gameObject.AddOrGetComponent<MessageHandler>();
            message_handler_instance.failed_to_connect = api.failed_to_connect;
            message_handler_instance.upgrade_required = api.upgrade_required;
            message_handler_instance.upgrade_required_message = api.upgrade_required_message;
            message_handler_instance.error_message = api.server_error_message;
            api.failed_to_connect = false;
            api.upgrade_required = false;
            api.upgrade_required_message = null;
            api.server_error_message = null;           
        }
    }


}