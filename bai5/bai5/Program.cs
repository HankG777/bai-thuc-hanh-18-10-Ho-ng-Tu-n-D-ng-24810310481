
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HospitalMDI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmHospitalMain());
        }
    }

    public class frmHospitalMain : Form
    {
        private readonly MenuStrip menu = new MenuStrip();
        private readonly StatusStrip status = new StatusStrip();
        private readonly ToolStripStatusLabel statusText =
            new ToolStripStatusLabel("Bác sĩ trực: Nguyễn Văn An | Sẵn sàng");
        private readonly ToolStripProgressBar progress =
            new ToolStripProgressBar();
        private bool loading;

        public frmHospitalMain()
        {
            Text = "Hệ thống quản lý bệnh viện";
            IsMdiContainer = true;
            WindowState = FormWindowState.Maximized;

            var system = new ToolStripMenuItem("Hệ thống");
            var logout = new ToolStripMenuItem("Đăng xuất");
            var reload = new ToolStripMenuItem("Nạp lại dữ liệu");
            var exit = new ToolStripMenuItem("Thoát");

            logout.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có muốn đăng xuất?",
                    "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CloseAllChildren();
                    statusText.Text = "Đã đăng xuất";
                }
            };

            reload.Click += async (s, e) => await ReloadDataAsync();
            exit.Click += (s, e) => Close();

            system.DropDownItems.AddRange(new ToolStripItem[]
            {
                logout, reload, new ToolStripSeparator(), exit
            });

            var manage = new ToolStripMenuItem("Quản lý");
            AddMenu(manage, "Hồ sơ Bệnh nhân",
                () => OpenChild<frmPatientProfiles>());
            AddMenu(manage, "Danh mục Bác sĩ",
                () => OpenChild<frmDoctorCatalog>());
            AddMenu(manage, "Danh mục Khoa/Phòng",
                () => OpenChild<frmDepartmentCatalog>());
            AddMenu(manage, "Lịch trực Bác sĩ",
                () => OpenChild<frmDoctorSchedule>());

            var windows = new ToolStripMenuItem("Cửa sổ");
            AddMenu(windows, "Xếp tầng (Cascade)",
                () => LayoutMdi(MdiLayout.Cascade));
            AddMenu(windows, "Xếp ngang (Tile Horizontal)",
                () => LayoutMdi(MdiLayout.TileHorizontal));
            AddMenu(windows, "Xếp dọc (Tile Vertical)",
                () => LayoutMdi(MdiLayout.TileVertical));
            AddMenu(windows, "Đóng tất cả", CloseAllChildren);

            menu.Items.AddRange(new ToolStripItem[]
            {
                system, manage, windows
            });
            MainMenuStrip = menu;
            Controls.Add(menu);

            progress.Minimum = 0;
            progress.Maximum = 100;
            progress.Visible = false;
            progress.Size = new Size(150, 16);

            status.Items.Add(statusText);
            status.Items.Add(new ToolStripStatusLabel { Spring = true });
            status.Items.Add(new ToolStripStatusLabel("Tiến trình:"));
            status.Items.Add(progress);
            Controls.Add(status);
        }

        private void AddMenu(ToolStripMenuItem parent,
            string text, Action action)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += (s, e) => action();
            parent.DropDownItems.Add(item);
        }

        private void OpenChild<T>() where T : Form, new()
        {
            foreach (Form child in MdiChildren)
            {
                if (child is T)
                {
                    if (child.WindowState == FormWindowState.Minimized)
                        child.WindowState = FormWindowState.Normal;

                    child.Activate();
                    statusText.Text = "Đang sử dụng: " + child.Text;
                    return;
                }
            }

            T form = new T();
            form.MdiParent = this;
            form.FormClosed += (s, e) =>
                statusText.Text = "Đã đóng: " + form.Text;
            form.Show();
            statusText.Text = "Đang mở: " + form.Text;
        }

        private void CloseAllChildren()
        {
            foreach (Form child in MdiChildren)
                child.Close();

            statusText.Text = "Đã đóng tất cả cửa sổ con";
        }

        private async Task ReloadDataAsync()
        {
            if (loading) return;
            loading = true;
            progress.Visible = true;
            progress.Value = 0;
            statusText.Text = "Đang nạp dữ liệu...";

            try
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    progress.Value = i;
                    await Task.Delay(80);
                }

                foreach (Form child in MdiChildren)
                {
                    if (child is frmPatientProfiles p) p.RefreshData();
                    if (child is frmDoctorSchedule d) d.RefreshData();
                }

                statusText.Text = "Nạp dữ liệu hoàn tất";
            }
            finally
            {
                progress.Visible = false;
                loading = false;
            }
        }
    }

    public class frmPatientProfiles : Form
    {
        private readonly TextBox txtId = new TextBox();
        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtAge = new TextBox();
        private readonly TextBox txtDisease = new TextBox();
        private readonly DataGridView grid = new DataGridView();

        public frmPatientProfiles()
        {
            Text = "Hồ sơ Bệnh nhân";
            Size = new Size(800, 500);

            AddLabel("Mã bệnh nhân:", 20, 20);
            AddInput(txtId, 140, 17, 180);
            AddLabel("Họ tên:", 20, 60);
            AddInput(txtName, 140, 57, 180);
            AddLabel("Tuổi:", 370, 20);
            AddInput(txtAge, 440, 17, 150);
            AddLabel("Bệnh án:", 370, 60);
            AddInput(txtDisease, 440, 57, 200);

            var add = new Button
            {
                Text = "Thêm bệnh nhân",
                Bounds = new Rectangle(140, 105, 140, 32)
            };
            add.Click += (s, e) => AddPatient();

            var clear = new Button
            {
                Text = "Làm mới",
                Bounds = new Rectangle(290, 105, 100, 32)
            };
            clear.Click += (s, e) => ClearInputs();

            var delete = new Button
            {
                Text = "Xóa bệnh nhân",
                Bounds = new Rectangle(400, 105, 120, 32)
            };
            delete.Click += (s, e) => DeletePatient();

            grid.SetBounds(20, 155, 740, 280);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.Columns.Add("Id", "Mã bệnh nhân");
            grid.Columns.Add("Name", "Họ tên");
            grid.Columns.Add("Age", "Tuổi");
            grid.Columns.Add("Disease", "Bệnh án");

            grid.SelectionChanged += (s, e) =>
            {
                if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow)
                    return;

                txtId.Text = Convert.ToString(grid.CurrentRow.Cells[0].Value) ?? "";
                txtName.Text = Convert.ToString(grid.CurrentRow.Cells[1].Value) ?? "";
                txtAge.Text = Convert.ToString(grid.CurrentRow.Cells[2].Value) ?? "";
                txtDisease.Text = Convert.ToString(grid.CurrentRow.Cells[3].Value) ?? "";
            };

            Controls.AddRange(new Control[] { add, clear, delete, grid });
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private void AddInput(TextBox box, int x, int y, int width)
        {
            box.SetBounds(x, y, width, 25);
            Controls.Add(box);
        }

        private void AddPatient()
        {
            string id = txtId.Text.Trim();
            string name = txtName.Text.Trim();
            string disease = txtDisease.Text.Trim();

            if (id == "" || name == "" || txtAge.Text.Trim() == "" ||
                disease == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age) ||
                age < 1 || age > 120)
            {
                MessageBox.Show("Tuổi phải từ 1 đến 120.");
                return;
            }

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (string.Equals(Convert.ToString(row.Cells[0].Value),
                    id, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Mã bệnh nhân đã tồn tại.");
                    return;
                }
            }

            grid.Rows.Add(id, name, age, disease);
            ClearInputs();
            MessageBox.Show("Thêm hồ sơ thành công.");
        }

        private void DeletePatient()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Hãy chọn bệnh nhân cần xóa.");
                return;
            }

            grid.Rows.Remove(grid.CurrentRow);
            ClearInputs();
        }

        private void ClearInputs()
        {
            txtId.Clear();
            txtName.Clear();
            txtAge.Clear();
            txtDisease.Clear();
            txtId.Focus();
        }

        public void RefreshData()
        {
            grid.Refresh();
        }
    }

    public class frmDoctorSchedule : Form
    {
        private readonly TextBox txtDoctor = new TextBox();
        private readonly TextBox txtDepartment = new TextBox();
        private readonly DateTimePicker date = new DateTimePicker();
        private readonly ComboBox shift = new ComboBox();
        private readonly DataGridView grid = new DataGridView();

        public frmDoctorSchedule()
        {
            Text = "Lịch trực Bác sĩ";
            Size = new Size(800, 500);

            AddLabel("Tên bác sĩ:", 20, 20);
            AddInput(txtDoctor, 130, 17, 180);
            AddLabel("Khoa/Phòng:", 350, 20);
            AddInput(txtDepartment, 450, 17, 180);

            AddLabel("Ngày trực:", 20, 60);
            date.SetBounds(130, 57, 180, 25);
            date.Format = DateTimePickerFormat.Short;
            Controls.Add(date);

            AddLabel("Ca trực:", 350, 60);
            shift.SetBounds(450, 57, 180, 25);
            shift.DropDownStyle = ComboBoxStyle.DropDownList;
            shift.Items.AddRange(new object[]
            {
                "Sáng (07:00 - 11:00)",
                "Chiều (13:00 - 17:00)",
                "Tối (18:00 - 21:00)"
            });
            shift.SelectedIndex = 0;
            Controls.Add(shift);

            var add = new Button
            {
                Text = "Thêm lịch trực",
                Bounds = new Rectangle(130, 100, 130, 32)
            };
            add.Click += (s, e) => AddSchedule();

            var clear = new Button
            {
                Text = "Làm mới",
                Bounds = new Rectangle(270, 100, 100, 32)
            };
            clear.Click += (s, e) => ClearInputs();

            var delete = new Button
            {
                Text = "Xóa lịch trực",
                Bounds = new Rectangle(380, 100, 120, 32)
            };
            delete.Click += (s, e) => DeleteSchedule();

            grid.SetBounds(20, 150, 740, 280);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.Columns.Add("Doctor", "Bác sĩ");
            grid.Columns.Add("Department", "Khoa/Phòng");
            grid.Columns.Add("Date", "Ngày trực");
            grid.Columns.Add("Shift", "Ca trực");

            Controls.AddRange(new Control[] { add, clear, delete, grid });
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private void AddInput(TextBox box, int x, int y, int width)
        {
            box.SetBounds(x, y, width, 25);
            Controls.Add(box);
        }

        private void AddSchedule()
        {
            string doctor = txtDoctor.Text.Trim();
            string department = txtDepartment.Text.Trim();
            string dateText = date.Value.ToString("dd/MM/yyyy");
            string shiftText = shift.Text;

            if (doctor == "" || department == "")
            {
                MessageBox.Show("Nhập tên bác sĩ và khoa/phòng.");
                return;
            }

            foreach (DataGridViewRow row in grid.Rows)
            {
                bool sameDoctor = string.Equals(
                    Convert.ToString(row.Cells[0].Value), doctor,
                    StringComparison.OrdinalIgnoreCase);

                bool sameDate = Convert.ToString(row.Cells[2].Value) == dateText;
                bool sameShift = Convert.ToString(row.Cells[3].Value) == shiftText;

                if (sameDoctor && sameDate && sameShift)
                {
                    MessageBox.Show("Bác sĩ đã có lịch trong ca này.");
                    return;
                }
            }

            grid.Rows.Add(doctor, department, dateText, shiftText);
            ClearInputs();
            MessageBox.Show("Thêm lịch trực thành công.");
        }

        private void DeleteSchedule()
        {
            if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Hãy chọn lịch cần xóa.");
                return;
            }

            grid.Rows.Remove(grid.CurrentRow);
            ClearInputs();
        }

        private void ClearInputs()
        {
            txtDoctor.Clear();
            txtDepartment.Clear();
            date.Value = DateTime.Today;
            shift.SelectedIndex = 0;
            txtDoctor.Focus();
        }

        public void RefreshData()
        {
            grid.Refresh();
        }
    }

    public class frmDoctorCatalog : Form
    {
        public frmDoctorCatalog()
        {
            Text = "Danh mục Bác sĩ";
            Size = new Size(500, 300);

            Controls.Add(new Label
            {
                Text = "DANH MỤC BÁC SĨ",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 30)
            });
        }
    }

    public class frmDepartmentCatalog : Form
    {
        public frmDepartmentCatalog()
        {
            Text = "Danh mục Khoa/Phòng";
            Size = new Size(500, 300);

            Controls.Add(new Label
            {
                Text = "DANH MỤC KHOA / PHÒNG",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(100, 30)
            });
        }
    }
}
