using System.Collections;
using System.Text;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Networking;

public sealed class GHunterBackendClient : MonoBehaviour
{
    public static GHunterBackendClient Instance { get; private set; }
    [SerializeField] string backendUrl="http://127.0.0.1:5077";
    void Awake(){Instance=this;}
    public void TestHealth()=>StartCoroutine(Send("/api/health","GET",null,false,null));
    public void LoadMySecureData()=>WithToken(t=>StartCoroutine(Send("/api/player/me","GET",null,true,t)));
    public void SelectCharacter(int id)=>WithToken(t=>StartCoroutine(Send("/api/player/character","POST",$"{{\"characterId\":{id}}}",true,t)));
    void WithToken(System.Action<string> ok){var u=FirebaseAuth.DefaultInstance.CurrentUser;if(u==null){Debug.LogError("[Backend] Chưa có Firebase user");return;}u.TokenAsync(false).ContinueWithOnMainThread(x=>{if(x.IsFaulted||x.IsCanceled)Debug.LogError(x.Exception);else ok(x.Result);});}
    IEnumerator Send(string path,string method,string json,bool auth,string token)
    {
        var url=backendUrl.TrimEnd('/')+path; UnityWebRequest r;
        if(method=="GET")r=UnityWebRequest.Get(url);else{r=new UnityWebRequest(url,method);r.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(json??"{}"));r.downloadHandler=new DownloadHandlerBuffer();r.SetRequestHeader("Content-Type","application/json");}
        if(r.downloadHandler==null)r.downloadHandler=new DownloadHandlerBuffer();if(auth)r.SetRequestHeader("Authorization","Bearer "+token);yield return r.SendWebRequest();if(r.result==UnityWebRequest.Result.Success)Debug.Log("[Backend] OK "+path+"
"+r.downloadHandler.text);else Debug.LogError("[Backend] ERROR "+r.responseCode+" "+path+"
"+r.downloadHandler.text);r.Dispose();
    }
}
