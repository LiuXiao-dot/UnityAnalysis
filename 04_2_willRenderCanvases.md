# Canvas的willRenderCanvases
该委托在Canvas需要刷新时执行，在UGUI中，CanvasUpdateRegistry添加了该委托的事件用于刷新UI界面的Layout。
## Canvas
```
// 执行Canvas渲染的委托
public static void ForceUpdateCanvases()
   {
       SendWillRenderCanvases();
   }

   [RequiredByNativeCode]
   private static void SendWillRenderCanvases()
   {
       willRenderCanvases?.Invoke();
   }
```
## CanvasUpdateRegistry
```
   // 在构造方法中注册
   protected CanvasUpdateRegistry()
   {
       Canvas.willRenderCanvases += PerformUpdate;
   }
   // Layout刷新
   private void PerformUpdate()
   {
       UISystemProfilerApi.BeginSample(UISystemProfilerApi.SampleType.Layout); // 开始检测，在Profile面板中可以看到
       CleanInvalidItems();

       m_PerformingLayoutUpdate = true;

       m_LayoutRebuildQueue.Sort(s_SortLayoutFunction);
       var layoutRebuildQueueCount = m_LayoutRebuildQueue.Count;

       for (int i = 0; i <= (int)CanvasUpdate.PostLayout; i++)
       {
           UnityEngine.Profiling.Profiler.BeginSample(m_CanvasUpdateProfilerStrings[i]);

           for (int j = 0; j < layoutRebuildQueueCount; j++)
           {
               var rebuild = m_LayoutRebuildQueue[j];
               try
               {
                   if (ObjectValidForUpdate(rebuild))
                       rebuild.Rebuild((CanvasUpdate)i);
               }
               catch (Exception e)
               {
                   Debug.LogException(e, rebuild.transform);
               }
           }
           UnityEngine.Profiling.Profiler.EndSample();
       }

       for (int i = 0; i < layoutRebuildQueueCount; ++i)
           m_LayoutRebuildQueue[i].LayoutComplete();

       m_LayoutRebuildQueue.Clear();
       m_PerformingLayoutUpdate = false;
       UISystemProfilerApi.EndSample(UISystemProfilerApi.SampleType.Layout);
       UISystemProfilerApi.BeginSample(UISystemProfilerApi.SampleType.Render);

       // now layout is complete do culling...
       UnityEngine.Profiling.Profiler.BeginSample(m_CullingUpdateProfilerString);
       ClipperRegistry.instance.Cull();
       UnityEngine.Profiling.Profiler.EndSample();

       m_PerformingGraphicUpdate = true;

       var graphicRebuildQueueCount = m_GraphicRebuildQueue.Count;
       for (var i = (int)CanvasUpdate.PreRender; i < (int)CanvasUpdate.MaxUpdateValue; i++)
       {
           UnityEngine.Profiling.Profiler.BeginSample(m_CanvasUpdateProfilerStrings[i]);
           for (var k = 0; k < graphicRebuildQueueCount; k++)
           {
               try
               {
                   var element = m_GraphicRebuildQueue[k];
                   if (ObjectValidForUpdate(element))
                       element.Rebuild((CanvasUpdate)i);
               }
               catch (Exception e)
               {
                   Debug.LogException(e, m_GraphicRebuildQueue[k].transform);
               }
           }
           UnityEngine.Profiling.Profiler.EndSample();
       }

       for (int i = 0; i < graphicRebuildQueueCount; ++i)
           m_GraphicRebuildQueue[i].GraphicUpdateComplete();

       m_GraphicRebuildQueue.Clear();
       m_PerformingGraphicUpdate = false;
       UISystemProfilerApi.EndSample(UISystemProfilerApi.SampleType.Render);
   }
```
