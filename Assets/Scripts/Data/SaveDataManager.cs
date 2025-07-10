using System;
using System.IO;
using DungeonMaster.Localization;
using DungeonMaster.Utility;
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
                    GameLogger.LogError(LocalizationManager.Instance.GetText("error_no_data_to_save"));
                    return false;
                }

                // 마지막 접속 시간 업데이트
                userData.LastAccessTime = DateTime.Now;

                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(userData, true);
                
                // 파일로 저장
                File.WriteAllText(SaveFilePath, jsonData);
                
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_user_data_saved", SaveFilePath));
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_user_data_save_failed", ex.Message));
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
                    
                    GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_user_data_loaded", SaveFilePath));
                    return userData;
                }
                else
                {
                    // 새로운 유저 데이터 생성
                    GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_new_user_data_created"));
                    return new UserData();
                }
            }
            catch (Exception ex)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_user_data_load_failed", ex.Message));
                
                // 실패 시 새로운 데이터 생성
                GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_creating_new_data_due_to_load_fail"));
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
                    GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_save_file_deleted"));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_save_file_delete_failed", ex.Message));
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
                
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("info_backup_created", backupPath));
                return true;
            }
            catch (Exception ex)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_backup_create_failed", ex.Message));
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
                    
                    GameLogger.LogInfo(LocalizationManager.Instance.GetText("info_restored_from_backup"));
                    return userData;
                }
                
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("warn_backup_file_not_exist"));
                return null;
            }
            catch (Exception ex)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("error_restore_from_backup_failed", ex.Message));
                return null;
            }
        }
    }
} 