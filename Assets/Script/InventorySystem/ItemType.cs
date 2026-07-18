//物品的大分类
public enum ItemType
{
    Food, //食物
    Animal, //动物
    Material, //材料
    Weapon, //武器
    Tool,  //工具
    Seed, //种子
    Bullet, //子弹
}

//二级分类
public enum ItemSubType
{

    None, //空

    //工具相关
    Axe, //斧子
    Pickaxe, //镐
    Hoe, //锄头
    Sickle, //镰刀
    WaterCan, //水壶
    FishingRod, //鱼竿


    //Food相关
    Tomato, //西红柿
    Pumpkin, //南瓜

    //Animal
    Chicken, //成年鸡
    LittleChicken, //幼年鸡

    //食物
    ChickenMeat, //鸡肉
    ChickenEgg, //鸡蛋

    //武器
    Pistol, //手枪

    //子弹
    Bullet_9mm, //9毫米子弹



}