# GridLayoutGroup
Unity C#部分源码链接：<href>https://github.com/Unity-Technologies/UnityCsReference
## GridLayoutGroup基本介绍
网格布局组件，与其他布局组件不同，网格布局组件将忽略其包含的布局元素的min、prefer和flexible属性，
并为这些元素分配一个固定大小的网格，该网格的大小有GridLayoutGroup本身定义。

## 字段解析
```
// 网格的锚点
 protected Corner m_StartCorner;

// 主轴，
// 若为水平，则将在创建新一行对象前需要填充整行元素；
// 若为垂直，则将在创建新一列对象前需要填充整列元素。
 protected Axis m_StartAxis;

// 网格布局中为每个网格分配的大小
 protected Vector2 m_CellSize;

// 每个布局元素之间的间距
 protected Vector2 m_Spacing;

// 约束：将网格限定为固定行或固定列的布局方式
 protected Constraint m_Constraint;

// 约束的参数：限定行数或列数
protected int m_ConstraintCount;
```
## 属性解析
```
// 获取或设置约束的参数值
public int constraintCount { get; set; }

// 获取或设置约束类型
public Constraint constraint { get; set; }

// 获取或设置布局元素之间的间距
public Vector2 spacing { get; set; }

// 获取或设置网格的锚点
public Corner startCorner { get; set; }

// 获取或设置主轴
public Axis startAxis { get; set; }

// 获取或设置网格大小
public Vector2 cellSize { get; set; }
```
## 方法解析
注：Canvas中的布局修改需要通过Canvas.willRenderCanvases调用，当UI重新布局时，会触发CanvasUpdateRegistry.PerformUpdate（这个方法监听的是Canvas.willRenderCanvases这个委托）方法，
CanvasUpdateRegistry.PerformUpdate会调用LayoutRebuider中的Rebuid方法，该方法源码如下：
```
public void Rebuild(CanvasUpdate executing)
{
    switch (executing)
    {
        case CanvasUpdate.Layout:
            PerformLayoutCalculation(m_ToRebuild, e => (e as ILayoutElement).CalculateLayoutInputHorizontal());
            PerformLayoutControl(m_ToRebuild, e => (e as ILayoutController).SetLayoutHorizontal());
            PerformLayoutCalculation(m_ToRebuild, e => (e as ILayoutElement).CalculateLayoutInputVertical());
            PerformLayoutControl(m_ToRebuild, e => (e as ILayoutController).SetLayoutVertical());
            break;
    }
}
```
可以看到，在刷新布局时，会依次调用CalculateLayoutInputHorizontal，SetLayoutHorizontal，
CalculateLayoutInputVertical，SetLayoutVertical方法。

GridLayoutGroup中的上述四个方法代码如下：
```
public override void CalculateLayoutInputHorizontal()
{
  // 在base.CalculateLayoutInputHorizontal()中将获取到左右参与排序的元素
    base.CalculateLayoutInputHorizontal();

    int minColumns = 0;
    int preferredColumns = 0;
    if (m_Constraint == Constraint.FixedColumnCount)
    {
      // 固定列数：列数等于约束值
        minColumns = preferredColumns = m_ConstraintCount;
    }
    else if (m_Constraint == Constraint.FixedRowCount)
    {
      // 固定行数：列数等于总数量除以行数
        minColumns = preferredColumns = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
    }
    else
    {
      // 自适应：最小列数为1，首选列数为对总数量取平方根
        minColumns = 1;
        preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(rectChildren.Count));
    }

    // 设定该布局的大小参数，分别传入了min,prefer,flexible大小和轴（0：水平，1：竖直）
    // 布局系统会根据该值计算出该布局的实际大小
    SetLayoutInputForAxis(
        padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
        padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
        -1, 0);
}

public override void SetLayoutHorizontal()
{
    // 设置格子水平方向坐标
    SetCellsAlongAxis(0);
}

// 设定格子某方向上的坐标
private void SetCellsAlongAxis(int axis)
       {
           // 布局元素总数量
           var rectChildrenCount = rectChildren.Count;
           if (axis == 0)
           {
               // 只在水平方向layout时计算一次
               for (int i = 0; i < rectChildrenCount; i++)
               {
                   RectTransform rect = rectChildren[i];

                   m_Tracker.Add(this, rect,
                       DrivenTransformProperties.Anchors |
                       DrivenTransformProperties.AnchoredPosition |
                       DrivenTransformProperties.SizeDelta);
                   // 设定所有布局元素的锚点为up(0,1)，设置所有布局元素的大小为cellSize
                   rect.anchorMin = Vector2.up;
                   rect.anchorMax = Vector2.up;
                   rect.sizeDelta = cellSize;
               }
               return;
           }

           // 该布局的宽度和高度
           float width = rectTransform.rect.size.x;
           float height = rectTransform.rect.size.y;

           int cellCountX = 1;
           int cellCountY = 1;
           if (m_Constraint == Constraint.FixedColumnCount)
           {
               // 固定列数：水平方向格子数量为列数，竖直方向格子数量为总数量除以列数，
               // 不能整除时，再额外增加一行
               cellCountX = m_ConstraintCount;

               if (rectChildrenCount > cellCountX)
                   cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
           }
           else if (m_Constraint == Constraint.FixedRowCount)
           {
               // 固定行数：与固定列数计算方式相同
               cellCountY = m_ConstraintCount;

               if (rectChildrenCount > cellCountY)
                   cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
           }
           else
           {
               // 自适应大小:
               // 总大小-边距+一个间距的大小  除以  格子大小+间距 (注意：n个格子，间距是n-1个，所以计算时左边额外增加了一个间距值)
               if (cellSize.x + spacing.x <= 0)
                   cellCountX = int.MaxValue;
               else
                   cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

               if (cellSize.y + spacing.y <= 0)
                   cellCountY = int.MaxValue;
               else
                   cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
           }

           int cornerX = (int)startCorner % 2; // 0：左 1：右
           int cornerY = (int)startCorner / 2; // 0：上 1：下

           int cellsPerMainAxis, actualCellCountX, actualCellCountY;
           if (startAxis == Axis.Horizontal)
           {
               // 水平方向为主轴
               // 水平元素数量：1到总数量之间，上面代码中有设定为Int.MaxValue的会被约束到这个范围中 = rectChildrenCount。
              // 竖直元素数量：总数量/水平元素数量 并进行范围约束

               cellsPerMainAxis = cellCountX;
               actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);
               actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
           }
           else
           {
             // 竖直方向为主轴：计算方式同上
               cellsPerMainAxis = cellCountY;
               actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);
               actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
           }

           // 元素实际占用的大小
           Vector2 requiredSpace = new Vector2(
               actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
               actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
           );

           // 第一个元素左上角点的坐标
           Vector2 startOffset = new Vector2(
               GetStartOffset(0, requiredSpace.x),
               GetStartOffset(1, requiredSpace.y)
           );

           for (int i = 0; i < rectChildrenCount; i++)
           {
               int positionX;
               int positionY;
               if (startAxis == Axis.Horizontal)
               {
                   positionX = i % cellsPerMainAxis;
                   positionY = i / cellsPerMainAxis;
               }
               else
               {
                   positionX = i / cellsPerMainAxis;
                   positionY = i % cellsPerMainAxis;
               }

               if (cornerX == 1)
                   positionX = actualCellCountX - 1 - positionX;
               if (cornerY == 1)
                   positionY = actualCellCountY - 1 - positionY;

               // 设置每一个元素的大小与位置
               SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
               SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
           }
       }
```
