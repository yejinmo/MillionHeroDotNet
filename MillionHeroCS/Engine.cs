using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MillionHeroCS
{

    public static class Engine
    {

        public static void KeyPressed(object sender, HotKeyEventArgs e)
        {
            Task.Factory.StartNew(delegate { Analyse(); });
        }

        static void Analyse()
        {
            if (first)
            {
                Point lt = Cursor.Position;
                Thread.Sleep(500);
                Console.WriteLine("请将鼠标放置于题目框右下角并悬停");
                Thread.Sleep(1500);
                Point rb = Cursor.Position;
                Console.WriteLine(lt.ToString() + rb.ToString());
                OCR.SetPoint(lt, rb);

                first = false;
            }
            Console.WriteLine("===============================>");
            Console.WriteLine("开始答题");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var ocr = OCR.GetString();
            Console.WriteLine(string.Format("图像识别耗时 {0}", watch.Elapsed));
            var obj = JsonConvert.DeserializeObject<dynamic>(ocr);
            string num_str = obj.words_result_num;
            int.TryParse(num_str, out var num);
            if (num > 0)
            {
                int question_count = 0;
                string question = obj.words_result[0].words;
                Dictionary<string, int> answers = new Dictionary<string, int>();
                Dictionary<string, int> question_answers = new Dictionary<string, int>();
                Dictionary<string, int> zhidao_answers = new Dictionary<string, int>();
                List<double> pmi = new List<double>();
                bool flag = false;
                bool flag_4 = false;
                bool flag_is_no = false;
                if (num > 4)
                {
                    flag_4 = true;
                    question = "";
                }
                int word_index = 0;
                foreach (var word in obj.words_result)
                {
                    if (num > 4 && word_index < num - 3)
                    {
                        question = question + word.words;
                    }
                    else
                    {
                        if (!flag && !flag_4)
                        {
                            flag = true;
                        }
                        else
                        {
                            string temp = word.words;
                            temp = temp.Trim();
                            if (temp.IndexOf('.') > 0 &&
                                temp.ToLower()[0] >= 'a' &&
                                temp.ToLower()[0] <= 'z')
                            {
                                temp = temp.Substring(temp.IndexOf('.') + 1);
                            }
                            answers.Add(temp, 0);
                            question_answers.Add(temp, 0);
                            zhidao_answers.Add(temp, 0);
                        }
                    }
                    word_index++;
                }
                question = question.Trim();
                question = question.Substring(question.IndexOf('.') + 1);
                Console.WriteLine("===============================>");
                Console.WriteLine("问题");
                if (question.IndexOf('不') > 0)
                {
                    flag_is_no = true;
                    question.Replace("不", "");
                }
                if (question.IndexOf('未') > 0)
                {
                    flag_is_no = true;
                    question.Replace("未", "");
                }
                ThreadPool.QueueUserWorkItem(delegate
                {
                    Process.Start("http://www.baidu.com/s?wd=" + question);
                });
                {
                    int total = 0;
                    int done_count = 0;
                    total++;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        question_count = GetCount("http://www.baidu.com/s?rn=1&wd=" + question);
                        done_count++;
                    });
                    Console.WriteLine(string.Format("\t{0}", question));
                    Console.WriteLine("答案");
                    foreach (var word in answers)
                    {
                        total++;
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            answers[word.Key] = GetCount("http://www.baidu.com/s?rn=1&wd=" + word.Key);
                            done_count++;
                        });
                        total++;
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            question_answers[word.Key] = GetCount("http://www.baidu.com/s?rn=1&wd=" + question + "%20" + word.Key);
                            done_count++;
                        });
                        total++;
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            zhidao_answers[word.Key] = GetCount("http://zhidao.baidu.com/search?&word=" + question + "%20" + word.Key, word.Key);
                            done_count++;
                        });
                    }
                    while (done_count != total)
                    {
                        if (watch.Elapsed > new TimeSpan(0, 0, 10))
                        {
                            Console.WriteLine(string.Format("查询超时 {0}", watch.Elapsed));
                            return;
                        }
                        Thread.Sleep(1);
                    }
                }
                double max = double.MinValue;
                int max_i = 0;
                double min = double.MaxValue;
                int min_i = 0;
                for (int i = 0; i < answers.Count; i++)
                {
                    var p = (double)question_answers.ElementAt(i).Value / (double)question_count * (double)answers.ElementAt(i).Value;
                    pmi.Add(p);
                    if (p > max)
                    {
                        max = p;
                        max_i = i;
                    }
                    if (p < min)
                    {
                        min = p;
                        min_i = i;
                    }
                }
                double zhidao_max = double.MinValue;
                int zhidao_max_i = 0;
                double zhidao_min = double.MaxValue;
                int zhidao_min_i = 0;
                bool all_zero = true;
                for (int i = 0; i < zhidao_answers.Count; i++)
                {
                    var p = zhidao_answers.ElementAt(i).Value;
                    if (p != 0)
                    {
                        all_zero = false;
                    }
                    if (p > zhidao_max)
                    {
                        zhidao_max = p;
                        zhidao_max_i = i;
                    }
                    if (p < zhidao_min)
                    {
                        zhidao_min = p;
                        zhidao_min_i = i;
                    }
                }
                Console.WriteLine(string.Format("|{0,2}|{1,15}|{2,15}|{3,15}|{4,15}|{5,5}|",
                    "选项", "答案", "单项指数", "组合指数", "PMI指数", "知道指数"));
                for (int i = 0; i < answers.Count; i++)
                {
                    Console.Write("|");
                    Console.Write(string.Format("{0,2}", (i + 1).ToString()));
                    Console.Write("|");
                    Console.Write(string.Format("{0,15}", answers.ElementAt(i).Key.ToString()));
                    Console.Write("|");
                    Console.Write(string.Format("{0,15}", answers.ElementAt(i).Value.ToString()));
                    Console.Write("|");
                    Console.Write(string.Format("{0,15}", question_answers.ElementAt(i).Value.ToString()));
                    Console.Write("|");
                    if (max_i == i && !flag_is_no)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    if (min_i == i && flag_is_no)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(string.Format("{0,15}", pmi[i].ToString("f2")));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("|");
                    if (zhidao_max_i == i && !flag_is_no && !all_zero)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    if (zhidao_min_i == i && flag_is_no && !all_zero)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(string.Format("{0,5}", zhidao_answers.ElementAt(i).Value.ToString()));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("|");
                    Console.Write("\n");
                }
            }
            watch.Stop();
            Console.WriteLine(string.Format("耗时 {0} S", watch.Elapsed));
        }

        static bool first = true;

        static int GetCount(string url)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.AllowWriteStreamBuffering = false;
                webrequest.ServicePoint.UseNagleAlgorithm = false;
                webrequest.Method = "GET";
                webrequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();
                using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string html = reader.ReadToEnd();
                        watch.Stop();
                        Console.WriteLine(url + " - " + watch.Elapsed);
                        html = html.Substring(html.IndexOf("百度为您找到相关结果约") + "百度为您找到相关结果约".Length);
                        html = html.Substring(0, html.IndexOf("个"));
                        html = html.Replace(",", "");
                        return int.Parse(html);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return 0;
            }
        }

        static int GetCount(string url, string keyword)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.AllowWriteStreamBuffering = false;
                webrequest.ServicePoint.UseNagleAlgorithm = false;
                webrequest.Method = "GET";
                webrequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();
                using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.Default))
                    {
                        string html = reader.ReadToEnd();
                        watch.Stop();
                        Console.WriteLine(url + " - " + watch.Elapsed);
                        if (html.IndexOf("抱歉，暂时没有找到与") > 0)
                            return 0;
                        int count = 0, n = 0;
                        if (keyword != "")
                        {
                            while ((n = html.IndexOf(keyword, n, StringComparison.InvariantCulture)) != -1)
                            {
                                n += keyword.Length;
                                ++count;
                            }
                        }
                        return count - 4;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return 0;
            }
        }

    }

}
