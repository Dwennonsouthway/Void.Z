using UnityEngine;
using UnityEngine.EventSystems;
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public Texture2D defaultCursor;
    public Texture2D hoverCursor;   // 泡泡上方
    public Texture2D creepyCursor;  // Entity 出現時

    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        SetDefault();
    }
    void OnEnable()
    {
        SetDefault();
    }

    public void SetDefault()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }

    public void SetHover()
    {
        Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    public void SetCreepy()
    {
        Cursor.SetCursor(creepyCursor, hotspot, CursorMode.Auto);
    }
}