-- ==============================================
-- SCRIPT CẬP NHẬT DATABASE - VERSION 2.0
-- Thêm: Dịch vụ linh hoạt, Thu-Chi, Xe cộ, Công nợ
-- ==============================================

USE QuanLyPhongTroDB;
GO

-- ==============================================
-- 1. BẢNG DỊCH VỤ (Services) - Linh hoạt bật/tắt
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
BEGIN
    CREATE TABLE Services (
        ServiceId INT PRIMARY KEY IDENTITY(1,1),
        ServiceCode NVARCHAR(20) NOT NULL,       -- 'DIEN', 'NUOC', 'WIFI', 'XE', 'RAC'...
        ServiceName NVARCHAR(100) NOT NULL,      -- Tên hiển thị
        ServiceType NVARCHAR(20) NOT NULL,       -- 'ChiSo' (theo số), 'CoDinh' (cố định), 'SoLuong' (theo số lượng)
        UnitName NVARCHAR(50),                   -- 'kWh', 'Khối', 'Xe', 'Người'...
        UnitPrice DECIMAL(18,0) DEFAULT 0,       -- Đơn giá
        IsActive BIT DEFAULT 1,                  -- Đang sử dụng
        IsFree BIT DEFAULT 0,                    -- Miễn phí (không tính tiền)
        IsRequired BIT DEFAULT 0,                -- Bắt buộc (điện, nước)
        SortOrder INT DEFAULT 0,                 -- Thứ tự hiển thị
        Note NVARCHAR(500)
    );

    -- Dữ liệu mặc định
    INSERT INTO Services (ServiceCode, ServiceName, ServiceType, UnitName, UnitPrice, IsActive, IsFree, IsRequired, SortOrder, Note) VALUES
        ('DIEN', N'Tiền điện', 'ChiSo', N'kWh', 4000, 1, 0, 1, 1, N'Tính theo chỉ số đồng hồ'),
        ('NUOC', N'Tiền nước', 'ChiSo', N'Khối', 10000, 1, 0, 1, 2, N'Tính theo chỉ số đồng hồ'),
        ('WIFI', N'Internet/Wifi', 'CoDinh', N'Tháng', 0, 1, 1, 0, 3, N'Miễn phí - đã bao gồm'),
        ('XE_MAY', N'Gửi xe máy', 'SoLuong', N'Xe', 0, 1, 1, 0, 4, N'Miễn phí xe đầu tiên'),
        ('XE_DAP', N'Gửi xe đạp', 'SoLuong', N'Xe', 0, 1, 1, 0, 5, N'Miễn phí'),
        ('RAC', N'Phí rác', 'CoDinh', N'Tháng', 0, 1, 1, 0, 6, N'Miễn phí - đã bao gồm'),
        ('NUOC_UONG', N'Nước uống', 'SoLuong', N'Bình', 15000, 0, 0, 0, 7, N'Tính theo số bình 20L'),
        ('THU_CUNG', N'Phụ thu thú cưng', 'CoDinh', N'Tháng', 100000, 0, 0, 0, 8, N'Nếu có nuôi thú cưng'),
        ('QUA_NGUOI', N'Phụ thu quá người', 'SoLuong', N'Người', 200000, 0, 0, 0, 9, N'Từ người thứ 3 trở đi');
    
    PRINT N'✓ Đã tạo bảng Services';
END
GO

-- ==============================================
-- 2. BẢNG DỊCH VỤ PHÒNG (RoomServices) - Dịch vụ từng phòng
-- Cho phép override giá và trạng thái free cho từng phòng
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomServices')
BEGIN
    CREATE TABLE RoomServices (
        RoomServiceId INT PRIMARY KEY IDENTITY(1,1),
        RoomId INT FOREIGN KEY REFERENCES Rooms(RoomId),
        ServiceId INT FOREIGN KEY REFERENCES Services(ServiceId),
        IsEnabled BIT DEFAULT 1,                 -- Bật/tắt cho phòng này
        IsFreeOverride BIT NULL,                 -- NULL = theo setting chung, 0/1 = override
        CustomPrice DECIMAL(18,0) NULL,          -- NULL = theo giá chung, có giá = override
        Quantity INT DEFAULT 1,                  -- Số lượng (xe, người...)
        Note NVARCHAR(500)
    );
    
    PRINT N'✓ Đã tạo bảng RoomServices';
END
GO

-- ==============================================
-- 3. BẢNG CHI TIẾT HÓA ĐƠN (InvoiceDetails) - Đa dịch vụ
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceDetails')
BEGIN
    CREATE TABLE InvoiceDetails (
        DetailId INT PRIMARY KEY IDENTITY(1,1),
        InvoiceId INT FOREIGN KEY REFERENCES Invoices(InvoiceId) ON DELETE CASCADE,
        ServiceId INT FOREIGN KEY REFERENCES Services(ServiceId),
        ServiceName NVARCHAR(100),               -- Snapshot tên dịch vụ
        Quantity INT DEFAULT 1,                  -- Số lượng
        OldValue INT NULL,                       -- Chỉ số cũ (nếu có)
        NewValue INT NULL,                       -- Chỉ số mới (nếu có)
        UnitPrice DECIMAL(18,0),                 -- Đơn giá snapshot
        Amount DECIMAL(18,0),                    -- Thành tiền
        IsFree BIT DEFAULT 0,                    -- Miễn phí
        Note NVARCHAR(500)
    );
    
    PRINT N'✓ Đã tạo bảng InvoiceDetails';
END
GO

-- ==============================================
-- 4. BẢNG XE (Vehicles)
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    CREATE TABLE Vehicles (
        VehicleId INT PRIMARY KEY IDENTITY(1,1),
        CustomerId INT FOREIGN KEY REFERENCES Customers(CustomerId),
        VehicleType NVARCHAR(20),                -- 'XeMay', 'XeDap', 'OTo'
        LicensePlate NVARCHAR(20),               -- Biển số
        Brand NVARCHAR(50),                      -- Hãng xe
        Color NVARCHAR(30),                      -- Màu
        MonthlyFee DECIMAL(18,0) DEFAULT 0,      -- Phí/tháng (0 = free)
        IsFree BIT DEFAULT 1,                    -- Miễn phí
        Note NVARCHAR(500),
        CreatedAt DATETIME DEFAULT GETDATE()
    );
    
    PRINT N'✓ Đã tạo bảng Vehicles';
END
GO

-- ==============================================
-- 5. BẢNG THU CHI (Transactions) - Sổ thu chi tổng hợp
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        TransactionId INT PRIMARY KEY IDENTITY(1,1),
        TransactionDate DATE NOT NULL,
        TransactionType NVARCHAR(10) NOT NULL,   -- 'Thu' hoặc 'Chi'
        CategoryCode NVARCHAR(20),               -- 'TIEN_PHONG', 'TIEN_COC', 'SUA_CHUA', 'DIEN_CHUNG'...
        CategoryName NVARCHAR(100),              -- Tên loại
        Amount DECIMAL(18,0) NOT NULL,           -- Số tiền
        RoomId INT NULL,                         -- Liên quan phòng (nếu có)
        CustomerId INT NULL,                     -- Liên quan khách (nếu có)
        InvoiceId INT NULL,                      -- Liên quan hóa đơn (nếu có)
        Description NVARCHAR(500),               -- Mô tả
        PaymentMethod NVARCHAR(20),              -- 'TienMat', 'ChuyenKhoan', 'MoMo'...
        ReceivedBy NVARCHAR(100),                -- Người thu/chi
        Note NVARCHAR(500),
        CreatedAt DATETIME DEFAULT GETDATE(),
        CreatedBy NVARCHAR(50)
    );
    
    PRINT N'✓ Đã tạo bảng Transactions';
END
GO

-- ==============================================
-- 6. BẢNG LOẠI THU CHI (TransactionCategories)
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TransactionCategories')
BEGIN
    CREATE TABLE TransactionCategories (
        CategoryCode NVARCHAR(20) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        TransactionType NVARCHAR(10) NOT NULL,   -- 'Thu' hoặc 'Chi'
        IsSystem BIT DEFAULT 0,                  -- Hệ thống tạo, không xóa được
        SortOrder INT DEFAULT 0
    );
    
    INSERT INTO TransactionCategories VALUES
        -- Các khoản THU
        ('TIEN_PHONG', N'Thu tiền phòng', 'Thu', 1, 1),
        ('TIEN_COC', N'Thu tiền cọc', 'Thu', 1, 2),
        ('TIEN_DIEN', N'Thu tiền điện', 'Thu', 1, 3),
        ('TIEN_NUOC', N'Thu tiền nước', 'Thu', 1, 4),
        ('TIEN_DICHVU', N'Thu tiền dịch vụ', 'Thu', 1, 5),
        ('THU_KHAC', N'Thu khác', 'Thu', 0, 99),
        
        -- Các khoản CHI
        ('SUA_CHUA', N'Chi sửa chữa', 'Chi', 1, 1),
        ('DIEN_CHUNG', N'Chi điện chung', 'Chi', 1, 2),
        ('NUOC_CHUNG', N'Chi nước chung', 'Chi', 1, 3),
        ('MUA_SAM', N'Chi mua sắm', 'Chi', 0, 4),
        ('THUE', N'Chi nộp thuế', 'Chi', 1, 5),
        ('CHI_KHAC', N'Chi khác', 'Chi', 0, 99);
    
    PRINT N'✓ Đã tạo bảng TransactionCategories';
END
GO

-- ==============================================
-- 7. BẢNG CÔNG NỢ (CustomerDebts) - View tổng hợp nợ
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_CustomerDebts')
BEGIN
    EXEC('
    CREATE VIEW vw_CustomerDebts AS
    SELECT 
        c.CustomerId,
        c.FullName,
        c.Phone,
        r.RoomId,
        r.RoomName,
        SUM(CASE WHEN i.Status = ''ChuaThu'' THEN i.TotalAmount ELSE 0 END) AS TotalDebt,
        COUNT(CASE WHEN i.Status = ''ChuaThu'' THEN 1 END) AS UnpaidInvoiceCount,
        MAX(i.CreatedAt) AS LastInvoiceDate
    FROM Customers c
    INNER JOIN Contracts ct ON c.CustomerId = ct.CustomerId AND ct.IsActive = 1
    INNER JOIN Rooms r ON ct.RoomId = r.RoomId
    LEFT JOIN Invoices i ON ct.ContractId = i.ContractId
    GROUP BY c.CustomerId, c.FullName, c.Phone, r.RoomId, r.RoomName
    ');
    
    PRINT N'✓ Đã tạo view vw_CustomerDebts';
END
GO

-- ==============================================
-- 8. BẢNG NGƯỜI Ở CÙNG (CoOccupants)
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CoOccupants')
BEGIN
    CREATE TABLE CoOccupants (
        OccupantId INT PRIMARY KEY IDENTITY(1,1),
        ContractId INT FOREIGN KEY REFERENCES Contracts(ContractId),
        FullName NVARCHAR(100) NOT NULL,
        CCCD NVARCHAR(20),
        Phone NVARCHAR(20),
        Relationship NVARCHAR(50),               -- 'VoChong', 'ConCai', 'BanBe'...
        DateOfBirth DATE,
        Gender NVARCHAR(10),
        Job NVARCHAR(100),
        Note NVARCHAR(500),
        CreatedAt DATETIME DEFAULT GETDATE()
    );
    
    PRINT N'✓ Đã tạo bảng CoOccupants';
END
GO

-- ==============================================
-- 9. BẢNG TÀI SẢN PHÒNG (RoomAssets)
-- ==============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RoomAssets')
BEGIN
    CREATE TABLE RoomAssets (
        AssetId INT PRIMARY KEY IDENTITY(1,1),
        RoomId INT FOREIGN KEY REFERENCES Rooms(RoomId),
        AssetName NVARCHAR(100) NOT NULL,        -- Tên tài sản
        AssetType NVARCHAR(50),                  -- 'DieuHoa', 'NongLanh', 'Giuong'...
        Brand NVARCHAR(50),                      -- Hãng
        Quantity INT DEFAULT 1,
        Condition NVARCHAR(20) DEFAULT 'Tot',    -- 'Moi', 'Tot', 'Cu', 'Hong'
        PurchaseDate DATE,
        PurchasePrice DECIMAL(18,0),
        Note NVARCHAR(500)
    );
    
    -- Dữ liệu mẫu
    INSERT INTO RoomAssets (RoomId, AssetName, AssetType, Brand, Quantity, Condition, Note) VALUES
        (2, N'Điều hòa', 'DieuHoa', 'Daikin', 1, 'Tot', N'1HP - Inverter'),
        (2, N'Bình nóng lạnh', 'NongLanh', 'Ariston', 1, 'Tot', N'20L'),
        (2, N'Giường', 'Giuong', NULL, 1, 'Tot', N'1m6'),
        (2, N'Tủ quần áo', 'TuAo', NULL, 1, 'Tot', N'2 cánh'),
        (6, N'Điều hòa', 'DieuHoa', 'LG', 1, 'Tot', N'1.5HP'),
        (6, N'Bình nóng lạnh', 'NongLanh', 'Panasonic', 1, 'Tot', NULL),
        (8, N'Điều hòa', 'DieuHoa', 'Samsung', 1, 'Tot', N'Mới lắp 2024');
    
    PRINT N'✓ Đã tạo bảng RoomAssets';
END
GO

-- ==============================================
-- 10. CẬP NHẬT BẢNG SETTINGS - Thêm cài đặt mới
-- ==============================================
IF NOT EXISTS (SELECT * FROM Settings WHERE SettingKey = 'TenChuTro')
BEGIN
    INSERT INTO Settings VALUES 
        ('TenChuTro', NULL, N'Tên chủ trọ'),
        ('SoDienThoai', NULL, N'Số điện thoại liên hệ'),
        ('DiaChi', NULL, N'Địa chỉ nhà trọ'),
        ('MaSoThue', NULL, N'Mã số thuế (nếu có)'),
        ('NgayThuTien', 5, N'Ngày thu tiền hàng tháng (1-28)'),
        ('SoNgayNhacNo', 3, N'Nhắc nợ trước bao nhiêu ngày'),
        ('XeMayMienPhi', 1, N'Số xe máy miễn phí/phòng'),
        ('GiaDienBac2', 4500, N'Giá điện từ số 51 trở đi');
    
    PRINT N'✓ Đã thêm Settings mới';
END
GO

-- ==============================================
-- STORED PROCEDURE: Tính hóa đơn tự động
-- ==============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_CalculateInvoice')
    DROP PROCEDURE sp_CalculateInvoice;
GO

CREATE PROCEDURE sp_CalculateInvoice
    @RoomId INT,
    @Month INT,
    @Year INT,
    @ElecNew INT,
    @WaterNew INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ContractId INT, @RoomPrice DECIMAL(18,0);
    DECLARE @ElecOld INT = 0, @WaterOld INT = 0;
    DECLARE @ElecPrice DECIMAL(18,0), @WaterPrice DECIMAL(18,0);
    DECLARE @ElecAmount DECIMAL(18,0), @WaterAmount DECIMAL(18,0);
    DECLARE @OtherAmount DECIMAL(18,0) = 0;
    DECLARE @TotalAmount DECIMAL(18,0);
    
    -- Lấy hợp đồng đang active
    SELECT @ContractId = ContractId, @RoomPrice = MonthlyRent
    FROM Contracts WHERE RoomId = @RoomId AND IsActive = 1;
    
    IF @ContractId IS NULL
    BEGIN
        RAISERROR('Không tìm thấy hợp đồng cho phòng này', 16, 1);
        RETURN;
    END
    
    -- Lấy chỉ số cũ từ hóa đơn trước
    SELECT TOP 1 @ElecOld = ElecNew, @WaterOld = WaterNew
    FROM Invoices WHERE ContractId = @ContractId
    ORDER BY InvoiceId DESC;
    
    -- Lấy giá điện nước
    SELECT @ElecPrice = SettingValue FROM Settings WHERE SettingKey = 'GiaDien';
    SELECT @WaterPrice = SettingValue FROM Settings WHERE SettingKey = 'GiaNuoc';
    
    -- Tính tiền
    SET @ElecAmount = (@ElecNew - @ElecOld) * @ElecPrice;
    SET @WaterAmount = (@WaterNew - @WaterOld) * @WaterPrice;
    
    -- Tính các dịch vụ khác (không free)
    SELECT @OtherAmount = ISNULL(SUM(
        CASE 
            WHEN s.ServiceType = 'CoDinh' THEN ISNULL(rs.CustomPrice, s.UnitPrice)
            WHEN s.ServiceType = 'SoLuong' THEN ISNULL(rs.CustomPrice, s.UnitPrice) * ISNULL(rs.Quantity, 1)
            ELSE 0
        END
    ), 0)
    FROM Services s
    LEFT JOIN RoomServices rs ON s.ServiceId = rs.ServiceId AND rs.RoomId = @RoomId
    WHERE s.IsActive = 1 
      AND s.ServiceCode NOT IN ('DIEN', 'NUOC')
      AND ISNULL(rs.IsFreeOverride, s.IsFree) = 0
      AND ISNULL(rs.IsEnabled, 1) = 1;
    
    SET @TotalAmount = @RoomPrice + @ElecAmount + @WaterAmount + @OtherAmount;
    
    -- Trả về kết quả
    SELECT 
        @ContractId AS ContractId,
        @RoomPrice AS RoomPrice,
        @ElecOld AS ElecOld, @ElecNew AS ElecNew, @ElecPrice AS ElecPrice, @ElecAmount AS ElecAmount,
        @WaterOld AS WaterOld, @WaterNew AS WaterNew, @WaterPrice AS WaterPrice, @WaterAmount AS WaterAmount,
        @OtherAmount AS OtherAmount,
        @TotalAmount AS TotalAmount;
END
GO

PRINT N'✓ Đã tạo stored procedure sp_CalculateInvoice';
GO

-- ==============================================
-- DỮ LIỆU MẪU: Xe cộ
-- ==============================================
IF NOT EXISTS (SELECT * FROM Vehicles)
BEGIN
    INSERT INTO Vehicles (CustomerId, VehicleType, LicensePlate, Brand, Color, MonthlyFee, IsFree) VALUES
        (1, 'XeMay', '65F1-12345', 'Honda Wave', N'Đen', 0, 1),
        (2, 'XeMay', '65B2-23456', 'Yamaha Exciter', N'Đỏ', 0, 1),
        (2, 'XeDap', NULL, 'Giant', N'Xanh', 0, 1),
        (3, 'XeMay', '65C1-34567', 'Honda SH', N'Trắng', 50000, 0),  -- Thu phí vì xe thứ 2
        (4, 'XeMay', '65D1-45678', 'Vespa', N'Hồng', 0, 1),
        (5, 'XeMay', '65E1-56789', 'Honda Vision', N'Xám', 0, 1);
    
    PRINT N'✓ Đã thêm dữ liệu mẫu Vehicles';
END
GO

-- ==============================================
-- HOÀN TẤT
-- ==============================================
PRINT N'';
PRINT N'============================================';
PRINT N'  DATABASE CẬP NHẬT THÀNH CÔNG - V2.0';
PRINT N'============================================';
PRINT N'  Đã thêm:';
PRINT N'  ✓ Services - Quản lý dịch vụ linh hoạt';
PRINT N'  ✓ RoomServices - Dịch vụ từng phòng';
PRINT N'  ✓ InvoiceDetails - Chi tiết hóa đơn';
PRINT N'  ✓ Vehicles - Quản lý xe';
PRINT N'  ✓ Transactions - Sổ thu chi';
PRINT N'  ✓ TransactionCategories - Loại thu chi';
PRINT N'  ✓ CoOccupants - Người ở cùng';
PRINT N'  ✓ RoomAssets - Tài sản phòng';
PRINT N'  ✓ vw_CustomerDebts - View công nợ';
PRINT N'============================================';
GO
