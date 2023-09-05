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

        // 원본 JSON을 JObject로 파싱
        JObject originalJObject = JObject.Parse(originalJson);

        // 새로운 JSON 구조를 저장할 JObject
        JObject newJObject = new JObject();
        newJObject["Dic"] = new JObject();

        // 원본 JSON 데이터를 새로운 형태로 변환
        foreach (var pair in originalJObject["Dic"].ToObject<Dictionary<string, string>>())
        {
            JObject newEntry = new JObject();
            newEntry["path"] = pair.Value;
            newEntry["type"] = "minion";  // 실제 타입 설정

            newJObject["Dic"][pair.Key] = newEntry;
        }

        // 변환된 JObject를 JSON 문자열로 변환
        string newJson = newJObject.ToString();
        string json2 = JsonConvert.SerializeObject(newJObject, Formatting.Indented
         , new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        File.WriteAllText("Assets/Resources/test.json", json2);
        Debug.Log("Converted JSON: " + newJson);
    }
}