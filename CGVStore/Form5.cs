using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CGVStore.Models;
using System.Data.Entity;

namespace CGVStore
{
    public partial class Form5 : Form
    {
        private decimal totalPrice = 0;
        private List<Button> seatButtons;

        private int selectedMaHD = 0;
        private int selectedMaKH = 0;

        public Form5()
        {
            InitializeComponent();

            SetupSeatButtons();
            LoadArea();
            LoadHoaDonToDGV();
            LoadSoldSeats(); // <--- TẢI GHẾ ĐÃ BÁN KHI FORM KHỞI TẠO

            // Gán sự kiện cho các nút chức năng
            btnExit.Click += (s, e) => this.Close();
            btnCancel.Click += BtnCancel_Click;
            txtTotal.Enabled = false;
            btnBuyTicket.Click += BtnBuyTicket_Click;
            btnAddArea.Click += BtnAddArea_Click;
            dgvBill.CellClick += dgvBill_CellClick;

            // Cấu hình ComboBox
            cboArea.ValueMember = "AreaID";
            cboArea.DisplayMember = "AreaName";
        }

        // =======================================================
        //                 KHỞI TẠO & TẢI DỮ LIỆU
        // =======================================================

        private void SetupSeatButtons()
        {
            seatButtons = grbManHinh.Controls.OfType<Button>().ToList();
            foreach (Button btn in seatButtons)
            {
                btn.Click -= btnChooseSeats_Click;
                btn.Click += btnChooseSeats_Click;
                // Màu mặc định sẽ được LoadSoldSeats() thiết lập lại
            }
        }

        private void LoadArea()
        {
            try
            {
                using (var db = new Model1())
                {
                    var areas = db.Areas.ToList();
                    cboArea.DataSource = areas;
                    cboArea.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Khu Vực: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHoaDonToDGV()
        {
            // ... (Giữ nguyên logic LoadHoaDonToDGV) ...
            try
            {
                using (var db = new Model1())
                {
                    var hoaDons = db.HoaDons
                                    .Include(hd => hd.KhachHang)
                                    .OrderByDescending(hd => hd.NgayMua)
                                    .Select(hd => new
                                    {
                                        MaHD = hd.MaHD,
                                        MaKH = hd.MaKH,
                                        TenKH = hd.KhachHang.TenKH,
                                        SDT = hd.KhachHang.SDT,
                                        TongTien = hd.TongTien
                                    }).ToList();

                    dgvBill.Rows.Clear();
                    dgvBill.Refresh();

                    if (dgvBill.Columns.Count < 5 || dgvBill.Columns[0].Name != "colMaHD")
                    {
                        dgvBill.Columns.Clear();
                        dgvBill.Columns.Add("colMaHD", "MaHD");
                        dgvBill.Columns.Add("colMaKH", "MaKH");
                        dgvBill.Columns.Add("colTenKhachHang", "Tên Khách Hàng");
                        dgvBill.Columns.Add("colSDT", "SĐT");
                        dgvBill.Columns.Add("colTongTien", "Tổng Tiền");

                        dgvBill.Columns["colMaHD"].Visible = false;
                        dgvBill.Columns["colMaKH"].Visible = false;
                    }

                    foreach (var hd in hoaDons)
                    {
                        dgvBill.Rows.Add(
                            hd.MaHD,
                            hd.MaKH,
                            hd.TenKH,
                            hd.SDT,
                            hd.TongTien.GetValueOrDefault().ToString("N0")
                        );
                    }

                    ResetSelectionState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Hóa Đơn: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tính năng MỚI: Tải tất cả các ghế đã bán (có trong ChiTiet) và vô hiệu hóa chúng.
        /// </summary>
        private void LoadSoldSeats()
        {
            try
            {
                using (var db = new Model1())
                {
                    // 1. Lấy danh sách các số ghế đã được bán
                    var soldSeats = db.ChiTiets
                                        .Where(ct => ct.SoGheNgoi.HasValue)
                                        .Select(ct => ct.SoGheNgoi.Value)
                                        .Distinct()
                                        .ToList();

                    // 2. Duyệt qua tất cả các nút ghế và cập nhật trạng thái
                    foreach (Button btn in seatButtons)
                    {
                        if (int.TryParse(btn.Text, out int seatNumber))
                        {
                            if (soldSeats.Contains(seatNumber))
                            {
                                // Ghế đã bán: Vô hiệu hóa và tô màu Vàng
                                btn.BackColor = Color.Yellow;
                                btn.Enabled = false;
                            }
                            else
                            {
                                // Ghế chưa bán: Có thể mua, đặt màu Trắng
                                btn.BackColor = Color.White;
                                btn.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu ghế đã bán: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // =======================================================
        //                   HÀM XỬ LÝ SỰ KIỆN
        // =======================================================

        private void dgvBill_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ResetSelectionState();

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvBill.Rows[e.RowIndex];

                int.TryParse(row.Cells["colMaHD"].Value?.ToString(), out selectedMaHD);
                int.TryParse(row.Cells["colMaKH"].Value?.ToString(), out selectedMaKH);

                txtFullName.Text = row.Cells["colTenKhachHang"].Value?.ToString();
                txtPhone.Text = row.Cells["colSDT"].Value?.ToString();
                txtTotal.Text = row.Cells["colTongTien"].Value?.ToString();

                try
                {
                    using (var db = new Model1())
                    {
                        var customer = db.KhachHangs.FirstOrDefault(kh => kh.MaKH == selectedMaKH);

                        if (customer != null)
                        {
                            var area = db.Areas.FirstOrDefault(a => a.AreaName.Equals(customer.DiaChi));

                            if (area != null)
                            {
                                cboArea.SelectedValue = area.AreaID;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải chi tiết khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                btnBuyTicket.Enabled = false;
                btnCancel.Text = "Hủy Vé";
            }
        }

        private void BtnAddArea_Click(object sender, EventArgs e)
        {
            Form3 addAreaForm = new Form3();
            addAreaForm.StartPosition = FormStartPosition.CenterParent;
            addAreaForm.ShowDialog();
            LoadArea();
        }

        private void BtnBuyTicket_Click(object sender, EventArgs e)
        {
            List<Button> seatsToBuy = grbManHinh.Controls.OfType<Button>()
                                                .Where(s => s.BackColor == Color.Blue).ToList();

            if (seatsToBuy.Count > 0)
            {
                // 1. Validate Dữ liệu
                if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text) || (!radFemale.Checked && !radMale.Checked) || cboArea.SelectedIndex == -1)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ Họ tên, Số điện thoại, Giới tính và chọn Khu vực.", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Thực hiện giao dịch lưu vào Database
                if (SaveTransaction(seatsToBuy))
                {
                    // 3. Cập nhật giao diện sau khi lưu thành công

                    LoadHoaDonToDGV();
                    LoadSoldSeats(); // <--- CẬP NHẬT GHẾ ĐÃ BÁN SAU KHI MUA THÀNH CÔNG
                    ResetForm();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn ít nhất một ghế để mua.", "Chưa Chọn Ghế", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (selectedMaHD > 0)
            {
                // Chức năng HỦY VÉ (Xóa Hóa Đơn)
                if (MessageBox.Show($"Bạn có chắc chắn muốn hủy Hóa Đơn số {selectedMaHD} không?", "Xác nhận Hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (DeleteHoaDon(selectedMaHD))
                    {
                        MessageBox.Show($"Đã hủy thành công Hóa Đơn số {selectedMaHD}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadHoaDonToDGV();
                        LoadSoldSeats(); // <--- CẬP NHẬT GHẾ SAU KHI GIẢI PHÓNG
                        ResetForm();
                    }
                }
            }
            else
            {
                // Chức năng RESET FORM
                ResetForm();
            }
        }

        private void btnChooseSeats_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            // Chỉ xử lý khi nút ghế đang Enabled (chưa bán)
            if (clickedButton != null && clickedButton.Enabled)
            {
                int seatNumber = int.Parse(clickedButton.Text);

                if (clickedButton.BackColor != Color.Blue)
                {
                    // Chọn ghế
                    clickedButton.BackColor = Color.Blue;
                    totalPrice += GetServicePrice(seatNumber);
                }
                else
                {
                    // Hủy chọn
                    clickedButton.BackColor = Color.White;
                    totalPrice -= GetServicePrice(seatNumber);
                }
            }

            txtTotal.Text = totalPrice.ToString("C");
        }

        private decimal GetServicePrice(int seatNumber)
        {
            if (seatNumber >= 1 && seatNumber <= 5) return 80000;
            else if (seatNumber >= 6 && seatNumber <= 10) return 90000;
            else if (seatNumber >= 11 && seatNumber <= 15) return 100000;
            else return 110000;
        }

        // =======================================================
        //                      LOGIC DATABASE
        // =======================================================

        private bool SaveTransaction(List<Button> seatsToBuy)
        {
            // ... (Giữ nguyên logic SaveTransaction) ...
            try
            {
                using (var db = new Model1())
                {
                    string fullName = txtFullName.Text.Trim();
                    string phoneString = txtPhone.Text.Trim();
                    int? phone = int.Parse(phoneString);

                    int areaID = (int)cboArea.SelectedValue;

                    // 1. Xử lý Khách Hàng (Tìm hoặc Thêm mới)
                    KhachHang customer = db.KhachHangs.FirstOrDefault(kh => kh.SDT == phone);
                    int maKH;

                    if (customer == null)
                    {
                        maKH = db.KhachHangs.Any() ? db.KhachHangs.Max(kh => kh.MaKH) + 1 : 1;
                        customer = new KhachHang
                        {
                            MaKH = maKH,
                            TenKH = fullName,
                            SDT = phone,
                            DiaChi = cboArea.Text
                        };
                        db.KhachHangs.Add(customer);
                    }
                    else
                    {
                        maKH = customer.MaKH;
                        customer.TenKH = fullName;
                        customer.DiaChi = cboArea.Text;
                    }

                    // 2. Tạo Hóa Đơn mới
                    int maHD = db.HoaDons.Any() ? db.HoaDons.Max(hd => hd.MaHD) + 1 : 1;

                    HoaDon newHoaDon = new HoaDon
                    {
                        MaHD = maHD,
                        MaKH = maKH,
                        TongTien = (double)totalPrice,
                        NgayMua = DateTime.Now
                    };
                    db.HoaDons.Add(newHoaDon);

                    // 3. Tạo Chi Tiết Hóa Đơn (Cho từng ghế)
                    int currentMaxMaCT = db.ChiTiets.Any() ? db.ChiTiets.Max(ct => ct.MaCT) : 0;

                    foreach (Button seat in seatsToBuy)
                    {
                        currentMaxMaCT++;
                        int soGheNgoi = int.Parse(seat.Text);

                        ChiTiet chiTiet = new ChiTiet
                        {
                            MaCT = currentMaxMaCT,
                            MaHD = maHD,
                            MaKH = maKH,
                            SoGheNgoi = soGheNgoi, // LƯU SỐ GHẾ VÀO DATABASE
                            NgayThanhToan = DateTime.Now
                        };
                        db.ChiTiets.Add(chiTiet);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện giao dịch: " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool DeleteHoaDon(int maHD)
        {
            // ... (Giữ nguyên logic DeleteHoaDon) ...
            try
            {
                using (var db = new Model1())
                {
                    // 1. Tìm Chi Tiết liên quan đến Hóa Đơn này
                    var chiTietsToDelete = db.ChiTiets.Where(ct => ct.MaHD == maHD && ct.MaKH == selectedMaKH).ToList();
                    db.ChiTiets.RemoveRange(chiTietsToDelete);

                    // 2. Tìm Hóa Đơn cần xóa
                    var hoaDonToDelete = db.HoaDons.SingleOrDefault(hd => hd.MaHD == maHD && hd.MaKH == selectedMaKH);

                    if (hoaDonToDelete != null)
                    {
                        db.HoaDons.Remove(hoaDonToDelete);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy Hóa Đơn để hủy.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hủy vé (xóa Hóa Đơn): " + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // =======================================================
        //                      LOGIC UI
        // =======================================================

        private void ResetSelectionState()
        {
            selectedMaHD = 0;
            selectedMaKH = 0;
            btnBuyTicket.Enabled = true;
            btnCancel.Text = "Hủy";
        }

        /// <summary>
        /// Đặt lại toàn bộ form, bao gồm input và ghế ngồi đang được chọn
        /// </summary>
        private void ResetForm()
        {
            // Reset trạng thái chọn DGV
            dgvBill.ClearSelection();

            // Reset màu các nút đang chọn (màu Blue) về màu White (chỉ các ghế đang chọn tạm thời)
            foreach (Control control in grbManHinh.Controls.OfType<Button>().Where(s => s.BackColor == Color.Blue))
            {
                control.BackColor = Color.White;
            }

            totalPrice = 0;
            txtTotal.Text = totalPrice.ToString("C");

            // Reset Input Fields
            txtFullName.Clear();
            txtPhone.Clear();
            radMale.Checked = false;
            radFemale.Checked = false;
            cboArea.SelectedIndex = -1;

            ResetSelectionState();
        }
    }
}