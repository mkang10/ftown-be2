using Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Template
{
    public class EmailTemplateBuilder
    {
        public static string BuildInvoiceHtml(InvoiceForEmailDTO invoice)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html><body style='font-family: Arial, sans-serif; color: #000;'>");

            // Header: Logo + Company Info
            sb.AppendLine("<div style='display: flex; justify-content: space-between; align-items: center;'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<h2 style='margin: 0;'>FUNKY TOWN</h2>");
            sb.AppendLine("<p style='margin: 0;'>Địa chỉ: 26 Lý Tự Trọng, Bến Nghé, Quận 1, Hồ Chí Minh 700000<br/>SĐT: 093.990.5767<br/>Shopee: shope.ee/9pFQssAyaF<br/>Instagram: https://www.instagram.com/funkytown.gallery/</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div>");
            sb.AppendLine("<img src='https://res.cloudinary.com/dpbscvwv3/image/upload/v1744794172/71ecf1849f9887a5649c505595aa7586_tn_uxgitg.webp' style='width: 60px; height: 60px; border-radius: 50%; object-fit: cover;' alt='Logo'/>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<hr style='margin: 20px 0;'/>");

            sb.AppendLine($"<p><strong>Thời gian:</strong> {DateTime.Now:MM/dd/yyyy}</p>");

            // Customer Info
            sb.AppendLine("<div style='display: flex; justify-content: space-between;'>");

            sb.AppendLine("<div>");
            sb.AppendLine("<h3>Gửi đến:</h3>");
            sb.AppendLine($"<p>Tên: {invoice.FullName}<br/>Địa chỉ: {invoice.Address}<br/>SĐT: {invoice.PhoneNumber}<br/>Email: {invoice.Email}</p>");
            sb.AppendLine("</div>");

           
            sb.AppendLine("</div>");

            // Product Table
            sb.AppendLine("<h3>Hóa đơn chi tiết</h3>");
            sb.AppendLine("<table border='1' cellspacing='0' cellpadding='8' style='width: 100%; border-collapse: collapse;'>");
            sb.AppendLine("<thead style='background: #000; color: #fff;'>");
            sb.AppendLine("<tr><th>Tên sản phẩm</th><th>Size</th><th>Màu sắc</th><th>S.Lượng</th><th>Giá</th><th>Tổng</th></tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            decimal subTotal = 0;
            var culture = new CultureInfo("vi-VN");

            foreach (var item in invoice.OrderdetailEmail)
            {
                var p = item.Item;
                var itemTotal = item.Quantity * item.PriceAtPurchase;
                subTotal += itemTotal;

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{p.ProductId}</td>");
                sb.AppendLine($"<td>{p.SizeId}</td>");
                sb.AppendLine($"<td>{p.ColorId}</td>");
                sb.AppendLine($"<td>{item.Quantity}</td>");
                sb.AppendLine($"<td>{string.Format(culture, "{0:#,##0} ₫", item.PriceAtPurchase)}</td>");
                sb.AppendLine($"<td>{string.Format(culture, "{0:#,##0} ₫", itemTotal)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Totals Section
            sb.AppendLine("<div style='margin-top: 20px; display: flex; justify-content: flex-end;'>");
            sb.AppendLine("<table style='width: 300px;'>");
            sb.AppendLine($"<tr><td>Tạm tính:</td><td style='text-align:right;'>{string.Format(new CultureInfo("vi-VN"), "{0:#,##0} ₫", subTotal)}</td></tr>");
            sb.AppendLine("<tr><td>Giảm giá:</td><td style='text-align:right;'>0.00₫</td></tr>");
            sb.AppendLine($"<tr><td>Thuế (0%):</td><td style='text-align:right;'>0.00₫</td></tr>");
            sb.AppendLine("<tr><td>Phí vận chuyển:</td><td style='text-align:right;'>0.00₫</td></tr>");
            sb.AppendLine("<tr style='font-weight:bold;'><td>Tổng cộng:</td><td style='text-align:right;'>" + string.Format(new CultureInfo("vi-VN"), "{0:#,##0} ₫", subTotal) + "</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Balance Due
            sb.AppendLine("<div style='margin-top: 20px; text-align: right;'>");
            sb.AppendLine("<div style='background: #000; color: #fff; padding: 10px 20px; display: inline-block;'>");
            sb.AppendLine($"Tổng tiền phải trả: " + string.Format(new CultureInfo("vi-VN"), "{0:#,##0} ₫", subTotal));
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");

            return sb.ToString();
        }


    }
}
