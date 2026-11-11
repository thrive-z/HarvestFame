# 丰收农场 · HarvestFame

一款用 Unity 2023.1.1f1 和 C# 开发的第三人称农场经营游戏。玩家从锄头和花洒起步，在农场商店购买种子，开垦、播种、浇水和收获，再把作物用来烹饪或出售。跟随主线任务可以驾驶拖拉机前往驿站，也可以自由经营农场、饲养小鸡和采集资源。

## 游戏演示

[在线观看丰收农场游戏演示](https://thrive-z.github.io/HarvestFame/)（约 8 分钟） · [下载原版 MP4](https://github.com/thrive-z/HarvestFame/releases/download/showcase-2026-11-11/HarvestFame-Gameplay.mp4)

## 下载与运行

| 文件 | 使用方式 |
| --- | --- |
| [Windows 试玩版 ZIP](https://github.com/thrive-z/HarvestFame/releases/download/showcase-2026-11-11/HarvestFame-Windows.zip) | 完整解压到同一目录，双击 `HarvestFame.exe`。 |
| [Unity 资源包](https://github.com/thrive-z/HarvestFame/releases/download/showcase-2026-11-11/HarvestFame-Assets.unitypackage) | 在 Unity 2023.1.1f1 中通过 **Assets → Import Package → Custom Package** 导入。包含项目的 `Assets`，完整工程配置由 `Packages` 和 `ProjectSettings` 提供。 |

[查看 GitHub Releases](https://github.com/thrive-z/HarvestFame/releases/tag/showcase-2026-11-11)

## 我实现的功能

- **种植与经营**：耕地、播种、浇水和收获都由地块记录状态。收获的作物可以出售，也可以和木柴一起放进烹饪锅，等制作完成后领取成品。鸡场可以喂食、添水和收蛋。
- **背包与工具栏**：物品能在背包、工具栏和储物箱之间移动，支持堆叠与拆分。种植、烹饪和商店交易共用同一套物品数据。
- **任务与存档**：把 NPC 对话、拖拉机路标和任务面板串成一条可玩的流程。存档记录任务进度、物品栏、作物和车辆位置，重新进入后可以继续。

## 用到的技术

- **Unity 2023.1.1f1、C#、uGUI / TextMesh Pro**：制作交互提示、任务面板、背包和烹饪界面。
- **3D 射线检测**：从镜头判定当前交互目标，农具通过各自的行为脚本处理耕地、采石和砍树。
- **JSON 配置与存档**：使用 Newtonsoft.Json 读取物品、作物和配方配置。本地存档有版本号，保存时先写临时文件，再替换正式存档。

## 基本操作

| 按键 | 操作 |
| --- | --- |
| W / A / S / D、鼠标 | 移动、调整视角 |
| 1–0 | 选择工具栏格子 |
| 鼠标左键 | 使用当前工具 |
| F | 与人物或场景物件交互 |
| B | 打开背包 |
| G | 食用当前选中的食物 |
| J | 查看农场日志 |

## Unity 工程

使用 Unity Hub 添加仓库根目录，选择 Unity 2023.1.1f1 打开工程。用 Build Settings 中启用的 `SampleScene` 作为入口，进入 Play Mode 试玩。仓库中的 Unity 工程与本次试玩版和资源包对应。
