
-- =====================================================
-- База данных VendingSystem
-- СУБД: PostgreSQL 15+
-- =====================================================

DROP SCHEMA IF EXISTS vending CASCADE;
CREATE SCHEMA vending;
SET search_path TO vending;

-- -----------------------------------------------------
-- 1. Справочники и основные сущности
-- -----------------------------------------------------

CREATE TABLE Companies (
    CompanyID SERIAL PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    INN VARCHAR(12) UNIQUE,
    Address TEXT,
    Phone VARCHAR(20),
    Email VARCHAR(100)
);

CREATE TABLE Modems (
    ModemID SERIAL PRIMARY KEY,
    IMEI VARCHAR(50) UNIQUE NOT NULL,
    Model VARCHAR(100),
    Provider VARCHAR(100),
    Status VARCHAR(50) DEFAULT 'Активен'
);

CREATE TABLE Users (
    UserID SERIAL PRIMARY KEY,
    FullName VARCHAR(200) NOT NULL,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Contacts TEXT,
    Role VARCHAR(50) NOT NULL CHECK (Role IN ('Администратор', 'Оператор', 'Инженер')),
    PasswordHash VARCHAR(255) NOT NULL DEFAULT '$2a$10$N9qo8uLOickgx2ZMRZoMy.MqrqBm0XjOcgJ1Qqj8z3W3yYVqQyD7C',
    PhotoUrl VARCHAR(500),
    TabNumber VARCHAR(50) UNIQUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Связь сотрудника с моделями ТА (для графика работ)
CREATE TABLE UserModels (
    UserModelID SERIAL PRIMARY KEY,
    UserID INT NOT NULL REFERENCES Users(UserID) ON DELETE CASCADE,
    ModelName VARCHAR(100) NOT NULL,
    UNIQUE(UserID, ModelName)
);

CREATE TABLE Products (
    ProductID SERIAL PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Price DECIMAL(10,2) NOT NULL CHECK (Price > 0),
    InStock INT NOT NULL DEFAULT 0 CHECK (InStock >= 0),
    MinStock INT NOT NULL DEFAULT 0 CHECK (MinStock >= 0),
    PropensityToSell DECIMAL(3,1)
);

-- -----------------------------------------------------
-- 2. Торговые автоматы (Vending Machines)
-- -----------------------------------------------------

CREATE TABLE Machines (
    MachineID SERIAL PRIMARY KEY,
    Location TEXT NOT NULL,
    Model VARCHAR(100) NOT NULL,
    PaymentType VARCHAR(50) NOT NULL CHECK (PaymentType IN ('с оплатой картой', 'с оплатой наличными', 'два вида оплаты')),
    FullIncome DECIMAL(15,2) DEFAULT 0 CHECK (FullIncome >= 0),
    SerialNumber VARCHAR(100) NOT NULL UNIQUE,
    InventoryNumber VARCHAR(100) NOT NULL UNIQUE,
    Manufacturer VARCHAR(200),
    ManufactureDate DATE NOT NULL,
    DateOfCommissioning DATE NOT NULL,
    LastVerificationDate DATE,
    VerificationInterval INT CHECK (VerificationInterval > 0),
    ResourceHours INT CHECK (ResourceHours > 0),
    DateOfNextFixing DATE,
    MaintenanceTimeHours INT CHECK (MaintenanceTimeHours BETWEEN 1 AND 20),
    MachineStatus VARCHAR(50) NOT NULL CHECK (MachineStatus IN ('Работает', 'Вышел из строя', 'В ремонте/на обслуживании')),
    Country VARCHAR(100),
    InventoryDate DATE,
    DateAdded DATE NOT NULL DEFAULT CURRENT_DATE,
    LastCheckedByUser INT REFERENCES Users(UserID),
    CompanyID INT REFERENCES Companies(CompanyID),
    ModemID INT REFERENCES Modems(ModemID),

    CONSTRAINT chk_commissioning_after_manufacture 
        CHECK (DateOfCommissioning >= ManufactureDate),
    CONSTRAINT chk_commissioning_before_added 
        CHECK (DateOfCommissioning <= DateAdded),
    CONSTRAINT chk_lastverify_after_manufacture 
        CHECK (LastVerificationDate IS NULL OR LastVerificationDate >= ManufactureDate),
    CONSTRAINT chk_lastverify_not_future 
        CHECK (LastVerificationDate IS NULL OR LastVerificationDate <= CURRENT_DATE),
    CONSTRAINT chk_nextfixing_after_added 
        CHECK (DateOfNextFixing IS NULL OR DateOfNextFixing > DateAdded),
    CONSTRAINT chk_inventory_after_manufacture 
        CHECK (InventoryDate IS NULL OR InventoryDate >= ManufactureDate),
    CONSTRAINT chk_inventory_not_future 
        CHECK (InventoryDate IS NULL OR InventoryDate <= CURRENT_DATE)
);

CREATE OR REPLACE FUNCTION calc_next_fixing()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.LastVerificationDate IS NOT NULL AND NEW.VerificationInterval IS NOT NULL THEN
        NEW.DateOfNextFixing := NEW.LastVerificationDate + (NEW.VerificationInterval || ' months')::INTERVAL;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_calc_next_fixing
BEFORE INSERT OR UPDATE ON Machines
FOR EACH ROW
EXECUTE FUNCTION calc_next_fixing();

-- -----------------------------------------------------
-- 3. Продажи
-- -----------------------------------------------------

CREATE TABLE Sales (
    SaleID SERIAL PRIMARY KEY,
    MachineID INT NOT NULL REFERENCES Machines(MachineID),
    ProductID INT NOT NULL REFERENCES Products(ProductID),
    Quantity INT NOT NULL CHECK (Quantity > 0),
    SaleSum DECIMAL(12,2) NOT NULL CHECK (SaleSum >= 0),
    SaleDateTime TIMESTAMP NOT NULL,
    PaymentType VARCHAR(50) CHECK (PaymentType IN ('Карта', 'Наличные', 'QR-код'))
);

-- -----------------------------------------------------
-- 4. Обслуживание
-- -----------------------------------------------------

CREATE TABLE Maintenance (
    NoteID SERIAL PRIMARY KEY,
    MachineID INT NOT NULL REFERENCES Machines(MachineID),
    MaintenanceDate DATE NOT NULL,
    Description TEXT,
    Problems TEXT,
    DoneByUser INT REFERENCES Users(UserID),
    ProtocolID INT
);

-- -----------------------------------------------------
-- 5. Дополнительные сущности для критериев
-- -----------------------------------------------------

CREATE TABLE News (
    NewsID SERIAL PRIMARY KEY,
    Title VARCHAR(300),
    Content TEXT,
    PublishDate DATE DEFAULT CURRENT_DATE,
    CompanyID INT REFERENCES Companies(CompanyID)
);

CREATE TABLE Incassations (
    IncassationID SERIAL PRIMARY KEY,
    MachineID INT NOT NULL REFERENCES Machines(MachineID),
    Amount DECIMAL(12,2) NOT NULL CHECK (Amount >= 0),
    IncassationDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    DoneByUser INT REFERENCES Users(UserID)
);

CREATE TABLE ServiceRequests (
    RequestID SERIAL PRIMARY KEY,
    MachineID INT NOT NULL REFERENCES Machines(MachineID),
    UserID INT REFERENCES Users(UserID),
    RequestType VARCHAR(50) NOT NULL CHECK (RequestType IN ('Плановое', 'Авария')),
    Status VARCHAR(50) DEFAULT 'Новая' CHECK (Status IN ('Новая', 'В работе', 'Закрыта', 'Отменена')),
    Priority INT DEFAULT 1,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ScheduledDate DATE,
    Description TEXT
);

CREATE TABLE StatusHistory (
    HistoryID SERIAL PRIMARY KEY,
    EntityType VARCHAR(50) NOT NULL,
    EntityID INT NOT NULL,
    OldStatus VARCHAR(50),
    NewStatus VARCHAR(50) NOT NULL,
    ChangedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ChangedBy INT REFERENCES Users(UserID)
);

CREATE TABLE Protocols (
    ProtocolID SERIAL PRIMARY KEY,
    RequestID INT REFERENCES ServiceRequests(RequestID),
    MachineID INT NOT NULL REFERENCES Machines(MachineID),
    UserID INT REFERENCES Users(UserID),
    ProtocolType VARCHAR(50) CHECK (ProtocolType IN ('Плановое ТО', 'Аварийное')),
    Content JSONB,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PdfPath VARCHAR(500)
);

-- -----------------------------------------------------
-- 6. Импорт предоставленных данных
-- -----------------------------------------------------

INSERT INTO Companies (Name, Address) VALUES 
('ООО ВендТех', 'г. Москва'),
('АО КофеМаш', 'г. Москва'),
('ЗАО СнекВенд', 'г. Казань'),
('ООО АкваВенд', 'г. Екатеринбург'),
('ООО ТехноВенд', 'г. Новосибирск'),
('ИП МиниВенд', 'г. Сочи'),
('ООО ГорячийНапиток', 'г. Нижний Новгород'),
('АО ФрэшФудВенд', 'г. Самара'),
('ООО АйсВенд', 'г. Ростов-на-Дону'),
('ЗАО ПринтВенд', 'г. Владивосток');

INSERT INTO Modems (IMEI, Model, Provider) VALUES
('860000000000001', 'SIM800L', 'МТС'),
('860000000000002', 'SIM800L', 'Билайн'),
('860000000000003', 'Quectel M26', 'МегаФон'),
('860000000000004', 'SIM800C', 'Теле2'),
('860000000000005', 'Quectel EC200', 'МТС'),
('860000000000006', 'SIM800L', 'Билайн'),
('860000000000007', 'SIM800C', 'МегаФон'),
('860000000000008', 'Quectel M26', 'Теле2'),
('860000000000009', 'SIM800L', 'МТС'),
('860000000000010', 'Quectel EC200', 'Билайн');

INSERT INTO Users (UserID, FullName, Contacts, Role, Email, Phone, PhotoUrl, TabNumber) VALUES
(1, 'Иванов Алексей Петрович', 'alex.ivanov@example.com, +7 916 123-45-67', 'Администратор', 'alex.ivanov@example.com', '+79161234567', '/images/users/1.jpg', 'T001'),
(2, 'Петрова Мария Ивановна', 'maria.petrova@mail.ru, +7 903 234-56-78', 'Оператор', 'maria.petrova@mail.ru', '+79032345678', '/images/users/2.jpg', 'T002'),
(3, 'Сидоров Дмитрий Викторович', 'dmitry.sidorov@yandex.ru, +7 926 345-67-89', 'Оператор', 'dmitry.sidorov@yandex.ru', '+79263456789', '/images/users/3.jpg', 'T003'),
(4, 'Кузнецова Елена Павловна', 'elena.kuznetsova@gmail.com, +7 915 456-78-90', 'Оператор', 'elena.kuznetsova@gmail.com', '+79154567890', '/images/users/4.jpg', 'T004'),
(5, 'Морозов Роман Николаевич', 'roman.morozov@company.org, +7 909 567-89-01', 'Администратор', 'roman.morozov@company.org', '+79095678901', '/images/users/5.jpg', 'T005'),
(6, 'Волкова Татьяна Леонидовна', 'tatyana.volkova@example.net, +7 925 678-90-12', 'Оператор', 'tatyana.volkova@example.net', '+79256789012', '/images/users/6.jpg', 'T006'),
(7, 'Алексеев Сергей Михайлович', 'sergey.alekseev@biz.ru, +7 910 789-01-23', 'Оператор', 'sergey.alekseev@biz.ru', '+79107890123', '/images/users/7.jpg', 'T007'),
(8, 'Никитина Ольга Александровна', 'olga.nikitina@proton.me, +7 905 890-12-34', 'Оператор', 'olga.nikitina@proton.me', '+79058901234', '/images/users/8.jpg', 'T008'),
(9, 'Фёдоров Игорь Борисович', 'igor.fedorov@outlook.com, +7 927 901-23-45', 'Администратор', 'igor.fedorov@outlook.com', '+79279012345', '/images/users/9.jpg', 'T009'),
(10, 'Григорьева Наталья Константиновна', 'natalia.grigorieva@mail.com, +7 901 012-34-56', 'Администратор', 'natalia.grigorieva@mail.com', '+79010123456', '/images/users/10.jpg', 'T010');

INSERT INTO UserModels (UserID, ModelName) VALUES
(1, 'VendCore X-200'), (1, 'CoffeeMaster Pro 500'),
(2, 'CoffeeMaster Pro 500'), (2, 'QuickBite Mini 100'),
(3, 'SnackVend S-300'), (3, 'FreshFood Vend 700'),
(4, 'AquaVend Water 2025'), (4, 'HotDrink Station 600'),
(5, 'VendoTech Elite 400'), (5, 'Print&Go Kiosk 150'),
(6, 'QuickBite Mini 100'), (6, 'IceCream Vend 250'),
(7, 'HotDrink Station 600'), (7, 'VendCore X-200'),
(8, 'FreshFood Vend 700'), (8, 'SnackVend S-300'),
(9, 'IceCream Vend 250'), (9, 'AquaVend Water 2025'),
(10, 'Print&Go Kiosk 150'), (10, 'VendoTech Elite 400');

INSERT INTO Products (ProductID, Name, Description, Price, InStock, MinStock, PropensityToSell) VALUES
(1, 'Кофе «Эспрессо»', 'Эспрессо из 100 % арабики, без добавок. Объём: 250 мл', 120, 18, 5, 3.5),
(2, 'Чипсы «Сыр & Лук»', 'Картофельные чипсы с ароматом сыра и лука. Без ГМО', 95, 25, 8, 2.1),
(3, 'Вода минеральная негазированная', 'Природная минеральная вода, низкоминерализованная. Без газа', 60, 40, 10, 4.8),
(4, 'Шоколадный батончик «Ореховый восторг»', 'Молочный шоколад с цельным фундуком и карамельной начинкой', 85, 30, 7, 1.9),
(5, 'Газированный напиток «Кола»', 'Газированный напиток со вкусом колы, с кофеином', 75, 22, 6, 2.7),
(6, 'Смесь орехов «Классика»', 'Смесь миндаля, фундука и грецкого ореха, слегка подсоленная', 150, 15, 4, 1.2),
(7, 'Леденцы «Мятные»', 'Мятные леденцы без сахара, с натуральным ароматизатором', 45, 50, 12, 5.3),
(8, 'Попкорн «Сливочный»', 'Воздушный попкорн со сливочным маслом и солью', 70, 28, 9, 1.8),
(9, 'Энергетический напиток «Turbo»', 'Энергетический напиток с таурином, кофеином и витаминами группы B', 130, 12, 3, 2.4);

INSERT INTO Machines (MachineID, Location, Model, PaymentType, FullIncome, SerialNumber, InventoryNumber, Manufacturer, ManufactureDate, DateOfCommissioning, LastVerificationDate, VerificationInterval, ResourceHours, DateOfNextFixing, MaintenanceTimeHours, MachineStatus, Country, InventoryDate, LastCheckedByUser, CompanyID, ModemID) VALUES
(1, 'г. Санкт-Петербург, Невский пр., д. 50, ТЦ «Галерея», 2-й этаж.', 'VendCore X-200', 'с оплатой картой', 1250000, 'SC123456789', 'INV-2025-001', 'ООО «ВендТех»', '2025-05-01', '2025-05-10', '2025-06-01', 6, 2500, '2025-12-01', 4, 'Работает', 'Россия', '2025-07-15', 1, 1, 1),
(2, 'Московская обл., г. Химки, ул. Московская, д. 15, офис 301.', 'CoffeeMaster Pro 500', 'с оплатой наличными', 1250000, 'SN987654321', 'INV-2025-002', 'АО «КофеМаш»', '2025-06-15', '2025-06-20', '2025-07-20', 12, 1800, '2026-07-20', 8, 'Вышел из строя', 'Китай', '2025-08-10', 2, 2, 2),
(3, 'г. Казань, ул. Баумана, д. 20, кафетерий', 'SnackVend S-300', 'два вида оплаты', 1250000, 'VCX200-001', 'INV-2025-003', 'ЗАО «СнекВенд»', '2025-07-20', '2025-07-22', '2025-08-01', 24, 1801, '2026-08-01', 12, 'В ремонте/на обслуживании', 'Германия', '2025-09-15', 3, 3, 3),
(4, 'г. Екатеринбург, ул. Ленина, д. 50, холл бизнес-центра.', 'AquaVend Water 2025', 'с оплатой картой', 1250000, 'CM500-PRO-002', 'INV-2025-004', 'ООО «АкваВенд»', '2025-08-10', '2025-08-15', '2025-10-15', 18, 1802, '2026-04-15', 6, 'Работает', 'Южная Корея', '2025-09-15', 4, 4, 4),
(5, 'г. Новосибирск, Красный пр., д. 100, университетский кампус.', 'VendoTech Elite 400', 'два вида оплаты', 1250000, 'SV300-SN003', 'INV-2025-005', 'ООО «ТехноВенд»', '2025-09-25', '2025-09-30', '2025-10-01', 36, 1803, '2026-10-01', 16, 'В ремонте/на обслуживании', 'США', '2025-10-20', 5, 5, 5),
(6, 'г. Сочи, Курортный пр., д. 70, отель «Морская звезда», лобби.', 'QuickBite Mini 100', 'с оплатой наличными', 1250000, 'AW2025-004', 'INV-2025-006', 'ИП «МиниВенд»', '2025-10-05', '2025-10-10', '2025-11-20', 12, 1804, '2026-11-20', 10, 'Вышел из строя', 'Италия', '2025-11-20', 6, 6, 6),
(7, 'г. Нижний Новгород, ул. Большая Покровская, д. 40, торговый пассаж.', 'HotDrink Station 600', 'с оплатой картой', 1250000, 'VT400-ELT-005', 'INV-2025-007', 'ООО «ГорячийНапиток»', '2025-11-12', '2025-11-15', '2025-12-20', 6, 1805, '2026-06-20', 3, 'Работает', 'Турция', '2025-12-20', 7, 7, 7),
(8, 'г. Самара, ул. Молодогвардейская, д. 120, ТЦ «Мега».', 'FreshFood Vend 700', 'два вида оплаты', 1250000, 'QB100-MIN-006', 'INV-2025-008', 'АО «ФрэшФудВенд»', '2025-12-18', '2025-12-20', '2026-01-05', 24, 1806, '2027-01-05', 18, 'В ремонте/на обслуживании', 'Япония', '2026-01-20', 8, 8, 8),
(9, 'г. Ростов-на-Дону, ул. Садовая, д. 80, административное здание.', 'IceCream Vend 250', 'с оплатой наличными', 1250000, 'HDS600-007', 'INV-2025-009', 'ООО «АйсВенд»', '2026-01-03', '2026-01-10', '2026-01-12', 18, 1807, '2026-07-12', 7, 'Вышел из строя', 'Польша', '2026-01-15', 9, 9, 9),
(10, 'г. Владивосток, ул. Светланская, д. 60, морской вокзал.', 'Print&Go Kiosk 150', 'с оплатой картой', 1250000, 'FF700-VND-008', 'INV-2025-010', 'ЗАО «ПринтВенд»', '2026-01-08', '2026-01-14', '2026-01-19', 12, 1808, '2027-01-19', 14, 'Работает', 'Тайвань', '2026-01-20', 10, 10, 10);

INSERT INTO Sales (SaleID, MachineID, ProductID, Quantity, SaleSum, SaleDateTime, PaymentType) VALUES
(1, 2, 9, 1, 120, '2026-01-22 08:15:30', 'Карта'),
(2, 5, 2, 3, 285, '2026-01-22 10:45:12', 'Наличные'),
(3, 9, 6, 2, 120, '2026-01-22 12:30:45', 'QR-код'),
(4, 8, 5, 1, 85, '2026-01-22 14:20:05', 'Карта'),
(5, 6, 7, 4, 300, '2026-01-22 16:55:22', 'Наличные'),
(6, 1, 1, 1, 150, '2026-01-22 18:03:17', 'QR-код'),
(7, 3, 4, 5, 225, '2026-01-22 19:40:50', 'Карта'),
(8, 10, 2, 2, 140, '2026-01-22 21:10:33', 'Наличные'),
(9, 4, 8, 1, 130, '2026-01-22 22:50:47', 'QR-код'),
(10, 7, 7, 3, 165, '2026-01-22 23:59:01', 'Карта');

INSERT INTO Maintenance (NoteID, MachineID, MaintenanceDate, Description, Problems, DoneByUser) VALUES
(1, 3, '2026-01-22', 'Плановое ТО: очистка камер, проверка датчиков, смазка механизмов', 'Загрязнение датчиков наличия товара, ложные срабатывания', 1),
(2, 2, '2026-01-21', 'Пополнение запасов: загружены 50 шт. воды, 30 шт. снеков', 'Низкий уровень запасов: осталось 5 бутылок воды, 2 батончика', 2),
(3, 1, '2026-01-20', 'Замена вышедшего из строя дисплея управления', 'Экран не реагирует на касания, вероятный обрыв шлейфа', 3),
(4, 7, '2026-01-19', 'Чистка системы подачи напитков, промывка трубок', 'Протечка в системе подачи воды, износ уплотнителя', 4),
(5, 6, '2026-01-18', 'Обновление ПО до версии 2.1.5, перезагрузка системы', 'Ошибка связи с платёжным терминалом (код 105)', 5),
(6, 5, '2026-01-17', 'Регулировка механизма выдачи товара, калибровка сенсоров', 'Заедание механизма выдачи, скопление мусора в лотке', 6),
(7, 9, '2026-01-16', 'Замена аккумулятора резервного питания', 'Разряд резервного аккумулятора ниже 20 %', 7),
(8, 10, '2026-01-15', 'Пополнение монетного механизма, инкассация наличных', 'Некорректное отображение цен на экране (сбой кэша)', 8),
(9, 8, '2026-01-14', 'Установка нового модуля безналичной оплаты', 'Повреждение кабеля питания, оголение контактов', 9),
(10, 4, '2026-01-13', 'Проверка герметичности корпуса, устранение зазоров', 'Повышенный шум вентилятора охлаждения, износ подшипников', 10);

INSERT INTO News (Title, Content, PublishDate) VALUES
('Запуск новой сети в СПб', 'Открытие 5 новых торговых автоматов в ТЦ Галерея', '2026-01-20'),
('Обновление ПО', 'Версия 3.0 с поддержкой QR-кодов уже доступна', '2026-01-18'),
('Плановое обслуживание', 'С 25 по 27 января проводится плановое ТО в Сочи', '2026-01-15'),
('Новый продукт', 'В ассортименте появился энергетический напиток Turbo', '2026-01-10'),
('Расширение сети', 'До конца квартала планируется установка 20 новых ТА', '2026-01-05');

INSERT INTO Incassations (MachineID, Amount, IncassationDate, DoneByUser) VALUES
(1, 45000, '2026-01-20 09:00:00', 2),
(2, 32000, '2026-01-19 10:30:00', 3),
(3, 28000, '2026-01-18 11:00:00', 4),
(4, 51000, '2026-01-17 09:15:00', 5),
(5, 15000, '2026-01-16 14:00:00', 6),
(6, 22000, '2026-01-15 16:30:00', 7),
(7, 38000, '2026-01-14 10:00:00', 8),
(8, 12000, '2026-01-13 12:00:00', 9),
(9, 9000, '2026-01-12 15:00:00', 10),
(10, 41000, '2026-01-11 09:30:00', 1);

INSERT INTO StatusHistory (EntityType, EntityID, OldStatus, NewStatus, ChangedBy) VALUES
('Machine', 2, 'Работает', 'Вышел из строя', 1),
('Machine', 3, 'Работает', 'В ремонте/на обслуживании', 2),
('Machine', 6, 'Работает', 'Вышел из строя', 3);

INSERT INTO ServiceRequests (MachineID, UserID, RequestType, Status, ScheduledDate, Description) VALUES
(2, 2, 'Авария', 'Новая', '2026-01-23', 'Неисправность дисплея'),
(3, 3, 'Плановое', 'Новая', '2026-01-24', 'Плановое ТО'),
(6, 6, 'Авария', 'В работе', '2026-01-23', 'Ошибка связи с терминалом'),
(9, 9, 'Плановое', 'Новая', '2026-01-25', 'Замена аккумулятора');

-- Представление для Монитора ТА
CREATE OR REPLACE VIEW vw_MachineMonitor AS
SELECT 
    m.MachineID,
    m.Location,
    m.Model,
    m.PaymentType,
    m.MachineStatus,
    m.Country,
    m.CompanyID,
    c.Name as CompanyName,
    COALESCE(m.ModemID, -1) as ModemID,
    CASE 
        WHEN m.MachineStatus = 'Работает' THEN 'Стабильная'
        WHEN m.MachineStatus = 'В ремонте/на обслуживании' THEN 'Прервана'
        ELSE 'Отсутствует'
    END as ConnectionStatus,
    CASE 
        WHEN m.MachineStatus = 'Работает' THEN ROUND(50 + RANDOM()*49)::INT
        ELSE ROUND(RANDOM()*30)::INT
    END as LoadPercent,
    COALESCE(
        (SELECT SUM(s.SaleSum) FROM Sales s WHERE s.MachineID = m.MachineID AND s.SaleDateTime >= CURRENT_DATE - INTERVAL '7 days'),
        ROUND(RANDOM()*5000)::INT
    ) as MoneyInMachine,
    m.LastVerificationDate,
    m.DateOfNextFixing,
    m.ResourceHours
FROM Machines m
LEFT JOIN Companies c ON m.CompanyID = c.CompanyID;

-- Индексы
CREATE INDEX idx_sales_machine ON Sales(MachineID);
CREATE INDEX idx_sales_date ON Sales(SaleDateTime);
CREATE INDEX idx_maintenance_machine ON Maintenance(MachineID);
CREATE INDEX idx_machines_status ON Machines(MachineStatus);
CREATE INDEX idx_machines_company ON Machines(CompanyID);
