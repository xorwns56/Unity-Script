using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour
{
    public RectTransform stickRect;
    public Transform player;

    void Update()
    {
        if (stickRect.localPosition.magnitude != 0)
        {
            Vector3 stickVec = stickRect.localPosition.normalized;
            stickVec.z = stickVec.y;
            stickVec.y = 0;
            player.position += stickVec * Time.deltaTime * 30f;
        }
    }

    public void Drag(BaseEventData e)
    {
        PointerEventData data = e as PointerEventData;
        RectTransform canvas = transform.parent.GetComponent<RectTransform>();
        RectTransform bg = GetComponent<RectTransform>();
        float w = canvas.rect.width / Screen.width;
        float h = canvas.rect.height / Screen.height;
        Vector2 tVec = new Vector2(data.position.x * w - bg.anchoredPosition.x, data.position.y * h - bg.anchoredPosition.y);


        float radius = bg.sizeDelta.x * 0.5f;
        if (tVec.magnitude < radius)
        {
            stickRect.localPosition = Vector2.zero;
        }
        else
        {
            stickRect.localPosition = tVec.normalized * radius;
        }
    }

    public void DragEnd()
    {
        stickRect.localPosition = Vector2.zero;
    }
}
