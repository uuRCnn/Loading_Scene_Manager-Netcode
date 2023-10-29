using System;
using System.Collections;
using _Utility;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Managers
{
    public enum SceneName : byte
    {
        Bootstrap, // not used
        Menu,
        CharacterSelection, // todo: Karakter seçme ekranında ise Hero belirli özelliklerini kullanmicak. onuda bu Enum ile saglarsın
        GamePlay,
    }

    public class LoadingSceneManager : NetworkBehaviour
    {
        public SceneName ActiveScene => _activeScene;
        private SceneName _activeScene;

        public static LoadingSceneManager Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void Init()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }

        public void LoadScene(SceneName sceneToLoad, bool isNetworkSceneActive = true)
        {
            StartCoroutine(Loading(sceneToLoad, isNetworkSceneActive));
        }

        private IEnumerator Loading(SceneName sceneToLoad, bool isNetworkSceneActive = true)
        {
            LoadingFadeEffect.Instance.FadeIn();

            yield return new WaitUntil(() => LoadingFadeEffect.CanLoadScene == true);

            if (isNetworkSceneActive == true)
                LoadSceneNetwork(sceneToLoad);
            else
                LoadSceneLoacal(sceneToLoad);

            yield return new WaitForSeconds(1f);

            LoadingFadeEffect.Instance.FadeOut();
        }

        private void LoadSceneLoacal(SceneName sceneToLoad)
        {
            SceneManager.LoadScene(sceneToLoad.ToString(), LoadSceneMode.Single);
            switch (sceneToLoad) // Sahne yüklendikten sonra çalışmasını istedigin şeyleri yaz
            {
                case SceneName.Menu:
                    // todo: OwnerSceneInit (Müzik çalabilir)
                    break;
            }
        }

        private void LoadSceneNetwork(SceneName sceneToLoad)
        {
            if (NetworkManager.Singleton.IsServer)
                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad.ToString(), LoadSceneMode.Single);
        }

        // Burdaki fonksiyon = O sahne Serverda herkes tarafından yüklendikten sonra ortak olan şeyleri yapıcak. (ServerSceneInit)
        private void OnLoadComplete(ulong clientıd, string scenename, LoadSceneMode loadscenemode)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            Enum.TryParse(scenename, out _activeScene);

            if (!ClientConnection.Instance.CanClientConnect(clientıd)) // baglanabilir ise True döner.
                return;
            
            
            switch (_activeScene)
            {
                case SceneName.CharacterSelection:
                    CharacterSelectionManager.Instance.ServerSceneInit(clientıd);
                    break;
                case SceneName.GamePlay:
                    GamePlayManager.Instance.ServerSceneInit(clientıd);
                    break;
            }
        }
    }
}