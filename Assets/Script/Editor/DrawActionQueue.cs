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
        // �⺻ �ν����� �ڿ� �׸���
        base.OnInspectorGUI(); 

        BattleManager bm = (BattleManager)target;

        exCount = bm.ActionQueue.Count;
       
        EditorGUILayout.LabelField("Action Queue Count: " + bm.ActionQueue.Count);

        // ���� ���� �ڷ�ƾ
        EditorGUILayout.LabelField("���� : " + ((bm.currCo != null) ? bm.currCo.ToString() : "����"));

        if (bm.ActionQueue.Count > 0)
        {
            // List�� �ٲ㼭 ǥ���ϱ�
            List<IEnumerator> list = bm.ActionQueue.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.LabelField($"{i + 1}��° : {list[i]?.ToString() ?? "null"}");
            }
        }
        
    }
}
