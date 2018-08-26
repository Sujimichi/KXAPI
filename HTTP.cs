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
    //The HTTP class is basically a wrapper around UnityWebRequest and enables chaining calls ie:
    //HTTP.get("http://some_url.com").send((resp, code) =>{ } );
    //OR
    //HTTP.get("http://some_url.com").set_header("header key", "header value").send((resp, code) =>{ } );
    //OR for POST requests; 
    //HTTP.get("http://some_url.com", form_data).set_header("header key", "header value").send((resp, code) =>{ } );
    //form data can either be a WWWForm or a NameValueCollection
    //When calling send it can take a lambda as show above, or a RequestCallback delegate. Into which will be passed the response body string and the status code
    //send hands off to the RequestHandler to handle the actual send/receive process as a Coroutine and deal with error codes
    //The only slightly special method is verify_image which takes an ImageUrlCheck delegate instead of the RequestCallback.
    internal class HTTP
    {
        internal UnityWebRequest request;


        internal static HTTP get(string url){
            HTTP http = new HTTP();
            http.request = UnityWebRequest.Get(url);
            return http;
        }

        //used for making a post request without any form data. Contructs a GET request (as UnityWebRequest's POST doesn't enable creating a POST without
        //form data and then change the method to POST.
        internal static HTTP post(string url){
            HTTP http = new HTTP();
            http.request = UnityWebRequest.Get(url);
            http.request.method = "POST";
            return http;
        }

        internal static HTTP post(string url, NameValueCollection data){
            WWWForm form_data = new WWWForm();
            foreach(string key in data){
                form_data.AddField(key, data[key]);
            }
            HTTP http = new HTTP();
            http.request = UnityWebRequest.Post(url, form_data);
            return http;
        }

        internal static HTTP post(string url, WWWForm form_data){
            HTTP http = new HTTP();
            http.request = UnityWebRequest.Post(url, form_data);
            return http;
        }

        //This differs from the other HTTP static methods in that is doesn't return anything and only fetches the HEADER info from the url
        //It also uses a different method in the RequestHandler which doesn't deal with status codes and only returns the Content-Type into the callback.
        //This is the one route which will make calls to other sites, but only to urls entered by the user for images
        internal static void verify_image(string url, ImageUrlCheck callback){
            HTTP http = new HTTP();
            http.request = UnityWebRequest.Get(url);
            http.request.method = "HEAD";
            http.send(callback);
        }


        internal HTTP set_header(string key, string value){
            if(key == "token" && String.IsNullOrEmpty(value)){
                throw new Exception("[KerbalXAPI] Unable to make request - User not logged in");
            }
            request.SetRequestHeader(key, value);
            return this;
        }

        internal void send(KerbalXAPI api, RequestCallback callback){
            if(String.IsNullOrEmpty(api.client_version) || String.IsNullOrEmpty(api.client)){
                KerbalXAPI.log("client info has not been set");
                return;
            }
            set_header("MODCLIENT", api.client);
            set_header("MODCLIENTVERSION", api.client_version);
            set_header("KSPVERSION", Versioning.GetVersionString());
            if(RequestHandler.instance == null){
                throw new Exception("[KerbalXAPI] RequestHandler is not ready, unable to make request");
            } else{
                RequestHandler.instance.send_request(api, request, callback);
            }
        }

        //override for send when using ImageUrlCheck callback
        internal void send(ImageUrlCheck callback){
            RequestHandler.instance.send_request(request, callback);
        }
    }

}

