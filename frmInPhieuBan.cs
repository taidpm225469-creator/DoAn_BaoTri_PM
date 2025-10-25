using CuahangNongduoc.BusinessObject;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CuahangNongduoc
{
    public partial class frmInPhieuBan : Form
    {
        // Ghi chú: Sử dụng 'private readonly' để đảm bảo phiếu bán không bị thay đổi sau khi form được tạo.
        // Quy tắc đặt tên _camelCase là chuẩn C# hiện đại cho biến private.
        private readonly PhieuBan _phieuBan;
        public frmInPhieuBan(CuahangNongduoc.BusinessObject.PhieuBan phieuBan)
        {
            InitializeComponent();

            // Ghi chú: Kiểm tra đầu vào ngay lập tức để tránh lỗi.
            // Nếu phieuBan là null, chương trình sẽ báo lỗi ngay thay vì sập ở một nơi khác.
            _phieuBan = phieuBan ?? throw new ArgumentNullException(nameof(phieuBan));

            // Ghi chú: Gán sự kiện xử lý cho subreport (danh sách sản phẩm).
            reportViewer.LocalReport.SubreportProcessing += LocalReport_SubreportProcessing;
        }

        void LocalReport_SubreportProcessing(object sender, Microsoft.Reporting.WinForms.SubreportProcessingEventArgs e)
        {
            // Ghi chú: Hàm này được gọi tự động khi report chính cần hiển thị subreport.
            // Ta cung cấp dữ liệu cho subreport tại đây (danh sách chi tiết sản phẩm).
            e.DataSources.Clear();
            var chiTietDataSource = new ReportDataSource("CuahangNongduoc_BusinessObject_ChiTietPhieuBan", _phieuBan.ChiTiet);
            e.DataSources.Add(chiTietDataSource);
        }
        private void frmInPhieuBan_Load(object sender, EventArgs e)
        {
            // Ghi chú: Dòng code cũ ExecuteReportInCurrentAppDomain đã được loại bỏ vì không còn cần thiết.
            try
            {
                // Xóa các nguồn dữ liệu cũ để đảm bảo báo cáo luôn mới.
                reportViewer.LocalReport.DataSources.Clear();

                // Ghi chú: Sử dụng ReportDataSource trực tiếp thay vì BindingSource.
                // Cách này nhất quán với cách xử lý subreport và các form báo cáo khác.
                // Tham số thứ hai yêu cầu một collection, vì vậy ta tạo một List chứa duy nhất phiếu bán của chúng ta.
                var phieuBanDataSource = new ReportDataSource("CuahangNongduoc_BusinessObject_PhieuBan", new List<PhieuBan> { _phieuBan }); reportViewer.LocalReport.DataSources.Add(phieuBanDataSource);

                // Ghi chú: Chuẩn bị các tham số để truyền vào báo cáo (tên cửa hàng, địa chỉ,...).
                CuaHang ch = ThamSo.LayCuaHang();
                var numConverter = new Num2Str();

                var reportParameters = new List<ReportParameter>
                {
                    new ReportParameter("ten_cua_hang", ch.TenCuaHang),
                    new ReportParameter("dia_chi", ch.DiaChi),
                    new ReportParameter("dien_thoai", ch.DienThoai),
                    new ReportParameter("bang_chu", numConverter.NumberToString(_phieuBan.TongTien.ToString()))
                };

                reportViewer.LocalReport.SetParameters(reportParameters);

                // Yêu cầu ReportViewer vẽ lại báo cáo với dữ liệu và tham số mới.
                reportViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                // Vòng lặp này sẽ tìm ra lỗi gốc, là nguyên nhân sâu xa nhất.
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }

                // Hiển thị thông báo lỗi gốc đó
                MessageBox.Show("Đã xảy ra lỗi khi tạo báo cáo:\n\n" + inner.Message, "Lỗi Chi Tiết", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}