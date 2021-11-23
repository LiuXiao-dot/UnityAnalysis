# Unity静态合批
## Unity Graphic状态面板
CPU:处理和渲染一帧所需要的时间。（不包括编辑器中的逻辑耗时）

Batches:一帧内Unity合批处理的次数，包括：dynamic,static,instancing,即DrawCall次数。

Saved by batching:批处理后减少的DrawCall次数.
## UGUI中的深度
深度值的计算规则：
1：按照Hierarchy中从上往下的顺序依次遍历Canvas下的所有UI元素

2：

UI元素不渲染：depth = -1
UI需要渲染且不与其他UI元素相交，depth = 0
UI需要渲染，且下面只有一个UI元素与其相交，且两者的材质与贴图相同，两者depth相等，否则depth = 下方ui的depth + 1
UI需要渲染，且有n个元素相交时，按照上一条计算出n个depth，并取最大值为depth
## MeshBind
在UI的depth计算完毕后，会依次按照depth、material、textureID、RendererOder排序，然后提出depth = -1的UI元素，得到batch前的ui元素队列，合批在Canvas的Update方法中进行，但是查看C#部分代码无法详细了解。
## 合批
在同一个Canvas下，任何一个元素的材质、网格改变或创建删除UI元素都将使UI重新计算合批，重新计算生成网格，但不会影响子canvas和父canvas。

## 优化
1.使用图集，同一个图集将被视为可合批
2.动静分离，放置Canvas被大量重新计算
3.不需要UI触发的取消选中Raycast，这个操作将会在RaycastTargets中移除该UI元素
