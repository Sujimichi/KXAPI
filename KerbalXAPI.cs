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
    //define delegates to be used as callbacks in request methods.
    public delegate void RequestCallback(string data,int status_code);
    public delegate void ImageUrlCheck(string content_type);
    public delegate void ActionCallback();
    public delegate void CraftListCallback(Dictionary<int, Dictionary<string, string>> craft_data);

    //The KerbalXAPI class handles all interaction with KerbalX.com and is responsible for holding the authentication token
    //The class depends on there being an instance of the RequestHandler class present (which handles the actual send/receive process and error handling).
    public class KerbalXAPI
    {
        internal static string token_path = Paths.joined(KSPUtil.ApplicationRootPath, "KerbalX.key");
//        private  static string site_url                 = "https://kerbalx.com";
        internal static string site_url                 = "http://mizu.local:3000";
//        private  static string site_url                 = "http://kerbalx-stage.herokuapp.com";


        private static string token                    = null;
        internal static string kx_username              = null; //not used for any authentication, just for being friendly!

        internal string client                   = "";
        internal string client_version           = "";        
        public   string upgrade_required_message = null;
        public   string server_error_message     = null;
        public   bool   failed_to_connect        = false;
        public   bool   upgrade_required         = false;

        public Dictionary<int, Dictionary<string, string>> user_craft;//container for listing of user's craft already on KX and some details about them.


        public KerbalXAPI(string client_name, string client_version){
            this.client = client_name;
            this.client_version = client_version;
        }



        //takes partial url and returns full url to site; ie url_to("some/place") -> "http://whatever_domain_site_url_defines.com/some/place"
        public string url_to(string path){
            if(!path.StartsWith("/")){
                path = "/" + path;
            }
            return KerbalXAPI.site_url + path;
        }



        //Authentication POST requests

        //make request to site to authenticate username and password and get token back
        public void login(string username, string password, RequestCallback callback){
            KerbalXAPI.log("loging into KerbalX.com...");
            NameValueCollection data = new NameValueCollection() { { "username", username }, { "password", password } };
            RequestHandler.show_401_message = false; //don't show standard 401 error dialog
            HTTP.post(url_to("api/login"), data).send(this, (resp, code) => {
                if(code == 200){
                    var resp_data = JSON.Parse(resp);
                    KerbalXAPI.token = resp_data["token"];
                    KerbalXAPI.save_token(resp_data["token"]);
                    KerbalXAPI.kx_username = resp_data["username"];                    
                }
                callback(resp, code);
            });
        }

        //attempt to login to KerbalX with the users auth token.  If the token doesn't exist, or is no longer valid the response will be a 401
        public void login(RequestCallback callback){
            try{
                if(File.Exists(KerbalXAPI.token_path)){
                    KerbalXAPI.log("Reading token from " + KerbalXAPI.token_path);
                    string current_token = File.ReadAllText(KerbalXAPI.token_path);
                    authenticate_token(current_token, (resp, code) => {
                        if(code == 200){
                            var resp_data = JSON.Parse(resp);
                            KerbalXAPI.kx_username = resp_data["username"];
                            KerbalXAPI.token = current_token;
                        }
                        callback(resp, code);
                    });
                }else{
                    callback("", 401);
                }
            }
            catch{
                callback("", 401);
            }
        }

        //make request to site to authenticate token. If token authentication fails, no error message is shown, it just sets the login window to show u-name/password fields.
        internal void authenticate_token(string current_token, RequestCallback callback){
            KerbalXAPI.log("Authenticating with KerbalX.com...");
            NameValueCollection data = new NameValueCollection() { { "token", current_token } };
            RequestHandler.show_401_message = false; //don't show standard 401 error dialog
            HTTP.post(url_to("api/authenticate"), data).send(this, callback);
        }



        //nukes the authentication token and user variables and sets the login gui to enable login again.
        public void logout(RequestCallback callback){
            token = null; 
            kx_username = null;
            File.Delete(KerbalXAPI.token_path);
            callback("", 200);
            KerbalXAPI.log("logged out");
        }


        public bool logged_in{
            get{
                return token != null;
            }
        }
        public bool logged_out{
            get{
                return token == null;
            }
        }
        public string logged_in_as{
            get{ 
                return kx_username;
            }
        }

        protected static void save_token(string token){
            File.WriteAllText(KerbalXAPI.token_path, token);
        }



        //Settings requests

        //Tells KerbalX not to bug this user about the current minor/patch version update available
        //There is no callback for this request.
        public void dismiss_current_update_notification(){
            HTTP.post(url_to("api/dismiss_update_notification")).set_header("token", token).send(this, (resp, code) => { });
        }
        public void deferred_downloads_enabled(RequestCallback callback){
            HTTP.get(url_to("api/deferred_downloads_enabled")).set_header("token", token).send(this, callback);
        }
        public void enable_deferred_downloads(RequestCallback callback){
            HTTP.post(url_to("api/enable_deferred_downloads")).set_header("token", token).send(this, callback);
        }



        //Craft GET requests

        //Get the craft the user has tagged for download
        public void fetch_download_queue(CraftListCallback callback){
            fetch_craft_list("api/download_queue.json", callback);
        }

        //Get the craft the user has previously downloaded
        public void fetch_past_downloads(CraftListCallback callback){
            fetch_craft_list("api/past_downloads.json", callback);
        }

        //Get the craft the user has favourited
        public void fetch_favoutite_craft(CraftListCallback callback){
            fetch_craft_list("api/favourite_craft.json", callback);
        }

        //Get the craft the user has uploaded (really rather similar to fetch_existing_craft, just slightly different info, will try to unify these two at some point)
        public void fetch_users_craft(CraftListCallback callback){
            fetch_craft_list("api/user_craft.json", callback);
        }

        //Remove a craft from the list of craft the user has tagged for download
        public void remove_from_queue(int craft_id, RequestCallback callback){
            HTTP.get(url_to("api/remove_from_queue/" + craft_id)).set_header("token", token).send(this, callback);
        }

        //Does exactly what is says on the tin, it fetches a craft by ID from KerbalX.
        //Just to note though, the ID must be for a craft that is either in the users download queue, has been downloaded before or is one of the users craft
        public void download_craft(int id, RequestCallback callback){
            HTTP.get(url_to("api/craft/" + id)).set_header("token", token).send(this, callback);
        }


        //handles fetching a list of craft from KerbalX, processes the response for certain craft attributes and
        //assembles a Dictionary which is passed into the callback.
        private void fetch_craft_list(string path, CraftListCallback callback){
            HTTP.get(url_to(path)).set_header("token", token).send(this, (resp, code) =>{
                if(code == 200){
                    callback(process_craft_data(resp, "id", "name", "version", "url", "type", "part_count", "crew_capacity", "cost", "mass", "stages", "created_at", "updated_at", "description" ));
                }
            });
        }

        //Fetches data on the users current craft on the site.  This is kept in a Dictionary of craft_id => Dict of key value pairs....here let me explain it in Ruby;
        //{craft_id => {:id => craft.id, :name => craft.name, :version => craft.ksp_version, :url => craft.unique_url}, ...}
        public void fetch_existing_craft(ActionCallback callback){
            HTTP.get(url_to("api/existing_craft.json")).set_header("token", token).send(this, (resp, code) =>{
                if(code == 200){                    
                    user_craft = process_craft_data(resp, "id", "name", "version", "url", "type", "part_count", "crew_capacity", "cost", "mass", "stages", "created_at", "updated_at", "description" );
                    callback();
                }
            });
        }


        //Takes craft list JSON data from the site and converts it into a nested Dictionary of craft.id => { various craft attrs }
        //the attrs it reads out of the JSON from the site is determined by the strings passed in after the JSON.
        private Dictionary<int, Dictionary<string, string>> process_craft_data(string craft_data_json, params string[] attrs){
            JSONNode craft_data = JSON.Parse(craft_data_json);
            Dictionary<int, Dictionary<string, string>> craft_list = new Dictionary<int, Dictionary<string, string>>();
            for(int i = 0; i < craft_data.Count; i++){
                var c = craft_data[i];
                int id = int.Parse((string)c["id"]);
                Dictionary<string,string> cd = new Dictionary<string,string>();
                foreach(string attr in attrs){
                    try{
                        cd.Add(attr, c[attr]);                            
                    }
                    catch{}
                }
                craft_list.Add(id, cd);
            }
            return craft_list;
        }



        //Craft POST and PUT requests

        //Send new craft to Mun....or KerbalX.com as a POST request
        public void upload_craft(WWWForm craft_data, RequestCallback callback){
            HTTP http = HTTP.post(url_to("api/craft"), craft_data);
            http.set_header("token", token);
            http.set_header("Content-Type", "multipart/form-data");
            http.send(this, callback);
        }

        //Update existing craft on KerbalX as a PUT request with the KerbalX database ID of the craft to be updated
        public void update_craft(int id, WWWForm craft_data, RequestCallback callback){
            HTTP http = HTTP.post(url_to("api/craft/" + id), craft_data);
            http.request.method = "PUT"; //because unity's PUT method doesn't take a form, so we create a POST with the form and then change the verb.
            http.set_header("token", token);
            http.set_header("Content-Type", "multipart/form-data");
            http.send(this, callback);
        }

        public void lookup_parts(WWWForm part_info, RequestCallback callback){
            HTTP http = HTTP.post(url_to("api/lookup_parts"), part_info);
            http.set_header("token", token);
            http.set_header("Content-Type", "multipart/form-data");
            http.send(this, callback);           
        }

        public static void log(string s){
            s = "[KerbalXAPI] " + s;
            Debug.Log(s);
        }

    }









}


