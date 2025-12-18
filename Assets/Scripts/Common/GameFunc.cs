using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace TJ.Common
{
    /// <summary>
    /// 定义公用方法
    /// </summary>
    public class GameFunc
    {


        private static object Lock = new object();

        private static string s_strBundlePath = "";


        /// <summary>
        /// 获取游戏根目录
        /// </summary>
        static string appDir = "";
        public static string GetAppDir()
        {
            if (string.IsNullOrEmpty(appDir))
            {
                appDir = Application.streamingAssetsPath + "/";
            }

            return appDir;
        }

        public static string GetWritableDir()
        {
            return Application.persistentDataPath + "/";
        }


        /// <summary>
        /// 对象拷贝(纯反射实现)
        /// </summary>
        /// <param name="obj">被复制对象</param>
        /// <returns>新对象</returns>
        public static T CopyOjbect<T>(T obj)
        {
            if (obj == null)
            {
                return default;
            }
            object targetDeepCopyObj;
            Type targetType = obj.GetType();
            //值类型  
            if (targetType.IsValueType)
            {
                targetDeepCopyObj = obj;
            }
            //引用类型   
            else
            {
                targetDeepCopyObj = Activator.CreateInstance(targetType);   //创建引用对象   
                MemberInfo[] memberCollection = obj.GetType().GetMembers();

                foreach (MemberInfo member in memberCollection)
                {
                    //拷贝字段
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        object fieldValue = field.GetValue(obj);
                        if (fieldValue is ICloneable)
                        {
                            field.SetValue(targetDeepCopyObj, (fieldValue as ICloneable).Clone());
                        }
                        else
                        {
                            field.SetValue(targetDeepCopyObj, CopyOjbect(fieldValue));
                        }

                    }//拷贝属性
                    else if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo myProperty = (PropertyInfo)member;

                        MethodInfo info = myProperty.GetSetMethod(false);
                        if (info != null)
                        {
                            try
                            {
                                object propertyValue = myProperty.GetValue(obj, null);
                                if (propertyValue is ICloneable)
                                {
                                    myProperty.SetValue(targetDeepCopyObj, (propertyValue as ICloneable).Clone(), null);
                                }
                                else
                                {
                                    myProperty.SetValue(targetDeepCopyObj, CopyOjbect(propertyValue), null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("GameFunc CopyOjbect error: " + ex.Message);
                            }
                        }
                    }
                }
            }
            return (T)targetDeepCopyObj;
        }

        /// <summary>
        /// 对象拷贝(序列化实现)
        /// </summary>
        /// <param name="obj">被复制对象</param>
        /// <returns>新对象</returns>
        public static T DeepClone<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象
                retval = formatter.Deserialize(ms);
                ms.Close();
                ms.Dispose();
            }
            return (T)retval;
        }

        /// <summary>
        /// 科学计数法转为普通数字串
        /// </summary>
        /// <returns></returns>
        public static string PreseScientificCount(string strVal)
        {
            if (string.IsNullOrEmpty(strVal))
            {
                return null;
            }

            if (!strVal.Contains("E+"))
            {
                return strVal;
            }

            string[] strNums = strVal.Split("E+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strNums != null && strNums.Length == 2)
            {
                int idx = strNums[0].IndexOf('.');
                if (idx != -1)
                {
                    //有小数点
                    StringBuilder strNum = new StringBuilder(strNums[0].Remove(idx, 1));
                    int m = int.Parse(strNums[1]);
                    if (strNum.Length - 1 > m)
                    {
                        strNum.Insert(m, '.');
                    }
                    else if (strNum.Length - 1 < m)
                    {
                        strNum.Append('0', m - (strNum.Length - 1));
                    }
                    return strNum.ToString();
                }
                else
                {
                    StringBuilder strNum = new StringBuilder(strNums[0]);
                    int m = int.Parse(strNums[1]);
                    strNum.Append('0', m - (strNum.Length - 1));
                    return strNum.ToString();
                }
            }
            else
            {
                return strVal;
            }
        }

        /// <summary>
        /// 获得超大数字的简写
        /// x.x万 等
        /// </summary>
        /// <returns></returns>
        public static string GetSoBigNumV(string val, int endSize = 0)
        {
            string res = val;
            if (val.Length < 6)
            {
                StringBuilder sb = new StringBuilder(val);
                int insertIdx = sb.Length;
                while (insertIdx > 3)
                {
                    insertIdx -= 3;
                    sb.Insert(insertIdx, ",");
                }

                res = sb.ToString();
            }
            else
            {
                int wei = 0;
                string end = null;
                if (val.Length < 7)
                {
                    wei = 3;
                    end = "K";
                }
                else if (val.Length < 10)
                {
                    wei = 6;
                    end = "M";
                }
                else if (val.Length < 13)
                {
                    wei = 9;
                    end = "B";
                }
                else if (val.Length < 16)
                {
                    wei = 12;
                    end = "T";
                }
                else if (val.Length < 25)
                {
                    wei = 15;
                    end = "P";
                }
                else if (val.Length < 29)
                {
                    wei = 24;
                    end = "兆兆";
                }
                else if (val.Length < 33)
                {
                    wei = 28;
                    end = "穣";
                }
                else if (val.Length < 37)
                {
                    wei = 32;
                    end = "沟";
                }
                else if (val.Length < 41)
                {
                    wei = 36;
                    end = "涧";
                }
                else if (val.Length < 45)
                {
                    wei = 40;
                    end = "正";
                }
                else if (val.Length < 49)
                {
                    wei = 44;
                    end = "载";
                }
                else
                {
                    wei = 48;
                    end = "极";
                }

                val = val.Remove(val.Length - (wei - 2), wei - 2);
                if (val.Length < 5)
                {

                    val = val.Insert(val.Length - 2, ".");
                    res = val;
                }
                else
                {
                    val = val.Remove(val.Length - 2);

                    int insertIdx = val.Length;
                    while (insertIdx > 3)
                    {
                        insertIdx -= 3;
                        val.Insert(insertIdx, ",");
                    }

                    res = val.ToString();
                }

                if (endSize == 0)
                {
                    res += end;
                }
                else
                {
                    res += $"[size={endSize}]{end}[/size]";
                }
                res.Trim();
            }



            return res;
        }

        public static void GetTime(Callback<DateTime> end)
        {
            WebRequest request = null;
            WebResponse response = null;
            WebHeaderCollection headerCollection = null;
            DateTime time = DateTime.MinValue;
            try
            {
                request = WebRequest.Create("https://www.baidu.com");
                request.Timeout = 3000;
                request.Credentials = CredentialCache.DefaultCredentials;
                response = request.GetResponse();
                headerCollection = response.Headers;
                time = Convert.ToDateTime(headerCollection["Date"]);
            }
            catch (Exception e)
            {
                Debug.LogError("请求网络时间错误 " + e.Message);
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (headerCollection != null)
                {
                    headerCollection.Clear();
                }
                if (end != null)
                {
                    end(time);
                }
            }
        }

        public static async void GetTimeAsync(Callback<DateTime> end)
        {
            WebRequest request = null;
            WebResponse response = null;
            WebHeaderCollection headerCollection = null;
            DateTime time = DateTime.MinValue;
            try
            {
                request = WebRequest.Create("https://www.baidu.com");
                request.Timeout = 3000;
                request.Credentials = CredentialCache.DefaultCredentials;

                response = await request.GetResponseAsync();
                headerCollection = response.Headers;
                time = Convert.ToDateTime(headerCollection["Date"]);
            }
            catch (Exception e)
            {
                Debug.LogError("请求网络时间错误 " + e.Message);
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
                if (response != null)
                {
                    response.Close();
                }
                if (headerCollection != null)
                {
                    headerCollection.Clear();
                }
                if (end != null)
                {
                    end(time);
                }
            }
        }

        public static string ParseNum(double num)
        {
            decimal dNum = decimal.Parse(num.ToString(), System.Globalization.NumberStyles.Float);//去除科学计数法
            decimal absNum = Math.Abs(dNum);//取绝对值
            absNum = Math.Round(absNum);//取整
            string strVal = absNum.ToString();

            strVal = GetSoBigNumV(strVal);

            //添加符号
            if (dNum < 0)
            {
                strVal = strVal.Insert(0, "-");
            }

            return strVal;
        }



        public static List<string> FilterParamList(string strDescribe)
        {
            List<string> paramList = new List<string>();
            if (strDescribe.Contains("{#"))
            {
                int length = strDescribe.Length;
                int startIndex = 0;
                char[] removeChar = new char[] { '{', '#', '}' };
                while (length > 0)
                {
                    length = strDescribe.Length;
                    int tempIndex = strDescribe.IndexOf("{#", startIndex);
                    int endIndex = strDescribe.IndexOf('}', startIndex);
                    if (tempIndex != -1 && endIndex != -1)
                    {
                        string param = strDescribe.Substring(tempIndex, endIndex - tempIndex);
                        param = param.Trim(removeChar);
                        paramList.Add(param);
                        startIndex = endIndex + 1;
                        length -= endIndex;
                    }
                    else
                    {
                        length = 0;
                    }
                }
            }
            return paramList;
        }

        /// <summary>
        /// 将C#数据实体转化为JSON数据
        /// </summary>
        /// <param name="obj">要转化的数据实体</param>
        /// <returns>JSON格式字符串</returns>
        public static string JsonSerialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0;

            StreamReader sr = new StreamReader(stream);
            string resultStr = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return resultStr;
        }

        /// <summary>
        /// 将JSON数据转化为C#数据实体
        /// </summary>
        /// <param name="json">符合JSON格式的字符串</param>
        /// <returns>T类型的对象</returns>
        public static T JsonDeserialize<T>(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json.ToCharArray()));
            T obj = (T)serializer.ReadObject(ms);
            ms.Close();

            return obj;
        }

        public static byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// 通用路径
        /// </summary>
        /// <returns></returns>
        public static string GetBundleResPath()
        {
            if (!string.IsNullOrEmpty(s_strBundlePath))
            {
                return s_strBundlePath;
            }
            string strAssetPath = GetAppDir();
#if UNITY_ANDROID || (UNITY_STANDALONE && UNITY_EDITOR)
            return strAssetPath + "Res_Android/";
#elif UNITY_IOS
                return strAssetPath + "Res_IOS/";
#else
        		return strAssetPath + "Res/";
#endif
        }

    }
}