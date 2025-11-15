@echo off
REM ActionSequence 文档构建脚本 (Windows)
REM 使用 DocFX 生成 HTML 文档

echo =========================================
echo ActionSequence 文档构建
echo =========================================

REM 检查 DocFX 是否安装
where docfx >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo 错误: DocFX 未安装
    echo 请访问 https://dotnet.github.io/docfx/ 安装 DocFX
    echo.
    echo 安装方法：
    echo   dotnet tool install -g docfx
    exit /b 1
)

echo √ DocFX 已安装
echo.

REM 清理旧的构建文件
echo 清理旧的构建文件...
if exist _site rmdir /s /q _site
if exist api rmdir /s /q api
if exist obj rmdir /s /q obj

echo √ 清理完成
echo.

REM 生成 API 元数据
echo 生成 API 元数据...
docfx metadata docfx.json

if %ERRORLEVEL% NEQ 0 (
    echo × API 元数据生成失败
    exit /b 1
)

echo √ API 元数据生成完成
echo.

REM 构建文档网站
echo 构建文档网站...
docfx build docfx.json

if %ERRORLEVEL% NEQ 0 (
    echo × 文档构建失败
    exit /b 1
)

echo √ 文档构建完成
echo.

REM 提示
echo =========================================
echo 文档构建成功！
echo =========================================
echo.
echo 输出目录: _site\
echo.
echo 预览文档：
echo   docfx serve _site
echo.
echo 然后在浏览器中访问: http://localhost:8080
echo.

pause
