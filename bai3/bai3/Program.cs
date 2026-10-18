using bai3;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace GymMemberManager;

public class MemberModel { public string MemberId { get; set; } = ""; public string FullName { get; set; } = ""; public string Phone { get; set; } = ""; public string Package { get; set; } = "Theo tháng"; public DateTime StartDate { get; set; } = DateTime.Today; public decimal Fee { get; set; } }
static class Program { [STAThread] static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); } }
public class MainForm : Form
{
    BindingList<MemberModel> members = new(); BindingSource bs = new(); ErrorProvider ep = new(); TextBox id = new(), name = new(), phone = new(), fee = new(); ComboBox pack = new(); DateTimePicker start = new(); DataGridView grid = new(); ToolStripStatusLabel status = new();
    public MainForm() { Text = "Gym Member Manager"; Size = new(1050, 650); StartPosition = FormStartPosition.CenterScreen; ep.ContainerControl = this; Build(); bs.DataSource = members; grid.DataSource = bs; members.ListChanged += (_, __) => status.Text = $"Tổng hội viên: {members.Count}"; members.Add(new() { MemberId = "HV001", FullName = "Nguyễn Minh Anh", Phone = "0912345678", Package = "Theo tháng", Fee = 500000 }); NewId(); }
    void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new(10), RowCount = 3, ColumnCount = 1 }; root.RowStyles.Add(new(SizeType.Absolute, 210)); root.RowStyles.Add(new(SizeType.Percent, 100)); root.RowStyles.Add(new(SizeType.Absolute, 25)); Controls.Add(root); var g = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 }; for (int i = 0; i < 4; i++) g.ColumnStyles.Add(new(SizeType.Percent, 25)); for (int i = 0; i < 3; i++) g.RowStyles.Add(new(SizeType.Percent, 33.3f)); id.ReadOnly = true; pack.DropDownStyle = ComboBoxStyle.DropDownList; pack.Items.AddRange(new object[] { "Theo tháng", "6 tháng", "1 năm" }); pack.SelectedIndex = 0; start.Format = DateTimePickerFormat.Short;
        Field(g, "Mã hội viên", id, 0, 0); Field(g, "Họ tên *", name, 1, 0); Field(g, "SĐT *", phone, 2, 0); Field(g, "Gói tập", pack, 3, 0); Field(g, "Ngày đăng ký", start, 0, 1); Field(g, "Phí dịch vụ *", fee, 1, 1);
        var b = new FlowLayoutPanel { Dock = DockStyle.Fill }; b.Controls.Add(Btn("Thêm mới", (_, __) => Add())); b.Controls.Add(Btn("Sửa thông tin", (_, __) => Edit())); b.Controls.Add(Btn("Xóa hội viên", (_, __) => Delete())); b.Controls.Add(Btn("Làm mới", (_, __) => Clear())); g.Controls.Add(b, 2, 1); g.SetColumnSpan(b, 2); root.Controls.Add(g, 0, 0);
        grid.Dock = DockStyle.Fill; grid.AutoGenerateColumns = false; grid.ReadOnly = true; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Col("Mã HV", "MemberId"); Col("Tên hội viên", "FullName"); Col("SĐT", "Phone"); Col("Gói tập", "Package"); Col("Ngày ĐK", "StartDate", "dd/MM/yyyy"); Col("Phí dịch vụ", "Fee", "N0"); grid.SelectionChanged += (_, __) => LoadRow(); root.Controls.Add(grid, 0, 1); var s = new StatusStrip(); s.Items.Add(status); root.Controls.Add(s, 0, 2);
    }
    void Field(TableLayoutPanel p, string t, Control c, int x, int y) { var a = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new(3) }; a.RowStyles.Add(new(SizeType.Absolute, 22)); a.RowStyles.Add(new(SizeType.Percent, 100)); a.Controls.Add(new Label { Text = t, Dock = DockStyle.Fill }, 0, 0); c.Dock = DockStyle.Fill; a.Controls.Add(c, 0, 1); p.Controls.Add(a, x, y); }
    void Col(string t, string p, string f = null) { var c = new DataGridViewTextBoxColumn { HeaderText = t, DataPropertyName = p }; if (f != null) c.DefaultCellStyle.Format = f; grid.Columns.Add(c); }
    Button Btn(string t, EventHandler e) { var b = new Button { Text = t, Width = 125, Height = 30 }; b.Click += e; return b; }
    void NewId() { int n = members.Select(x => int.TryParse(x.MemberId.Replace("HV", ""), out var v) ? v : 0).DefaultIfEmpty().Max() + 1; id.Text = $"HV{n:000}"; }
    MemberModel Selected() => grid.CurrentRow?.DataBoundItem as MemberModel;
    bool Valid() { ep.Clear(); bool ok = true; if (string.IsNullOrWhiteSpace(name.Text) || !Regex.IsMatch(name.Text.Trim(), @"^[\p{L}\s'-]+$")) { ep.SetError(name, "Tên không được trống/chứa ký tự đặc biệt."); ok = false; } if (!Regex.IsMatch(phone.Text, @"^\d{10}$")) { ep.SetError(phone, "SĐT phải đúng 10 chữ số."); ok = false; } if (!long.TryParse(fee.Text, out var f) || f <= 0) { ep.SetError(fee, "Phí phải là số nguyên dương."); ok = false; } return ok; }
    MemberModel Read() => new() { MemberId = id.Text, FullName = name.Text.Trim(), Phone = phone.Text.Trim(), Package = pack.Text, StartDate = start.Value.Date, Fee = decimal.Parse(fee.Text) };
    void Add() { if (!Valid()) return; members.Add(Read()); Clear(); }
    void Edit() { var x = Selected(); if (x == null) { MessageBox.Show("Chọn hội viên."); return; } if (!Valid()) return; var v = Read(); x.FullName = v.FullName; x.Phone = v.Phone; x.Package = v.Package; x.StartDate = v.StartDate; x.Fee = v.Fee; bs.ResetBindings(false); }
    void Delete() { var x = Selected(); if (x != null && MessageBox.Show("Xóa hội viên này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { members.Remove(x); Clear(); } }
    void Clear() { ep.Clear(); name.Clear(); phone.Clear(); fee.Clear(); pack.SelectedIndex = 0; start.Value = DateTime.Today; grid.ClearSelection(); NewId(); }
    void LoadRow() { var x = Selected(); if (x == null) return; id.Text = x.MemberId; name.Text = x.FullName; phone.Text = x.Phone; pack.Text = x.Package; start.Value = x.StartDate; fee.Text = x.Fee.ToString("0"); }
}
