using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUi : MonoBehaviour
{
    public LayerMask layerMask;
    //HUD
    [SerializeField] private TextMeshProUGUI coinsTmp;
    [SerializeField] private TextMeshProUGUI livesTmp;
    [SerializeField] private TextMeshProUGUI wavesTotalTmp;
    [SerializeField] private TextMeshProUGUI currentWaveTmp;
    
    [SerializeField] private GameObject settingsPanel;
    
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject[] victoryStars;
    [SerializeField] private GameObject defeatPanel;
    
    [SerializeField] private GameObject enemyPanel;
    [SerializeField] private Image enemyImage;
    [SerializeField] private Image shadeEnemyImage;
    [SerializeField] private TextMeshProUGUI enemyDescription;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private Image towerImage;
    [SerializeField] private Image shadeTowerImage;
    [SerializeField] private TextMeshProUGUI towerDescription;

    [SerializeField] private AudioClip buttonClickSfx;
    
    private int _starsAmount;
    private Enemy _selectedEnemy;
    private Tower _selectedTower;

    private Camera _camera;
    private AudioSource _audioSource;
    
    public void UpdateUi()
    {
        coinsTmp.text = PlayerStats.Instance.Coins.ToString();
        livesTmp.text = PlayerStats.Instance.Lives.ToString();
        wavesTotalTmp.text = PlayerStats.Instance.WavesTotal.ToString();
        currentWaveTmp.text = PlayerStats.Instance.CurrentWave.ToString();
    }
    
    private void Start()
    {
        _camera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        GameManager.Instance.OnGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void HandleGameStateChanged(GameState previousState, GameState currentState)
    {
        settingsPanel.gameObject.SetActive(currentState == GameState.PAUSED);

        if (currentState == GameState.END)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        if (PlayerStats.Instance.Lives <= 0)
        {
            defeatPanel.SetActive(true);
        }
        else
        {
            victoryPanel.SetActive(true);
            _starsAmount = FindObjectOfType<PlayerStats>().EndGame();

            for (int i = 0; i < _starsAmount; i++)
            {
                victoryStars[i].SetActive(true);
            }
        }
    }

    public void FinishGame()
    {
        GameManager.Instance.FinishGame();  
    }

    public void OnSettingsClick()
    {
        PlayClickSfx();
        GameManager.Instance.TogglePause();
    }
    
    public void RestartGame()
    {
        PlayClickSfx();
        GameManager.Instance.LoadLevel(GameManager.Instance.currentLevelSo);
    }

    public void QuitToSelectionMenu()
    {
        PlayClickSfx();
        GameManager.Instance.LoadLevelSelection(Game.Instance.Path);
    }

    private void ShowEnemyDescription(EnemySO enemy)
    {
        if (towerPanel.activeSelf)
        {
            CloseTowerPanel();
        }
        
        _selectedEnemy.SelectEnemy();
        
        enemyImage.sprite = enemy.enemyIcon;
        shadeEnemyImage.sprite = enemy.enemyIcon;
        enemyDescription.text = enemy.enemyDescriptionShort;
        
        enemyPanel.SetActive(true);
    }
    
    //Show tower description after selecting it
    private void ShowTowerDescription(TowerSO tower)
    {
        if (enemyPanel.activeSelf)
        {
            CloseEnemyPanel();
        }
        
        towerImage.sprite = tower.towerImage;
        shadeTowerImage.sprite = tower.towerImage;
        towerDescription.text = tower.towerName + ": " +tower.towerDescription;
        
        towerPanel.SetActive(true);
    }
    
    private void CloseTowerPanel()
    {
        _selectedTower = null;
        towerPanel.SetActive(false);
    }

    private void CloseEnemyPanel()
    {
        if (_selectedEnemy != null)
        {
            _selectedEnemy.UnselectEnemy();
            _selectedEnemy = null;
        }
        
        enemyPanel.SetActive(false);
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.collider.GetComponent<Enemy>())
            {
                _selectedEnemy = hit.collider.GetComponent<Enemy>();
                ShowEnemyDescription(_selectedEnemy.enemySo);
                
            }
            else if(hit.collider.GetComponent<Tower>())
            {
                if (_selectedTower == null || _selectedTower != hit.collider.GetComponent<Tower>())
                {
                    _selectedTower = hit.collider.GetComponent<Tower>();
                    ShowTowerDescription(_selectedTower.currentTower);
                }
            }
        }
        else
        {
            CloseTowerPanel();
            CloseEnemyPanel();
        }
    }
    
    private void PlayClickSfx()
    {
        _audioSource.Stop();
        _audioSource.clip = buttonClickSfx;
        _audioSource.Play();
    }
}