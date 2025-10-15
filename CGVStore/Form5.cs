using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CGVStore.BUS; // Cần tham chiếu đến Project BUS
using CGVStore.Models; // Cần tham chiếu đến Entity Models (cho Area/KhachHang/HoaDon/ChiTiet nếu cần)

namespace CGVStore
{
    public partial class Form5 : Form
    {
        private decimal totalPrice = 0;
        private List<Button> seatButtons;

        private int selectedMaHD = 0;
        private int selectedMaKH = 0;

        // Khai báo các đối tượng BUS
        private readonly TicketBUS ticketBUS = new TicketBUS();
        private readonly AreaBUS areaBUS = new AreaBUS();

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
            // Giữ lại cấu hình này vì nó dựa trên thuộc tính của Area Model
            cboArea.ValueMember = "AreaID";
            cboArea.DisplayMember = "AreaName";
        }

        // =======================================================
        //                 KHỞI TẠO & TẢI DỮ LIỆU
        // =======================================================

        private void SetupSeatButtons()
        {
            // Vẫn là logic UI
            seatButtons = grbManHinh.Controls.OfType<Button>().ToList();
            foreach (Button btn in seatButtons)
            {
                btn.Click -= btnChooseSeats_Click;
                btn.Click += btnChooseSeats_Click;
            }
        }

        private void LoadArea()
        {
            try
            {
                // Thay thế truy cập DAL trực tiếp bằng AreaBUS
                var areas = areaBUS.LayDanhSachKhuVuc();
                cboArea.DataSource = areas;
                cboArea.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Khu Vực: " + ex.Message, "Lỗi Nghiệp Vụ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHoaDonToDGV()
        {
            try
            {
                // Thay thế truy cập DAL trực tiếp bằng TicketBUS
                // Giả định LayDanhSachHoaDonView trả về List<object> (hoặc DTO) phù hợp cho DGV
                var hoaDonsView = ticketBUS.LayDanhSachHoaDonView() as IEnumerable<dynamic>;

                dgvBill.Rows.Clear();
                dgvBill.Refresh();

                // Cấu hình cột (Chỉ thực hiện lần đầu)
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

                dgvBill.Rows.Clear();

                // ÉP KIỂU SANG dynamic TRƯỚC KHI LẶP
                if (hoaDonsView is System.Collections.IEnumerable list) // Kiểm tra nếu là một tập hợp
                {
                    // Ép kiểu các phần tử sang dynamic khi lặp
                    foreach (dynamic hd in list)
                    {
                        // Bây giờ bạn có thể truy cập các thuộc tính
                        dgvBill.Rows.Add(
                            hd.MaHD,
                            hd.MaKH,
                            hd.TenKhachHang,
                            hd.SDT,
                            ((decimal)hd.TongTien).ToString("N0"),
                            hd.NgayMua // Cần đảm bảo cột này tồn tại trong DGV và DTO/Anonymous object
                        );
                    }
                }

                ResetSelectionState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu Hóa Đơn: " + ex.Message, "Lỗi Nghiệp Vụ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tính năng MỚI: Tải tất cả các ghế đã bán (có trong ChiTiet) và vô hiệu hóa chúng.
        /// Sử dụng TicketBUS.
        /// </summary>
        private void LoadSoldSeats()
        {
            try
            {
                // 1. Lấy danh sách các số ghế đã được bán từ BUS (thay thế truy cập DAL trực tiếp)
                // Giả định LayDanhSachGheDaBan trả về List<string> chứa mã ghế (vd: "1", "2")
                List<string> soldSeats = ticketBUS.LayDanhSachGheDaBan();

                // 2. Duyệt qua tất cả các nút ghế và cập nhật trạng thái
                foreach (Button btn in seatButtons)
                {
                    // Đặt lại màu về trắng trước khi kiểm tra, để tránh ghế hủy vé vẫn bị tô màu Blue/Yellow
                    btn.BackColor = Color.White;
                    btn.Enabled = true;

                    // Mã ghế trong UI (Text của button) là string (vd: "1", "2")
                    if (soldSeats.Contains(btn.Text))
                    {
                        // Ghế đã bán: Vô hiệu hóa và tô màu Vàng
                        btn.BackColor = Color.Yellow;
                        btn.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu ghế đã bán: " + ex.Message, "Lỗi Nghiệp Vụ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // =======================================================
        //                      HÀM XỬ LÝ SỰ KIỆN
        // =======================================================

        private void dgvBill_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Giữ nguyên logic UI
            ResetSelectionState();

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvBill.Rows[e.RowIndex];

                int.TryParse(row.Cells["colMaHD"].Value?.ToString(), out selectedMaHD);
                int.TryParse(row.Cells["colMaKH"].Value?.ToString(), out selectedMaKH);

                txtFullName.Text = row.Cells["colTenKhachHang"].Value?.ToString();
                txtPhone.Text = row.Cells["colSDT"].Value?.ToString();

                // Loại bỏ logic truy vấn database trực tiếp để lấy Area/DiaChi
                // Thông thường, DGV chỉ hiển thị thông tin cơ bản. 
                // Việc fill chi tiết Khu Vực khi click Cell sẽ phức tạp hơn khi tách lớp, 
                // cần một DTO phức tạp hơn hoặc một hàm BUS chuyên biệt để lấy chi tiết KhachHang.
                // Ở đây, ta chỉ fill các trường đơn giản và BỎ QUA việc fill cboArea khi click.
                // Nếu cần thiết, có thể thêm: txtTotal.Text = row.Cells["colTongTien"].Value?.ToString();

                btnBuyTicket.Enabled = false;
                btnCancel.Text = "Hủy Vé";
            }
        }

        private void BtnAddArea_Click(object sender, EventArgs e)
        {
            // Logic UI: Mở Form con
            Form3 addAreaForm = new Form3();
            addAreaForm.StartPosition = FormStartPosition.CenterParent;
            addAreaForm.ShowDialog();
            LoadArea(); // Tải lại danh sách khu vực sau khi Form3 đóng
        }

        private void BtnBuyTicket_Click(object sender, EventArgs e)
        {
            // 1. Lấy danh sách ghế đã chọn từ UI
            List<string> selectedSeatTexts = grbManHinh.Controls.OfType<Button>()
                                                        .Where(s => s.BackColor == Color.Blue)
                                                        .Select(s => s.Text) // Lấy mã ghế (vd: "1", "2", "3")
                                                        .ToList();

            // 2. Validate Dữ liệu (UI Validation)
            if (selectedSeatTexts.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một ghế để mua.", "Chưa Chọn Ghế", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string gioiTinh = radMale.Checked ? "Nam" : (radFemale.Checked ? "Nữ" : string.Empty);

            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrEmpty(gioiTinh) || cboArea.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng điền đầy đủ Họ tên, Số điện thoại, Giới tính và chọn Khu vực.", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Chuẩn bị dữ liệu cho BUS
            string tenKH = txtFullName.Text.Trim();
            string sdt = txtPhone.Text.Trim();
            object areaID = cboArea.SelectedValue; // Dùng SelectedValue (int)

            // 4. GỌI LOGIC NGHIỆP VỤ TỪ BUS
            try
            {
                ticketBUS.XuLyMuaVe(tenKH, sdt, gioiTinh, areaID, totalPrice, selectedSeatTexts);

                // 5. Cập nhật giao diện sau khi lưu thành công
                MessageBox.Show("Mua vé thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadHoaDonToDGV();
                LoadSoldSeats(); // CẬP NHẬT GHẾ ĐÃ BÁN
                ResetForm();
            }
            catch (ArgumentException ex)
            {
                // Lỗi kiểm tra rỗng, định dạng từ BUS
                MessageBox.Show("Lỗi nhập liệu: " + ex.Message, "Lỗi Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi nghiệp vụ từ BUS (ví dụ: ghế đã bán)
                MessageBox.Show("Lỗi nghiệp vụ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Lỗi chung (DB hoặc lỗi khác)
                MessageBox.Show("Lỗi khi thực hiện giao dịch: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (selectedMaHD > 0)
            {
                // Chức năng HỦY VÉ (Xóa Hóa Đơn)
                if (MessageBox.Show($"Bạn có chắc chắn muốn hủy Hóa Đơn số {selectedMaHD} không?", "Xác nhận Hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // GỌI LOGIC NGHIỆP VỤ HỦY TỪ BUS
                    try
                    {
                        ticketBUS.XuLyHuyVe(selectedMaHD, selectedMaKH); // Cần truyền cả MaKH để xác định đúng

                        MessageBox.Show($"Đã hủy thành công Hóa Đơn số {selectedMaHD}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadHoaDonToDGV();
                        LoadSoldSeats(); // CẬP NHẬT GHẾ SAU KHI GIẢI PHÓNG
                        ResetForm();
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show("Lỗi nhập liệu: " + ex.Message, "Lỗi Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show("Lỗi nghiệp vụ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi hủy vé: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // Giữ nguyên logic UI
            Button clickedButton = sender as Button;

            if (clickedButton != null && clickedButton.Enabled)
            {
                if (int.TryParse(clickedButton.Text, out int seatNumber))
                {
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

                    txtTotal.Text = totalPrice.ToString("N0"); // Định dạng tiền
                }
            }
        }

        private decimal GetServicePrice(int seatNumber)
        {
            // Logic tính giá vẫn giữ ở UI/Shared Utility nếu nó không phải logic nghiệp vụ quan trọng
            if (seatNumber >= 1 && seatNumber <= 5) return 80000;
            else if (seatNumber >= 6 && seatNumber <= 10) return 90000;
            else if (seatNumber >= 11 && seatNumber <= 15) return 100000;
            else return 110000;
        }

        // =======================================================
        //                      LOGIC DATABASE
        // =======================================================
        // Các hàm SaveTransaction và DeleteHoaDon CŨ đã được LOẠI BỎ và thay thế 
        // bằng việc gọi các hàm nghiệp vụ từ TicketBUS trong BtnBuyTicket_Click và BtnCancel_Click.


        // =======================================================
        //                          LOGIC UI
        // =======================================================

        private void ResetSelectionState()
        {
            // Giữ nguyên logic UI
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
            // Giữ nguyên logic UI
            dgvBill.ClearSelection();

            // Reset màu các nút đang chọn (màu Blue) về màu White (chỉ các ghế đang chọn tạm thời)
            foreach (Control control in grbManHinh.Controls.OfType<Button>().Where(s => s.BackColor == Color.Blue))
            {
                // Không cần kiểm tra Enabled ở đây vì chỉ reset ghế đang được chọn
                control.BackColor = Color.White;
            }

            totalPrice = 0;
            txtTotal.Text = totalPrice.ToString("N0");

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