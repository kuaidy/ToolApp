# Git 命令速查表
Git 是一款流行的代码版本管理工具，本页提供了常用 Git 命令的速查功能，以及相关的术语参考，是程序开发人员必备的参考工具。

## Git 常用命令
### 创建命令 Create
|Git 命令| 命令说明
|---|---|
|git clone url |克隆远程仓库
|git init |初始化本地 git 仓库（即创建新的本地仓库）

### 本地更改 Local Changes
|Git 命令| 命令说明
|---|---|
|git status |查看当前分支状态
|git diff |查看已跟踪文件的变更
|git add file |将指定的文件添加到暂存区
|git add .  |将所有有变更的文件添加到暂存区
|git commit -a |提交所有本地修改
|git commit -m "xxx" |把已添加至暂存区的文件执行提交，并以 xxx 作为本次提交的描述
|git commit --amend -m "xxx" |修改上一次提交（请勿用该命令修改已发布的提交）
|git commit -am "xxx" |该命令是 git add . 和 git commit -m "xxx" 的快捷方式
|git stash |暂存当前修改，将所有置为 HEAD 状态
|git stash list |查看所有暂存列表
|git stash push |把当前工作区的文件暂存临时空间
|git stash pop |把文件从临时空间中恢复到当前工作区

### 提交历史 Commit History
|Git 命令| 命令说明
|---|---|
|git log |查看提交日志
|git log -n |显示 n 行日志，n 为整数
|git log --stat |查看本地提交日志
|git show commit |查看提交日志及相关变动文件
|git show HEAD |查看 HEAD 提交日志
|git show HEAD^ |查看 HEAD 的上一个版本提交日志。另外，git show HEAD^^ 是查看上 2 个版本的提交日志；git show HEAD^5 是查看上 5 个版本的提交日志
|git blame file |对于指定文件，逐行显示提交的哈希ID、提交者、提交日期以及修改的内容
|git whatchanged |显示提交历史，以及每次提交变更的文件

### 分支和标签 Branches & Tags
|Git 命令| 命令说明
|---|---|
|git branch |查看本地分支
|git branch -r |查看远程分支
|git branch -a |查看所有分支（本地和远程）
|git branch --merged |查看所有分支已合并到当前分支的分支
|git branch --no-merged |查看所有分支未合并到当前分支的分支
|git branch -m new-branch |把当前分支的名称改成 new-branch；如果 new-branch 已存在，将不会执行改名
|git branch -M new-branch |强制把当前分支的名称改成 new-branch（即使 new-branch 已存在）
|git branch -m old-branch new-branch |把分支 old-branch 的名称改成 new-branch，如果 new-branch 已存在，将不会执行改名
|git branch -M old-branch new-branch |强制把分支 old-branch 的名称改成 new-branch（即使 new-branch 已存在）
|git checkout branch-name |切换到 branch-name 分支
|git branch new-branch |新建分支（也可以用 git checkout -b new-branch）
|git branch --track new remote |基于远程分支创建一个新分支，同 git checkout --track remote/branch
|git branch -d branch-name |删除本地分支
|git tag |列出所有本地标签
|git tag tag-name |基于最新的提交创建标签
|git tag -d tag-name |删除标签

### 删除命令 Remove
|Git 命令| 命令说明
|---|---|
|git rm file |删除文件（将从磁盘中删除文件）
|git rm -r directory |递归删除指定目录下的文件
|git rm --cached file |停止跟踪文件，不会从磁盘中删除

## 合并和衍合 Merge & Rebase
|Git 命令| 命令说明
|---|---|
|git merge branch |合并指定分支到当前分支，保留两个
|git rebase branch |合并指定分支到当前分支，只保留一个
|git rebase --abort |终止 rebase 操作，即回到执行 rebase 之前的状态
|git rebase --continue |解决冲突后继续执行 rebase
|git mergetool |使用配置文件指定的 mergetool 解决冲突
|git add resolved-file git rm resolved-file |使用编辑器手动解决文件冲突，并在冲突解决后，把文件标记为 resolved 

### 撤销命令 Undo
|Git 命令| 命令说明
|---|---|
|git reset --hard HEAD |将当前版本重置为 HEAD（用于 merge 失败的时候）
|git reset commit |将当前版本重置为某一个提交状态，代码不变
|git reset --hard commit |强制将当前版本重置为某一个提交状态，并丢弃那个状态之后的所有修改（请谨慎使用该命令）
|git reset --merge commit |将当前版本重置为某一个提交状态，并保留版本库中不同的文件
|git reset --keep commit |将当前版本重置为某一个提交状态，并保留未提交的本地修改
|git revert commit |撤销提交
|git restore file |丢弃指定文件的修改信息，即恢复到文件修改前的状态
|git checkout -- file |同 git restore file 命令
|git checkout HEAD file |对于指定文件，丢弃该文件的本地修改信息
|git clean |清除工作目录中未跟踪的文件
|git clean -n |列出哪些文件将从工作目录中删除

### 配置命令 Git Configuration
|Git 命令| 命令说明
|---|---|
|git config --list |列出当前 Git 配置
|git config --global user.name name |把参数 name 设置为当前用户使用的提交者的姓名；如果未指定 name 参数，则显示当前用户使用的提交者姓名
|git config --global user.email email |把参数 email 设置为当前用户使用的提交者的邮箱；如果未指定 email 参数，则显示当前用户使用的提交者邮箱
|git config --global alias.alias command |为 Git 命令创建全局的别名。比如，执行 alias.glog log --graph --oneline --decorate 命令后，git glog 就相当于 git log --graph --oneline --decorate。
|git config --system core.editor editor |对于本机的所有用户，设置命令使用的编辑器（比如 vim）
|git config --global --edit |在编辑器中打开全局配置文件（用于手动修改）
|git config --global color.ui auto |使用不同的颜色渲染 Git 命令的输出结果
### 其他命令 Other
|Git 命令| 命令说明
|---|---|
|git var -l |列出 Git 环境变量
|git help command |显示指定命令的帮助（将呼出该命令的 man 文件）

## 相关术语
下面列出了 Git 常见的术语，通过这些术语，可以更好地了解 Git 版本控制系统。

- git：一个开源的、分布式的版本控制系统
- commit：提交，指一个 Git 对象，该对象是整个仓库以 SHA 表示的快照
- branch：分支，一个轻量的、可移动的指向一个 commit 的指针
- clone：克隆，指在本地创建一份远程仓库的副本
- remote：远程仓库，是指托管在 GitHub、GitLab 等地方的公共（或私有）仓库，团队所有成员均可向该仓库提交修改
- fork：创建一个其他用户的仓库的副本
- pull request：简称 PR，指针对某个仓库，请求别人拉取并合并你的修改。通常，发起 pull request 的人都是从被请求人那里 clone 代码
- HEAD：表示当前工作区，使用 git checkout命令，可以把 HEAD 指针切换到不同的分支、标签和 commit 上