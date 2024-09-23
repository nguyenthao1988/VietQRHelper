# Vietnam QR Pay Csharp

A c# port from [https://github.com/xuannghia/vietnam-qr-pay](https://github.com/xuannghia/vietnam-qr-pay), Thư viện hỗ trợ encode/decode mã QR của VietQR (QR Ngân hàng, QR Đa năng Momo/ZaloPay) & VNPayQR.
Thư viện C# được fork từ [https://github.com/nick-hoang/vietnam-qr-pay-csharp](https://github.com/nick-hoang/vietnam-qr-pay-csharp)


## Encode - Tạo mã QR

### VietQR Tĩnh
```csharp

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VietQRHelper;

namespace TestQRPay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateQR_Click(object sender, EventArgs e)
        {
            var qrPay = QRPay.InitVietQR(
                  bankBin: BankApp.BanksObject[BankKey.VIETCOMBANK].bin,
                  bankNumber: "0721000584901", // Số tài khoản
                  amount: "20000", // Số tiền
                  purpose: "Donate LapTrinhVB"  // Nội dung chuyển tiền
                );
            var content = qrPay.Build();

            var imageQR = QRCodeHelper.TaoVietQRCodeImage(content );
            pictureBox1.Image = imageQR;    
        }
    }
}

 
```

### VietQR Động

```csharp
using vietnam_qr_pay_csharp;

var qrPay = QRPay.initVietQR(
  bankBin: BankApp.BanksObject[BankKey.ACB].bin,
  bankNumber: "257678859", // Số tài khoản
  amount: "10000", // Số tiền
  purpose: "Chuyen tien", // Nội dung chuyển tiền
);
var content = qrPay.Build();

Console.Writeline(content)
// 00020101021238530010A0000007270123000697041601092576788590208QRIBFTTA53037045405100005802VN62150811Chuyen tien630453E6

```

### QR Đa năng của MoMo và ZaloPay

Hiện tại, QR Đa năng của MoMo và ZaloPay đang thông qua Ngân hàng Bản Việt (BVBank) để nhận tiền.

Mỗi tài khoản MoMo/ZaloPay sẽ được gán một STK tương ứng tại BVBank. Tiền chuyển đến STK này sẽ được chuyển tiếp đến ví MoMo/ZaloPay.

Bạn có thể lấy STK này tại trang chi tiết của QR Nhận tiền trong ứng dụng MoMo/ZaloPay.

#### MoMo

```csharp
using vietnam_qr_pay_csharp;

// Số tài khoản trong ví MoMo
var accountNumber = "99MM24011M34875080";

var momoQR = QRPay.initVietQR(
  bankBin: BankApp.BanksObject[BankKey.BANVIET].bin,
  bankNumber: accountNumber,
  // amount: "10000", // Số tiền (không bắt buộc)
  // purpose: "Chuyen tien", // Nội dung (không bắt buộc)
);

// Trong mã QR của MoMo có chứa thêm 1 mã tham chiếu tương ứng với STK
momoQR.additionalData.reference = "MOMOW2W" + accountNumber.Substring(10)

// Mã QR của MoMo có thêm 1 trường ID 80 với giá trị là 3 số cuối của SỐ ĐIỆN THOẠI của tài khoản nhận tiền
momoQR.SetUnreservedField("80", "046")

const content = momoQR.Build()

// 00020101021138620010A00000072701320006970454011899MM24011M348750800208QRIBFTTA53037045802VN62190515MOMOW2W3487508080030466304EBC8

```

<img src="test/momo.svg" width="160">

#### ZaloPay

> Trong mã QR của ZaloPay có chứa một số thông tin bổ sung ở trường ID 26. Tuy nhiên chưa rõ chức năng của các thông tin này (có thể là dùng để định danh từng mã QR đc tạo trên hệ thống của ZaloPay). Trong ví dụ dưới sẽ bỏ qua các thông tin này.

```csharp
using vietnam_qr_pay_csharp;

// Số tài khoản trong ví ZaloPay
var accountNumber = "99ZP24009M07248267";
  
const zaloPayQR = QRPay.initVietQR(
  bankBin: BankApp.BanksObject[BankKey.BANVIET].bin,
  bankNumber: accountNumber,
  // amount: '10000', // Số tiền (không bắt buộc)
  // purpose: 'Chuyen tien', // Nội dung (không bắt buộc)
)

var content = zaloPayQR.Build()
// 00020101021138620010A00000072701320006970454011899ZP24009M072482670208QRIBFTTA53037045802VN6304073C
```

<img src="test/zalopay.svg" width="160">


### VNPay 

```csharp
var qrPay = QRPay.initVNPayQR(
  merchantId: "0102154778",
  merchantName: "TUGIACOMPANY",
  store: "TU GIA COMPUTER",
  terminal: "TUGIACO1",
)
var content = qrPay.Build()
Console.Writeline(content)
// 00020101021126280010A0000007750110010531314453037045408210900005802VN5910CELLPHONES62600312CPSHN ONLINE0517021908061613127850705ONLHN0810CellphoneS63047685

```


## Decode mã QR

### VietQR
```javascript
using vietnam_qr_pay_csharp;

const qrContent = '00020101021238530010A0000007270123000697041601092576788590208QRIBFTTA5303704540410005802VN62150811Chuyen tien6304BBB8'
const qrPay = new QRPay(qrContent);
Console.Writeline(qrPay.isValid) // true
Console.Writeline(qrPay.provider.name) // VIETQR
Console.Writeline(qrPay.consumer.bankBin) // 970416
Console.Writeline(qrPay.consumer.bankNumber) // 257678859
Console.Writeline(qrPay.amount) // 1000
Console.Writeline(qrPay.additionalData.purpose) // Chuyen tien

```

### VNPay
```csharp
using vietnam_qr_pay_csharp;

const qrContent = "00020101021126280010A0000007750110010531314453037045408210900005802VN5910CELLPHONES62600312CPSHN ONLINE0517021908061613127850705ONLHN0810CellphoneS63047685"
const qrPay = new QRPay(qrContent);
Console.Writeline(qrPay.isValid) // true
Console.Writeline(qrPay.provider.name) // VNPAY
Console.Writeline(qrPay.merchant.merchantId) // 0105313144
Console.Writeline(qrPay.amount) // 21090000
Console.Writeline(qrPay.additionalData.store) // CPSHN ONLINE
Console.Writeline(qrPay.additionalData.terminal) // ONLHN
Console.Writeline(qrPay.additionalData.purpose) // CellphoneS
Console.Writeline(qrPay.additionalData.reference) // 02190806161312785

```


## `QRPay` class


```csharp
using vietnam_qr_pay_csharp;
```

| Name | Type | Description |
| --- | --- | --- |
| `isValid` | `boolean` | Kiểm tra tính hợp lệ của mã QR |
| `initMethod` | `string` | Phương thức khởi tạo (`11` - QR Tĩnh, `12` - QR động) |
| `provider` | `Provider` | Thông tin nhà cung cấp |
| `merchant` | `Merchant` | Thông tin merchant |
| `consumer` | `Consumer` | Thông tin người thanh toán |
| `amount` | `string` | Số tiền giao dịch |
| `currency` | `string` | Mã tiền tệ (VNĐ: 704) |
| `nation` | `string` | Mã quốc gia |
| `additionalData` | `AdditionalData` | Thông tin bổ sung |
| `crc` | `string` | Mã kiểm tra |
| `build()` | `method` | Tạo lại mã QR mới |

### `Provider` class

Thông tin đơn vị cung cấp mã QR (VietQR, VNPay)


| Name | Type | Description |
| --- | --- | --- |
| `guid` | `string` | Mã định danh toàn cầu |
| `name` | `string` | Tên nhà cung cấp |

### `Merchant` class

Thông tin merchant (Đơn vị chấp nhận thanh toán)

| Name | Type | Description |
| --- | --- | --- |
| `id` | `string` | Mã định danh đơn vị CNTT |
| `name` | `string` | Tên đơn vị CNTT |

### `Consumer` class

Thông tin người thanh toán

| Name | Type | Description |
| --- | --- | --- |
| `bankBin` | `string` | Mã ngân hàng |
| `bankNumber` | `string` | Số tài khoản |

### `AdditionalData` class

Thông tin bổ sung

| Name | Type | Description |
| --- | --- | --- |
| `billNumber` | `string` | Số hóa đơn |
| `mobileNumber` | `string` | Số điện thoại di động |
| `store` | `string` | Tên cửa hàng |
| `loyaltyNumber` | `string` | Mã khách hàng thân thiết |
| `reference` | `string` | Mã Tham chiếu |
| `customerLabel` | `string` | Mã khách hàng |
| `terminal` | `string` | Tên điểm bản |
| `purpose` | `string` | Nội dung giao dịch |

###  `Build()` method

Trả về nội dung mã QR mới

```javascript
import { QRPay } from 'vietnam-qr-pay';


const qrContent = '00020101021238530010A0000007270123000697041601092576788590208QRIBFTTA5303704540410005802VN62150811Chuyen tien6304BBB8'
const qrPay = new QRPay(qrContent);

// qrPay.amount === "10000"
// qrPay.additionalData.purpose === "Chuyen tien"

qrPay.amount = '999999';
qrPay.additionalData.purpose = 'Cam on nhe';

const newQRContent = qrPay.build();
/* 
00020101021238530010A0000007270123000697041601092576788590208QRIBFTTA530370454069999995802VN62140810Cam on nhe6304E786
*/

```
