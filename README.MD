# ExHantai 本子下载器

ExhantaiCrawler 是 Exhantai.org 网站的爬虫，可以获取本子信息以及下载本子图片。

虽然已有很多同类的项目存在，但还是自己写一个用得舒服，顺便练练多年没用过的C#。也因为有练手的因素存在，该项目尽可能多地使用第三方库，熟悉下生态。

# 用法

从浏览器读取Cookie并保存，支持Chrome和Firefox：
```
Cli inspect --save
```

根据本子的地址下载图片：
```
Cli download https://exhentai.org/g/<GID>/<Token>
```

# 关站风波

Exhantai.org 在 2019-7-26 日因荷兰法律而关站，该项目因此暂停开发。
但是没过几天就接受了不明人士的赞助，又好了，但是这个项目到底有无必要继续开发还是个问题。

# TODO

增加Hentai@Home支持？我看了下H@H客户端的源码，写的很烂啊。
