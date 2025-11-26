using AntdUI;

namespace QuanLyPhongTro
{
    public class VietnameseLocalization : ILocalization
    {
        public string GetLocalizedString(string key)
        {
            switch (key)
            {
                // DatePicker
                case "Year": return "Năm";
                case "Month": return "Tháng";
                case "Day": return "Ngày";
                case "Today": return "Hôm nay";
                case "Now": return "Bây giờ";
                case "OK": return "Đồng ý";
                case "Cancel": return "Hủy";
                case "Clear": return "Xóa";
                
                // Days of week
                case "Su": return "CN";
                case "Mo": return "T2";
                case "Tu": return "T3";
                case "We": return "T4";
                case "Th": return "T5";
                case "Fr": return "T6";
                case "Sa": return "T7";
                
                case "Sunday": return "Chủ nhật";
                case "Monday": return "Thứ hai";
                case "Tuesday": return "Thứ ba";
                case "Wednesday": return "Thứ tư";
                case "Thursday": return "Thứ năm";
                case "Friday": return "Thứ sáu";
                case "Saturday": return "Thứ bảy";
                
                // Months
                case "January": return "Tháng 1";
                case "February": return "Tháng 2";
                case "March": return "Tháng 3";
                case "April": return "Tháng 4";
                case "May": return "Tháng 5";
                case "June": return "Tháng 6";
                case "July": return "Tháng 7";
                case "August": return "Tháng 8";
                case "September": return "Tháng 9";
                case "October": return "Tháng 10";
                case "November": return "Tháng 11";
                case "December": return "Tháng 12";
                
                case "Jan": return "Th1";
                case "Feb": return "Th2";
                case "Mar": return "Th3";
                case "Apr": return "Th4";
                case "Jun": return "Th6";
                case "Jul": return "Th7";
                case "Aug": return "Th8";
                case "Sep": return "Th9";
                case "Oct": return "Th10";
                case "Nov": return "Th11";
                case "Dec": return "Th12";
                
                // Table
                case "NoData": return "Không có dữ liệu";
                case "Filter": return "Lọc";
                case "Reset": return "Đặt lại";
                case "SortAsc": return "Sắp xếp tăng dần";
                case "SortDesc": return "Sắp xếp giảm dần";
                
                // Message
                case "Info": return "Thông tin";
                case "Success": return "Thành công";
                case "Warning": return "Cảnh báo";
                case "Error": return "Lỗi";
                
                // Input
                case "PleaseInput": return "Vui lòng nhập";
                case "PleaseSelect": return "Vui lòng chọn";
                
                // Pagination
                case "Total": return "Tổng";
                case "Items": return "mục";
                case "Page": return "Trang";
                case "GoTo": return "Đến";
                
                default: return key;
            }
        }
    }
}
