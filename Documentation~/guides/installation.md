# 安装说明

本文档介绍如何在 Unity 项目中安装和配置 ActionSequence 系统。

## 系统要求

- **Unity 版本**: 2019.3 或更高版本
- **脚本后端**: .NET Standard 2.0 或更高
- **平台支持**: 所有 Unity 支持的平台

## 安装方式

### 方式一：通过 Unity Package Manager（推荐）

1. 打开 Unity 编辑器
2. 打开 Package Manager（Window > Package Manager）
3. 点击左上角的 "+" 按钮
4. 选择 "Add package from git URL..."
5. 输入仓库 URL 并点击 "Add"

```
https://github.com/your-repo/ActionSequence.git
```

### 方式二：通过 manifest.json

1. 打开项目的 `Packages/manifest.json` 文件
2. 在 `dependencies` 部分添加：

```json
{
  "dependencies": {
    "com.yourcompany.actionsequence": "https://github.com/your-repo/ActionSequence.git",
    ...
  }
}
```

3. 保存文件，Unity 会自动下载并安装包

### 方式三：本地安装

1. 下载或克隆仓库到本地
2. 将 ActionSequence 文件夹复制到项目的 `Packages` 目录
3. Unity 会自动识别并导入包

## 验证安装

安装完成后，您应该能够：

1. 在 Project 窗口的 Packages 下看到 ActionSequence 包
2. 在代码中使用 `using ActionSequenceSystem;`
3. 在 GameObject 上添加 `ActionSequenceComponent` 组件

### 测试安装

创建一个测试脚本验证安装：

```csharp
using ActionSequenceSystem;
using UnityEngine;

public class InstallationTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("ActionSequence 安装成功！");
        
        // 创建一个简单的测试序列
        ActionSequences.AddSequence(new ActionSequenceModel
        {
            clips = new[]
            {
                new ActionClip
                {
                    StartTime = 0f,
                    Duration = 1f,
                    Action = new CallbackAction(() => Debug.Log("测试序列执行成功！"))
                }
            }
        }).Play();
    }
}
```

将脚本附加到场景中的 GameObject 上，运行场景，如果看到日志输出，说明安装成功。

## 配置选项

### 程序集定义

ActionSequence 包含以下程序集：

- `ActionSequence.Runtime` - 核心运行时
- `ActionSequence.Editor` - 编辑器扩展
- `ActionSequence.Tests.Runtime` - 运行时测试
- `ActionSequence.Tests.Editor` - 编辑器测试

如果您的项目使用程序集定义文件（.asmdef），需要在您的程序集中添加对 `ActionSequence.Runtime` 的引用。

### 脚本定义符号

ActionSequence 不需要特殊的脚本定义符号。

## 更新

### 通过 Package Manager 更新

1. 打开 Package Manager
2. 找到 ActionSequence 包
3. 如果有新版本，点击 "Update" 按钮

### 手动更新

1. 删除旧版本的 ActionSequence 文件夹
2. 按照安装步骤重新安装新版本

## 卸载

### 通过 Package Manager 卸载

1. 打开 Package Manager
2. 找到 ActionSequence 包
3. 点击 "Remove" 按钮

### 手动卸载

1. 从 `Packages/manifest.json` 中删除 ActionSequence 条目
2. 或直接删除 `Packages/ActionSequence` 文件夹

## 故障排除

### 编译错误

如果遇到编译错误：

1. 确保 Unity 版本满足要求（2019.3+）
2. 检查脚本后端设置（.NET Standard 2.0+）
3. 尝试重新导入包（右键 > Reimport）
4. 清理 Library 文件夹并重新打开项目

### 找不到命名空间

如果无法使用 `using ActionSequenceSystem;`：

1. 检查包是否正确安装
2. 如果使用 .asmdef，确保添加了程序集引用
3. 尝试重启 Unity 编辑器

### 组件不显示

如果无法添加 ActionSequenceComponent：

1. 检查包是否正确安装
2. 确保在 Play Mode 下运行
3. 检查控制台是否有错误信息

更多问题请查看 [故障排除](../troubleshooting.md) 或 [FAQ](../faq.md)。

## 下一步

安装完成后，请查看 [快速入门指南](quick-start.md) 开始使用 ActionSequence。
