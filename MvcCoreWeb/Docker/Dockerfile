# 需要复制到发布文件中

FROM mcr.microsoft.com/dotnet/aspnet:3.1

ENV MYPATH /usr/local/src
ENV WEBDIR DockerWebTest

# 进入 docker 中的 、usr/local/src 目录
RUN cd $MYPATH

# 创建 DockerWeb 目录
RUN mkdir $WEBDIR

# 设置工作路径
WORKDIR $MYPATH/$WEBDIR

# 将当前文件夹下的所有文件复制到工作目录
COPY . .

# 向外暴露 5001 端口
EXPOSE 5001

# 执行 dotnet MvcCoreWeb.dll 命令
CMD ["dotnet", "MvcCoreWeb.dll"]