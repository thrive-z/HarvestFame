using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

/// <summary>
/// 加载数据管理器
/// </summary>
public class LoadManager : MonoBehaviour
{
    public static LoadManager instance;

    public List<ItemData>itemDatas = new (); //物品json的数据
    public Dictionary<int, ItemData> itemDatasDict = new();//所有物品数据的字典

    public Dictionary<string, Sprite> spriteDict = new(); //所有图像的字典


    public Dictionary<int, string> prefab_mapping = new(); //模型对应的id字典
    public Dictionary<int, GameObject> prefab_model = new(); //世界中的模型字典

    private void Awake()
    {
        instance = this;

        //加载itemdata数据
        TextAsset ta = Resources.Load<TextAsset>("Data/ItemData");
        itemDatas = JsonConvert.DeserializeObject<List<ItemData>>(ta.text);

        //加载所有图片
        LoadAltas();

        //注入图片数据
        foreach (var itemData in itemDatas)
        {
            //
            if (spriteDict.ContainsKey(itemData.icon_name))
            {
                itemData.icon = spriteDict[itemData.icon_name];
            }
             itemDatasDict.Add(itemData.id, itemData);//添加到字典中
        }

        //获取mapping
        TextAsset mapping_prefab = Resources.Load<TextAsset>("Data/PrefabMapping");
        prefab_mapping = JsonConvert.DeserializeObject<Dictionary<int, string>>(mapping_prefab.text);
        //读取模型对象
        foreach (var KeyValuePair in prefab_mapping)
        {
            GameObject go = Resources.Load<GameObject>(KeyValuePair.Value);
            prefab_model.Add(KeyValuePair.Key, go);
        }

    }

    //加载所有图片
    private void LoadAltas()
    {
        Sprite[] item = Resources.LoadAll<Sprite>("Image/UI/springobjects");
        foreach (var sprite in item)
        {
            //添加到图片字典
            spriteDict.Add(sprite.name, sprite);
        }

        Sprite[] item2 = Resources.LoadAll<Sprite>("Image/UI/tools");
        foreach (var sprite in item2)
        {
            //添加到图片字典
            spriteDict.Add(sprite.name, sprite);
        }

        foreach (var sprite in Resources.LoadAll<Sprite>("Image/UI/weapons"))
            spriteDict.Add(sprite.name, sprite);

        Sprite[] item3 = Resources.LoadAll<Sprite>("Image/Other");
        foreach (var sprite in item3)
        {
            //添加到图片字典
            spriteDict.Add(sprite.name, sprite);
        }

        Debug.Log("加载图片" + spriteDict.Count);


    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
     
    //获取预制体
    public GameObject GetPrefab(int id)
    {
        //判断字典中是否有该id
        if (!prefab_model.ContainsKey(id))
        {
            Debug.Log("没有该预制体");
            return null;
        }

        return prefab_model[id];

    }

}
