# Horizontal&VerticalLayoutGroup
水平与垂直列表
## HorizontalOrVerticalLayoutGroup 基类
<div id = "HorizontalOrVerticalLayoutGroup"]</div>
HorizontalOrVerticalLayoutGroup为HorizontalLayoutGroup与VerticalLayoutGroup的基类，
查看源码可以发现，这两个类只有一个参数IsVertical不一样。

## 字段解析

```
// 两个元素间的间隔
protected float m_Spacing = 0;

// 是否控制子对象填充水平方向的额外空间
protected bool m_ChildForceExpandWidth = true;

// 是否控制子对象填充垂直方向的额外空间
protected bool m_ChildForceExpandHeight = true;

// true:LayoutGroup控制子元素的宽度
// false:子元素自行确定宽度
protected bool m_ChildControlWidth = true;

// true:LayoutGroup控制子元素的高度
// false:子元素自行确定高度
protected bool m_ChildControlHeight = true;

// true:在计算子元素宽度时应用子对象的x轴的scale
// false:缩放不影响坐标
protected bool m_ChildScaleWidth = false;

// true:在计算子元素宽度时应用子对象的y轴的scale
// false:缩放不影响坐标
protected bool m_ChildScaleHeight = false;

// true:反置列表,最后的元素会展示在第一个
protected bool m_ReverseArrangement = false;
```

## 方法解析
```
// 计算子元素在axis轴方向的属性（大小等）
protected void CalcAlongAxis(int axis, bool isVertical)
{
    float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical); // 获取axis方向的边距
    bool controlSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight); // 是否控制axis方向的子元素size
    bool useScale = (axis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight); // 是否应用axis方向子元素的scale
    bool childForceExpandSize = (axis == 0 ? m_ChildForceExpandWidth : m_ChildForceExpandHeight); // 是否控制子元素填充axis方向的空间

    float totalMin = combinedPadding;
    float totalPreferred = combinedPadding;
    float totalFlexible = 0;

    bool alongOtherAxis = (isVertical ^ (axis == 1)); // 异或运算,相同为true，不同为false，false:列表方向与axis相同，true:列表方向与axis不同
    var rectChildrenCount = rectChildren.Count;
    for (int i = 0; i < rectChildrenCount; i++)
    {
        RectTransform child = rectChildren[i];
        float min, preferred, flexible;
        GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible);

        if (useScale) // 应用scale
        {
            float scaleFactor = child.localScale[axis];
            min *= scaleFactor;
            preferred *= scaleFactor;
            flexible *= scaleFactor;
        }

        if (alongOtherAxis) // 应用边距
        {
            totalMin = Mathf.Max(min + combinedPadding, totalMin);
            totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
            totalFlexible = Mathf.Max(flexible, totalFlexible);
        }
        else
        {
            totalMin += min + spacing;
            totalPreferred += preferred + spacing;

            // Increment flexible size with element's flexible size.
            totalFlexible += flexible;
        }
    }

    if (!alongOtherAxis && rectChildren.Count > 0) // 应用间隔
    {
        totalMin -= spacing;
        totalPreferred -= spacing;
    }
    totalPreferred = Mathf.Max(totalMin, totalPreferred);
    SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
}

// 计算子元素的坐标
protected void SetChildrenAlongAxis(int axis, bool isVertical)
{
    float size = rectTransform.rect.size[axis];
    bool controlSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight);
    bool useScale = (axis == 0 ? m_ChildScaleWidth : m_ChildScaleHeight);
    bool childForceExpandSize = (axis == 0 ? m_ChildForceExpandWidth : m_ChildForceExpandHeight);
    float alignmentOnAxis = GetAlignmentOnAxis(axis);

    bool alongOtherAxis = (isVertical ^ (axis == 1));
    int startIndex = m_ReverseArrangement ? rectChildren.Count - 1 : 0;
    int endIndex = m_ReverseArrangement ? 0 : rectChildren.Count;
    int increment = m_ReverseArrangement ? -1 : 1;
    if (alongOtherAxis)
    {
        float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);

        for (int i = startIndex; m_ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
        {
            RectTransform child = rectChildren[i];
            float min, preferred, flexible;
            GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible);
            float scaleFactor = useScale ? child.localScale[axis] : 1f;

            float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
            float startOffset = GetStartOffset(axis, requiredSpace * scaleFactor);
            if (controlSize)
            {
                SetChildAlongAxisWithScale(child, axis, startOffset, requiredSpace, scaleFactor);
            }
            else
            {
                float offsetInCell = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
                SetChildAlongAxisWithScale(child, axis, startOffset + offsetInCell, scaleFactor);
            }
        }
    }
    else
    {
        float pos = (axis == 0 ? padding.left : padding.top);
        float itemFlexibleMultiplier = 0;
        float surplusSpace = size - GetTotalPreferredSize(axis);

        if (surplusSpace > 0)
        {
            if (GetTotalFlexibleSize(axis) == 0)
                pos = GetStartOffset(axis, GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));
            else if (GetTotalFlexibleSize(axis) > 0)
                itemFlexibleMultiplier = surplusSpace / GetTotalFlexibleSize(axis);
        }

        float minMaxLerp = 0;
        if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
            minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));

        for (int i = startIndex; m_ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
        {
            RectTransform child = rectChildren[i];
            float min, preferred, flexible;
            GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible);
            float scaleFactor = useScale ? child.localScale[axis] : 1f;

            float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
            childSize += flexible * itemFlexibleMultiplier;
            if (controlSize)
            {
                SetChildAlongAxisWithScale(child, axis, pos, childSize, scaleFactor);
            }
            else
            {
                float offsetInCell = (childSize - child.sizeDelta[axis]) * alignmentOnAxis;
                SetChildAlongAxisWithScale(child, axis, pos + offsetInCell, scaleFactor);
            }
            pos += childSize * scaleFactor + spacing;
        }
    }
}    

// 获取子元素的属性值
private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
    out float min, out float preferred, out float flexible)
{
    if (!controlSize)
    {
        min = child.sizeDelta[axis];
        preferred = min;
        flexible = 0;
    }
    else
    {
        min = LayoutUtility.GetMinSize(child, axis);
        preferred = LayoutUtility.GetPreferredSize(child, axis);
        flexible = LayoutUtility.GetFlexibleSize(child, axis);
    }

    if (childForceExpand)
        flexible = Mathf.Max(flexible, 1);
}
```
