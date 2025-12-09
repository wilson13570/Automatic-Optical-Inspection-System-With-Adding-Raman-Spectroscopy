import cv2
import numpy as np
import serial
import time
import os
from datetime import datetime
from pathlib import Path
import gc
import socket

last_sent_progress_char = None  # 用來避免重複送出相同進度字元


def send_uart(ser, command="C"):
    try:
        ser.write(command.encode())
        print(f"已送出 UART 指令：{command}")
        time.sleep(0.3)
        return True
    except Exception as e:
        print(f"UART 傳送失敗：{e}")
        return False


import socket  # 確保有引入

def send_progress_to_csharp(letter, host='127.0.0.1', port=5001):
    try:
        msg = f"PROGRESS:{letter}\n"
        with socket.create_connection((host, port), timeout=2) as sock:
            sock.sendall(msg.encode('utf-8'))      # 傳送完整資料
            sock.shutdown(socket.SHUT_WR)          # 確保寫入完成才關閉連線
            print(f"傳送進度給C#: {msg.strip()}")
    except Exception as e:
        print(f"無法傳送進度給C#：{e}")

def send_progress_to_mcu(letter, ser=None):
    global last_sent_progress_char
    if letter == last_sent_progress_char:
        return True  # 不重送相同字元
    last_sent_progress_char = letter

    try:
        if ser and ser.is_open:
            ser.write(letter.encode('utf-8'))
            print(f"傳送進度給MCU: {letter}")
            time.sleep(0.2)
            return True
        else:
            print("MCU 串口尚未開啟")
            return False
    except Exception as e:
        print(f"傳送進度給MCU失敗：{e}")
        return False




def detect_orange_circle(frame, frame_center_x=240, frame_center_y=240, draw=False):
    """使用 HoughCircles 偵測銀色圓點（位於橘色遮罩區域）"""
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # 與原邏輯相同的橘色 HSV 範圍
    lower_orange = np.array([0, 0, 40])
    upper_orange = np.array([179, 114, 255])
    mask = cv2.inRange(hsv, lower_orange, upper_orange)

    # 消除雜訊
    kernel = np.ones((5, 5), np.uint8)
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)

    # 模糊化以利偵測圓形
    mask_blur = cv2.GaussianBlur(mask, (9, 9), 2)

    # 偵測圓形：目標半徑約 38px，設定容忍範圍 30~45
    circles = cv2.HoughCircles(
        mask_blur,
        cv2.HOUGH_GRADIENT,
        dp=1.2,
        minDist=60,
        param1=100,
        param2=30,
        minRadius=30,
        maxRadius=45
    )

    best_score = 0
    best_circle = None

    if circles is not None:
        circles = np.uint16(np.around(circles))
        for (x, y, r) in circles[0, :]:
            # 中心距離分數（越靠近畫面中心越好）
            dist = np.sqrt((x - frame_center_x)**2 + (y - frame_center_y)**2)
            distance_score = 1.0 / (1.0 + dist / 200)

            # 半徑越接近 38px 給予越高分數
            radius_score = 1.0 / (1.0 + abs(r - 38) / 10)

            score = distance_score * radius_score * r

            if score > best_score:
                best_score = score
                best_circle = (x, y, r)

    if best_circle and best_score > 20:
        x, y, r = best_circle
        print(f"偵測到銀色圓球：中心=({x},{y}) 半徑={r} 分數={best_score:.1f}")
        if draw:
            cv2.circle(frame, (x, y), r, (0, 255, 0), 2)
            cv2.circle(frame, (x, y), 2, (0, 0, 255), 3)
            cv2.line(frame, (x, y), (frame_center_x, frame_center_y), (255, 0, 255), 1)
        return x, y
    else:
        print("未偵測到銀色圓球")
        return None

def move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25):
    """移動裝置使LED居中，返回是否已經居中（一律使用小步）"""
    x_diff = cx - frame_center_x
    y_diff = cy - frame_center_y

    if abs(y_diff) > tolerance:
        if y_diff > 0:
            cmd = 'b'
            print(f"垂直調整: LED在下方，需要向上移動 {abs(y_diff)} 像素")
        else:
            cmd = 'a'
            print(f"垂直調整: LED在上方，需要向下移動 {abs(y_diff)} 像素")
        send_uart(ser, command=cmd)
        return False

    if abs(x_diff) > tolerance:
        if x_diff < 0:
            cmd = 'd'
            print(f"水平調整: 銀框在左側，需要向右移動 {abs(x_diff)} 像素")
        else:
            cmd = 'c'
            print(f"水平調整: 銀框在右側，需要向左移動 {abs(x_diff)} 像素")
        send_uart(ser, command=cmd)
        return False

    print(f"LED已居中！偏差: X={x_diff}, Y={y_diff}")
    return True

def generate_desktop_folder(columns, rows):
    desktop = Path.home() / "Desktop"
    timestamp = datetime.now().strftime("%Y-%m-%d-%H-%M-%S")
    folder_name = f"{timestamp}_{columns}x{rows}"
    full_path = desktop / "SampleData" / folder_name
    full_path.mkdir(parents=True, exist_ok=True)
    return str(full_path)


def auto_scan(columns=3, rows=3, uart_port="COM17", uart_baud=38400):

    print(f"AutoScan 開始：{columns} x {rows}")
    
    # 將原本 main() 的內容整合進來
    #  替換原本 main 裡面的 columns, rows 設定
    #  其餘邏輯不變
        # === 建立 UART 實體 ===
    try:
        ser = serial.Serial(uart_port, uart_baud, timeout=1)
        print(f"UART 開啟成功 ({uart_port})")
        time.sleep(0.5)
    except Exception as e:
        print(f"UART 開啟失敗：{e}")
        return False

    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("無法打開攝影機")
        return False  #  明確回傳失敗
    ret, frame = cap.read()

    try:
        if not ret:
            print("無法讀取攝影機畫面")
            cap.release()
            cv2.destroyAllWindows()
            return False


        cap.set(cv2.CAP_PROP_BRIGHTNESS, 80)
        cap.set(cv2.CAP_PROP_CONTRAST, 80)
        cap.set(cv2.CAP_PROP_SATURATION, 100)
        cap.set(cv2.CAP_PROP_GAIN, 0)
        cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 0.25)
        cap.set(cv2.CAP_PROP_EXPOSURE, -6)
        cap.set(cv2.CAP_PROP_SHARPNESS, 100)

        current_row = 0
        current_col = 0
        direction = 1
        state = 'init'

        save_dir = generate_desktop_folder(columns, rows)
        os.makedirs(save_dir, exist_ok=True)

        print("銀色圓球 Z字形掃描模式啟動")

        #cv2.namedWindow("Camera", cv2.WINDOW_NORMAL)
        #cv2.resizeWindow("Camera", 800, 600)

        frame_center_x = 240
        frame_center_y = 240

        last_move_time = time.time()
        search_timeout = 2.0
        retry_count = 0
        max_retry = 3
        move_count = 0
        captured_photos = []
        last_capture_time = 0
        stable_count = 0
        required_stable_frames = 8
        last_positions = []
        max_positions = 5

        while True:
            ret, frame = cap.read()
            if not ret:
                print("自動掃描未完成，中途被中止")
                return False


            if current_row >= rows:
                print("已完成所有拍攝！共計{}張照片".format(len(captured_photos)))
                break

            #status_text = f"位置: 第{current_row+1}行第{current_col+1}列 方向:{'→' if direction==1 else '←'} 狀態:{state}"
            #cv2.putText(frame, status_text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 200, 255), 2)

            result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)

            if state == 'init':
                print("初始化：開始向右移動")
                send_uart(ser, command='C')
                time.sleep(0.5)
                last_move_time = time.time()
                state = 'searching'
                move_count = 1

            elif state == 'searching':
                if result:
                    cx, cy = result
                    last_positions.append((cx, cy))
                    if len(last_positions) > max_positions:
                        last_positions.pop(0)
                    retry_count = 0
                    move_count = 0
                    state = 'centering'
                    print("找到銀球，開始中心對準")
                elif time.time() - last_move_time > search_timeout:
                    retry_count += 1
                    if retry_count <= max_retry:
                        cmd = 'C' if direction == 1 else 'D'
                        print(f"搜尋超時，繼續移動 (第{retry_count}次)")
                        send_uart(ser, command=cmd)
                        move_count += 1
                        last_move_time = time.time()
                        if move_count > 2:
                            print("嘗試垂直調整")
                            if retry_count % 3 == 0:
                                send_uart(ser, command='B')
                            elif retry_count % 3 == 1:
                                send_uart(ser, command='A')
                            else:
                                send_uart(ser, command='B')
                                time.sleep(0.2)
                                send_uart(ser, command='B')
                            time.sleep(0.5)
                            move_count = 0
                    else:
                        print("多次搜尋失敗，切換位置")
                        if current_col == 0 and direction == 1:
                            send_uart(ser, command='C')
                            send_uart(ser, command='C')
                        elif current_col == columns-1 and direction == -1:
                            send_uart(ser, command='D')
                            send_uart(ser, command='D')
                        else:
                            current_col += direction
                            if current_col >= columns or current_col < 0:
                                current_row += 1
                                if current_col >= columns:
                                    current_col = columns - 1
                                    direction = -1
                                else:
                                    current_col = 0
                                    direction = 1
                                if current_row < rows:
                                    print(f"換到下一行: 第{current_row+1}行")
                                    send_uart(ser, command='B')
                                    time.sleep(1.0)
                                    send_uart(ser, command='B')
                                    time.sleep(1.0)
                                    send_uart(ser, command='b')  # 新增小步補償
                                    time.sleep(1.0)
                                    cmd = 'D' if direction == 1 else 'C'
                                    send_uart(ser, command=cmd)
                            retry_count = 0
                            move_count = 0
                        last_move_time = time.time()

            elif state == 'centering':
                if result:
                    cx, cy = result
                    last_positions.append((cx, cy))
                    if len(last_positions) > max_positions:
                        last_positions.pop(0)
                    if move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25):
                        if len(last_positions) >= 3:
                            x_positions = [x for x, y in last_positions]
                            y_positions = [y for x, y in last_positions]
                            x_range = max(x_positions) - min(x_positions)
                            y_range = max(y_positions) - min(y_positions)
                            if x_range < 20 and y_range < 20:
                                stable_count += 1
                                print(f"LED位置穩定中... ({stable_count}/{required_stable_frames})")
                            else:
                                print(f"不夠穩定，X變動: {x_range}, Y變動: {y_range}")
                                stable_count = 0
                        else:
                            stable_count += 1
                            print(f"蒐集資料中... ({len(last_positions)}/{max_positions})")
                        if stable_count >= required_stable_frames:
                            current_time = time.time()
                            if current_time - last_capture_time > 1.0:
                                print(f"拍攝 第{current_row+1}行 第{current_col+1}列")
                                filename = f"{save_dir}/r{current_row}_c{current_col}.jpg"
                                cv2.imwrite(filename, frame)
                                captured_photos.append(filename)
                                last_capture_time = current_time
                                stable_count = 0
                                last_positions = []
                                state = 'moving'

                                # 傳送進度更新
                                total_photos = rows * columns
                                progress_percent = int((len(captured_photos) / total_photos) * 100)
                                progress_percent = (progress_percent // 10) * 10  # 只取10的倍數

                                # 對應字元 E:0%, F:10%, ..., O:100%
                                progress_char = chr(ord('E') + (progress_percent // 10))
                                send_progress_to_csharp(progress_char)
                                send_progress_to_mcu(progress_char, ser=ser)


                    else:
                        stable_count = 0
                else:
                    print("對準過程中丟失LED，恢復搜尋")
                    stable_count = 0
                    last_positions = []
                    state = 'searching'
                    last_move_time = time.time()

            elif state == 'moving':
                current_col += direction
                if current_col >= columns or current_col < 0:
                    current_row += 1
                    if current_col >= columns:
                        current_col = columns - 1
                        direction = -1
                    else:
                        current_col = 0
                        direction = 1
                    if current_row < rows:
                        print(f"換到下一行: 第{current_row+1}行")
                        send_uart(ser, command='B')
                        time.sleep(1.0)
                        send_uart(ser, command='B')
                        time.sleep(1.0)
                        send_uart(ser, command='b')  #  小步補償
                        time.sleep(1.0)
                        cmd = 'D' if direction == 1 else 'C'
                        send_uart(ser, command=cmd)
                        stable_count = 0
                        last_positions = []
                        state = 'searching'
                    else:
                        print("所有行掃描完成")
                else:
                    cmd = 'C' if direction == 1 else 'D'
                    print(f"移動到: 第{current_row+1}行 第{current_col+1}列")
                    send_uart(ser, command=cmd)
                    time.sleep(0.7)
                    send_uart(ser, command=cmd)
                    time.sleep(0.7)
                    stable_count = 0
                    last_positions = []
                    state = 'searching'
                move_count = 0
                retry_count = 0
                last_move_time = time.time()

            h, w = frame.shape[:2]
            # cv2.line(frame, (0, frame_center_y), (w, frame_center_y), (255, 0, 0), 1)
            # cv2.line(frame, (frame_center_x, 0), (frame_center_x, h), (0, 255, 255), 1)
            # stability_text = f"穩定度: {stable_count}/{required_stable_frames}"
            # cv2.putText(frame, stability_text, (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 255), 2)
            # progress_text = f"進度: {len(captured_photos)}/{rows*columns} 完成"
            # cv2.putText(frame, progress_text, (10, h-20), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
            #cv2.imshow("Camera", frame)

            #if cv2.getWindowProperty("Camera", cv2.WND_PROP_VISIBLE) < 1:
                #print("使用者關閉了視窗，終止掃描")
                #cap.release()
                #cv2.destroyAllWindows()
                #return False

            #if cv2.waitKey(1) & 0xFF == ord('q'):
                #print("自動掃描未完成，中途被中止")
                #break
            #if cv2.getWindowProperty("Camera", cv2.WND_PROP_VISIBLE) < 1:
                #print("使用者關閉了視窗，終止掃描")
                #break

        #cap.release()
        #cv2.destroyAllWindows()
        #gc.collect()
        #time.sleep(1.0)
        print("Python 自動掃描結束")
        return len(captured_photos) == (columns * rows)  # 若拍照數量不足則 False
    
    finally:
        if cap.isOpened():
            cap.release()
        cv2.destroyAllWindows()
        gc.collect()
        time.sleep(1.0)
        print("資源清理完畢")
# 注意：不再用 main()
