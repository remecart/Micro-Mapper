using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Bookmarks : MonoBehaviour
{
    [SerializeField] private GameObject bookmarkMenu;
    [SerializeField] private TextMeshProUGUI bookmarkName;
    [SerializeField] private GameObject bookmarkPrefab;
    [SerializeField] private Transform parent;

    private Slider timeline;
    [SerializeField] private List<GameObject> bookmarks = new List<GameObject>();

    public static Bookmarks instance;


    private Color FloatToColor(List<float> floats)
    {
        return new Color(floats[0], floats[1], floats[2], floats[3]);
    }

    private List<float> ColorToFloat(Color color)
    {
        return new() { color.r, color.g, color.b, color.a };
    }

    private void Start()
    {
        instance = this;
        timeline = SpawnObjects.instance.timeSlider;
    }

    public void ToggleBookmark()
    {
        bookmarkMenu.SetActive(true);
    }

    private void CloseBookmark()
    {
        bookmarkMenu.SetActive(false);
    }

    public void AddBookmarkToList()
    {
        var bookmark = CreateBookmark();
        LoadMap.instance.bookmarks.Add(bookmark);
        CloseBookmark();
        LoadBookmarks();
    }

    private bookmarks CreateBookmark()
    {
        return new bookmarks()
        {
            n = bookmarkName.text,
            b = SpawnObjects.instance.currentBeat,
            c = ColorToFloat(Random.ColorHSV(0f, 1f, 0.7f, 1f, 1f, 1f))
        };
    }

    public void LoadBookmarks()
    {
        ClearExistingBookmarks();
        foreach (var bookmark in LoadMap.instance.bookmarks)
        {
            CreateAndPlaceBookmark(bookmark);
        }
    }

    private void ClearExistingBookmarks()
    {
        foreach (var bookmark in bookmarks)
        {
            Destroy(bookmark);
        }

        bookmarks.Clear();
    }

    private void CreateAndPlaceBookmark(bookmarks bookmark)
    {
        var newBookmark = Instantiate(bookmarkPrefab, parent);
        AddEventTrigger(newBookmark);
        SetBookmarkColor(newBookmark, FloatToColor(bookmark.c));
        SetBookmarkPosition(newBookmark, bookmark.b);
        bookmarks.Add(newBookmark);
    }

    private void AddEventTrigger(GameObject newBookmark)
    {
        var trigger = newBookmark.GetComponent<EventTrigger>();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener((data) => { JumpToBookmark(bookmarks.IndexOf(newBookmark)); });
        trigger.triggers.Add(entry);
    }

    private void SetBookmarkColor(GameObject newBookmark, Color color)
    {
        newBookmark.GetComponentInChildren<RawImage>().color = color;
    }

    private void SetBookmarkPosition(GameObject newBookmark, float beat)
    {
        var parentsParent = parent.parent.GetComponent<RectTransform>();
        var pos = CalculatePosition(beat, parentsParent.rect.width);
        var rect = newBookmark.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(pos, rect.anchoredPosition.y);
    }

    private float CalculatePosition(float beat, float parentWidth)
    {
        var totalBeats = SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length);
        var normalizedBeat = beat / totalBeats;
        var pos = normalizedBeat * parentWidth - parentWidth / 2;
        return pos;
    }

    public void JumpToBookmark(int index)
    {
        var bookmark = LoadMap.instance.bookmarks[index];
        SpawnObjects.instance.currentBeat = bookmark.b;
        MoveSlider(bookmark.b / SpawnObjects.instance.BeatFromRealTime(LoadSong.instance.audioSource.clip.length));
        SpawnObjects.instance.LoadObjectsFromScratch(bookmark.b, true, true);
        DrawLines.instance.DrawLinesFromScratch(bookmark.b, SpawnObjects.instance.precision);
    }

    private void MoveSlider(float value)
    {
        Slider.SliderEvent sliderEvent = timeline.onValueChanged;
        timeline.onValueChanged = new Slider.SliderEvent();
        Debug.Log(value);
        timeline.value = value;
        timeline.onValueChanged = sliderEvent;
    }
}