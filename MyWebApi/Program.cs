﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MyWebApi;
using MyWebApi.test;
using cfg = MyWebApi.Config;

// 加载配置选项
cfg.Load();

// webapp 设置
var webapp = (IApplicationBuilder app) =>
{
    // 生产环境异常处理
    app.UseExceptionHandler(ApiHandler.CustomExceptMW);
    // 开发环境异常处理 (系统中间件)
    //app.UseDeveloperExceptionPage();

    // 默认文档,静态文件 (系统中间件)
    app.UseDefaultFiles()
       .UseStaticFiles();

    // 提供wwwroot以外的其它虚拟目录(静态文件的) (系统中间件)
    if (cfg.VirtualDirsOptions != null)
    {
        foreach (var item in cfg.VirtualDirsOptions)
            app.UseStaticFiles(item);
    }

    // 跨域策略 (系统中间件)
    app.UseCors(cfg.CorsConfigBuild);

    // url映射到类的方法
    app.Run(ApiHandler.UrlMapMethodMW);
};

// 通用主机:(https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-7.0)
// web主机:(https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/host/web-host?view=aspnetcore-7.0)

// 建立通用主机->载入配置->运行
var host = new HostBuilder();
host.ConfigureServices((IServiceCollection services) =>
{
    // 跨域服务(https://learn.microsoft.com/zh-cn/aspnet/core/security/cors?view=aspnetcore-7.0)
    services.AddCors();
    // MemoryCache内存缓存工具.在ApiBase.SetHttpContext里获取使用
    services.AddMemoryCache();

    // 打印信息,用于测试
    services.AddSingleton<TestInfoService>()
            .AddHostedService<TestInfoService>();
})
.ConfigureWebHost((IWebHostBuilder webHostBuild) =>
{
    // 设置web服务主机
    // 使用kestrel服务器
    webHostBuild.UseKestrel()
    // 配置监听端点
    .UseUrls(cfg.Urls)
    // 加入应用程序
    .Configure(webapp);
});
// 部署成windows服务
//host.UseWindowsService();
// 启动
host.Build()
    .Run();


