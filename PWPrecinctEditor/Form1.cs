using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using NAudio;
using NAudio.Wave;
using System.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PWPrecinctEditor
{   
    public partial class Form1 : Form
    {
        WaveOut musicPlayer = new WaveOut();
        Mp3FileReader mp3Reader;
        WaveFileReader wmvReader;
        bool musicPlaying = false;

        List<PrecinctClt> precincts = new List<PrecinctClt>();
        //List<PrecinctSev> precinctsSev = new List<PrecinctSev>();

        List<string> mapList = new List<string>();

        PrecinctClt selectedPrecinct = null;
        //PrecinctSev selectedPrecinctSev = null;

        Area selectedArea = null;
        //AreaSev selectedAreaSev = null;

        FileInfo selectedMusic = null;
        List<FileInfo> clientMusics = new List<FileInfo>();
        string basePath, zonesText, areasOfText, pointCountText, markerCountText, musicCountText, playingText;        
        public Form1()
        {
            InitializeComponent();
            Helper.LoadOffset();
            LoadVersion();
            zonesText = labelAreas.Text;
            areasOfText = labelAreasOf.Text;
            pointCountText = labelPointCount.Text;
            markerCountText = labelMarkerCount.Text;
            musicCountText = labelMusicCount.Text;
            playingText = labelPlaying.Text;
        }
       

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void panelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        void LoadVersion()
        {
            foreach (Offset offset in Helper.PWVersion)
                comboBoxVersion.Items.Add(offset.name);

            comboBoxVersion.SelectedIndex = 0;
        }
       
        private void buttonGetCoordinates_Click(object sender, EventArgs e)
        {
            try
            {
                Helper.GetElementClientProcess();
                Console.WriteLine("Sucesso");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"falhou: \n{ex.Message}");
            }    
        }

        private void dataGridViewAreas_SelectionChanged(object sender, EventArgs e)
        {
            foreach(Area area in selectedPrecinct.Areas)
            {
                if (dataGridViewAreas.Rows.Count > 0 && dataGridViewAreas.SelectedRows.Count > 0)
                {
                    if(selectedPrecinct.Areas.IndexOf(area).Equals(dataGridViewAreas.SelectedRows[0].Cells[2].RowIndex))                    
                    {
                        selectedArea = area;
                        //int index = selectedPrecinct.Areas.IndexOf(area);
                        //selectedAreaSev = selectedPrecinctSev.Areas[index];
                        break;
                    }                   
                }
            }
            LoadAreaInfo();
        }
        private void toolStripMenuItemAddArea_Click(object sender, EventArgs e)
        {
            Area newArea = new Area()
            {
                ID = selectedPrecinct.Areas.Last().ID +1,
                Name = "",
                OriginMapID = 0,
                DestinationMapID = 0,
                Respawn = new Point(0, 0, 0, 0),
                PointsCount = 0,
                MarkersCount = 0,
                DomainID = 0,
                Priority = 0,
                DaySoundtrack = "",
                NightSoundtrack = "",
                SoundTracksCount = 0,
                SoundtrackInterval = 0,
                LoopSoundtrack = false,
                PKProtection = false,
            };
            selectedPrecinct.Areas.Add(newArea);
            dataGridViewAreas.Rows.Add((int)dataGridViewAreas.Rows[dataGridViewAreas.Rows.Count-1].Cells[0].Value + 1, newArea.Name, newArea.ID);            
            labelAreas.Text = zonesText + selectedPrecinct.Areas.Count.ToString();
            dataGridViewAreas.Rows[dataGridViewAreas.Rows.Count - 1].Selected = true;
        }
        private void toolStripMenuItemRemoveArea_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridViewAreas.SelectedRows[0];
            dataGridViewAreas.Rows.Remove(row);
            selectedPrecinct.Areas.Remove(selectedArea);
            dataGridViewAreas.Rows[dataGridViewAreas.Rows.Count - 1].Selected = true;
        }
        private void toolStripMenuItemAddPoint_Click(object sender, EventArgs e)
        {
            Point newPoint = new Point()
            {
                ID = selectedArea.Points.Count > 0 ? selectedArea.Points.Last().ID+1 : 1,
                X = 0,
                Y = 0,
                Z = 0
            };
            selectedPrecinct.Areas[selectedPrecinct.Areas.IndexOf(selectedArea)].AddPoint(newPoint);
            labelPointCount.Text = pointCountText + selectedArea.PointsCount.ToString();
            dataGridViewPoints.Rows.Add(
                newPoint.ID, 
                newPoint.X.ToString("0.000000"), 
                newPoint.Y.ToString("0.000000"), 
                newPoint.Z.ToString("0.000000"));
            dataGridViewPoints.Rows[dataGridViewPoints.Rows.Count - 1].Cells[0].Selected = true;
        }
        private void toolStripMenuItemRemovePoint_Click(object sender, EventArgs e)
        {            
            foreach (Point point in selectedArea.Points)
            {
                if (point.ID.Equals(dataGridViewPoints.SelectedRows[0].Cells[0].Value))
                {
                    selectedArea.RemovePoint(point);
                    break;
                }                 
            }
            dataGridViewPoints.Rows.Clear();
            foreach (Point point in selectedArea.Points)
                dataGridViewPoints.Rows.Add(
                    point.ID,
                    point.X.ToString("0.000000"),
                    point.Y.ToString("0.000000"),
                    point.Z.ToString("0.000000"));
            labelPointCount.Text = pointCountText + selectedArea.PointsCount.ToString();
        }
        private void toolStripMenuItemAddMusic_Click(object sender, EventArgs e)
        {
            selectedArea.AddMusic("");
            dataGridViewMusics.Rows.Add("");
            labelMusicCount.Text = musicCountText + selectedArea.SoundTracksCount.ToString();
        }
        private void toolStripMenuItemRemoveMusic_Click(object sender, EventArgs e)
        {
            if (dataGridViewMusics.SelectedRows.Count > 0)
            {
                if (selectedArea.SoundTracksCount > 0)
                    selectedArea.RemoveMusic(dataGridViewMusics.SelectedRows[0].Cells[0].Value.ToString());

                labelMusicCount.Text = musicCountText + selectedArea.SoundTracksCount.ToString();
                dataGridViewMusics.Rows.Clear();
                foreach (string music in selectedArea.Musics)
                    dataGridViewMusics.Rows.Add(music);
            }   
        }
        void LoadMusics(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            List<FileInfo> fi = di.GetFiles("*.*", SearchOption.AllDirectories).Where(s => s.Extension.Equals(".mp3") || s.Extension.Equals(".wav")).ToList();
            foreach(FileInfo file in fi)
            {                
                clientMusics.Add(file);
                dataGridViewMusicTest.Rows.Add(file.Name);
            }    
        }
        void LoadAreaInfo()
        {
            dataGridViewPoints.Rows.Clear();
            dataGridViewMarkers.Rows.Clear();
            dataGridViewMusics.Rows.Clear();

            textBoxID.Text = selectedArea.ID.ToString();
            textBoxName.Text = selectedArea.Name;
            textBoxMapDeath.Text = selectedArea.OriginMapID.ToString();
            textBoxMapRespawn.Text = selectedArea.DestinationMapID.ToString();
            textBoxWidth.Text = selectedArea.Respawn.X.ToString("0.000000");
            textBoxHeigth.Text = selectedArea.Respawn.Y.ToString("0.000000");
            textBoxLength.Text = selectedArea.Respawn.Z.ToString("0.000000");
            labelPointCount.Text = pointCountText + selectedArea.PointsCount.ToString();
            checkBoxPkProtection.Checked = selectedArea.PKProtection;
            textBoxDomainID.Text = selectedArea.DomainID.ToString();
            textBoxPriority.Text = selectedArea.Priority.ToString();
            if(selectedArea.PointsCount > 0)
                foreach(Point point in selectedArea.Points)            
                    dataGridViewPoints.Rows.Add(
                        point.ID, 
                        point.X.ToString("0.000000"), 
                        point.Y.ToString("0.000000"), 
                        point.Z.ToString("0.000000"));
            labelMarkerCount.Text = markerCountText + selectedArea.MarkersCount.ToString();
            /*if (selectedArea.MarkersCount > 0)
                foreach (Marker marker in selectedArea.Markers)
                    dataGridViewMarkers.Rows.Add(marker.Name, marker.X, marker.Y, marker.Z);*/
            textBoxSoundtrackDay.Text = selectedArea.DaySoundtrack;
            textBoxSoundtrackNight.Text = selectedArea.NightSoundtrack;
            checkBoxLoopMusic.Checked = selectedArea.LoopSoundtrack;
            //checkBoxEnableTimer.Checked = selectedArea.;
            textBoxTimer.Text = selectedArea.SoundtrackInterval.ToString();
            labelMusicCount.Text = musicCountText + selectedArea.SoundTracksCount.ToString();
            if (selectedArea.SoundTracksCount > 0)
                foreach(string soundtrack in selectedArea.Musics)
                    dataGridViewMusics.Rows.Add(soundtrack);
        }
        void LoadPrecinctAreas(string mapName)
        {             
            dataGridViewAreas.Rows.Clear();
            List<Area> areas = new List<Area>();
            foreach(PrecinctClt precinct in precincts)
            {
                string map = precinct.Map;
                if (mapName.Equals(map))
                {
                    areas = precinct.Areas;
                    selectedPrecinct = precinct;
                    //int index = precincts.IndexOf(precinct);
                    //selectedPrecinctSev = precinctsSev[index];
                    break;
                }                    
            }
            int count = 1;
            foreach(Area area in areas)
            {
                dataGridViewAreas.Rows.Add(count,area.Name,area.ID);
                count++;
            }
            labelAreasOf.Text = areasOfText + selectedPrecinct.Map;
            labelAreas.Text = zonesText + selectedPrecinct.Areas.Count.ToString();
            textBoxKey.Text = selectedPrecinct.Key.ToString();
            if(!comboBoxPrecinctVersion.Items.Contains(selectedPrecinct.Version.ToString()))            
                comboBoxPrecinctVersion.Items.Add(selectedPrecinct.Version.ToString());                
            
            comboBoxPrecinctVersion.SelectedIndex = comboBoxPrecinctVersion.Items.IndexOf(selectedPrecinct.Version.ToString());
        }       

        private void buttonOpenFolder_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Select the element folder of your game client";
                CommonFileDialogResult result = dialog.ShowDialog();
                if(result.Equals(CommonFileDialogResult.Ok) && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    LoadMusics(dialog.FileName + @"\music\");
                    basePath = dialog.FileName + @"\maps\";                  
                    try
                    {
                        string[] mapsFolders = Directory.GetDirectories(basePath);
                        foreach (string mapFolder in mapsFolders)
                        {                           
                            if (File.Exists(mapFolder + @"\precinct.clt"))
                            {                               
                                string[] precicntLines = File.ReadAllLines(mapFolder + @"\precinct.clt");
                                StreamReader file = new StreamReader(mapFolder + @"\precinct.clt");
                                PrecinctClt precinct = new PrecinctClt();
                                
                                if (Directory.Exists(Directory.GetCurrentDirectory() + mapFolder))
                                {
                                    string[] mapImages = Directory.GetFiles(Directory.GetCurrentDirectory() + mapFolder);
                                    foreach (string image in mapImages)
                                        Console.WriteLine(image);
                                }
                                int version = int.Parse(precicntLines[2].Substring(8));
                                int key = int.Parse(precicntLines[3]);
                                precinct.Path = mapFolder;
                                precinct.Version = version;
                                precinct.Key = key;

                                //PrecinctSev precinctSev = new PrecinctSev();//.sev
                                //precinctSev.Version = uint.Parse(version.ToString());
                                //precinctSev.Key = uint.Parse(key.ToString());
                               

                                for (int x = 5; x < precicntLines.Length; x++)
                                {
                                    Area area = new Area();
                                    AreaSev areaSev = new AreaSev();

                                    area.Name = precicntLines[x].Replace("\"", "");
                                    x++;
                                    string[] split = precicntLines[x].Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                    area.ID = int.Parse(split[0]);
                                    int coordCount = int.Parse(split[1]);
                                    int markCount = int.Parse(split[2]);
                                    int domainID = 0;
                                    if (version >= 6)
                                        domainID = int.Parse(split[3]);
                                    int destinationMapID = int.Parse(split[4]);
                                    //int soundTracksCount = int.Parse(split[5]);
                                    int soundtrackInterval = int.Parse(split[6]);
                                    bool loopSoundtrack = int.Parse(split[7]) == 1 ? true : false;
                                    int originMapID = int.Parse(split[8]);
                                    int priority = int.Parse(split[9]);
                                    bool pkProtection = false;
                                    if (version >= 7)
                                        pkProtection = int.Parse(split[10]) == 1 ? true : false;
                                    x++;
                                    string[] splitCoord = precicntLines[x].Split(',');                                  
                                    Point respawn = new Point();
                                    int count = 0;
                                    respawn.ID = count++;
                                    respawn.X = float.Parse(splitCoord[0], CultureInfo.InvariantCulture);
                                    respawn.Y = float.Parse(splitCoord[1], CultureInfo.InvariantCulture);
                                    respawn.Z = float.Parse(splitCoord[2], CultureInfo.InvariantCulture);
                                    x++;
                                    for (int y = x; y < x + coordCount; y++)
                                    {
                                        string[] splitPoint = precicntLines[y].Split(' ').Where(a => !string.IsNullOrEmpty(a)).ToArray();
                                        Point point = new Point();
                                        point.ID = count++;
                                        point.X = float.Parse(splitPoint[0], CultureInfo.InvariantCulture);
                                        point.Y = float.Parse(splitPoint[1], CultureInfo.InvariantCulture);
                                        point.Z = float.Parse(splitPoint[2], CultureInfo.InvariantCulture);
                                        area.Points.Add(point);//.clt
                                        //areaSev.Points.Add(point);//.sev
                                    }
                                    x += coordCount + 1;
                                    if (markCount > 0)
                                    {
                                        for (int y = x; y < x + markCount; y++)
                                        {
                                            Marker mark = new Marker();
                                            int firstIndex = precicntLines[y].IndexOf('\"');
                                            int secondIndex = precicntLines[y].IndexOf('\"', precicntLines[y].IndexOf('\"') + 1);
                                            mark.Name = precicntLines[y].Substring(firstIndex + 1, secondIndex - firstIndex - 1);
                                            string[] splitMark = precicntLines[y].Substring(secondIndex + 1).Split(' ').Where(a => !string.IsNullOrEmpty(a)).ToArray();
                                            mark.X = float.Parse(splitMark[0], CultureInfo.InvariantCulture);
                                            mark.Y = float.Parse(splitMark[1], CultureInfo.InvariantCulture);
                                            mark.Z = float.Parse(splitMark[2], CultureInfo.InvariantCulture);
                                            area.Markers.Add(mark);
                                        }
                                    }
                                    x += markCount + 1;
                                    string daySoundtrack;
                                    if (precicntLines[x].Contains(".mp3"))
                                    {
                                        daySoundtrack = "";
                                        x--;
                                    }
                                    else daySoundtrack = precicntLines[x].Replace("\"", "");
                                    x++;
                                    while (precicntLines[x].Split('\\')[0].Contains("Music"))
                                    {
                                        string music = precicntLines[x].Replace("\"", "");
                                        area.Musics.Add(music);
                                        x++;
                                    }
                                    string nightSoundtrack = precicntLines[x].Replace("\"", "");
                                    x++;        
                                    //precinct.clt
                                    area.DaySoundtrack = daySoundtrack;
                                    area.DestinationMapID = destinationMapID;
                                    area.DomainID = domainID;
                                    area.LoopSoundtrack = loopSoundtrack;
                                    area.MarkersCount = area.Markers.Count;
                                    area.NightSoundtrack = nightSoundtrack;
                                    area.OriginMapID = originMapID;
                                    area.PKProtection = pkProtection;
                                    area.PointsCount = area.Points.Count;
                                    area.Priority = priority;
                                    area.Respawn = respawn;
                                    area.SoundtrackInterval = soundtrackInterval;
                                    area.SoundTracksCount = area.Musics.Count;
                                    precinct.Areas.Add(area);

                                    //Precinct.sev
                                    /*areaSev.PointsCount = area.Points.Count;
                                    areaSev.DomainID = domainID;
                                    areaSev.Priority = priority;
                                    areaSev.OriginMapID = originMapID;
                                    areaSev.DestinationMapID = destinationMapID;
                                    areaSev.PKProtection = pkProtection;
                                    areaSev.Respawn = respawn;
                                    precinctSev.Areas.Add(areaSev);*/
                                }                                                            
                                precinct.Map = mapFolder.Substring(basePath.Length);
                                mapList.Add(precinct.Map);
                                precincts.Add(precinct);
                                //precinctsSev.Add(precinctSev);
                            }
                        }
                        comboBoxPrecincts.Items.AddRange(mapList.ToArray());
                        comboBoxMaps.Items.AddRange(mapList.ToArray());
                        comboBoxPrecincts.SelectedIndex = 0;

                        //PCKManager.LoadPck(dialog.SelectedPath + @"\surfaces.pck");
                       
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}\n\n{ex.Source}\n\n{ex.InnerException}");
                    }
                    LoadMaps();
                }
            }
        }        
        void LoadMaps()
        {                                
            foreach (string item in comboBoxPrecincts.Items)
            {
                if (item != "world" )
                    continue;
                if(!Directory.Exists(PCKManager.path + @"surfaces\minimaps\" + item))                
                    continue;
                
                DirectoryInfo directory = new DirectoryInfo(PCKManager.path + @"surfaces\minimaps\"+ item);
                FileInfo[] mapImages = directory.GetFiles();
                int imageHeight = 0;
                int imageWidth = 0;
                List<string> lines = new List<string>();
                List<string> columns = new List<string>();

                foreach (FileInfo map in mapImages)
                {
                    DDSImage ddsImage = new DDSImage(map.FullName);
                    string imagePath = @$"{PCKManager.path}\output\{item}";
                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);
                    string imageFile = imagePath+@$"\{map.Name.Replace(".dds", ".png")}";
                    ddsImage.Save(imageFile);
                    Image img = Image.FromFile(imageFile);                    
                    imageHeight += img.Height;
                    imageWidth += img.Width;
                    img.Dispose();
                    
                    lines.Add(map.Name.Substring(0, 2));
                    columns.Add(map.Name.Substring(2, 2));   
                }
                lines = lines.Distinct().ToList();
                columns = columns.Distinct().ToList();
                lines.Sort();
                columns.Sort();
                Bitmap img3 = new Bitmap(imageWidth/columns.Count, imageHeight/lines.Count);
                Graphics g = Graphics.FromImage(img3);
                g.Clear(SystemColors.AppWorkspace);

                for (int x = 0; x <= int.Parse(lines.Last()); x++)
                {
                    string linePosition = x.ToString("00");
                    for (int y = 0; y <= int.Parse(columns.Last()); y++)
                    {
                        string columnPosition = y.ToString("00");
                        if(File.Exists(@$"{PCKManager.path}\output\{item}\{linePosition}{columnPosition}.png"))
                        using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(@$"{PCKManager.path}\output\{item}\{linePosition}{columnPosition}.png")))
                        {
                            using (Image img = Image.FromStream(ms))
                            {                                
                                g.DrawImage(img, new PointF(y * img.Width, x * img.Height));
                            }
                        }
                        //Image img = Image.FromFile(directory.FullName + @"\" + linePosition + columnPosition + ".dds");
                        
                        //g.DrawImage(img, new PointF(y*img.Width, x*img.Height)); 
                       
                        //img.Dispose();
                    }
                }               
                g.Dispose();
                img3.Save(item+".png", System.Drawing.Imaging.ImageFormat.Png);
                img3.Dispose();
            }
        }
        private void CombineImages(FileInfo[] files, string finalName)
        {
            try
            {
                //change the location to store the final image.
                string finalImage = PCKManager.path + finalName;
                List<int> imageHeights = new List<int>();
                int nIndex = 0;
                int width = 0;

                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);
                    imageHeights.Add(img.Height);
                    width += img.Width;
                    img.Dispose();
                }
                imageHeights.Sort();
                int height = imageHeights[imageHeights.Count - 1];
                Bitmap img3 = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img3);
                g.Clear(SystemColors.AppWorkspace);

                foreach (FileInfo file in files)
                {
                    Image img = Image.FromFile(file.FullName);
                    if (nIndex == 0)
                    {
                        g.DrawImage(img, new PointF(0, 0));
                        nIndex++;
                        width = img.Width;
                    }
                    else
                    {
                        g.DrawImage(img, new PointF(width, 0));
                        width += img.Width;
                    }
                    img.Dispose();
                }
                g.Dispose();
                img3.Save(finalImage, System.Drawing.Imaging.ImageFormat.Jpeg);
                img3.Dispose();
                //pictureBox1.Image = Image.FromFile(finalImage);
            }
            catch
            {

            }

        }

        private void comboBoxPrecincts_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox senderBox = (ComboBox)sender;               
            LoadPrecinctAreas(senderBox.SelectedItem.ToString());            
        }
        private void dataGridViewMusicTest_SelectionChanged(object sender, EventArgs e)
        {
            try 
            {
                if (dataGridViewMusicTest.SelectedRows[0].Cells[0].Value.ToString().Contains(".wav") && !musicPlaying)
                {
                    foreach (FileInfo file in clientMusics)
                    {
                        if (file.Name.Equals(dataGridViewMusicTest.SelectedRows[0].Cells[0].Value))
                        {
                            WaveFileReader wf = new WaveFileReader(file.FullName);
                            labelMusicLength.Text = $"{wf.TotalTime.Minutes.ToString()}:{wf.TotalTime.Seconds.ToString("00")}";
                            selectedMusic = file;
                            break;
                        }
                    }

                }
                else if (dataGridViewMusicTest.SelectedRows[0].Cells[0].Value.ToString().Contains(".mp3") && !musicPlaying)
                {
                    foreach (FileInfo file in clientMusics)
                    {
                        if (file.Name.Equals(dataGridViewMusicTest.SelectedRows[0].Cells[0].Value))
                        {
                            Mp3FileReader mp3fr = new Mp3FileReader(file.FullName);
                            labelMusicLength.Text = $"{mp3fr.TotalTime.Minutes.ToString()}:{mp3fr.TotalTime.Seconds.ToString("00")}";
                            selectedMusic = file;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Source}\n{ex.Message}");
            }
           
        }
     
        private void buttonStop_Click(object sender, EventArgs e)
        {
            musicPlayer.Dispose();
            timerMusic.Stop();
            labelPlaying.Visible = false;
            musicPlaying = false;
            trackBarMusic.Value = 0;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (buttonPause.Text.Equals("Pause"))
            {
                musicPlayer.Pause();              
                labelPlaying.Text = playingText + selectedMusic.Name + " (Paused)";
                buttonPause.Text = "Continue";
            }else if (buttonPause.Text.Equals("Continue"))
            {
                musicPlayer.Resume();
                labelPlaying.Text = playingText + selectedMusic.Name;
                buttonPause.Text = "Pause";
            }
        }

        private void timerMusic_Tick(object sender, EventArgs e)
        {
            if (mp3Reader != null)
            {               
                trackBarMusic.Maximum = (int)mp3Reader.TotalTime.TotalMilliseconds;
                trackBarMusic.Value = (int)mp3Reader.CurrentTime.TotalMilliseconds;
            }else if (wmvReader != null)
            {
                trackBarMusic.Maximum = (int)wmvReader.TotalTime.TotalMilliseconds;
                trackBarMusic.Value = (int)wmvReader.CurrentTime.TotalMilliseconds;
            }
            if (trackBarMusic.Maximum == trackBarMusic.Value)
            {
                trackBarMusic.Value = 0;
                labelPlaying.Text = "";
            }
                
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            dataGridViewMusics.Rows.Add(selectedMusic.FullName.Substring(selectedMusic.FullName.IndexOf(@"music\")));
        }

        private void toolStripMenuItemAddToMusics_Click(object sender, EventArgs e)
        {
            dataGridViewMusics.Rows.Add(selectedMusic.FullName.Substring(selectedMusic.FullName.IndexOf(@"music\")));
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            musicPlayer.Dispose();
            timerMusic.Stop();
            trackBarMusic.Value = 0;
            buttonPause.Text = "Pause";
            if (selectedMusic.Extension.Equals(".mp3"))
            {
                wmvReader = null;
                mp3Reader = new Mp3FileReader(selectedMusic.FullName);                
                musicPlayer.Init(mp3Reader);
                labelPlaying.Text = playingText + selectedMusic.Name;
                labelPlaying.Visible = true;                
                timerMusic.Enabled = true;
                timerMusic.Start();
                musicPlayer.Play();
                musicPlaying = true;
            }
            else if (selectedMusic.Extension.Equals(".wav"))
            {
                mp3Reader = null;
                wmvReader = new WaveFileReader(selectedMusic.FullName);             
                musicPlayer.Init(wmvReader);
                labelPlaying.Text = playingText + selectedMusic.Name;
                labelPlaying.Visible = true;                
                timerMusic.Enabled = true;
                timerMusic.Start();
                musicPlayer.Play();
                musicPlaying = true;
            }
        }
        private void buttonNextMusic_Click(object sender, EventArgs e)
        {         
            if(dataGridViewMusicTest.SelectedRows.Count > 0 && dataGridViewMusicTest.SelectedRows[0].Index < dataGridViewMusicTest.Rows.Count-1)
            {                
                buttonStop_Click(sender, e);
                dataGridViewMusicTest.Rows[dataGridViewMusicTest.SelectedRows[0].Index + 1].Selected = true;
                dataGridViewMusicTest.Rows[dataGridViewMusicTest.SelectedRows[0].Index].Cells[0].Selected = true;
                buttonPlay_Click(sender, e);
            }
        }

        private void buttonPreviousMusic_Click(object sender, EventArgs e)
        {
            if (dataGridViewMusicTest.SelectedRows.Count > 0 && dataGridViewMusicTest.SelectedRows[0].Index > 0)
            {
                buttonStop_Click(sender, e);
                dataGridViewMusicTest.Rows[dataGridViewMusicTest.SelectedRows[0].Index - 1].Selected = true;
                dataGridViewMusicTest.Rows[dataGridViewMusicTest.SelectedRows[0].Index].Cells[0].Selected = true;
                buttonPlay_Click(sender, e);
            }
        }
        private void textBoxID_Leave(object sender, EventArgs e)
        {
            if(selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.ID = int.Parse(textBoxID.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxName_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.Name = textBoxName.Text;
                dataGridViewAreas.SelectedRows[0].Cells[1].Value = textBoxName.Text;
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxMapDeath_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.OriginMapID = int.Parse(textBoxMapDeath.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxMapRespawn_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.DestinationMapID = int.Parse(textBoxMapRespawn.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxWidth_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.Respawn.X = float.Parse(textBoxWidth.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxHeigth_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.Respawn.Y = float.Parse(textBoxHeigth.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxLength_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.Respawn.Z = float.Parse(textBoxLength.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxDomainID_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.DomainID = int.Parse(textBoxDomainID.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void textBoxPriority_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                selectedArea.Priority = int.Parse(textBoxPriority.Text);
                selectedPrecinct.hasChanged = true;
            }
        }

        private void checkBoxPkProtection_Click(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {                
                selectedArea.PKProtection = checkBoxPkProtection.Checked;
                selectedPrecinct.hasChanged = true;
            }
        }

        private void dataGridViewPoints_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {

            }
        }

        private void textBoxKey_Leave(object sender, EventArgs e)
        {
            if (selectedPrecinct != null && selectedArea != null)
            {
                bool isInt = int.TryParse(textBoxKey.Text, out int result);
                if (isInt)
                {
                    selectedPrecinct.Key = result;
                    selectedPrecinct.hasChanged = true;
                }                    
                else textBoxKey.Text = selectedPrecinct.Key.ToString();
             
            }
        }

        private void buttonSave_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
                contextMenuStripSave.Show(Cursor.Position);
        }

        private void toolStripMenuItemSaveCLT_Click(object sender, EventArgs e)
        {
            if (selectedPrecinct.hasChanged)
            {
                string savepath = selectedPrecinct.Path + @"\newly saved\";
                if (!Directory.Exists(savepath))
                    Directory.CreateDirectory(savepath);
                FileStream stream = new FileStream(savepath + @"\precinct.clt",FileMode.Create);
                using (StreamWriter writer = new StreamWriter(stream,Encoding.GetEncoding("Unicode")))
                {
                    writer.WriteLine("//  Element precinct file (client version)\n");
                    writer.WriteLine($"version  {selectedPrecinct.Version}");
                    writer.WriteLine($"{selectedPrecinct.Key}\n");
                    foreach (Area area in selectedPrecinct.Areas)
                    {
                        writer.WriteLine($"\"{area.Name}\"");
                        string line = $"{area.ID}" +
                            $"  {area.PointsCount}" +
                            $"  {area.MarkersCount}" +
                            $"  {area.DomainID}" +
                            $"  {area.DestinationMapID}" +
                            $"  {area.SoundTracksCount}" +
                            $"  {area.SoundtrackInterval}";
                        string loopSoundtrack = null;
                        if (area.LoopSoundtrack == true)
                            loopSoundtrack = "1";
                        else
                            loopSoundtrack = "0";
                        line += $"  {loopSoundtrack}" +
                            $"  {area.OriginMapID}" +
                            $"  {area.Priority}";
                        string pkProtection = null;
                        if (selectedPrecinct.Version >= 7)
                        {
                            if (area.PKProtection == true)
                            {
                                pkProtection = "1";
                            }
                            else 
                            { 
                                pkProtection = "0"; 
                            }
                        }
                        line += $"  {pkProtection}";
                        writer.WriteLine(line);
                        writer.WriteLine($"{area.Respawn.X.ToString("0.000000", CultureInfo.InvariantCulture)}," +
                            $" {area.Respawn.Y.ToString("0.000000", CultureInfo.InvariantCulture)}," +
                            $" {area.Respawn.Z.ToString("0.000000", CultureInfo.InvariantCulture)}");

                        foreach (Point point in area.Points)                        
                            writer.WriteLine($"{point.X.ToString("0.000000", CultureInfo.InvariantCulture)}" +
                                $"  {point.Y.ToString("0.000000", CultureInfo.InvariantCulture)}" +
                                $"  {point.Z.ToString("0.000000", CultureInfo.InvariantCulture)}");

                        writer.WriteLine("");

                        foreach (Marker marker in area.Markers)
                            writer.WriteLine($"\"{marker.Name}\"  {marker.X.ToString("0.000000", CultureInfo.InvariantCulture)}" +
                                $"  {marker.Y.ToString("0.000000", CultureInfo.InvariantCulture)}" +
                                $"  {marker.Z.ToString("0.000000", CultureInfo.InvariantCulture)}");

                        writer.WriteLine("");

                        writer.WriteLine($"\"{area.DaySoundtrack}\"");
                        foreach(string music in area.Musics)
                            writer.WriteLine($"\"{music}\"");
                        writer.WriteLine($"\"{area.NightSoundtrack}\"");

                        writer.WriteLine("");
                    }     
                }
            }
        }

        private void toolStripMenuItemSaveSEV_Click(object sender, EventArgs e)
        {
            string savepath = selectedPrecinct.Path + @"\newly saved\";
            if (!Directory.Exists(savepath))
                Directory.CreateDirectory(savepath);

            using (BinaryWriter bw = new BinaryWriter(new FileStream(savepath + @"\precinct.sev", FileMode.Create, FileAccess.Write)))
            {
                bw.Write(uint.Parse(selectedPrecinct.Version.ToString()));
                bw.Write(selectedPrecinct.Areas.Count);
                bw.Write(uint.Parse(selectedPrecinct.Key.ToString()));

                foreach (Area asev in selectedPrecinct.Areas)
                {
                    bw.Write(asev.PointsCount);
                    if (selectedPrecinct.Version >= 6)
                        bw.Write(asev.DomainID);
                    bw.Write(asev.DestinationMapID);
                    if (selectedPrecinct.Version >= 4)
                        bw.Write(asev.OriginMapID);                 
                    bw.Write(asev.Priority);
                    if(selectedPrecinct.Version >= 7)
                        bw.Write(asev.PKProtection);
                    bw.Write(asev.Respawn.X);
                    bw.Write(asev.Respawn.Y);
                    bw.Write(asev.Respawn.Z);
                   
                    foreach (Point point in asev.Points)
                    {
                        bw.Write(point.X);
                        bw.Write(point.Y);
                        bw.Write(point.Z);
                    }

                }

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = 0;
            int count2 = 0;
            if (selectedPrecinct != null && selectedArea != null)
            {
                foreach(Area area in selectedPrecinct.Areas)
                {
                    //if (area.PKProtection)
                    //count++;
                    // else count2++;
                    area.PKProtection = true;
                }
            }
            Console.WriteLine($"Com proteção: {count}\nSem Proteção: {count2}");
        }

        private void comboBoxPrecinctVersion_SelectedValueChanged(object sender, EventArgs e)
        {
            if (selectedPrecinct != null)
            {
                selectedPrecinct.Version = int.Parse(comboBoxPrecinctVersion.SelectedItem.ToString());
                selectedPrecinct.hasChanged = true;
            }
        }

        private void dataGridViewAreas_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right))
                contextMenuStripArea.Show(Cursor.Position);
        }

        private void dataGridViewPoints_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right))
                contextMenuStripPoint.Show(Cursor.Position);
        }

        private void dataGridViewMarkers_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right))
                contextMenuStripMarker.Show(Cursor.Position);
        }

        private void dataGridViewMusics_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right))
                contextMenuStripMusic.Show(Cursor.Position);
        }

        private void dataGridViewMusicTest_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right))
                contextMenuStripMusicTest.Show(Cursor.Position);
        }
        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
