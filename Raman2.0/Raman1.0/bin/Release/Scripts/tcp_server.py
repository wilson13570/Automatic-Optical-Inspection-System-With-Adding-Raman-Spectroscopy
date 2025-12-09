import sys
import io
import os
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
sys.path.append(os.path.dirname(__file__))

import socket
from cv_cl import auto_scan  # ⭐ 引入你剛剛改好的函式

def start_tcp_server(host='0.0.0.0', port=5000):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((host, port))
    server_socket.listen(1)

    print(f"TCP Server啟動，正在監聽 {host}:{port} ...")

    while True:
        client_socket, addr = server_socket.accept()
        print(f"有連線來自：{addr}")

        data = client_socket.recv(1024).decode('utf-8').strip()
        print(f"收到資料: {data}")

        if data.startswith("AUTO_SCAN:"):
            try:
                size = data.split(":")[1].strip().lower().replace("×", "x")
                columns, rows = map(int, size.split("x"))
                print(f"啟動 AutoScan ({columns} x {rows}) ...")

                #  呼叫實際拍照函式，改為有回傳 True / False
                result = auto_scan(columns, rows)

                if result:
                    client_socket.sendall("OK\n".encode('utf-8'))     # 成功才回 OK
                else:
                    client_socket.sendall("ERROR\n".encode('utf-8'))  # 掃描過程中失敗
                client_socket.close()

            except Exception as e:
                print(f"指令錯誤或執行失敗：{e}")
                client_socket.sendall(f"ERROR: {e}\n".encode('utf-8'))
                client_socket.close()

        else:
            print("收到未知指令")
            client_socket.sendall("UNKNOWN\n".encode('utf-8'))
            client_socket.close()

if __name__ == "__main__":
    start_tcp_server()
