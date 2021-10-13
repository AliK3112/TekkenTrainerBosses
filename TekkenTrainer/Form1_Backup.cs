using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Memory;

namespace TekkenTrainer
{
    public partial class Form1 : Form
    {
        // Moveset Structure Offsets
        enum OFFSETS
        {
            reaction_list = 0x150,
            requirements = 0x160,
            hit_condition = 0x170,
            projectile = 0x180,
            pushback = 0x190,
            pushback_extra = 0x1A0,
            cancel_list = 0x1B0,
            group_cancel_list = 0x1C0,
            cancel_extra = 0x1D0,
            extraprops = 0x1E0,
            moves = 0x210,
            voice_clip = 0x220
        };

        // Move Attribute Offsets
        enum offsets
        {
            name = 0x0,
            anim_name = 0x8,
            anim_addr = 0x10,
            vuln = 0x18,
            hitlevel = 0x1c,
            cancel_addr = 0x20,
            transition = 0x54,
            anim_len = 0x68,
            startup = 0xA0,
            recovery = 0xA4,
            hitbox = 0x9C,
            hit_cond_addr = 0x60,
            ext_prop_addr = 0x80,
            voiceclip_addr = 0x78
        };

        readonly Mem mem = new Mem();
        // GAME VERSION: v4.11

        // Structure Addresses
        public static string p1struct = "TekkenGame-Win64-Shipping.exe+0x34DA0B0"; // Value: 1434E5250
        public static string p1profileStruct = "TekkenGame-Win64-Shipping.exe+0x034D0510,0x0,0x0";
        public static string visuals = "TekkenGame-Win64-Shipping.exe+0x03797020,0x58,0x388,0x68,0x8,0x0,0x470";

        // Offsets
        /*
        reactions = 0x150
        requirements = 0x160
        hit condition = 0x170
        pushback extradata = 0x1A0
        cancels = 0x1B0
        group cancels = 0x1C0
        cancel extradata = 0x1D0
        extra properties = 0x1E0
        moves = 0x210
        voiceclips = 0x220
         */


        // Costume Related stuff
        const string cs_kaz_final = "/Game/Demo/StoryMode/Character/Sets/CS_KAZ_final.CS_KAZ_final";
        const string cs_hei_final = "/Game/Demo/StoryMode/Character/Sets/CS_HEI_final.CS_HEI_final";
        const string cs_mrx_final = "/Game/Demo/StoryMode/Character/Sets/CS_MRX_final.CS_MRX_final";
        static ulong MOVESET; // This holds current moveset whether it is P1 or P2
        //static string stageAddress = "TekkenGame-Win64-Shipping.exe+0x034CB390,0x0,0x0,0x18";

        // Variable to keep track of which character is being written currently
        bool IsWritten = false;
        //bool IsWritten2 = false;

        public Form1()
        {
            InitializeComponent();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// MAIN FORM
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Form1_Load(object sender, EventArgs e)
        {
            //int count = 0;
            int PID = mem.GetProcIdFromName("TekkenGame-Win64-Shipping.exe");
            if (PID > 0)
            {
                //TextBox("Program attached to TEKKEN 7\n");
                mem.OpenProcess(PID);
                Thread_moveset();
                BossThread();
            }
            else if (PID < 0)
            {
                MessageBox.Show("Failed to attach to process\nApplication Quitting.");
                CloseProgram();
            }
            else if (PID == 0)
            {
                //Thread_AttachToGame();
                //while (true)
                //{
                //    PID = mem.GetProcIdFromName("TekkenGame-Win64-Shipping.exe");
                //    Thread.Sleep(100);
                //}
                MessageBox.Show("TEKKEN 7 not running. Run this program after running the game\nApplication Quitting.");
                //if (count == 0) textBox1.AppendText("Waiting to attach to TEKKEN 7");
                CloseProgram();
            }
            
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// MAIN BUTTONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // This is the button for Kazuya
        private void Button_kazuya_Click(object sender, EventArgs e)
        {
            IsWritten = false;
            //IsWritten2 = false;
            if (checkBox1.Checked)
            {
                checkBox1.Checked = false;
                return;
            }
            Checkboxes_checks(false);
            checkBox1.Checked = true;
        }

        // This is the button for Heihachi
        private void Button_heihachi_Click(object sender, EventArgs e)
        {
            IsWritten = false;
            //IsWritten2 = false;
            if (checkBox2.Checked)
            {
                checkBox2.Checked = false;
                return;
            }
            Checkboxes_checks(false);
            checkBox2.Checked = true;
        }

        // This is the button for Akuma
        private void Button_akuma_Click(object sender, EventArgs e)
        {
            IsWritten = false;
            //IsWritten2 = false;
            if (checkBox3.Checked)
            {
                checkBox3.Checked = false;
                return;
            }
            Checkboxes_checks(false);
            checkBox3.Checked = true;
        }

        // This button is for Devil Kazumi
        private void Button_kazumi_Click(object sender, EventArgs e)
        {
            IsWritten = false;
            //IsWritten2 = false;
            if (checkBox4.Checked)
            {
                checkBox4.Checked = false;
                return;
            }
            Checkboxes_checks(false);
            checkBox4.Checked = true;
        }

        // This button is for Asura Jin
        private void Button_Jin_Click(object sender, EventArgs e)
        {
            IsWritten = false;
            //IsWritten2 = false;
            if (checkBox5.Checked)
            {
                checkBox5.Checked = false;
                Checkboxes_checks(false);
                return;
            }
            Checkboxes_checks(false);
            checkBox5.Checked = true;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// MAIN THREADS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
       private void BossThread()
        {
            Thread BOSSES = new Thread(BossThreadLoop) { IsBackground = true };
            BOSSES.Start();
        }
        
        private void Thread_moveset()
        {
            Thread abc = new Thread(Moveset_Pointer_Read) { IsBackground = true };
            abc.Start();
        }

        private void Thread_Outfits()
        {
            Thread abc = new Thread(Costumes) { IsBackground = true };
            abc.Start();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// THREADS FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
       private void Costumes()
        {

            while (true)
            {
                uint charID = GetCharID();
                if (checkBox2.Checked && charID == 8) Costume(cs_hei_final);
                else if (checkBox1.Checked && charID == 9) Costume(cs_kaz_final);
                else if (checkBox3.Checked && charID == 32) Costume(cs_mrx_final);
                else if (checkBox4.Checked) LoadCharacter(27);  // Load Devil Kazumi
                else if (charID == 255) break;  // An error occured
                Thread.Sleep(100);
            }

        }

        private void BossThreadLoop()
        {
            // Address for costume instruction (v4.11): "TekkenGame-Win64-Shipping.exe" + 4EF6540

            string address = "TekkenGame-Win64-Shipping.exe+4EF6534";
            byte[] instructions = new byte[]
            {
                0x66, 0x66, 0x66, 0x2E, 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, // nop word ptr cs:[rax+rax+00000000]
                0xBA, 0x04, // mov edx,04
                0x89, 0x51, 0x04, // mov [rcx+04],edx
                0x89, 0x91, 0xB4, 0x01, 0x00, 0x00, // mov [rcx+000001B4],edx
                0xE9, 0xE9, 0xB0, 0x64, 0xF0, 0x04, // jmp "TekkenGame-Win64-Shipping.exe"+4EF6549
            };
            UIntPtr codeCaveBase;
            try
            {
                codeCaveBase = mem.CreateCodeCave(address, instructions, 12);
            }
            catch(Exception e)
            {
                if ((uint)e.HResult == 0x80131516)
                {
                    Debug.WriteLine("Arithematic Overflow happened in creating a code cave");
                }
            }
            
            //Debug.WriteLine("Code Cave Allocated at: 0x" + codeCaveBase.ToString());
            uint charID;
            while(true)
            {
                Thread.Sleep(10);

                if (!MovesetExists(MOVESET))
                {
                    IsWritten = false;
                    continue;
                }

                charID = GetCharID();
                if (checkBox1.Checked)
                {
                    if (charID == 9 && !IsWritten) DVKCancelRequirements();
                }
                else if (checkBox2.Checked)
                {
                    if (charID == 8 && !IsWritten) ASHCancelRequirements();
                }
                else if (checkBox3.Checked)
                {
                    if (charID == 32 && !IsWritten) SHACancelRequirements();
                }
                else if (checkBox4.Checked)
                {
                    if (charID == 27 && !IsWritten) BS7CancelRequirements();
                }
                else if (checkBox5.Checked)
                {
                    if (charID == 6 && !IsWritten) JINCancelRequirements();
                }
            }
        }

        // This function continuously reads the moveset pointer
        private void Moveset_Pointer_Read()
        {
            string moveset_addr;
            //string moveset = "TekkenGame-Win64-Shipping.exe+0x34E6720";
            while(true)
            {
                if (mem.ReadInt(visuals + ",0x14") == 0) // If playing at P1 side
                    moveset_addr = "TekkenGame-Win64-Shipping.exe+0x34E6720";
                else  // For P2 side
                    moveset_addr = "TekkenGame-Win64-Shipping.exe+0x34E9D40";
                ReadMoveset(moveset_addr);
                Thread.Sleep(100);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// REMAINING FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Costume(string costume)
        {
            if (mem.ReadInt(visuals + ",0x14") == 0) // If playing at P1 side
            {
                mem.WriteMemory(visuals + ",0xCC", "int", "4");
                mem.WriteMemory(visuals + ",0x27C", "int", "4");
                mem.WriteMemory(visuals + ",0x428", "string", costume);
            }
            else if (mem.ReadInt(visuals + ",0x14") == 1) // Else if playing at P2 side
            {
                mem.WriteMemory(visuals + ",0x52C", "int", "4");
                mem.WriteMemory(visuals + ",0x6DC", "int", "4");
                mem.WriteMemory(visuals + ",0x888", "string", costume);
            }
        }

        private void DVKCancelRequirements()
        {
            if (IsWritten)
                return;

            const int Co_Dummy_00 = 838;
            const int Co_Dummy_00_cancel_idx = 4235;
            long[,] arr = new long[,]
            {
                {48, 2},    // Juggle escape
                {1022, 1},  // Activate Special Chapter Flag (1)
                {1070, 2},  // Activate Special Chapter Flag (2)
                //{1668, 1},  // Special chapter flag
                {2381, 4},  // Boss Kazuya stance
                {2386, 0},  // AI transformation revert
                {2981, 3},  // 1,1,2 (1)
                {2990, 3},  // 1,1,2 (2)
                {2999, 3},  // 1,1,2 (3)
                {3008, 3},  // 1,1,2 (4)
                //{3204, 4},  // Blue Demon God Fist
                {3232, 4},  // f+1+2 reaction
                //{3642, 1},  // Ground Laser
                //{4024, 2},  // Double Spinning Demon
                {4255, 3},  // Regular Rage Art
                {4412, 1},  // Ultimate Rage Art
                {4523, 3},  // Intro
                {4528, 3},  // Outro (1)
                {4536, 3},  // Outro (2)
            };
            if (!RemoveRequirements(MOVESET, arr, arr.GetLength(0), 9))
                return;

            // Stopping Story mode version of Rage Art from coming out (cancel list: 10177)
            //ulong addr = (ulong)mem.ReadLong(string.Format("{0:X}", MOVESET + 432)); // 0x1B0
            //addr = addr + (40 * 10177) + 8;
            //ulong n_addr = (ulong)mem.ReadLong(string.Format("{0:X}", addr));
            //mem.WriteMemory(string.Format("{0:X}", addr+40), "long", n_addr.ToString());

            // Writing into group cancel for Ultimate Rage Art
            arr = new long[,]
            {
                {1720, Co_Dummy_00}, // 1566 + 154
            };
            if (!EditGroupCancels(MOVESET, arr, arr.GetLength(0)))
                return;

            int[] arr1 = new int[]
            {
                2103, // To, From is fixed to Co_Dummy_00 (838)
                1658,
                1600
            };
            // Copying move "RageArt00" (2103) to "Co_Dummy_00" (838)
            if (!CopyMoves(MOVESET, arr1, arr1.Length, Co_Dummy_00))
                return;

            int ind1 = Co_Dummy_00_cancel_idx; // Cancel list index for Co_Dummy_00
            arr = new long[,]
            {
		        // For Ultimate Rage Art
		        {ind1++, 0, 0, 11, 1, 1, 1, 2208, 65},
                {ind1++, 32768, 0, 0, 0, 0, 0, 32769, 336},
		        // For d/f+2,1 cancel
		        {ind1++, 0, 3559, 52, 23, 23, 23, 1600, 65},
                {ind1++, 0, 3516, 23, 1, 32767, 1, 32769, 257},
                {ind1++, 0, 3410, 23, 1, 32767, 1, 32769, 257},
                {ind1++, 32768, 0, 0, 46, 32767, 46, 32769, 336},
		        // For f+1+2,2 cancel
		        {ind1++, 0, 1882, 23, 1, 32767, 1, 32769, 257},
                {ind1++, 0, 3191, 11, 1, 32767, 1, 1601, 65},
                {ind1++, 0, 0, 16, 32, 32, 32, 1655, 65},
                {ind1++, 32768, 0, 0, 58, 32767, 58, 32769, 336},
		        // For f+1+2,2 cancel (blending)
		        {7521, 0x4000000200000000, 0, 11, 1, 24, 24, Co_Dummy_00 + 2, 80},
		        // For d/f+2,1 cancel (blending)
		        {7808, 0x4000000100000000, 0, 11, 1, 13, 13, Co_Dummy_00 + 1, 80},
                // For Extended Spinning Demon to Heaven's Door (cancel list: 8358, entry 3)
                {8360, -1, 4044, -1, -1, -1, -1, -1, -1},
		        // For Stopping Story Rage Art from Coming out (cancel list: 10177, entry 2)
		        {10178, -1, 3624, -1, -1, -1, -1, -1, -1}
            };

            // Updating cancel lists
            if (!Edit_Cancels(MOVESET, arr, arr.GetLength(0)))
                return;

            // Adjusting cancel lists
            arr = new long[,]
            {
                {Co_Dummy_00+1, Co_Dummy_00_cancel_idx + 2} // Co_Dummy_02 (839), Index number to be assigned
            };
            if (!AssignCancelListIndexes(MOVESET, arr, arr.GetLength(0)))
                return;

            // Adjusting hit condition
            ulong addr = (ulong)mem.ReadLong(string.Format("{0:X}", MOVESET + 0x210)); // 0x210;
            addr += (176 * 1600); // Kz_vipLP (1600)
            ulong n_addr = (ulong)mem.ReadLong(string.Format("{0:X}", MOVESET + 0x170)) + (24 * 350);
            mem.WriteMemory(string.Format("{0:X}", addr + 96), "long", n_addr.ToString());
            IsWritten = true; // Memory has been successfully editied
        }

        private void ASHCancelRequirements()    // Ascended Heihachi requirements
        {
            if (IsWritten)
                return;

            const int Co_Dummy_00 = 848;
            const int Co_Dummy_00_cancel_idx = 4242;

            // Editing requirements
            long[,] arr = new long[,]
            {
                {2730, 4}, // Most boss Heihachi moves
                {3435, 3}, // Battering Ram
                {3663, 3}, // WFG (1)
                {3668, 3}, // WFG (2)
                {3675, 3}, // WFG (3)
                {3682, 3}, // WFG (4)
                {3722, 3}, // WFG Hit reaction
                {4713, 3}, // Boss intro
                {4718, 3}, // Boss outro
            };
            if (!RemoveRequirements(MOVESET, arr, arr.GetLength(0), 8))
                return;

            // Writing into group cancels
            arr = new long[,]
            {
                {899, 2247}, // ID of He_WK00F_7CS
                {1674, Co_Dummy_00}, //(1541 + 133)
            };
            if (!EditGroupCancels(MOVESET, arr, arr.GetLength(0)))
                return;

            // This array is for copying moves
            int[] arr1 = new int[]
            {
                2168 // He_RageArt00
            };

            if (!CopyMoves(MOVESET, arr1, arr1.GetLength(0), 848))
                return;

            int ind1 = Co_Dummy_00_cancel_idx;
            int ind2 = 8418; // Cancel list of Spinning Demon kick 3 (boss version), idx 0
            // Updating cancel lists
            // {index, command, req_idx, ext_idx, w_start, w_end, starting_frame, move, option}
            arr = new long[,]
            {
                // For Ultimate Rage Art
		        {ind1++, 0, 0, 11, 7, 7, 7, 2176, 65},
                {ind1++, 32768, 0, 0, 0, 0, 0, 32769, 336},
		        // For Spinning Demon (kick 1)
		        {ind1++, 0x4000000100000000, 0, 11, 1, 15, 15, 1717, 80},
                {ind1++, 0x400000080000004E, 0, 16, 1, 16, 16, 1711, 80},
                {ind1++, 0x4000000800000000, 0, 11, 1, 15, 15, 1719, 80},
                {ind1++, 0x8000, 0, 0, 49, 32767, 49, 32769, 336},
		        // For Spinning Demon (kick 2)
		        {ind1++, 0x400000080000004E, 0, 16, 1, 24, 24, 1712, 80},
                {ind1++, 0x4000000100000000, 0, 11, 1, 16, 16, 1724, 80},
                {ind1++, 0x4000000800000020, 0, 11, 1, 23, 23, 1725, 80},
                {ind1++, 0x8000, 0, 0, 59, 32767, 59, 32769, 336},
		        // For Spinning Demon (kick 3)
		        {ind2 + 0, 0x4000000100000000, 0, -1, 1, -1, -1, -1, 80},
                {ind2 + 1, 0x4000000800000020, 0, -1, 1, -1, -1, -1, 80},
                {ind2 + 2, 0x400000080000004e, 0, -1, 1, -1, -1, -1, 80},
		        // For Spinning Demon (kick 4)
		        {ind2 + 4, 0x4000000100000000, 0, -1, 1, -1, -1, -1, 80},
                {ind2 + 5, 0x4000000800000020, 0, -1, 1, -1, -1, -1, 80},
                {ind2 + 6, 0x400000080000004e, 0, -1, 1, -1, -1, -1, 80},
		        // For Spinning Demon (kick 5)
		        {ind2 + 8, 0x4000000100000000, 0, -1, 1, -1, -1, -1, 80},
                {ind2 + 9, 0x4000000800000020, 0, -1, 1, -1, -1, -1, 80},
                {ind2 +10, 0x400000080000004e, 0, -1, 1, -1, -1, -1, 80},
		        // For Spinning Demon (kick 6)
		        {ind2 +12, 0x4000000100000000, 0, -1, 1, -1, -1, -1, 80},
                {ind2 +13, 0x4000000800000020, 0, -1, 1, -1, -1, -1, 80},
		        // From Regular Spinning Demon to boss version
		        {ind2 +15, -1, 0, -1, -1, -1, -1, -1, -1}
            };
            if (!Edit_Cancels(MOVESET, arr, arr.GetLength(0)))
                return;

            arr = new long[,]
            {
                {1710, Co_Dummy_00_cancel_idx + 2}, // For Spinning Demon Kick 1, 4244
		        {1711, Co_Dummy_00_cancel_idx + 6}  // For Spinning Demon Kick 2, 4248
            };
            if (!AssignCancelListIndexes(MOVESET, arr, arr.GetLength(0)))
                return;

            IsWritten = true; // Memory has been successfully modified
        }

        private void SHACancelRequirements() // For Shin Akuma
        {
            if (IsWritten) // This checks if the moveset is already edited or not
                return;
            
            // For removing requirements from cancels
            // {RequirementIndex, how many requirements to zero}
            long[,] arr = new long[,]
            {
                {1707, 3}, // Parry (1)
                {1713, 3}, // Parry (2)
                {4262, 3}, // Focus Attack (1)
                {4267, 3}, // Focus Attack (2)
                {4547, 3}, // Triple Fireballs (1)
                {4564, 3}, // Triple Fireballs (2)
                {4570, 3}, // Triple Fireballs (3)
                {4575, 3}, // Triple Fireballs (4)
                {5452, 3}, // Parry followup
                {5475, 3}, // Parry (3)
                {5482, 3}, // Parry (4)
                {5660, 3}, // Intro
                {5665, 3}, // Outro
            };
            if(!RemoveRequirements(MOVESET, arr, arr.GetLength(0), 32))
                return;

            // For extra move properties
            // {MoveID, Extraprop index value to be assigned to it}
            arr = new long[,]
            {
                {1937, 8775},
                {1938, 8796},
                {1941, 8833},
            };
            if (!Extraprops(MOVESET, arr, arr.GetLength(0)))
                return;

            arr = new long[,]
            {
                {11373, -1, 0, -1, -1, -1, -1, -1, -1}, // Cancel to Rage Art finish L -> Treasure RA
                {11389, -1, 0, -1, -1, -1, -1, -1, -1}, // Cancel to Rage Art finish R -> Treasure RA
            };
            
            // Updating cancel lists
            if (!Edit_Cancels(MOVESET, arr, arr.GetLength(0)))
                return;

            // Writing into group cancels
            arr = new long[,]
            {
                {768, 1999},  // 763+5 - for d+3+4 meter charge
                {588, 1999},  // 583+5 - for d+3+4 meter charge
            };
            if (!EditGroupCancels(MOVESET, arr, arr.GetLength(0)))
                return;

            IsWritten = true;  // This means the moveset has been modified successfully
        }

        private void JINCancelRequirements()    // Asura Jin requirements
        {
            if (IsWritten)
                return;

            const int Co_Dummy_00 = 841;
            const int Co_Dummy_00_list_idx = 4210;

            // Editing requirements
            long[,] arr = new long[,]
            {
                {1100, 3}, // Zen into ETU
		        {2230, 3}, // D+1+2 Slide (1)
	        	{2236, 3}, // D+1+2 Slide (2)
		        {2252, 3}, // Slide Player forward (1)
		        {2258, 3}, // Slide Player forward (2)
		        {2279, 3}, // Slide Player forward during ULLRK
		        {2306, 3}, // Slide Player forward during UEWHF
		        {2342, 3}  // Slide Player forward during UETU
            };
            if (!RemoveRequirements(MOVESET, arr, arr.GetLength(0), 6))
                return;

            // Writing into group cancels
            //arr = new long[,]
            //{
            //    {1546, 838} // 1566 + 154
            //};
            //if (!EditGroupCancels(MOVESET, arr, arr.GetLength(0)))
            //    return;

            // This array is for copying moves
            int[] arr1 = new int[]
            {
                1583, // For b+1 into d/f+1
		        1583, // For b+1 into d/f+2
		        1583, // For b+1 into d/f+4
		        1579, // For d+1+2 into 1+2
		        1579, // For d+1+2 into 3+4
		        1579, // For d+1+2 into 2
		        1579, // For d+1+2 into 4
		        1579, // For d+1+2 into 3
		        1629, // For Standing 4 into UEWHF
		        1835  // For WHF > UEWHF
	        };

            if (!CopyMoves(MOVESET, arr1, arr1.GetLength(0), Co_Dummy_00))
                return;

            int ind1 = Co_Dummy_00_list_idx; // Cancel list index of Co_Dummy_00
            int ind2 = 7166; // Cancel list index of d+1+2, idx 3
            int ind3 = 7188; // Cancel list index of boss b+1, idx 3
            int ind4 = 7607; // Cancel list index of 1,3 / d/f+3,3 ZEN cancel, idx 1
            int ind5 = 7801; // Cancel list index of b,f+2,3 ZEN cancel, idx 1
            int ind6 = 7857; // Cancel list index of ws+1,2 ZEN cancel, idx 1
            int ind7 = 7926; // Cancel list index of b+3 ZEN cancel, idx 1
            int ind8 = 7957; // Cancel list index of f+4 ZEN cancel, idx 1
            // Updating cancel lists
            // {index, command, req_idx, ext_idx, w_start, w_end, starting_frame, move, option}
            arr = new long[,]
            {
                // For b+1 into d/f+1
		        {ind1++, 0, 0, 20, 30, 30, 30, 1586, 65},
                {ind1++, 0x8000, 0, 0, 55, 32767, 55, 32769, 336},
		        // For b+1 into d/f+2
		        {ind1++, 0, 0, 20, 30, 30, 30, 1585, 65},
                {ind1++, 0x8000, 0, 0, 55, 32767, 55, 32769, 336},
		        // For b+1 into d/f+4
		        {ind1++, 0, 0, 20, 30, 30, 30, 1584, 65},
                {ind1++, 0x8000, 0, 0, 55, 32767, 55, 32769, 336},
		        // For d+1+2 into 1+2
		        {ind1++, 0, 0, 13, 25, 25, 25, 1580, 65},
                {ind1++, 0x8000, 0, 0, 45, 32767, 45, 32769, 336},
		        // For d+1+2 into 3+4
		        {ind1++, 0, 0, 13, 25, 25, 25, 1798, 65},
                {ind1++, 0x8000, 0, 0, 45, 32767, 45, 32769, 336},
		        // For d+1+2 into 2
		        {ind1++, 0, 0, 13, 25, 25, 25, 1597, 65},
                {ind1++, 0x8000, 0, 0, 45, 32767, 45, 32769, 336},
		        // For d+1+2 into 4
		        {ind1++, 0, 0, 13, 25, 25, 25, 1592, 65},
                {ind1++, 0x8000, 0, 0, 45, 32767, 45, 32769, 336},
		        // For d+1+2 into 3
		        {ind1++, 0, 0, 13, 25, 25, 25, 1797, 65},
                {ind1++, 0x8000, 0, 0, 45, 32767, 45, 32769, 336},
		        // For standing 4 into UEWHF
		        {ind1++, 0, 0, 15, 40, 40, 40, 1585, 65},
                {ind1++, 0x8000, 0, 0, 40, 32767, 40, 32769, 336},
		        // For WHF into UEWHF
		        {ind1++, 0, 0, 15, 36, 36, 36, 1585, 65},
                {ind1++, 0x8000, 0, 0, 38, 32767, 38, 32769, 336},
		        // d+1+2 (boss version) cancel list
		        {ind2++, 0x4000000300000000, 0, 10, 16, 24, 24, Co_Dummy_00 + 3, 80},
                {ind2++, 0x4000000C00000000, 0, 10, 16, 24, 24, Co_Dummy_00 + 4, 80},
                {ind2++, 0x4000000200000000, 0, 10, 16, 24, 24, Co_Dummy_00 + 5, 80},
                {ind2++, 0x4000000800000000, 0, 10, 16, 24, 24, Co_Dummy_00 + 6, 80},
                {ind2++, 0x4000000400000000, 0, 10, 16, 24, 24, Co_Dummy_00 + 7, 80},
		        // b+1 Cancel list
		        {7176, 0, 0, 10, 1, 1, 1, 1583, 65},
		        // b+1 (boss version) cancel list
		        {ind3++, 0x4000000100000000, 0, 10, 16, 29, 29, Co_Dummy_00 + 0, 80},
                {ind3++, 0x4000000200000000, 0, 10, 16, 29, 29, Co_Dummy_00 + 1, 80},
                {ind3++, 0x4000000800000000, 0, 10, 16, 29, 29, Co_Dummy_00 + 2, 80},
		        // For UEWHF into UEWHF
		        {7203, 0x4000000200000040, 0, -1, 24, -1, -1, -1, 80},
		        // Standing 4 cancel list
		        {7338, 0x4000000200000040, 0, 10, 1, 39, 39, Co_Dummy_00 + 8, 80},
		        // For d/f+4,4 / 1,3 cancel list
		        {ind4++, 0x4000000100000008, 0, -1, 1, -1, -1, -1, 80},
                {ind4++, 0x4000000200000008, 0, -1, 1, -1, -1, -1, 80},
                {ind4++, 0x4000000800000008, 0, -1, 1, -1, -1, -1, 80},
		        // For b,f+2,3 cancel list
		        {ind5++, 0x4000000100000008, 0, -1, 1, -1, -1, -1, 80},
                {ind5++, 0x4000000200000008, 0, -1, 1, -1, -1, -1, 80},
                {ind5++, 0x4000000800000008, 0, -1, 1, -1, -1, -1, 80},
		        // For ws+1,2 cancel list
		        {ind6++, 0x4000000100000008, 0, -1, 1, -1, -1, -1, 80},
                {ind6++, 0x4000000200000008, 0, -1, 1, -1, -1, -1, 80},
                {ind6++, 0x4000000800000008, 0, -1, 1, -1, -1, -1, 80},
		        // For b+3 cancel list
		        {ind7++, 0x4000000100000008, 0, -1, 1, -1, -1, -1, 80},
                {ind7++, 0x4000000200000008, 0, -1, 1, -1, -1, -1, 80},
                {ind7++, 0x4000000800000008, 0, -1, 1, -1, -1, -1, 80},
		        // For f+4 cancel list
		        {ind8++, 0x4000000100000008, 0, -1, 1, -1, -1, -1, 80},
                {ind8++, 0x4000000200000008, 0, -1, 1, -1, -1, -1, 80},
                {ind8++, 0x4000000800000008, 0, -1, 1, -1, -1, -1, 80},
		        // For WHF into Dummy Copy
		        {8115, 0x4000000200000040, 0, 10, 1, 35, 35, Co_Dummy_00 + 9, 80},
		        // For EWHF into UEWHF
		        {8126, 0x4000000200000040, 0, -1, 1, -1, -1, -1, 80}
            };
            if (!Edit_Cancels(MOVESET, arr, arr.GetLength(0)))
                return;

            ind1 = Co_Dummy_00;  // ID of Co_Dummy_00 move
            ind2 = Co_Dummy_00_list_idx; // Cancel Index
            arr = new long[,]
            {
                {ind1++, ind2 + 0}, // {Co_Dummy_00 (841), 4210}
		        {ind1++, ind2 + 2}, // {Co_Dymmy_02 (842), 4212}
		        {ind1++, ind2 + 4}, // {Co_Dymmy_03 (843), 4214}
		        {ind1++, ind2 + 6}, // {Co_Dymmy_05 (844), 4216}
		        {ind1++, ind2 + 8}, // {Co_Dymmy_06 (845), 4218}
		        {ind1++, ind2 +10}, // {Co_Dymmy_07 (846), 4219}
		        {ind1++, ind2 +12}, // {Co_Dymmy_08 (847), 4220}
		        {ind1++, ind2 +14}, // {Co_Dymmy_09 (848), 4222}
		        {ind1++, ind2 +16}, // {Co_Dymmy_10 (849), 4224}
		        {ind1++, ind2 +18}  // {Co_Dymmy_11 (850), 4226}
            };
            if (!AssignCancelListIndexes(MOVESET, arr, arr.GetLength(0)))
                return;

            IsWritten = true; // Memory has been successfully modified
        }

        private void BS7CancelRequirements() // For Devil Kazumi
        {
            if (IsWritten) // This checks if the moveset is already edited or not
                return;

            // For removing requirements from cancels
            // {RequirementIndex, how many requirements to zero}
            long[,] arr = new long[,]
            {
                {24, 3},   // Juggle escape
                {2043, 3}, // 1,1,2
                {2532, 3}, // Intro
                {2537, 3}, // Outro
            };
            if (!RemoveRequirements(MOVESET, arr, arr.GetLength(0), 27))
                return;

            IsWritten = true;  // This means the moveset has been modified successfully
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// UTILITY FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // This function reads moveset address
        private bool ReadMoveset(string addr)
        {
            MOVESET = (ulong)mem.ReadLong(addr);
            if (MOVESET == 0)
                return false;
            return true;
        }
        // This function loads given character
        private void LoadCharacter(int ID)
        {
            if (mem.ReadInt(visuals + ",0x14") == 0) // If playing at P1 side
            {
                if (MovesetExists(MOVESET))
                    return;
                mem.WriteMemory(visuals + ",0x1C", "int", ID.ToString());
            }
            else
            {
                if (!MovesetExists(MOVESET))
                    return;
                mem.WriteMemory(visuals + ",0x20", "int", ID.ToString());
            }
            mem.WriteMemory(p1profileStruct + ",0x10", "int", ID.ToString());
        }
        // This function checks if the moveset exists or not
        private bool MovesetExists(ulong addr)
        {
            if (mem.ReadInt(string.Format("{0:X}", addr)) == 65536)
                return true;
            return false;
        }        
        // This function checks/unchecks the check boxes
        private void Checkboxes_checks(bool value)
        {
            checkBox1.Checked = value;
            checkBox2.Checked = value;
            checkBox3.Checked = value;
            checkBox4.Checked = value;
            checkBox5.Checked = value;
        }
        int GetMoveID(ulong moveset, string moveName)
        {
            ulong moves_addr = (ulong)mem.ReadLong(string.Format("{0:X}", moveset + 0x210));
            if (moves_addr == 0) return -1;
            ulong moves_size = (ulong)mem.ReadLong(string.Format("{0:X}", moveset + 0x218));
            if (moves_size == 0) return -1;
            ulong addr, moveNameAddr;
            string moveNameRead;
            for (int i = 0; i < (int)moves_size; i++)
            {
                addr = moves_addr + (ulong)(i * 176);
                moveNameAddr = (ulong)mem.ReadLong(string.Format("{0:X}", addr));
                if (moveNameAddr == 0) return -1;
                moveNameRead = mem.ReadString(string.Format("{0:X}", moveNameAddr));
                if (moveNameRead == string.Empty) return -1;
                if (moveName.CompareTo(moveNameRead) == 0) return i;
            }
            return -1;
        }
        int GetMoveAttributeIndex(ulong moveset, int moveID, int offset)
        {
            if (moveID < 0) return -1;
            ulong moves_addr = (ulong)mem.ReadLong(string.Format("{0:X}", moveset + 0x210));
            if (moves_addr == 0) return -1;
            ulong moves_size = (ulong)mem.ReadLong(string.Format("{0:X}", moveset + 0x218));
            if (moves_size == 0) return -1;
            ulong addr = moves_addr + (ulong)(moveID * 176);
            ulong attr_addr = (ulong)mem.ReadLong(string.Format("{0:X}", addr + (ulong)offset));
            int index;
            if (offset == (int)offsets.cancel_addr)
            {
                addr = (ulong)mem.ReadLong(string.Format("{0:X}", addr + (ulong)OFFSETS.cancel_list));
                index = (int)(attr_addr - addr) / 40;
            }
            else if (offset == (int)offsets.hit_cond_addr)
            {
                addr = (ulong)mem.ReadLong(string.Format("{0:X}", addr + (ulong)OFFSETS.hit_condition));
                index = (int)(attr_addr - addr) / 24;
            }
            else if (offset == (int)offsets.ext_prop_addr)
            {
                addr = (ulong)mem.ReadLong(string.Format("{0:X}", addr + (ulong)OFFSETS.extraprops));
                index = (int)(attr_addr - addr) / 12;
            }
            else index = -1;
            return index;
        }
        private bool RemoveRequirements(ulong moveset, long[,] arr, int rows, int charID)
        {
            ulong requirements_addr = (ulong)mem.ReadLong(string.Format("{0:X}", moveset + 352)); // 0x160
            if (requirements_addr == 0) return false; // Return in case of null
            ulong addr, n_addr;
            // Removing requirements from the given array
            for (int i = 0; i < rows; i++)
            {
                addr = requirements_addr + (ulong)(8 * arr[i, 0]);
                // Writing and replacing the code to make the HUD comeback and stop AI from reverting Devil Transformation
                if (arr[i, 0] == 2386 && charID == 9)
                {
                    mem.WriteMemory(string.Format("{0:X}", addr), "int", "563");
                    mem.WriteMemory(string.Format("{0:X}", addr + 16), "int", "33437");
                    mem.WriteMemory(string.Format("{0:X}", addr + 20), "int", "1");
                }
                // Handling the requirements to allow Akuma's parry
                else if (arr[i, 0] == 5452 && charID == 32)
                {
                    mem.WriteMemory(string.Format("{0:X}", addr + (4 * 8)), "int", "0");
                    mem.WriteMemory(string.Format("{0:X}", addr + (4 * 8) + 4), "int", "0");
                    mem.WriteMemory(string.Format("{0:X}", addr + (8 * 8)), "int", "0");
                    mem.WriteMemory(string.Format("{0:X}", addr + (8 * 8) + 4), "int", "0");
                }
                for (int j = 0; j < arr[i, 1]; j++)
                {
                    n_addr = addr + (ulong)(8 * j);
                    if (!mem.WriteMemory(string.Format("{0:X}", n_addr), "long", "0"))
                    {
                        Debug.WriteLine("Unable to write");
                        return false;
                    }
                }
            }
            return true;
        }
        private bool EditGroupCancels(ulong moveset, long[,] arr, int rows)
        {
            long grp_cancel = mem.ReadLong(string.Format("{0:X}", moveset + 448)); // 0x1C0
            if (grp_cancel == 0) return false; // Return in case of null
            long addr;
            for (int i = 0; i < rows; i++)
            {
                addr = grp_cancel + (40 * arr[i, 0]) + 36;
                if (!mem.WriteMemory(string.Format("{0:X}", addr), "2bytes", arr[i, 1].ToString()))
                {
                    Debug.WriteLine("Failed to write group cancels");
                    return false;
                }
            }
            return true;
        }
        private bool CopyMoves(ulong moveset, int[] arr, int rows, int Co_Dummy)
        {
            long moves_addr = mem.ReadLong(string.Format("{0:X}", moveset + 528)); // 0x210
            if (moves_addr == 0) return false;
            long FromMove, ToMove;
            for (int i = 0; i < rows; i++)
            {
                FromMove = moves_addr + (176 * arr[i]);
                ToMove = moves_addr + (176 * (Co_Dummy + i));
                //Debug.WriteLine(string.Format("{0:X}", ToMove));
                for (int j = 0; j < 176 / 4; j++)
                {
                    if (j * 4 == 32) continue;
                    int abc = mem.ReadInt(string.Format("{0:X}", FromMove + (j * 4)));
                    if (!mem.WriteMemory(string.Format("{0:X}", ToMove + (j * 4)), "int", abc.ToString()))
                    {
                        Debug.WriteLine("Failed to copy data of one move to another");
                        return false;
                    }
                }
            }
            return true;
        }
        private bool Edit_Cancels(ulong moveset, long[,] arr, int rows)
        {
            long cancel_addr = mem.ReadLong(string.Format("{0:X}", moveset + 432)); // 0x1B0
            if (cancel_addr == 0) return false;
            long requirement = mem.ReadLong(string.Format("{0:X}", moveset + 352)); // 0x160;
            if (requirement == 0) return false;
            long extradata = mem.ReadLong(string.Format("{0:X}", moveset + 464)); // 0x1D0
            if (extradata == 0) return false;
            long addr, req, ext;
            for (int i = 0; i < rows; i++)
            {
                // Reaching Address
                addr = cancel_addr + (arr[i, 0] * 40); // 1 cancel is of 40 bytes
                // Command
                if (arr[i,1] != -1)
                    if (!mem.WriteMemory(string.Format("{0:X}", addr), "long", arr[i, 1].ToString())) return false;
                // Requirement address
                if (arr[i, 2] != -1) {
                    req = requirement + (8 * arr[i, 2]); // 1 requirement field is of 8 bytes
                    if (!mem.WriteMemory(string.Format("{0:X}", addr + 8), "long", req.ToString())) return false;
                }
                // Extradata address
                if (arr[i, 3] != -1) {
                    ext = extradata + (4 * arr[i, 3]); // 1 Extradata field is of 4 bytes
                    if (!mem.WriteMemory(string.Format("{0:X}", addr + 16), "long", ext.ToString())) return false;
                }
                // Frame window start, end & starting frame
                if (arr[i, 4] != -1) if (!mem.WriteMemory(string.Format("{0:X}", addr + 24), "int", arr[i, 4].ToString())) return false;
                if (arr[i, 5] != -1) if (!mem.WriteMemory(string.Format("{0:X}", addr + 28), "int", arr[i, 5].ToString())) return false;
                if (arr[i, 6] != -1) if (!mem.WriteMemory(string.Format("{0:X}", addr + 32), "int", arr[i, 6].ToString())) return false;
                // Cancel move
                if (arr[i, 7] != -1) if (!mem.WriteMemory(string.Format("{0:X}", addr + 36), "2bytes", arr[i, 7].ToString())) return false;
                // Cancel option
                if (arr[i, 8] != -1) if (!mem.WriteMemory(string.Format("{0:X}", addr + 38), "2bytes", arr[i, 8].ToString())) return false;
            }
            return true;
        }

        private bool AssignCancelListIndexes(ulong moveset, long[,] arr, int rows)
        {
            long cancel_addr = mem.ReadLong(string.Format("{0:X}", moveset + 432)); // 0x1B0
            if (cancel_addr == 0) return false;
            long moves_addr = mem.ReadLong(string.Format("{0:X}", moveset + 528)); // 0x210;
            if (moves_addr == 0) return false;
            long addr, idx;
            for (int i = 0; i < rows; i++)
            {
                // Reaching Address
                addr = moves_addr + (arr[i, 0] * 176);
                // Calculating index number address
                idx = cancel_addr + (arr[i, 1] * 40);
                // Writing (offset for cancel address is 0x20)
                if (!mem.WriteMemory(string.Format("{0:X}", addr + 32), "long", idx.ToString())) return false;
            }
            return true;
        }

        // This function changes the extraproperty values of given moves to given indexes
        private bool Extraprops(ulong moveset, long[,] arr, int rows)
        {
            long moves_addr = mem.ReadLong(string.Format("{0:X}", moveset + 528)); // 0x210
            if (moves_addr == 0) return false;
            long extraprops_addr = mem.ReadLong(string.Format("{0:X}", moveset + 480)); // 0x1E0
            if (extraprops_addr == 0) return false;
            long addr, idx;
            for(int i = 0; i < rows; i++)
            {
                // Reaching the move Address
                addr = moves_addr + (176 * arr[i, 0]);
                // Calculating extraprop index value
                idx = extraprops_addr + (arr[i, 1] * 12); // 4 byte is for 1 field. Total 3 fields of a single property. Hence 12.
                if (!mem.WriteMemory(string.Format("{0:X}", addr + 128), "long", idx.ToString())) return false; // 0x80
            }
            return true;
        }
        // Returns the ID of currently selected character by Player 1
        private uint GetCharID()
        {
            uint side = (uint)mem.ReadInt(visuals + ",0x14");
            if (side == 0) // If playing at P1 side
            {
                return (uint)mem.ReadInt(visuals + ",0x1C");
            }
            else if (side == 1) // If playing at P2 side
            {
                return (uint)mem.ReadInt(visuals + ",0x20");
            }
            return 255;
        }

        private void TextBox(String msg)
        {
            textBox1.AppendText(msg);
        }
        private void Button_quit_Click(object sender, EventArgs e)
        {
            CloseProgram();
        }
        private void CloseProgram()
        {
            Checkboxes_checks(false);
            Application.Exit();
        }


    }
}
