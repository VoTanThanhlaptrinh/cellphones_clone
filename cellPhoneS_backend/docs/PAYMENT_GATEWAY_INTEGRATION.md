# Kết nối CellphoneS với payment-module

Tài liệu này mô tả hai service nói chuyện với nhau thế nào và những gì cần cấu hình ở **mỗi bên**.
Hướng dẫn gốc của payment-module: `D:\IT\payment-module\doc\API_INTEGRATION_GUIDE.md`.

## 1. Tổng quan

Có hai chiều, mỗi chiều dùng một loại thông tin xác thực riêng:

| Chiều | Ai gọi | Endpoint | Xác thực |
|---|---|---|---|
| CellphoneS → payment-module | CellphoneS | `POST /api/v1/payments/checkout`, `GET /api/v1/payments/status`, `POST /api/v1/payments/cancel` | Header `x-api-key` (+ `Idempotency-Key` khi checkout) |
| payment-module → CellphoneS | payment-module | `POST /api/payments/callback` | Chữ ký HMAC-SHA256 (`X-Payment-Signature`) |

Payment-module còn nhận webhook của PayOS/VNPay ở `/webhooks/**`; chiều đó không liên quan tới CellphoneS.

Luồng một đơn:

1. Khách bấm thanh toán online → CellphoneS gọi `checkout`, nhận `checkoutUrl` / mã QR.
2. Khách trả tiền → PayOS/VNPay báo payment-module → payment-module đổi payment sang `PAID`.
3. Payment-module gửi **callback** về CellphoneS (có thử lại nếu lỗi).
4. CellphoneS **hỏi lại** `GET /status` rồi đánh dấu đơn `Paid`. Callback chỉ là gợi ý, nguồn sự thật luôn là gateway.
5. Dự phòng khi mất callback: trang thanh toán, `GET /api/orders/{id}/status` và job `sync-online-payments` (mỗi 5 phút) cũng hỏi gateway.

CellphoneS không lưu trạng thái payment, chỉ lưu `Order.PaymentAttempt` để sinh `Idempotency-Key` ổn định: `cellphones-order-{orderId}-attempt-{n}`.

## 2. Cấu hình

### 2.1 Phía CellphoneS (`cellPhoneS_backend`)

| Khoá | Ý nghĩa |
|---|---|
| `PaymentGateway:BaseUrl` | URL gốc của payment-module (không có `/api`) |
| `PaymentGateway:ApiKey` | API key của app `CELLPHONES`, cấp từ payment-module |
| `PaymentGateway:CallbackSecret` | Secret ký callback, **phải giống** `APP_CELLPHONES_CALLBACK_SECRET` ở payment-module (>= 16 ký tự) |
| `PaymentGateway:TestAmountOverride` | Chỉ để thử: ép mọi đơn thanh toán số tiền này. **Xoá ở production** |

Tự sinh secret (>= 16 ký tự, nên dùng 64 ký tự hex):

```bash
openssl rand -hex 32
```

Local: dùng user-secrets (project đã có `UserSecretsId`).

```bash
cd cellPhoneS_backend
dotnet user-secrets set "PaymentGateway:CallbackSecret" "<secret>"
dotnet user-secrets list
```

Render: đặt biến môi trường `PaymentGateway__CallbackSecret` (hai dấu gạch dưới thay cho `:`), tương tự `PaymentGateway__BaseUrl`, `PaymentGateway__ApiKey`.

**Không commit và không dán secret vào chat, log hay frontend.**

### 2.2 Phía payment-module

Biến môi trường (xem khối `CELLPHONES` trong `application.yaml`):

| Biến | Giá trị |
|---|---|
| `APP_CELLPHONES_API_KEY` | API key (>= 32 ký tự), cùng giá trị với `PaymentGateway:ApiKey` của CellphoneS |
| `APP_CELLPHONES_PROVIDERS` | `PAYOS,VNPAY` (hoặc một trong hai) |
| `APP_CELLPHONES_RETURN_URL` | `https://<frontend>/payment/return` |
| `APP_CELLPHONES_CANCEL_URL` | `https://<frontend>/payment/return` |
| `APP_CELLPHONES_CALLBACK_URL` | `https://<backend>/api/payments/callback` |
| `APP_CELLPHONES_CALLBACK_SECRET` | Secret ở mục 2.1 |

Callback chỉ được gửi khi `APP_CELLPHONES_CALLBACK_URL` có giá trị. URL phải truy cập được từ payment-module: nếu CellphoneS chạy local mà payment-module ở nơi khác thì cần một tunnel (ngrok, Cloudflare Tunnel...) trỏ vào `http://localhost:5169`.

## 3. Hợp đồng callback

`POST /api/payments/callback`, `Content-Type: application/json`, tối đa 16 KB.

Header:

| Header | Nội dung |
|---|---|
| `X-Payment-Event-Id` | UUID của sự kiện |
| `X-Payment-Timestamp` | Epoch giây |
| `X-Payment-Signature` | `sha256=` + hex thường của `HMAC-SHA256(secret, "<timestamp>.<body thô>")` |

Body:

```json
{
  "eventId": "6f1c3b9e-3a5b-4a39-9d1f-0b8f5e2d7c11",
  "eventType": "payment.paid",
  "occurredAt": "2026-10-16T10:05:12+07:00",
  "payment": {
    "paymentId": 3951234567890123456, "orderId": 1001, "status": "PAID", "provider": "PAYOS",
    "amount": 150000, "currency": "VND",
    "transactionId": "3951234567890123457", "transactionStatus": "SUCCESS", "paidAt": "2026-10-16T10:05:10+07:00"
  }
}
```

Gateway gửi một lần cho mỗi trạng thái cuối: `PAID`, `FAILED`, `EXPIRED`, `CANCELLED`, `REVIEW_REQUIRED`.

CellphoneS xử lý:

1. Kiểm chữ ký trên **byte thô** của body (so sánh hằng thời gian) và timestamp lệch tối đa 5 phút. Sai: `401`.
2. Đọc `payment.orderId`, hỏi lại gateway `GET /status?orderId=...`.
3. `PAID` → đánh dấu đơn `Paid`, ghi một `PaymentTransaction`, xoá sản phẩm đã đặt khỏi giỏ (idempotent, gọi lại nhiều lần vẫn an toàn). `REVIEW_REQUIRED` → ghi log lỗi để xử lý thủ công.

Mã trả về:

| Mã | Khi nào | Gateway làm gì |
|---|---|---|
| `200` | Đã xử lý, hoặc đơn không tồn tại (thử lại cũng vô ích) | Dừng |
| `400` | Body không phải JSON hợp lệ | Thử lại |
| `401` | Sai chữ ký / timestamp quá hạn | Thử lại |
| `500` | Chưa cấu hình `CallbackSecret`, hoặc không gọi được gateway | Thử lại |

Gateway thử lại ở 30 giây, 2, 5, 15 phút, 1, 3, 6, 12 giờ, tối đa 10 lần, nên phải trả 2xx trong vòng 5 giây.

## 4. Các bước kết nối lần đầu

1. Ở payment-module, sinh API key: `openssl rand -hex 32`, đặt `APP_CELLPHONES_API_KEY`.
2. Lấy `PaymentGateway:CallbackSecret` từ CellphoneS (mục 2.1), đặt cùng giá trị vào `APP_CELLPHONES_CALLBACK_SECRET`.
3. Đặt các biến còn lại ở mục 2.2 rồi khởi động lại payment-module.
4. Ở CellphoneS đặt `PaymentGateway:BaseUrl` và `PaymentGateway:ApiKey` (cùng key bước 1).
5. Chạy migration: `dotnet ef database update`.
6. Thử: tạo đơn, chọn thanh toán online, trả tiền sandbox. Xem log của CellphoneS có dòng `Callback thanh toán payment.paid ...` và đơn chuyển sang `Paid`.

## 5. Tự thử callback bằng tay

Tạo một callback giả đã ký đúng (cần `openssl`, `curl`):

```bash
SECRET='<secret>'
BODY='{"eventId":"test-1","eventType":"payment.paid","occurredAt":"2026-10-17T10:00:00+07:00","payment":{"paymentId":1,"orderId":1001,"status":"PAID","provider":"PAYOS","amount":150000,"currency":"VND"}}'
TS=$(date +%s)
SIG="sha256=$(printf '%s.%s' "$TS" "$BODY" | openssl dgst -sha256 -hmac "$SECRET" -hex | sed 's/^.* //')"
curl -i -X POST http://localhost:5169/api/payments/callback \
  -H "Content-Type: application/json" \
  -H "X-Payment-Event-Id: test-1" -H "X-Payment-Timestamp: $TS" -H "X-Payment-Signature: $SIG" \
  --data-binary "$BODY"
```

`--data-binary` giữ nguyên từng byte của `$BODY`. Đơn `1001` phải tồn tại và đã từng bắt đầu thanh toán online, và payment-module phải đang chạy (CellphoneS sẽ hỏi lại nó). Đổi một ký tự của `$BODY` sau khi ký thì phải nhận `401`.

## 6. Xử lý sự cố

| Triệu chứng | Nguyên nhân thường gặp |
|---|---|
| Callback luôn `401` | Secret hai bên khác nhau; body bị proxy sửa (nén, đổi khoảng trắng); đồng hồ hai máy lệch quá 5 phút |
| Callback `500` | Chưa đặt `PaymentGateway:CallbackSecret`; payment-module không truy cập được từ CellphoneS (`PaymentGateway:BaseUrl`, `ApiKey`) |
| Callback tới nhưng đơn vẫn `Pending` | Đơn chưa từng gọi checkout (`PaymentAttempt = 0`); payment ở gateway chưa `PAID`; xem log `REVIEW_REQUIRED` |
| Payment-module không gửi callback | `APP_CELLPHONES_CALLBACK_URL` trống hoặc không truy cập được; xem bảng outbox / log `RestWebhookNotifier` |
| Mất callback | Không sao: job 5 phút và trang thanh toán sẽ đồng bộ lại |

## 7. Lưu ý bảo mật

- Dùng **HTTPS** cho cả hai chiều ở production.
- `x-api-key` và `callbackSecret` là hai thứ khác nhau; không dùng chung và không để lộ ra frontend.
- Có thể xoay secret: đổi cả hai bên cùng lúc; trong lúc đó callback có thể `401` và gateway sẽ thử lại.
- Tắt `PaymentGateway:TestAmountOverride` ở production.
- Callback chỉ là gợi ý nên kẻ không có secret cũng không đánh dấu được đơn đã trả; chữ ký chủ yếu chống lạm dụng endpoint.
