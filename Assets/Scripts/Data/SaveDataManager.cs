using System;
using System.IO;
using UnityEngine;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 저장 데이터 관리자 - 파일 저장/로드 전담
    /// </summary>
    public static class SaveDataManager
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "userdata.json");
        
        /// <summary>
        /// 유저 데이터 저장
        /// </summary>
        public static bool SaveUserData(UserData userData)
        {
            try
            {
                if (userData == null)
                {
                    Debug.LogError("저장할 유저 데이터가 없습니다!");
                    return false;
                }

                // 마지막 접속 시간 업데이트
                userData.LastAccessTime = DateTime.Now;

                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(userData, true);
                
                // 파일로 저장
                File.WriteAllText(SaveFilePath, jsonData);
                
                Debug.Log($"유저 데이터 저장 완료: {SaveFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"유저 데이터 저장 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 유저 데이터 로드
        /// </summary>
        public static UserData LoadUserData()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    // 파일에서 로드
                    string jsonData = File.ReadAllText(SaveFilePath);
                    var userData = JsonUtility.FromJson<UserData>(jsonData);
                    
                    // 마지막 접속 시간 업데이트
                    userData.LastAccessTime = DateTime.Now;
                    
                    Debug.Log($"유저 데이터 로드 완료: {SaveFilePath}");
                    return userData;
                }
                else
                {
                    // 새로운 유저 데이터 생성
                    Debug.Log("저장 파일이 없어 새로운 유저 데이터 생성");
                    return new UserData();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"유저 데이터 로드 실패: {ex.Message}");
                
                // 실패 시 새로운 데이터 생성
                Debug.Log("유저 데이터 로드 실패로 새로운 데이터 생성");
                return new UserData();
            }
        }
        
        /// <summary>
        /// 저장 파일 존재 여부 확인
        /// </summary>
        public static bool SaveFileExists()
        {
            return File.Exists(SaveFilePath);
        }
        
        /// <summary>
        /// 저장 파일 삭제
        /// </summary>
        public static bool DeleteSaveFile()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("저장 파일 삭제 완료");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"저장 파일 삭제 실패: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 백업 저장
        /// </summary>
        public static bool CreateBackup(UserData userData)
        {
            try
            {
                string backupPath = SaveFilePath + ".backup";
                string jsonData = JsonUtility.ToJson(userData, true);
                File.WriteAllText(backupPath, jsonData);
                
                Debug.Log($"백업 생성 완료: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"백업 생성 실패: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 백업에서 복원
        /// </summary>
        public static UserData RestoreFromBackup()
        {
            try
            {
                string backupPath = SaveFilePath + ".backup";
                if (File.Exists(backupPath))
                {
                    string jsonData = File.ReadAllText(backupPath);
                    var userData = JsonUtility.FromJson<UserData>(jsonData);
                    
                    Debug.Log("백업에서 복원 완료");
                    return userData;
                }
                
                Debug.LogWarning("백업 파일이 존재하지 않습니다.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"백업 복원 실패: {ex.Message}");
                return null;
            }
        }
    }
} 