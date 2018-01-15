using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MillionHeroDotNet
{
    public static class Config
    {

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static readonly string ConfigPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini";

        public static string ClientId = "";
        public static string ClientSecret = "";


        /// <summary>
        /// 加载配置
        /// </summary>
        public static bool LoadConfig()
        {
            //判断是否存在配置文件
            if (!File.Exists(ConfigPath))
            {
                //写入配置
                ProfileWriteValue("Config", "ClientId", "", ConfigPath);
                ProfileWriteValue("Config", "ClientSecret", "", ConfigPath);
            }
            //读取配置 
            ClientId = ProfileReadValue("Config", "ClientId", ConfigPath, "");
            ClientSecret = ProfileReadValue("Config", "ClientSecret", ConfigPath, "");
            if (string.IsNullOrEmpty(ClientId) ||
                string.IsNullOrEmpty(ClientSecret))
                return false;
            return true;
        }

        /// <summary>
        /// 写入配置文件的接口
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 读取配置文件的接口
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="retVal"></param>
        /// <param name="size"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 向配置文件写入值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        private static long ProfileWriteValue(string section, string key, string value, string path)
        {
            var res = WritePrivateProfileString(section, key, value, path);
            return res;
        }

        /// <summary>
        /// 读取配置文件的值
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string ProfileReadValue(string section, string key, string path, string default_value = "")
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, default_value, sb, 255, path);
            return sb.ToString().Trim();
        }


    }
}
