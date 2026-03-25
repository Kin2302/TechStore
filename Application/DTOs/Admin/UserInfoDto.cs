namespace Application.DTOs.Admin
{
    public class UserInfoDto
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Role { get; set; } = "User";
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
