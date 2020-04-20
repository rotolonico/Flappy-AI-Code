using System;
using System.Collections;
using System.IO;
using AI.NEAT;
using NN;
using UnityEngine;
using UnityEngine.Networking;

namespace NNUtils
{
    public class NetworkStorage : MonoBehaviour
    {
        public static NetworkStorage Instance;

        private void Awake() => Instance = this;

        public static void SaveNetwork(NeuralNetwork save, string path)
        {
            var json = StringSerializationAPI.Serialize(typeof(NetworkSave), save);

            var file = new FileInfo($"{Application.persistentDataPath}/networkSaves/{path}");
            file.Directory?.Create();
            File.WriteAllText(file.FullName, json);
        }

        public static NeuralNetwork LoadNetwork(string path)
        {
            var json = !File.Exists(path)
                ? ""
                : File.ReadAllText(path);

            return json == "" ? null : StringSerializationAPI.Deserialize(typeof(NeuralNetwork), json) as NeuralNetwork;
        }

        public void DownloadNetwork(string url, Action<NeuralNetwork> callback, Action<string> fallback) =>
            StartCoroutine(DownloadNetworkCoroutine(url, callback, fallback));

        private static IEnumerator DownloadNetworkCoroutine(string url, Action<NeuralNetwork> callback,
            Action<string> fallback)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                    fallback(request.error);
                else callback(StringSerializationAPI.Deserialize(typeof(NeuralNetwork), request.downloadHandler.text) as NeuralNetwork);
            }
        }
        
        public static Genome LoadGenome(string path)
        {
            var json = !File.Exists(path)
                ? ""
                : File.ReadAllText(path);

            return json == "" ? null : StringSerializationAPI.Deserialize(typeof(Genome), json) as Genome;
        }

        public void DownloadGenome(string url, Action<Genome> callback, Action<string> fallback) =>
            StartCoroutine(DownloadGenomeCoroutine(url, callback, fallback));

        private static IEnumerator DownloadGenomeCoroutine(string url, Action<Genome> callback,
            Action<string> fallback)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                    fallback(request.error);
                else callback(StringSerializationAPI.Deserialize(typeof(Genome), request.downloadHandler.text) as Genome);
            }
        }
    }
}