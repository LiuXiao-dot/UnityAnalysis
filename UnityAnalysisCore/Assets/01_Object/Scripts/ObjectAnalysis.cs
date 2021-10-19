using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAnalysis.Transform
{
    /// <summary>
    /// Object∑÷Œˆ
    /// </summary>
    public class ObjectAnalysis : MonoBehaviour
    {
        private List<UnityAction> tasks;
        private int index;
        private AsyncOperationHandle<GameObject> handle;

        private void Awake()
        {
            tasks = new List<UnityAction>();
            index = 0;
            //tasks.Add(Task0);
            //tasks.Add(Task1);
            //tasks.Add(Task2);
            tasks.Add(Task3);
        }

        public void MoveNext()
        {
            if (tasks.Count == 0) return;
            if (index >= tasks.Count) index = 0;
            tasks[index]?.Invoke();
            index++;
        }


        /// <summary>
        ///  ‰≥ˆhideFlags
        /// </summary>
        private void Task0()
        {
            Debug.Log("task0:" + gameObject.hideFlags);
        }

        /// <summary>
        /// –ﬁ∏ƒhideFlags
        /// </summary>
        private void Task1()
        {
            gameObject.hideFlags = (HideFlags)(((byte)gameObject.hideFlags) << 1);
            if (gameObject.hideFlags == HideFlags.None) gameObject.hideFlags = HideFlags.HideInHierarchy;
            if (gameObject.hideFlags == HideFlags.HideAndDontSave) gameObject.hideFlags = HideFlags.None;
        }

        /// <summary>
        /// ≤‚ ‘FindObjectsOfType
        /// </summary>
        private void Task2()
        {
            Debug.Log($"count:{Object.FindObjectsOfType(typeof(BoxCollider)).Length}");
            handle = Addressables.LoadAssetAsync<GameObject>("Object");
            handle.Completed += result =>
            {
                var newObj = Instantiate(result.Result);
                Debug.Log($"count:{Object.FindObjectsOfType(typeof(BoxCollider)).Length}");
            };
        }

        /// <summary>
        /// ≤‚ ‘Destroy
        /// </summary>
        private void Task3()
        {
            handle = Addressables.LoadAssetAsync<GameObject>("Object");
            handle.Completed += result =>
            {
                StartCoroutine(Task3Coroutine());
            };
        }

        private IEnumerator Task3Coroutine()
        {
            var prefab = handle.Result;
            var newObj = GameObject.Instantiate(prefab);
            Debug.Log($"create:{Time.frameCount}");
            Destroy(newObj,1f);
            yield return new WaitUntil(() => newObj == null);
            Debug.Log($"destroy:{Time.frameCount}");
            newObj = GameObject.Instantiate(prefab);
            DestroyImmediate(newObj);
            yield return new WaitUntil(() => newObj == null);
            Debug.Log($"destroyImmediate:{Time.frameCount}");
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.hideFlags = HideFlags.DontSaveInBuild;
        }
#endif

        private void OnDestroy()
        {
            if(handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}