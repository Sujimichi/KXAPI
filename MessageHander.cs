using System;
using System.Collections.Generic;
using UnityEngine;
using KatLib;

namespace KXAPI
{
    internal class MessageHandler : KXAPIUI
    {

        internal bool failed_to_connect = false;
        internal bool upgrade_required = false;
        internal string upgrade_required_message = null;
        internal string error_message = null;
        internal KerbalXAPI api_instance;


        private void Start(){
            window_title = null;
            window_pos = new Rect(0,0,0,0);
        }

        protected override void WindowContent(int win_id) {                              
            if(failed_to_connect){
                error_dialog(() =>{
                    label("Unable to Connect to KerbalX.com!", "alert.h1");
                    label("Check your net connection and that you can reach KerbalX in a browser", "alert.h2");
                });

            } else if(upgrade_required){                
                error_dialog(() =>{
                    label("Upgrade Required", "h2");
                    label(upgrade_required_message);
                    section(() =>{                        
                        section("dialog.section", () =>{
                            button("Goto KerbalX.com/KXAPI/" + api_instance.client + " for more info", "hyperlink.left", () =>{
                                Application.OpenURL(KerbalXAPI.site_url_to("/KXAPI/" + api_instance.client));
                            });
                        });
                    });
                }, "Upgrade Required");
                on_error();
            
            } else if(error_message != null){                
                List<string> messages = new List<string>();
                foreach(string s in error_message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)){
                    messages.Add(s);
                }
                string title = messages[0];
                messages[0] = "";
                error_dialog(() =>{
                    label(title, "alert.h2");
                    foreach(string message in messages){
                        if(message != ""){
                            label(message);
                        }
                    }
                }, "KerbalX.com Error");
                on_error();
            }

            GameObject.Destroy(this);
        }

        protected void error_dialog(ContentNoArgs content, string title = "KerbalX API Error"){
            ModalDialog dialog = show_modal_dialog(d =>{
                content();
                section(() =>{
                    fspace();
                    button("Close", 60f, close_dialog);
                });
            });
            dialog.dialog_pos.width = 600;
            dialog.dialog_pos.x = Screen.width / 2 - (dialog.dialog_pos.width / 2);
            dialog.dialog_pos.y = Screen.height * 0.3f;
            dialog.window_title = title;
        }

    }
}

