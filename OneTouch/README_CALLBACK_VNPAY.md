# Callback VNPay Page

## Tổng quan

Đã tạo một page mới `CallbackVNPay` để xử lý callback từ VNPay thay vì sử dụng model như trước đây. Page này cung cấp giao diện đẹp và logic xử lý callback hoàn chỉnh.

## Cấu trúc

### Files đã tạo:
- `Pages/CallbackVNPay.cshtml` - Giao diện người dùng
- `Pages/CallbackVNPay.cshtml.cs` - Logic xử lý callback

### Files đã xóa:
- `Pages/PaymentCallback.cshtml` - Page cũ
- `Pages/PaymentCallback.cshtml.cs` - Code-behind cũ
- `Models/VnPayCallbackModel.cs` - Model không còn cần thiết

### Files đã cập nhật:
- `Controllers/PaymentController.cs` - Đơn giản hóa logic callback
- `appsettings.json` - Cập nhật URL callback

## Tính năng

### 1. Giao diện đẹp
- Loading spinner khi đang xử lý
- Hiển thị kết quả thành công/thất bại với icon
- Thông tin giao dịch chi tiết
- Các nút điều hướng hữu ích

### 2. Logic xử lý hoàn chỉnh
- Xử lý tất cả các trường hợp callback từ VNPay
- Kiểm tra response code và transaction status
- Parse thông tin đặt lịch từ OrderInfo
- Tạo appointment và invoice tự động
- Xử lý lỗi và logging

### 3. Bảo mật
- Kiểm tra trùng lặp appointment
- Validate dữ liệu đầu vào
- Logging chi tiết cho debug

## Luồng xử lý

1. **VNPay gửi callback** → `PaymentController.PaymentCallbackVnpay()`
2. **Controller redirect** → `Pages/CallbackVNPay` với query parameters
3. **Page xử lý** → Parse dữ liệu và tạo appointment/invoice
4. **Hiển thị kết quả** → Thành công hoặc thất bại

## Cấu hình

URL callback trong `appsettings.json`:
```json
"PaymentBackReturnUrl": "http://localhost:7101/Payment/PaymentCallbackVnpay"
```

## Lợi ích

1. **Tách biệt concerns**: Logic callback được tách riêng khỏi controller
2. **Giao diện tốt hơn**: UX đẹp và thân thiện với người dùng
3. **Dễ maintain**: Code sạch và có cấu trúc rõ ràng
4. **Flexible**: Dễ dàng mở rộng và tùy chỉnh

## Sử dụng

Khi VNPay gửi callback, hệ thống sẽ:
1. Tự động redirect đến page CallbackVNPay
2. Hiển thị loading spinner
3. Xử lý dữ liệu và tạo appointment
4. Hiển thị kết quả với thông tin chi tiết
5. Cung cấp các nút điều hướng hữu ích 