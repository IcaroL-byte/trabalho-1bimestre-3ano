using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameMenager : MonoBehaviour
{
    public enum GameState
    {
        Iniciando,
        MenuPrincipal,
        Gameplay
    }

    public static GameMenager Instance { get; private set; }

    [Header("Configurações de Cenas")]
    [SerializeField] private string cenaSplash = "CenaSplash";
    [SerializeField] private string cenaMenu = "MenuPrincipal";
    [SerializeField] private string cenaGameplay = "GetStarted_Scene";

    [Header("Status")]
    public GameState estadoAtual;

    private PlayerInput _playerInputNaCena;

    private void Awake()
    {
        ConfigurarSingleton();
    }

    private void Start()
    {
        // Garante que o jogo comece carregando a primeira cena lógica
        CarregarCena(cenaSplash);
    }

    private void ConfigurarSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Altera o estado lógico do jogo e dispara comportamentos específicos.
    /// </summary>
    public void TrocarEstado(GameState novoEstado)
    {
        if (estadoAtual == novoEstado) return;

        estadoAtual = novoEstado;
        Debug.Log($"<color=cyan>[GameManager]</color> Novo Estado: <b>{estadoAtual}</b>");

        switch (estadoAtual)
        {
            case GameState.Gameplay:
                IniciarConfiguracaoGameplay();
                break;
            case GameState.MenuPrincipal:
                Time.timeScale = 1f; // Garante que o jogo não esteja pausado no menu
                break;
        }
    }

    private void IniciarConfiguracaoGameplay()
    {
        if (gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(ConfigurarPlayerInputRoutine());
        }
    }

    public void CarregarCena(string nomeDaCena)
    {
        // Inscreve no evento antes de carregar
        SceneManager.sceneLoaded += AoTerminarDeCarregar;
        SceneManager.LoadScene(nomeDaCena);
    }

    private void AoTerminarDeCarregar(Scene cena, LoadSceneMode modo)
    {
        // Importante: Desinscrever sempre para evitar vazamento de memória
        SceneManager.sceneLoaded -= AoTerminarDeCarregar;

        // Lógica de troca de estado baseada na cena carregada
        if (cena.name == cenaMenu)
            TrocarEstado(GameState.MenuPrincipal);
        else if (cena.name == cenaGameplay)
            TrocarEstado(GameState.Gameplay);
    }

    private IEnumerator ConfigurarPlayerInputRoutine()
    {
        // Espera o fim do frame para garantir que os objetos de Awake/Start da cena carregada rodaram
        yield return new WaitForEndOfFrame();

        _playerInputNaCena = Object.FindFirstObjectByType<PlayerInput>();

        if (_playerInputNaCena != null)
        {
            _playerInputNaCena.ActivateInput();
            Debug.Log("<color=green>[GameManager]</color> Controle do Jogador Ativado.");
        }
        else
        {
            Debug.LogWarning("[GameManager] PlayerInput não encontrado na cena atual.");
        }
    }

    public void SairDoJogo()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
