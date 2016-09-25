﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using BMS;

public class SelectSongManager: MonoBehaviour {
    static int savedSortMode;
    
    public RectTransform loadingDisplay;
    public RectTransform loadingPercentageDisplay;
    public RectTransform optionsPanel;
    public RectTransform detailsPanel;
    public Button optionsButton;
    public Button optionsBackButton;
    public Dropdown gameMode;
    public Dropdown colorMode;
    public Toggle autoModeToggle;
    public Toggle detuneToggle;
    public Toggle bgaToggle;
    public Dropdown judgeModeDropDown;
    public Slider speedSlider;
    public Dropdown sortMode;
    public RawImage background;
    public ColorRampLevel colorSet;
    public Button startGameButton;

    [SerializeField]
    NoteLayoutOptionsHandler layoutOptionsHandler;
    [SerializeField]
    SelectSongScrollView selectSongScrollView;
    [SerializeField]
    SongInfoDetails detailsDisplay;

    public BMSManager bmsManager;

    SongInfo? currentInfo;

    void Awake() {
        if(!bmsManager) bmsManager = GetComponent<BMSManager>();
        if(!bmsManager) bmsManager = gameObject.AddComponent<BMSManager>();
        SongInfoLoader.SetBMSManager(bmsManager);

        gameMode.value = Loader.gameMode;
        gameMode.onValueChanged.AddListener(GameModeChange);
        colorMode.value = (int)Loader.colorMode;
        colorMode.onValueChanged.AddListener(ColorModeChange);
        autoModeToggle.isOn = Loader.autoMode;
        autoModeToggle.onValueChanged.AddListener(ToggleAuto);
        detuneToggle.isOn = Loader.enableDetune;
        detuneToggle.onValueChanged.AddListener(ToggleDetune);
        bgaToggle.isOn = Loader.enableBGA;
        bgaToggle.onValueChanged.AddListener(ToggleBGA);
        judgeModeDropDown.value = Loader.judgeMode;
        judgeModeDropDown.onValueChanged.AddListener(JudgeModeChange);
        speedSlider.value = Loader.speed;
        speedSlider.onValueChanged.AddListener(ChangeSpeed);
        sortMode.value = savedSortMode;
        sortMode.onValueChanged.AddListener(ChangeSortMode);
        startGameButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(ShowOptions);
        optionsBackButton.onClick.AddListener(HideOptions);

        currentInfo = SongInfoLoader.SelectedSong;
        SongInfoLoader.OnStartLoading += OnLoadingChanged;
        SongInfoLoader.OnListUpdated += OnLoadingChanged;
        SongInfoLoader.OnSelectionChanged += SelectionChanged;
        OnLoadingChanged();
    }

    void Start() {
        SongInfoLoader.CurrentCodePage = 932; // Hardcoded to Shift-JIS as most of BMS are encoded by this.
        SongInfoLoader.ReloadDirectory();
        InternalHideOptions();
    }

    void OnDestroy() {
        SongInfoLoader.OnStartLoading -= OnLoadingChanged;
        SongInfoLoader.OnListUpdated -= OnLoadingChanged;
        SongInfoLoader.OnSelectionChanged -= SelectionChanged;
    }

    void OnLoadingChanged() {
        loadingDisplay.gameObject.SetActive(!SongInfoLoader.IsReady);
    }

    void SelectionChanged(SongInfo? newInfo) {
        bool changed = false;
        if(currentInfo.HasValue != newInfo.HasValue)
            changed = true;
        else if(newInfo.HasValue && currentInfo.Value.filePath != newInfo.Value.filePath)
            changed = true;
        if(changed) {
            HideOptions();
        }
        currentInfo = newInfo;
    }

    public void GameModeChange(int index) {
        Loader.gameMode = index;
    }

    public void ColorModeChange(int index) {
        Loader.colorMode = (BMS.Visualization.ColoringMode)index;
    }

    public void ToggleAuto(bool state) {
        Loader.autoMode = autoModeToggle.isOn;
    }

    public void ToggleBGA(bool state) {
        Loader.enableBGA = bgaToggle.isOn;
    }

    public void ToggleDetune(bool state) {
        Loader.enableDetune = detuneToggle.isOn;
    }

    public void JudgeModeChange(int index) {
        Loader.judgeMode = index;
    }

    public void ChangeSpeed(float value) {
        Loader.speed = speedSlider.value;
    }

    public void ChangeSortMode(int mode) {
        savedSortMode = mode;
        switch(mode) {
            case 0: SongInfoLoader.CurrentSortMode = SongInfoComparer.SortMode.Name; break;
            case 1: SongInfoLoader.CurrentSortMode = SongInfoComparer.SortMode.Artist; break;
            case 2: SongInfoLoader.CurrentSortMode = SongInfoComparer.SortMode.Genre; break;
            case 3: SongInfoLoader.CurrentSortMode = SongInfoComparer.SortMode.Level; break;
            case 4: SongInfoLoader.CurrentSortMode = SongInfoComparer.SortMode.BPM; break;
        }
    }

    public void StartGame() {
        if(string.IsNullOrEmpty(Loader.songPath))
            return;
        HideOptions();
        switch(Loader.gameMode) {
            case 0: SceneManager.LoadScene("GameScene"); break;
            case 1: SceneManager.LoadScene("ClassicGameScene"); break;
        }
    }

    public void ShowOptions() {
        detailsPanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(true);
    }

    public void HideOptions() {
        layoutOptionsHandler.Apply();
        InternalHideOptions();
        selectSongScrollView.RefreshDisplay();
        detailsDisplay.ReloadRecord();
    }

    void InternalHideOptions() {
        detailsPanel.gameObject.SetActive(true);
        optionsPanel.gameObject.SetActive(false);
    }
}
