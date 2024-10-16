using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($"Entered scene: {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null)
            {
                Debug.Log("Playing scene music with fade.");
                AudioManager.i.PlayMusic(sceneMusic, fade: true);
            }

            // Load all connected scenes
            Debug.Log("Loading connected scenes...");
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
                Debug.Log($"Loaded connected scene: {scene.gameObject.name}");
            }

            // Unload the scenes that are no longer connected
            var prevScene = GameController.Instance.PrevScene;
            if (prevScene != null)
            {
                Debug.Log("Unloading previously connected scenes...");
                var previouslyLoadedScenes = GameController.Instance.PrevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                        Debug.Log($"Unloaded scene: {scene.gameObject.name}");
                    }
                }

                if (!connectedScenes.Contains(prevScene) && prevScene != this)
                {
                    prevScene.UnloadScene();
                    Debug.Log($"Unloaded previous scene: {prevScene.gameObject.name}");
                }
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            Debug.Log($"Loading scene: {gameObject.name}");
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            //IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                IsLoaded = true;
                Debug.Log($"Scene {gameObject.name} loaded. Restoring entities...");
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            Debug.Log($"Unloading scene: {gameObject.name}");
            SavingSystem.i.CaptureEntityStates(savableEntities);
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        Debug.Log($"Found {savableEntities.Count} savable entities in scene: {gameObject.name}");
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
