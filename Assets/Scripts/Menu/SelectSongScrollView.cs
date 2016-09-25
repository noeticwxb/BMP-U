﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using BMS;

using Entry = SongInfoLoader.Entry;

public class SelectSongScrollView: MonoBehaviour {
    public BMSManager bmsManager;

    public GameObject prefab;
    public ScrollRect scroller;
    public Vector2 sizePerEntry;
    
    IList<Entry> entries;
    readonly List<SelectSongEntry> entryDisplay = new List<SelectSongEntry>();
    
    void Awake() {
        scroller.onValueChanged.AddListener(OnScroll);

        entries = SongInfoLoader.Entries;
        SongInfoLoader.OnListUpdated += UpdateList;
    }

    IEnumerator Start() {
        yield return null;
        for(float y = 0, h = scroller.viewport.rect.height + sizePerEntry.y; y < h; y += sizePerEntry.y) {
            var go = Instantiate(prefab);
            var t = go.GetComponent<RectTransform>();
            t.SetParent(scroller.content, false);
            entryDisplay.Add(go.GetComponent<SelectSongEntry>());
        }
    }

    void OnDestroy() {
        SongInfoLoader.OnListUpdated -= UpdateList;
    }

    void OnScroll(Vector2 position) {
        if(!SongInfoLoader.IsReady) return;
        Vector2 actualSize = scroller.content.rect.size;
        int startIndex = Mathf.Max(0, Mathf.FloorToInt(scroller.content.anchoredPosition.y / sizePerEntry.y));
        for(int i = 0, l = entryDisplay.Count, c = entries.Count; i < l; i++) {
            int actualIndex = startIndex + i;
            SelectSongEntry entryDisp = entryDisplay[i];
            if(actualIndex < c) {
                Vector2 pos = entryDisp.transform.anchoredPosition;
                pos.y = -actualIndex * sizePerEntry.y;
                entryDisp.transform.anchoredPosition = pos;

                Entry entry = entries[actualIndex];
                if(entry.isDirectory)
                    entryDisp.Load(entry.dirInfo, entry.isParentDirectory, this);
                else
                    entryDisp.Load(entry.songInfo, this);
                entryDisp.gameObject.SetActive(true);
            } else {
                entryDisp.gameObject.SetActive(false);
            }
        }
    }

    void UpdateList() {
        RectTransform content = scroller.content;
        content.sizeDelta = new Vector2(content.sizeDelta.x, sizePerEntry.y * entries.Count);
        OnScroll(scroller.normalizedPosition);
    }

    public void RefreshDisplay() {
        foreach(var entry in entryDisplay)
            if(entry.gameObject.activeSelf)
                entry.UpdateDisplay();
    }
}
