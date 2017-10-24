using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Pokemon_Shuffle_Save_Editor.Properties;
using static Pokemon_Shuffle_Save_Editor.Main;
using System.Windows.Forms;

namespace Pokemon_Shuffle_Save_Editor
{
    public static class ToolFunctions
    {
        public static int GetMonFrommSlot(int slot)
        {
            return (BitConverter.ToInt32(savedata, TeamData.Ofset(slot)) >> TeamData.Shift(slot)) & 0xFFF;
        }

        public static void SetMonToSlot(int slot, int ind)
        {
            if (slot == ltir)
                ltir = -1;
            int data = BitConverter.ToInt32(savedata, TeamData.Ofset(slot));
            data = (data & ~(0xFFF << TeamData.Shift(slot))) | (ind << TeamData.Shift(slot));
            Array.Copy(BitConverter.GetBytes(data), 0, savedata, TeamData.Ofset(slot), 4);
        }

        public static monItem GetMon(int ind)
        {
            bool caught = true;
            foreach (int array in Caught.Ofset(ind))
            {
                if (((savedata[array] >> Caught.Shift(ind)) & 1) != 1)
                    caught = false;
            }
            int lev = Math.Max((BitConverter.ToUInt16(savedata, Level1.Ofset(ind)) >> Level1.Shift(ind)) & 0xF, (BitConverter.ToUInt16(savedata, Level2.Ofset(ind)) >> Level2.Shift(ind)) & 0x3F);
            lev = (lev == 0) ? 1 : lev;
            int rml = (BitConverter.ToUInt16(savedata, Lollipop.Ofset(ind)) >> Lollipop.Shift(ind)) & 0x3F;
            int exp = (BitConverter.ToInt32(savedata, Experience.Ofset(ind)) >> Experience.Shift(ind)) & 0xFFFFFF;
            short stone = (short)((savedata[Mega.Ofset(ind)] >> Mega.Shift(ind)) & 3);  //0 = 00, 1 = X0, 2 = 0Y, 3 = XY
            short speedUpX = (short)(db.HasMega[ind][0] ? (BitConverter.ToInt16(savedata, SpeedUpX.Ofset(ind)) >> SpeedUpX.Shift(ind)) & 0x7F : 0);
            short speedUpY = (short)(db.HasMega[ind][1] ? (BitConverter.ToInt32(savedata, SpeedUpY.Ofset(ind)) >> SpeedUpY.Shift(ind)) & 0x7F : 0);
            short selSkill = (short)((BitConverter.ToInt16(savedata, CurrentSkill.Ofset(ind)) >> CurrentSkill.Shift(ind)) & 0x7);
            int[] skillLvl = new int[5], skillExp = new int[5];
            for (int i = 0; i < db.Rest[ind].Item2; i++)
            {
                int sLv = (BitConverter.ToInt16(savedata, SkillLevel.Ofset(ind, i)) >> SkillLevel.Shift(ind)) & 0x7;
                skillLvl[i] = (sLv < 2) ? 1 : sLv;
                skillExp[i] = savedata[SkillExp.Ofset(ind, i)];
            }

            return new monItem { Caught = caught, Level = lev, Lollipops = rml, Exp = exp, Stone = stone, SpeedUpX = speedUpX, SpeedUpY = speedUpY, CurrentSkill = selSkill, SkillLevel = skillLvl, SkillExp = skillExp };
        }

        public static void SetCaught(int ind, bool caught = false)
        {
            foreach (int array in Caught.Ofset(ind))
                savedata[array] = (byte)((savedata[array] & (byte)(~(1 << Caught.Shift(ind)))) | (byte)((caught ? 1 : 0) << Caught.Shift(ind)));
        }

        public static void SetLevel(int ind, int lev = 1, int set_rml = -1, int set_exp = -1)
        {
            //level patcher
            lev = (lev < 2) ? 0 : lev;
            short level1 = (short)((BitConverter.ToInt16(savedata, Level1.Ofset(ind)) & ~(0xF << Level1.Shift(ind))) | (Math.Min(lev, 0xF) << Level1.Shift(ind)));
            Array.Copy(BitConverter.GetBytes(level1), 0, savedata, Level1.Ofset(ind), 2);    //write to original ofsets ( <= 15)
            short level2 = (short)((BitConverter.ToInt16(savedata, Level2.Ofset(ind)) & ~(0x3F << Level2.Shift(ind))) | ((lev < 15) ? 0 : Math.Min(lev, 0x3F) << Level2.Shift(ind)));
            Array.Copy(BitConverter.GetBytes(level2), 0, savedata, Level2.Ofset(ind), 2);    //write to 1.3.25 ofsets ( >= 15, 0 if below)

            //lollipop patcher
            set_rml = (set_rml < 0 || (set_rml < lev - 10) ? Math.Max(0, lev - 10) : set_rml);
            short numRaiseMaxLevel = (short)((BitConverter.ToInt16(savedata, Lollipop.Ofset(ind)) & ~(0x3F << Lollipop.Shift(ind))) | (Math.Min(set_rml, 0x3F) << Lollipop.Shift(ind)));
            Array.Copy(BitConverter.GetBytes(numRaiseMaxLevel), 0, savedata, Lollipop.Ofset(ind), 2);

            //experience patcher
            int entrylen = BitConverter.ToInt32(db.MonLevel, 0x4);
            byte[] data = db.MonLevel.Skip(0x50 + (Math.Min(db.Mons[ind].Item4 + 10, (Math.Max(0, lev - 1))) * entrylen)).Take(entrylen).ToArray(); //makes sure to read exp values within the correct range (0 to max level)
            set_exp = (set_exp < 0) ? BitConverter.ToInt32(data, 0x4 * (db.Mons[ind].Item5 - 1)) : set_exp;
            int exp = (BitConverter.ToInt32(savedata, Experience.Ofset(ind)) & ~(0xFFFFFF << Experience.Shift(ind))) | (Math.Min(set_exp, 0xFFFFFF) << Experience.Shift(ind));
            Array.Copy(BitConverter.GetBytes(exp), 0, savedata, Experience.Ofset(ind), 4);
        }

        public static void SetSkill(int ind, int skind = 0, int lvl = 1, bool current = false)
        {
            //level
            lvl = (lvl < 2) ? 0 : lvl;
            skind = (skind < 0) ? 0 : ((skind > 4) ? 4 : skind); //hardcoded 5 skills maximum
            int skilllvl = BitConverter.ToInt16(savedata, SkillLevel.Ofset(ind, skind));
            skilllvl = (skilllvl & ~(0x7 << SkillLevel.Shift(ind))) | (Math.Min(0x7, lvl) << SkillLevel.Shift(ind));
            Array.Copy(BitConverter.GetBytes(skilllvl), 0, savedata, SkillLevel.Ofset(ind, skind), 2);

            //exp
            int entrylen = BitConverter.ToInt32(db.MonAbility, 0x4);
            savedata[SkillExp.Ofset(ind, skind)] = (lvl < 2) ? (byte)0 : db.MonAbility.Skip(0x50 + db.Mons[ind].Item6[skind] * entrylen).Take(entrylen).ToArray()[0x1A + lvl];

            //current
            if (current)
            {
                int selskill = BitConverter.ToInt16(savedata, CurrentSkill.Ofset(ind));
                selskill = (selskill & ~(0x7 << CurrentSkill.Shift(ind))) | (skind << CurrentSkill.Shift(ind));
                Array.Copy(BitConverter.GetBytes(selskill), 0, savedata, CurrentSkill.Ofset(ind), 2);
            }
        }//hardcoded 5 skills maximum

        public static void SetSpeedup(int ind, bool X = false, int suX = 0, bool Y = false, int suY = 0)
        {
            if (db.HasMega[ind][0] || db.HasMega[ind][1])
            {
                int speedUp_Val = BitConverter.ToInt32(savedata, SpeedUpX.Ofset(ind));
                if (db.HasMega[ind][0])
                {
                    speedUp_Val &= ~(0x7F << SpeedUpX.Shift(ind));
                    speedUp_Val |= (X ? suX : 0) << SpeedUpX.Shift(ind);
                }
                if (db.HasMega[ind][1])
                {   //Y shifts are relative to X ofsets.
                    speedUp_Val &= ~(0x7F << ((SpeedUpY.Ofset(ind) - SpeedUpX.Ofset(ind)) * 8 + SpeedUpY.Shift(ind)));
                    speedUp_Val |= (Y ? suY : 0) << ((SpeedUpY.Ofset(ind) - SpeedUpX.Ofset(ind)) * 8 + SpeedUpY.Shift(ind));
                }
                Array.Copy(BitConverter.GetBytes(speedUp_Val), 0, savedata, SpeedUpX.Ofset(ind), 4);
            }
        }

        public static void SetStone(int ind, bool X = false, bool Y = false)
        {
            short mega_val = (short)((BitConverter.ToInt16(savedata, Mega.Ofset(ind)) & ~(3 << Mega.Shift(ind))) | (((X ? 1 : 0) | (Y ? 2 : 0)) << Mega.Shift(ind)));
            Array.Copy(BitConverter.GetBytes(mega_val), 0, savedata, Mega.Ofset(ind), 2);
        }

        public static rsItem GetRessources()
        {
            ShuffleItems res = new ShuffleItems();
            for (int i = 0; i < res.Items.Length; i++)
                res.Items[i] = (BitConverter.ToUInt16(savedata, Items.Ofset(i)) >> Items.Shift()) & 0x7F;
            for (int i = 0; i < res.Enchantments.Length; i++)
                res.Enchantments[i] = (savedata[Enhancements.Ofset(i)] >> Enhancements.Shift()) & 0x7F;
            return new rsItem
            {
                Hearts = (BitConverter.ToUInt16(savedata, Hearts.Ofset()) >> Hearts.Shift()) & 0x7F,
                Coins = (BitConverter.ToInt32(savedata, Coins.Ofset()) >> Coins.Shift()) & 0x1FFFF,
                Jewels = (BitConverter.ToInt32(savedata, Jewels.Ofset()) >> Jewels.Shift()) & 0xFF,
                Items = res.Items,
                Enhancements = res.Enchantments
            };
        }

        public static void SetResources(uint hearts = 0, uint coins = 0, uint jewels = 0, int[] items = null, int[] enhancements = null)
        {
            if (items == null)
                items = new int[ShuffleItems.ILength];
            if (enhancements == null)
                enhancements = new int[ShuffleItems.ELength];
            Array.Copy(BitConverter.GetBytes((BitConverter.ToUInt32(savedata, Coins.Ofset()) & 0xF0000007) | (Math.Min(0x1FFFF, coins) << Coins.Shift()) | (Math.Min(0xFF, jewels) << Jewels.Shift())), 0, savedata, Coins.Ofset(), 4);
            Array.Copy(BitConverter.GetBytes((BitConverter.ToUInt16(savedata, Hearts.Ofset()) & (uint)0xC07F) | (Math.Min(0x7F, hearts) << Hearts.Shift())), 0, savedata, Hearts.Ofset(), 2);
            for (int i = 0; i < ShuffleItems.ILength; i++) //Items (battle)
            {
                ushort val = BitConverter.ToUInt16(savedata, Items.Ofset(i));
                val &= 0x7F;
                val |= (ushort)(Math.Min(0x7F, items[i]) << Items.Shift());
                Array.Copy(BitConverter.GetBytes(val), 0, savedata, Items.Ofset(i), 2);
            }
            for (int i = 0; i < ShuffleItems.ELength; i++) //Enhancements (pokemon)
                savedata[Enhancements.Ofset(i)] = (byte)(((Math.Min(0x7F, enhancements[i]) << Enhancements.Shift()) & 0xFE) | (savedata[Enhancements.Ofset(i)] & Enhancements.Shift()));
        }

        public static stgItem GetStage(int ind, int type)
        {
            return new stgItem
            {
                Completed = ((BitConverter.ToInt16(savedata, Completed.Ofset(ind, type)) >> Completed.Shift(ind, type)) & 7) == 5,
                Rank = (BitConverter.ToInt16(savedata, Rank.Ofset(ind, type)) >> Rank.Shift(ind, type)) & 0x3,
                Score = (int)((BitConverter.ToUInt64(savedata, Score.Ofset(ind, type)) >> Score.Shift(ind, type)) & 0xFFFFFF)
            };
        }

        public static void SetRank(int ind, int type, int newRank = 0)
        {
            short rank = (short)((BitConverter.ToInt16(savedata, Rank.Ofset(ind, type)) & ~(0x3 << Rank.Shift(ind, type))) | (newRank << Rank.Shift(ind, type)));
            Array.Copy(BitConverter.GetBytes(rank), 0, savedata, Rank.Ofset(ind, type), 2);
            PatchScore(ind, type);
        }

        public static void SetScore(int ind, int type, int newScore = 0)
        {
            long score = (BitConverter.ToInt64(savedata, Score.Ofset(ind, type)) & ~(0xFFFFFF << Score.Shift(ind, type))) | (Math.Min(0xFFFFFF, (long)newScore) << Score.Shift(ind, type));
            Array.Copy(BitConverter.GetBytes(score), 0, savedata, Score.Ofset(ind, type), 8);
        }

        public static void SetStage(int ind, int type, bool completed = false)
        {
            short stage = (short)(BitConverter.ToInt16(savedata, Completed.Ofset(ind, type)) & (~(0x7 << Completed.Shift(ind, type))) | ((completed ? 5 : 0) << Completed.Shift(ind, type)));
            Array.Copy(BitConverter.GetBytes(stage), 0, savedata, Completed.Ofset(ind, type), 2);
            PatchScore(ind, type);
        }

        public static void PatchScore(int ind, int type)
        {  
            byte[] stage = new byte[][] { db.StagesMain, db.StagesExpert, db.StagesEvent}[type];
            int entrylen = BitConverter.ToInt32(stage, 0x4);
            byte[] data = stage.Skip(0x50 + (ind + ((type == 0) ? 1 : 0)) * entrylen).Take(entrylen).ToArray();
            int score = (int)Math.Max((ulong)GetStage(ind, type).Score, (BitConverter.ToUInt64(data, 0x4) & 0xFFFFFF) + (ulong)(Math.Min(7000, ((GetStage(ind, type).Rank > 0) ? ((BitConverter.ToInt16(data, 0x30 + GetStage(ind, type).Rank - 1) >> 4) & 0xFF) : 0) * 500)));  //score = Max(current_highscore, hitpoints + minimum_bonus_points (a.k.a min moves left times 500, capped at 7000))
            SetScore(ind, type, GetStage(ind,type).Completed ? score : 0); 
        }

        public static void SetExcalationStep(int step = 1)
        {   //Will only update 1 escalation battle. Update offsets if there ever are more than 1 at once
            if (step < 1)
                step = 1;
            if (step > 999)
                step = 999;
            int data = BitConverter.ToUInt16(savedata, EscalationStep.Ofset());
            data = (data & (~(0x3FF << EscalationStep.Shift()))) | (step-- << EscalationStep.Shift());  //sets previous step as beaten = selected step shown in game
            Array.Copy(BitConverter.GetBytes(data), 0, savedata, EscalationStep.Ofset(), 2);
        }

        //public static smItem GetPokathlon()
        //{
        //    return new smItem
        //    {
        //        Opponent = BitConverter.ToInt16(savedata, PokathlonOpponent.Ofset()) >> PokathlonOpponent.Shift(),
        //        Moves = (savedata[PokathlonMoves.Ofset()] & 0x7F),
        //        Step = savedata[PokathlonStep.Ofset()]
        //    };
        //}

        //public static void SetOpponent(int opponent)
        //{
        //    Array.Copy(BitConverter.GetBytes((BitConverter.ToInt16(savedata, PokathlonOpponent.Ofset()) & ~(0x3FF << PokathlonOpponent.Shift())) | (opponent << PokathlonOpponent.Shift())), 0, savedata, PokathlonOpponent.Ofset(), 2);
        //}

        public static Bitmap GetMonImage(int ind, bool megaWithOverlay = false)
        {
            string imgname = string.Empty;
            int mon_num = db.Mons[ind].Item1, form = db.Mons[ind].Item2;
            bool mega = db.Mons[ind].Item3;
            if (mega || megaWithOverlay)
            {
                form -= db.HasMega[mon_num][1] ? 1 : 2; //Differenciate Rayquaza/Gyarados from Charizard/Mewtwo, otherwise either stage 300 is Shiny M-Ray or stage 150 is M-mewtwo X
                imgname += "mega_";
            }
            imgname += "pokemon_" + mon_num.ToString("000");
            if (form > 0 && mon_num > 0)
                imgname += "_" + form.ToString("00");
            if (mega)
                imgname += "_lo";
            return new Bitmap((Image)Properties.Resources.ResourceManager.GetObject(imgname));
        }

        public static Bitmap GetCaughtImage(int ind, bool caught = false)
        {
            Bitmap bmp = GetMonImage(ind);
            if (!caught)
                bmp = GetShadow(bmp); //add a color here to change the default color of the "shadow"
            return bmp;
        }

        public static Bitmap GetStageImage(int ind, int type, bool hidden = false)
        {
            Bitmap bmp = new Bitmap(64, 80);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                if (db.Mons[ind].Item3 && (type == 0))
                    g.DrawImage(Properties.Resources.PlateMega, new Point(0, 16));
                else
                    g.DrawImage(Properties.Resources.Plate, new Point(0, 16));
                g.DrawImage(ResizeImage(GetCaughtImage(ind, !hidden), 48, 48), new Point(8, 7));
            }

            return bmp;
        }

        public static Bitmap GetRankImage(int ind, int type, bool completed, int rank, bool hidden = false)
        {
            Bitmap stage = GetStageImage(ind, type, hidden);
            if (completed && rank >= 0 && rank <= 4)
            {
                using (Graphics g = Graphics.FromImage(stage))
                    g.DrawImage(ResizeImage(new Bitmap[] { Resources.Rank_C, Resources.Rank_B, Resources.Rank_A, Resources.Rank_S }[rank], 32, 32), new Point(32, 48));
            }
            return stage;
        }

        //public static Bitmap GetCompletedImage(int ind, int type, bool completed = true)
        //{
        //    Bitmap bmp = GetStageImage(ind, type);
        //    if (!completed)
        //        bmp = GetShadow(bmp);
        //    return bmp;
        //}

        public static Bitmap GetTeamImage(int ind, int slot = -1, bool selected = false, int w = 48, int h = 48)
        {
            bool megaOverlay = (GetMon(ind).Stone != 0 && slot == 0);
            Bitmap bmp = GetMonImage(ind, megaOverlay);
            if (selected)
            {
                Bitmap bmp2 = ResizeImage(bmp, 54, 54);
                using (Graphics g = Graphics.FromImage(GetShadow(bmp, GetDominantColor(bmp))))
                    g.DrawImage(bmp2, new Point(5, 5));
            }
            bmp = ResizeImage(bmp, 48, 48);
            if (megaOverlay)
            {
                Bitmap bmp3 = db.HasMega[ind][0] ? new Bitmap((Image)Properties.Resources.ResourceManager.GetObject("MegaStone" + db.Mons[ind].Item1.ToString("000") + (db.HasMega[ind][1] ? "_X" : string.Empty))) : new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(Properties.Resources.MegaStoneBase, new Point(0, 0));
                    g.DrawImage(bmp3, new Point(3, 2));
                }                    
            }
            return bmp;
            //return ResizeImage(ChangeOpacity(GetMonImage(ind), opacity ? 0.5F : 1), w, h);
        }

        public static Bitmap GetShadow(Bitmap bmp, Color c = default(Color))
        {
            if (c == default(Color))
                c = Color.Black;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pix = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(pix.A, c.R, c.G, c.B));
                }
            }
            return bmp;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            if (image.HorizontalResolution > 0 && image.VerticalResolution > 0)
                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Color GetDominantColor(Bitmap bmp)
        {
            int r = 0, g = 0, b = 0, total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (c.A == 0) { continue; } //ignore transparent pixels
                    r += c.R;
                    g += c.G;
                    b += c.B;
                    total++;
                }
            }
            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb(r, g, b);
        }

        //public static Bitmap ChangeOpacity(Image img, float opacityvalue)
        //{
        //    Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
        //    Graphics graphics = Graphics.FromImage(bmp);
        //    ColorMatrix colormatrix = new ColorMatrix();
        //    colormatrix.Matrix33 = opacityvalue;
        //    ImageAttributes imgAttribute = new ImageAttributes();
        //    imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        //    graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
        //    graphics.Dispose();   // Releasing all resource used by graphics
        //    return bmp;
        //}
    }

    #region Shifts&Ofsets

    public static class Caught
    {
        public static int[] Ofset(int ind)
        {
            int j = 0;
            int[] ofsets = new int[3];
            foreach (int caught_array_start in new[] { 0xE6, 0x546, 0x5E6 })
            {
                ofsets[j] = caught_array_start + ((ind - 1) + 6) / 8;
                j++;
            }
            return ofsets;
        }

        public static int Shift(int ind)
        {
            return ((ind - 1) + 6) % 8;
        }
    }
    public static class Experience
    {
        public static int Ofset(int ind)
        {
            return 0x3241 + (4 + (ind - 1) * 24) / 8;
        }

        public static int Shift(int ind)
        {
            return (4 + (ind - 1) * 24) % 8;
        }
    }
    public static class Level1
    {//"old" offsets, lvl 15 max (0xF), keep saying 15 if level is higher.
        public static int Ofset(int ind)
        {
            return 0x187 + ((ind - 1) * 4 + 1) / 8;
        }

        public static int Shift(int ind)
        {
            return ((ind - 1) * 4 + 1) % 8;
        }
    }
    public static class Level2
    {//"new" 1.3.25 offsets, register level if >= 15 (max 0x3F = 63), doesn't register lvl 15 mons that were leveled up before the update.
        public static int Ofset(int ind)
        {
            return 0xA61B + ind * 6 / 8;
        }

        public static int Shift(int ind)
        {
            return ind * 6 % 8;
        }
    }
    public static class Lollipop
    {
        public static int Ofset(int ind)
        {
            return 0xA9DB + (ind * 6) / 8;
        }

        public static int Shift(int ind)
        {
            return (ind * 6) % 8;
        }
    }
    public static class CurrentSkill
    {
        public static int Ofset(int ind)
        {
            return 0xA43B + ind * 3 / 8;
        }

        public static int Shift(int ind)
        {
            return ind * 3 % 8;
        }
    }
    public static class SkillExp
    {
        public static int Ofset(int ind, int skill)
        {
            return 0xC9BB + 0x500 * skill + (ind * 8) / 8;
        }

        public static int Shift(int ind)
        {
            return (ind * 8) % 8;
        }
    }
    public static class SkillLevel
    {
        public static int Ofset(int ind, int skill)
        {
            return 0xAD9B + 0x1E0 * skill + (ind * 3) / 8;
        }

        public static int Shift(int ind)
        {
            return (ind * 3) % 8;
        }
    }
    public static class Mega
    {
        public static int Ofset(int ind)
        {
            return 0x406 + (ind + 2) / 4;
        }

        public static int Shift(int ind)
        {
            return (5 + (ind << 1)) % 8;
        }
    }
    public static class SpeedUpX
    {
        public static int Ofset(int ind)
        {
            return 0x2D5B + (db.MegaList.IndexOf(ind) * 7 + 3) / 8;
        }

        public static int Shift(int ind)
        {
            return (db.MegaList.IndexOf(ind) * 7 + 3) % 8;
        }
    }
    public static class SpeedUpY
    {
        public static int Ofset(int ind)
        {
            return 0x2D5B + (db.MegaList.IndexOf(ind, db.MegaList.IndexOf(ind) + 1) * 7 + 3) / 8;
        }

        public static int Shift(int ind)
        {
            return (db.MegaList.IndexOf(ind, db.MegaList.IndexOf(ind) + 1) * 7 + 3) % 8;
        }
    }
    public static class TeamData
    {
        public static int Ofset(int slot)
        {
            return 0xE0 + new int[] { 0, 0x2, 0x3, 0x5 }[slot];
        }

        public static int Shift(int slot)
        {
            return new int[] { 5, 1 }[(slot) % 2];
        }
    }

    public static class Completed
    {
        public static int Ofset(int ind, int type)
        {
            switch (type)
            {
                case 0: //Main
                    return 0x688 + ind * 3 / 8;

                case 1: //Expert
                    return 0x84A + ind * 3 / 8;

                case 2: //Event
                    return 0x8BA + (4 + ind * 3) / 8;

                default:
                    throw new System.ArgumentException("Invalid type parameter", "type");
            }
        }

        public static int Shift(int ind, int type)
        {
            switch (type)
            {
                case 0: //Main
                    return (ind * 3) % 8;

                case 1: //Expert
                    return (ind * 3) % 8;

                case 2: //Event
                    return (4 + ind * 3) % 8;

                default:
                    throw new System.ArgumentException("Invalid type parameter", "type");
            }
        }
    }
    public static class Rank
    {
        public static int Ofset(int ind, int type)
        {
            switch (type)
            {
                case 0: //Main
                    return 0x987 + (7 + ind * 2) / 8;

                case 1: //Expert
                    return 0xAB3 + (7 + ind * 2) / 8;

                case 2: //Event
                    return 0xAFE + (7 + ind * 2) / 8;

                default:
                    throw new System.ArgumentException("Invalid type parameter", "type");
            }
        }

        public static int Shift(int ind, int type)
        {
            switch (type)
            {
                case 0: //Main
                    return (7 + ind * 2) % 8;

                case 1: //Expert
                    return (7 + ind * 2) % 8;

                case 2: //Event
                    return (7 + ind * 2) % 8;

                default:
                    throw new System.ArgumentException("Invalid type parameter", "type");
            }
        }
    }
    public static class Score
    {
        public static int Ofset(int ind, int type)
        {
            switch (type)
            {
                case 0: //Main
                    return 0x4141 + 3 * ind;

                case 1: //Expert
                    return 0x4F51 + 3 * ind;

                case 2: //Event
                    return 0x52D5 + 3 * ind;

                default:
                    throw new System.ArgumentException("Invalid type parameter", "type");
            }
        }

        public static int Shift(int ind, int type)
        {
            switch (type)
            {
                case 0: //Main
                    return 4;

                case 1: //Expert
                    return 4;

                case 2: //Event
                    return 4;

                default:
                    throw new System.ArgumentException("Invalid type parameter", "type");
            }
        }
    }
    public static class EscalationStep
    {
        public static int Ofset()
        {
            return 0x2D59;
        }

        public static int Shift()
        {
            return 2;
        }
    }

    public static class Coins
    {
        public static int Ofset()
        {
            return 0x68;
        }

        public static int Shift()
        {
            return 3;
        }
    }
    public static class Enhancements
    {
        public static int Ofset(int i)
        {
            return 0x2D4C + i;
        }

        public static int Shift()
        {
            return 1;
        }
    }
    public static class Hearts
    {   //these are stock (99) hearts only
        public static int Ofset()
        {
            return 0x2D4A;
        }

        public static int Shift()
        {
            return 7;
        }
    }
    public static class Items
    {
        public static int Ofset(int i)
        {
            return 0xD0 + i;
        }

        public static int Shift()
        {
            return 7;
        }
    }
    public static class Jewels
    {
        public static int Ofset()
        {
            return 0x68;
        }

        public static int Shift()
        {
            return 20;
        }
    }

    public static class StreetCount
    {
        public static int Ofset()
        {
            return 0x5967;
        }
    }
    public static class StreetTag
    {
        public static int Ofset(int i)
        {
            return 0x59A7 + (i * Length());
        }
        public static int Length()
        {
            return 0x68;
        }
    }
    public static class MissionCards
    {
        public static int Ofset(int card, int mission = 0)
        {
            return 0xB6FC + (card * 10 + mission) / 8;
        }
        public static int Shift(int card, int mission = 0)
        {
            return (card * 10 + mission) % 8;
        }
    }
    public static class PokathlonOpponent
    {
        public static int Ofset()
        {
            return 0xB762;
        }

        public static int Shift()
        {
            return 6;
        }
    }
    public static class PokathlonMoves
    {
        public static int Ofset()
        {
            return 0xB768;
        }

        public static int Shift()
        {
            return 0;
        }
    }
    public static class PokathlonStep
    {
        public static int Ofset()
        {
            return 0xB760;
        }

        public static int Shift()
        {
            return 0;
        }
    }
    #endregion Shifts&Ofsets

    #region Custom Objects

    public class cbItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class monItem
    {
        public bool Caught { get; set; }
        public int Exp { get; set; }
        public int Level { get; set; }
        public int Lollipops { get; set; }
        public int CurrentSkill { get; set; }
        public int[] SkillExp { get; set; }
        public int[] SkillLevel { get; set; }
        public int SpeedUpX { get; set; }
        public int SpeedUpY { get; set; }
        public int Stone { get; set; }
    }

    public class rsItem
    {
        public int Coins { get; set; }
        public int[] Enhancements { get; set; }
        public int Hearts { get; set; }
        public int[] Items { get; set; }
        public int Jewels { get; set; }
    }

    public class stgItem
    {
        public bool Completed { get; set; }
        public int Rank { get; set; }
        public int Score { get; set; }
    }

    public class smItem
    {
        public int Opponent { get; set; }
        public int Step { get; set; }
        public int Moves { get; set; }
    }
    #endregion Custom Objects
}