import cv2
import time
import serial
import gc
import sys

def send_uart_command(command, port="COM17", baudrate=38400, delay=1.5):
    """發送單個 UART 命令到 MCU"""
    try:
        with serial.Serial(port, baudrate, timeout=1) as ser:
            ser.write(command.encode('utf-8'))
            time.sleep(delay)
    except Exception as e:
        print(f"UART 傳送失敗: {e}", file=sys.stderr)

def calculate_focus_score(image):
    """使用 Laplacian 變異數計算影像清晰度"""
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    laplacian = cv2.Laplacian(gray, cv2.CV_64F)
    return laplacian.var()

def auto_focus_uart_headless():
    port = "COM17"
    baudrate = 38400
    delay = 1.5
    steps = 10

    cap = cv2.VideoCapture(1)
    if not cap.isOpened():
        print("無法開啟攝影機", file=sys.stderr)
        sys.exit(1)

    try:
        for _ in range(steps):
            send_uart_command("U", port, baudrate, delay)

        focus_scores = []
        images = []

        for i in range(2 * steps + 1):
            ret, frame = cap.read()
            if not ret:
                print("拍照失敗", file=sys.stderr)
                sys.exit(1)

            score = calculate_focus_score(frame)
            focus_scores.append(score)
            images.append(frame)

            if i < 2 * steps:
                send_uart_command("V", port, baudrate, delay)

        best_index = focus_scores.index(max(focus_scores))
        move_back_steps = (2 * steps) - best_index
        for _ in range(move_back_steps):
            send_uart_command("U", port, baudrate, delay)

        print("FOK")  #  給 C# 端識別成功
        sys.exit(0)

    except Exception as e:
        print(f" 發生例外錯誤: {e}", file=sys.stderr)
        sys.exit(1)

    finally:
        cap.release()
        gc.collect()
        time.sleep(1.0)  #  給 C# 端保險時間確保釋放 COM

if __name__ == "__main__":
    auto_focus_uart_headless()
