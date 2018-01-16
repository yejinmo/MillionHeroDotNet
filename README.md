# 百万英雄答题助手 C# 版本
## 功能
![demo](https://ws2.sinaimg.cn/large/c07597a3ly1fnhomcg7mbg20qo0f0x6p.jpg)
- 自动识别指定区域中的问题及答案
- 根据识别到的问题直接打开浏览器
- 从开始识别到出答案最快不到一秒钟（与网速有关）
- 根据百度搜索统计数目确定一个组合项目的最优解
- 根据百度知道统计数目确定一个最优解
## 使用方法
1. [点此下载夜神模拟器](https://www.yeshen.com/cn/download/fullPackage) 安装 并在之后安装西瓜视频
2. [点此下载](https://github.com/yejinmo/MillionHeroDotNet/releases/download/1.0/Release.rar) 答题助手的发布版本或自行编译
3. 打开 [百度通用文字识别](http://ai.baidu.com/tech/ocr/general) 并获取使用权限
4. 将 `API Key` 及 `Secret Key` 分别填到程序目录下 `config.ini`  文件中的 `ClientId` 和 `ClientSecret`
5. 运行模拟器并启动西瓜视频
6. 运行 `MillionHeroDotNet.exe`
7. 当程序启动并加载完成后使用全局热键 `Ctrl + Q` 激活选择题目区域的功能，按照提示操作即可
8. 之后再次使用 `Ctrl + Q` 即自动寻找答案
```
赶工代码写得又烂又丑 不要吐槽
```
## 效果图
![demo2](https://ws2.sinaimg.cn/large/c07597a3ly1fnhp5cto63j21hc0u0k7s.jpg)
## Tips
- 死记硬背类或课本上的问题，百度知道的准确率更高些
- 普通问题PMI指数的准确率更高些
## 原理
- 通过图像识别获取问题及答案
- 对问题和答案文本进行处理 如 判断问题是否包含 '不' 等表是否定的含义 以及去掉编号
- 将 `问题 + 各答案` 通过百度搜索统计出各项的组合数值 并计算得到一个 `PMI指数` 值最高的答案
- 有关 `PMI指数` 相关文献参考自 [https://en.wikipedia.org/wiki/Pointwise_mutual_information](https://en.wikipedia.org/wiki/Pointwise_mutual_information)
- 将 `问题 + 各答案` 通过百度知道搜索出页面中包含 `各答案` 的条数 理论上包含的条数越多 答案与问题的关联性越强
- 将 `PMI指数` 和 `知道指数` 进行排序得到一个最优解 若问题有否定含义则选择一个最低值作为最优解
## 参考项目
- [wuditken/MillionHeroes](https://github.com/wuditken/MillionHeroes)
- [lingfengsan/MillionHero](https://github.com/lingfengsan/MillionHero)
