using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BookmarkData : MonoBehaviour, IPointerClickHandler
{
    public bookmarks bookmark;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SpawnObjects.instance.currentBeat = bookmark.b;
            SpawnObjects.instance.LoadObjectsFromScratch(bookmark.b, true, true);
            Bookmarks.instance.MoveSlider(bookmark.b / SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
            if (SpawnObjects.instance.playing)
            {
                LoadSong.instance.StopSong();
                LoadSong.instance.Offset(SpawnObjects.instance.GetRealTimeFromBeat(bookmark.b));
            }
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            LoadMap.instance.bookmarks.Remove(bookmark);
            this.GetComponent<Tooltip>().OnPointerExit(eventData);
            Bookmarks.instance.LoadBookmarks();
        }
    }

    private void Start()
    {
        GetComponent<Tooltip>().text = bookmark.n;
    }
}
