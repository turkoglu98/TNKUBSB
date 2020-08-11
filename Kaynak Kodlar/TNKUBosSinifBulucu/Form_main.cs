using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TNKUBosSinifBulucu
{
    public partial class Form_main : Form
    {
        public List<Day_DTO> days = new List<Day_DTO>();
        LiteDatabase db = null;
        public DayOfWeek[] daysofweek = {
                 DayOfWeek.Monday,
                 DayOfWeek.Tuesday,
                 DayOfWeek.Wednesday,
                 DayOfWeek.Thursday,
                 DayOfWeek.Friday };
        public Form_main()
        {
            InitializeComponent();
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            List<fakulte_DTO> fakulteler = null;
            try
            {
                db = new LiteDatabase(Global.settingsfile);
                var col = db.GetCollection<fakulte_DTO>("Fakulte");
                fakulteler = col.FindAll().ToList();
                if (fakulteler.Count == 0)
                {
                    List<fakulte_DTO> def_fakulte = new List<fakulte_DTO>() {
                    new fakulte_DTO { name = "Çorlu Mühendislik Fakültesi", foldername = "cmf", id = ObjectId.NewObjectId() },
                    new fakulte_DTO { name = "Fen Edebiyat Fakültesi", foldername = "fened", id = ObjectId.NewObjectId() },
                    new fakulte_DTO { name = "Güzel Sanatlar Tas. ve Mimarlık Fak.", foldername = "gsf", id = ObjectId.NewObjectId() },
                    new fakulte_DTO { name = "İktisadi ve İdari Bilimler Fakültesi", foldername = "iibf", id = ObjectId.NewObjectId() },
                    new fakulte_DTO { name = "Ziraat Fakültesi", foldername = "ziraat", id = ObjectId.NewObjectId() },

                };

                    foreach (var item in def_fakulte)
                    {
                        col.Insert(item);
                        fakulteler.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Ayarlar alınırken bir hata oluştu: "+ex.Message);
            }

            try
            {
                if (!Directory.Exists(Global.fakultefolder))
                {
                    panel1.Visible = true;
                }
                else
                {
                    string[] fileEntries = Directory.GetFiles(Global.fakultefolder, "*.json", SearchOption.AllDirectories);
                    if (fileEntries.Length == 0)
                    {
                        panel1.Visible = true;
                    }
                }

                foreach (var item in fakulteler)
                {
                    comboBox1.Items.Add(item);
                }

                for (int i = 0; i <= 24; i++)
                {
                    Hours.Items.Add(i);
                }
                for (int i = 0; i <= 60; i++)
                {
                    Minutes.Items.Add(i);
                }

                foreach (var item in daysofweek)
                {
                    day.Items.Add(CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.DayNames[(int)item]);

                }
            }
            catch (Exception)
            {

                throw;
            }
            
            db.Dispose();
        }
        /// <summary>
        /// Verilen listeden benzer olanları çıkarır
        /// </summary>
        /// <param name="subs">string liste</param>
        /// <returns></returns>
        public List<string> getunique(List<string> subs)
        {
            var unique_items = new HashSet<string>(subs);
            return unique_items.ToList<string>();
        }
        /// <summary>
        /// verilen stringden ders: kodu,ismi,fakultesi,öğretim üyesi ve sınıfa göre ayırır
        /// </summary>
        /// <param name="arg1"></param>
        /// <returns></returns>
        private Lesson_DTO parseLesson(string arg1)
        {
            if (arg1 == "")
                return new Lesson_DTO();

            Lesson_DTO callback = new Lesson_DTO();
            string[] forparser = arg1.Split('\n');
            int parser = 0;
            foreach (var item in forparser)
            {
               
                 if (item != "")
                    {
                    string Write = item.Trim();
                        if (parser == 0)
                        {
                        callback.code = Write;
                            parser++;
                        continue;
                        }
                        if (parser == 1)
                        {
                        string[] Writeto = Write.Split(new[] { "                " }, StringSplitOptions.None);
                        callback.name = Writeto[0].Trim();
                            parser++;
                        }
                        if (parser == 2)
                        {
                        string[] Writeto = Write.Split(new[] { "                " }, StringSplitOptions.None);
                        callback.faculty = Writeto[1].Trim();
                        parser++;
                        continue;
                        }
                        if (parser == 3)
                        {
                        callback.teacher = Write;
                            parser++;
                        continue;
                        }
                        if (parser == 4)
                        {
                        int ifnotclass = Write.IndexOf("Öğretim Üyesi Odası");
                        int ifnotclass2 = Write.IndexOf("Seminer Odası");
                        int ifnotclass3 = Write.IndexOf("Birleştirme");
                        int ifnotclass4 = Write.IndexOf("Uzaktan Eğitim");
                        
                        if (ifnotclass != -1 || ifnotclass2 != -1 || ifnotclass3 != -1 || ifnotclass4 != -1)
                         {
                            Write = null;
                         }
                        
                        callback.@class = Write;
                            parser++;
                        continue;
                    }
                    
                }
            }
            
            return callback;
        }
        /// <summary>
        /// json olarak tutulmuş olan ders programlarını böler ve days listesine aktarır
        /// </summary>
        /// <param name="parse"></param>
        private void parsejson(string parse)
        {
            List<List<string>> alltables = null;
            try
            {
                alltables = JsonConvert.DeserializeObject<List<List<string>>>(parse);
            }
            catch (Exception)
            {
                MessageBox.Show("Kayıtlı ders programı bozulmuş olabilir, lütfen hepsini tekrar güncelleyiniz.");
                return;              
            }
            foreach (var item in alltables)
            {

                string saat = item[0].Replace(" ", "").Replace(".", ":");
                Lesson_DTO spzrtesi = parseLesson(item[2]);
                Lesson_DTO ssali = parseLesson(item[3]);
                Lesson_DTO scar = parseLesson(item[4]);
                Lesson_DTO sper = parseLesson(item[5]);
                Lesson_DTO scum = parseLesson(item[6]);

                days[0].time.Add(new Time_DTO { time = saat, lesson = new List<Lesson_DTO>() { new Lesson_DTO { @class = spzrtesi.@class, code = spzrtesi.code, faculty = spzrtesi.faculty, name = spzrtesi.name, teacher = spzrtesi.teacher } } }); // lste ders ekle
                days[1].time.Add(new Time_DTO { time = saat, lesson = new List<Lesson_DTO>() { new Lesson_DTO { @class = ssali.@class, code = ssali.code, faculty = ssali.faculty, name = ssali.name, teacher = ssali.teacher } } });
                days[2].time.Add(new Time_DTO { time = saat, lesson = new List<Lesson_DTO>() { new Lesson_DTO { @class = scar.@class, code = scar.code, faculty = scar.faculty, name = scar.name, teacher = scar.teacher } } });
                days[3].time.Add(new Time_DTO { time = saat, lesson = new List<Lesson_DTO>() { new Lesson_DTO { @class = sper.@class, code = sper.code, faculty = sper.faculty, name = sper.name, teacher = sper.teacher } } });
                days[4].time.Add(new Time_DTO { time = saat, lesson = new List<Lesson_DTO>() { new Lesson_DTO { @class = scum.@class, code = scum.code, faculty = scum.faculty, name = scum.name, teacher = scum.teacher } } });

            }
        }
        /// <summary>
        ///  days listesindeki tüm ders programlarındaki dersleri aynı saatlerde toplamayı sağlar 
        /// </summary>
        /// <param name="getlist"></param>
        /// <returns></returns>
        private List<Time_DTO> updatetimelist(List<Time_DTO> getlist)
        {
            List<Time_DTO> yenitimelist = new List<Time_DTO>();

            List<string> timelist = new List<string>();
            foreach (var item in getlist)
            {
                timelist.Add(item.time);
            }
            var unique_times = new HashSet<string>(timelist);
            
            foreach (var item in unique_times)
            {
                yenitimelist.Add(new Time_DTO { time = item, lesson = new List<Lesson_DTO>() });
            }
            foreach (var item in getlist)
            {
                int s = 0;
                foreach (var items in unique_times)
                {
                    if (item.time == items)
                    {
                        yenitimelist[s].lesson.Add(item.lesson[0]);
                    } 
                    s++;
                }
            }
            var callback = new HashSet<Time_DTO>(yenitimelist).ToList<Time_DTO>();
            return callback;
        }
        /// <summary>
        /// fakülteye göre programdaki ders programlarını günceller
        /// tüm bölümlerin kayıtlı olan ders programları alınır
        /// her bölümün her dersi saatlik olarak ayrılır ve her ders Lesson_DTO ya göre parçalanır
        /// sonra aynı saatte olan dersler tek bir liste altında toplanır (updatetimelist)
        /// en son kullanılabilir günler listesi oluşturulur
        /// </summary>
        /// <param name="fakultefolder"></param>
        public void updatedaysbyfakulte(string fakultefolder)
        {
            days.Clear();
            days.Add(new Day_DTO { Day = 0, time = new List<Time_DTO>() });
            days.Add(new Day_DTO { Day = 1, time = new List<Time_DTO>() });
            days.Add(new Day_DTO { Day = 2, time = new List<Time_DTO>() });
            days.Add(new Day_DTO { Day = 3, time = new List<Time_DTO>() });
            days.Add(new Day_DTO { Day = 4, time = new List<Time_DTO>() });
            string[] Getbolums = Directory.GetDirectories(Environment.CurrentDirectory + "\\fakulte");
            foreach (string fakulte in Getbolums)
            {
                string nfakulte = new DirectoryInfo(fakulte).Name;
                if (nfakulte == fakultefolder)
                    ProcessFolder(fakulte);
            }
            List<Day_DTO> updateddays = new List<Day_DTO>();
            int i = 0;
            foreach (var item in days)
            {
                var updateday = updatetimelist(item.time);
                updateddays.Add(new Day_DTO { Day = i, time = updateday });
                i++;
            }
            days.Clear();
            days = updateddays;
        }
        /// <summary>
        /// verilen yolda *.json dosyalarını arar ve parse eder
        /// </summary>
        /// <param name="path"></param>
        public void ProcessFolder(string path)
        {
            try
            {
                string[] fileEntries = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                foreach (string fileName in fileEntries)
                {
                    string getjson = File.ReadAllText(fileName);
                    parsejson(getjson);
                    //FileInfo info = new FileInfo(fileName);

                }
            }
            catch { }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
            fakulte_DTO sec_fakulte = null;
            string h = null;
            string m = null;
            string d = null;
            try
            {
                sec_fakulte = (fakulte_DTO)comboBox1.SelectedItem;
                if (sec_fakulte == null)
                  throw new Exception();

                h = Hours.SelectedItem.ToString();
                m = Minutes.SelectedItem.ToString();
                d = day.SelectedItem.ToString();
            }
            catch (Exception)
            {
                textBox3.Text = "Lütfen Tüm Boş Seçim Bırakmayın.";
                return;
            }
            string hm = h + ":" + m;
            int dayid = 0;
            foreach (var item in daysofweek)
            {
                if (d == CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.DayNames[(int)item])
                {
                    break;
                }
                dayid++;
            }
            updatedaysbyfakulte(sec_fakulte.foldername);

            string sectime = hm;
            TimeSpan sectimespan = TimeSpan.Parse(sectime);
            int gun = dayid; // pazartesi
            int count = 0;
            foreach (var item in days[gun].time)
            {
                string[] aralik = item.time.Split('-');
                TimeSpan aralik1 = TimeSpan.Parse(aralik[0]);
                TimeSpan aralik2 = TimeSpan.Parse(aralik[1]);
                if (sectimespan >= aralik1 && sectimespan <= aralik2)
                {
                    
                    foreach (var items in item.lesson)
                    {
                        string dersyazdir = items.@class+" | "+items.name+" | "+items.teacher;
                        if(count == 0)
                        {
                            textBox3.Text = "Saat: "+item.time+" için tüm dersler:"+ Environment.NewLine;
                            count++;
                        }
                        if (items.@class != null)
                        {
                            textBox3.Text += "Ders: " + dersyazdir + Environment.NewLine;
                            count++;
                        }
                       
                    }

                    
                }

            }
            if(count == 1)
            {
                textBox3.Text += "Hiç Ders Bulunamadı.";
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            fakulte_DTO sec_fakulte = null;
            try
            {
                sec_fakulte = (fakulte_DTO)comboBox1.SelectedItem;
                if (sec_fakulte == null)
                    throw new Exception();
            }
            catch (Exception)
            {
                textBox3.Text = "Lütfen Bir Fakülte Seçin.";
                return;
            }
            updatedaysbyfakulte(sec_fakulte.foldername);
            List<string> lessons = new List<string>();
            foreach (var day in days)
            {
                foreach (var times in day.time)
                {
                    foreach (var lesson in times.lesson)
                    {
                        if(lesson.@class != null)
                        lessons.Add(lesson.@class);
                    }  
                }      
            }
            List<string> uniquelist = getunique(lessons);
            uniquelist.Sort();
            textBox3.Text = "";

            int newline = 0;
            int split = 8;
            foreach (var item in uniquelist)
            {
                if (newline == split)
                {
                    textBox3.Text += Environment.NewLine;
                    newline = 0;
                }
                textBox3.Text += item;
                if (newline != split - 1)
                {
                    textBox3.Text += " | ";
                }

                newline++;
            }
        }



        private void button7_Click(object sender, EventArgs e)
        {
            fakulte_DTO sec_fakulte = null;
            string h = null;
            string m = null;
            string d = null;
            try
            {
                sec_fakulte = (fakulte_DTO)comboBox1.SelectedItem;
                if (sec_fakulte == null)
                    throw new Exception();

                h = Hours.SelectedItem.ToString();
                m = Minutes.SelectedItem.ToString();
                d = day.SelectedItem.ToString();
            }
            catch (Exception)
            {
                textBox3.Text = "Lütfen Tüm Boş Seçim Bırakmayın.";
                return;
            }
            
            string hm = h + ":" + m;
            int dayid = 0;
            foreach (var item in daysofweek)
            {
                if(d == CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.DayNames[(int)item])
                {
                    break;
                }
                dayid++;
            }
            updatedaysbyfakulte(sec_fakulte.foldername);
            List<string> lessons = new List<string>();
            foreach (var day in days)
            {
                foreach (var times in day.time)
                {
                    foreach (var lesson in times.lesson)
                    {
                        if (lesson.@class != null)
                        {
                            lessons.Add(lesson.@class);
                        }
                    }
                }
            }
            List<string> uniqueclasslist = getunique(lessons);

            List<string> doluclasslist = new List<string>();
            string sectime = hm;
            TimeSpan sectimespan = TimeSpan.Parse(sectime);
            int gun = dayid;
            foreach (var item in days[gun].time)
            {
                string[] aralik = item.time.Split('-');
                TimeSpan aralik1 = TimeSpan.Parse(aralik[0]);
                TimeSpan aralik2 = TimeSpan.Parse(aralik[1]);
                if (sectimespan >= aralik1 && sectimespan <= aralik2)
                {

                    foreach (var items in item.lesson)
                    {
                        string dersyazdir = items.@class + "|" + items.name + "|" + items.faculty;
                        if (items.@class != null)
                        {
                            doluclasslist.Add(items.@class);
                        }

                    }


                }

            }
            var bossiniflist = uniqueclasslist.Except(doluclasslist).ToList();
            textBox3.Text = "Tüm Boş Sınıflar:" + Environment.NewLine;
            int newline = 0;
            int split = 5;
            foreach (var item in bossiniflist)
            {
                if(newline == split)
                {
                    textBox3.Text += Environment.NewLine;
                    newline = 0;
                }
                textBox3.Text += item;
                if (newline != split-1)
                {
                    textBox3.Text += " | ";
                }

                newline++;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form_updater a = new Form_updater();
            a.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string h = now.Hour.ToString();
            string m = now.Minute.ToString();
            string d =  CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.DayNames[(int)DateTime.Now.DayOfWeek];
            Hours.SelectedIndex = Hours.FindStringExact(h);
            Minutes.SelectedIndex = Minutes.FindStringExact(m);
            day.SelectedItem = d;
        }
    }
}
