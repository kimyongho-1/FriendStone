using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(BattleManager))]  
public class DrawActionQueue : Editor
{
    int exCount = 0;
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 뒤에 그리기
        base.OnInspectorGUI(); 

        BattleManager bm = (BattleManager)target;

        exCount = bm.ActionQueue.Count;
       
        EditorGUILayout.LabelField("Action Queue Count: " + bm.ActionQueue.Count);

        // 현재 진행 코루틴
        EditorGUILayout.LabelField("현재 : " + ((bm.currCo != null) ? bm.currCo.ToString() : "없음"));

        if (bm.ActionQueue.Count > 0)
        {
            // List로 바꿔서 표시하기
            List<IEnumerator> list = bm.ActionQueue.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.LabelField($"{i + 1}번째 : {list[i]?.ToString() ?? "null"}");
            }
        }
        
    }
}
