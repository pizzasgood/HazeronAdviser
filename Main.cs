﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HazeronAdviser
{
    public partial class Main : Form
    {
        List<HCity> hCityList = new List<HCity>();
        List<HShip> hShipList = new List<HShip>();
        List<HOfficer> hOfficerList = new List<HOfficer>();
        List<HEvent> hEventList = new List<HEvent>();

        List<int> charFilterList = new List<int>();
        List<HCharacter> charList = new List<HCharacter>();

        Image imageCity;
        Image imageShip;
        Image imageOfficer;
        Image imageDiplomacy;
        Image imageFriend;
        Image imageGovernment;
        Image imageChannel;
        Image imageAccept;
        Image imageTreasury;
        Image imageLocator;
        Image imageStation;
        Image imageVoice;
        Image imageFlag;
        Image imageTarget;

        Color attantionMinor = Color.FromArgb(255, 255, 150); // Somewhere between LightYellow and Yellow.
        Color attantionMajor = Color.LightPink;

        string hMailFolder;

        public Main()
        {
            InitializeComponent();
            #if DEBUG
            this.Text += " (DEBUG MODE)";
            #endif
            toolStripProgressBar1.Visible = false;
            toolStripProgressBar2.Visible = false;
            imageCity = HazeronAdviser.Properties.Resources.MsgCity;
            imageShip = HazeronAdviser.Properties.Resources.c_Spacecraft;
            imageOfficer = HazeronAdviser.Properties.Resources.Officer;
            imageDiplomacy = HazeronAdviser.Properties.Resources.CommDiplomacy;
            imageFriend = HazeronAdviser.Properties.Resources.CommFriend;
            imageGovernment = HazeronAdviser.Properties.Resources.CommGovernment;
            imageChannel = HazeronAdviser.Properties.Resources.CommStdChannel;
            imageAccept = HazeronAdviser.Properties.Resources.MsgAccept;
            imageTreasury = HazeronAdviser.Properties.Resources.c_Money;
            imageLocator = HazeronAdviser.Properties.Resources.Locator;
            imageStation = HazeronAdviser.Properties.Resources.CommStation;
            imageVoice = HazeronAdviser.Properties.Resources.CommVoice;
            imageFlag = HazeronAdviser.Properties.Resources.c_Flag;
            imageTarget = HazeronAdviser.Properties.Resources.MsgSpot;
            dgvCity.Columns["ColumnCityAbandonment"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCity.Columns["ColumnCityAbandonment"].DefaultCellStyle.Font = new Font("Lucida Console", 9);
            dgvShip.Columns["ColumnShipAbandonment"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvShip.Columns["ColumnShipAbandonment"].DefaultCellStyle.Font = new Font("Lucida Console", 9);
            cmbCharFilter.SelectedIndex = 0;
#if !DEBUG
            hMailFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Shores of Hazeron", "Mail"); // %USERPROFILE%\Shores of Hazeron\Mail
#else
            hMailFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Shores of Hazeron", "Mail (Backup)"); // %USERPROFILE%\Shores of Hazeron\Mail (Backup)
#endif
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripProgressBar2.Visible = false;

            // Scan the Hazeron Mail folder and get all file names.
            if (!Directory.Exists(hMailFolder))
            {
                toolStripStatusLabel1.Text = "\"" + hMailFolder + "\" does not exist.";
                if (DialogResult.Yes == MessageBox.Show("Could not find Hazeron Mail folder:" + Environment.NewLine + hMailFolder + Environment.NewLine + Environment.NewLine + "Copy directory path to clipboard?", "Mail Folder Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2))
                    Clipboard.SetText(hMailFolder);
                return;
            }
            string[] fileList = Directory.GetFiles(hMailFolder);

            // Clear Character Filter dropdown box.
            cmbCharFilter.Enabled = false;
            cmbCharFilter.Items.Clear();
            cmbCharFilter.Items.Add("Show all");
            cmbCharFilter.SelectedIndex = 0;

            // Clear the DataGridView tables.
            dgvCity.Rows.Clear();
            dgvShip.Rows.Clear();
            dgvOfficer.Rows.Clear();
            dgvEvent.Rows.Clear();
            dgvCity.Refresh();
            dgvShip.Refresh();
            dgvOfficer.Refresh();
            dgvEvent.Refresh();

            ClearSelectedInfo();

            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = fileList.Length;
            toolStripProgressBar1.Visible = true;
            toolStripStatusLabel1.Text = "Scanning mails...";
            toolStripStatusLabel1.Invalidate();
            statusStrip1.Update();
            #region Scan HMails
            foreach (string file in fileList)
            {
                if (HMail.IsUni4(file)) // Check if signature is 0x2110 before trying to read it.
                {
                    #if !DEBUG
                    try
                    {
                    #endif
                        HMail mail = new HMail(file);
                        if (HMail.IsCityReport(mail))
                        {
                            HCity temp = new HCity(mail);
                            if (hCityList.Any(city => city.ID == temp.ID))
                                hCityList.Find(city => city.ID == temp.ID).Update(mail);
                            else
                                hCityList.Add(temp);
                        }
                        else if (HMail.IsShipLog(mail))
                        {
                            HShip temp = new HShip(mail);
                            if (hShipList.Any(ship => ship.ID == temp.ID))
                                hShipList.Find(ship => ship.ID == temp.ID).Update(mail);
                            else
                                hShipList.Add(temp);
                        }
                        else if (HMail.IsOfficerTenFour(mail))
                        {
                            HOfficer temp = new HOfficer(mail);
                            if (hOfficerList.Any(officer => officer.ID == temp.ID))
                                hOfficerList.Find(officer => officer.ID == temp.ID).Update(mail);
                            else
                                hOfficerList.Add(temp);
                        }
                        else if (HMail.IsEventNotice(mail))
                        {
                            HEvent temp = new HEvent(mail);
                            if (hEventList.Any(notice => notice.MessageID == temp.MessageID))
                                hEventList.Find(notice => notice.MessageID == temp.MessageID).Update(mail);
                            else
                                hEventList.Add(temp);
                        }
                        else if (mail.MessageType == 0x00 && !charList.Any(x => x.IdNum == mail.SenderID)) // Add to character list.
                            charList.Add(new HCharacter(mail));
                        if (!charFilterList.Contains(mail.RecipientID))
                            charFilterList.Add(mail.RecipientID);
                        toolStripProgressBar1.Increment(1);
                    #if !DEBUG
                    }
                    catch (IOException ioex)
                    {
                        System.Diagnostics.Debug.WriteLine("### Error while scanning mail file:");
                        System.Diagnostics.Debug.WriteLine("### " + ioex.ToString());
                        toolStripStatusLabel1.Text = "Error while scanning mail file: " + file;
                        if (DialogResult.Yes == MessageBox.Show("Failed to located or open mail file:" + Environment.NewLine + file + Environment.NewLine + Environment.NewLine + "Copy mail filepath to clipboard?", "Mail Scanning Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2))
                            Clipboard.SetText(file);
                        continue; // Continue reading the rest of the mails even though one failed, may cause more than one popup to appear if multiple failures.
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("### Error while reading mail file:");
                        System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
                        toolStripStatusLabel1.Text = "Error while reading mail file: " + file;
                        if (DialogResult.Yes == MessageBox.Show("Failed reading mail file:" + Environment.NewLine + file + Environment.NewLine + Environment.NewLine + "Copy mail filepath to clipboard?", "Mail Reading Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2))
                            Clipboard.SetText(file);
                        continue; // Continue reading the rest of the mails even though one failed, may cause more than one popup to appear if multiple failures.
                    }
                    #endif
                }
            }
            #endregion
            toolStripProgressBar2.Value = 0;
            toolStripProgressBar2.Maximum = hCityList.Count + (hShipList.Count * 2) + hOfficerList.Count + hEventList.Count;
            toolStripProgressBar2.Visible = true;
            toolStripStatusLabel1.Text = "Filling tables...";
            toolStripStatusLabel1.Invalidate();
            statusStrip1.Update();
            #region Fill City Table
            foreach (var hCity in hCityList)
            {
                dgvCity.Rows.Add();
                int row = dgvCity.RowCount - 1;
                dgvCity.Rows[row].Cells["ColumnCityIndex"].Value = row;
                dgvCity.Rows[row].Cells["ColumnCitySelection"].Value = false;
                dgvCity.Rows[row].Cells["ColumnCityIcon"].Value = imageCity;
                dgvCity.Rows[row].Cells["ColumnCityName"].Value = hCity.Name;
                dgvCity.Rows[row].Cells["ColumnCityMorale"].Value = hCity.SMoraleShort;
                dgvCity.Rows[row].Cells["ColumnCityAbandonment"].Value = hCity.SDecayDay;
                dgvCity.Rows[row].Cells["ColumnCityPopulation"].Value = hCity.SPopulationShort;
                dgvCity.Rows[row].Cells["ColumnCityLoyalty"].Value = hCity.SLoyalty;
                dgvCity.Rows[row].Cells["ColumnCityLivingConditions"].Value = hCity.SLivingShort;
                dgvCity.Rows[row].Cells["ColumnCityDate"].Value = hCity.LastUpdaredString;
                // AttentionCodes
                if (hCity.AttentionCode != 0x00)
                {
                    dgvCity.Rows[row].Cells["ColumnCityName"].Style.BackColor = attantionMinor;
                    if (HHelper.FlagCheck(hCity.AttentionCode, 0x01)) // 0b00000001 // More jobs than homes, or too many unemployed.
                        dgvCity.Rows[row].Cells["ColumnCityLivingConditions"].Style.BackColor = attantionMinor;
                    if (HHelper.FlagCheck(hCity.AttentionCode, 0x02)) // 0b00000010 // Population not full, or more than full.
                        dgvCity.Rows[row].Cells["ColumnCityPopulation"].Style.BackColor = attantionMinor;
                    if (HHelper.FlagCheck(hCity.AttentionCode, 0x04)) // 0b00000100 // Less than 12 days to decay.
                        dgvCity.Rows[row].Cells["ColumnCityAbandonment"].Style.BackColor = attantionMinor;
                    if (HHelper.FlagCheck(hCity.AttentionCode, 0x08)) // 0b00001000 // Less than 4 days to decay.
                        dgvCity.Rows[row].Cells["ColumnCityAbandonment"].Style.BackColor = attantionMajor;
                    if (HHelper.FlagCheck(hCity.AttentionCode, 0x10)) // 0b00010000 // Population is 0, or zone over populated!
                        dgvCity.Rows[row].Cells["ColumnCityPopulation"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hCity.AttentionCode, 0x20)) // 0b00100000 // Nothing yet!
                    //    dgvCity.Rows[row].Cells["ColumnCityIndex"].Style.BackColor = attantionMajor;
                    if (HHelper.FlagCheck(hCity.AttentionCode, 0x40)) // 0b01000000 // Morale not full.
                        dgvCity.Rows[row].Cells["ColumnCityMorale"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hCity.AttentionCode, 0x80)) // 0b10000000 // Nothing yet!
                    //    dgvCity.Rows[row].Cells["ColumnCityIndex"].Style.BackColor = attantionMajor;
                }
                toolStripProgressBar2.Increment(1);
            }
            #endregion
            #region Fill Ship Table
            foreach (var hShip in hShipList)
            {
                dgvShip.Rows.Add();
                int row = dgvShip.RowCount - 1;
                dgvShip.Rows[row].Cells["ColumnShipIndex"].Value = row;
                dgvShip.Rows[row].Cells["ColumnShipSelection"].Value = false;
                dgvShip.Rows[row].Cells["ColumnShipIcon"].Value = imageShip;
                dgvShip.Rows[row].Cells["ColumnShipName"].Value = hShip.Name;
                dgvShip.Rows[row].Cells["ColumnShipAbandonment"].Value = hShip.DecayDay;
                dgvShip.Rows[row].Cells["ColumnShipFuel"].Value = hShip.FuelShort;
                dgvShip.Rows[row].Cells["ColumnShipDamage"].Value = hShip.DamageShort;
                dgvShip.Rows[row].Cells["ColumnShipDate"].Value = hShip.LastUpdaredString;
                // AttentionCodes
                if (hShip.AttentionCode != 0x00)
                {
                    if (HHelper.FlagCheck(hShip.AttentionCode, 0x01)) // 0b00000001 // 2 weeks until decay.
                        dgvShip.Rows[row].Cells["ColumnShipAbandonment"].Style.BackColor = attantionMinor;
                    if (HHelper.FlagCheck(hShip.AttentionCode, 0x02)) // 0b00000010 // 1 weeks until decay.
                        dgvShip.Rows[row].Cells["ColumnShipAbandonment"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hShip.AttentionCode, 0x04)) // 0b00000100 // Nothing yet!
                    //    dgvShip.Rows[row].Cells["ColumnShipIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hShip.AttentionCode, 0x08)) // 0b00001000 // Nothing yet!
                    //    dgvShip.Rows[row].Cells["ColumnShipIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hShip.AttentionCode, 0x10)) // 0b00010000 // Nothing yet!
                    //    dgvShip.Rows[row].Cells["ColumnShipIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hShip.AttentionCode, 0x20)) // 0b00100000 // Nothing yet!
                    //    dgvShip.Rows[row].Cells["ColumnShipIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hShip.AttentionCode, 0x40)) // 0b01000000 // Nothing yet!
                    //    dgvShip.Rows[row].Cells["ColumnShipIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hSHip.AttentionCode, 0x80)) // 0b10000000 // Nothing yet!
                    //    dgvShip.Rows[row].Cells["ColumnShipIndex"].Style.BackColor = attantionMajor;
                }
                toolStripProgressBar2.Increment(1);
            }
            #endregion
            #region Fill Officer Table
            foreach (var hOfficer in hOfficerList)
            {
                dgvOfficer.Rows.Add();
                int row = dgvOfficer.RowCount - 1;
                dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Value = row;
                dgvOfficer.Rows[row].Cells["ColumnOfficerSelection"].Value = false;
                dgvOfficer.Rows[row].Cells["ColumnOfficerIcon"].Value = imageOfficer;
                dgvOfficer.Rows[row].Cells["ColumnOfficerName"].Value = hOfficer.Name;
                dgvOfficer.Rows[row].Cells["ColumnOfficerHome"].Value = hOfficer.Home;
                dgvOfficer.Rows[row].Cells["ColumnOfficerLocation"].Value = hOfficer.Location;
                dgvOfficer.Rows[row].Cells["ColumnOfficerDate"].Value = hOfficer.LastUpdaredString;
                // AttentionCodes
                if (hOfficer.AttentionCode != 0x00)
                {
                    dgvOfficer.Rows[row].Cells["ColumnOfficerName"].Style.BackColor = attantionMinor;
                    if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x01)) // 0b00000001 // MSG_OfficerContact
                        dgvOfficer.Rows[row].Cells["ColumnOfficerLocation"].Style.BackColor = attantionMinor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x02)) // 0b00000010 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x04)) // 0b00000100 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x08)) // 0b00001000 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x10)) // 0b00010000 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x20)) // 0b00100000 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x40)) // 0b01000000 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hOfficer.AttentionCode, 0x80)) // 0b10000000 // Nothing yet!
                    //    dgvOfficer.Rows[row].Cells["ColumnOfficerName"].Style.BackColor = attantionMajor;
                }
                toolStripProgressBar2.Increment(1);
            }
            foreach (var hShipOfficer in hShipList)
            {
                dgvOfficer.Rows.Add();
                int row = dgvOfficer.RowCount - 1;
                dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Value = -(hShipList.FindIndex(ship => ship.ID == hShipOfficer.ID) + 1);
                dgvOfficer.Rows[row].Cells["ColumnOfficerSelection"].Value = false;
                dgvOfficer.Rows[row].Cells["ColumnOfficerIcon"].Value = imageShip;
                dgvOfficer.Rows[row].Cells["ColumnOfficerName"].Value = hShipOfficer.OfficerName;
                dgvOfficer.Rows[row].Cells["ColumnOfficerHome"].Value = hShipOfficer.OfficerHome;
                dgvOfficer.Rows[row].Cells["ColumnOfficerLocation"].Value = hShipOfficer.Name;
                dgvOfficer.Rows[row].Cells["ColumnOfficerDate"].Value = hShipOfficer.LastUpdaredString;
                // AttentionCodes
                //if (hShipOfficer.AttentionCode != 0x00)
                //{
                //    dgvOfficer.Rows[row].Cells["ColumnOfficerName"].Style.BackColor = attantionMinor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x01)) // 0b00000001 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x02)) // 0b00000010 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x04)) // 0b00000100 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x08)) // 0b00001000 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x10)) // 0b00010000 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x20)) // 0b00100000 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x40)) // 0b01000000 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerIndex"].Style.BackColor = attantionMajor;
                //    if (HHelper.FlagCheck(hShipOfficer.AttentionCode, 0x80)) // 0b10000000 // Nothing yet!
                //        dgvOfficer.Rows[row].Cells["ColumnOfficerName"].Style.BackColor = attantionMajor;
                //}
                toolStripProgressBar2.Increment(1);
            }
            #endregion
            #region Fill Event Table
            foreach (var hEvent in hEventList)
            {
                dgvEvent.Rows.Add();
                int row = dgvEvent.RowCount - 1;
                dgvEvent.Rows[row].Cells["ColumnEventIndex"].Value = row;
                dgvEvent.Rows[row].Cells["ColumnEventSelection"].Value = false;
                if (hEvent.MessageType == 0x13) // MSG_Government
                {
                    if (hEvent.Name == "State Department")
                        dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageFriend;
                    if (hEvent.Name == "War Department")
                        dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageTarget;
                    if (hEvent.Name == "Treasury Department")
                        dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageTreasury;
                }
                else if (hEvent.MessageType == 0x16) // MSG_OfficerDeath
                    dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageOfficer;
                else if (hEvent.MessageType == 0x17) // MSG_CityFinalDecayReport
                    dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageCity;
                else if (hEvent.MessageType == 0x12) // MSG_ShipLogFinal
                    dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageShip;
                else if (hEvent.MessageType == 0x18) // MSG_DiplomaticMessage (?)
                    dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageDiplomacy;
                else if (hEvent.MessageType == 0x03 || hEvent.MessageType == 0x05) // MSG_CityOccupationReport or MSG_CityIntelligenceReport
                    dgvEvent.Rows[row].Cells["ColumnEventIcon"].Value = imageFlag;
                dgvEvent.Rows[row].Cells["ColumnEventName"].Value = hEvent.Name;
                dgvEvent.Rows[row].Cells["ColumnEventSubject"].Value = hEvent.Subject;
                dgvEvent.Rows[row].Cells["ColumnEventDate"].Value = hEvent.LastUpdaredString;
                // AttentionCodes
                if (hEvent.AttentionCode != 0x00)
                {
                    //dgvEvent.Rows[row].Cells["ColumnEventName"].Style.BackColor = attantionMinor;
                    //if (HHelper.FlagCheck(hEvent.AttentionCode, 0x01)) // 0b00000001 // Nothing yet!
                    //    dgvEvent.Rows[row].Cells["ColumnEventIndex"].Style.BackColor = attantionMinor;
                    //if (HHelper.FlagCheck(hEvent.AttentionCode, 0x02)) // 0b00000010 // Nothing yet!
                    //    dgvEvent.Rows[row].Cells["ColumnEventIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hEvent.AttentionCode, 0x04)) // 0b00000100 // Nothing yet!
                    //    dgvEvent.Rows[row].Cells["ColumnEventIndex"].Style.BackColor = attantionMajor;
                    if (HHelper.FlagCheck(hEvent.AttentionCode, 0x08)) // 0b00001000 // Attantion
                        dgvEvent.Rows[row].Cells["ColumnEventName"].Style.BackColor = attantionMinor;
                    //if (HHelper.FlagCheck(hEvent.AttentionCode, 0x10)) // 0b00010000 // Nothing yet!
                    //    dgvEvent.Rows[row].Cells["ColumnEventIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hEvent.AttentionCode, 0x20)) // 0b00100000 // Nothing yet!
                    //    dgvEvent.Rows[row].Cells["ColumnEventIndex"].Style.BackColor = attantionMajor;
                    //if (HHelper.FlagCheck(hEvent.AttentionCode, 0x40)) // 0b01000000 // Nothing yet!
                    //    dgvEvent.Rows[row].Cells["ColumnEventIndex"].Style.BackColor = attantionMajor;
                    if (HHelper.FlagCheck(hEvent.AttentionCode, 0x80)) // 0b10000000 // Death
                        dgvEvent.Rows[row].Cells["ColumnEventName"].Style.BackColor = attantionMajor;
                }
                toolStripProgressBar2.Increment(1);
            }
            #endregion

            foreach (int charId in charFilterList) // Fill the Character Filter dropdown box.
            {
                if (charList.Any(x => x.IdNum == charId))
                {
                    HCharacter hChar = charList.Find(x => x.IdNum == charId);
                    cmbCharFilter.Items.Add(hChar.Name + " (" + hChar.ID + ")");
                }
                else
                    cmbCharFilter.Items.Add("??? (" + HHelper.ToID(charId) + ")");
            }
            cmbCharFilter.Enabled = true;

            toolStripProgressBar1.Visible = false;
            toolStripProgressBar2.Visible = false;
            ClearSeletion();
            toolStripStatusLabel1.Text = "Done!";
        }

        private void ClearSeletion()
        {
            dgvCity.ClearSelection();
            dgvShip.ClearSelection();
            dgvOfficer.ClearSelection();
            dgvEvent.ClearSelection();
        }

        private void ClearSelectedInfo()
        {
            tbxCity.Clear();
            tbxShip.Clear();
            tbxOfficer.Clear();
            tbxEvent.Clear();
            tabControlCity.Refresh();
            tabControlShip.Refresh();
            tabControlOfficer.Refresh();
            tabControlEvent.Refresh();
        }

        #region List Selection
        private void dgvCity_SelectionChanged(object sender, EventArgs e)
        {
            ClearSelectedInfo();
            if (dgvCity.SelectedRows.Count != 0 && dgvCity.SelectedRows[0].Index != -1 && dgvCity.Rows[(int)dgvCity.SelectedRows[0].Index].Cells["ColumnCityIndex"].Value != null)
            {
                int rowIndex = (int)dgvCity.SelectedRows[0].Index;
                int listIndex = (int)dgvCity.Rows[rowIndex].Cells["ColumnCityIndex"].Value;
                HCity city = hCityList[listIndex];
                rtbCityMorale.Text = city.SMorale;
                rtbCityPopulation.Text = city.SPopulation;
                rtbCityLivingConditions.Text = city.SLiving;
                tbxCity.Text = city.BodyTest;
                // Refresh graphs to make them update.
                pCityOverviewPopulation.Refresh();
                pCityOverviewMorale.Refresh();
            }
        }

        private void dgvShip_SelectionChanged(object sender, EventArgs e)
        {
            ClearSelectedInfo();
            if (dgvShip.SelectedRows.Count != 0 && dgvShip.SelectedRows[0].Index != -1 && dgvShip.Rows[(int)dgvShip.SelectedRows[0].Index].Cells["ColumnShipIndex"].Value != null)
            {
                int rowIndex = (int)dgvShip.SelectedRows[0].Index;
                int listIndex = (int)dgvShip.Rows[rowIndex].Cells["ColumnShipIndex"].Value;
                HShip ship = hShipList[listIndex];
                rtbShipOverview.Text = ship.Account + Environment.NewLine + Environment.NewLine + ship.Cargo;
                tbxShip.Text = ship.BodyTest;
            }
        }

        private void dgvOfficer_SelectionChanged(object sender, EventArgs e)
        {
            ClearSelectedInfo();
            if (dgvOfficer.SelectedRows.Count != 0 && dgvOfficer.SelectedRows[0].Index != -1 && dgvOfficer.Rows[(int)dgvOfficer.SelectedRows[0].Index].Cells["ColumnOfficerIndex"].Value != null)
            {
                int rowIndex = (int)dgvOfficer.SelectedRows[0].Index;
                int listIndex = (int)dgvOfficer.Rows[rowIndex].Cells["ColumnOfficerIndex"].Value;
                if (listIndex >= 0)
                {
                    HOfficer officer = hOfficerList[listIndex];
                    tbxOfficer.Text = officer.BodyTest;
                }
                else
                {
                    HShip ship = hShipList[Math.Abs(listIndex) - 1];
                    tbxOfficer.Text = ship.BodyTest;
                }
            }
        }

        private void dgvEvent_SelectionChanged(object sender, EventArgs e)
        {
            ClearSelectedInfo();
            if (dgvEvent.SelectedRows.Count != 0 && dgvEvent.SelectedRows[0].Index != -1 && dgvEvent.Rows[(int)dgvEvent.SelectedRows[0].Index].Cells["ColumnEventIndex"].Value != null)
            {
                int rowIndex = (int)dgvEvent.SelectedRows[0].Index;
                int listIndex = (int)dgvEvent.Rows[rowIndex].Cells["ColumnEventIndex"].Value;
                HEvent hevent = hEventList[listIndex]; // "event" is a reserved word.
                tbxEvent.Text = hevent.BodyTest;
            }
        }
        #endregion

        #region Character Filter
        private void cmbCharFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCharFilter.Enabled)
            {
                if (cmbCharFilter.SelectedIndex == 0)
                {
                    foreach (DataGridViewRow row in dgvCity.Rows)
                        row.Visible = true;
                    foreach (DataGridViewRow row in dgvShip.Rows)
                        row.Visible = true;
                    foreach (DataGridViewRow row in dgvOfficer.Rows)
                        row.Visible = true;
                    foreach (DataGridViewRow row in dgvEvent.Rows)
                        row.Visible = true;
                }
                else
                {
                    int charId = charFilterList[cmbCharFilter.SelectedIndex - 1];
                    foreach (DataGridViewRow row in dgvCity.Rows)
                    {
                        int listIndex = (int)row.Cells["ColumnCityIndex"].Value;
                        row.Visible = hCityList[listIndex].Onwers.Contains(charId);
                    }
                    foreach (DataGridViewRow row in dgvShip.Rows)
                    {
                        int listIndex = (int)row.Cells["ColumnShipIndex"].Value;
                        row.Visible = hShipList[listIndex].Onwers.Contains(charId);
                    }
                    foreach (DataGridViewRow row in dgvOfficer.Rows)
                    {
                        int listIndex = (int)row.Cells["ColumnOfficerIndex"].Value;
                        if (listIndex >= 0)
                            row.Visible = hOfficerList[listIndex].Onwers.Contains(charId);
                        else
                            row.Visible = hShipList[Math.Abs(listIndex) - 1].Onwers.Contains(charId);
                    }
                    foreach (DataGridViewRow row in dgvEvent.Rows)
                    {
                        int listIndex = (int)row.Cells["ColumnEventIndex"].Value;
                        row.Visible = hEventList[listIndex].Onwers.Contains(charId);
                    }
                }
                ClearSelectedInfo();
                ClearSeletion();
            }
        }
        #endregion

        #region Graph Graphics
        private void pCityPopulation_Paint(object sender, PaintEventArgs e)
        {
            if (dgvCity.SelectedRows.Count != 0 && dgvCity.SelectedRows[0].Index != -1 && dgvCity.Rows[(int)dgvCity.SelectedRows[0].Index].Cells["ColumnCityIndex"].Value != null)
            {
                HCity city = hCityList[(int)dgvCity.Rows[(int)dgvCity.SelectedRows[0].Index].Cells["ColumnCityIndex"].Value];
                int yValue;
                BarGraph graphPop = new BarGraph(sender, e);
                graphPop.DrawXAxle("?", 5);
                graphPop.DrawYAxle("Population", (int)(city.VPopulationLimit * 1.05));
                yValue = city.VLoyalty;
                if (yValue > 0)
                    graphPop.DrawBar(Color.Yellow, 0, yValue);
                else if (yValue < 0)
                    graphPop.DrawBar(Color.Orange, 0, yValue);
                yValue = city.VPopulation;
                graphPop.DrawBar(Color.LightGreen, 1, yValue);
                yValue = city.VHomes;
                graphPop.DrawBar(Color.Green, 2, yValue);
                yValue = city.VJobs;
                graphPop.DrawBar(Color.Blue, 3, yValue);
                yValue = city.VPopulationLimit;
                graphPop.DrawBar(Color.Red, 4, yValue);
            }
        }

        private void pCityMorale_Paint(object sender, PaintEventArgs e)
        {
            if (dgvCity.SelectedRows.Count != 0 && dgvCity.SelectedRows[0].Index != -1 && dgvCity.Rows[(int)dgvCity.SelectedRows[0].Index].Cells["ColumnCityIndex"].Value != null)
            {
                HCity city = hCityList[(int)dgvCity.Rows[(int)dgvCity.SelectedRows[0].Index].Cells["ColumnCityIndex"].Value];
                int yValue;
                BarGraph graphMorale = new BarGraph(sender, e);
                graphMorale.DrawXAxle("?", 3);
                graphMorale.DrawYAxle("Morale", 20, -20);
                yValue = city.VMorale;
                graphMorale.DrawBar(Color.Blue, 0, yValue);
                yValue = city.VMoraleModifiers.Sum();
                graphMorale.DrawBar(Color.Yellow, 1, yValue);
                yValue = city.VMoraleModifiers.Where(y => y > 0).Sum();
                if (yValue != 0)
                    graphMorale.DrawBar(Color.Green, 2, yValue);
                yValue = city.VMoraleModifiers.Where(y => y < 0).Sum();
                if (yValue != 0)
                    graphMorale.DrawBar(Color.Red, 2, yValue);
            }
        }

        private void GraphicPanel_SizeChanged(object sender, EventArgs e) // SizeChanged event for all graph panels.
        {
            (sender as Panel).Refresh();
        }
        #endregion
    }
}