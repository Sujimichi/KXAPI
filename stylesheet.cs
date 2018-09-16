using System;
using System.Collections.Generic;
using UnityEngine;
using KatLib;

namespace KXAPI
{
    //StyleSheet defines a set of GUIStyle and assigns them as custom styles to a new skin which is Instantiated from the current base_skin
    //StyleSheet.prepare will be called from inside OnGUI on the base class KerbalXWindow but only on the first call to OnGUI.
    //That will Instantiate the new skin and set it to a static var on KerbalXWindow (KXskin), once it's set further calls to StyleSheet.prepare won't do anything
    //Essentially this is a one time process that sets up all the GUIStyles needed and makes them available as named styles on the base_skin (OnGUI in KerbalXWindow
    //will set base_skin to the KXskin and unset it at the end so as to not effect other windows
    //....it's like we need a sorta sheet of styles, maybe one that can cascade, a cascading style sheet if you will....

    internal delegate void StyleConfig(GUIStyle style);

    internal class StyleSheet : MonoBehaviour
    {

        internal static Dictionary<string, Texture> assets = new Dictionary<string, Texture>() { 
            { "logo_small",             GameDatabase.Instance.GetTexture(Paths.joined("KXAPI", "Assets", "KXlogo_small"), false) },     //166x30 
            { "logo_large",             GameDatabase.Instance.GetTexture(Paths.joined("KXAPI", "Assets", "KXlogo"), false) },           //664x120 
        };

        internal GUISkin skin;
        internal Dictionary<string, GUIStyle> custom_styles = new Dictionary<string, GUIStyle>();
        internal Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        internal void define_style(string name, GUIStyle inherits_from, StyleConfig config){
            GUIStyle style = new GUIStyle(inherits_from);
            style.name = name;
            custom_styles.Add(name, style);
            config(style);
        }

        internal void define_style(string name, string inherits_from_name, StyleConfig config){
            GUIStyle style = new GUIStyle(custom_styles[inherits_from_name]);
            style.name = name;
            custom_styles.Add(name, style);
            config(style);
        }

        internal void set_texture(string name, Color colour){
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, colour);
            tex.Apply();
            textures.Add(name, tex);
        }
        internal void set_texture(string name, Color colour, TextureWrapMode wrap_mode){
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, colour);
            tex.wrapMode = wrap_mode;
            tex.Apply();
            textures.Add(name, tex);
        }

        internal Texture2D make_texture(int width, int height, Color col){
            Color[] pix = new Color[width * height];
            for( int i = 0; i < pix.Length; ++i ){
                pix[ i ] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        internal StyleSheet(GUISkin base_skin){

            set_texture("blue_background", new Color(0.4f, 0.5f, 0.9f, 1), TextureWrapMode.Repeat);           
            set_texture("light_blue_background", new Color(0.37f, 0.41f, 0.62f, 0.4f));           
            set_texture("lighter_blue_background", new Color(0.4f, 0.5f, 0.9f, 0.6f));           
            set_texture("navy_background", new Color(0.26f, 0.34f, 0.45f, 0.8f));
            set_texture("light_navy_background", new Color(0.26f, 0.34f, 0.45f, 0.2f));



            set_texture("dark_background", new Color(0.12f, 0.12f, 0.12f, 0.5f));
            set_texture("pic_highlight", new Color(0.4f, 0.5f, 0.9f, 1));
            set_texture("green_background", new Color(0.2f, 0.6f, 0.2f, 1));
            set_texture("light_green_background", new Color(0.3f, 0.5f, 0.3f, 1));
            set_texture("red_background", new Color(0.51f, 0.44f, 0.44f, 0.4f));
            set_texture("clear_background", new Color(0f, 0f, 0f, 0f));
            set_texture("grey_background", Color.gray);


            set_texture("logging_in", Color.yellow);
            set_texture("logged_out", Color.red);
            set_texture("logged_in", Color.green);



            define_style("h1", base_skin.label, s =>{
                s.fontStyle = FontStyle.Bold;
                s.fontSize = 30;                
            });
            define_style("h2", "h1", s =>{
                s.fontSize = 20;
            });
            define_style("h3", "h1", s =>{
                s.fontSize = 15;
            });
            define_style("h1.centered", "h1", s =>{
                s.alignment = TextAnchor.LowerCenter;
            });
            define_style("upload_header", "h1", s =>{
                s.margin = new RectOffset(0,0,0,0);
                s.padding = new RectOffset(10,0,10,0);
                s.fontSize = 60;
            });
            define_style("upload_header.logo", base_skin.label, s =>{
                s.margin = new RectOffset(0,0,0,0);
                s.padding = new RectOffset(0,0,0,0);
            });

            define_style("h2.centered", "h2", s =>{
                s.alignment = TextAnchor.LowerCenter;
            });
            define_style("h2.tight", "h2", s =>{
                s.margin.bottom = 0; 
            });
            define_style("bold", base_skin.label, s =>{
                s.fontStyle = FontStyle.Bold;
            });
            define_style("small", base_skin.label, s =>{
                s.fontSize = 12;
            });
            define_style("centered", base_skin.label, s =>{
                s.alignment = TextAnchor.LowerCenter;
            });

            define_style("compact", base_skin.label, s =>{
                s.margin.top = 0;
                s.margin.bottom = 2;
            });

            define_style("line", "compact", s =>{                
                s.margin.bottom = 0;
            });
            define_style("bold.compact", "compact", s =>{
                s.fontStyle = FontStyle.Bold;
            });
            define_style("small.compact", "compact", s =>{
                s.fontSize = 12;
            });

            define_style("error", base_skin.label, s =>{
                s.normal.textColor = Color.red;
            });
            define_style("error.bold", "error", s =>{
                s.fontStyle = FontStyle.Bold;
            });
            define_style("alert", base_skin.label, s =>{
                s.normal.textColor = new Color(0.8f,0.3f,0.2f,1);
            });
            define_style("alert.h3", "alert", s =>{               
                s.fontSize = 15;
            });
            define_style("alert.h2", "alert", s =>{               
                s.fontSize = 20;
                s.fontStyle = FontStyle.Bold;
            });
            define_style("alert.h1", "alert.h2", s =>{               
                s.fontSize = 30;
            });

            define_style("modal.title", base_skin.label, s =>{
                s.fontStyle = FontStyle.Bold;
                s.fontSize = 18;
                s.alignment = TextAnchor.MiddleCenter;
                s.padding.top = 10;

            });


            define_style("hyperlink", base_skin.button, s =>{
                s.normal.background = base_skin.label.normal.background;
                s.hover.background = make_texture(2,2, Color.clear);
                s.active.background = make_texture(2,2, Color.clear);
                s.focused.background = make_texture(2,2, Color.clear);


                s.fontStyle = FontStyle.Normal;
                s.normal.textColor = new Color(0.4f, 0.5f, 0.9f, 1); //roughly KerbalX Blue - #6E91EB
                s.hover.textColor = Color.blue;
            });
            define_style("hyperlink.bold", "hyperlink", s =>{
                s.fontStyle = FontStyle.Bold;
            });
            define_style("hyperlink.left", "hyperlink", s =>{
                s.alignment = TextAnchor.UpperLeft;
            });
            define_style("hyperlink.inline", "hyperlink", s =>{
                s.alignment = TextAnchor.UpperLeft;
                s.stretchWidth = false;
                s.padding = base_skin.label.padding;
                s.margin = base_skin.label.margin;                    
            });
            define_style("hyperlink.bold.compact", "hyperlink.bold", s =>{
                s.margin = new RectOffset(0,0,0,0);
            });
            define_style("hyperlink.update_url", "hyperlink.bold", s =>{
                s.fontSize = 20;
                s.wordWrap = true;
                s.margin = new RectOffset(0,0,0,0);
            });



            define_style("button.login", base_skin.button, s =>{
                s.fontSize = 15;
                s.fontStyle = FontStyle.Bold;
                s.padding = new RectOffset(0, 0, 3, 3);
            });
            define_style("button.login.toggle", base_skin.button, s =>{
                s.fixedWidth = 20f;
                s.fixedHeight = 100f;
                s.margin = new RectOffset(0,0,0,0);
            });

            define_style("login.container", base_skin.window, s =>{
                s.margin = new RectOffset(0,0,0,0);
                s.padding = new RectOffset(0,0,0,0);
                //                s.normal.background = textures["grey_background"];
            });
            define_style("login.window", base_skin.box, s =>{
                s.margin = new RectOffset(0,0,0,0);
                s.padding = new RectOffset(0,0,0,0);
                s.border = new RectOffset(0,0,0,0);
                s.normal.background = textures["clear_background"];
            });
            define_style("h1.login", "h1", s =>{
//                s.normal.textColor = new Color(0.4f, 0.5f, 0.9f, 1); //roughly KerbalX Blue - #6E91EB
                s.normal.textColor = new Color(0.48f, 0.48f, 0.48f, 1); 
                s.fontSize = 32;
                s.margin = new RectOffset(0,0,0,0);
                s.padding = new RectOffset(0,0,0,0);
                s.padding.top = 8;
                s.margin.right = 2;
                    
            });


            define_style("login.logging_in", base_skin.box, s =>{
                s.normal.background = textures["logging_in"];
                s.margin = new RectOffset(4,5,0,8);
                s.fixedWidth = 10f;
                s.fixedHeight = 10f;
            });
            define_style("login.logged_in", "login.logging_in", s =>{
                s.normal.background = textures["logged_in"];
            });
            define_style("login.logged_out", "login.logging_in", s =>{
                s.normal.background = textures["logged_out"];
            });

            define_style("dialog.section", base_skin.label, s =>{
                s.normal.background = textures["dark_background"];
                s.margin = new RectOffset(0,0,0,8);
            });



            //set the custom styles onto the base_skin;
            skin = Instantiate(base_skin);
            GUIStyle[] temp = new GUIStyle[custom_styles.Count];
            custom_styles.Values.CopyTo(temp, 0);                
            skin.customStyles = temp;
            skin.window.padding.bottom = 2;
        }
    }
}

