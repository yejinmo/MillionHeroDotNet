using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MillionHeroCS
{

    /// <summary>
    /// 图像识别 
    /// </summary>
    public static class OCR
    {

        private static Point PointLT = new Point(0, 0);
        private static Point PointRB = new Point(0, 0);
        private static string token = string.Empty;
        private static string clientId = "49nIniNfesLDzqnWUCdqHmim";
        private static string clientSecret = "ThLvKl9EtCvDNG1oPHsYlIiX71G3N6aU";

        /// <summary>
        /// 设置识别区域
        /// </summary>
        /// <param name="Point_LT">左上角坐标</param>
        /// <param name="Point_RB">右下角坐标</param>
        public static void SetPoint(Point Point_LT, Point Point_RB)
        {
            PointLT = Point_LT;
            PointRB = Point_RB;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            token = AccessToken.GetAccessToken(clientId, clientSecret);
            var data = JsonConvert.DeserializeObject<dynamic>(token);
            if (data.access_token != null)
            {
                token = data.access_token;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取识别返回结果
        /// </summary>
        /// <returns></returns>
        public static string GetString()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);
            g.Dispose();
            bmp = Cut(bmp, PointLT.X, PointLT.Y,
                 PointRB.X - PointLT.X, PointRB.Y - PointLT.Y);
            Compression(bmp, 50);
            return PostToAPI(bmp);
        }

        /// <summary>
        /// 图片压缩
        /// </summary>
        /// <param name="source">源图</param>
        /// <param name="flag">压缩比</param>
        /// <returns>压缩后的图</returns>
        private static Image Compression(Image source, int flag)
        {
            ImageFormat tFormat = source.RawFormat;
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageDecoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                return source;
            }
            catch
            {
                return source;
            }
        }

        /// <summary>
        /// 裁剪
        /// </summary>
        /// <param name="b"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        /// <returns></returns>
        private static Bitmap Cut(Bitmap b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null)
            {
                return null;
            }
            int w = b.Width;
            int h = b.Height;
            if (StartX >= w || StartY >= h)
            {
                return null;
            }
            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }
            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(bmpOut);
                g.DrawImage(b, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);
                g.Dispose();
                return bmpOut;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 提交至API
        /// </summary>
        /// <param name="bmp">图片</param>
        /// <returns></returns>
        private static string PostToAPI(Bitmap bmp)
        {
            string strbaser64 = ImgToBase64String(bmp); // 图片的base64编码
            string host = "https://aip.baidubce.com/rest/2.0/ocr/v1/general?access_token=" + token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = true;
            string str = "image=" + HttpUtility.UrlEncode(strbaser64);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 将图片转换为 Base64 编码
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        private static string ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch
            {
                return null;
            }
        }

    }

    /// <summary>
    /// 获取 Token
    /// </summary>
    public static class AccessToken
    {

        public static string GetAccessToken(string clientId, string clientSecret)
        {
            string authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<string, string>> paraList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            };

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
    }

}
