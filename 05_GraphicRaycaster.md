# GraphicRaycaster
Unity C#部分源码链接：<href>https://github.com/Unity-Technologies
## GraphicRaycaster基本介绍
GraphicRaycaster继承自BaseRaycaster,是依赖于Canvas的射线检测控件。
GraphicRaycaster将检测Canvas中的所有Graphic是否有被射线击中的。
GraphicRaycaster可以配置为忽略背面图形以及被它前面的2D或3D对象阻挡。

## 字段解析
```
// 是否忽略背面图形，若为false，当击中背面图形时会忽略
private bool m_IgnoreReversedGraphics = true;

// 可阻挡射线检测的对象类型
private BlockingObjects m_BlockingObjects = BlockingObjects.None;

// 可阻挡射线检测的层级
protected LayerMask m_BlockingMask = kNoEventMaskSet;

// 当前脚本所在对象上的Canvas组件
private Canvas m_Canvas;

// 射线检测的结果
[NonSerialized] private List<Graphic> m_RaycastResults = new List<Graphic>();
```
## 属性解析
```
// 获取射线作用的摄像机
public override Camera eventCamera
    {
        get
        {
            var canvas = this.canvas;
            var renderMode = canvas.renderMode;
            if (renderMode == RenderMode.ScreenSpaceOverlay
                || (renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
                return null;

            return canvas.worldCamera ?? Camera.main;
        }
    }

```
## 方法解析
```
// 根据eventData执行射线检测，并将结果添加到resultAppendList中
public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
{
    // 不存在canvas时，不执行射线检测
    if (canvas == null)
        return;

    // 获取所有canvas组件所在对象上的Graphic组件：该方法在GraphicRegistry.cs中
    var canvasGraphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
    if (canvasGraphics == null || canvasGraphics.Count == 0)
        return;

    // camera支持8个displayIndex，每个相机只能选择其中一个，可以切换displayIndex实现多个屏幕显示的效果
    int displayIndex;
    var currentEventCamera = eventCamera; // Property can call Camera.main, so cache the reference

    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || currentEventCamera == null)
        displayIndex = canvas.targetDisplay;
    else
        displayIndex = currentEventCamera.targetDisplay;
    // x, y 返回相对空间中的坐标，z 返回处理鼠标输入的屏幕
    var eventPosition = Display.RelativeMouseAt(eventData.position);
    if (eventPosition != Vector3.zero)
    {
        // We support multiple display and display identification based on event position.

        int eventDisplayIndex = (int)eventPosition.z;

        // Discard events that are not part of this display so the user does not interact with multiple displays at once.
        if (eventDisplayIndex != displayIndex)
            return;
    }
    else
    {
        // The multiple display system is not supported on all platforms, when it is not supported the returned position
        // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
        eventPosition = eventData.position;

        // We dont really know in which display the event occured. We will process the event assuming it occured in our display.
    }

    // Convert to view space
    Vector2 pos;
    if (currentEventCamera == null)
    {
        // Multiple display support only when not the main display. For display 0 the reported
        // resolution is always the desktops resolution since its part of the display API,
        // so we use the standard none multiple display method. (case 741751)
        float w = Screen.width;
        float h = Screen.height;
        if (displayIndex > 0 && displayIndex < Display.displays.Length)
        {
            w = Display.displays[displayIndex].systemWidth;
            h = Display.displays[displayIndex].systemHeight;
        }
        pos = new Vector2(eventPosition.x / w, eventPosition.y / h);
    }
    else
        // 屏幕坐标转相机视口坐标
        pos = currentEventCamera.ScreenToViewportPoint(eventPosition);

    // If it's outside the camera's viewport, do nothing
    if (pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
        return;

    float hitDistance = float.MaxValue;

    Ray ray = new Ray();

    if (currentEventCamera != null)
        ray = currentEventCamera.ScreenPointToRay(eventPosition);

    if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && blockingObjects != BlockingObjects.None)
    {
        float distanceToClipPlane = 100.0f;

        if (currentEventCamera != null)
        {
            float projectionDirection = ray.direction.z;
            distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                ? Mathf.Infinity
                : Mathf.Abs((currentEventCamera.farClipPlane - currentEventCamera.nearClipPlane) / projectionDirection);
        }
    #if PACKAGE_PHYSICS
        if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
        {
            if (ReflectionMethodsCache.Singleton.raycast3D != null)
            {
                var hits = ReflectionMethodsCache.Singleton.raycast3DAll(ray, distanceToClipPlane, (int)m_BlockingMask);
                if (hits.Length > 0)
                    hitDistance = hits[0].distance;
            }
        }
    #endif
    #if PACKAGE_PHYSICS2D
        if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
        {
            if (ReflectionMethodsCache.Singleton.raycast2D != null)
            {
                var hits = ReflectionMethodsCache.Singleton.getRayIntersectionAll(ray, distanceToClipPlane, (int)m_BlockingMask);
                if (hits.Length > 0)
                    hitDistance = hits[0].distance;
            }
        }
    #endif
    }

    m_RaycastResults.Clear();

    Raycast(canvas, currentEventCamera, eventPosition, canvasGraphics, m_RaycastResults);

    int totalCount = m_RaycastResults.Count;
    for (var index = 0; index < totalCount; index++)
    {
        var go = m_RaycastResults[index].gameObject;
        bool appendGraphic = true;

        // 是否忽略背面的图形
        if (ignoreReversedGraphics)
        {
            if (currentEventCamera == null)
            {
                // If we dont have a camera we know that we should always be facing forward
                var dir = go.transform.rotation * Vector3.forward;
                appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
            }
            else
            {
                // If we have a camera compare the direction against the cameras forward.
                var cameraForward = currentEventCamera.transform.rotation * Vector3.forward * currentEventCamera.nearClipPlane;
                appendGraphic = Vector3.Dot(go.transform.position - currentEventCamera.transform.position - cameraForward, go.transform.forward) >= 0;
            }
        }

        if (appendGraphic)
        {
            float distance = 0;
            Transform trans = go.transform;
            Vector3 transForward = trans.forward;

            if (currentEventCamera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                distance = 0;
            else
            {
                // http://geomalgorithms.com/a06-_intersect-2.html
                distance = (Vector3.Dot(transForward, trans.position - ray.origin) / Vector3.Dot(transForward, ray.direction));

                // Check to see if the go is behind the camera.
                if (distance < 0)
                    continue;
            }

            if (distance >= hitDistance)
                continue;

            var castResult = new RaycastResult
            {
                gameObject = go,
                module = this,
                distance = distance,
                screenPosition = eventPosition,
                displayIndex = displayIndex,
                index = resultAppendList.Count,
                depth = m_RaycastResults[index].depth,
                sortingLayer = canvas.sortingLayerID,
                sortingOrder = canvas.sortingOrder,
                worldPosition = ray.origin + ray.direction * distance,
                worldNormal = -transForward
            };
            resultAppendList.Add(castResult);
        }
    }
}

// 进行射线检测，并将素有触碰到的对象添加到results中
[NonSerialized] static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();
private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
{
    // Necessary for the event system
    int totalCount = foundGraphics.Count;
    for (int i = 0; i < totalCount; ++i)
    {
        Graphic graphic = foundGraphics[i];

        // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
        if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
            continue;

        // 坐标检测
        if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera, graphic.raycastPadding))
            continue;

        // 裁剪距离检测
        if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
            continue;

        if (graphic.Raycast(pointerPosition, eventCamera))
        {
            s_SortedGraphics.Add(graphic);
        }
    }

    // 按depth排序
    s_SortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
    totalCount = s_SortedGraphics.Count;
    for (int i = 0; i < totalCount; ++i)
        results.Add(s_SortedGraphics[i]);

    s_SortedGraphics.Clear();
}
```
