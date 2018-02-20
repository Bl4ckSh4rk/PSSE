using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pokemon_Shuffle_Save_Editor
{
    public class Database
    {
        #region Properties

        public byte[] MegaStoneBin { get; private set; }
        public byte[] MonAbilityBin { get; private set; }
        public byte[] MonDataBin { get; private set; }
        public byte[] MonLevelBin { get; private set; }
        public byte[] MissionCardBin { get; private set; }
        public byte[] MessageDexBin { get; private set; }
        public byte[] StagesEventBin { get; private set; }
        public byte[] StagesExpertBin { get; private set; }
        public byte[] StagesMainBin { get; private set; }
        public byte[] PokeLoadBin { get; private set; }

        public bool[][] HasMega { get; private set; }   // [X][0] = X, [X][1] = Y
        public int[] Forms { get; private set; }
        public List<int>[] Pokathlon { get; private set; }
        public bool[][] Missions { get; private set; }
        public string[] MonsList { get; private set; }
        public string[] SpeciesList { get; private set; }
        public string[] SkillsList { get; private set; }
        public string[] SkillsTextList { get; private set; }
        public Tuple<int, int>[] Megas { get; private set; }    //monsIndex, speedups
        public dbMon[] Mons { get; private set; }   //specieIndex, formIndex, isMega, raiseMaxLevel, basePower, skills, type, stageNum, skillsCount
        public List<int> MegaList { get; private set; } //derivate a List from Megas.Item1 to use with IndexOf() functions (in UpdateForms() & UpdateOwnedBox())
        public dbStage[][] Stages { get; private set; }

        public int MegaStartIndex { get; private set; } // Indexes of first mega & second "---", respectively,...
        public int MonStopIndex { get; private set; }   //...should allow PSSE to work longer without needing an update.

        #endregion Properties

        public Database(bool shwmsg = false, bool dev = false)
        {
            //if a new resource file is needed, don't forget to add a line to Resource_Popup's TLP !
            string[] filenames = { "megaStone.bin", "pokemonData.bin", "stageData.bin", "stageDataEvent.bin", "stageDataExtra.bin", "pokemonLevel.bin", "pokemonAbility.bin", "missionCard.bin", "messagePokedex_US.bin", "pokeLoad.bin" };
            string resourcedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "resources" + Path.DirectorySeparatorChar;
            bool[] overRide = new bool[filenames.Length];
            for (int i = 0; i < overRide.Length; i++)
                overRide[i] = true;
            if (shwmsg)
            {
                string[] fn = new string[filenames.Length];
                for (int i = 0; i < fn.Length; i++)
                    fn[i] = filenames[i];
                Array.Sort(fn, (x, y) => String.Compare(x, y));
                using (var form = new Resources_Popup(fn, resourcedir, dev))
                {
                    form.ShowDialog();
                    if (form.DialogResult == DialogResult.OK && dev)
                    {
                        for (int i = 0; i < overRide.Length; i++)
                            overRide[i] = form.retChk[Array.IndexOf(fn, filenames[i])];
                    }
                }
            }

            //bin init
            MegaStoneBin = Properties.Resources.megaStone;
            MissionCardBin = Properties.Resources.missionCard;
            MonAbilityBin = Properties.Resources.pokemonAbility;
            MonDataBin = Properties.Resources.pokemonData;
            MonLevelBin = Properties.Resources.pokemonLevel;
            StagesMainBin = Properties.Resources.stageData;
            StagesEventBin = Properties.Resources.stageDataEvent;
            StagesExpertBin = Properties.Resources.stageDataExtra;
            MessageDexBin = Properties.Resources.messagePokedex_US;
            PokeLoadBin = Properties.Resources.pokeLoad;

            //resources override            
            if (Directory.Exists(resourcedir))
            {
                for (int i = 0; i < filenames.Length; i++)
                {
                    if (File.Exists(resourcedir + filenames[i]) && overRide[i])
                        switch (i) //don't forget that part or resources files won't override Database files, add an entry if a file is added above
                        {
                            case 0:
                                MegaStoneBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;

                            case 1:
                                MonDataBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;

                            case 2:
                                StagesMainBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;

                            case 3:
                                StagesEventBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;

                            case 4:
                                StagesExpertBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;

                            case 5:
                                MonLevelBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;

                            case 6:
                                MonAbilityBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;
                            case 7:
                                MissionCardBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;
                            case 8:
                                MessageDexBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;
                            case 9:
                                PokeLoadBin = File.ReadAllBytes(resourcedir + filenames[i]);
                                break;
                            default:
                                MessageBox.Show("Error loading resources :\nfilename = " + (filenames[i] != null ? filenames[i] : "null") + "\ni = " + i);
                                break;
                        }
                }

            }


            //txt init
            SpeciesList = Properties.Resources.species.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            MonsList = Properties.Resources.mons.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //PokathlonList = Properties.Resources.pokathlon.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            MegaStartIndex = MonsList.ToList().IndexOf("Mega Venusaur");
            MonStopIndex = MonsList.ToList().IndexOf("---", 1);

            //megas
            int entrylen = BitConverter.ToInt32(MonDataBin, 0x4);
            Megas = new Tuple<int, int>[BitConverter.ToUInt32(MegaStoneBin, 0) - 1];
            for (int i = 0; i < Megas.Length; i++)
            {
                int monIndex = BitConverter.ToUInt16(MegaStoneBin, MegaStoneBin[0x10] + (i + 1) * 4) & 0x3FF;
                string str = "Mega " + MonsList[monIndex];
                int spec = (BitConverter.ToInt32(MonDataBin.Skip(0x50 + entrylen * monIndex).Take(entrylen).ToArray(), 0xE) >> 6) & 0x7FF;
                if (spec == 6 || spec == 150) //if specie is Mewtwo/Charizard, specify if entry is for X or Y stone.
                    str += (monIndex != (BitConverter.ToUInt16(MegaStoneBin, MegaStoneBin[0x10] + i * 4) & 0x3FF)) ? " X" : " Y";
                byte[] data = MonDataBin.Skip(0x50 + entrylen * MonsList.ToList().IndexOf(str)).Take(entrylen).ToArray();
                int maxSpeedup = (BitConverter.ToInt32(data, 0xA) >> 7) & 0x7F;
                Megas[i] = new Tuple<int, int>(monIndex, maxSpeedup);
            }
            MegaList = new List<int>();
            for (int i = 0; i < Megas.Length; i++)
                MegaList.Add(Megas[i].Item1);
            HasMega = new bool[MonsList.Length][];
            for (int i = 0; i < MonsList.Length; i++)
                HasMega[i] = new bool[2];
            for (int i = 0; i < Megas.Length; i++)
                HasMega[BitConverter.ToUInt16(MegaStoneBin, 0x54 + i * 4) & 0x3FF][(MegaStoneBin[0x54 + (i * 4) + 1] >> 3) & 1] = true;

            //pokemons
            Forms = new int[SpeciesList.Length];
            Mons = new dbMon[BitConverter.ToUInt32(MonDataBin, 0)];
            for (int i = 0; i < Mons.Length; i++)
            {
                byte[] data = MonDataBin.Skip(0x50 + entrylen * i).Take(entrylen).ToArray();
                bool isMega = i >= MegaStartIndex && i <= MonsList.Count() - 1;
                int spec = (isMega && i <= MegaStartIndex + Megas.Length - 1)
                    ? SpeciesList.ToList().IndexOf(MonsList[Megas[i - MegaStartIndex].Item1].Substring(0, (MonsList[Megas[i - MegaStartIndex].Item1].LastIndexOf(' ') <= 0) ? MonsList[Megas[i - MegaStartIndex].Item1].Length : MonsList[Megas[i - MegaStartIndex].Item1].LastIndexOf(' ')))
                    : (BitConverter.ToInt32(data, 0xE) >> 6) & 0x7FF;
                int raiseMaxLevel = (BitConverter.ToInt16(data, 0x4)) & 0x3F;
                int basePower = (BitConverter.ToInt16(data, 0x3)) & 0x7; //ranges 1-7 for now (30-90 BP), may need an update later on
                int[] skillsadr = new int[] { 0x02, 0x20, 0x21, 0x22, 0x23 }, skill = new int[skillsadr.Length];
                int j = 0, skillCount = 0;
                foreach (int adr in skillsadr)
                {
                    skill[j] = data[adr]; //ranges 1-~130 for now, ordered list in MESSAGE_XX/message_PokedexXX.bin ("Opportunist" to "Transform" then a bunch more with a lot of placeholders)
                    if (skill[j] != 0) { skillCount++; }
                    j++;
                }
                skillCount = Math.Max(skillCount, 1);
                int type = (BitConverter.ToInt16(data, 0x01) >> 3) & 0x1F; //ranges 0-17 (normal - fairy) (https://gbatemp.net/threads/psse-pokemon-shuffle-save-editor.396499/page-33#post-6278446)
                int index = (BitConverter.ToInt16(data, 0)) & 0x3FF; //ranges 1-999, it's the number you can see on the team selection menu
                Mons[i] = new dbMon(spec, Forms[spec], isMega, raiseMaxLevel, basePower, skill, type, index, skillCount);
                Forms[spec]++;
            }

            //Survival mode
            int smEntry = BitConverter.ToInt32(PokeLoadBin, 0x4), smSkip = BitConverter.ToInt32(PokeLoadBin, 0x10), smTake = BitConverter.ToInt32(PokeLoadBin, 0x14);
            Pokathlon = new List<int>[BitConverter.ToInt16(PokeLoadBin.Skip(smSkip + smTake - smEntry).Take(smEntry).ToArray(), 0) & 0x3FF]; //# of entries doesn't match # of steps since some are collided so I take the last entry and read its 'lowStep' value (should compare to 'highStep' but I don't want to overcomplicate thigns for now)
            for (int i = 0; i < BitConverter.ToInt32(PokeLoadBin, 0); i++)
            {
                byte[] data = PokeLoadBin.Skip(smSkip + i * smEntry).Take(smEntry).ToArray();
                int lowStep = BitConverter.ToInt16(data, 0) & 0x3FF, highStep = (BitConverter.ToInt16(data, 0x01) >> 2) & 0x3FF; //if highStep !=0 then data[] applies to all steps in the lowStep - highStep range
                int min = (BitConverter.ToInt16(data, 0x02) >> 4) & 0xFFF, max = BitConverter.ToInt16(data, 0x04) & 0xFFF; //if max !=0 then all stages in min-max range are possibilities for corresponding step(s)
                List<int> stagesList = Enumerable.Range(min, max != 0 ? max - min + 1 : 1).ToList();
                for (int j = 0x08; j < (data.Length - 3); j += 4) //weird pattern for excluded stages : each 32-bits block starting at 0x08 contains 3 10-bits long stages #
                {
                    int exception = 0;
                    for (int w = 0; w < 3; w++)
                    {
                        exception = (BitConverter.ToInt32(data, j) >> (w * 10)) & 0x3FF;
                        if (exception == 0)
                            break;
                        else if (stagesList.Contains(exception))
                            stagesList.Remove(exception);
                    }
                    if (exception == 0)
                        break;
                }
                foreach (int step in Enumerable.Range(lowStep, 1 + Math.Max(0, highStep - lowStep)))
                    Pokathlon[step - 1] = stagesList;
            }

            #region old Survival
            //pokathlon
            //PokathlonRand = new int[PokathlonList.Length / 2][];
            //for (int i = 0; i < PokathlonRand.Length; i++)
            //{
            //    PokathlonRand[i] = new int[2];
            //    Int32.TryParse(PokathlonList[2 * i], out PokathlonRand[i][0]);
            //    Int32.TryParse(PokathlonList[1 + 2 * i], out PokathlonRand[i][1]);
            //}
            #endregion

            //missions
            Missions = new bool[BitConverter.ToInt32(MissionCardBin, 0)][];
            for (int i = 0; i < Missions.Length; i++)
            {
                Missions[i] = new bool[10];
                int ientrylen = BitConverter.ToInt32(MissionCardBin, 0x4);
                byte[] data = MissionCardBin.Skip(BitConverter.ToInt32(MissionCardBin, 0x10) + i * ientrylen).Take(ientrylen).ToArray();
                for (int j = 0; j < Missions[i].Length; j++)
                    Missions[i][j] = BitConverter.ToInt16(data, 0x8 + 2 * j) != 0;
            }

            //dictionnary (new)
            string temp = Encoding.Unicode.GetString(MessageDexBin.Skip(BitConverter.ToInt32(MessageDexBin, 0x08)).Take(BitConverter.ToInt32(MessageDexBin, 0x0C) - 0x17).ToArray()); //Relevant chunk specified in .bin file, UTF16 Encoding, 0x17 bytes at the end are a useless stamp (data.messagePokedex)
            temp = temp.Replace(Encoding.Unicode.GetString(MessageDexBin.Skip(BitConverter.ToInt32(MessageDexBin, 0x08)).Take(0x10).ToArray()), "[name]"); //because this variable ends with 0x00 it messes with Split() later on, so I replace it here
            temp = temp.Replace(Encoding.Unicode.GetString(new byte[] { 0x01, 0x00, 0x03, 0x01, 0x01, 0x00, 0x03, 0x00, 0x05, 0x00, 0x6D, 0x65, 0x67, 0x61, 0x4E, 0x61, 0x6D, 0x65, 0x00, 0x00 }), "[megaName]"); //same but this variable isn't declared on a fixed position so I copied it directly
            string[] arr = temp.Split((char)0x00); //split the single string in an array
            arr = arr.Skip(Array.IndexOf(arr, "Opportunist")).ToArray(); //we only care for skills so I get rid of anything before Opportunist
            for (int i = 0; i < arr.Length; i++)
            {
                if (String.IsNullOrEmpty(arr[i]))
                    arr[i] = "-Placeholder-"; //make sure there is no empty strings just in case
            }

            /* This code below separates Skills entries from Text entries while ignoring a few mega-skills entries :
             * Right now (1.5.7) the list of strings looks like that : [Skills1][Text for Skills1][Text for mega skills][Skills2][Text for Skills2][Skills3][Text for Skills3].
             * If another group of [Skills][Text for skills] is ever added this will need a 3rd string to concatenate.
             * Also, note that there is no [Mega Skills], which is why I didn't implement it yet (the names of Mega Skills are probably inside another file).
             */

            string[] hardcoded = new string[] { "Opportunist", "Transform", "Big Wave", "Super Cheer", "Not Caught", "Hammering Streak" };  //add hardcoded strings here
            int[] indexes = new int[hardcoded.Length];
            for (int i = 0; i < indexes.Length; i++)
                indexes[i] = Array.IndexOf(arr, hardcoded[i]) + (i % 2); //I add 1 to every odd entry because they correspond to the first text of a group whereas I use the hardcoded last skill of former group instead.
            string[][] stringChunks = new string[indexes.Length][];
            int chunksLength = 0;
            for (int i = 0; i < stringChunks.Length; i++)
            {
                if (i == 4)
                    stringChunks[i] = arr.Skip(indexes[i] + 1).Take(indexes[2 * (i / 2) + 1] - indexes[2 * (i / 2)] - 1).ToArray(); //because for some reason in v1.5.7 skill "Not Caught" doesn't have a flavour entry so I had to remove it manually so the texts match correctly.
                else
                    stringChunks[i] = arr.Skip(indexes[i] + ((i == 4) ? 1 : 0)).Take(indexes[2 * (i / 2) + 1] - indexes[2 * (i / 2)]).ToArray();    
                if (i % 2 == 1)
                    chunksLength += Math.Max(stringChunks[i].Length, stringChunks[i - 1].Length);
            }
            string[][] skillText = new string[2][]; //[0] = skill name / [1] = flavor text
            for (int i = 0; i < skillText.Length; i++)
                skillText[i] = new string[chunksLength];
            for (int i = 0; i < stringChunks.Length; i++)
                stringChunks[i].CopyTo(skillText[i % 2], Array.IndexOf(skillText[i % 2], null));
            SkillsList = skillText[0];
            SkillsTextList = skillText[1];

            //stages --> there are a lot of things that could be added here
            Stages = new dbStage[3][];
            Stages[0] = new dbStage[(BitConverter.ToInt32(StagesMainBin, 0) - 1) * 2];  //Main + UX stages
            Stages[1] = new dbStage[BitConverter.ToInt32(StagesExpertBin, 0)];  //EX stages
            Stages[2] = new dbStage[BitConverter.ToInt32(StagesEventBin, 0)];   //Event stages
            for (int i = 0; i < Stages.Length; i++)
            {
                byte[] stagesData = new byte[][] { StagesMainBin, StagesExpertBin, StagesEventBin }[i];
                int stgEntrylen = BitConverter.ToInt32(stagesData, 0x4);
                for (int j = 0; j < Stages[i].Length; j++)
                {
                    int index = j;
                    bool isux = (i == 0 && j >= (Stages[i].Length / 2));
                    if (i == 0)
                    {
                        if (isux)
                            index -= Stages[i].Length / 2;
                        index++;
                    }
                    byte[] data = stagesData.Skip(0x50 + stgEntrylen * index).Take(stgEntrylen).ToArray();
                    int pkmn = BitConverter.ToInt16(data, 0) & 0x7FF;
                    int sranks = BitConverter.ToInt16(data, 0x4C) & 0x3FF;  //999 = unreleased EX stage, anything else = number of S ranks (EX stages) or completed stages (Main stages) required for that level to be unlocked
                    int hp = (int)(BitConverter.ToUInt64(data, 0x4) & 0xFFFFFF) * (isux ? 3 : 1);
                    int[] minmoves = new int[4];    //required number of moves left for C, B, A, S rank
                    for (int k = 0; k < minmoves.Length; k++)
                        minmoves[k] = ((k > 0) ? ((BitConverter.ToInt16(data, 0x30 + k - 1) >> 4) & 0xFF) : 0);  //data only has numbers for A, B & S ranks since C rank always require 0 moves left

                    Stages[i][j] = new dbStage(isux, pkmn, sranks, hp, minmoves);
                }
            }
        }
    }
}