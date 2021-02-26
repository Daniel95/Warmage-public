using UnityEngine;
using UnityEngine.UI;

public abstract class UiWindowBase : MonoBehaviour
{
    [SerializeField] private Button closeButton = null;

    private WindowMoveRaycastTarget windowMoveRaycastTarget = null;
    private bool started;
    private bool draggingWindow;
    private Vector3 offset;
    private Transform canvasTransform;
    private RectTransform canvasRectTransform;

    protected abstract void OnWindowOpen();

    protected abstract void OnWindowClose();

    protected virtual void Awake()
    {
        canvasTransform = UiGame.GetInstance().transform;

        started = true;
        gameObject.SetActive(false);

        windowMoveRaycastTarget = GetComponentInChildren<WindowMoveRaycastTarget>();
        canvasRectTransform = UiGame.GetInstance().GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && windowMoveRaycastTarget.mouseOver)
        {
            draggingWindow = true;
            offset = transform.position - Input.mousePosition;
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            draggingWindow = false;
        }

        if (draggingWindow)
        {
            Vector3 mousePosition = Input.mousePosition;

            if(mousePosition.x < 0)
            {
                mousePosition.x = 0;
            }
            else if(mousePosition.x > canvasRectTransform.rect.width)
            {
                mousePosition.x = canvasRectTransform.rect.width;
            }

            if (mousePosition.y < 0)
            {
                mousePosition.y = 0;
            }
            else if (mousePosition.y > canvasRectTransform.rect.height)
            {
                mousePosition.y = canvasRectTransform.rect.height;
            }

            transform.position = mousePosition + offset;
        } 

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        closeButton.onClick.AddListener(OnCloseButtonClick);

        if(started)
        {
            OnWindowOpen();
        }
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(OnCloseButtonClick);

        OnWindowClose();
    }

    private void OnCloseButtonClick()
    {
        gameObject.SetActive(false);
    }
}
