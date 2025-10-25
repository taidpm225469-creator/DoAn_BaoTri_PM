using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CuahangNongduoc.BusinessObject;
using CuahangNongduoc.Controller;

namespace CuahangNongduoc
{
    public partial class frmDanhsachPhieuBanSi : Form
    {
        // Khai báo các controller ở cấp lớp để tái sử dụng
        private readonly PhieuBanController ctrl = new PhieuBanController();
        private readonly KhachHangController ctrlKH = new KhachHangController();
        private readonly ChiTietPhieuBanController ctrlChiTiet = new ChiTietPhieuBanController();
        private frmBanSi BanSi = null;
        public frmDanhsachPhieuBanSi()
        {
            InitializeComponent();
        }


        // xóa đổi tên thôi
        //private void frmDanhsachPhieuNhap_Load(object sender, EventArgs e)
        //{
        //    ctrlKH.HienthiDaiLyDataGridviewComboBox(colKhachhang);
        //    ctrl.HienthiPhieuBanSi(bindingNavigator, dataGridView);
        //}
        // Phương thức chung để mở hoặc kích hoạt form Bán Sỉ
        private void MoHoacKichHoatFormBanSi(string idPhieuBan = null)
        {
            if (BanSi == null || BanSi.IsDisposed)
            {
                // Nếu có idPhieuBan, gọi constructor để xem/sửa, ngược lại là tạo mới
                BanSi = (idPhieuBan != null) ? new frmBanSi() : new frmBanSi();
                BanSi.Show();
            }
            else
            {
                BanSi.Activate();
            }
        }
        private void dataGridView_DoubleClick(object sender, EventArgs e)
        {
            // SỬA: DoubleClick giờ sẽ mở phiếu đã chọn để xem/sửa
            DataRowView currentRow = (DataRowView)bindingNavigator.BindingSource.Current;
            if (currentRow != null)
            {
                string idPhieu = currentRow["ID"].ToString();
                MoHoacKichHoatFormBanSi(idPhieu);
            }
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            // Gọi phương thức chung để tạo phiếu mới
            MoHoacKichHoatFormBanSi();
        }
        // 2. TỐI ƯU: Phương thức chung cho việc xóa, gộp logic từ hai nơi
        private void XoaPhieuBanHienTai()
        {
            DataRowView currentRow = (DataRowView)bindingNavigator.BindingSource.Current;
            if (currentRow == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hỏi xác nhận trước khi xóa
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu này?\nHành động này sẽ hoàn lại số lượng sản phẩm vào kho.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string idPhieu = currentRow["ID"].ToString();

                    // Lấy danh sách chi tiết và hoàn lại số lượng sản phẩm vào kho
                    IList<ChiTietPhieuBan> dsChiTiet = ctrlChiTiet.ChiTietPhieuBan(idPhieu);
                    foreach (ChiTietPhieuBan ct in dsChiTiet)
                    {
                        // Giả sử CapNhatSoLuong nhận số dương để cộng vào kho
                        DataLayer.MaSanPhanFactory.CapNhatSoLuong(ct.MaSanPham.Id, ct.SoLuong);
                    }

                    // Xóa phiếu khỏi giao diện và lưu thay đổi vào CSDL
                    bindingNavigator.BindingSource.RemoveCurrent();
                    this.ctrl.Save();
                    MessageBox.Show("Xóa phiếu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // Ngăn DataGridView tự xóa, thay vào đó gọi hàm xóa của chúng ta
            e.Cancel = true;
            XoaPhieuBanHienTai();
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            // Cả nút xóa trên navigation bar và phím Delete đều gọi cùng một hàm
            XoaPhieuBanHienTai();
        }

        private void toolIn_Click(object sender, EventArgs e)
        {
            DataRowView row = (DataRowView)bindingNavigator.BindingSource.Current;
            if (row != null)
            {
                string ma_phieu = row["ID"].ToString();
                // 3. TỐI ƯU: Tái sử dụng đối tượng 'ctrl' đã có, không tạo mới
                PhieuBan ph = ctrl.LayPhieuBan(ma_phieu);
                if (ph != null)
                {
                    frmInPhieuBan phieuBanPrintForm = new frmInPhieuBan(ph);
                    phieuBanPrintForm.Show();
                }
            }
        }

        private void toolTimKiem_Click(object sender, EventArgs e)
        {
            // Dùng 'using' để đảm bảo form TimKiem được giải phóng tài nguyên
            using (frmTimPhieuBanLe tim = new frmTimPhieuBanLe(true))
            {
                Point p = this.PointToScreen(toolTimKiem.Bounds.Location);
                p.X += toolTimKiem.Width;
                p.Y += toolTimKiem.Height;
                tim.Location = p;

                if (tim.ShowDialog() == DialogResult.OK)
                {
                    ctrl.TimPhieuBan(tim.cmbNCC.SelectedValue.ToString(), tim.dtNgayNhap.Value.Date);
                }
            }
        }

        private void frmDanhsachPhieuBanSi_Load(object sender, EventArgs e)
        {
            ctrlKH.HienthiDaiLyDataGridviewComboBox(colKhachhang);
            ctrl.HienthiPhieuBanSi(bindingNavigator, dataGridView);
        }
    }
}