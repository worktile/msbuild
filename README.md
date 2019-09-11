想把MSBuild编译和构建的所有消息通过[Worktile](https://www.worktile.com)实时推送到你眼前？找对地方了 :stuck_out_tongue_closed_eyes:

# MSBuild Plugin for Worktile
![screenshot](https://raw.githubusercontent.com/worktile/msbuild/master/.images/2015-07-24_13-04-27.png)

# 怎么用？
下载代码，用Visual Studio 2015编译整个Solution。
> 别问我为什么不能用2013、2012或者2010，你应该紧跟时代的步伐。况且社区版的Visual Studio 2015是免费的。

找到`.output`目录，复制里面所有的文件到你方便调用的地方。比如`D:\tools\msbuild-worktile`。

使用MSBuild编译你的代码时，加入`/logger`开关并指定使用`Worktile.Plugins.MSBuildLogger.dll`以及你Worktile里面Incoming Message的地址。比如这段命令
```msbuild Poda.csproj /nologo /noconsolelogger /logger:D:\tools\msbuild-worktile\Worktile.Plugins.MSBuildLogger.dll;adce412979234872b56a5cf87895dcbe /v:m /t:rebui```
其中`/logger`开关后面紧跟`msbuild-worktile`文件所在位置，分号后面是要输出的Lesschar Incoming Message ID。
> 在使用之前你还需要在Worktile里面创建一个Incoming Message服务。创建之后的URL就是这里你需要填写的地址了。关于如何在Worktile里面创建Incoming Message服务，请参考[这里](https://shaunxu.worktile.com/help/services)。我知道他们的帮助写的很不完善，不过我相信你可以自己创建一个试试，马上就会明白的。

执行上述命令，保证你的Worktile已经打开并且在你已经绑定的频道，那么你就会看到如上面截图类似的实时消息了。

# 支持推送什么消息？
除了Target和Task启动结束消息以外，所有的消息原则上都会被推送。这主要取决于你的命令中`/verbosity`的级别。
- `quiet`：不会推送任何消息。
- `minimal`：推送错误、警告、重要级别通知、项目编译完成和构建完成的消息。
- `normal`：除`minimal`以外，推送普通级别通知、项目编译开始和构建开始的消息。
- `detailed`：除`normal`以外，推送不重要级别通知的消息。
- `diagnostic`：和`detailed`一样。

# 消息内容
- 构建开始时间，结束时间，所耗费的总时间，构建结果以及错误、警告的数量。
- 项目编译的开始、结束消息及编译结果。
- 导致错误和警告的源代码文件名、所在位置（行、列）。

# 我想自己控制输出的内容和逻辑
源代码都在这里，很简单，你完全可以自己修改一些代码来达到所需的要求。

# 有Bug了！
我推荐你在这里提Issue。我确信Github会通过邮件通知我并且我会尽量第一时间回复。如果实在找不到我，可以试着自己改并且发Pull Request。
