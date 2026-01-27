using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // ✅ TẠO ROLES
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ✅ TẠO HOẶC RESET ADMIN
            await CreateOrResetUserAsync(userManager, "admin@techstore.com", "Admin@123", "Admin");

            // ✅ TẠO HOẶC RESET USER
            await CreateOrResetUserAsync(userManager, "user@techstore.com", "User@123", "User");

            // Seed product data
            if (await context.Brands.AnyAsync()) return;
            await SeedProductDataAsync(context);
        }

        private static async Task CreateOrResetUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            else
            {
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                }

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, password);

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedProductDataAsync(ApplicationDbContext context)
        {
            // ==================== BRANDS ====================
            var brands = new Dictionary<string, Brand>
            {
                ["Arduino"] = new Brand { Name = "Arduino", Origin = "Italy", LogoUrl = "arduino-logo.png" },
                ["Espressif"] = new Brand { Name = "Espressif", Origin = "China", LogoUrl = "espressif-logo.png" },
                ["STMicroelectronics"] = new Brand { Name = "STMicroelectronics", Origin = "Switzerland", LogoUrl = "st-logo.png" },
                ["Raspberry Pi"] = new Brand { Name = "Raspberry Pi", Origin = "UK", LogoUrl = "rpi-logo.png" },
                ["Texas Instruments"] = new Brand { Name = "Texas Instruments", Origin = "USA", LogoUrl = "ti-logo.png" },
                ["Microchip"] = new Brand { Name = "Microchip", Origin = "USA", LogoUrl = "microchip-logo.png" },
                ["Adafruit"] = new Brand { Name = "Adafruit", Origin = "USA", LogoUrl = "adafruit-logo.png" },
                ["SparkFun"] = new Brand { Name = "SparkFun", Origin = "USA", LogoUrl = "sparkfun-logo.png" },
                ["Waveshare"] = new Brand { Name = "Waveshare", Origin = "China", LogoUrl = "waveshare-logo.png" },
                ["Generic"] = new Brand { Name = "Generic", Origin = "China", LogoUrl = "generic-logo.png" }
            };

            context.Brands.AddRange(brands.Values);
            await context.SaveChangesAsync();

            // ==================== CATEGORIES ====================
            var categories = new Dictionary<string, Category>
            {
                ["DevBoard"] = new Category { Name = "Board Phát Triển", Slug = "board-phat-trien", Description = "Các loại board vi điều khiển" },
                ["Sensor"] = new Category { Name = "Cảm Biến", Slug = "cam-bien", Description = "Module cảm biến các loại" },
                ["Display"] = new Category { Name = "Màn Hình & LED", Slug = "man-hinh-led", Description = "LCD, OLED, LED Matrix" },
                ["Motor"] = new Category { Name = "Động Cơ & Driver", Slug = "dong-co-driver", Description = "Motor, Servo, Stepper và driver" },
                ["Communication"] = new Category { Name = "Module Truyền Thông", Slug = "module-truyen-thong", Description = "WiFi, Bluetooth, RF, LoRa" },
                ["Power"] = new Category { Name = "Nguồn & Pin", Slug = "nguon-pin", Description = "Module nguồn, pin, sạc" },
                ["Relay"] = new Category { Name = "Relay & Công Tắc", Slug = "relay-cong-tac", Description = "Module relay, transistor switch" },
                ["Passive"] = new Category { Name = "Linh Kiện Thụ Động", Slug = "linh-kien-thu-dong", Description = "Điện trở, tụ điện, cuộn cảm" },
                ["Connector"] = new Category { Name = "Dây Nối & Connector", Slug = "day-noi-connector", Description = "Jumper wire, header, terminal" },
                ["Tool"] = new Category { Name = "Dụng Cụ & Phụ Kiện", Slug = "dung-cu-phu-kien", Description = "Mỏ hàn, đồng hồ, test board" },
                ["Kit"] = new Category { Name = "Bộ Kit Học Tập", Slug = "bo-kit-hoc-tap", Description = "Starter kit, project kit" },
                ["IC"] = new Category { Name = "IC & Vi Mạch", Slug = "ic-vi-mach", Description = "IC logic, op-amp, regulator" }
            };

            context.Categories.AddRange(categories.Values);
            await context.SaveChangesAsync();

            // ==================== PRODUCTS ====================
            var products = new List<Product>();

            // ---------- BOARD PHÁT TRIỂN (15 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Arduino Uno R3 (Chính hãng)", "ARD-UNO-R3", "arduino-uno-r3",
                    "Mạch lập trình phổ biến nhất cho người mới bắt đầu.",
                    450000, 100, brands["Arduino"].Id, categories["DevBoard"].Id,
                    new[] { ("Vi điều khiển", "ATmega328P"), ("Điện áp", "5V"), ("Digital I/O", "14 chân") }),

                CreateProduct("Arduino Nano V3", "ARD-NANO-V3", "arduino-nano-v3",
                    "Phiên bản nhỏ gọn của Arduino Uno, dùng chip CH340.",
                    85000, 200, brands["Arduino"].Id, categories["DevBoard"].Id,
                    new[] { ("Vi điều khiển", "ATmega328P"), ("Kích thước", "45x18mm") }),

                CreateProduct("Arduino Mega 2560 R3", "ARD-MEGA-2560", "arduino-mega-2560",
                    "Board Arduino mạnh mẽ với nhiều I/O cho dự án lớn.",
                    350000, 50, brands["Arduino"].Id, categories["DevBoard"].Id,
                    new[] { ("Vi điều khiển", "ATmega2560"), ("Digital I/O", "54 chân"), ("Flash", "256KB") }),

                CreateProduct("Arduino Leonardo", "ARD-LEONARDO", "arduino-leonardo",
                    "Board Arduino có thể giả lập bàn phím/chuột USB.",
                    280000, 40, brands["Arduino"].Id, categories["DevBoard"].Id,
                    new[] { ("Vi điều khiển", "ATmega32U4"), ("USB", "Native USB") }),

                CreateProduct("ESP32 DevKit V1", "ESP32-DEVKIT", "esp32-devkit-v1",
                    "Board phát triển WiFi + Bluetooth mạnh mẽ, giá rẻ.",
                    135000, 300, brands["Espressif"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "Dual-Core 240MHz"), ("WiFi", "2.4GHz"), ("Bluetooth", "BLE 4.2") }),

                CreateProduct("ESP8266 NodeMCU V3", "ESP8266-NODEMCU", "esp8266-nodemcu-v3",
                    "Board WiFi phổ biến nhất cho IoT, hỗ trợ Lua và Arduino.",
                    75000, 400, brands["Espressif"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "80MHz"), ("WiFi", "802.11 b/g/n"), ("Flash", "4MB") }),

                CreateProduct("ESP32-CAM", "ESP32-CAM", "esp32-cam",
                    "Board ESP32 tích hợp camera OV2640, hỗ trợ microSD.",
                    125000, 150, brands["Espressif"].Id, categories["DevBoard"].Id,
                    new[] { ("Camera", "OV2640 2MP"), ("WiFi", "2.4GHz"), ("MicroSD", "Có") }),

                CreateProduct("Raspberry Pi Pico", "RPI-PICO", "raspberry-pi-pico",
                    "Vi điều khiển RP2040 của Raspberry Pi, giá siêu rẻ.",
                    95000, 200, brands["Raspberry Pi"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "Dual-Core ARM Cortex-M0+"), ("RAM", "264KB"), ("Flash", "2MB") }),

                CreateProduct("Raspberry Pi Pico W", "RPI-PICO-W", "raspberry-pi-pico-w",
                    "Raspberry Pi Pico với WiFi và Bluetooth tích hợp.",
                    165000, 100, brands["Raspberry Pi"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "RP2040"), ("WiFi", "2.4GHz"), ("Bluetooth", "BLE 5.2") }),

                CreateProduct("STM32F103C8T6 Blue Pill", "STM32-BLUEPILL", "stm32-blue-pill",
                    "Board ARM Cortex-M3 giá rẻ, hiệu năng cao.",
                    55000, 250, brands["STMicroelectronics"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "ARM Cortex-M3 72MHz"), ("Flash", "64KB"), ("RAM", "20KB") }),

                CreateProduct("STM32F407VET6 Black Board", "STM32F407-BLACK", "stm32f407-black",
                    "Board ARM Cortex-M4 mạnh mẽ cho ứng dụng phức tạp.",
                    280000, 30, brands["STMicroelectronics"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "ARM Cortex-M4 168MHz"), ("Flash", "512KB"), ("RAM", "192KB") }),

                CreateProduct("Teensy 4.0", "TEENSY-40", "teensy-40",
                    "Board ARM siêu nhanh 600MHz, tương thích Arduino.",
                    750000, 20, brands["SparkFun"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "ARM Cortex-M7 600MHz"), ("Flash", "2MB"), ("RAM", "1MB") }),

                CreateProduct("Arduino Pro Mini 3.3V", "ARD-PROMINI-33", "arduino-pro-mini-33v",
                    "Board Arduino nhỏ gọn chạy 3.3V, không có USB.",
                    45000, 150, brands["Arduino"].Id, categories["DevBoard"].Id,
                    new[] { ("Vi điều khiển", "ATmega328P"), ("Điện áp", "3.3V"), ("Clock", "8MHz") }),

                CreateProduct("Arduino Pro Mini 5V", "ARD-PROMINI-5V", "arduino-pro-mini-5v",
                    "Board Arduino nhỏ gọn chạy 5V, cần adapter USB riêng.",
                    45000, 150, brands["Arduino"].Id, categories["DevBoard"].Id,
                    new[] { ("Vi điều khiển", "ATmega328P"), ("Điện áp", "5V"), ("Clock", "16MHz") }),

                CreateProduct("Seeeduino XIAO", "SEEED-XIAO", "seeeduino-xiao",
                    "Board siêu nhỏ với ARM Cortex-M0+, USB-C.",
                    120000, 80, brands["Generic"].Id, categories["DevBoard"].Id,
                    new[] { ("CPU", "ARM Cortex-M0+ 48MHz"), ("Flash", "256KB"), ("USB", "Type-C") })
            });

            // ---------- CẢM BIẾN (20 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Cảm biến nhiệt độ độ ẩm DHT11", "SEN-DHT11", "cam-bien-dht11",
                    "Cảm biến nhiệt độ độ ẩm giá rẻ cho người mới.",
                    25000, 500, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Nhiệt độ", "0-50°C ±2°C"), ("Độ ẩm", "20-90% ±5%") }),

                CreateProduct("Cảm biến nhiệt độ độ ẩm DHT22", "SEN-DHT22", "cam-bien-dht22",
                    "Phiên bản chính xác hơn của DHT11.",
                    65000, 300, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Nhiệt độ", "-40~80°C ±0.5°C"), ("Độ ẩm", "0-100% ±2%") }),

                CreateProduct("Cảm biến siêu âm HC-SR04", "SEN-HCSR04", "cam-bien-sieu-am-hcsr04",
                    "Đo khoảng cách bằng sóng siêu âm, phổ biến nhất.",
                    18000, 400, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Khoảng cách", "2cm - 400cm"), ("Độ chính xác", "±3mm") }),

                CreateProduct("Cảm biến hồng ngoại IR", "SEN-IR-OBSTACLE", "cam-bien-hong-ngoai-ir",
                    "Phát hiện vật cản bằng hồng ngoại, dùng cho robot.",
                    12000, 300, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Khoảng cách", "2-30cm"), ("Điện áp", "3.3-5V") }),

                CreateProduct("Module cảm biến chuyển động PIR HC-SR501", "SEN-PIR-SR501", "cam-bien-chuyen-dong-pir",
                    "Phát hiện chuyển động người, dùng cho hệ thống báo động.",
                    22000, 250, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Góc phát hiện", "120°"), ("Khoảng cách", "3-7m") }),

                CreateProduct("Cảm biến ánh sáng LDR Module", "SEN-LDR", "cam-bien-anh-sang-ldr",
                    "Module quang trở đo cường độ ánh sáng.",
                    10000, 400, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Output", "Digital + Analog"), ("Điện áp", "3.3-5V") }),

                CreateProduct("Cảm biến nhiệt độ DS18B20", "SEN-DS18B20", "cam-bien-nhiet-do-ds18b20",
                    "Cảm biến nhiệt độ 1-Wire chống nước.",
                    35000, 200, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Nhiệt độ", "-55°C ~ +125°C"), ("Độ chính xác", "±0.5°C") }),

                CreateProduct("Cảm biến gia tốc MPU6050", "SEN-MPU6050", "cam-bien-gia-toc-mpu6050",
                    "Module 6 trục: gia tốc kế + con quay hồi chuyển.",
                    45000, 150, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Giao tiếp", "I2C"), ("Gia tốc", "±2/4/8/16g"), ("Gyro", "±250/500/1000/2000°/s") }),

                CreateProduct("Cảm biến la bàn HMC5883L", "SEN-HMC5883L", "cam-bien-la-ban-hmc5883l",
                    "Module la bàn số 3 trục, dùng cho định hướng.",
                    55000, 100, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Giao tiếp", "I2C"), ("Độ phân giải", "1-2°") }),

                CreateProduct("Cảm biến áp suất BMP280", "SEN-BMP280", "cam-bien-ap-suat-bmp280",
                    "Đo áp suất khí quyển và nhiệt độ, tính độ cao.",
                    38000, 120, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Áp suất", "300-1100 hPa"), ("Nhiệt độ", "-40~85°C") }),

                CreateProduct("Cảm biến nhịp tim MAX30102", "SEN-MAX30102", "cam-bien-nhip-tim-max30102",
                    "Đo nhịp tim và SpO2 bằng hồng ngoại.",
                    85000, 80, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Giao tiếp", "I2C"), ("LED", "Red + IR") }),

                CreateProduct("Cảm biến gas MQ-2", "SEN-MQ2", "cam-bien-gas-mq2",
                    "Phát hiện khí gas LPG, propane, hydrogen.",
                    28000, 200, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Khí phát hiện", "LPG, Propane, H2"), ("Điện áp", "5V") }),

                CreateProduct("Cảm biến khói MQ-135", "SEN-MQ135", "cam-bien-khoi-mq135",
                    "Phát hiện chất lượng không khí, CO2, NH3.",
                    32000, 180, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Khí phát hiện", "CO2, NH3, Benzene"), ("Output", "Analog + Digital") }),

                CreateProduct("Cảm biến độ ẩm đất", "SEN-SOIL", "cam-bien-do-am-dat",
                    "Đo độ ẩm đất cho hệ thống tưới cây tự động.",
                    15000, 300, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Output", "Analog + Digital"), ("Điện áp", "3.3-5V") }),

                CreateProduct("Cảm biến mực nước", "SEN-WATER-LEVEL", "cam-bien-muc-nuoc",
                    "Đo mực nước trong bể, bồn chứa.",
                    12000, 250, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Chiều dài", "40mm"), ("Output", "Analog") }),

                CreateProduct("Cảm biến dòng điện ACS712 20A", "SEN-ACS712-20A", "cam-bien-dong-dien-acs712",
                    "Đo dòng điện AC/DC đến 20A.",
                    45000, 100, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Dòng điện", "±20A"), ("Độ nhạy", "100mV/A") }),

                CreateProduct("Cảm biến điện áp 0-25V", "SEN-VOLTAGE-25V", "cam-bien-dien-ap-25v",
                    "Module đo điện áp DC 0-25V cho Arduino.",
                    12000, 200, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Điện áp đo", "0-25V DC"), ("Output", "Analog") }),

                CreateProduct("Loadcell 5kg + HX711", "SEN-LOADCELL-5KG", "loadcell-5kg-hx711",
                    "Cảm biến trọng lượng 5kg kèm module ADC HX711.",
                    75000, 80, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Tải trọng", "5kg"), ("Độ chính xác", "±0.05%") }),

                CreateProduct("Cảm biến màu TCS3200", "SEN-TCS3200", "cam-bien-mau-tcs3200",
                    "Nhận diện màu sắc vật thể, dùng cho robot.",
                    55000, 60, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Output", "Frequency"), ("Màu", "RGB") }),

                CreateProduct("Cảm biến khoảng cách laser VL53L0X", "SEN-VL53L0X", "cam-bien-laser-vl53l0x",
                    "Đo khoảng cách chính xác bằng laser ToF.",
                    95000, 70, brands["Generic"].Id, categories["Sensor"].Id,
                    new[] { ("Khoảng cách", "3cm - 2m"), ("Giao tiếp", "I2C") })
            });

            // ---------- MÀN HÌNH & LED (12 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("LCD 16x2 xanh dương", "LCD-1602-BLUE", "lcd-16x2-xanh-duong",
                    "Màn hình LCD 16 ký tự x 2 dòng, nền xanh.",
                    45000, 200, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Ký tự", "16x2"), ("Backlight", "Xanh dương"), ("Giao tiếp", "Parallel") }),

                CreateProduct("Module I2C cho LCD 16x2", "LCD-I2C-ADAPTER", "module-i2c-lcd",
                    "Chuyển đổi LCD sang giao tiếp I2C, tiết kiệm chân.",
                    18000, 300, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Địa chỉ", "0x27 hoặc 0x3F"), ("Điện áp", "5V") }),

                CreateProduct("LCD 20x4 I2C", "LCD-2004-I2C", "lcd-20x4-i2c",
                    "Màn hình LCD 20x4 tích hợp sẵn module I2C.",
                    95000, 100, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Ký tự", "20x4"), ("Giao tiếp", "I2C"), ("Backlight", "Xanh dương") }),

                CreateProduct("OLED 0.96 inch I2C SSD1306", "OLED-096-I2C", "oled-096-inch-i2c",
                    "Màn hình OLED nhỏ gọn 128x64 pixel, hiển thị đẹp.",
                    65000, 250, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Độ phân giải", "128x64"), ("Giao tiếp", "I2C"), ("Màu", "Trắng/Xanh") }),

                CreateProduct("OLED 1.3 inch I2C SH1106", "OLED-13-I2C", "oled-13-inch-i2c",
                    "Màn hình OLED lớn hơn 128x64, chip SH1106.",
                    85000, 150, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Độ phân giải", "128x64"), ("Kích thước", "1.3 inch"), ("Giao tiếp", "I2C") }),

                CreateProduct("TFT LCD 1.8 inch SPI ST7735", "TFT-18-ST7735", "tft-lcd-18-inch",
                    "Màn hình TFT màu 128x160 pixel, giao tiếp SPI.",
                    75000, 120, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Độ phân giải", "128x160"), ("Màu", "65K màu"), ("Giao tiếp", "SPI") }),

                CreateProduct("TFT LCD 2.4 inch Touch ILI9341", "TFT-24-TOUCH", "tft-lcd-24-inch-touch",
                    "Màn hình TFT cảm ứng 320x240, dùng cho Arduino Shield.",
                    165000, 80, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Độ phân giải", "320x240"), ("Cảm ứng", "Resistive"), ("Giao tiếp", "SPI") }),

                CreateProduct("LED Matrix 8x8 MAX7219", "LED-MATRIX-8X8", "led-matrix-8x8-max7219",
                    "Module LED ma trận 8x8 với driver MAX7219.",
                    35000, 200, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("LED", "8x8 = 64 LED"), ("Driver", "MAX7219"), ("Giao tiếp", "SPI") }),

                CreateProduct("LED 7 đoạn 4 số TM1637", "LED-7SEG-TM1637", "led-7-doan-4-so-tm1637",
                    "Module LED 7 đoạn 4 chữ số với driver TM1637.",
                    28000, 250, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Số digit", "4"), ("Driver", "TM1637"), ("Màu", "Đỏ") }),

                CreateProduct("LED RGB WS2812B Strip 1m (60 LED)", "LED-WS2812B-1M", "led-rgb-ws2812b-strip",
                    "Dải LED RGB có thể lập trình từng LED, 60 LED/m.",
                    145000, 100, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Số LED", "60 LED/m"), ("Chip", "WS2812B"), ("Điện áp", "5V") }),

                CreateProduct("LED RGB Ring 12 LED WS2812", "LED-RING-12", "led-rgb-ring-12",
                    "Vòng LED RGB 12 LED cho hiệu ứng đẹp mắt.",
                    45000, 150, brands["Generic"].Id, categories["Display"].Id,
                    new[] { ("Số LED", "12"), ("Chip", "WS2812"), ("Đường kính", "37mm") }),

                CreateProduct("E-Paper Display 2.9 inch", "EPAPER-29", "e-paper-display-29-inch",
                    "Màn hình giấy điện tử tiết kiệm pin, 296x128.",
                    280000, 30, brands["Waveshare"].Id, categories["Display"].Id,
                    new[] { ("Độ phân giải", "296x128"), ("Màu", "Đen/Trắng"), ("Giao tiếp", "SPI") })
            });

            // ---------- ĐỘNG CƠ & DRIVER (12 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Động cơ Servo SG90", "MOTOR-SG90", "dong-co-servo-sg90",
                    "Servo mini 9g phổ biến cho robot và mô hình.",
                    28000, 400, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Góc quay", "0-180°"), ("Torque", "1.8 kg.cm"), ("Điện áp", "4.8-6V") }),

                CreateProduct("Động cơ Servo MG996R", "MOTOR-MG996R", "dong-co-servo-mg996r",
                    "Servo kim loại mạnh mẽ cho robot arm.",
                    85000, 150, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Góc quay", "0-180°"), ("Torque", "13 kg.cm"), ("Bánh răng", "Kim loại") }),

                CreateProduct("Động cơ DC giảm tốc 3-6V", "MOTOR-DC-GEARED", "dong-co-dc-giam-toc",
                    "Motor DC có hộp số, dùng cho xe robot.",
                    25000, 300, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Điện áp", "3-6V"), ("Tốc độ", "200 RPM"), ("Torque", "0.8 kg.cm") }),

                CreateProduct("Động cơ bước 28BYJ-48 + ULN2003", "MOTOR-28BYJ48", "dong-co-buoc-28byj48",
                    "Stepper motor nhỏ kèm driver ULN2003.",
                    45000, 200, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Góc bước", "5.625°"), ("Điện áp", "5V"), ("Tỉ số", "1:64") }),

                CreateProduct("Động cơ bước NEMA17", "MOTOR-NEMA17", "dong-co-buoc-nema17",
                    "Stepper motor tiêu chuẩn cho CNC, máy in 3D.",
                    145000, 80, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Góc bước", "1.8°"), ("Dòng điện", "1.7A"), ("Torque", "4.2 kg.cm") }),

                CreateProduct("Module Driver L298N", "DRIVER-L298N", "module-driver-l298n",
                    "Driver điều khiển 2 motor DC hoặc 1 stepper.",
                    45000, 250, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Dòng điện", "2A/kênh"), ("Điện áp", "5-35V"), ("Kênh", "2") }),

                CreateProduct("Module Driver L293D Shield", "DRIVER-L293D-SHIELD", "driver-l293d-shield",
                    "Shield Arduino điều khiển 4 DC motor hoặc 2 stepper.",
                    95000, 100, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Dòng điện", "0.6A/kênh"), ("Motor DC", "4"), ("Servo", "2") }),

                CreateProduct("Module Driver A4988", "DRIVER-A4988", "module-driver-a4988",
                    "Driver stepper motor với microstepping.",
                    25000, 200, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Dòng điện", "2A max"), ("Microstepping", "1/16"), ("Điện áp", "8-35V") }),

                CreateProduct("Module Driver DRV8825", "DRIVER-DRV8825", "module-driver-drv8825",
                    "Driver stepper nâng cao, microstepping 1/32.",
                    35000, 150, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Dòng điện", "2.5A max"), ("Microstepping", "1/32"), ("Điện áp", "8.2-45V") }),

                CreateProduct("Module Driver TB6612FNG", "DRIVER-TB6612", "module-driver-tb6612fng",
                    "Driver motor hiệu suất cao, thay thế L298N.",
                    55000, 120, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Dòng điện", "1.2A/kênh"), ("Điện áp", "2.5-13.5V"), ("Kênh", "2") }),

                CreateProduct("Quạt tản nhiệt 5V 40x40mm", "FAN-5V-40MM", "quat-tan-nhiet-5v-40mm",
                    "Quạt DC 5V cho tản nhiệt Raspberry Pi, ESP32.",
                    20000, 200, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Kích thước", "40x40x10mm"), ("Điện áp", "5V"), ("Tốc độ", "5000 RPM") }),

                CreateProduct("Bơm nước mini 3-6V", "PUMP-WATER-MINI", "bom-nuoc-mini-3-6v",
                    "Bơm chìm mini cho hệ thống tưới cây tự động.",
                    35000, 150, brands["Generic"].Id, categories["Motor"].Id,
                    new[] { ("Điện áp", "3-6V"), ("Lưu lượng", "120L/h"), ("Độ cao", "0.4m") })
            });

            // ---------- MODULE TRUYỀN THÔNG (10 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Module WiFi ESP8266 ESP-01", "WIFI-ESP01", "module-wifi-esp8266-esp01",
                    "Module WiFi nhỏ gọn, giá rẻ cho IoT.",
                    45000, 300, brands["Espressif"].Id, categories["Communication"].Id,
                    new[] { ("WiFi", "802.11 b/g/n"), ("GPIO", "2 chân"), ("Điện áp", "3.3V") }),

                CreateProduct("Module Bluetooth HC-05", "BT-HC05", "module-bluetooth-hc05",
                    "Bluetooth 2.0 SPP, có thể làm Master hoặc Slave.",
                    85000, 200, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Bluetooth", "2.0 + EDR"), ("Giao tiếp", "UART"), ("Khoảng cách", "10m") }),

                CreateProduct("Module Bluetooth HC-06", "BT-HC06", "module-bluetooth-hc06",
                    "Bluetooth 2.0 chế độ Slave only, giá rẻ hơn HC-05.",
                    65000, 250, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Bluetooth", "2.0"), ("Chế độ", "Slave"), ("Baud rate", "9600") }),

                CreateProduct("Module Bluetooth BLE HM-10", "BT-HM10", "module-bluetooth-ble-hm10",
                    "Bluetooth 4.0 BLE, tiết kiệm năng lượng.",
                    95000, 120, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Bluetooth", "4.0 BLE"), ("Giao tiếp", "UART"), ("Điện áp", "3.3V") }),

                CreateProduct("Module RF 433MHz TX + RX", "RF-433MHZ", "module-rf-433mhz",
                    "Cặp module thu phát RF 433MHz cho điều khiển từ xa.",
                    25000, 300, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Tần số", "433MHz"), ("Khoảng cách", "100m"), ("Điện áp", "5V") }),

                CreateProduct("Module NRF24L01", "RF-NRF24L01", "module-nrf24l01",
                    "Module RF 2.4GHz truyền dữ liệu tốc độ cao.",
                    35000, 200, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Tần số", "2.4GHz"), ("Tốc độ", "2Mbps"), ("Khoảng cách", "100m") }),

                CreateProduct("Module NRF24L01+PA+LNA", "RF-NRF24L01-PA", "module-nrf24l01-pa-lna",
                    "NRF24L01 có khuếch đại, phạm vi 1000m.",
                    85000, 100, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Tần số", "2.4GHz"), ("Công suất", "20dBm"), ("Khoảng cách", "1000m") }),

                CreateProduct("Module LoRa SX1278 433MHz", "LORA-SX1278", "module-lora-sx1278",
                    "Module LoRa truyền xa đến 10km.",
                    145000, 80, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Tần số", "433MHz"), ("Khoảng cách", "10km"), ("Giao tiếp", "SPI") }),

                CreateProduct("Module SIM800L GSM/GPRS", "GSM-SIM800L", "module-sim800l-gsm",
                    "Module GSM gửi SMS, gọi điện, GPRS.",
                    145000, 60, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Mạng", "2G GSM"), ("SIM", "Micro SIM"), ("Điện áp", "3.4-4.4V") }),

                CreateProduct("Module GPS NEO-6M", "GPS-NEO6M", "module-gps-neo-6m",
                    "Module GPS định vị vệ tinh với antenna.",
                    125000, 100, brands["Generic"].Id, categories["Communication"].Id,
                    new[] { ("Giao tiếp", "UART"), ("Độ chính xác", "2.5m"), ("Antenna", "Ceramic") })
            });

            // ---------- NGUỒN & PIN (10 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Module nguồn Breadboard MB102", "POWER-MB102", "module-nguon-breadboard-mb102",
                    "Cấp nguồn 3.3V/5V cho breadboard từ USB hoặc DC.",
                    25000, 300, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Output", "3.3V / 5V"), ("Input", "USB / 6.5-12V DC") }),

                CreateProduct("Module hạ áp LM2596 DC-DC", "POWER-LM2596", "module-ha-ap-lm2596",
                    "Buck converter điều chỉnh điện áp xuống 1.25-35V.",
                    28000, 250, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Input", "4-40V"), ("Output", "1.25-35V"), ("Dòng", "3A") }),

                CreateProduct("Module tăng áp MT3608 DC-DC", "POWER-MT3608", "module-tang-ap-mt3608",
                    "Boost converter tăng điện áp lên đến 28V.",
                    18000, 200, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Input", "2-24V"), ("Output", "5-28V"), ("Dòng", "2A") }),

                CreateProduct("Module sạc pin TP4056 Type-C", "CHARGER-TP4056", "module-sac-pin-tp4056",
                    "Mạch sạc pin Lithium 1S với bảo vệ.",
                    15000, 400, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Input", "5V USB-C"), ("Dòng sạc", "1A"), ("Bảo vệ", "Có") }),

                CreateProduct("Pin Lithium 18650 2600mAh", "BATTERY-18650", "pin-lithium-18650",
                    "Pin Li-ion 18650 dung lượng cao, có mạch bảo vệ.",
                    55000, 200, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Dung lượng", "2600mAh"), ("Điện áp", "3.7V"), ("Bảo vệ", "Có") }),

                CreateProduct("Đế pin 18650 1 cell", "HOLDER-18650-1", "de-pin-18650-1-cell",
                    "Đế đựng 1 pin 18650 có dây nối.",
                    8000, 300, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Số cell", "1"), ("Dây", "Có sẵn") }),

                CreateProduct("Đế pin 18650 2 cell nối tiếp", "HOLDER-18650-2S", "de-pin-18650-2-cell",
                    "Đế đựng 2 pin 18650 nối tiếp cho 7.4V.",
                    12000, 200, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Số cell", "2 nối tiếp"), ("Output", "7.4V") }),

                CreateProduct("Module nguồn AMS1117 3.3V", "POWER-AMS1117-33", "module-nguon-ams1117-33v",
                    "LDO regulator 3.3V 800mA cho ESP8266.",
                    8000, 400, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Output", "3.3V"), ("Dòng", "800mA"), ("Input", "4.5-12V") }),

                CreateProduct("Pin Lipo 3.7V 1000mAh", "BATTERY-LIPO-1000", "pin-lipo-37v-1000mah",
                    "Pin Lithium Polymer mỏng nhẹ có JST connector.",
                    65000, 100, brands["Generic"].Id, categories["Power"].Id,
                    new[] { ("Dung lượng", "1000mAh"), ("Điện áp", "3.7V"), ("Connector", "JST PH 2.0") }),

                CreateProduct("Module UPS cho Raspberry Pi", "UPS-RPI", "module-ups-raspberry-pi",
                    "Nguồn dự phòng cho Raspberry Pi với pin 18650.",
                    185000, 40, brands["Waveshare"].Id, categories["Power"].Id,
                    new[] { ("Pin", "2x 18650"), ("Output", "5V 3A"), ("UPS", "Có") })
            });

            // ---------- RELAY & CÔNG TẮC (8 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Module Relay 1 kênh 5V", "RELAY-1CH-5V", "module-relay-1-kenh-5v",
                    "Relay 1 kênh cách ly quang, điều khiển tải AC/DC.",
                    18000, 300, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Kênh", "1"), ("Điện áp cuộn", "5V"), ("Tải", "10A 250VAC") }),

                CreateProduct("Module Relay 2 kênh 5V", "RELAY-2CH-5V", "module-relay-2-kenh-5v",
                    "Relay 2 kênh cho smart home, IoT.",
                    28000, 250, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Kênh", "2"), ("Cách ly", "Optocoupler"), ("Tải", "10A 250VAC") }),

                CreateProduct("Module Relay 4 kênh 5V", "RELAY-4CH-5V", "module-relay-4-kenh-5v",
                    "Relay 4 kênh điều khiển nhiều thiết bị.",
                    55000, 150, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Kênh", "4"), ("Trigger", "Low level"), ("Tải", "10A 250VAC") }),

                CreateProduct("Module Relay 8 kênh 5V", "RELAY-8CH-5V", "module-relay-8-kenh-5v",
                    "Relay 8 kênh cho hệ thống tự động hóa lớn.",
                    95000, 80, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Kênh", "8"), ("Nguồn riêng", "Cần JD-VCC"), ("Tải", "10A 250VAC") }),

                CreateProduct("Module SSR Relay 1 kênh", "RELAY-SSR-1CH", "module-ssr-relay-1-kenh",
                    "Solid State Relay không tiếng ồn, đóng ngắt nhanh.",
                    45000, 100, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Loại", "Solid State"), ("Tải", "2A 240VAC"), ("Trigger", "3-32VDC") }),

                CreateProduct("Module MOSFET IRF520", "SWITCH-IRF520", "module-mosfet-irf520",
                    "Module công tắc MOSFET điều khiển PWM.",
                    15000, 200, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("MOSFET", "IRF520"), ("Tải", "24V 5A"), ("PWM", "Có") }),

                CreateProduct("Nút nhấn tự giữ 12mm", "BUTTON-LATCHING", "nut-nhan-tu-giu-12mm",
                    "Nút nhấn có giữ trạng thái ON/OFF.",
                    8000, 400, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Đường kính", "12mm"), ("Loại", "Latching"), ("Dòng", "2A") }),

                CreateProduct("Công tắc gạt ON-OFF-ON", "SWITCH-TOGGLE", "cong-tac-gat-on-off-on",
                    "Công tắc gạt 3 vị trí cho panel điều khiển.",
                    12000, 250, brands["Generic"].Id, categories["Relay"].Id,
                    new[] { ("Vị trí", "3 (ON-OFF-ON)"), ("Dòng", "6A 125VAC") })
            });

            // ---------- LINH KIỆN THỤ ĐỘNG (8 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Bộ điện trở 1/4W 30 giá trị (600 con)", "RESISTOR-KIT-600", "bo-dien-tro-14w-600-con",
                    "Bộ điện trở các giá trị từ 10Ω đến 1MΩ.",
                    45000, 100, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Số lượng", "600 con"), ("Giá trị", "30 loại"), ("Công suất", "1/4W") }),

                CreateProduct("Bộ tụ điện gốm 30 giá trị (300 con)", "CAPACITOR-KIT-CERAMIC", "bo-tu-dien-gom-300-con",
                    "Bộ tụ gốm từ 2pF đến 100nF.",
                    55000, 80, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Số lượng", "300 con"), ("Giá trị", "30 loại"), ("Loại", "Ceramic") }),

                CreateProduct("Bộ tụ hóa 12 giá trị (120 con)", "CAPACITOR-KIT-ELECTRO", "bo-tu-hoa-120-con",
                    "Bộ tụ hóa từ 1uF đến 470uF.",
                    35000, 100, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Số lượng", "120 con"), ("Giá trị", "12 loại"), ("Loại", "Electrolytic") }),

                CreateProduct("Biến trở 10K Potentiometer", "POT-10K", "bien-tro-10k-potentiometer",
                    "Biến trở xoay 10KΩ cho điều chỉnh tín hiệu.",
                    5000, 300, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Giá trị", "10KΩ"), ("Loại", "Rotary"), ("Chân", "3") }),

                CreateProduct("Bộ LED 5mm 5 màu (100 con)", "LED-KIT-5MM", "bo-led-5mm-100-con",
                    "Bộ LED 5mm các màu: đỏ, xanh lá, xanh dương, vàng, trắng.",
                    25000, 200, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Số lượng", "100 con"), ("Kích thước", "5mm"), ("Màu", "5 màu") }),

                CreateProduct("Bộ LED 3mm 5 màu (100 con)", "LED-KIT-3MM", "bo-led-3mm-100-con",
                    "Bộ LED 3mm nhỏ gọn cho các dự án compact.",
                    22000, 200, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Số lượng", "100 con"), ("Kích thước", "3mm"), ("Màu", "5 màu") }),

                CreateProduct("Buzzer 5V thụ động", "BUZZER-PASSIVE", "buzzer-5v-thu-dong",
                    "Còi passive phát nhiều tần số âm thanh.",
                    8000, 300, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Loại", "Passive"), ("Điện áp", "5V"), ("Tần số", "Tùy chỉnh") }),

                CreateProduct("Buzzer 5V chủ động", "BUZZER-ACTIVE", "buzzer-5v-chu-dong",
                    "Còi active phát âm thanh cố định khi cấp điện.",
                    10000, 300, brands["Generic"].Id, categories["Passive"].Id,
                    new[] { ("Loại", "Active"), ("Điện áp", "5V"), ("Tần số", "2300Hz") })
            });

            // ---------- DÂY NỐI & CONNECTOR (7 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Dây Jumper Đực-Đực 20cm (40 sợi)", "JUMPER-MM-20CM", "day-jumper-duc-duc-20cm",
                    "Dây nối breadboard Male-Male 40 sợi nhiều màu.",
                    25000, 300, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Loại", "Male-Male"), ("Chiều dài", "20cm"), ("Số sợi", "40") }),

                CreateProduct("Dây Jumper Đực-Cái 20cm (40 sợi)", "JUMPER-MF-20CM", "day-jumper-duc-cai-20cm",
                    "Dây nối Male-Female cho module và board.",
                    25000, 300, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Loại", "Male-Female"), ("Chiều dài", "20cm"), ("Số sợi", "40") }),

                CreateProduct("Dây Jumper Cái-Cái 20cm (40 sợi)", "JUMPER-FF-20CM", "day-jumper-cai-cai-20cm",
                    "Dây nối Female-Female cho sensor module.",
                    25000, 300, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Loại", "Female-Female"), ("Chiều dài", "20cm"), ("Số sợi", "40") }),

                CreateProduct("Breadboard 830 điểm", "BREADBOARD-830", "breadboard-830-diem",
                    "Board cắm linh kiện không hàn, chuẩn 830 điểm.",
                    35000, 200, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Điểm cắm", "830"), ("Kích thước", "165x55mm"), ("Màu", "Trắng") }),

                CreateProduct("Breadboard Mini 400 điểm", "BREADBOARD-400", "breadboard-mini-400-diem",
                    "Board cắm nhỏ gọn 400 điểm.",
                    18000, 300, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Điểm cắm", "400"), ("Kích thước", "85x55mm") }),

                CreateProduct("Header đực 40 pin 2.54mm", "HEADER-MALE-40", "header-duc-40-pin",
                    "Thanh header đực 40 pin, pitch 2.54mm.",
                    3000, 500, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Số pin", "40"), ("Pitch", "2.54mm"), ("Loại", "Male") }),

                CreateProduct("Header cái 40 pin 2.54mm", "HEADER-FEMALE-40", "header-cai-40-pin",
                    "Thanh header cái 40 pin để cắm module.",
                    5000, 500, brands["Generic"].Id, categories["Connector"].Id,
                    new[] { ("Số pin", "40"), ("Pitch", "2.54mm"), ("Loại", "Female") })
            });

            // ---------- DỤNG CỤ & PHỤ KIỆN (5 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Mỏ hàn điện 60W điều chỉnh nhiệt", "SOLDER-60W", "mo-han-60w-dieu-chinh-nhiet",
                    "Mỏ hàn 60W có núm chỉnh nhiệt độ 200-450°C.",
                    125000, 50, brands["Generic"].Id, categories["Tool"].Id,
                    new[] { ("Công suất", "60W"), ("Nhiệt độ", "200-450°C"), ("Đầu hàn", "Có thể thay") }),

                CreateProduct("Thiếc hàn 0.8mm 100g", "SOLDER-WIRE-08", "thiec-han-08mm-100g",
                    "Dây thiếc hàn 63/37 có nhựa thông.",
                    45000, 100, brands["Generic"].Id, categories["Tool"].Id,
                    new[] { ("Đường kính", "0.8mm"), ("Trọng lượng", "100g"), ("Thành phần", "63/37 Sn/Pb") }),

                CreateProduct("Đồng hồ vạn năng DT830B", "MULTIMETER-DT830B", "dong-ho-van-nang-dt830b",
                    "Đồng hồ đo điện áp, dòng điện, điện trở cơ bản.",
                    85000, 80, brands["Generic"].Id, categories["Tool"].Id,
                    new[] { ("Điện áp", "200mV-600V"), ("Dòng điện", "200µA-10A"), ("Điện trở", "200Ω-2MΩ") }),

                CreateProduct("Kính lúp hàn kẹp bàn", "MAGNIFIER-CLAMP", "kinh-lup-han-kep-ban",
                    "Kính lúp có đèn LED và tay kẹp giữ linh kiện.",
                    145000, 40, brands["Generic"].Id, categories["Tool"].Id,
                    new[] { ("Độ phóng đại", "2.5X / 7.5X / 10X"), ("Đèn", "LED"), ("Kẹp", "2 tay") }),

                CreateProduct("Bộ tua vít chính xác 25 in 1", "SCREWDRIVER-25IN1", "bo-tua-vit-chinh-xac-25in1",
                    "Bộ tua vít đa năng cho sửa chữa điện tử.",
                    65000, 60, brands["Generic"].Id, categories["Tool"].Id,
                    new[] { ("Số đầu", "25"), ("Loại", "Phillips, Torx, Hex, Flat"), ("Chất liệu", "CR-V") })
            });

            // ---------- BỘ KIT HỌC TẬP (5 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Arduino Starter Kit cơ bản", "KIT-ARDUINO-BASIC", "arduino-starter-kit-co-ban",
                    "Bộ kit học Arduino gồm Uno R3 + 30 linh kiện cơ bản.",
                    350000, 50, brands["Arduino"].Id, categories["Kit"].Id,
                    new[] { ("Board", "Arduino Uno R3"), ("Linh kiện", "30+ loại"), ("Tài liệu", "Có") }),

                CreateProduct("Arduino Starter Kit nâng cao", "KIT-ARDUINO-ADV", "arduino-starter-kit-nang-cao",
                    "Bộ kit đầy đủ với Mega 2560 + 100 linh kiện + LCD + sensor.",
                    650000, 30, brands["Arduino"].Id, categories["Kit"].Id,
                    new[] { ("Board", "Arduino Mega 2560"), ("Linh kiện", "100+ loại"), ("Tài liệu", "PDF + Video") }),

                CreateProduct("ESP32 IoT Kit", "KIT-ESP32-IOT", "esp32-iot-kit",
                    "Bộ kit học IoT với ESP32 + sensor + relay + OLED.",
                    450000, 40, brands["Espressif"].Id, categories["Kit"].Id,
                    new[] { ("Board", "ESP32 DevKit"), ("Linh kiện", "20+ loại"), ("Chủ đề", "IoT & Smart Home") }),

                CreateProduct("Raspberry Pi Pico Starter Kit", "KIT-PICO-STARTER", "raspberry-pi-pico-starter-kit",
                    "Bộ kit học MicroPython với Pico + sensor + LED.",
                    320000, 35, brands["Raspberry Pi"].Id, categories["Kit"].Id,
                    new[] { ("Board", "Raspberry Pi Pico"), ("Ngôn ngữ", "MicroPython"), ("Linh kiện", "25+ loại") }),

                CreateProduct("Bộ Kit Robot Car 4WD", "KIT-ROBOT-4WD", "bo-kit-robot-car-4wd",
                    "Khung xe robot 4 bánh + motor + driver + Arduino.",
                    285000, 45, brands["Generic"].Id, categories["Kit"].Id,
                    new[] { ("Khung", "Acrylic 2 tầng"), ("Motor", "4 DC"), ("Driver", "L298N") })
            });

            // ---------- IC & VI MẠCH (8 sản phẩm) ----------
            products.AddRange(new[]
            {
                CreateProduct("Module WiFi ESP32-WROOM-32", "ESP32-WROOM", "esp32-wifi-ble",
                    "Combo WiFi + Bluetooth cực mạnh dạng module.",
                    120000, 500, brands["Espressif"].Id, categories["IC"].Id,
                    new[] { ("CPU", "Dual-Core 32-bit"), ("WiFi", "2.4 GHz"), ("Bluetooth", "BLE 4.2") }),

                CreateProduct("IC ATmega328P-PU DIP-28", "IC-ATMEGA328P", "ic-atmega328p-dip28",
                    "Vi điều khiển ATmega328P dạng DIP, chip của Arduino Uno.",
                    55000, 100, brands["Microchip"].Id, categories["IC"].Id,
                    new[] { ("Flash", "32KB"), ("RAM", "2KB"), ("Chân", "DIP-28") }),

                CreateProduct("IC 555 Timer DIP-8", "IC-NE555", "ic-555-timer-dip8",
                    "IC tạo xung cổ điển, đa năng cho nhiều ứng dụng.",
                    3000, 500, brands["Texas Instruments"].Id, categories["IC"].Id,
                    new[] { ("Loại", "Timer"), ("Chân", "DIP-8"), ("Điện áp", "4.5-16V") }),

                CreateProduct("IC LM7805 Voltage Regulator", "IC-LM7805", "ic-lm7805-regulator",
                    "IC ổn áp tuyến tính 5V 1A, TO-220.",
                    5000, 400, brands["Texas Instruments"].Id, categories["IC"].Id,
                    new[] { ("Output", "5V"), ("Dòng", "1A"), ("Package", "TO-220") }),

                CreateProduct("IC LM7812 Voltage Regulator", "IC-LM7812", "ic-lm7812-regulator",
                    "IC ổn áp tuyến tính 12V 1A, TO-220.",
                    5000, 400, brands["Texas Instruments"].Id, categories["IC"].Id,
                    new[] { ("Output", "12V"), ("Dòng", "1A"), ("Package", "TO-220") }),

                CreateProduct("IC LM317 Adjustable Regulator", "IC-LM317", "ic-lm317-adjustable",
                    "IC ổn áp điều chỉnh được 1.25-37V.",
                    6000, 300, brands["Texas Instruments"].Id, categories["IC"].Id,
                    new[] { ("Output", "1.25-37V"), ("Dòng", "1.5A"), ("Package", "TO-220") }),

                CreateProduct("IC 74HC595 Shift Register", "IC-74HC595", "ic-74hc595-shift-register",
                    "IC mở rộng output 8-bit, giao tiếp SPI.",
                    5000, 300, brands["Texas Instruments"].Id, categories["IC"].Id,
                    new[] { ("Loại", "Shift Register"), ("Bit", "8"), ("Giao tiếp", "SPI") }),

                CreateProduct("IC PCF8574 I2C I/O Expander", "IC-PCF8574", "ic-pcf8574-io-expander",
                    "IC mở rộng 8 GPIO qua I2C.",
                    18000, 150, brands["Texas Instruments"].Id, categories["IC"].Id,
                    new[] { ("GPIO", "8"), ("Giao tiếp", "I2C"), ("Địa chỉ", "0x20-0x27") })
            });

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        // Helper method để tạo Product
        private static Product CreateProduct(
            string name, string code, string slug,
            string shortDesc, decimal price, int stock,
            int brandId, int categoryId,
            (string Name, string Value)[] specs)
        {
            return new Product
            {
                Name = name,
                Code = code,
                Slug = slug,
                ShortDescription = shortDesc,
                Description = $"<p>{shortDesc}</p>",
                Price = price,
                Stock = stock,
                BrandId = brandId,
                CategoryId = categoryId,
                Specifications = specs.Select(s => new ProductSpecification
                {
                    Name = s.Name,
                    Value = s.Value
                }).ToList(),
                Images = new List<ProductImage>
                {
                    new ProductImage
                    {
                        ImageUrl = $"https://via.placeholder.com/400x300?text={Uri.EscapeDataString(name.Split(' ')[0])}",
                        IsMain = true
                    }
                }
            };
        }
    }
}