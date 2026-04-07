using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SourceFiles.Scripts
{
    public class EditorPlayModeInitializer
    {
        private static string _bootScenePath = "Assets/Scenes/_Boot.unity";
        private static string _initialScenePath;
        private static bool _isProcessing;
        // Keep track of AudioListeners we changed so we can restore them after play
        private static System.Collections.Generic.List<System.Tuple<AudioListener, bool>> _modifiedAudioListeners =
            new System.Collections.Generic.List<System.Tuple<AudioListener, bool>>();

        [InitializeOnLoadMethod]
        private static void InitializePlayMode()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && !_isProcessing)
            {
                _isProcessing = true;
                
                _initialScenePath = EditorSceneManager.GetActiveScene().path;
                
                if (!_initialScenePath.Contains("_Boot"))
                {
                    var activeScene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.SaveScene(activeScene);
                    EditorSceneManager.OpenScene(_bootScenePath, OpenSceneMode.Single);
                    EditorSceneManager.OpenScene(_initialScenePath, OpenSceneMode.Additive);
                    // After both scenes are open in the editor, ensure only one AudioListener will be active
                    EnsureSingleAudioListenerBeforePlay();
                }
            }
            else if (state == PlayModeStateChange.EnteredPlayMode && _isProcessing)
            {
                UnloadBootScene();
                _isProcessing = false;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Restore any AudioListener enable states changed before entering play
                RestoreModifiedAudioListeners();
            }
        }

        private static void UnloadBootScene()
        {
            var bootScene = SceneManager.GetSceneByPath(_bootScenePath);
            if (bootScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(bootScene);
            }
        }

        private static void EnsureSingleAudioListenerBeforePlay()
        {
            try
            {
                _modifiedAudioListeners.Clear();

                // Find all AudioListeners in the currently loaded editor scenes
                // Use FindObjectsOfType with includeInactive; suppress obsolete warning for older Unity versions
                #pragma warning disable CS0618
                var listeners = Object.FindObjectsOfType<AudioListener>(true);
                #pragma warning restore CS0618
                if (listeners == null || listeners.Length <= 1)
                    return;

                // Prefer keeping the listener that belongs to the initially active scene
                AudioListener preferred = null;
                foreach (var l in listeners)
                {
                    if (l == null) continue;
                    var scene = l.gameObject.scene;
                    if (!string.IsNullOrEmpty(_initialScenePath) && scene.path == _initialScenePath)
                    {
                        preferred = l;
                        break;
                    }
                }
                if (preferred == null)
                    preferred = listeners[0];

                // Disable all other enabled listeners and record their previous state
                foreach (var l in listeners)
                {
                    if (l == null) continue;
                    if (l == preferred) continue;
                    // Record previous enabled state and disable if enabled
                    _modifiedAudioListeners.Add(System.Tuple.Create(l, l.enabled));
                    if (l.enabled)
                        l.enabled = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error ensuring single AudioListener before play: {ex}");
            }
        }

        private static void RestoreModifiedAudioListeners()
        {
            try
            {
                if (_modifiedAudioListeners == null || _modifiedAudioListeners.Count == 0)
                    return;

                foreach (var tup in _modifiedAudioListeners)
                {
                    var listener = tup.Item1;
                    var prev = tup.Item2;
                    if (listener != null)
                    {
                        listener.enabled = prev;
                    }
                }

                _modifiedAudioListeners.Clear();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error restoring AudioListeners after play: {ex}");
            }
        }
    }
}
#endif




