const net = require('net');
const WebSocket = require('ws');

const HOST = '127.0.0.1';
const PORT = 505;

let transactionId = 0;
let fromPlc = new Uint16Array(100);
let toPlc = new Uint16Array(100);
let bToPlc = new Array(160).fill(false);
let bFromPlc = new Array(160).fill(false);

const client = new net.Socket();
let toggle = 0;

// 🌐 WebSocket 서버 생성
const wss = new WebSocket.Server({ port: 8080 });
let sockets = [];

wss.on('connection', (ws) => {
    sockets.push(ws);
    console.log('🌐 클라이언트 WebSocket 연결됨');

    ws.on('close', () => {
        sockets = sockets.filter(s => s !== ws);
        console.log('❌ 클라이언트 연결 종료');
    });
});

function broadcastPlcData() {
    const payload = JSON.stringify({
        fromPlc: Array.from(fromPlc),
        toPlc: Array.from(toPlc),
        bFromPlc,
        bToPlc
    });

    sockets.forEach(ws => {
        if (ws.readyState === WebSocket.OPEN) {
            ws.send(payload);
        }
    });
}

client.connect(PORT, HOST, () => {
    console.log('✅ PLC 연결됨');

    setInterval(() => {
        const send = Buffer.alloc(256);

        // --- HEADER ---
        send.writeUInt16BE(transactionId++, 0); // Transaction ID
        send.writeUInt16BE(0, 2);               // Protocol ID
        send.writeUInt16BE(12, 4);              // Length
        send[6] = 1;                            // Unit ID
        send[7] = 0x04;                         // FC 03: Read Holding Registers

        if (toggle === 0) {
            send.writeUInt16BE(0, 8);     // 주소 0부터
            send.writeUInt16BE(100, 10);  // 100개
        } else {
            send.writeUInt16BE(100, 8);   // 주소 100부터
            send.writeUInt16BE(100, 10);  // 100개
        }

        client.write(send);

        client.removeAllListeners('data');
        client.once('data', (data) => {
            if (toggle === 0) {
                for (let i = 0; i < 100; i++) {
                    fromPlc[i] = (data[9 + (i * 2)] << 8) | data[10 + (i * 2)];
                }
                for (let i = 0; i < 10; i++) {
                    const word = fromPlc[90 + i];
                    for (let j = 0; j < 16; j++) {
                        bFromPlc[i * 16 + j] = (word & (1 << j)) !== 0;
                    }
                }
            } else {
                for (let i = 0; i < 100; i++) {
                    toPlc[i] = (data[9 + (i * 2)] << 8) | data[10 + (i * 2)];
                }
                for (let i = 0; i < 10; i++) {
                    const word = toPlc[90 + i];
                    for (let j = 0; j < 16; j++) {
                        bToPlc[i * 16 + j] = (word & (1 << j)) !== 0;
                    }
                }
            }

            broadcastPlcData(); // 🛰 실시간 전송
        });

        toggle = 1 - toggle;
    }, 100);
});

client.on('error', (err) => {
    console.error('❌ PLC 통신 오류:', err.message);
});

client.on('close', () => {
    console.log('🔌 PLC 연결 종료');
});
