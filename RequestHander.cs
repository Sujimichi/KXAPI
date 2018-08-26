using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;
using UnityEngine.Networking;

using SimpleJSON;

namespace KXAPI
{

    //The RequestHandler is used to handel sending and receiving requests.  It does this inside a Coroutine so delays in response 
    //don't lag the interface.  As such it has to inherit MonoBehaviour and needs to be an instance (can't be used as static).
    //It provides a send_request method which take a UnityWebRequest object and a Callback.  In most cases the callback is a RequestCallback
    //except for the special case using a ImageUrlCheck callback.  
    //When using the RequestCallback (as all interaction with KerbalX does) the RequestHandler will perform different actions based on 
    //the status code returned by the request.
    internal class RequestHandler : MonoBehaviour
    {
        internal static RequestHandler instance = null;
        private static NameValueCollection status_codes = new NameValueCollection(){ 
            { "200", "OK" }, { "401", "Unauthorized" }, { "404", "Not Found" }, { "500", "Server Error!" } 
        };


        internal static bool show_401_message = true;

        private UnityWebRequest last_request = null;
        private RequestCallback last_callback = null;

        internal void try_again(){        
//            send_request(last_request, last_callback);
        }

        internal bool can_retry(){
            return last_request != null;
        }


        //Used to fetch Content-Type Header info for urls entered by user for an image (to check if image is an image)
        internal void send_request(UnityWebRequest request, ImageUrlCheck callback){
            StartCoroutine(transmit(request, callback));
        }
        //Used in request to url entered by user for image, returns just the content type header info
        private IEnumerator transmit(UnityWebRequest request, ImageUrlCheck callback){
            KerbalXAPI.log("sending request to: " + request.url);
            yield return request.Send();
            callback(request.GetResponseHeaders()["Content-Type"]);
        }


        //Used in all requests to KerbalX
        internal void send_request(KerbalXAPI api, UnityWebRequest request, RequestCallback callback){
            StartCoroutine(transmit(api, request, callback));
        }

        //Used in all interacton with KerbalX, called from a Coroutine and handles the response error codes from the site
        private IEnumerator transmit(KerbalXAPI api, UnityWebRequest request, RequestCallback callback){

            last_request = null;
            last_callback = null;

            api.server_error_message = null;
            api.failed_to_connect = false;
            api.upgrade_required = false;

            KerbalXAPI.log("sending request to: " + request.url);
            yield return request.Send();


            if(request.isNetworkError){                                                            //Request Failed, most likely due to being unable to get a response, therefore no status code
                api.failed_to_connect = true;
                KerbalXAPI.log("request failed: " + request.error);

                last_request = new UnityWebRequest(request.url, request.method);                    //  \ create a copy of the request which is about to be sent
                if(request.method != "GET"){                                                        //  | if the request fails because of inability to connect to site
                    last_request.uploadHandler = new UploadHandlerRaw(request.uploadHandler.data);  // <  then try_again() can be used to fire the copied request
                }                                                                                   //  | and the user can carry on from where they were when connection was lost.
                last_request.downloadHandler = request.downloadHandler;                             //  | upload and download handlers have to be duplicated too
                last_callback = callback;                                                           // /  and the callback is also stuffed into a var for reuse.

            } else{
                int status_code = (int)request.responseCode;                                //server responded - get status code
                KerbalXAPI.log("request returned " + status_code + " " + status_codes[status_code.ToString()]);                         

                if(status_code == 500){                                                     //KerbalX server error
                    string error_message = "KerbalX server error!!\n" +                     //default error message incase server doesn't come back with something more helpful
                        "An error has occurred on KerbalX (it was probably Jebs fault)";
                    var resp_data = JSON.Parse(request.downloadHandler.text);               //read response message and assuming there is one change the error_message
                    if(!(resp_data["error"] == null || resp_data["error"] == "")){
                        error_message = "KerbalX server error!!\n" + resp_data["error"];
                    }
                    KerbalXAPI.log(error_message);
                    api.server_error_message = error_message;                           //Set the error_message on KerbalX, any open window will pick this up and render error dialog
                    callback(request.downloadHandler.text, status_code);                    //Still call the callback, assumption is all callbacks will test status code for 200 before proceeding, this allows for further handling if needed

                } else if(status_code == 426){                                              //426 - Upgrade Required, only for a major version change that makes past versions incompatible with the site's API
                    api.upgrade_required = true;
                    var resp_data = JSON.Parse(request.downloadHandler.text);    
                    api.upgrade_required_message = resp_data["upgrade_message"];

                } else if(status_code == 401){                                              //401s (Unauthorized) - response to the user's token not being recognized.
                    if(RequestHandler.show_401_message == true){                            //In the case of login/authenticate, the 401 message is not shown (handled by login dialog)
                        api.server_error_message = "Authorization Failed\nKerbalX did not recognize your authorization token, perhaps you were logged out.";
                        api.logout((resp, code)=>{});
                    } else{
                        callback(request.downloadHandler.text, status_code);
                    }

                } else if(status_code == 200 || status_code == 400 || status_code == 422){  //Error codes returned for OK and failed validations which are handled by the requesting method
                    callback(request.downloadHandler.text, status_code);                    

                } else{                                                                     //Unhandled error codes - All other error codes. 
                    api.server_error_message = "Unknown Error!!\n" + request.downloadHandler.text;
                    callback(request.downloadHandler.text, status_code);
                }
                request.Dispose();
                RequestHandler.show_401_message = true;
            }
        }
    }

}

