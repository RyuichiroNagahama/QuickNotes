using System.IO;
using UnityEngine;
using Game;
using Game.Modding;
using ModCommon;

namespace QuickNotes {
  [System.Serializable] public class Settings { public bool showOnStart=false; public string text=""; public int fontSize=16; public float winX=300, winY=120; }
  static class P { public static string Dir => Path.Combine(Application.persistentDataPath,"Mods","QuickNotes");
                   public static string Json => Path.Combine(Dir,"settings.json"); }

  public class Mod : IMod {
    GameObject go;
    public void OnLoad(UpdateSystem us){ if(go==null){ go=new GameObject("QuickNotesUI"); Object.DontDestroyOnLoad(go); go.AddComponent<UI>(); } }
    public void OnDispose(){ if(go!=null){ Object.Destroy(go); go=null; } }
  }

  public class UI : MonoBehaviour {
    static UI inst; static bool latchedSet=false, latched=false;
    public static bool Visible{ get=> inst?inst._vis:(latchedSet?latched:false);
                                set{ if(inst) inst._vis=value; else { latched=value; latchedSet=true; } } }

    Settings s = new Settings();
    Rect win = new Rect(300,120,520,320);
    Vector2 scroll; bool _vis; Font jpFont; GUIStyle textStyle, labelStyle; Rect lastTextRect;

    void Awake(){
      inst=this; Directory.CreateDirectory(P.Dir); s=Load();
      _vis = latchedSet? latched : s.showOnStart;
      win.x=s.winX; win.y=s.winY;
    }
    void OnDestroy(){ if(inst==this) inst=null; }

    void Update(){
      if (Input.GetKeyDown(KeyCode.F7)) _vis=!_vis;
      Cursor.visible = true;
      if (_vis){
        Input.imeCompositionMode = IMECompositionMode.On;
        var p = new Vector2(lastTextRect.x + 12f, lastTextRect.y + lastTextRect.height - 18f);
        Input.compositionCursorPos = GUIUtility.GUIToScreenPoint(p);
      } else {
        Input.imeCompositionMode = IMECompositionMode.Auto;
      }
    }

    void EnsureFont(){
      if (jpFont==null || jpFont.fontSize!=s.fontSize){
        try{ jpFont = Font.CreateDynamicFontFromOSFont(new[]{ "Yu Gothic UI","Meiryo","MS Gothic","Yu Gothic" }, s.fontSize); }catch{ jpFont=null; }
      }
      textStyle = new GUIStyle(GUI.skin.textArea){ wordWrap=true, fontSize=s.fontSize, font=jpFont };
      labelStyle = new GUIStyle(GUI.skin.label){ fontSize=s.fontSize, font=jpFont };
    }

    void OnGUI(){
      if(!_vis){ /* hosted-inline */ /* ClickBlocker.SetZone("QuickNotes", Rect.zero, false); */ return; }
// [hosted] ClickBlocker.SetZone("QuickNotes", win, true);

      EnsureFont();
      win = GUI.Window(57001, win, id=>{
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
ModCommon.ImGuiBlockerShim.Mark("QuickNotes", win);
        GUI.SetNextControlName("QN_Text");
        s.text = GUILayout.TextArea(s.text, textStyle, GUILayout.ExpandHeight(true));
        lastTextRect = GUILayoutUtility.GetLastRect();
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Save")) Save(s);
        if(GUILayout.Button("Clear")) s.text="";
        GUILayout.Label("Font", labelStyle, GUILayout.Width(40));
        int newSize = Mathf.RoundToInt(GUILayout.HorizontalSlider(s.fontSize,12,28));
        if (newSize != s.fontSize){ s.fontSize = newSize; EnsureFont(); }
        s.showOnStart = GUILayout.Toggle(s.showOnStart, " Show on start");
        if(GUILayout.Button("Close (F7)")) _vis=false;
        GUILayout.EndHorizontal();

        GUI.DragWindow();
      }, "QuickNotes (F7)");

      s.winX = win.x; s.winY = win.y;
    }

    Settings Load(){ try{ if(File.Exists(P.Json)) return JsonUtility.FromJson<Settings>(File.ReadAllText(P.Json)) ?? new Settings(); }catch{} return new Settings(); }
    void Save(Settings t){ try{ Directory.CreateDirectory(P.Dir); File.WriteAllText(P.Json, JsonUtility.ToJson(t,true)); }catch{} }
  }
}


