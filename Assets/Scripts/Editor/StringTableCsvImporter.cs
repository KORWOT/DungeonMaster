using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using DungeonMaster.Localization;
using System;
using System.Linq;

public class StringTableCsvImporter
{
    // CSV 파일이 위치한 기본 경로
    private const string DefaultCsvPath = "Assets/LocalizationData/InitialKeys.csv";
    
    // StringTable 에셋을 찾기 위한 기본 검색 경로
    private const string DefaultStringTableSearchPath = "Assets/Resources";

    [MenuItem("Tools/Localization/Import Keys from CSV")]
    public static void ShowImportWindow()
    {
        // 간단한 확인 창을 띄워 사용자에게 실행 의사를 묻습니다.
        if (EditorUtility.DisplayDialog(
            "CSV에서 키 가져오기",
            $"'{DefaultCsvPath}' 파일에서 키를 가져와 StringTable에 추가하시겠습니까?\n\n" +
            "기존에 있는 키는 건너뜁니다.",
            "가져오기", 
            "취소"))
        {
            ImportKeys();
        }
    }

    private static void ImportKeys()
    {
        // 1. StringTable 에셋 찾기
        var stringTable = FindMainStringTable();
        if (stringTable == null)
        {
            Debug.LogError($"[StringTableCsvImporter] StringTable 에셋을 찾을 수 없습니다. '{DefaultStringTableSearchPath}' 경로를 확인해주세요.");
            return;
        }

        // 2. CSV 파일 읽기
        if (!File.Exists(DefaultCsvPath))
        {
            Debug.LogError($"[StringTableCsvImporter] CSV 파일을 찾을 수 없습니다: {DefaultCsvPath}");
            return;
        }
        var csvLines = File.ReadAllLines(DefaultCsvPath);
        if (csvLines.Length <= 1)
        {
            Debug.LogWarning("[StringTableCsvImporter] CSV 파일이 비어있거나 헤더만 존재합니다.");
            return;
        }

        // 3. CSV 데이터 파싱 및 키 추가
        ProcessCsv(csvLines, stringTable);
    }

    private static StringTable FindMainStringTable()
    {
        // 지정된 경로에서 StringTable 타입의 에셋을 찾습니다.
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(StringTable).Name}", new[] { DefaultStringTableSearchPath });
        if (guids.Length == 0)
        {
            return null;
        }
        
        // 여러 개가 있다면 첫 번째 것을 사용합니다.
        // (프로젝트 규칙에 따라 MainStringTable을 특정하는 더 좋은 방법이 있을 수 있음)
        if (guids.Length > 1)
        {
            Debug.LogWarning($"[StringTableCsvImporter] 여러 개의 StringTable 에셋을 찾았습니다. 첫 번째 에셋 '{AssetDatabase.GUIDToAssetPath(guids[0])}'을 사용합니다.");
        }
        
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<StringTable>(path);
    }

    private static void ProcessCsv(string[] lines, StringTable table)
    {
        // CSV 헤더 파싱
        var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
        var keyIndex = headers.IndexOf("Key");

        if (keyIndex == -1)
        {
            Debug.LogError("[StringTableCsvImporter] CSV 파일에 'Key' 헤더가 없습니다.");
            return;
        }
        
        // 언어 헤더 매핑
        var languageHeaderMap = new Dictionary<string, SupportedLanguage>();
        foreach (var lang in (SupportedLanguage[])Enum.GetValues(typeof(SupportedLanguage)))
        {
            if (headers.Contains(lang.ToString()))
            {
                languageHeaderMap[lang.ToString()] = lang;
            }
        }

        // 성능 최적화: 기존 키를 HashSet에 저장하여 O(1) 조회를 위함
        var existingKeys = new HashSet<string>(table.Entries.Select(e => e.Key));
        var newEntries = new List<LocalizedEntry>();
        int skippedCount = 0;

        // 각 라인 처리 (헤더 제외)
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            if (values.Length <= keyIndex) continue; // 데이터가 없는 라인 건너뛰기
            
            string key = values[keyIndex].Trim();

            if (string.IsNullOrEmpty(key)) continue;

            // 이미 키가 존재하는지 확인 (HashSet 사용)
            if (existingKeys.Contains(key))
            {
                skippedCount++;
                continue;
            }

            // 새 LocalizedEntry 생성
            var newEntry = new LocalizedEntry(key);
            
            // 각 언어별 텍스트 설정
            foreach (var langHeader in languageHeaderMap)
            {
                var langIndex = headers.IndexOf(langHeader.Key);
                if (langIndex != -1 && langIndex < values.Length)
                {
                    var text = values[langIndex].Trim().Replace("\"", ""); // 따옴표 제거
                    newEntry.SetText(langHeader.Value, text);
                }
            }

            newEntries.Add(newEntry);
            existingKeys.Add(key); // CSV 내 중복 처리를 위해 추가
        }

        // 변경사항 저장
        if (newEntries.Count > 0)
        {
            Undo.RecordObject(table, "Import Keys from CSV");
            
            table.AddEntries(newEntries);
            
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            Debug.Log($"[StringTableCsvImporter] 임포트 완료! 추가된 키: {newEntries.Count}, 건너뛴 키: {skippedCount}");
        }
        else
        {
            Debug.Log($"[StringTableCsvImporter] 새로운 키가 없습니다. 건너뛴 키: {skippedCount}");
        }
    }
} 