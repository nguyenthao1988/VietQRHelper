using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using VietQRHelper;


namespace TestQRPay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateQR_Click(object sender, EventArgs e)
        {
            var qrPay = QRPay.InitVietQR(
                  bankBin: BankApp.BanksObject[BankKey.VIETCOMBANK].bin,
                  bankNumber: "0721000584901", // Số tài khoản
                  amount: "20000", // Số tiền
                  purpose: "Donate LapTrinhVB"  // Nội dung chuyển tiền
                );
            var content = qrPay.Build();

            var imageQR = QRCodeHelper.TaoVietQRCodeImage(content );
            pictureBox1.Image = imageQR;    
        }
    }
}
