
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevenueAnalytics
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new RevenueForm());
        }
    }

    public class RevenueForm : Form
    {
        private Label lblTitle;
        private Label lblStatus;
        private Label lblResult;
        private ProgressBar progressBar;
        private Button btnStart;
        private Button btnCancel;
        private CancellationTokenSource cts;

        public RevenueForm()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Bài 4 - Thống kê doanh thu";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            lblTitle = new Label
            {
                Text = "THỐNG KÊ DOANH THU",
                Font = new Font("Arial", 18, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(30, 25),
                Size = new Size(520, 45)
            };

            lblStatus = new Label
            {
                Text = "Trạng thái: Sẵn sàng",
                Font = new Font("Arial", 11),
                Location = new Point(40, 100),
                Size = new Size(500, 30)
            };

            progressBar = new ProgressBar
            {
                Location = new Point(40, 145),
                Size = new Size(500, 30),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            lblResult = new Label
            {
                Text = "Kết quả doanh thu sẽ hiển thị tại đây.",
                Font = new Font("Arial", 11),
                Location = new Point(40, 195),
                Size = new Size(500, 60)
            };

            btnStart = new Button
            {
                Text = "Bắt đầu thống kê",
                Location = new Point(115, 285),
                Size = new Size(160, 40),
                BackColor = Color.LightGreen
            };

            btnCancel = new Button
            {
                Text = "Hủy thống kê",
                Location = new Point(305, 285),
                Size = new Size(160, 40),
                BackColor = Color.LightCoral,
                Enabled = false
            };

            btnStart.Click += async (sender, e) =>
                await StartStatisticsAsync();

            btnCancel.Click += (sender, e) =>
            {
                cts?.Cancel();
                lblStatus.Text = "Trạng thái: Đang yêu cầu hủy...";
                btnCancel.Enabled = false;
            };

            Controls.Add(lblTitle);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(lblResult);
            Controls.Add(btnStart);
            Controls.Add(btnCancel);
        }

        private async Task StartStatisticsAsync()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();

            CancellationToken token = cts.Token;

            btnStart.Enabled = false;
            btnCancel.Enabled = true;

            progressBar.Value = 0;
            lblResult.Text = "Đang xử lý dữ liệu doanh thu...";
            lblStatus.Text = "Trạng thái: Đang thống kê...";

            // Progress được tạo trên UI thread để cập nhật giao diện an toàn.
            IProgress<int> progress = new Progress<int>(value =>
            {
                progressBar.Value = Math.Clamp(value, 0, 100);
                lblStatus.Text =
                    $"Trạng thái: Đang thống kê... {value}%";
            });

            try
            {
                // Task.Run chạy công việc nền, tránh làm treo giao diện.
                double revenue = await Task.Run(async () =>
                {
                    Random random = new Random();
                    double totalRevenue = 0;
                    const int totalSteps = 50;

                    for (int i = 1; i <= totalSteps; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        // Giả lập xử lý dữ liệu mất khoảng 5 giây.
                        await Task.Delay(100, token);

                        totalRevenue += random.Next(100000, 1000000);

                        int percent = i * 100 / totalSteps;
                        progress.Report(percent);
                    }

                    return totalRevenue;
                }, token);

                progressBar.Value = 100;
                lblStatus.Text = "Trạng thái: Hoàn thành";

                lblResult.Text =
                    $"Tổng doanh thu: {revenue:N0} VNĐ\n" +
                    "Đã xử lý thành công 50 lượt dữ liệu.";
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Trạng thái: Đã hủy";
                lblResult.Text =
                    "Quá trình thống kê đã bị hủy.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Trạng thái: Có lỗi";
                lblResult.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnStart.Enabled = true;
                btnCancel.Enabled = false;

                cts?.Dispose();
                cts = null;
            }
        }
    }
}
