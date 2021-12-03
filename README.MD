# ExHantai爬虫

ExhantaiCrawler 是 Exhantai.org 网站的爬虫，可以获取本子信息以及下载本子图片。

虽然已有很多同类的项目存在，但还是自己写一个用得舒服，顺便练练多年没用过的C#。也因为有练手的因素存在，该项目尽可能多地使用第三方库，熟悉下生态。

小爬怡情，大爬伤身，爬虫不是 DOS 攻击，E绅士仅有一家，它需要我们共同的爱护~

## 不再更新

爬了几个本子，发现完全不会再去看，我也懒得当收藏家（况且盗版资源也没啥好收藏的），这项目似乎没有价值了。

# 用法

从浏览器读取Cookie并保存，支持Chrome和Firefox：

```shell script
Cli inspect --save
```

根据本子的地址下载图片：

```shell script
Cli download https://exhentai.org/g/<GID>/<Token>
```

# TODO

增加 Hentai@Home 支持？我看了下 H@H 客户端的源码，写的很烂啊，也没搜到其它开源实现。
