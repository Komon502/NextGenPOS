# NextGen POS 🍽

ระบบ Point-of-Sale สำหรับร้านอาหาร พัฒนาด้วย **C# Windows Forms (.NET Framework 4.6.2)**

## ✨ ฟีเจอร์

| หน้า | รายละเอียด |
|------|------------|
| 🪑 **Tables** | แผนผังโต๊ะแบบ floor plan, filter ตาม zone, สีบอกสถานะ |
| 📋 **Orders** | ดูออเดอร์ทั้งหมด, filter ตาม status, เปิดแก้ไขได้ |
| 🍳 **Kitchen** | Kitchen Display System, auto-refresh 10 วินาที, Mark as Ready |
| 💳 **Payment** | เงินสด / QR Pay / โอนเงิน, คำนวณเงินทอน real-time |
| 📊 **Reports** | ยอดขายประจำวัน, Top เมนู, Export .txt |
| ⚙ **Settings** | แก้ไขชื่อร้าน, VAT, Staff, TestMode |

## 🏗 โครงสร้าง

`
NextGenPOS/
├── Models/          # Data models (Table, Order, MenuItem, ...)
├── Data/
│   ├── Mock/        # In-memory repositories (TestMode)
│   └── AccessDb/    # MS Access database repositories
├── Services/        # Business logic (OrderService, PaymentService, ...)
├── Controls/        # Custom WinForms controls (TableButton, MenuItemCard, ...)
├── Forms/           # UI Forms (MainForm, OrderEntryForm, KitchenForm, ...)
├── Helpers/         # ThemeManager (dark violet theme)
├── Program.cs       # Entry point
└── settings.txt     # Config file
`

## 🚀 วิธีรัน

1. เปิด NextGenPOS.sln ใน Visual Studio 2019+
2. Build → Run (F5)
3. โปรแกรมจะรันในโหมด **TestMode** (ข้อมูล mock) โดยอัตโนมัติ

## ⚙ settings.txt

`
RestaurantName=My Restaurant
TaxRate=7
Currency=THB
DefaultStaff=Cashier
TestMode=true
DbPath=Database\NextGenPOS.accdb
`

## 📋 Requirements

- .NET Framework 4.6.2+
- Visual Studio 2019 / 2022
- (Optional) Microsoft Access Database Engine สำหรับโหมด Production

## 📄 License

MIT License
