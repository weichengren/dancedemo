using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TJ.Common
{
    public class SingleCoroutine : MonoBehaviour
    {
        private static Transform s_Parent = null;   //父节点
        private static List<SingleCoroutine> s_UsingList = new List<SingleCoroutine>(); //使用中
        private static List<SingleCoroutine> s_UnUsingList = new List<SingleCoroutine>();   //未使用

        private bool m_IsLoading = false;   //正在加载

        void Start()
        {
            GameObject.DontDestroyOnLoad(this.gameObject.transform.root);
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Respwan()
        {
            if (gameObject.activeSelf)
            {
                this.StopAllCoroutines();
                gameObject.SetActive(false);
                m_IsLoading = false;
                if (!s_UnUsingList.Contains(this))
                {
                    s_UnUsingList.Add(this);
                }

                if (s_UsingList.Contains(this))
                {
                    s_UsingList.Remove(this);
                }
            }
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public IEnumerator LoadingSync(IEnumerator routine)
        {
            m_IsLoading = true;
            IEnumerator itor = routine;
            while (itor.MoveNext())
            {
                yield return null;
            }
            Respwan();
        }

        public bool IsLoading { get { return m_IsLoading; } }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static SingleCoroutine Loading(IEnumerator routine)
        {
            SingleCoroutine s = Spwan();
            s.StartCoroutine(s.LoadingSync(routine));

            return s;
        }

        /// <summary>
        /// 获取一个可用的coroutine
        /// </summary>
        /// <returns></returns>
        public static SingleCoroutine Spwan()
        {
            SingleCoroutine item = null;

            if (s_UnUsingList.Count > 0)
            {
                item = s_UnUsingList[0];
                item.gameObject.SetActive(true);
                s_UnUsingList.RemoveAt(0);
                s_UsingList.Add(item);
            }
            else
            {
                if (s_Parent == null)
                {
                    GameObject parentGO = new GameObject();
                    GameObject.DontDestroyOnLoad(parentGO);
                    parentGO.name = "Current_SingleCoroutines";
                    s_Parent = parentGO.transform;
                }

                GameObject go = new GameObject();
                go.transform.parent = s_Parent;
                item = go.AddComponent<SingleCoroutine>();
                item.gameObject.SetActive(true);
                s_UsingList.Add(item);
            }

            return item;
        }
    }
}