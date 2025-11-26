-- ==============================================
-- SCRIPT TẠO DATABASE QUẢN LÝ PHÒNG TRỌ
-- Server: HUYDIEN
-- Database: QuanLyPhongTroDB
-- Version: 1.0
-- ==============================================

-- XÓA DATABASE CŨ VÀ TẠO MỚI
USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyPhongTroDB')
BEGIN
    ALTER DATABASE QuanLyPhongTroDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyPhongTroDB;
END
GO

CREATE DATABASE QuanLyPhongTroDB;
GO

USE QuanLyPhongTroDB;
GO

-- ==============================================
-- 1. BẢNG CẤU HÌNH (Settings)
-- ==============================================
CREATE TABLE Settings (
    SettingKey NVARCHAR(50) PRIMARY KEY,
    SettingValue DECIMAL(18, 0),
    Description NVARCHAR(200)
);
GO

-- ==============================================
-- 2. BẢNG NGƯỜI DÙNG (Users)
-- ==============================================
CREATE TABLE Users (
    Username NVARCHAR(50) PRIMARY KEY,
    PasswordHash NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(100),
    Role NVARCHAR(20) DEFAULT 'Admin' -- 'Admin', 'Staff'
);
GO

-- ==============================================
-- 3. BẢNG LOẠI PHÒNG (RoomTypes)
-- ==============================================
CREATE TABLE RoomTypes (
    RoomTypeId INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100),         -- Tên loại phòng
    HasLoft BIT DEFAULT 0,          -- Có gác lửng không
    HasFurniture BIT DEFAULT 0,     -- Có nội thất không
    BasePrice DECIMAL(18, 0),       -- Giá cơ bản
    Area FLOAT,                     -- Diện tích (m2)
    Description NVARCHAR(500)       -- Mô tả chi tiết
);
GO

-- ==============================================
-- 4. BẢNG PHÒNG (Rooms)
-- ==============================================
CREATE TABLE Rooms (
    RoomId INT PRIMARY KEY IDENTITY(1,1),
    RoomName NVARCHAR(50) NOT NULL,
    RoomTypeId INT FOREIGN KEY REFERENCES RoomTypes(RoomTypeId),
    Floor INT DEFAULT 1,                    -- Tầng
    Price DECIMAL(18,0) DEFAULT 0,          -- Giá riêng
    MaxOccupants INT DEFAULT 2,             -- Số người tối đa
    Status NVARCHAR(20) DEFAULT 'Trong',    -- 'Trong', 'DangThue', 'BaoTri'
    Description NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ==============================================
-- 5. BẢNG KHÁCH THUÊ (Customers)
-- ==============================================
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    CCCD NVARCHAR(20),              -- Căn cước công dân
    Email NVARCHAR(100),
    DateOfBirth DATE,               -- Ngày sinh
    Gender NVARCHAR(10),            -- Nam/Nữ
    Job NVARCHAR(100),              -- Nghề nghiệp
    Workplace NVARCHAR(200),        -- Nơi làm việc/học tập
    Address NVARCHAR(500),          -- Địa chỉ thường trú
    EmergencyContact NVARCHAR(100), -- Người liên hệ khẩn cấp
    EmergencyPhone NVARCHAR(20),    -- SĐT khẩn cấp
    Note NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ==============================================
-- 6. BẢNG HỢP ĐỒNG (Contracts)
-- ==============================================
CREATE TABLE Contracts (
    ContractId INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT FOREIGN KEY REFERENCES Rooms(RoomId),
    CustomerId INT FOREIGN KEY REFERENCES Customers(CustomerId),
    StartDate DATE NOT NULL,
    EndDate DATE,
    Deposit DECIMAL(18,0) DEFAULT 0,    -- Tiền cọc
    MonthlyRent DECIMAL(18,0),          -- Giá thuê hàng tháng
    IsActive BIT DEFAULT 1,             -- Còn hiệu lực
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ==============================================
-- 7. BẢNG HÓA ĐƠN (Invoices) - Lưu snapshot giá
-- ==============================================
CREATE TABLE Invoices (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    ContractId INT FOREIGN KEY REFERENCES Contracts(ContractId),
    Month INT,
    Year INT,
    -- Điện
    ElecOld INT DEFAULT 0,
    ElecNew INT DEFAULT 0,
    ElecPrice DECIMAL(18,0),        -- Giá điện tại thời điểm
    -- Nước
    WaterOld INT DEFAULT 0,
    WaterNew INT DEFAULT 0,
    WaterPrice DECIMAL(18,0),       -- Giá nước tại thời điểm
    -- Tổng
    RoomPrice DECIMAL(18,0),        -- Giá phòng
    OtherFee DECIMAL(18,0) DEFAULT 0,
    Discount DECIMAL(18,0) DEFAULT 0,   -- Giảm giá
    TotalAmount DECIMAL(18,0),
    Note NVARCHAR(500),
    Status NVARCHAR(20) DEFAULT 'ChuaThu', -- 'ChuaThu', 'DaThu'
    PaidDate DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ==============================================
-- 8. BẢNG TẠM TRÚ (TemporaryResidence) - Đăng ký tạm trú
-- ==============================================
CREATE TABLE TemporaryResidence (
    ResidenceId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT FOREIGN KEY REFERENCES Customers(CustomerId),
    RoomId INT FOREIGN KEY REFERENCES Rooms(RoomId),
    RegisterDate DATE,              -- Ngày đăng ký
    ExpiryDate DATE,                -- Ngày hết hạn
    PoliceStation NVARCHAR(200),    -- Công an phường/xã
    Status NVARCHAR(20) DEFAULT 'DaDangKy', -- 'DaDangKy', 'HetHan', 'ChuaDangKy'
    Note NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ==============================================
-- 9. BẢNG SỬA CHỮA (Maintenance) - Bảo trì
-- ==============================================
CREATE TABLE Maintenance (
    MaintenanceId INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT FOREIGN KEY REFERENCES Rooms(RoomId),
    ReportDate DATE,                -- Ngày báo
    FixDate DATE,                   -- Ngày sửa
    Issue NVARCHAR(500),            -- Vấn đề
    Cost DECIMAL(18,0) DEFAULT 0,   -- Chi phí
    PaidBy NVARCHAR(20),            -- 'ChuNha', 'KhachThue'
    Status NVARCHAR(20) DEFAULT 'ChuaSua', -- 'ChuaSua', 'DangSua', 'DaSua'
    Note NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ==============================================
-- DỮ LIỆU MẪU (SEED DATA)
-- ==============================================

-- Cài đặt mặc định
INSERT INTO Settings VALUES 
    ('GiaDien', 4000, N'VNĐ/kWh'),
    ('GiaNuoc', 10000, N'VNĐ/Khối'),
    ('PhiInternet', 100000, N'Phí Internet/tháng'),
    ('PhiRac', 20000, N'Phí rác/tháng'),
    ('NguongMienThue', 100000000, N'Mức miễn thuế 100 triệu/năm'),
    ('TyLeThue', 10, N'% Thuế GTGT + TNCN');
GO

-- Tài khoản admin (Password: 123456 - SHA256)
INSERT INTO Users VALUES 
    ('admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', N'Nguyễn Văn Điên', 'Admin');
GO

-- Loại phòng (Gác / Nội thất)
INSERT INTO RoomTypes (TypeName, HasLoft, HasFurniture, BasePrice, Area, Description) VALUES 
    (N'Phòng trọ không gác, không nội thất', 0, 0, 1200000, 12, N'Phòng trống cơ bản, có WC riêng'),
    (N'Phòng trọ không gác, có nội thất', 0, 1, 1800000, 14, N'Có giường, tủ, bàn học, máy lạnh'),
    (N'Phòng trọ có gác, không nội thất', 1, 0, 1500000, 16, N'Có gác lửng để ngủ, WC riêng'),
    (N'Phòng trọ có gác, có nội thất', 1, 1, 2200000, 18, N'Có gác + đầy đủ nội thất, máy lạnh');
GO

-- Phòng mẫu (3 tầng, mỗi tầng 4 phòng)
INSERT INTO Rooms (RoomName, RoomTypeId, Floor, Price, MaxOccupants, Status, Description) VALUES 
    -- Tầng 1
    (N'P.101', 1, 1, 1200000, 2, 'Trong', N'Phòng góc, gần cổng'),
    (N'P.102', 2, 1, 1800000, 2, 'DangThue', N'Có cửa sổ lớn'),
    (N'P.103', 1, 1, 1200000, 2, 'Trong', NULL),
    (N'P.104', 3, 1, 1500000, 3, 'DangThue', N'Gác rộng'),
    -- Tầng 2
    (N'P.201', 3, 2, 1500000, 3, 'Trong', NULL),
    (N'P.202', 4, 2, 2200000, 3, 'DangThue', N'Phòng đẹp nhất tầng 2'),
    (N'P.203', 2, 2, 1800000, 2, 'Trong', NULL),
    (N'P.204', 4, 2, 2200000, 3, 'DangThue', N'View đẹp'),
    -- Tầng 3
    (N'P.301', 1, 3, 1100000, 2, 'Trong', N'Giảm giá vì tầng cao'),
    (N'P.302', 3, 3, 1400000, 3, 'DangThue', N'Có ban công nhỏ'),
    (N'P.303', 2, 3, 1700000, 2, 'Trong', NULL),
    (N'P.304', 4, 3, 2100000, 3, 'Trong', N'Phòng góc, 2 cửa sổ');
GO

-- Khách thuê mẫu (Cần Thơ) - đầy đủ thông tin
INSERT INTO Customers (FullName, Phone, CCCD, Email, DateOfBirth, Gender, Job, Workplace, Address, EmergencyContact, EmergencyPhone) VALUES 
    (N'Nguyễn Văn An', '0901234567', '092201012345', 'an.nguyen@gmail.com', '2001-05-15', N'Nam', N'Sinh viên', N'Đại học Cần Thơ', N'Phường An Khánh, Quận Ninh Kiều, Cần Thơ', N'Nguyễn Văn Cha', '0911111111'),
    (N'Trần Thị Bình', '0912345678', '092302023456', 'binh.tran@gmail.com', '2002-08-20', N'Nữ', N'Sinh viên', N'Đại học Y Dược Cần Thơ', N'Phường Xuân Khánh, Quận Ninh Kiều, Cần Thơ', N'Trần Văn Bố', '0922222222'),
    (N'Lê Văn Cường', '0923456789', '092198034567', 'cuong.le@gmail.com', '1998-03-10', N'Nam', N'Nhân viên văn phòng', N'Công ty ABC', N'Phường Hưng Lợi, Quận Ninh Kiều, Cần Thơ', N'Lê Thị Mẹ', '0933333333'),
    (N'Phạm Thị Dung', '0934567890', '092295045678', 'dung.pham@gmail.com', '1995-11-25', N'Nữ', N'Kế toán', N'Công ty XYZ', N'Phường An Hòa, Quận Ninh Kiều, Cần Thơ', N'Phạm Văn Anh', '0944444444'),
    (N'Hoàng Văn Em', '0945678901', '092190056789', 'em.hoang@gmail.com', '1990-07-30', N'Nam', N'Kỹ sư', N'Công ty DEF', N'Phường Long Hòa, Quận Bình Thủy, Cần Thơ', N'Hoàng Thị Chị', '0955555555'),
    (N'Võ Thị Phương', '0956789012', '092399067890', 'phuong.vo@gmail.com', '1999-01-05', N'Nữ', N'Sinh viên', N'Đại học Cần Thơ', N'Phường Trà An, Quận Bình Thủy, Cần Thơ', N'Võ Văn Ba', '0966666666'),
    (N'Đặng Minh Tuấn', '0967890123', '092097078901', 'tuan.dang@gmail.com', '1997-12-12', N'Nam', N'Giáo viên', N'Trường THPT Châu Văn Liêm', N'Phường Thới Bình, Quận Ô Môn, Cần Thơ', N'Đặng Thị Má', '0977777777'),
    (N'Huỳnh Thị Mai', '0978901234', '092300089012', 'mai.huynh@gmail.com', '2000-04-18', N'Nữ', N'Sinh viên', N'Đại học FPT Cần Thơ', N'Xã Giai Xuân, Huyện Phong Điền, Cần Thơ', N'Huỳnh Văn Cậu', '0988888888');
GO

-- Hợp đồng mẫu (cho các phòng đang thuê)
INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, MonthlyRent, IsActive) VALUES 
    (2, 1, '2024-06-01', 3600000, 1800000, 1),  -- P.102 - Nguyễn Văn An
    (4, 2, '2024-08-15', 3000000, 1500000, 1),  -- P.104 - Trần Thị Bình
    (6, 3, '2024-09-01', 4400000, 2200000, 1),  -- P.202 - Lê Văn Cường
    (8, 4, '2024-10-01', 4400000, 2200000, 1),  -- P.204 - Phạm Thị Dung
    (10, 5, '2024-11-01', 2800000, 1400000, 1); -- P.302 - Hoàng Văn Em
GO

-- Đăng ký tạm trú (1 cái sắp hết hạn để test cảnh báo)
INSERT INTO TemporaryResidence (CustomerId, RoomId, RegisterDate, ExpiryDate, PoliceStation, Status) VALUES
    (1, 2, '2024-06-05', '2024-12-15', N'Công an Phường An Khánh', 'DaDangKy'),  -- Sắp hết hạn!
    (2, 4, '2024-08-20', '2025-08-20', N'Công an Phường An Khánh', 'DaDangKy'),
    (3, 6, '2024-09-05', '2025-09-05', N'Công an Phường An Khánh', 'DaDangKy'),
    (4, 8, '2024-10-05', '2025-10-05', N'Công an Phường An Khánh', 'DaDangKy'),
    (5, 10, '2024-11-05', '2025-11-05', N'Công an Phường An Khánh', 'DaDangKy');
GO

-- Hóa đơn mẫu (tháng 10, 11/2024)
INSERT INTO Invoices (ContractId, Month, Year, ElecOld, ElecNew, ElecPrice, WaterOld, WaterNew, WaterPrice, RoomPrice, OtherFee, TotalAmount, Status, PaidDate) VALUES
    -- Tháng 10/2024 - Đã thu
    (1, 10, 2024, 100, 150, 4000, 10, 15, 10000, 1800000, 120000, 2120000, 'DaThu', '2024-11-05'),
    (2, 10, 2024, 200, 280, 4000, 20, 28, 10000, 1500000, 120000, 2020000, 'DaThu', '2024-11-03'),
    (3, 10, 2024, 150, 220, 4000, 15, 22, 10000, 2200000, 120000, 2670000, 'DaThu', '2024-11-02'),
    (4, 10, 2024, 180, 250, 4000, 18, 25, 10000, 2200000, 120000, 2670000, 'DaThu', '2024-11-04'),
    (5, 10, 2024, 120, 170, 4000, 12, 18, 10000, 1400000, 120000, 1780000, 'DaThu', '2024-11-01'),
    -- Tháng 11/2024 - Một số chưa thu (test cảnh báo quá hạn)
    (1, 11, 2024, 150, 210, 4000, 15, 21, 10000, 1800000, 120000, 2220000, 'ChuaThu', NULL),  -- Quá hạn!
    (2, 11, 2024, 280, 350, 4000, 28, 35, 10000, 1500000, 120000, 1970000, 'DaThu', '2024-12-03'),
    (3, 11, 2024, 220, 300, 4000, 22, 30, 10000, 2200000, 120000, 2720000, 'ChuaThu', NULL),  -- Quá hạn!
    (4, 11, 2024, 250, 320, 4000, 25, 32, 10000, 2200000, 120000, 2670000, 'DaThu', '2024-12-02'),
    (5, 11, 2024, 170, 230, 4000, 18, 24, 10000, 1400000, 120000, 1800000, 'DaThu', '2024-12-01');
GO

PRINT N'============================================';
PRINT N'  DATABASE QUẢN LÝ PHÒNG TRỌ - THÀNH CÔNG';
PRINT N'============================================';
PRINT N'  Server: HUYDIEN';
PRINT N'  Database: QuanLyPhongTroDB';
PRINT N'  Tài khoản: admin';
PRINT N'  Mật khẩu: 123456';
PRINT N'============================================';
GO
