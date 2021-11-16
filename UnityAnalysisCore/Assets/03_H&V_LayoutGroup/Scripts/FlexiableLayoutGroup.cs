using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexiableLayoutGroup : LayoutGroup
{
    public enum FitType
    {
        // 方形 自适应
        Auto,
        // 固定行数
        FixedHorizontal,
        // 固定列数
        FixedVertical,
        // 固定行列数
        Fixed
    }

    public enum CellFitType
    {
        // 自适应
        Auto,
        // 固定宽度
        FixedWidth,
        // 固定高度
        FixedHeight,
        // 固定大小
        Fixed
    }

    [SerializeField] private int m_rows = 1;
    [SerializeField] private int m_columns = 1;
    [SerializeField] private FitType m_FitType;
    [SerializeField] private CellFitType m_cellFitType;

    [SerializeField] private Vector2 m_cellSize;

    [SerializeField] private Vector2 m_spacing;

    public override void CalculateLayoutInputVertical()
    {
        base.CalculateLayoutInputHorizontal();

        var childCount = rectChildren.Count;

        int rows = 0;
        int columns = 0;

        switch (m_FitType)
        {
            case FitType.Auto:
                var sqrt = Mathf.Sqrt(childCount);
                rows = Mathf.CeilToInt(sqrt);
                columns = rows;
                break;
            case FitType.FixedHorizontal:
                rows = Mathf.Max(m_rows,1);
                columns = Mathf.CeilToInt(childCount / rows) + (childCount % rows == 0? 0: 1);
                break;
            case FitType.FixedVertical:
                columns = Mathf.Max(m_columns,1);
                rows = Mathf.CeilToInt(childCount / columns) + (childCount % columns == 0 ? 0 : 1);
                break;
            case FitType.Fixed:
                rows = m_rows;
                columns = m_columns;
                break;
            default:
                break;
        }

        float parentWidth = rectTransform.rect.width - (columns - 1) * m_spacing.x - padding.left - padding.right;
        float parentHeight = rectTransform.rect.height - (rows - 1) * m_spacing.y - padding.top - padding.bottom;

        float cellWidth;
        float cellHeight;

        cellWidth = parentWidth / columns;
        cellHeight = parentHeight / rows;
        switch (m_cellFitType)
        {
            case CellFitType.Auto:
                break;
            case CellFitType.FixedWidth:
                cellWidth = m_cellSize.x;
                break;
            case CellFitType.FixedHeight:
                cellHeight = m_cellSize.y;
                break;
            case CellFitType.Fixed:
                cellWidth = m_cellSize.x;
                cellHeight = m_cellSize.y;
                break;
            default:
                break;
        }


        for (int i = 0; i < childCount; i++)
        {
            int rowCount = 0;
            int columnCount = 0;

            var item = rectChildren[i];

            switch (m_FitType)
            {
                case FitType.Auto:
                case FitType.Fixed:
                case FitType.FixedVertical:
                    rowCount = i / columns;
                    columnCount = i % columns;
                    break;
                case FitType.FixedHorizontal:
                    columnCount = i / rows;
                    rowCount = i % rows;
                    break;
                default:
                    break;
            }

            var xPos = (cellWidth * columnCount) + (m_spacing.x * columnCount) + padding.left;
            var yPos = (cellHeight * rowCount) + (m_spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, cellWidth);
            SetChildAlongAxis(item, 1, yPos, cellHeight);
        }
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
    }
}
