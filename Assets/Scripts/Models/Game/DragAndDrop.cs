using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.RuleTile.TilingRuleOutput;
using TMPro;
using System.Linq;

/// <summary>
/// Class used for drag and drop mechanics in level 3
/// </summary>
public class DragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Image image;
    private Vector2 originalPosition;
    private const float MINDISTANCE = 0.50f;
    private RectTransform rectTransform;

    /// <summary>
    /// Start is called before the first frame update, save original position
    /// </summary>
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        image = GetComponent<Image>();

    }

    /// <summary>
    /// Trigger on start dragging block. Check closest "busy" slot and, if not in range MINDISTANCE, set it as "available"
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        image.color = new Color32(255, 255, 255, 170);

        var slotsArray = GameObject.FindGameObjectsWithTag("Busy");
        if (slotsArray.Any())
        {
            var slots = slotsArray.ToList();

            var closestSlot = slots.Select(x => (x, Vector2.Distance(x.transform.position, transform.position))).OrderBy(x => x.Item2).FirstOrDefault();

            if (closestSlot.Item2 <= MINDISTANCE)
            {
                closestSlot.Item1.tag = "Available";
                closestSlot.Item1.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "-1";
            }
        }

    }

    /// <summary>
    /// Edit block position while dragging
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = Input.mousePosition;
        rectTransform.anchoredPosition += eventData.delta;
    }

    /// <summary>
    /// Trigger on dropping block. Modify block position to closest "available" slot in range MINDISTANCE and set slot "busy", set position to original position otherwise.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        var slotsArray = GameObject.FindGameObjectsWithTag("Available");
        if (slotsArray.Any())
        {
            var slots = slotsArray.ToList();
            var closestSlot = slots.Select(x => (x, Vector2.Distance(x.transform.position, transform.position))).OrderBy(x => x.Item2).FirstOrDefault();

            if (closestSlot.Item2 <= MINDISTANCE)
            {
                transform.position = closestSlot.Item1.transform.position;
                closestSlot.Item1.tag = "Busy";
                closestSlot.Item1.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
            }
            else
            {
                rectTransform.anchoredPosition = originalPosition;
            }
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        image.color = new Color32(255, 255, 255, 255);

    }



}
