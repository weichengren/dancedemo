using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TJ.Common
{
    /// <summary>
    /// 文件系统工具类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 搜索目录，获取指定文件
        /// </summary>
        /// <param name="dirPath">目标目录</param>
        /// <param name="fileter">过滤器："*.*"</param>
        /// <param name="recursive">true：包含子文件夹</param>
        /// <param name="fileList">返回值</param>
        public static List<string> GetFiles(string dirPath, string fileter, bool recursive)
        {
            List<string> fileList = null;

            if (Directory.Exists(dirPath))
            {
                string[] fileArr = Directory.GetFiles(dirPath, fileter);
                fileList = new List<string>(fileArr);

                if (recursive)
                {
                    string[] dirArr = Directory.GetDirectories(dirPath);
                    foreach (string dir in dirArr)
                    {
                        if (!dir.Contains("/."))
                        {
                            fileList.AddRange(GetFiles(dir, fileter, recursive));
                        }
                    }
                }
            }
            else
            {
                Debug.Log("GetFiles failed. Path is not exist: " + dirPath);
            }

            return fileList;
        }

        public static string GetFileName(string path)
        {
            string sTemp = path.Replace('\\', '/');
            int startIndex = sTemp.LastIndexOf('/') + 1;
            int endIndex = sTemp.LastIndexOf('.') - 1;
            string assetName = sTemp.Substring(startIndex, endIndex - startIndex + 1);

            return assetName;
        }


        /// <summary>
        /// 返回保存对象的目录
        /// </summary>
        public static string GetParentPath(string path)
        {
            return path.Substring(0, path.LastIndexOf('/') + 1);
        }

        /// <summary>
        ///  创建文件夹
        /// </summary>
        /// <param name="bRestrist">true-delete exist folder</param>
        /// <param name="recursive">true-delete include children folder</param>
        public static void CreateFolder(string path, bool bRestrist, bool recursive)
        {
            if (bRestrist)
            {
                Directory.Delete(path, recursive);
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateFolderDepth(string fileName)
        {
            string strPath = fileName.Replace('\\', '/');
            if (!strPath.EndsWith("/"))
            {
                strPath = strPath.Substring(0, strPath.LastIndexOf('/'));
            }

            string[] strPathArr = strPath.Split('/');
            string strNewPath = "";
            foreach (string s in strPathArr)
            {
                strNewPath += s;
                strNewPath += '/';

                if (!Directory.Exists(strNewPath))
                {
                    Directory.CreateDirectory(strNewPath);
                }
            }

        }

        public static void DeleteFolder(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }

        public static bool ExistFolder(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        ///  创建文件
        /// </summary>
        /// <param name="bRestrist">true-delete exist folder</param>
        public static void CreateFile(string path, bool bRestrist)
        {
            if (bRestrist)
            {
                File.Delete(path);
                File.Create(path).Dispose();
            }
            else
            {
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="suffix">后缀</param>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}