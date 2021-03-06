﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Drawing.Printing;


namespace TeteHardware
{
    public partial class formReports : Form
    {
        public formAfterLogin ReferenceToAfterLogin { get; set; } //Reference formEmployeeManage to this form
        public MySqlConnection conn; //connection
        public string myType;
        Test func = new Test();
        float myGroupDiscount = 0;
        float myTotDiscount = 0;
        float myGroupSales = 0;
        float myTotSales = 0;
        string mySelectSQLChild = "";
        string myOrderSQL = "";
        string[] myDatesSQL = new string[3];
        string myID = "";
        bool mouseDown; //boolean for mousedown
        Point lastLocation; //variable for the last location of the mouse
        int myCounter;

        public formReports()
        {
            InitializeComponent();
            conn = new MySqlConnection("Server=localhost;Database=tetehardware;Uid=root;Pwd=root"); //connection
            this.Opacity = 0; //form transition using timer
            timer1.Start(); //form transition using timer
        }

        private void formReports_Load(object sender, EventArgs e)
        {
            monCalFrom.Location = txtDateFrom.Location;
            monCalFrom.Visible = true;
            int myScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            int myScreenHeight = Screen.PrimaryScreen.Bounds.Height;
/*
            //Change size and location of the form
            this.Size = new Size(530, 300);
            this.Location = new Point((myScreenWidth - this.Width) / 2, (myScreenHeight - this.Height) / 2);
            //Put Print button in the lowermiddle portion of the form
            btnPrintRep.Location = new Point((this.Width - btnPrintRep.Width) / 2, 225);
            btnPrintRep.Visible = true;
            datagridTableParent.Visible = false;
            datagridTableChild.Visible = false;
            */
            //set Dates
            datagridTableChild.RowTemplate.Height = 60;
            txtDateFrom.Text = DateTime.Now.ToString();
            txtDateTo.Text = DateTime.Now.ToString();
            monCalFrom.MinDate = Convert.ToDateTime("6/13/2017");
            monCalFrom.MaxDate = DateTime.Now;
            monCalTo.MaxDate = DateTime.Now;
            populateComboReport();
            comboReports.Focus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.1; //form transition using timer
        }

        //mouse handling
        private void formReports_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true; //sets mousedown to true
            lastLocation = e.Location; //gets the location of the form and sets it to lastlocation
        }

        private void formReports_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown) //if mouseDown is true, point to the last location of the mouse
            {
                this.Location = new Point((this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y); //gets the coordinates of the location of the mouse
                this.Update(); //updates the location of the mouse
            }
        }

        private void formReports_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false; //sets mousedown to false
        }

        //Hot Keys Handling - put any special keys with special functions here
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)   // To print report
            {
                if (myType == "Report")
                {
                    goPrint();
                }
                return true;    // indicate that you handled this keystroke
            }
            else if (keyData == Keys.Escape)     //Close Window
            {
                ReferenceToAfterLogin.Show();
                this.Dispose();
            }
            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //Buttons Handling - put all codes for any buttons here
        private void btnClose_Click(object sender, EventArgs e)
        {
            ReferenceToAfterLogin.Show();
            this.Dispose();
        }
        private void btnPrintRep_Click(object sender, EventArgs e)
        {
            goPrint();
        }

        //Date Handling - put all dates here
        private void txtDateFrom_Enter(object sender, EventArgs e)
        {
            monCalFrom.Location = txtDateFrom.Location;
            monCalFrom.Visible = true;
            monCalFrom.Focus();
        }
        private void txtDateTo_Enter(object sender, EventArgs e)
        {
            monCalTo.Location = txtDateTo.Location;
            monCalTo.Visible = true;
            monCalTo.Focus();
        }
        private void monCalFrom_DateSelected(object sender, DateRangeEventArgs e)
        {
            txtDateFrom.Text = monCalFrom.SelectionRange.Start.ToShortDateString();
            monCalTo.MinDate = monCalFrom.SelectionStart;
            monCalFrom.Visible = false;
            txtDateTo.Focus();
            maketheDataGrid();
        }

        private void monCalTo_DateSelected(object sender, DateRangeEventArgs e)
        {
            txtDateTo.Text = monCalTo.SelectionRange.Start.ToShortDateString();
            monCalTo.Visible = false;
            maketheDataGrid();
        }

        //Controls Handling
        private void comboReports_MouseClick(object sender, MouseEventArgs e)
        {
            maketheDataGrid();
        }

        private void txtDateFrom_Leave(object sender, EventArgs e)
        {
            getSQLDates();
            maketheDataGrid();
        }

        private void txtDateTo_Leave(object sender, EventArgs e)
        {
            getSQLDates();
            maketheDataGrid();
        }

        private void comboReports_Leave(object sender, EventArgs e)
        {
            getSQLDates();
            maketheDataGrid();
        }

        private void populateComboReport()
        {
            comboReports.Items.Clear();
            comboReports.Items.Add("Sales by Product");
            comboReports.Items.Add("Sales by Category");
            comboReports.Items.Add("Inventory by Products");
            comboReports.Items.Add("Products by Category");
            comboReports.Items.Add("Products by Supplier");
            comboReports.Items.Add("Good Deliveries by Supplier");
            comboReports.Items.Add("Bad Deliveries by Supplier");
            comboReports.Items.Add("Good Deliveries by Product");
            comboReports.Items.Add("Bad Deliveries by Product");
            comboReports.Items.Add("Returns To Supplier by Product");
            comboReports.Items.Add("Returns From Customer by Product");
            comboReports.Items.Add("Inhouse Damage by Product");
        }

        private void comboReports_SelectedIndexChanged(object sender, EventArgs e)
        {
            maketheDataGrid();
        }

        private void maketheDataGrid()
        { 
            int myRowIndex = comboReports.SelectedIndex;
            switch (myRowIndex)
            {
                case 0:     //Sales by Product
                    {
                        myOrderSQL = "ORDER by prodName";
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName AS 'Product Name' FROM tbl_product " + myOrderSQL);
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 6;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product/Item             ";
                        datagridTableChild.Columns[1].Name = "Date Sold";
                        datagridTableChild.Columns[2].Name = "Quantity";
                        datagridTableChild.Columns[3].Name = "Unit";
                        datagridTableChild.Columns[4].Name = "Sales";
                        datagridTableChild.Columns[5].Name = "Discount";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 75;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 125;
                        datagridTableChild.Columns[5].Width = 125;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        myTotSales = 0;
                        myTotDiscount = 0;
                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            myDatesSQL[2] = "b.transDate between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                           // MessageBox.Show("SELECT a.prodName, b.transDate, b.transQty, a.prodUnit, b.transTotPrice, b.transDiscount from tbl_product a, tbl_transact b WHERE a.prodID = b.prodID AND b.prodID ='" + myID + "' AND " + myDatesSQL[2]);
                            mySelectSQLChild = "SELECT a.prodName, b.transDate, b.transQty, a.prodUnit, b.transTotPrice, b.transDiscount from tbl_product a, tbl_transact b WHERE a.prodID = b.prodID AND b.prodID ='" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }
                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], func.stringToDecimal(reader[4].ToString(), 2), func.stringToDecimal(reader[5].ToString(), 2));
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount-1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;

                                        myTotSales = myTotSales + float.Parse(reader[4].ToString());
                                        myTotDiscount = myTotDiscount + float.Parse(reader[5].ToString());
                                        myGroupSales = myGroupSales + float.Parse(reader[4].ToString());
                                        myGroupDiscount = myGroupDiscount + float.Parse(reader[5].ToString());
                                    }
                                }
                                conn.Close();
                                if(myGroupSales > 0)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString() + ": Sub-Total", "", "", "", myGroupSales.ToString("#,#.00#"), myGroupDiscount.ToString("#,#.00#"));
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                    datagridTableChild.Rows.Add("", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.Rows.Add("Grand Total", "", "", "", myTotSales.ToString("#,#.00#"),myTotDiscount.ToString("#,#.00#"));
                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                        datagridTableChild.AllowUserToResizeRows = false;
                        datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }

                case 1:         //Sales by Category
                    {
                        populatedatagridParent("SELECT catID AS 'Catalog ID', catName AS 'Catalog Name' FROM tbl_productcatalog ORDER by catName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 6;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Category                 ";
                        datagridTableChild.Columns[1].Name = "Product";
                        datagridTableChild.Columns[2].Name = "Date Sold";
                        datagridTableChild.Columns[3].Name = "Quantity";
                        datagridTableChild.Columns[4].Name = "Sales";
                        datagridTableChild.Columns[5].Name = "Discount";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 100;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 125;
                        datagridTableChild.Columns[5].Width = 125;

                        for (int i=0; i<datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }

                        myTotSales = 0;
                        myTotDiscount = 0;
                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Catalog ID"].Value.ToString();
                            myDatesSQL[2] = "b.transDate between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            //MessageBox.Show(myID);
                            //MessageBox.Show("SELECT a.catName, c.prodName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b, tbl_product c WHERE a.catID = left(b.prodID,2) AND c.prodID = b.prodID AND left(b.prodID,2) ='" + myID + "' ORDER by a.catName, c.prodName", "", MessageBoxButtons.OK)
                            //MessageBox.Show("SELECT a.catName, c.prodName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b, tbl_product c WHERE a.catID = left(b.prodID,2) AND c.prodID = b.prodID AND left(b.prodID,2) ='" + myID + "' AND " + myDatesSQL[2] + " ORDER by a.catName, c.prodName");
                            mySelectSQLChild = "SELECT a.catName, c.prodName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b, tbl_product c WHERE a.catID = left(b.prodID,2) AND c.prodID = b.prodID AND left(b.prodID,2) ='" + myID + "' AND " + myDatesSQL[2] + " ORDER by a.catName, c.prodName";
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if(reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Catalog Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0].ToString() != "")
                                    {
                                        if (reader[0] != null)
                                        {
                                            myCounter++;
                                            datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], func.stringToDecimal(reader[4].ToString(), 2), func.stringToDecimal(reader[5].ToString(), 2));
                                            datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                            datagridTableChild.AllowUserToResizeRows = false;
                                            myTotSales = myTotSales + float.Parse(reader[4].ToString());
                                            myTotDiscount = myTotDiscount + float.Parse(reader[5].ToString());
                                            myGroupSales = myGroupSales + float.Parse(reader[4].ToString());
                                            myGroupDiscount = myGroupDiscount + float.Parse(reader[5].ToString());
                                        }
                                    }
                                }
                                conn.Close();
                                if (myGroupSales > 0)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Catalog Name"].Value.ToString() + ": Sub-Total", "", "", "", myGroupSales.ToString("#,#.00#"), myGroupDiscount.ToString("#,#.00#"));
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                    datagridTableChild.Rows.Add("", "", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.Rows.Add("Grand Total", "", "", "", myTotSales.ToString("#,#.00#"), myTotDiscount.ToString("#,#.00#"));
                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                        datagridTableChild.AllowUserToResizeRows = false;
                        datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 2:         //Inventory by Products
                    {
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName AS 'Product Name' FROM tbl_product ORDER by prodName");
                        //MessageBox.Show("SELECT a.catName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b WHERE a.prodID = left(b.prodID,2) AND left(b.prodID,2) ='" + myID + "'", "", MessageBoxButtons.OK);
                        mySelectSQLChild = "SELECT prodName, prodDesc, prodStock, prodUnit, prodStatus FROM tbl_product ORDER by prodName";

                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 5;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product";
                        datagridTableChild.Columns[1].Name = "Description";
                        datagridTableChild.Columns[2].Name = "Stock";
                        datagridTableChild.Columns[3].Name = "Unit";
                        datagridTableChild.Columns[4].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 200;
                        datagridTableChild.Columns[2].Width = 75;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[1].Width = 250;
                        datagridTableChild.Columns[4].Width = 350;

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            //MessageBox.Show("SELECT prodName, prodDesc, prodStock, prodUnit, prodStatus from tbl_product WHERE prodID ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT prodName, prodDesc, prodStock, prodUnit, prodStatus from tbl_product WHERE prodID ='" + myID + "'";
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                while (reader.Read())
                                {
                                    myCounter++;
                                    datagridTableChild.Rows.Add(reader[0], reader[1], reader[2], reader[3], reader[4]);
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCellsExceptHeader);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                }
                                conn.Close();
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 3:             //Products by Category
                    {
                        populatedatagridParent("SELECT catID AS 'Catalog ID', catName AS 'Catalog Name' FROM tbl_productcatalog ORDER by catName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 6;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Category                 ";
                        datagridTableChild.Columns[1].Name = "Product";
                        datagridTableChild.Columns[2].Name = "Description";
                        datagridTableChild.Columns[3].Name = "Stock";
                        datagridTableChild.Columns[4].Name = "Unit";
                        datagridTableChild.Columns[5].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 200;
                        datagridTableChild.Columns[2].Width = 250;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 75;
                        datagridTableChild.Columns[5].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[2].Width = 250;
                        datagridTableChild.Columns[5].Width = 350;

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Catalog ID"].Value.ToString();
                            //MessageBox.Show("SELECT a.catName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b WHERE a.prodID = left(b.prodID,2) AND left(b.prodID,2) ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.catName, b.prodName, b.prodDesc, b.prodStock, b.prodUnit, b.prodStatus FROM tbl_productcatalog a, tbl_product b WHERE LEFT(b.prodID,2) = catID AND catID = '" + myID + "'";
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if(reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Catalog Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }
                                while (reader.Read())
                                {
                                    myCounter++;
                                    datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4], reader[5]);
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                }
                                conn.Close();
                                if(myCounter>0)
                                {
                                    datagridTableChild.Rows.Add("","","","","","");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 4:             //Products by Supplier
                    {
                        populatedatagridParent("SELECT supID AS 'Supplier ID', supName AS 'Supplier Name' FROM tbl_supplier ORDER by supName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 7;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Item                     ";
                        datagridTableChild.Columns[1].Name = "Reference";
                        datagridTableChild.Columns[2].Name = "Product";
                        datagridTableChild.Columns[3].Name = "Description";
                        datagridTableChild.Columns[4].Name = "Stock";
                        datagridTableChild.Columns[5].Name = "Unit";
                        datagridTableChild.Columns[6].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 100;
                        datagridTableChild.Columns[3].Width = 150;
                        datagridTableChild.Columns[4].Width = 75;
                        datagridTableChild.Columns[5].Width = 75;
                        datagridTableChild.Columns[6].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[2].Width = 250;          //Set width of Status
                        datagridTableChild.Columns[6].Width = 350;          //Set width of Status

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Supplier ID"].Value.ToString();
                            //MessageBox.Show("SELECT a.catName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b WHERE a.prodID = left(b.prodID,2) AND left(b.prodID,2) ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.supName, c.arrRef, b.prodName, b.prodDesc, b.prodStock, b.prodUnit, b.prodStatus from tbl_supplier a, tbl_product b, tbl_arr c WHERE c.supID = a.supID AND b.prodID = c.prodID AND a.supID ='" + myID + "'";
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if(reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Supplier Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4], reader[5], reader[6]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                conn.Close();
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 5:             //Good Deliveries by Supplier
                    {
                        populatedatagridParent("SELECT supID AS 'Supplier ID', supName AS 'Supplier Name' FROM tbl_supplier ORDER by supName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 7;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Item                     ";
                        datagridTableChild.Columns[1].Name = "Reference";
                        datagridTableChild.Columns[2].Name = "Date Arrival";
                        datagridTableChild.Columns[3].Name = "Product";
                        datagridTableChild.Columns[4].Name = "Quantity";
                        datagridTableChild.Columns[5].Name = "Unit";
                        datagridTableChild.Columns[6].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 100;
                        datagridTableChild.Columns[3].Width = 100;
                        datagridTableChild.Columns[4].Width = 75;
                        datagridTableChild.Columns[5].Width = 75;
                        datagridTableChild.Columns[6].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[2].Width = 250;          //Set width of Product
                        datagridTableChild.Columns[5].Width = 350;          //Set width of Status

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Supplier ID"].Value.ToString();
                            myDatesSQL[2] = "b.dateArrival between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            myOrderSQL = " ORDER BY b.arrRef";
                            //MessageBox.Show("SELECT a.supName, b.dateArrival, c.prodName, b.Quantity, c.prodUnit, b.Status from tbl_supplier a, tbl_arr b, tbl_product c WHERE b.supID = a.supID AND c.prodID = b.prodID AND a.supID ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT b.arrRef, a.supName, b.dateArrival, c.prodName, b.Quantity, c.prodUnit, b.Status from tbl_supplier a, tbl_arr b, tbl_product c WHERE b.supID = a.supID AND c.prodID = b.prodID AND a.supID ='" + myID + "' AND " + myDatesSQL[2] + myOrderSQL;
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if(reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Supplier Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4], reader[5], reader[6]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                conn.Close();
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 6:         //Bad Deliveries by Supplier
                    {
                        populatedatagridParent("SELECT supID AS 'Supplier ID', supName AS 'Supplier Name' FROM tbl_supplier ORDER by supName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 6;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Item                     ";
                        datagridTableChild.Columns[1].Name = "Date Arrival";
                        datagridTableChild.Columns[2].Name = "Product";
                        datagridTableChild.Columns[3].Name = "Quantity";
                        datagridTableChild.Columns[4].Name = "Unit";
                        datagridTableChild.Columns[5].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 150;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 75;
                        datagridTableChild.Columns[5].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[2].Width = 250;          //Set width of Product
                        datagridTableChild.Columns[5].Width = 350;          //Set width of Status

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Supplier ID"].Value.ToString();
                            myDatesSQL[2] = "b.dateArrival between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            //MessageBox.Show("SELECT a.supName, b.dateArrival, c.prodName, b.Quantity, c.prodUnit, b.Status from tbl_supplier a, tbl_arr b, tbl_product c WHERE b.supID = a.supID AND c.prodID = b.prodID AND a.supID ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.supName, b.dateArrival, c.prodName, b.Quantity, c.prodUnit, b.Status from tbl_supplier a, tbl_arrdef b, tbl_product c WHERE b.supID = a.supID AND c.prodID = b.prodID AND a.supID ='" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Supplier Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }
                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4], reader[5]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                conn.Close();
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 7:             //Good Deliveries by Product
                    {
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName AS 'Product Name' FROM tbl_product ORDER by prodName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 5;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product/Item             ";
                        datagridTableChild.Columns[1].Name = "Date Arrived";
                        datagridTableChild.Columns[2].Name = "Quantity";
                        datagridTableChild.Columns[3].Name = "Unit";
                        datagridTableChild.Columns[4].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 75;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[4].Width = 400;

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            myDatesSQL[2] = "b.dateArrival between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            //MessageBox.Show("SELECT a.catName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b WHERE a.prodID = left(b.prodID,2) AND left(b.prodID,2) ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.prodName, b.dateArrival, b.Quantity, a.prodUnit, b.status from tbl_product a, tbl_arr b WHERE b.prodID = a.prodID AND a.prodID ='" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                conn.Close();
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 8:                //Bad Deliveries by Product
                    {
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName AS 'Product Name' FROM tbl_product ORDER by prodName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 5;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product/Item             ";
                        datagridTableChild.Columns[1].Name = "Date Arrived";
                        datagridTableChild.Columns[2].Name = "Quantity";
                        datagridTableChild.Columns[3].Name = "Unit";
                        datagridTableChild.Columns[4].Name = "Status";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 200;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 75;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[4].Width = 400;

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            myDatesSQL[2] = "b.dateArrival between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            //MessageBox.Show("SELECT a.catName, b.transDate, b.transQty, b.transTotPrice, b.transDiscount from tbl_productcatalog a, tbl_transact b WHERE a.prodID = left(b.prodID,2) AND left(b.prodID,2) ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.prodName, b.dateArrival, b.Quantity, a.prodUnit, b.status from tbl_product a, tbl_arrdef b WHERE b.prodID = a.prodID AND a.prodID ='" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                conn.Close();
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }

                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 9:         //Returns To Supplier by Product
                    {
                        //MessageBox.Show("SELECT b.supName, a.prodName, c.retQty, b.prodUnit, c.reDate, c.retDefect from tbl_supplier a, tbl_product b, tbl_arr c WHERE b.supID = c.supID AND a.prodID = c.prodID AND c.prodID ='" + myID + "'", "", MessageBoxButtons.OK);
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName as 'Product Name' FROM tbl_product ORDER by prodName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 6;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product                  ";
                        datagridTableChild.Columns[1].Name = "Description";
                        datagridTableChild.Columns[2].Name = "Quantity";
                        datagridTableChild.Columns[3].Name = "Unit";
                        datagridTableChild.Columns[4].Name = "Date Returned";
                        datagridTableChild.Columns[5].Name = "Defect";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 150;
                        datagridTableChild.Columns[1].Width = 200;
                        datagridTableChild.Columns[2].Width = 75;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 100;
                        datagridTableChild.Columns[5].Width = 150;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[0].Width = 250;          //Set width of Product
                        datagridTableChild.Columns[1].Width = 350;          //Set width of Status

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            myDatesSQL[2] = "b.retDate between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            mySelectSQLChild = "SELECT a.prodName, a.prodDesc, b.retQty, a.prodUnit, b.retDate, b.retDefect from tbl_product a, tbl_returnto b WHERE b.prodID = a.prodID AND a.prodID ='" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                conn.Close();
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }                         
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 10:            //Returns From Customer by Product
                    {
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName AS 'Product Name' FROM tbl_product ORDER by prodName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 7;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product                  ";
                        datagridTableChild.Columns[1].Name = "Customer";
                        datagridTableChild.Columns[2].Name = "Transaction No";
                        datagridTableChild.Columns[3].Name = "Quantity";
                        datagridTableChild.Columns[4].Name = "Unit";
                        datagridTableChild.Columns[5].Name = "Date Returned";
                        datagridTableChild.Columns[6].Name = "Defect";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 150;
                        datagridTableChild.Columns[1].Width = 150;
                        datagridTableChild.Columns[2].Width = 75;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 75;
                        datagridTableChild.Columns[5].Width = 100;
                        datagridTableChild.Columns[6].Width = 200;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[0].Width = 250;          //Set width of Product
                        datagridTableChild.Columns[1].Width = 250;          //Set width of Status

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            myDatesSQL[2] = "b.retDate between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            //MessageBox.Show("SELECT a.prodName, b.custName, b.transNum, b.retQty, a.prodUnit, b.retDate, b.retDefect from tbl_product a, tbl_returnfrom b WHERE b.prodID = a.prodID AND a.prodID = '" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.prodName, b.custName, b.transNum, b.retQty, a.prodUnit, b.retDate, b.retDefect from tbl_product a, tbl_returnfrom b WHERE b.prodID = a.prodID AND a.prodID = '" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4], reader[5], reader[6]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                                conn.Close();
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
                case 11:            //Inhouse Damage by Product
                    {
                        populatedatagridParent("SELECT prodID AS 'Product ID', prodName AS 'Product Name' FROM tbl_product ORDER by prodName");
                        //set up datagridchild columns
                        datagridTableChild.Rows.Clear();
                        datagridTableChild.ColumnCount = 6;
                        datagridTableChild.ColumnHeadersVisible = true;
                        datagridTableChild.Columns[0].Name = "Product                  ";
                        datagridTableChild.Columns[1].Name = "Date Damaged";
                        datagridTableChild.Columns[2].Name = "Damaged By";
                        datagridTableChild.Columns[3].Name = "Quantity";
                        datagridTableChild.Columns[4].Name = "Unit";
                        datagridTableChild.Columns[5].Name = "Details";
                        datagridTableChild.ColumnHeadersDefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                        datagridTableChild.Columns[0].Width = 150;
                        datagridTableChild.Columns[1].Width = 100;
                        datagridTableChild.Columns[2].Width = 200;
                        datagridTableChild.Columns[3].Width = 75;
                        datagridTableChild.Columns[4].Width = 75;
                        datagridTableChild.Columns[5].Width = 300;

                        for (int i = 0; i < datagridTableChild.ColumnCount; i++)
                        {
                            setDatagridChildAlignment(i);
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        datagridTableChild.Columns[2].Width = 250;
                        datagridTableChild.Columns[5].Width = 350;

                        for (int i = 0; i < datagridTableParent.RowCount; i++)
                        {
                            myGroupSales = 0;
                            myGroupDiscount = 0;
                            datagridTableParent.Rows[i].Selected = true;
                            myID = datagridTableParent.Rows[i].Cells["Product ID"].Value.ToString();
                            myDatesSQL[2] = "b.damDate between '" + txtDateFrom.Text + "' AND '" + txtDateTo.Text + "'";
                            //MessageBox.Show("SELECT a.prodName, b.damDate, b.damBy, b.damQty, a.prodUnit, b.damDetails FROM tbl_product a, tbl_damage b WHERE b.prodID = a.prodID AND a.prodID ='" + myID + "'", "", MessageBoxButtons.OK);
                            mySelectSQLChild = "SELECT a.prodName, b.damDate, b.damBy, b.damQty, a.prodUnit, b.damDetails FROM tbl_product a, tbl_damage b WHERE b.prodID = a.prodID AND a.prodID ='" + myID + "' AND " + myDatesSQL[2];
                            try
                            {
                                conn.Open();
                                MySqlCommand query = new MySqlCommand(mySelectSQLChild, conn);
                                MySqlDataReader reader = query.ExecuteReader();
                                myCounter = 0;
                                if (reader.HasRows)
                                {
                                    datagridTableChild.Rows.Add(datagridTableParent.Rows[i].Cells["Product Name"].Value.ToString());
                                    datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                    datagridTableChild.AllowUserToResizeRows = false;
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].DefaultCellStyle.Font = new Font(this.Font, FontStyle.Bold);
                                }

                                while (reader.Read())
                                {
                                    if (reader[0] != null)
                                    {
                                        myCounter++;
                                        datagridTableChild.Rows.Add(myCounter, reader[1], reader[2], reader[3], reader[4], reader[5]);
                                        datagridTableChild.AutoResizeRow(datagridTableChild.RowCount - 1, DataGridViewAutoSizeRowMode.AllCells);
                                        datagridTableChild.AllowUserToResizeRows = false;
                                    }
                                }
                                if (!(myCounter == 0))
                                {
                                    datagridTableChild.Rows.Add("", "", "", "", "", "");
                                    datagridTableChild.Rows[datagridTableChild.RowCount - 1].Height = 8;
                                }
                                conn.Close();
                            }
                            catch (Exception x)

                            {
                                MessageBox.Show("Error in Load:" + x.ToString());
                                conn.Close();
                            }
                        }
                        datagridTableChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                        break;
                    }
            }
            noSortColumn();
        }


        private void populatedatagridParent(string selectCommand)
        {
            datagridTableParent.DataSource = null;      //remove datasource link for datagridProduct
            //MessageBox.Show("Parent - " + selectCommand,"",MessageBoxButtons.OK);
            try
            {
                conn.Open(); //opens the connection
                MySqlCommand query = new MySqlCommand(selectCommand, conn); //query to select all entries in tbl_productcatalog
                MySqlDataAdapter adp = new MySqlDataAdapter(query); //adapter for query
                DataTable dt = new DataTable(); //datatable for adapter
                BindingSource bs = new BindingSource();
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
                adp.Fill(dt);
                bs.DataSource = dt;
                datagridTableParent.DataSource = bs;
                conn.Close();
                datagridTableParent.AutoResizeRows();
            }
            catch (Exception x)
            {
                MessageBox.Show("Error in populating datagridTable : " + x.ToString());
                conn.Close();
            }
        }


        //set datagridChild tbl here

        private void populatedatagridChild(string selectCommand)
        {
            try
            {
                conn.Open();
                MySqlCommand query = new MySqlCommand(selectCommand, conn);
                MySqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    datagridTableChild.Rows.Add();
                }
                conn.Close();
                //datagridTableChild.AutoResizeRows();
            }
            catch (Exception x)
            {
                MessageBox.Show("Error in Child table:" + x.ToString());
                conn.Close();
            }
        }

        private void setDatagridChildAlignment(int mycolNum)
        {
            if (datagridTableChild.Columns[mycolNum].Name == "Sales" || datagridTableChild.Columns[mycolNum].Name == "Discount")
            {
                datagridTableChild.Columns[mycolNum].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                datagridTableChild.Columns[mycolNum].HeaderCell.Style.Alignment = DataGridViewContentAlignment.TopRight;
            }
            else if (datagridTableChild.Columns[mycolNum].Name == "Quantity" || datagridTableChild.Columns[mycolNum].Name == "Stock")
            {
                datagridTableChild.Columns[mycolNum].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
                datagridTableChild.Columns[mycolNum].HeaderCell.Style.Alignment = DataGridViewContentAlignment.TopCenter;
            }
            else
            {
                datagridTableChild.Columns[mycolNum].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
                datagridTableChild.Columns[mycolNum].HeaderCell.Style.Alignment = DataGridViewContentAlignment.TopLeft;
            }
//          datagridTableChild.AutoResizeColumns();
        }

        private void goPrint()
        {
            if (!(comboReports.Text == ""))
            {
                string myDate;
                string myStore = "Toril Advance Marketing Corporation" + Environment.NewLine + "J Saavedra St., Toril, Davao City, 8000 Davao del Sur" + Environment.NewLine + "(082) 291 0053" + Environment.NewLine;
                if (txtDateFrom.Text == txtDateTo.Text)
                {
                    myDate = "Daily Report: " + txtDateFrom.Text;
                }
                else
                {
                    myDate = "From " + txtDateFrom.Text + " To " + txtDateTo.Text;
                }
                ClsPrint printIT = new ClsPrint(datagridTableChild, myStore + comboReports.Text + Environment.NewLine + myDate);
                printIT.PrintForm();
            }
        }

        private void getSQLDates()
        {
            myDatesSQL[0] = txtDateFrom.Text;
            myDatesSQL[1] = txtDateTo.Text;
        }

        private void noSortColumn()
        {
            //set columns as notsortable
            for (int i = 0; i < datagridTableChild.ColumnCount - 1; i++)
            {
                datagridTableChild.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

    }
}
