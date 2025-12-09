import cv2
import numpy as np
import serial
import time

# UART 傳送指令
def send_uart(ser, command="C", delay=2):  # 預設延遲 0.8 秒
    try:
        ser.write(command.encode())
        time.sleep(delay)  # 這裡改長一點
        return True
    except Exception as e:
        return False


# 偵測橘色遮罩中的銀色圓點 (LED)
def detect_orange_circle(frame, frame_center_x=240, frame_center_y=240, draw=False):
    """使用 HoughCircles 偵測銀色圓點（位於橘色遮罩區域）"""
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # 定義橘色 HSV 範圍 (與 cv_cl.py 相同)
    lower_orange = np.array([0, 0, 40])
    upper_orange = np.array([179, 49, 255])
    mask = cv2.inRange(hsv, lower_orange, upper_orange)

    # 消除雜訊
    kernel = np.ones((5, 5), np.uint8)
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)
 
    # 模糊處理以利圓形偵測
    mask_blur = cv2.GaussianBlur(mask, (9,9), 2)

    # 使用 Hough 變換偵測圓形 (半徑約 38px，允許範圍 30~45)
    circles = cv2.HoughCircles(
        mask_blur,
        cv2.HOUGH_GRADIENT,
        dp=1.2,
        minDist=60,
        param1=100,
        param2=30,
        minRadius=30,
        maxRadius=55
    )

    best_score = 0
    best_circle = None

    if circles is not None:
        circles = np.uint16(np.around(circles))
        for (x, y, r) in circles[0, :]:
            # 距離畫面中心越近分數越高
            dist = np.sqrt((x - frame_center_x)**2 + (y - frame_center_y)**2)
            distance_score = 1.0 / (1.0 + dist /100)         # 放寬距離懲罰
            radius_score = 1.0 / (1.0 + abs(r - 38) / 10)     # 放寬半徑懲罰
            score = distance_score * radius_score * r
            if score > best_score:
                best_score = score
                best_circle = (x, y, r)

    if best_circle and best_score > 5:
        x, y, r = best_circle
        if draw:
            # 標記偵測到的圓球於影像上 (如需觀察)
            cv2.circle(frame, (x, y), r, (0, 255, 0), 2)
            cv2.circle(frame, (x, y), 2, (0, 0, 255), 3)
            cv2.line(frame, (x, y), (frame_center_x, frame_center_y), (255, 0, 255), 1)
        return x, y
    else:
        return None

# 移動裝置使 LED 居中，返回是否已經居中（始終使用小步移動）
def move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25):
    """移動裝置使LED居中，返回 True/False 表示是否已居中"""
    x_diff = cx - frame_center_x
    y_diff = cy - frame_center_y

    if abs(y_diff) > tolerance:
        if y_diff > 0:
            cmd = 'b'
        else:
            cmd = 'a'
        send_uart(ser, command=cmd)
        return False

    if abs(x_diff) > tolerance:
        if x_diff < 0:
            cmd = 'd'
        else:
            cmd = 'c'
        send_uart(ser, command=cmd)
        return False

    return True

if __name__ == "__main__":
    # === 建立 UART 與攝影機 ===
    uart_port = "COM17"
    uart_baud = 38400
    try:
        ser = serial.Serial(uart_port, uart_baud, timeout=1)
        print(f" UART 開啟成功 ({uart_port})")
        time.sleep(0.5)
    except Exception as e:
        print(f" UART 開啟失敗：{e}")
        exit(1)

    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        print("無法開啟攝影機")
        exit(1)
    ret, frame = cap.read()
    if not ret:
        print("無法讀取攝影機影像")
        cap.release()
        exit(1)

    # 設定攝影機參數 (與 cv_cl.py 保持一致)
    cap.set(cv2.CAP_PROP_BRIGHTNESS, 80)
    cap.set(cv2.CAP_PROP_CONTRAST, 80)
    cap.set(cv2.CAP_PROP_SATURATION, 100)
    cap.set(cv2.CAP_PROP_GAIN, 0)
    cap.set(cv2.CAP_PROP_AUTO_EXPOSURE, 0.25)
    cap.set(cv2.CAP_PROP_EXPOSURE, -6)
    cap.set(cv2.CAP_PROP_SHARPNESS, 100)

    frame_center_x = 240
    frame_center_y = 240

    # === 隨機移動搜尋 LED ===

    led_found = False
    move_attempts = 0
    # 先左右來回移動幾次，若找不到再嘗試垂直調整
    while not led_found:
        # 每次迴圈決定移動方向 (先左右交替)
        if move_attempts % 2 == 0:
            cmd = 'D'  # 向左移動
            print("隨機移動：向左一步")
        else:
            cmd = 'C'  # 向右移動
            print("隨機移動：向右一步")
        send_uart(ser, command=cmd)
        # 短暫延遲後抓取畫面進行偵測
        time.sleep(0.5)
        ret, frame = cap.read()
        if not ret:
            print("無法取得影像，跳過此次偵測")
        else:
            result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
            if result:
                led_found = True
                print("已偵測到 LED，開始主邏輯定位")
                # 紀錄目前 LED 中心位置，用於後續居中
                cx, cy = result
                # (不立即 break; 會在下方跳出迴圈)
        move_attempts += 1
        # 每移動 6 次尚未找到，嘗試改變垂直方向 (下 -> 上 -> 下)
        if not led_found and move_attempts == 6:
            # 嘗試向下微調
            print("尚未找到 LED，嘗試向下移動一些")
            send_uart(ser, command='B')
            time.sleep(0.5)
        if not led_found and move_attempts == 12:
            # 嘗試向上移動一些
            print("尚未找到 LED，嘗試向上移動一些")
            send_uart(ser, command='A')
            time.sleep(0.5)
        if move_attempts > 18:
            # 經多次嘗試仍未找到 LED，退出
            print("多次隨機搜尋仍未偵測到 LED，結束程式")
            cap.release()
            ser.close()
            exit(1)
        if led_found:
            break

    # === 開始往左移動至左上角 (先水平往左) ===
    # 確保攝影機視野中有 LED （led_found True 表示目前視野內有一顆 LED）
    not_found_count = 0
    move_count = 0
    print("開始向左移動，搜尋左邊界...")
    while True:
        # 發送向左的大步移動指令
        send_uart(ser, command='D')
        time.sleep(0.5)
        ret, frame = cap.read()
        if not ret:
            print(" 無法取得影像，跳過此次偵測")
            continue
        #cv2.imshow("Relocation", frame)
        #cv2.waitKey(1)

        result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
        if result:
            # 偵測到 LED，重置計數
            not_found_count = 0
            cx, cy = result
            move_count += 1
            # 每移動 3 步執行一次置中補償
            if move_count % 3 == 0:
                print("水平置中補償檢查")
                centered = move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25)
                if not centered:
                    # 若有移動則等待一段時間再抓取新畫面
                    time.sleep(0.5)
                    # 不改變狀態，下一迴圈會重新 detect
                else:
                    print("已水平居中")
        else:
            not_found_count += 1
            print(f"連續第 {not_found_count} 次未偵測到 LED")
            if not_found_count >= 5:
                # 視為抵達左邊界
                print("偵測不到 LED 達 5 次，推定已到達左邊界")
                break

    # 已達左邊界，回退 5 步並置中
    print("往右回退 5 步...")
    for i in range(5):
        send_uart(ser, command='C')
        time.sleep(0.5)
    # 取得當前畫面並對左邊界 LED 置中
    ret, frame = cap.read()
    if ret:
        result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
    else:
        result = None
    if result:
        cx, cy = result
        print("執行左邊界 LED 對準...")
        # 持續微調直到 LED 居中
        while result:
            if move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25):
                break
            # 若尚未居中，等待移動完成並更新影像位置
            time.sleep(0.5)
            ret, frame = cap.read()
            if not ret:
                result = None
                break
            result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
            if result:
                cx, cy = result
    else:
        print("左邊界 LED 遺失，無法對準")

    # === 往上移動至左上角 (垂直向上) ===
    not_found_count = 0
    move_count = 0
    print("開始向上移動，搜尋上邊界...")
    while True:
        # 發送向上的大步移動指令
        send_uart(ser, command='A')
        time.sleep(0.5)
        ret, frame = cap.read()
        if not ret:
            print("無法取得影像，跳過此次偵測")
            continue
        #cv2.imshow("Relocation", frame)
        #cv2.waitKey(1)

        result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
        #cv2.imshow("Relocation", frame)
        #cv2.waitKey(1)

        if result:
            # 偵測到 LED，重置計數
            not_found_count = 0
            cx, cy = result
            move_count += 1
            # 每走 3 步做一次置中補償
            if move_count % 1 == 0:
                print("垂直置中補償檢查")
                centered = move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25)
                if not centered:
                    time.sleep(0.5)
                else:
                    print("已垂直居中")
        else:
            not_found_count += 1
            print(f"連續第 {not_found_count} 次未偵測到 LED")
            if not_found_count >= 5:
                print("偵測不到 LED 達 5 次，推定已到達上邊界")
                break

    # 已達上邊界，回退 5 步並置中
    print("向下回退 5 步...")
    for i in range(5):
        send_uart(ser, command='B')
        time.sleep(0.5)
    # 取得當前畫面並對上邊界 LED 置中
    ret, frame = cap.read()
    if ret:
        result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
    else:
        result = None
    if result:
        cx, cy = result
        print("執行上邊界 LED 對準...")
        while result:
            if move_to_center(ser, cx, cy, frame_center_x, frame_center_y, tolerance=25):
                break
            time.sleep(0.5)
            ret, frame = cap.read()
            if not ret:
                result = None
                break
            result = detect_orange_circle(frame, frame_center_x, frame_center_y, draw=False)
            if result:
                cx, cy = result
    else:
        print("上邊界 LED 遺失，無法對準")

    print("已定位至左上角 LED（r0_c0），結束程式")
    # === 清理資源 ===
    cap.release()
    ser.close()
    cv2.destroyAllWindows()
