using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TNKUBosSinifBulucu
{
    public partial class Form_updater : Form
    {
        List<fakulte_DTO> fakulteler = null;
        
        LiteDatabase db = null;
        public Form_updater()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        /// <summary>
        /// Ders programı verisini okuma tabloya göre ayırma
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="writepath"></param>
        /// <returns></returns>
        async Task StreamReader(System.IO.Stream stream,string writepath)
        {
            try
            {


                var responseString = new StreamReader(stream).ReadToEnd();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(responseString);

                var tables = doc.DocumentNode.SelectNodes("//table[@class='table table-bordered .table-striped']");
                List<List<string>> alltables = new List<List<string>>();
                foreach (var item in tables)
                {
                    List<List<string>> table = doc.DocumentNode.SelectSingleNode(item.XPath).Descendants("tr")
                            .Where(tr => tr.Elements("td").Count() > 1)
                            .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                            .ToList();
                    alltables.AddRange(table);

                }

                var json = JsonConvert.SerializeObject(alltables);
                File.WriteAllText(writepath,json);
            }
            catch (Exception e)
            {

                throw;
            }

        }

        /// <summary>
        /// Ders programlarını güncelleme thread ı
        /// </summary>
        /// <param name="toupdate">Güncellenecek bölüm listesi</param>
        /// <param name="forbolum">Tek bölüm güncelleme</param>
        public void Updater(List<bolum_DTO> toupdate,bool forbolum = false)
        {
            lblstatus.Text = "Güncelleme Başladı..";
            Global.mainform.panel1.Visible = true;
            Global.mainform.label4.Text = "Güncelleme Devam Ediyor..";
            Global.mainform.richTextBox1.Text = "Güncelleme sırasında programı kullanamazsınız.";
            if (!Directory.Exists(Global.fakultefolder))
            {
                try
                {
                    Directory.CreateDirectory(Global.fakultefolder);
                }
                catch (Exception ex)
                {
                    txtstatus.Select(txtstatus.Text.Length, 0);
                    txtstatus.Text = "Hata: Güncellenirken bir hata oluştu. |"+ex.Message + Environment.NewLine;
                    return;
                }
            }
            if (forbolum)
            {
                string path = Global.fakultefolder + "\\" + toupdate[0].fakulte + "\\" + toupdate[0].filename;
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        txtstatus.Select(txtstatus.Text.Length, 0);
                        txtstatus.Text = "Hata: Güncellenirken bir hata oluştu. |" + ex.Message + Environment.NewLine;
                        return;
                    }
                   
                }
            }
            else
            {
                if (Directory.Exists(Global.fakultefolder + "\\" + toupdate[0].fakulte))
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(Global.fakultefolder + "\\" + toupdate[0].fakulte);
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }

                    }
                    catch (Exception ex)
                    {
                        txtstatus.Select(txtstatus.Text.Length, 0);
                        txtstatus.Text = "Hata: Güncellenirken bir hata oluştu. |" + ex.Message + Environment.NewLine;
                        return;
                    }
                }
            }
            foreach (var item in toupdate)
            {
                lblstatus.Text = item.name + " Güncelleniyor..";
                try
                {
                    HttpWebRequest req = WebRequest.Create(item.link) as HttpWebRequest;
                    req.Method = "GET";
                    var response = (HttpWebResponse)req.GetResponse();

                    if (!Directory.Exists(Global.fakultefolder + "\\" + item.fakulte))
                    {
                        try
                        {
                            Directory.CreateDirectory(Global.fakultefolder + "\\" + item.fakulte);
                        }
                        catch (Exception ex)
                        {
                            txtstatus.Select(txtstatus.Text.Length, 0);
                            txtstatus.Text += "Hata: " + item.name + " Güncellenirken bir hata oluştu. Hata: " + ex.Message + Environment.NewLine;
                            continue;
                        }
                    } 

                    string path = Global.fakultefolder+"\\" + item.fakulte + "\\" + item.filename;
                    StreamReader(response.GetResponseStream(), path);
                }
                catch (Exception)
                {
                    txtstatus.Select(txtstatus.Text.Length, 0);
                    txtstatus.Text +="Hata: "+ item.name + " Güncellenirken bir hata oluştu." + Environment.NewLine;
                    continue;
                }
                DateTime thisDay = DateTime.Now;
                txtstatus.Select(txtstatus.Text.Length, 0);
                txtstatus.Text += thisDay.ToString()+ " | "+ item.name+" Güncellendi."+Environment.NewLine;
            }
            if (Global.mainform.panel1.Visible)
            {
                Global.mainform.panel1.Visible = false;
            }
            lblstatus.Text = "Bitti.";
        }


        private void Form_updater_Load(object sender, EventArgs e)
        {
            db = new LiteDatabase(Global.settingsfile);
            var col = db.GetCollection<fakulte_DTO>("Fakulte");
            var col2 = db.GetCollection<bolum_DTO>("Bolum");
            /*
            col.Insert(new fakulte_DTO { id= ObjectId.NewObjectId(),name = "Çorlu Mühendislik Fakultesi", foldername="cmf" });
            col.Insert(new fakulte_DTO { id = ObjectId.NewObjectId(), name = "Fen Edebiyat Fakültesi", foldername = "fened" });
            col.Insert(new fakulte_DTO { id = ObjectId.NewObjectId(), name = "Güzel Sanatlar Tas. ve Mimarlık Fak.", foldername = "gsf" });
            col.Insert(new fakulte_DTO { id = ObjectId.NewObjectId(), name = "İktisadi ve İdari Bilimler Fakültesi", foldername = "iibf" });
            col.Insert(new fakulte_DTO { id = ObjectId.NewObjectId(), name = "Ziraat Fakültesi", foldername = "ziraat" });
            */
            fakulteler = col.FindAll().ToList();
            List<bolum_DTO> bolumler = col2.FindAll().ToList();
            foreach (var item in fakulteler)
            {
                comboBox1.Items.Add(item);
                comboBox3.Items.Add(item);
                comboBox4.Items.Add(item);
            }
            foreach (var item in bolumler)
            {
                listBox1.Items.Add(item);
            }
            // 
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            fakulte_DTO sec_fakulte = null;
            try
            {
               sec_fakulte = (fakulte_DTO)comboBox1.SelectedItem;
            }
            catch (Exception)
            {
                lblstatus.Text = "Lütfen Bir Fakulte Seçin.";
                return;
            }
            
            List<bolum_DTO> toupdate = new List<bolum_DTO>();
            var col2 = db.GetCollection<bolum_DTO>("Bolum");
            List<bolum_DTO> bolumler = col2.FindAll().ToList();
            foreach (var item in bolumler)
            {
                if (item.fakulte == sec_fakulte.foldername)
                {
                    toupdate.Add(item);
                }
            }
            if(toupdate.Count == 0)
            {
                lblstatus.Text = "Fakülteye Bölüm Eklememişsiniz.";
                return;
            }
            Thread th = new Thread(() => { Updater(toupdate); });
            th.Start();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var col2 = db.GetCollection<bolum_DTO>("Bolum");
            string name = textBox1.Text;
            string link = textBox2.Text;
            string fakulte = null;
            try
            {
                fakulte = ((fakulte_DTO)comboBox3.SelectedItem).foldername;
            }
            catch (Exception)
            {
                lblstatus.Text = "Lütfen Bir Fakülte Seçin.";
                return;
            }
            if(name == "" || link == "")
            {
                lblstatus.Text = "Lütfen Tüm Boşlukları Doldurun.";
                return;
            }
            try
            {
                ObjectId objectId = ObjectId.NewObjectId();
                col2.Insert(new bolum_DTO { id = objectId, name = name, fakulte = fakulte, filename = objectId.ToString() + ".json", link = link });
                listBox1.Items.Add((new bolum_DTO { id = objectId, name = name, fakulte = fakulte, filename = objectId.ToString() + ".json", link = link }));
                lblstatus.Text = name + " Eklendi";

            }
            catch (Exception)
            {

                lblstatus.Text = "Bir hata oluştu.";
            }

        }

        private void Form_updater_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bolum_DTO sec_bolum = null;
            try
            {
                sec_bolum = (bolum_DTO)comboBox2.SelectedItem;
            }
            catch (Exception)
            {

                lblstatus.Text = "Lütfen Bir bölüm Seçin";
                return;
            }
            
            List<bolum_DTO> toupdate = new List<bolum_DTO>();
            toupdate.Add(sec_bolum);
            Thread th = new Thread(() => { Updater(toupdate,true); });
            th.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var col = db.GetCollection<bolum_DTO>("Bolum");
            bolum_DTO selected = null;
            try
            {
                selected = (bolum_DTO)listBox1.SelectedItem;
            }
            catch (Exception)
            {
                lblstatus.Text = "Lütfen Bir Bölüm Seçin";
                return;
                
            }
           
            col.Delete(selected.id);
            lblstatus.Text = "Silindi: " + selected.name;
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            fakulte_DTO sec_fakulte = (fakulte_DTO)comboBox4.SelectedItem;
            var col2 = db.GetCollection<bolum_DTO>("Bolum");
            List<bolum_DTO> bolumler = col2.FindAll().ToList();
            foreach (var item in bolumler)
            {
                if (item.fakulte == sec_fakulte.foldername)
                {
                    comboBox2.Items.Add(item);
                }
            }
        }
    }
}
