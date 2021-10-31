using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityAnalysis.Layout
{
    /// <summary>
    /// ѭ���б���չ��������ScrollRect,ControllableGridLayoutGroup
    /// </summary>
    public class ScrollLoopExtension : MonoBehaviour
    {
        /// <summary>
        /// �����б�
        /// </summary>
        [SerializeField]
        private ScrollRect scrollRect;
        private RectTransform rtScrollRect;

        /// <summary>
        /// �ɿز���
        /// </summary>
        [SerializeField]
        private ControllableGridLayoutGroup layoutGroup;

        /// <summary>
        /// �Ƿ����ݱ��޸�
        /// </summary>
        private bool dirty;

        /// <summary>
        /// �ܵ��Ӷ�������
        /// </summary>
        [SerializeField]
        private int cellAmount = 100;
        private int actualCellCountX;
        private int cellsPerMainAxis;
        private int actualCellCountY;
        private List<RectTransform> cells;

        private void Awake()
        {
            rtScrollRect = scrollRect.GetComponent<RectTransform>();
            scrollRect.onValueChanged.AddListener(onScroll);
        }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="scrollPosition"></param>
        private void onScroll(Vector2 scrollPosition)
        {
            dirty = true;
        }

        private void LateUpdate()
        {
            if (!dirty) return;
            layoutGroup.GetMaxCellCount(out actualCellCountX, out actualCellCountY, out cellsPerMainAxis);
            cells = layoutGroup.GetCells();
            Loop();
        }

        /// <summary>
        /// ѭ���߼�
        /// </summary>
        private void Loop()
        {
            dirty = false;

            // �����ж�
            var content = scrollRect.content;
            var size = layoutGroup.cellSize + layoutGroup.spacing;

            var dif = (content.anchoredPosition - new Vector2((layoutGroup.OffsetX - 3f) * size.x, (layoutGroup.OffsetY + 3f) * size.y)) / size; // λ�ò�ֵ(����)

            if (layoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                // ˮƽ����
                bool isMoveUp = dif.y > 0;
                int moveCountY = (int)Mathf.Abs(dif.y);

                if (moveCountY > 0)
                    MoveItemsY(isMoveUp, moveCountY);
            }
            else
            {
                // ��ֱ����
                bool isMoveLeft = dif.x < 0;
                int moveCountX = (int)Math.Abs(dif.x);

                if (moveCountX > 0)
                    MoveItemsY(isMoveLeft, moveCountX);
            }
        }

        /// <summary>
        /// ���ݼ���ֵ�ƶ�����λ��
        /// </summary>
        private void MoveItemsY(bool isMoveUpLeft, int moveCount)
        {
            var actualCount = layoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal ? actualCellCountX : actualCellCountY;
            for (int j = 0; j < moveCount; j++)
            {
                if (isMoveUpLeft)
                {
                    // �ϻ��������ƶ���������
                    var lastIndex = GetLastIndex();
                    if (lastIndex + 1 == cellAmount)
                    {
                        return;
                    }
                    for (int i = 0; i < actualCount; i++)
                    {
                        var top = cells[i];
                        top.SetAsLastSibling();

                        if (lastIndex + 1 == cellAmount)
                        {
                            top.gameObject.SetActive(false);
                            continue;
                        }
                        lastIndex++;
                        top.gameObject.SetActive(true);
                    }
                }
                else
                {
                    var firstIndex = GetFirstIndex();
                    if (firstIndex == 0)
                        return;
                    var count = cells.Count - 1;
                    for (int i = 0; i < actualCount; i++)
                    {
                        var bottom = cells[count - i];
                        bottom.SetAsFirstSibling();

                        if (firstIndex == 0)
                        {
                            bottom.gameObject.SetActive(false);
                            continue;
                        }
                        firstIndex--;
                        bottom.gameObject.SetActive(true);
                    }
                }

                if(layoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
                    layoutGroup.OffsetY += isMoveUpLeft ? 1 : -1;
                else
                    layoutGroup.OffsetX += isMoveUpLeft ? -1 : 1;
            }

        }

        /// <summary>
        /// ��ȡ��ǰ��һ�����ݵ�Index
        /// </summary>
        /// <returns></returns>
        private int GetFirstIndex()
        {
            if(layoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                return layoutGroup.OffsetY * actualCellCountX - layoutGroup.OffsetX;
            }
            else
            {
                return -layoutGroup.OffsetX * actualCellCountY + layoutGroup.OffsetY;
            }
        }

        /// <summary>
        /// ���һ��index
        /// </summary>
        /// <returns></returns>
        private int GetLastIndex()
        {
            var firstIndex = GetFirstIndex();
            return Mathf.Min(cellAmount - 1, firstIndex + actualCellCountX * actualCellCountY - 1);
        }
    }
}