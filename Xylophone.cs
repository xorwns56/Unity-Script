using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xylophone : MonoBehaviour
{
    public AudioClip[] audioClips;
    public Transform stick;
    public Transform player;

    struct TouchInfo
    {
        public float touchTime;
        public Vector2 touchPosition;
        public Vector2? stickLocalPosition;

        public TouchInfo(Vector2 touchPos)
        {
            touchTime = Time.time;
            touchPosition = touchPos;
            stickLocalPosition = null;
        }
    }

    private Vector3? destPosition;

    private int stickId;
    private List<int> touchIdList;
    private List<TouchInfo> touchInfoList;

    private RectTransform canvas;
    private RectTransform bg;
    private float w;
    private float h;

    private const float ShortTouchRange = 10f;
    private const float ShortTouchTime = 0.125f;

    private bool isShortTouch(Vector2 deltaPos, float touchTime)
    {
        deltaPos.x *= w;
        deltaPos.y *= h;
        return deltaPos.magnitude <= ShortTouchRange && Time.time - touchTime <= ShortTouchTime;
    }

    void Start()
    {
        destPosition = null;

        stickId = -1;
        touchIdList = new List<int>();
        touchInfoList = new List<TouchInfo>();

        canvas = transform.parent.GetComponent<RectTransform>();
        bg = GetComponent<RectTransform>();
        w = canvas.rect.width / Screen.width;
        h = canvas.rect.height / Screen.height;
    }

    void FixedUpdate()
    {
        if (destPosition != null)
        {
            Vector3 delta = (Vector3)destPosition - player.position;
            float move = 1f;
            if (delta.magnitude < move)
            {
                player.position = (Vector3)destPosition;
                destPosition = null;
            }
            else
            {
                player.position += delta.normalized * move;
            }
        }
    }

    void Update()
    {
        List<int> removeIdList = new List<int>();
        foreach (Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                TouchInfo info = new TouchInfo(t.position);
                Vector2 stickLocalPos = new Vector2((t.position.x - Screen.width) * w - bg.anchoredPosition.x + bg.rect.width, t.position.y * h - bg.anchoredPosition.y);
                if (0 < stickLocalPos.x && stickLocalPos.x < bg.rect.width && 0 < stickLocalPos.y && stickLocalPos.y < bg.rect.height)
                {
                    info.stickLocalPosition = stickLocalPos;
                }
                touchIdList.Add(t.fingerId);
                touchInfoList.Add(info);
            }
            else
            {
                int curr = touchIdList.IndexOf(t.fingerId);
                TouchInfo info = touchInfoList[curr];
                if(t.phase == TouchPhase.Moved)
                {
                    if (t.fingerId == stickId)
                    {
                        Vector2 deltaPos = t.position - info.touchPosition;
                        player.rotation = Quaternion.Euler(0, Mathf.Atan2(deltaPos.x, deltaPos.y) * Mathf.Rad2Deg, 0);
                    }
                    else if (stickId == -1 && info.stickLocalPosition != null && !isShortTouch(t.position - info.touchPosition, info.touchTime))
                    {
                        stickId = t.fingerId;
                    }
                }
                else if (t.phase == TouchPhase.Ended)
                {
                    if(t.fingerId == stickId)
                    {
                        Vector2 stickLocalPos = (Vector2)info.stickLocalPosition;
                        int barIdx = (int)(stickLocalPos.x / (bg.rect.width / 8));
                        GetComponent<AudioSource>().clip = audioClips[barIdx];
                        GetComponent<AudioSource>().Play();
                        RectTransform rt = Instantiate(stick, transform).GetComponent<RectTransform>();
                        rt.anchoredPosition = stickLocalPos;
                        Vector2 deltaPos = t.position - info.touchPosition;
                        rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(-deltaPos.x, deltaPos.y) * Mathf.Rad2Deg);
                        Destroy(rt.gameObject, rt.GetChild(0).GetComponent<Animation>().clip.length);
                        stickId = -1;
                    }
                    else if (isShortTouch(t.position - info.touchPosition, info.touchTime))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(t.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            destPosition = hit.point;
                        }
                    }
                    removeIdList.Add(t.fingerId);
                }
            }
        }

        foreach(int removeId in removeIdList)
        {
            int curr = touchIdList.IndexOf(removeId);
            touchIdList.RemoveAt(curr);
            touchInfoList.RemoveAt(curr);
        }
    }
}
