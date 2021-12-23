using System;
using System.Linq;
using UnityEngine;

namespace _00_Basic
{
    public class TaskController : MonoBehaviour
    {
        private GameObject[] _tasks;
        [NonSerialized] private int _index;

        // 初始化任务
        private void Start()
        {
            _tasks = GameObject.FindGameObjectsWithTag("Task");
            var length = _tasks.Length;
            if (length == 0)
            {
                this.enabled = false;
                return;
            }

            var taskList = _tasks.ToList();
            taskList.Sort(SortByName);
            _tasks = taskList.ToArray();
            _index = 0;
            _tasks[0].SetActive(true);
            for (int i = 1; i < length; i++)
            {
                _tasks[i].SetActive(false);
            }
        }

        private int SortByName(GameObject x, GameObject y)
        {
            var sXs = x.name.Split('_');
            var sYs = y.name.Split('_');
            var isXMark = sXs.Length == 2;
            var isYMark = sYs.Length == 2;
            if (isXMark && isYMark)
                return int.Parse(sXs[1]).CompareTo(int.Parse(sYs[1]));

            return (isXMark ? 1 : -1) + (isYMark ? -1 : 1);
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                // 按下N键，切换任务
                var length = _tasks.Length;
                _tasks[_index].SetActive(false);
                _index = _index + 1 == length ? 0 : _index + 1;
                _tasks[_index].SetActive(true);
            }
        }

        public GameObject GetCurrentTask()
        {
            return _tasks[_index];
        }
    }
}