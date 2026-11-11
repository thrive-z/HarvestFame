# 丰收农场 · HarvestFame

一款用 Unity 2023.1.1f1 和 C# 开发的第三人称农场经营游戏。玩家从锄头和花洒起步，在农场商店购买种子，开垦、播种、浇水和收获，再把作物用来烹饪或出售。跟随主线任务可以驾驶拖拉机前往驿站，也可以自由经营农场、饲养小鸡和采集资源。

## 下载与运行

| 文件 | 使用方式 |
| --- | --- |
| [Windows 试玩版 ZIP](https://github.com/thrive-z/HarvestFame/releases/download/showcase-2026-11-11/HarvestFame-Windows.zip) | 完整解压到同一目录，双击 `HarvestFame.exe`。 |
| [Unity 资源包](https://github.com/thrive-z/HarvestFame/releases/download/showcase-2026-11-11/HarvestFame-Assets.unitypackage) | 在 Unity 2023.1.1f1 中通过 **Assets → Import Package → Custom Package** 导入。包含项目的 `Assets`，完整工程配置由 `Packages` 和 `ProjectSettings` 提供。 |

[查看 GitHub Releases](https://github.com/thrive-z/HarvestFame/releases/tag/showcase-2026-11-11)

## 游戏功能

- **种植与经营**：购买南瓜、西红柿等种子，按生长阶段浇水和收获。商店支持购买和出售。
- **背包与工具栏**：拖动移动物品，在背包和工具栏之间整理，支持同类物品堆叠和拆分，数量角标随操作更新。
- **烹饪与养殖**：在烹饪锅查看配方、食材和燃料，制作并领取番茄南瓜汤、水煮蛋等成品。鸡场支持添加草料和水、收集鸡蛋。
- **任务与载具**：与 NPC 对话推进任务，按顺序驾驶拖拉机通过路标，任务面板同步显示进度。
- **采集与存档**：使用对应工具采集资源，可通过游戏菜单继续存档。

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
