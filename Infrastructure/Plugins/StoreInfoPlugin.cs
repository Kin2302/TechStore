using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace TechStore.Infrastructure.Plugins
{
    /// <summary>
    /// Plugin cung cap thong tin cua hang va chinh sach
    /// </summary>
    public class StoreInfoPlugin
    {
        [KernelFunction("get_store_info")]
        [Description("Lay thong tin cua hang TechStore: dia chi, gio lam viec, so dien thoai, email, website. Goi khi khach hoi ve cua hang.")]
        public Task<string> GetStoreInfoAsync()
        {
            var storeInfo = new
            {
                success = true,
                store = new
                {
                    name = "TechStore",
                    description = "Cua hang linh kien dien tu hang dau Viet Nam",
                    address = "123 Nguyen Van Linh, Quan 7, TP. Ho Chi Minh",
                    phone = "0123-456-789",
                    email = "contact@techstore.vn",
                    website = "https://techstore.vn",
                    workingHours = new
                    {
                        weekdays = "08:00 - 21:00 (Thu 2 - Thu 6)",
                        weekend = "09:00 - 20:00 (Thu 7 - Chu Nhat)",
                        holidays = "09:00 - 17:00"
                    },
                    socialMedia = new
                    {
                        facebook = "fb.com/TechStore",
                        zalo = "0123-456-789"
                    }
                },
                message = "TechStore - Cua hang linh kien dien tu. Lien he: 0123-456-789"
            };

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(storeInfo));
        }

        [KernelFunction("get_warranty_policy")]
        [Description("Lay thong tin chinh sach bao hanh, doi tra, va tra gop cua TechStore. Goi khi khach hoi ve bao hanh, doi tra, hoan tien.")]
        public Task<string> GetWarrantyPolicyAsync()
        {
            var warrantyInfo = new
            {
                success = true,
                policies = new
                {
                    warranty = new
                    {
                        title = "Chinh sach Bao hanh",
                        details = new[]
                        {
                            "Bao hanh 12 thang cho tat ca san pham chinh hang",
                            "Bao hanh 6 thang cho linh kien le va phu kien",
                            "Bao hanh 1-doi-1 trong 7 ngay dau neu loi tu nha san xuat",
                            "Ho tro bao hanh tai cua hang hoac gui qua buu dien",
                            "Thoi gian xu ly bao hanh: 3-7 ngay lam viec"
                        }
                    },
                    returnPolicy = new
                    {
                        title = "Chinh sach Doi tra",
                        details = new[]
                        {
                            "Doi tra mien phi trong 7 ngay dau mua hang",
                            "San pham doi tra phai con nguyen tem, hop, phu kien",
                            "Hoan tien 100% neu loi tu nha san xuat",
                            "Khong ap dung doi tra voi san pham da qua su dung hoac hu hong do nguoi dung"
                        }
                    },
                    installment = new
                    {
                        title = "Chinh sach Tra gop",
                        details = new[]
                        {
                            "Ho tro tra gop 0% lai suat qua the tin dung (don tu 2 trieu)",
                            "Tra gop qua cong ty tai chinh: 6-12 thang",
                            "Chi can CCCD/CMND + Bang lai xe hoac So ho khau",
                            "Duyet ho so nhanh trong 15 phut"
                        }
                    }
                },
                message = "TechStore bao hanh 12 thang, doi tra 7 ngay, ho tro tra gop 0%."
            };

            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(warrantyInfo));
        }
    }
}
