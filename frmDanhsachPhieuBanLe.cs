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
    public partial class frmDanhsachPhieuBanLe : Form
    {
        // Khai báo các đối tượng cần thiết
        private readonly PhieuBanController ctrl = new PhieuBanController();
        private readonly KhachHangController ctrlKH = new KhachHangController();
        private readonly ChiTietPhieuBanController ctrlChiTiet = new ChiTietPhieuBanController();
        private frmBanLe BanLe = null;
        public frmDanhsachPhieuBanLe()
        {
            InitializeComponent();
        }

        private void frmDanhsachPhieuNhap_Load(object sender, EventArgs e)
        {
            ctrlKH.HienthiKhachHangDataGridviewComboBox(colKhachhang);
            ctrl.HienthiPhieuBanLe(bindingNavigator, dataGridView);
        }
        //TỐI ƯU: Tạo phương thức chung để mở hoặc kích hoạt form Bán Lẻ
        private void MoHoacKichHoatFormBanLe(string idPhieuBan = null)
        {
            if (BanLe == null || BanLe.IsDisposed)
            {
                // Nếu có idPhieuBan, gọi constructor để xem/sửa. Ngược lại là tạo mới.
                // Điều này yêu cầu frmBanLe phải có constructor tương ứng: frmBanLe(string id)
                BanLe = (idPhieuBan != null) ? new frmBanLe() : new frmBanLe();
                BanLe.Show();
            }
            else
            {
                BanLe.Activate();
            }
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e)
        {
            DataRowView currentRow = (DataRowView)bindingNavigator.BindingSource.Current;
            if (currentRow != null)
            {
                string idPhieu = currentRow["ID"].ToString();
                MoHoacKichHoatFormBanLe(idPhieu);
            }
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            // Gọi phương thức chung để tạo phiếu mới (không truyền ID)
            MoHoacKichHoatFormBanLe();
        }
        private void XoaPhieuBanHienTai()
        {
            DataRowView currentRow = (DataRowView)bindingNavigator.BindingSource.Current;
            if (currentRow == null)
            {
                MessageBox.Show("Chưa có phiếu nào được chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phiếu này không?\nHành động này sẽ hoàn lại số lượng sản phẩm vào kho.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string idPhieu = currentRow["ID"].ToString();

                    // NOTE: Lý tưởng nhất, toàn bộ logic dưới đây nên nằm trong một phương thức của Controller,
                    // ví dụ: ctrl.XoaPhieuBan(idPhieu), để Form không cần biết chi tiết về việc cập nhật kho.

                    // Lấy danh sách chi tiết phiếu bán
                    IList<ChiTietPhieuBan> dsChiTiet = ctrlChiTiet.ChiTietPhieuBan(idPhieu);

                    // Cập nhật (trả lại) số lượng cho từng sản phẩm
                    foreach (ChiTietPhieuBan ct in dsChiTiet)
                    {
                        // Giả sử CapNhatSoLuong nhận số dương để cộng vào kho
                        CuahangNongduoc.DataLayer.MaSanPhanFactory.CapNhatSoLuong(ct.MaSanPham.Id, ct.SoLuong);
                    }

                    // Xóa phiếu khỏi binding source và lưu các thay đổi
                    bindingNavigator.BindingSource.RemoveCurrent();
                    ctrl.Save(); // Lưu thay đổi (xóa phiếu)

                    MessageBox.Show("Xóa phiếu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // ADDED: Xử lý lỗi nếu có sự cố xảy ra
                    MessageBox.Show("Đã xảy ra lỗi trong quá trình xóa:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = true;
            XoaPhieuBanHienTai();
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            // Gọi phương thức xóa chung
            XoaPhieuBanHienTai();
        }

        private void toolPrint_Click(object sender, EventArgs e)
        {
            DataRowView row = (DataRowView)bindingNavigator.BindingSource.Current;
            if (row != null)
            {
                string ma_phieu = row["ID"].ToString();
                // Không tạo mới PhieuBanController, dùng lại 'ctrl' có sẵn
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
            //thêm `using` để tự động giải phóng tài nguyên
            using (frmTimPhieuBanLe tim = new frmTimPhieuBanLe(false))
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

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}