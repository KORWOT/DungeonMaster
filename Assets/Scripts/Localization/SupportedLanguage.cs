namespace DungeonMaster.Localization
{
    /// <summary>
    /// 게임에서 지원하는 언어 목록
    /// 값은 명시적으로 할당하여 서버-클라이언트 호환성 보장
    /// </summary>
    public enum SupportedLanguage
    {
        Korean = 0,      // 기본 언어 (한국어)
        English = 1,     // 영어
        Japanese = 2,    // 일본어
        Chinese = 3      // 중국어 (간체)
    }
} 