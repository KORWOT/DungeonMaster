import pandas as pd
import sys
import os

def read_excel_file(file_path):
    """엑셀 파일을 읽어서 내용을 출력합니다."""
    try:
        # 엑셀 파일 읽기
        df = pd.read_excel(file_path)
        
        print("=== 엑셀 파일 내용 ===")
        print(df.to_string(index=False))
        
        # 각 셀의 값과 타입 확인
        print("\n=== 셀별 상세 정보 ===")
        for i, row in df.iterrows():
            for j, value in enumerate(row):
                print(f"행 {i+1}, 열 {j+1}: {value} (타입: {type(value)})")
                
    except Exception as e:
        print(f"파일 읽기 오류: {e}")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        file_path = sys.argv[1]
        if os.path.exists(file_path):
            read_excel_file(file_path)
        else:
            print(f"파일을 찾을 수 없습니다: {file_path}")
    else:
        print("사용법: python excel_reader.py <엑셀파일경로>") 