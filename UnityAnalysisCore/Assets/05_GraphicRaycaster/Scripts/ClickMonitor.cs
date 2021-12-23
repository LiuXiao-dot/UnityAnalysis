using System.Collections.Generic;
using System.Linq;
using _00_Basic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _05_GraphicRaycaster.Scripts
{
    public class ClickMonitor : MonoBehaviour
    {
        private const string NoneUI = "未点击到UI";
        [SerializeField] private TaskController taskController;
        
        // Update is called once per frame
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
            RayCast0();
            // RayCast1();
        }

        private void RayCast0()
        {
            // 鼠标左键点击，进行射线检测，并打印所有被碰撞的物体
            var results = new List<RaycastResult>();
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var currentTask = taskController.GetCurrentTask();
            currentTask.GetComponentInChildren<GraphicRaycaster>().Raycast(pointerData, results);
            PrintList<string>(results.ConvertAll(temp => temp.gameObject.name));
        }

        private void RayCast1()
        {
            if (Camera.main is { })
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                PrintList<string>(hits.ToList().ConvertAll(temp => temp.transform.name));
            }
        }

        private void PrintList<T>(List<T> tList)
        {
            if (tList == null) return;
            string sLog = "";
            foreach (var t in tList)
            {
                sLog = $"{sLog}\n{t.ToString()}";
            }

            sLog = sLog == "" ? NoneUI : sLog;
            Debug.Log(sLog);
        }
    }
}
