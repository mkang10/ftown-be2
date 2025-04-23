namespace Application.Template
{
    public static class EmailTemplateBuilder
    {
        public static string BuildForgotPasswordEmail(string newPassword)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #000;'>
                    <div style='text-align: center;'>
                        <img src='https://res.cloudinary.com/dpbscvwv3/image/upload/v1744794172/71ecf1849f9887a5649c505595aa7586_tn_uxgitg.webp' style='width: 60px; height: 60px; border-radius: 50%; object-fit: cover;' alt='Logo'/>
                        <h2 style='margin: 10px 0;'>FUNKY TOWN</h2>
                        <p style='margin: 0;'>Địa chỉ: 26 Lý Tự Trọng, Bến Nghé, Quận 1, Hồ Chí Minh 700000</p>
                        <p style='margin: 0;'>SĐT: 093.990.5767</p>
                        <p style='margin: 0;'>Shopee: shope.ee/9pFQssAyaF</p>
                        <p style='margin: 0;'>Instagram: https://www.instagram.com/funkytown.gallery/</p>
                    </div>

                    <div style='margin-top: 30px;'>
                        <p>Xin chào,</p>
                        <p>Chúng mình là đội ngũ Chăm sóc khách hàng từ <strong>FUNKY TOWN</strong>.</p>
                        <p>Chúng mình xin gửi bạn mật khẩu đăng nhập mới:</p>
                        <p style='font-size: 18px; font-weight: bold;'>Mật khẩu: {newPassword}</p>
                        <p>Chúc bạn một ngày mới tốt lành!</p>
                    </div>
                </body>
                </html>
            ";
        }
    }
}
