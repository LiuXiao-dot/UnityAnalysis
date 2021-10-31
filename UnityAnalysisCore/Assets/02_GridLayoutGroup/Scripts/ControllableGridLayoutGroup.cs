using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityAnalysis.Layout
{
    /// <summary>
    /// 可控行列的列表
    /// </summary>
    public class ControllableGridLayoutGroup : GridLayoutGroup
    {
        /// <summary>
        /// 空余列数X
        /// </summary>
        public int OffsetX
        {
            get
            {
                return offsetX;
            }
            set
            {
                offsetX = value;
                SetCellsAlongAxisX();
            }
        }

        /// <summary>
        /// 空余行数Y
        /// </summary>
        public int OffsetY
        {
            get
            {
                return offsetY;
            }
            set
            {
                offsetY = value;
                SetCellsAlongAxisY();
            }
        }

        public int totalAmount;

        /// <summary>
        /// 偏移Item个数
        /// </summary>
        [SerializeField]
        private int offsetY = 0;
        [SerializeField]
        private int offsetX = 0;

        private List<RectTransform> childs = new List<RectTransform>();

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            int minColumns;
            int preferredColumns;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minColumns = preferredColumns = m_ConstraintCount;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minColumns = preferredColumns = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
            }
            else
            {
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(rectChildren.Count));
            }

            minColumns -= offsetX;
            preferredColumns -= offsetX;
            minColumns += 1;
            preferredColumns += 1;
            base.SetLayoutInputForAxis(
                padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
                padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
                -1, 0);

            CalculateCells();
        }

        public override void CalculateLayoutInputVertical()
        {
            int minRows = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minRows = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minRows = m_ConstraintCount;
            }
            else
            {
                float width = rectTransform.rect.width;
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                minRows = Mathf.CeilToInt(rectChildren.Count / (float)cellCountX);
            }

            minRows += offsetY;
            float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
            base.SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxisX();
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxisY();
        }

        private void SetCellsAlongAxisX()
        {
            var rectChildrenCount = rectChildren.Count;
            for (int i = 0; i < rectChildrenCount; i++)
            {
                RectTransform rect = rectChildren[i];

                m_Tracker.Add(this, rect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.SizeDelta);

                rect.anchorMin = Vector2.up;
                rect.anchorMax = Vector2.up;
                rect.sizeDelta = cellSize;
            }
        }

        private void SetCellsAlongAxisY()
        {
            var rectChildrenCount = rectChildren.Count;
            GetMaxCellCount(out int actualCellCountX, out int actualCellCountY, out int cellsPerMainAxis);
            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );
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

                positionX -= offsetX;
                positionY += offsetY;

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
            }
        }

        /// <summary>
        /// 获取一行和一列最大格子数量
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public void GetMaxCellCount(out int actualCellCountX, out int actualCellCountY, out int cellsPerMainAxis)
        {
            var rectChildrenCount = rectChildren.Count;

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;

                if (rectChildrenCount > cellCountX)
                    cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;

                if (rectChildrenCount > cellCountY)
                    cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);
                actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);
                actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
        }

        private void CalculateCells()
        {
            childs.Clear();
            var toIgnoreList = new List<Component>();
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var rect = rectTransform.GetChild(i) as RectTransform;
                if (rect == null)
                    continue;

                rect.GetComponents(typeof(ILayoutIgnorer), toIgnoreList);

                if (toIgnoreList.Count == 0)
                {
                    childs.Add(rect);
                    continue;
                }

                for (int j = 0; j < toIgnoreList.Count; j++)
                {
                    var ignorer = (ILayoutIgnorer)toIgnoreList[j];
                    if (!ignorer.ignoreLayout)
                    {
                        childs.Add(rect);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取子对象
        /// </summary>
        /// <returns></returns>
        public List<RectTransform> GetCells()
        {
            return childs;
        }
    }
}