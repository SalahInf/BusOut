using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace FunGames.Tools.Utils
{
    public class WebUtils
    {
        public static void SendRequest(UnityWebRequest webRequest, Action<UnityWebRequest> callback)
        {
            webRequest.SendWebRequest().completed += (req) => { callback?.Invoke(webRequest); };
        }

        public static UnityWebRequest DownloadRequest(string url)
        {
            UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            return webRequest;
        }

        public static void DownloadFile(string url, string path, Action action = null)
        {
            UnityWebRequest downloadRequest = DownloadRequest(url);
            SendRequest(downloadRequest, (s) => FileDownloaded(s, path, action));
        }

        private static void FileDownloaded(UnityWebRequest webRequest, string path, Action action = null)
        {
            Debug.Log("File downloaded: " + path);
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllBytes(path, webRequest.downloadHandler.data);
            action?.Invoke();
        }
    }
}