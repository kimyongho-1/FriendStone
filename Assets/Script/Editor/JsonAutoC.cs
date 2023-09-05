using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonAutoC : MonoBehaviour
{
    public TextAsset json;
    private void Awake()
    {
        ConvertJson();
    }
    public void ConvertJson()
    {
        string originalJson = json.ToString();

        // ���� JSON�� JObject�� �Ľ�
        JObject originalJObject = JObject.Parse(originalJson);

        // ���ο� JSON ������ ������ JObject
        JObject newJObject = new JObject();
        newJObject["Dic"] = new JObject();

        // ���� JSON �����͸� ���ο� ���·� ��ȯ
        foreach (var pair in originalJObject["Dic"].ToObject<Dictionary<string, string>>())
        {
            JObject newEntry = new JObject();
            newEntry["path"] = pair.Value;
            newEntry["type"] = "minion";  // ���� Ÿ�� ����

            newJObject["Dic"][pair.Key] = newEntry;
        }

        // ��ȯ�� JObject�� JSON ���ڿ��� ��ȯ
        string newJson = newJObject.ToString();
        string json2 = JsonConvert.SerializeObject(newJObject, Formatting.Indented
         , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        File.WriteAllText("Assets/Resources/test.json", json2);
        Debug.Log("Converted JSON: " + newJson);
    }
}