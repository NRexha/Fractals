using UnityEngine;

public class MandelbulbRenderer : MonoBehaviour
{
    #region Fields
    [Header("Mandelbulb Parameters")]
    [SerializeField] private ComputeShader _mandelbulbCompute;
    [SerializeField][Tooltip("Not real time")] private int _resolution = 1024;
    [SerializeField][Tooltip("Not real time")] private int _maxIterations = 100;
    [SerializeField] private float _power = 8.0f;
    [SerializeField] private float _scale = 1f;

    [Header("Exploring Parameters")]
    [SerializeField] private float _baseZoomSpeed = 0.1f;
    [SerializeField] private float _basePanSpeed = 0.005f;


    private RenderTexture _renderTexture;
    private Vector2 _focusPoint = Vector2.zero;
    private Vector2 _initialFocusPoint;
    private float _currentZoom = 1.0f;
    private float _initialZoom;
    private Vector2 _mouseLastPosition; 
    #endregion

    #region UnityMethods
    void Start()
    {
        InitializeRenderTexture();
        _initialZoom = _currentZoom;
        _initialFocusPoint = _focusPoint;
    }

    void Update()
    {
        HandleZoom();
        HandlePanning();


        if (Input.GetKeyDown(KeyCode.F))
        {
            ResetView();
        }

        RunComputeShader();

    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(_renderTexture, destination);
    }
    void OnDisable()
    {
        DisposeRenderTexture();
    }
    #endregion

    #region MandelbulbLogic
    void InitializeRenderTexture()
    {
        if (_renderTexture != null) _renderTexture.Release();

        _renderTexture = new RenderTexture(_resolution, _resolution, 0);
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();
    }
    void DisposeRenderTexture()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }
    }
    void RunComputeShader()
    {
        _mandelbulbCompute.SetTexture(0, "Result", _renderTexture);
        _mandelbulbCompute.SetInt("maxIterations", _maxIterations);
        _mandelbulbCompute.SetFloat("power", _power);
        _mandelbulbCompute.SetFloat("scale", _scale * _currentZoom);
        _mandelbulbCompute.SetFloats("focusPoint", _focusPoint.x, _focusPoint.y);

        int threadGroupsX = Mathf.CeilToInt(_renderTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(_renderTexture.height / 8.0f);
        _mandelbulbCompute.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    } 
    #endregion

    #region ExploreLogic
    void HandleZoom()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                scroll *= -1;

                float adjustedZoomSpeed = _baseZoomSpeed * _currentZoom;


                float newZoom = _currentZoom / (1.0f + scroll * adjustedZoomSpeed);

                Vector2 mousePosition = Input.mousePosition;
                Vector2 screenUV = new Vector2(
                    (mousePosition.x / Screen.width) * 2 - 1,
                    (mousePosition.y / Screen.height) * 2 - 1
                );
                _focusPoint -= screenUV * (_currentZoom - newZoom);
                _currentZoom = newZoom;
            }
        }
    }


    void HandlePanning()
    {
        if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftControl))
        {
            Vector2 mousePosition = Input.mousePosition;
            if (_mouseLastPosition != Vector2.zero)
            {
                float adjustedPanSpeed = _basePanSpeed * _currentZoom;

                Vector2 delta = (mousePosition - _mouseLastPosition) * adjustedPanSpeed;
                _focusPoint += delta;
            }
            _mouseLastPosition = mousePosition;
        }
        else
        {
            _mouseLastPosition = Vector2.zero;
        }
    }

    void ResetView()
    {
        _currentZoom = _initialZoom;
        _focusPoint = _initialFocusPoint;
    } 
    #endregion

}
