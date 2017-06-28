﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XAPI;
using XAPI.Callback;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace DataReceiver
{
    public class ApiBase
    {
        public int TradingDay;
        public string ConfigPath;
        public string DataPath;
        public string InstrumentInfoListFileName = @"InstrumentInfoList.json";
        public List<InstrumentInfo> InstrumentInfoList = new List<InstrumentInfo>();
        
        protected bool bIsLast;

        public List<XApi> XApiList = new List<XApi>();

        private static JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            // json文件格式使用非紧凑模式
            //NullValueHandling = NullValueHandling.Ignore,
            //DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        protected void Save(string path, string file, object obj)
        {
            using (FileStream fs = File.Open(Path.Combine(path, file), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (TextWriter writer = new StreamWriter(fs))
                {
                    writer.Write("{0}", JsonConvert.SerializeObject(obj, obj.GetType(), jSetting));
                    writer.Close();
                }
            }
        }
        protected object Load(string path, string file, object obj)
        {
            try
            {
                object ret;
                using (FileStream fs = File.Open(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (TextReader reader = new StreamReader(fs))
                    {
                        ret = JsonConvert.DeserializeObject(reader.ReadToEnd(), obj.GetType());
                        reader.Close();
                    }
                }
                
                return ret;
            }
            catch
            {
            }
            return obj;
        }

        protected object Load2(string path, string file, object obj)
        {
            try
            {
                object ret;
                using (FileStream fs = File.Open(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // 虽然是内置的，但因为DateTime转换不是想要的，所以弃用
                    var serializer = new DataContractJsonSerializer(obj.GetType(), new DataContractJsonSerializerSettings() {
                        DateTimeFormat = new DateTimeFormat("HH:MM:ss")
                    });
                    ret = serializer.ReadObject(fs);
                }

                return ret;
            }
            catch(Exception ex)
            {
                int i = 1;
            }
            return obj;
        }

        public virtual void Disconnect()
        {
            foreach (var api in XApiList)
            {
                api.Disconnect();
            }
        }

        /// <summary>
        /// 超时退出返回false
        /// 正常退出返回true
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool WaitConnectd(int timeout)
        {
            DateTime dt = DateTime.Now;
            
            do
            {
                bool IsConnected = true;
                foreach (var api in XApiList)
                {
                    if (!api.IsConnected)
                    {
                        IsConnected = false;
                        break;
                    }
                }

                if (IsConnected)
                    return true;

                // 超时退出
                if ((DateTime.Now - dt).TotalMilliseconds >= timeout)
                {
                    return false;
                }
                Thread.Sleep(1000);
            } while (true);
        }

        /// <summary>
        /// 超时退出返回false
        /// 正常退出返回true
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool WaitIsLast(int timeout)
        {
            DateTime dt = DateTime.Now;
            while (!bIsLast)
            {
                if ((DateTime.Now - dt).TotalMilliseconds >= timeout)
                {
                    return false;
                }
                Thread.Sleep(1000);
            }
            return true;
        }

        protected virtual void OnConnectionStatus(object sender, ConnectionStatus status, ref RspUserLoginField userLogin, int size1)
        {
            if (size1 > 0)
            {
                if (userLogin.RawErrorID != 0)
                {
                    (sender as XApi).GetLog().Info("{0}:{1}", status, userLogin.ToFormattedStringShort());
                }
                else
                {
                    (sender as XApi).GetLog().Info("{0}:{1}", status, userLogin.ToFormattedStringLong());
                }
            }
            else
            {
                (sender as XApi).GetLog().Info("{0}", status);
            }
            if (status == ConnectionStatus.Logined)
            {
                TradingDay = userLogin.TradingDay;
            }
        }
    }
}
