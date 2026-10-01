#!/usr/bin/env python3
"""PuckRust minimal LAN client (Windows/macOS/Linux, stdlib only).
Usage:
  python client_example.py --host 192.168.1.50 --port 7777 --name Zed
  python client_example.py --host 192.168.1.50 --discover  # broadcast LAN scan on :7778
Controls: immediately sends hello, then thrusts toward puck from snapshots.
Press Ctrl+C to disconnect.
"""
import argparse
import socket
import struct
import time

P_HELLO, P_WELCOME, P_REJECT = 0x01, 0x02, 0x03
P_INPUT, P_SNAPSHOT = 0x04, 0x05
P_EVENT, P_ACK = 0x06, 0x07
P_HEARTBEAT, P_DISCONNECT = 0x08, 0x09
P_DISC_QUERY = 0x10
P_CHAT = 0x20


def encode_hello(name, team_req=0, password=""):
    nb, pb = name.encode()[:24], password.encode()[:64]
    return bytes([P_HELLO, len(nb)]) + nb + bytes([team_req, len(pb)]) + pb


def encode_input(cid, seq, tx, ty, sprint=False, hit=False):
    flags = (0x01 if sprint else 0) | (0x02 if hit else 0)
    return struct.pack("<BBHbbB", P_INPUT, cid, seq, tx, ty, flags)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--host", default="127.0.0.1")
    ap.add_argument("--port", type=int, default=7777)
    ap.add_argument("--discovery-port", type=int, default=7778)
    ap.add_argument("--name", default="WinClient")
    ap.add_argument("--team", type=int, default=0)
    ap.add_argument("--discover", action="store_true")
    args = ap.parse_args()

    if args.discover:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        s.settimeout(3)
        s.sendto(bytes([P_DISC_QUERY, 0x01]), ("255.255.255.255", args.discovery_port))
        print(f"scanning :{args.discovery_port} ...")
        try:
            while True:
                data, addr = s.recvfrom(256)
                if data[0] == 0x11:
                    port = struct.unpack_from("<H", data, 2)[0]
                    print(f"found server at {addr[0]}:{port} name={data[6:]} players={data[4]}/{data[5]}")
        except socket.timeout:
            print("scan done")
        return

    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.settimeout(2.0)
    server = (args.host, args.port)
    s.sendto(encode_hello(args.name, args.team), server)
    print(f"hello -> {server} as '{args.name}'")
    try:
        data, _ = s.recvfrom(256)
    except socket.timeout:
        print("no reply (server down? firewall? wrong IP/port?)")
        return
    if data[0] == P_REJECT:
        print(f"rejected reason={data[1]} (1=full 2=badpass 3=banned)")
        return
    if data[0] != P_WELCOME:
        print(f"unexpected reply {data[0]:#x}")
        return
    cid, tick, team = data[1], struct.unpack_from("<Q", data, 2)[0], data[10]
    print(f"welcome id={cid} team={team} tick={tick}")
    s.settimeout(0.5)
    seq, last_hb, last_snap = 0, 0, time.time()
    puck = (0.0, 0.0)
    me = (0.0, 0.0)
    try:
        while True:
            now = time.time()
            if now - last_hb > 2.0:
                s.sendto(struct.pack("<BQ", P_HEARTBEAT, 0), server)
                last_hb = now
            # steer toward puck
            dx, dy = puck[0] - me[0], puck[1] - me[1]
            dist = (dx * dx + dy * dy) ** 0.5 or 1.0
            tx = max(-127, min(127, int(dx / dist * 100)))
            ty = max(-127, min(127, int(dy / dist * 100)))
            seq = (seq + 1) & 0xFFFF
            s.sendto(encode_input(cid, seq, tx, ty, dist > 6.0, dist < 1.4), server)
            try:
                while True:
                    data, _ = s.recvfrom(2048)
                    t = data[0]
                    if t == P_SNAPSHOT and len(data) >= 37:
                        tick = struct.unpack_from("<Q", data, 1)[0]
                        phase, sr, sb, period = data[9], data[10], data[11], data[12]
                        px, py = struct.unpack_from("<ff", data, 17)
                        puck = (px, py)
                        n = data[33]
                        for i in range(n):
                            off = 37 + i * 20
                            pid = data[off]
                            x, y = struct.unpack_from("<ff", data, off + 2)
                            if pid == cid:
                                me = (x, y)
                        if now - last_snap > 2.0:
                            print(f"tick={tick} phase={phase} {sr}-{sb} p{period} puck=({px:.1f},{py:.1f}) me=({me[0]:.1f},{me[1]:.1f})")
                            last_snap = now
                    elif t == P_EVENT and len(data) >= 6:
                        evseq = struct.unpack_from("<I", data, 1)[0]
                        s.sendto(struct.pack("<BI", P_ACK, evseq), server)
                        print(f"event kind={data[5]} seq={evseq} len={len(data)}")
            except socket.timeout:
                pass
            time.sleep(1 / 20)
    except KeyboardInterrupt:
        s.sendto(bytes([P_DISCONNECT, cid]), server)
        print("disconnected")


if __name__ == "__main__":
    main()
