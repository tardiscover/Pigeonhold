using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public Transform pixel1;
    public Transform pixel2;
    public Transform pixel3;
    public Transform pixel4;

    public RectTransform ladderTop;
    public RectTransform ladderMid;
    public RectTransform ladderBtm;

    private RectTransform rectTransform;
    private SpriteRenderer ladderMidRenderer;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ladderMidRenderer = ladderMid.GetComponent<SpriteRenderer>();

        pixel1.localPosition = new Vector3(pixel1.localPosition.x, 0, pixel1.localPosition.z);
        //pixel2.localPosition = new Vector3(pixel2.localPosition.x, ladderBot.rect.height * 2f, pixel2.localPosition.z);
        //pixel2.localPosition = new Vector3(pixel2.localPosition.x, ladderBot.rect.height * 2f, pixel2.localPosition.z);
        pixel2.localPosition = new Vector3(pixel2.localPosition.x, ladderMid.localPosition.y, pixel2.localPosition.z);
        //pixel3.localPosition = new Vector3(pixel3.localPosition.x, ladderMid.localPosition.y + ladderMidRenderer.size.y, pixel3.localPosition.z);
        pixel3.localPosition = new Vector3(pixel3.localPosition.x, ladderTop.localPosition.y - ladderTop.rect.height, pixel3.localPosition.z);
        pixel4.localPosition = new Vector3(pixel4.localPosition.x, rectTransform.rect.height, pixel4.localPosition.z);
        //pixel1.position = new Vector3(pixel1.position.x, pixel1.position.y, pixel1.position.z);
        //pixel4.position = new Vector3(pixel4.position.x, pixel4.position.y, pixel4.position.z);
    }

    private void Start()
    {
        ResizeLadderMid();
    }

    /// <summary>
    /// Resizes the middle portion of the ladder as a tiled sprite so that it fills the gap in the overall ladder height.
    /// </summary>
    void ResizeLadderMid()
    {
        float btmOfLadderTop = ladderTop.localPosition.y - ladderTop.rect.height;
        float topOfLadderBtm = ladderMid.localPosition.y;
        float newMidOfLadderHeight = btmOfLadderTop - topOfLadderBtm;

        Debug.Log($"btmOfLadderTop = {btmOfLadderTop}, topOfLadderBtm = {topOfLadderBtm}, newMidOfLadderHeight = {newMidOfLadderHeight}"); //!!!!!!!
        ladderMidRenderer.size = new Vector2(ladderMidRenderer.size.x, newMidOfLadderHeight);    //!!!!!!
        //ladderMidRenderer.UpdateGIMaterials();

    }
}
