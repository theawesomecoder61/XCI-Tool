using Be.Windows.Forms;
using ByteSizeLib;
using Force.Crc32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using theawesomecoder61.Helpers;

namespace XCI_Tool
{
    public partial class Form1 : Form
    {
        private BetterBinaryReader bbr;
        private BetterTreeNode rootNode;

        public static byte[] MAGIC_HEAD = new byte[] { 0x48, 0x45, 0x41, 0x44 };
        public static byte[] MAGIC_HFS0 = new byte[] { 0x48, 0x46, 0x53, 0x30 };

        public TreeViewFileSystem tvfs;
        public HFS0 PartitionTable;
        public List<HFS0> Partitions;

        public byte[] Signature;
        public int ofsGamePartition1;
        public short cartridge;
        public int szRomData;
        public long ofsPartitionTable;
        public long szPartitionTableHeader;
        public byte[] HashPartitionTable;
        public int ofsGamePartition2;
        public string hash;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sendbr, EventArgs e)
        {
            hexBoxPanel.Controls.Add(hexBox);

            bbr = new BetterBinaryReader();
            tvfs = new TreeViewFileSystem(treeView1);
            PartitionTable = new HFS0();
            Partitions = new List<HFS0>();

            hash = "Calculating... (check back in a minute)";

            rootNode = new BetterTreeNode("root");
            rootNode.Offset = -1;
            rootNode.Size = -1;
            treeView1.Nodes.Add(rootNode);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DisplayData();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName != bbr.FileName)
                {
                    bbr.Load(openFileDialog1.FileName);
                    ReadXCI();
                }
                else
                {
                    MessageBox.Show("That file is already open in XCI Tool.", "Ummm...");
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                BetterTreeNode btn = (BetterTreeNode)treeView1.SelectedNode;
                Console.WriteLine("OFFSET: " + btn.Offset + " SIZE: " + btn.Size);

                if (btn.Offset > -1 && btn.Size > 0)
                {
                    saveFileDialog1.FileName = btn.Text;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        Task t = Task.Factory.StartNew(() =>
                        {
                            long pos = bbr.Position();
                            bbr.Seek(btn.Offset);

                            // prepare your computer's drive, we're about to write some sh*t
                            using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                            {
                                fs.SetLength(btn.Size);
                                fs.Seek(0, SeekOrigin.Begin);

                                int chunkSize = 5392; // a divisor
                                long iterationsNoRemainder = btn.Size / chunkSize;
                                Console.WriteLine("ITERATIONS: " + iterationsNoRemainder);

                                for (int i = 0; i < iterationsNoRemainder; i++)
                                {
                                    if (i * chunkSize < btn.Size)
                                    {
                                        byte[] b1 = bbr.ReadBytes(chunkSize);
                                        foreach (byte b in b1)
                                        {
                                            fs.WriteByte(b);
                                        }
                                    }
                                }
                            }
                            Console.WriteLine("!! DONE !!");


                            /* Stream s;
                            using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                            {
                                fs.Seek(0, SeekOrigin.Begin);

                                s = bbr.ReadBytesButLonger(btn.Size);
                                s.Seek(0, SeekOrigin.Begin);

                                s.CopyTo(fs);
                            }
                            s.Close(); */

                            bbr.Seek(pos);
                        });
                    }
                }
            }
        }

        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/theawesomecoder61/XCI-Tool");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Created by theawesomecoder61\n\nThanks to Falo on GBA Temp (for some code) and SwitchBrew (for providing information about the XCI format)", "XCI Tool version " + Application.ProductVersion);
        }

        //
        //
        //

        private void ReadXCI()
        {
            // clear all child nodes
            rootNode.Nodes.Clear();

            Signature = bbr.ReadBytes(0x100);

            byte[] magic = bbr.ReadBytes(4);
            if (magic[0] != ToDec(MAGIC_HEAD[0]) && magic[1] != ToDec(MAGIC_HEAD[1]) && magic[2] != ToDec(MAGIC_HEAD[2]) && magic[3] != ToDec(MAGIC_HEAD[3]))
            {
                throw new Exception("Invalid magic!");
            }

            ofsGamePartition1 = bbr.ReadInt32(); // SECTOR_SIZE
            bbr.ReadInt32(); // always 0xFFFFFFFF
            bbr.Read(); // ?
            cartridge = bbr.ReadInt16(); // cartridge size
            bbr.Read(); // ?
            bbr.Read(); // ?
            bbr.ReadInt64(); // ?
            szRomData = bbr.ReadInt32(); // SECTOR_SIZE = Rom Data Size, without this header
            bbr.ReadBytes(0x4); // some key or hash?

            long pos = bbr.Position();
            bbr.Seek(304); // hardcoded value
            ofsPartitionTable = bbr.ReadInt64();
            bbr.Seek(pos);
            // Console.WriteLine(bbr.Position());

            szPartitionTableHeader = bbr.ReadInt64();
            HashPartitionTable = bbr.ReadBytes(0x20); // sha256 hash ovbr the PartitionTable (use szPartitionTableHeader)
            bbr.ReadBytes(0x20); // sha256 hash over cbrt area? if yes then bbb's dumps will never work on a flashcard...
            bbr.ReadInt32(); // ?
            bbr.ReadInt32(); // ?
            bbr.ReadInt32(); // ?
            ofsGamePartition2 = bbr.ReadInt32(); //* SECTOR_SIZE
            bbr.ReadBytes(0x70); // more hashes?

            // add cbrt area to file tree
            tvfs.AddFile("CbrT.dat", rootNode, 0x7000, 0x200); // dir ID, file name, offset, size

            // read Partition Table
            bbr.Seek(ofsPartitionTable);
            PartitionTable.ReadData(bbr);

            long partitionStart = bbr.Position();
            HFS0 temp;

            for (int i = 0; i < PartitionTable.FileCount; i++)
            {
                tvfs.AddFile(PartitionTable.names[i] + ".hfs0", rootNode, partitionStart + PartitionTable.entries[i].Offset, PartitionTable.entries[i].Size);
                var curPartition = tvfs.AddDir(PartitionTable.names[i], rootNode);

                // read Partition data
                bbr.Seek(partitionStart + PartitionTable.entries[i].Offset);
                temp = new HFS0();
                temp.ReadData(bbr);

                long curPartitionStart = bbr.Position();

                // add Partition data to file tree
                for (int j = 0; j < temp.FileCount; j++)
                {
                    tvfs.AddFile(temp.names[j], curPartition, curPartitionStart + temp.entries[j].Offset, temp.entries[j].Size);

                    TreeNode[] parents = treeView1.Nodes.Find(curPartition.Text, true);
                    if (parents.Length > 0)
                    {
                        tvfs.AddFile(temp.names[j], (BetterTreeNode)parents[0], 0, 0);
                    }
                }

                Partitions.Add(temp);
            }

            DisplayData();

            // GetInformationAboutGame();
            // Task.Run(async () => { await GetInformationAboutGame(); }).Wait();
        }

        private void DisplayData()
        {
            if (bbr.Initiated)
            {
                BetterTreeNode btn = (BetterTreeNode)treeView1.SelectedNode;

                // show information
                currentFileLabel.Text = string.Format("File: {0} ({1})", btn.Text, btn.FullPath);
                exportFileToolStripMenuItem.Enabled = btn.Size > -1 && btn != rootNode;

                // show different information depending on what is selected
                if (btn != rootNode)
                {
                    if (btn.Offset > -1)
                    {
                        infoRTB.Text = string.Format("File: {0}\nPath: {1}\n\nOffset: 0x{2} ({3} {4})\nSize: 0x{5} ({6} {7})", btn.Text, btn.FullPath, ((int)btn.Offset).ToString("X2"), Math.Round(ByteSize.FromBytes(btn.Offset).LargestWholeNumberValue, 2), ByteSize.FromBytes(btn.Offset).LargestWholeNumberSymbol, ((int)btn.Size).ToString("X2"), Math.Round(
                            ByteSize.FromBytes(btn.Size).LargestWholeNumberValue, 2), ByteSize.FromBytes(btn.Size).LargestWholeNumberSymbol);
                    }
                    else
                    {
                        infoRTB.Text = string.Format("Folder: {0}\nPath: {1}\n\nFiles: {2}", btn.Text, btn.FullPath, btn.Nodes.Count);
                    }
                }
                else
                {
                    infoRTB.Text = string.Format("File: {0}\nPath: {1}\n\nGame Data Size: {2} media units\nCartridge Size: {3}\n\nSHA256 Hash of the HFS0 Partition: {4}\nSHA256 Hash of the Crypto Partition: {5}\nCRC32 Hash: {6}", Path.GetFileName(bbr.FileName), Path.GetDirectoryName(bbr.FileName), szRomData, GetCartridge(), "", "", hash);
                }

                // get file data and display it in the hex viewer
                if (btn.Offset > -1)
                {
                    long pos = bbr.Position();
                    bbr.Seek(btn.Offset);

                    // no more than 10 MB of data
                    int size = 0;
                    if (btn.Size > -1 && ByteSize.FromBytes(btn.Size) <= ByteSize.FromMegaBytes(10) && btn.Size <= int.MaxValue)
                    {
                        size = (int)btn.Size;
                    }
                    else
                    {
                        size = (int)ByteSize.FromMegaBytes(10).Bytes;
                    }

                    // show the data and go back to the original position
                    hexBox.ByteProvider = new DynamicByteProvider(bbr.ReadBytes(size));
                    bbr.Seek(pos);
                }
                else
                {
                    hexBox.ByteProvider = new DynamicByteProvider(new byte[] { 0x0 });
                }
            }
        }

        private int ToDec(byte b)
        {
            return Convert.ToInt32(b);
        }

        private string GetCartridge()
        {
            string r = "";
            switch (cartridge.ToString("X2"))
            {
                case "F8":
                    r = "2 GB";
                    break;
                case "F0":
                    r = "4 GB";
                    break;
                case "E0":
                    r = "8 GB";
                    break;
                case "E1":
                    r = "16 GB";
                    break;
            }
            return r;
        }

        /* private byte[] GetBytesOfFile(BetterTreeNode btn)
        {
            if (btn.Offset > -1 && btn.Size > -1)
            {
                long pos = bbr.Position();
                bbr.Seek(btn.Offset);

                byte[] b = bbr.ReadBytes(btn.Size);

                bbr.Seek(pos);
                return b;
            }
            else
            {
                return new byte[] { };
            }
        } */

        private DateTime started;
        private DateTime ended;
        private Task<string> GetInformationAboutGameAsync()
        {
            started = DateTime.Now;

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task.Run(() =>
            {
                string h = "";
                bbr.Seek(0);

                // generate a CRC32 hash
                Crc32Algorithm crc32 = new Crc32Algorithm();
                foreach (byte b in crc32.ComputeHash(bbr.Stream))
                {
                    h += b.ToString("X2").ToUpper();
                }

                tcs.SetResult(h);
            });
            return tcs.Task;

            /* Task.Factory.ContinueWhenAll(tasks.ToArray(), completedTasks =>
            {
                // load the list of Switch games and match the hash with a game's hash
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NSWreleases.xml"));
                XmlNode n = doc.SelectSingleNode("releases/release[imgcrc = '" + hash + "']");
                Console.WriteLine("!!!!! " + n["name"].InnerText);
            }); */
        }

        private async void GetInformationAboutGame()
        {
            string result = await GetInformationAboutGameAsync();
            hash = result;

            ended = DateTime.Now;
            Console.WriteLine("calculating the HASH " + result + " took " + (ended - started).TotalSeconds + " seconds");
        }
    }
}