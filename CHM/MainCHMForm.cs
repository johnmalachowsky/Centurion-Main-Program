using System;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Timers;
using System.Xml;
using System.Windows.Threading;
using Eval3;



using CHMPluginAPICommon;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;



//Debug Coding
//1-All Plugin Traffic
//2-Shutdown Commands
//3-CHMAPI_SetFlagOnServer
//4-CHMAPI_RequestFlagFromServer
//6-CHMAPI_PluginInformationGoingToServer
//7-CHMAPI_InformationCommingFromServerServer
//8-CHMAPI_PluginInformationGoingToPlugin
//9-CHMAPI_PluginInformationCommingFromPlugin
//10-CHMAPI_PluginInformationCommingFromPlugin
//11-CHMAPI_Heartbeat
//12-Maintanence Timer
//13-Overall Watchdog Timer
//14-CHMAPI_ServerFunctions

namespace CHM
{


    internal partial class MainCHMForm : Form
    {
        #region Application Recovery and Restart
        [Flags]
        public enum RestartRestrictions
        {
            None = 0,
            NotOnCrash = 1,
            NotOnHang = 2,
            NotOnPatch = 4,
            NotOnReboot = 8
        }

        public delegate int RecoveryDelegate(RecoveryData parameter);

        public static class ArrImports
        {
            [DllImport("kernel32.dll")]
            public static extern void ApplicationRecoveryFinished(
                bool success);

            [DllImport("kernel32.dll")]
            public static extern int ApplicationRecoveryInProgress(
                out bool canceled);

            [DllImport("kernel32.dll")]
            public static extern int GetApplicationRecoveryCallback(
                IntPtr processHandle,
                out RecoveryDelegate recoveryCallback,
                out RecoveryData parameter,
                out uint pingInterval,
                out uint flags);

            [DllImport("KERNEL32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetApplicationRestartSettings(
                IntPtr process,
                IntPtr commandLine,
                ref uint size,
                out uint flags);

            [DllImport("kernel32.dll")]
            public static extern int RegisterApplicationRecoveryCallback(
                RecoveryDelegate recoveryCallback,
                RecoveryData parameter,
                uint pingInterval,
                uint flags);

            [DllImport("kernel32.dll")]
            public static extern int RegisterApplicationRestart(
                [MarshalAs(UnmanagedType.BStr)] string commandLineArgs,
                int flags);

            //[DllImport("kernel32.dll")]
            //public static extern int UnregisterApplicationRecoveryCallback();

            //[DllImport("kernel32.dll")]
            //public static extern int UnregisterApplicationRestart();

            [DllImport("kernel32.dll")]
            public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
        }

        public class RecoveryData
        {
            string currentUser;

            public RecoveryData(string who)
            {
                currentUser = who;
            }
            public string CurrentUser
            {
                get { return currentUser; }
            }
        }

        internal void CauseSystemCrash()
        {
            object p = 0;
            IntPtr pnt = (IntPtr)0x123456789;
            Marshal.StructureToPtr(p, pnt, false);
        }
        #endregion


        #region Special Classes
        internal class FlagData
        {


            static internal ConcurrentDictionary<string, FlagDataStruct> FlagDataDictionary;
            internal class EventsStruct
            {
                public string Name;
                public string OwnerDll;
                public string Value;
                public DateTime EventTime;
                public DateTime LastUpdate;
            }


            static internal ConcurrentDictionary<string, EventsStruct> EventsDataDictionary;
            static private ConcurrentQueue<FlagDataStruct> FlagsToDisplay;
            static private ConcurrentQueue<Tuple<string,string, string>> FlagsToDelete;
            static internal Func<long> CurrentTick;
            static internal SystemData SysData;
            static internal MainCHMForm MF;
            static internal Int64 UniqueCounter = 0;
            static internal int FlagChangeHistoryMaxSize = 1000;
            static private ListBox _ActionItemsListbox, _EventsListBox;

            internal FlagData(Func<long> CurrentTickMethod, SystemData Sdt, ListBox ActionItemsListbox, ListBox EventsListBox)
            {
                FlagDataDictionary = new ConcurrentDictionary<string, FlagDataStruct>();
                EventsDataDictionary = new ConcurrentDictionary<string, EventsStruct>();
                FlagsToDisplay = new ConcurrentQueue<FlagDataStruct>();
                FlagsToDelete = new ConcurrentQueue<Tuple<string, string, string>>();
                CurrentTick = CurrentTickMethod;
                SysData = Sdt;
                MF = new MainCHMForm();
                _ActionItemsListbox = ActionItemsListbox;
                _EventsListBox = EventsListBox;
            }

            internal FlagData()
            {
            }

            internal bool AddOrUpdateEventData(string Name, string Owner, string Value, DateTime EventTime)
            {
                EventsStruct EV;
                if(EventsDataDictionary.TryGetValue(Name.ToLower(), out EV))
                {
                    EV.OwnerDll = Owner;
                    EV.Value = Value;
                    EV.EventTime = EventTime;
                    EV.LastUpdate = _GetCurrentDateTime();
                    EventsItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>(Name + " " + Value + " " + EventTime.ToString("HH':'mm':'ss MM'/'dd'/'yyyy"), _EventsListBox, Name, Name));
                    return (false);
                }
                else
                {
                    EV = new EventsStruct();
                    EV.Name = Name;
                    EV.OwnerDll = Owner;
                    EV.Value = Value;
                    EV.EventTime = EventTime;
                    EV.LastUpdate = _GetCurrentDateTime();
                    EventsDataDictionary.TryAdd(Name.ToLower(), EV);
                    EventsItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>(Name + " " + Value + " " + EventTime.ToString("HH':'mm':'ss MM'/'dd'/'yyyy"), _EventsListBox, Name, Name));
                    return (true);
                }


            }



            internal bool GetFlagsToDeleteValues(out Tuple<string, string,string> NameOfFlagToDelete)
            {
                return (FlagsToDelete.TryDequeue(out NameOfFlagToDelete));
            }

            internal bool GetFlagsToDisplayValues(out FlagDataStruct NameOfFlagToDiaplay)
            {
                return (FlagsToDisplay.TryDequeue(out NameOfFlagToDiaplay));
            }

            internal Dictionary<string, FlagDataStruct> CreateSpecialFlagDictionary()
            {

                return(FlagDataDictionary.ToDictionary(entry => entry.Key,entry => entry.Value));
            }

            internal void WriteFlagDataToLogFile(string Location, string Type, int Versions)
            {
                int a = 0, b = 0, c = 0, d = 0;
                DateTime d1 = new DateTime();
                DateTime d2 = new DateTime();
                string S1, S2;
                StreamWriter sw = null;
                FlagDataStruct FX;

                try
                {

                    foreach (KeyValuePair<string, FlagDataStruct> pair in FlagDataDictionary)
                    {
                        try
                        {
                            a = Math.Max(a, pair.Value.Name.Length);
                            b = Math.Max(b, pair.Value.Value.Length + pair.Value.RawValue.Length);
                            c = Math.Max(c, pair.Value.CreatedValue.Length);
                            if (!string.IsNullOrEmpty(pair.Value.SubType))
                                d = Math.Max(d, pair.Value.SubType.Length);
                        }
                        catch
                        {
                        }
                    }

                    a = a + 2;
                    b = b + 5;
                    c = c + 2;
                    d = d + 2;
                    string S = "{0,-" + a.ToString() + "} {1,-" + d.ToString() + "} {2,-" + b.ToString() + "} {3,-18} {4,-30} {5,-18} {6,-30} {7,-" + c.ToString() + "} {8}";
                    CHM.MainCHMForm._SetupFileVersions(Location + "\\FlagLogs" + Type + ".txt", Versions);
                    sw = File.CreateText(Location + "\\FlagLogs" + Type + ".txt");

                    // Acquire keys and sort them.
                    var list = FlagDataDictionary.Keys.ToList();
                    list.Sort();

                    // Loop through keys.
                    foreach (var key in list)
                    {
                        FX = FlagDataDictionary[key];
                        d1 = d1.AddTicks(-d1.Ticks + FX.ChangeTick);
                        d2 = d2.AddTicks(-d2.Ticks + FX.CreateTick);

                        if (d1 > DateTime.MinValue)
                            S1 = d1.ToString(DEBUGDATETIMEFORMATSTRING);
                        else
                            S1 = "                              ";

                        if (d2 > DateTime.MinValue)
                            S2 = d2.ToString(DEBUGDATETIMEFORMATSTRING);
                        else
                            S2 = "                              ";

                        sw.WriteLine(S, "'" + FX.Name + "'", "'" + FX.SubType + "'", "'" + FX.Value + "' '" + FX.RawValue + "'", S1, FX.ChangedBy, S2, FX.CreatedBy, FX.CreatedValue, FX.ChangeMode);
                    }
                    sw.Close();

                }
                catch (Exception err)
                {
                    if (sw != null)
                    {
                        sw.Write(err);
                        sw.Close();
                    }
                }
            }


            internal bool GetFlag(string Name, string SubType, out FlagDataStruct PluginFlag)
            {
                string S = Name;
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType;

                if (!FlagDataDictionary.TryGetValue(S.ToLower(), out PluginFlag))
                {
                    return (false);
                }
                return (true);
            }

            internal string GetValue(string Name, string SubType, out string RawData)
            {

                if (Name.Substring(0, 2) == "$$")
                {
                    string SV;
                    GetSpecialFlagInfo(Name, out SV, out RawData);
                    return (SV);
                }

                FlagDataStruct SX;
                string S = Name;
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType;

                if (!FlagDataDictionary.TryGetValue(S.ToLower(), out SX))
                {
                    RawData = "";
                    return ("");
                }
                RawData = SX.RawValue;
                string Sv = SX.Value;
                return (Sv);


            }

            internal string GetValue(string Name, string SubType)
            {
                string RawData;
                return (GetValue(Name, SubType, out RawData));

            }

            internal int GetValueInt(string Name, string SubType)
            {
                int i;
                int.TryParse((string)GetValue(Name, SubType), out i);
                return (i);
            }

            internal bool GetValueBool(string Name, string SubType)
            {
                bool i;
                Boolean.TryParse((string)GetValue(Name, SubType), out i);
                return (i);
            }



            internal bool DeleteFlag(string Name, string SubType)
            {
                string S = Name;
                FlagDataStruct SX = new FlagDataStruct();

                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType;

                bool flag = FlagDataDictionary.TryRemove(S.ToLower(), out SX);
                if(flag)
                    FlagsToDelete.Enqueue(new Tuple<string, string, string>(Name, SubType, SX.UniqueID.ToString()));
                return (flag);
            }

            internal bool FlagValidityCheck(string Flag, string plugin)
            {
                FlagDataStruct SX;
                if (!FlagDataDictionary.TryGetValue(Flag.ToLower().Trim(), out SX))
                    return (true);
                if (SX.ChangeMode != FlagChangeCodes.OwnerOnly || (SX.ChangeMode == FlagChangeCodes.OwnerOnly && SX.CreatedBy == plugin))
                    return (true);
                return (false);
            }

            internal bool GetFlagDataByUniqueId(long Uniqueid, out FlagDataStruct Flag)
            {
                try
                {
                    var fv = FlagDataDictionary.First(t => t.Value.UniqueID == Uniqueid);
                    Flag = fv.Value;
                    return (true);
                }
                catch
                {
                    Flag = new FlagDataStruct();
                    return (false);
                }

            }

            internal bool ChangeFlagArchiveStatus(string Name, string SubType, bool ArchiveStatus)
            {
                FlagDataStruct SX;
                string S = Name.Trim();
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType.Trim();
                bool IsItThere = FlagDataDictionary.TryGetValue(S.Trim().ToLower(), out SX);
                if (!IsItThere)
                    return (false);
                SX.Archive = ArchiveStatus;
                return (true);

            }

            internal bool TakeDeviceOffLine(string Name, string SubType, string ActionInitatedBy, out bool AlreadyOffline)
            {
                FlagDataStruct SX;
                AlreadyOffline = false;
                long Now = CurrentTick();
                string S = Name.Trim();
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType.Trim();
                bool IsItThere = FlagDataDictionary.TryGetValue(S.Trim().ToLower(), out SX);
                if (!IsItThere)
                    return (false);
                if (!SX.IsDeviceOffline)
                {
                    SX.LastChangeHistory.ChangedBy = SX.ChangedBy;
                    SX.LastChangeHistory.ChangeTime = SX.ChangeTick;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    SX.IsDeviceOffline = true;
                    SX.Value = SysData.GetValue("OffLineName");
                    SX.RawValue = SX.Value;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.ChangeTick = Now;
                    AlreadyOffline = false;
                    FlagsToDisplay.Enqueue(SX);

                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;
                        SX.ChangeHistory.Add(FCH);
                        if (SX.ChangeHistory.Count > SX.MaxHistoryToSave)
                            SX.ChangeHistory.RemoveAt(0);
                    }

                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                    }

                }
                else
                    AlreadyOffline = true;

                return (true);
            }

            internal bool GetSpecialFlagInfo(string Name, out string Value, out string RawData)
            {
                if (Name.ToLower().Substring(0,17)== "$$actionitemsline")
                {
                    int c;
                    if(int.TryParse(Name.Substring(17, 2),out c))
                    {
                        if(c<=_ActionItemsListbox.Items.Count)
                        {
                            CHMListBoxItems LBI = (CHMListBoxItems)_ActionItemsListbox.Items[c-1];
                            Value = LBI.Text;
                            RawData= LBI.Text;
                            return (true);
                        }
                    }
                }
                Value = "";
                RawData = "";
                return (false);

            }



            internal bool FullSetSystemFlag(string Name, string SubType, string Value, string SystemFlag_FlagType, string SystemFlag_FlagCatagory, string SystemFlag_ValidValues, string ActionInitatedBy, bool Archive)
            {
                FlagDataStruct SX;
                string S = Name.Trim();
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType.Trim();
                long Now = CurrentTick();

                bool IsItThere = FlagDataDictionary.TryGetValue(S.Trim().ToLower(), out SX);
                if (!IsItThere)
                {
                    SX = new FlagDataStruct();
                    SX.ValidValues = "";
                    SX.UniqueID = Interlocked.Increment(ref UniqueCounter);
                    SX.ChangeHistory = new List<FlagChangeHistory>();
                    SX.CreatedBy = ActionInitatedBy;
                    SX.CreatedValue = Value.Trim();
                    SX.CreatedRawValue = Value.Trim();
                    SX.CreateTick = Now;
                    SX.MaxHistoryToSave = FlagChangeHistoryMaxSize;
                    SX.Name = Name.Trim();
                    SX.Value = Value.Trim();
                    SX.RawValue = SX.Value;
                    SX.ChangeTick = Now;
                    SX.LastChangeHistory = new FlagChangeHistory();
                    SX.LastChangeHistory.ChangedBy = ActionInitatedBy;
                    SX.LastChangeHistory.ChangeTime = Now;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    SX.Archive = Archive;
              }

                if (IsItThere && (SX.Value != Value.Trim() || SX.Archive != Archive))
                {
                    SX.LastChangeHistory.ChangedBy = SX.ChangedBy;
                    SX.LastChangeHistory.ChangeTime = SX.ChangeTick;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    SX.Value = Value.Trim();
                    SX.RawValue = SX.Value;
                    SX.SystemFlag_FlagCatagory = SystemFlag_FlagType;
                    SX.SystemFlag_FlagType = SystemFlag_FlagCatagory;
                    SX.ValidValues = SystemFlag_ValidValues;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.ChangeTick = Now;
                    SX.Archive = Archive;
                    if (SX.IsDeviceOffline && SX.Value!= SysData.GetValue("OffLineName"))
                    {
                        MainCHMForm.PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 204, SX.Name+" "+SX.SubType + " ", SX.SourceUniqueID));
                        ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>("", _ActionItemsListbox, "",  SX.SourceUniqueID+SysData.GetValue("OffLineName")));
                        SX.IsDeviceOffline = false;
                    }
                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;

                        SX.ChangeHistory.Add(FCH);
                        if (SX.ChangeHistory.Count > SX.MaxHistoryToSave)
                            SX.ChangeHistory.RemoveAt(0);
                    }

                    FlagsToDisplay.Enqueue(SX);
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }
                if (!IsItThere)
                {
                    FlagDataDictionary.TryAdd(S.ToLower(), SX);
                    FlagsToDisplay.Enqueue(SX);
                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;

                        SX.ChangeHistory.Add(FCH);
                    }
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }

                return (true);
            }


            internal bool FullSet(string Name, string SubType, string Room, string SourceUnique, string Value, string RawValue, FlagChangeCodes ChangeMode, string ActionInitatedBy, int MaxHistoryToSave, string ValidValues, string UOM)
            {
                FlagDataStruct SX;
                string S = Name.Trim();
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType.Trim();
                long Now = CurrentTick();

                bool IsItThere = FlagDataDictionary.TryGetValue(S.ToLower(), out SX);
                if (IsItThere)
                {
                    SX.ChangeMode = ChangeMode;
                    SX.MaxHistoryToSave = MaxHistoryToSave;
                }

                if (!IsItThere)
                {
                    SX = new FlagDataStruct();
                    SX.UniqueID = Interlocked.Increment(ref UniqueCounter);
                    SX.RoomUniqueID = Room;
                    SX.SourceUniqueID = SourceUnique;
                    SX.CreatedBy = ActionInitatedBy;
                    SX.CreateTick = Now;
                    SX.CreatedValue = Value;
                    SX.CreatedRawValue = RawValue;
                    SX.ChangeHistory = new List<FlagChangeHistory>();
                    SX.MaxHistoryToSave = MaxHistoryToSave;
                    SX.Name = Name.Trim();
                    SX.SubType = SubType;
                    SX.Value = Value.Trim();
                    SX.RawValue = RawValue;
                    SX.ChangeTick = Now;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.UOM = UOM;
                    SX.LastChangeHistory = new FlagChangeHistory();
                    SX.LastChangeHistory.ChangedBy = ActionInitatedBy;
                    SX.LastChangeHistory.ChangeTime = Now;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;

                }

                if (IsItThere && (SX.Value != Value.Trim() || SX.RawValue != RawValue || SX.UOM != UOM))
                {
                    SX.LastChangeHistory.ChangedBy = SX.ChangedBy;
                    SX.LastChangeHistory.ChangeTime = SX.ChangeTick;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    SX.Value = Value.Trim();
                    SX.RawValue = RawValue;
                    SX.ChangeMode = ChangeMode;
                    SX.RoomUniqueID = Room;
                    SX.SourceUniqueID = SourceUnique;
                    SX.MaxHistoryToSave = MaxHistoryToSave;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.ChangeTick = Now;
                    if (SX.IsDeviceOffline && SX.Value != SysData.GetValue("OffLineName"))
                    {
                        MainCHMForm.PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 204, SX.Name + " " + SX.SubType + " ", SX.SourceUniqueID));
                        ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>("", _ActionItemsListbox, "", SX.SourceUniqueID + SysData.GetValue("OffLineName")));
                        SX.IsDeviceOffline = false;
                    }

                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;
                        SX.ChangeHistory.Add(FCH);
                        if (SX.ChangeHistory.Count > SX.MaxHistoryToSave)
                            SX.ChangeHistory.RemoveAt(0);
                    }

                    SX.ValidValues = ValidValues;
                    SX.UOM = UOM;
                    FlagsToDisplay.Enqueue(SX);
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }
                if (!IsItThere)
                {
                    FlagDataDictionary.TryAdd(S.ToLower(), SX);
                    FlagsToDisplay.Enqueue(SX);
                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;

                        SX.ChangeHistory.Add(FCH);

                    }
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }
                return (true);
            }

            internal bool FullSetAddOnly(string Name, string SubType, string Room, string SourceUnique, string Value, string RawValue, FlagChangeCodes ChangeMode, string ActionInitatedBy, int MaxHistoryToSave, string ValidValues)
            {
                return (FullSetAddOnly(Name,  SubType,  Room,  SourceUnique,  Value,  RawValue,  ChangeMode,  ActionInitatedBy,  MaxHistoryToSave,  ValidValues, ""));
            }

            internal bool FullSetAddOnly(string Name, string SubType, string Room, string SourceUnique, string Value, string RawValue, FlagChangeCodes ChangeMode, string ActionInitatedBy, int MaxHistoryToSave, string ValidValues, string UOM)
            {
                FlagDataStruct SX = new FlagDataStruct();
                long Now = CurrentTick();

                SX.Name = Name.Trim();
                SX.Value = Value.Trim();
                SX.RawValue = RawValue;
                SX.SubType = SubType;
                SX.ChangeMode = ChangeMode;
                SX.CreatedBy = ActionInitatedBy;
                SX.CreateTick = Now;
                SX.CreatedValue = SX.Value;
                SX.CreatedRawValue = RawValue;
                SX.RoomUniqueID = Room;
                SX.SourceUniqueID = SourceUnique;
                SX.UOM = UOM;
                SX.ChangeTick = Now;
                SX.ChangedBy = ActionInitatedBy;
                SX.ChangeHistory = new List<FlagChangeHistory>();
                SX.MaxHistoryToSave = MaxHistoryToSave;
                SX.ValidValues = ValidValues;
                SX.LastChangeHistory = new FlagChangeHistory();
                SX.LastChangeHistory.ChangedBy = ActionInitatedBy;
                SX.LastChangeHistory.ChangeTime = Now;
                SX.LastChangeHistory.RawValue = SX.RawValue;
                SX.LastChangeHistory.Value = SX.Value;
                if (SX.MaxHistoryToSave > 0)
                {
                    FlagChangeHistory FCH = new FlagChangeHistory();
                    FCH.ChangedBy = ActionInitatedBy;
                    FCH.ChangeTime = Now;
                    FCH.RawValue = SX.RawValue;
                    FCH.Value = SX.Value;
                    SX.ChangeHistory.Add(FCH);
                }

                string S = Name;
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType;

                SX.UniqueID = Interlocked.Increment(ref UniqueCounter);
                FlagDataDictionary.TryAdd(S.ToLower(), SX);
                FlagsToDisplay.Enqueue(SX);
                if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                {
                    FlagArchiveStruct FAS = new FlagArchiveStruct();
                    FAS.ChangeTick = SX.ChangeTick;
                    FAS.CreateTick = SX.CreateTick;
                    FAS.IsDeviceOffline = SX.IsDeviceOffline;
                    FAS.Name = SX.Name;
                    FAS.RawValue = SX.RawValue;
                    FAS.RoomUniqueID = SX.RoomUniqueID;
                    FAS.SourceUniqueID = SX.SourceUniqueID;
                    FAS.SubType = SX.SubType;
                    FAS.Value = SX.Value;
                    ArchiveFlagChangesQueue.Enqueue(FAS);
                    if (SX.MaxHistoryToSave > 0)
                        SX.LastChangeHistory.Archived = true;
                }
                return (true);
            }

            internal bool FullSetUpdateOnly(string Name, string SubType, string Room, string SourceUnique, string Value, string RawValue, FlagChangeCodes ChangeMode, string ActionInitatedBy, int MaxHistoryToSave, string ValidValues, string UOM)
            {
                string S = Name;
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType;

                if (!FlagDataDictionary.ContainsKey(S.ToLower()))
                    return (false);

                return (FullSet(Name, SubType, Room, SourceUnique, Value, RawValue, ChangeMode, ActionInitatedBy, MaxHistoryToSave, ValidValues, UOM));
            }


            internal bool Set(string Name, string SubType, string Value, FlagChangeCodes ChangeMode, string ActionInitatedBy)
            {
                return (_SetX(Name, SubType, Value, Value, ChangeMode, ActionInitatedBy));
            }

            internal bool Set(string Name, string SubType, string Value, FlagChangeCodes ChangeMode, string ActionInitatedBy, int MaxHistorySize)
            {
                return (_SetX(Name, SubType, Value, Value, ChangeMode, ActionInitatedBy, MaxHistorySize));
            }

            internal bool _SetX(string Name, string SubType, string Value, string RawValue, FlagChangeCodes ChangeMode, string ActionInitatedBy)
            {
                FlagDataStruct SX;
                string S = Name.Trim();
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType.Trim();
                long Now = CurrentTick();

                bool IsItThere = FlagDataDictionary.TryGetValue(S.ToLower(), out SX);
                if(IsItThere)
                    SX.ChangeMode = ChangeMode;

                if (!IsItThere)
                {
                    SX = new FlagDataStruct();
                    SX.ValidValues = "";
                    SX.UniqueID = Interlocked.Increment(ref UniqueCounter);
                    SX.ChangeHistory = new List<FlagChangeHistory>();
                    SX.CreatedBy = ActionInitatedBy;
                    SX.CreateTick = Now;
                    SX.CreatedValue = Value;
                    SX.CreatedRawValue = RawValue;
                    SX.MaxHistoryToSave = FlagChangeHistoryMaxSize;
                    SX.Name = Name.Trim();
                    SX.SubType = SubType.Trim();
                    SX.Value = Value.Trim();
                    SX.RawValue = RawValue;
                    SX.ChangeTick = Now;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.LastChangeHistory = new FlagChangeHistory();
                    SX.LastChangeHistory.ChangedBy = ActionInitatedBy;
                    SX.LastChangeHistory.ChangeTime = Now;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;

                        SX.ChangeHistory.Add(FCH);

                    }
                }

                if (IsItThere && (SX.Value != Value.Trim() || SX.RawValue != RawValue))
                {
                    SX.LastChangeHistory.ChangedBy = SX.ChangedBy;
                    SX.LastChangeHistory.ChangeTime = SX.ChangeTick;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    SX.Value = Value.Trim();
                    SX.RawValue = RawValue;
                    SX.ChangeMode = ChangeMode;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.ChangeTick = Now;
                    if (SX.IsDeviceOffline && SX.Value != SysData.GetValue("OffLineName"))
                    {
                        MainCHMForm.PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 204, SX.Name + " " + SX.SubType + " ", SX.SourceUniqueID));
                        ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>("", _ActionItemsListbox, "", SX.SourceUniqueID + SysData.GetValue("OffLineName")));
                        SX.IsDeviceOffline = false;
                    }


                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;
                        SX.ChangeHistory.Add(FCH);
                        if (SX.ChangeHistory.Count > SX.MaxHistoryToSave)
                            SX.ChangeHistory.RemoveAt(0);
                    }
                    FlagsToDisplay.Enqueue(SX);
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }

                if (!IsItThere)
                {
                    FlagDataDictionary.TryAdd(S.ToLower(), SX);
                    FlagsToDisplay.Enqueue(SX);
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }
                return (true);
            }

            internal bool _SetX(string Name, string SubType, string Value, string RawValue, FlagChangeCodes ChangeMode, string ActionInitatedBy, int MaxHistoryToSave)
            {
                FlagDataStruct SX;
                string S = Name.Trim();
                if (!string.IsNullOrEmpty(SubType))
                    S = S + " " + SubType.Trim();
                long Now = CurrentTick();

                bool IsItThere = FlagDataDictionary.TryGetValue(S.ToLower(), out SX);
                if (IsItThere)
                {
                    SX.ChangeMode = ChangeMode;
                    SX.MaxHistoryToSave = MaxHistoryToSave;
                }

                if (!IsItThere)
                {
                    SX = new FlagDataStruct();
                    SX.ValidValues = "";
                    SX.UniqueID = Interlocked.Increment(ref UniqueCounter);
                    SX.ChangeHistory = new List<FlagChangeHistory>();
                    SX.CreatedBy = ActionInitatedBy;
                    SX.CreateTick =Now;
                    SX.CreatedValue = Value;
                    SX.CreatedRawValue = RawValue;
                    SX.MaxHistoryToSave = MaxHistoryToSave;
                    SX.Name = Name.Trim();
                    SX.SubType = SubType.Trim();
                    SX.Value = Value.Trim();
                    SX.RawValue = RawValue;
                    SX.ChangeTick = Now;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.LastChangeHistory = new FlagChangeHistory();
                    SX.LastChangeHistory.ChangedBy = ActionInitatedBy;
                    SX.LastChangeHistory.ChangeTime = Now;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;

                        SX.ChangeHistory.Add(FCH);
                    }
                }

                if (IsItThere && (SX.Value != Value.Trim() || SX.RawValue != RawValue))
                {
                    SX.LastChangeHistory.ChangedBy = SX.ChangedBy;
                    SX.LastChangeHistory.ChangeTime = SX.ChangeTick;
                    SX.LastChangeHistory.RawValue = SX.RawValue;
                    SX.LastChangeHistory.Value = SX.Value;
                    SX.Value = Value.Trim();
                    SX.RawValue = RawValue;
                    SX.ChangeMode = ChangeMode;
                    SX.ChangedBy = ActionInitatedBy;
                    SX.ChangeTick = Now;
                    if (SX.IsDeviceOffline && SX.Value != SysData.GetValue("OffLineName"))
                    {
                        MainCHMForm.PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 204, SX.Name + " " + SX.SubType + " ", SX.SourceUniqueID));
                        ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>("", _ActionItemsListbox, "", SX.SourceUniqueID + SysData.GetValue("OffLineName")));
                        SX.IsDeviceOffline = false;
                    }


                    if (SX.MaxHistoryToSave > 0)
                    {
                        FlagChangeHistory FCH = new FlagChangeHistory();
                        FCH.ChangedBy = ActionInitatedBy;
                        FCH.ChangeTime = Now;
                        FCH.RawValue = SX.RawValue;
                        FCH.Value = SX.Value;
                        SX.ChangeHistory.Add(FCH);
                        if (SX.ChangeHistory.Count > SX.MaxHistoryToSave)
                            SX.ChangeHistory.RemoveAt(0);
                    }
                    FlagsToDisplay.Enqueue(SX);
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }

                if (!IsItThere)
                {
                    FlagDataDictionary.TryAdd(S.ToLower(), SX);
                    FlagsToDisplay.Enqueue(SX);
                    if (Archiving && ((!String.IsNullOrEmpty(SX.SourceUniqueID) && SX.SourceUniqueID.Substring(0, 1) == "D") || SX.Archive == true))
                    {
                        FlagArchiveStruct FAS = new FlagArchiveStruct();
                        FAS.ChangeTick = SX.ChangeTick;
                        FAS.CreateTick = SX.CreateTick;
                        FAS.IsDeviceOffline = SX.IsDeviceOffline;
                        FAS.Name = SX.Name;
                        FAS.RawValue = SX.RawValue;
                        FAS.RoomUniqueID = SX.RoomUniqueID;
                        FAS.SourceUniqueID = SX.SourceUniqueID;
                        FAS.SubType = SX.SubType;
                        FAS.Value = SX.Value;
                        ArchiveFlagChangesQueue.Enqueue(FAS);
                        if (SX.MaxHistoryToSave > 0)
                            SX.LastChangeHistory.Archived = true;
                    }
                }
                return (true);
            }

 
            internal bool Set(string Name, string SubType, int Value, FlagChangeCodes Type, string ActionInitatedBy, int MaxHistoryToChange)
            {
                return (Set(Name, SubType, Value.ToString(), Type, ActionInitatedBy, MaxHistoryToChange));
            }

            internal bool Set(string Name, string SubType, long Value, FlagChangeCodes Type, string ActionInitatedBy)
            {
                return (Set(Name, SubType, Value.ToString(), Type, ActionInitatedBy));
            }

            internal bool Set(string Name, string SubType, bool Value, FlagChangeCodes Type, string ActionInitatedBy)
            {
                return (Set(Name, SubType, Value.ToString(), Type, ActionInitatedBy));
            }

            internal bool Set(string Name, string SubType, bool Value, FlagChangeCodes Type, string ActionInitatedBy, int MaxHistoryToSave)
            {
                return (Set(Name, SubType, Value.ToString(), Type, ActionInitatedBy, MaxHistoryToSave));
            }

            internal bool Set(string Name, string SubType, string Value, string ActionInitatedBy)
            {
                return (Set(Name, SubType, Value, FlagChangeCodes.Changeable, ActionInitatedBy));
            }

            internal bool Set(string Name, string SubType, int Value, string ActionInitatedBy)
            {
                return (Set(Name, SubType, Value.ToString(), FlagChangeCodes.Changeable, ActionInitatedBy));
            }

            internal bool Set(string Name, string SubType, long Value, string ActionInitatedBy)
            {
                return (Set(Name, SubType, Value.ToString(), FlagChangeCodes.Changeable, ActionInitatedBy));
            }

            internal bool Set(string Name, string SubType, bool Value, string ActionInitatedBy)
            {
                return (Set(Name, SubType, Value.ToString(), FlagChangeCodes.Changeable, ActionInitatedBy));
            }
        }

        internal class SystemData
        {

            internal struct SysDataStruct
            {
                internal string Name;
                internal string SubField;
                internal string Value;
                internal char Type; //S-Saved and internal     P-Saved and internal  N-Not Saved and internal   X-Not Saved and Private
                internal long Created;
                internal long LastUpdated;
                internal long LastDiskSave;
                internal bool Loaded;
            }

            internal ConcurrentDictionary<string, SysDataStruct> SystemDataDictionary;
            internal Func<long> _GetCurrentTick;
            internal long p;
            internal ConcurrentQueue<string> PendingMessageQueue;
            internal FlagData FlagAccess = null;
            internal ConcurrentQueue<SysDataStruct> PendingDatabaseQueue;

            internal SystemData(Func<long> CurrentTickMethod, ConcurrentQueue<string> Pmq)
            {
                SystemDataDictionary = new ConcurrentDictionary<string, SysDataStruct>();
                PendingDatabaseQueue = new ConcurrentQueue<SysDataStruct>();
                _GetCurrentTick = CurrentTickMethod;
                PendingMessageQueue = Pmq;
            }


            internal void AddFlagAccess(FlagData FAccess)
            {
                FlagAccess = FAccess;
            }

            internal void LoadConfigurationDataFromDatabase(DatabaseAccess DB)
            {
                DB.LoadConfigurationData(MODULESERIALNUMBER, _SetX);
            }

            internal void WriteSysDataToLogFile(string Location, string Type, int Versions)
            {
                int b = 0, c = 0;
                DateTime d1 = new DateTime();
                DateTime d2 = new DateTime();
                DateTime d3 = new DateTime();
                string S1, S2, S3;
                SysDataStruct SX = new SysDataStruct();
                StreamWriter sw = null;

                try
                {

                    foreach (KeyValuePair<string, SysDataStruct> pair in SystemDataDictionary)
                    {
                        b = Math.Max(b, pair.Value.Name.Length);
                        c = Math.Max(c, pair.Value.Value.Length);
                    }

                    b = b + 2;
                    c = c + 2;
                    string S = "{0,-" + b.ToString() + "} {1,-" + c.ToString() + "} {2,-5}   {3,-30}   {4,-30}   {5,-30} {6,1}";
                    CHM.MainCHMForm._SetupFileVersions(Location + "\\SysDataLogs" + Type + ".txt", Versions);
                    sw = File.CreateText(Location + "\\SysDataLogs" + Type + ".txt");

                    // Acquire keys and sort them.
                    var list = SystemDataDictionary.Keys.ToList();
                    list.Sort();

                    // Loop through keys.
                    foreach (var key in list)
                    {
                        SX = SystemDataDictionary[key];
                        d1 = d1.AddTicks(-d1.Ticks + SX.Created);
                        d2 = d2.AddTicks(-d2.Ticks + SX.LastUpdated);
                        d3 = d3.AddTicks(-d3.Ticks + SX.LastDiskSave);

                        if (d1 > DateTime.MinValue)
                            S1 = d1.ToString(DEBUGDATETIMEFORMATSTRING);
                        else
                            S1 = "                              ";

                        if (d2 > DateTime.MinValue)
                            S2 = d2.ToString(DEBUGDATETIMEFORMATSTRING);
                        else
                            S2 = "                              ";

                        if (d3 > DateTime.MinValue)
                            S3 = d3.ToString(DEBUGDATETIMEFORMATSTRING);
                        else
                            S3 = "                              ";
                        sw.WriteLine(S, SX.Name, SX.Value, SX.Type, S1, S2, S3, SX.Loaded.ToString().Substring(0, 1));
                    }
                    sw.Close();

                }
                catch (Exception err)
                {
                    if (sw != null)
                    {
                        sw.Write(err);
                        sw.Close();
                    }
                }
            }

            internal string GetValue(string Name)
            {
                SysDataStruct SX;

                if (!SystemDataDictionary.TryGetValue(Name, out SX))
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 60, "", "'" + Name + "'"));
                    return ("");
                }
                return (SX.Value);
            }


            internal int GetValueInt(string Name, int DefaultValue)
            {
                int i;

                if (PeekValue(Name, out i))
                    return (i);
                SetIfNotLoaded(Name, DefaultValue);
                return (DefaultValue);
            }


            internal int GetValueInt(string Name)
            {
                int i;
                int.TryParse((string)GetValue(Name), out i);
                return (i);
            }

            internal bool GetValueBool(string Name)
            {
                bool i;
                Boolean.TryParse((string)GetValue(Name), out i);
                return (i);
            }

            internal bool PeekValue(string Name, out string Value)
            {
                SysDataStruct SX;

                if (!SystemDataDictionary.TryGetValue(Name, out SX))
                {
                    Value = "";
                    return (false);
                }
                Value = SX.Value;
                return (true);
            }

            internal bool PeekValue(string Name, out bool Value)
            {
                string S;

                bool FL = PeekValue(Name, out S);
                Boolean.TryParse(S, out Value);
                return (FL);
            }

            internal bool PeekValue(string Name, out int Value)
            {
                string S;

                bool FL = PeekValue(Name, out S);
                int.TryParse(S, out Value);
                return (FL);
            }

            internal bool Set(string Name, string SubField, string Value, char Type)
            {
                return (_SetX(Name, SubField, Value, Type, false));
            }


            internal bool Set(string Name, string Value, char Type)
            {
                return (_SetX(Name, "", Value, Type, false));
            }

            internal bool _SetX(string Name, string SubField, string Value, char Type, bool Loaded)
            {
                SysDataStruct SX = new SysDataStruct();
                bool UpdateFlag = false;

                SX.Name = Name;
                SX.Value = Value;
                SX.SubField = SubField;
                SX.Type = Type;
                SX.LastDiskSave = 0;
                SX.Loaded = Loaded;
                SX.LastUpdated = _GetCurrentTick();
                if (Loaded)
                    SX.Created = 0;
                else
                    SX.Created = SX.LastUpdated;
                SystemDataDictionary.AddOrUpdate(Name, SX, (key, oldValue) => _Update(oldValue, Value, SX.LastDiskSave, out UpdateFlag));
                if (SystemDataDictionary.TryGetValue(Name, out SX))
                {
                    if (SX.Type == 'S' || SX.Type == 'N')//internal Value
                    {
                        FlagAccess.Set(Name, "", Value, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, 0);
                        if (!Loaded)
                        {
                            if (_SaveValueToDatabaseQueue(SX))
                                SX.LastDiskSave = SX.LastUpdated;
                        }

                    }
                }
                return (true);
            }

            internal SysDataStruct _Update(SysDataStruct OldValues, string Value, long LastDiskUpdate, out bool UpdateFlag)
            {
                SysDataStruct SX = new SysDataStruct();
                SX = OldValues;
                SX.Value = Value;
                SX.LastUpdated = LastDiskUpdate;
                SX.LastDiskSave = LastDiskUpdate;
                UpdateFlag = true;
                return (SX);
            }

            internal bool _SaveValueToDatabaseQueue(SysDataStruct SX)
            {
                if (SX.Type == 'S' || SX.Type == 'P')
                {
                    PendingDatabaseQueue.Enqueue(SX);
                    return (true);
                }
                return (false);
            }

            internal void _SaveQueuedValuesToDataBase(DatabaseAccess DB)
            {
                foreach (var SX in PendingDatabaseQueue)
                {
                    DB.AddorUpdateConfiguration(MODULESERIALNUMBER, SX.Name, SX.Value, SX.Type);
                }
            }

            internal int UpdateConfigInformation()
            {
                int count = 0;
                SysDataStruct SX = new SysDataStruct();
                bool Flag = true;
                try
                {
                    foreach (KeyValuePair<string, SysDataStruct> pair in SystemDataDictionary)
                    {

                        if (pair.Value.LastDiskSave == 0 && !pair.Value.Loaded)
                        {
                            SX = pair.Value;
                            if (_SaveValueToDatabaseQueue(SX))
                            {
                                SX.LastDiskSave = SX.LastUpdated;
                                SystemDataDictionary.AddOrUpdate(SX.Name, SX, (key, oldValue) => _Update(oldValue, SX.Value, SX.LastDiskSave, out Flag));
                                count++;
                            }
                        }
                    }
                }
                catch
                {
                }

                return (count);
            }



            internal bool SetIfNotLoaded(string Name, string Value)
            {
                if (SystemDataDictionary.ContainsKey(Name))
                    return (false);
                Set(Name, Value);
                return (true);
            }

            internal bool SetIfNotLoaded(string Name, int Value)
            {
                return (SetIfNotLoaded(Name, Value.ToString()));
            }

            internal bool SetIfNotLoaded(string Name, bool Value)
            {
                return (SetIfNotLoaded(Name, Value.ToString()));
            }

            internal bool SetIfNotLoaded(string Name, string Value, char Type)
            {
                if (SystemDataDictionary.ContainsKey(Name))
                    return (false);
                Set(Name, Value, Type);
                return (true);
            }

            internal bool SetIfNotLoaded(string Name, int Value, char Type)
            {
                return (SetIfNotLoaded(Name, Value.ToString(), Type));
            }

            internal bool SetIfNotLoaded(string Name, bool Value, char Type)
            {
                return (SetIfNotLoaded(Name, Value.ToString(), Type));
            }

            internal bool Set(string Name, int Value, char Type)
            {
                return (Set(Name, Value.ToString(), Type));
            }

            internal bool Set(string Name, long Value, char Type)
            {
                return (Set(Name, Value.ToString(), Type));
            }

            internal bool Set(string Name, bool Value, char Type)
            {
                return (Set(Name, Value.ToString(), Type));
            }

            internal string SetAndReturnValue(string Name, string Value, char Type)
            {
                Set(Name, Value, Type);
                return (Value);
            }

            internal bool Set(string Name, string Value)
            {
                return (Set(Name, Value, 'X'));
            }

            internal bool Set(string Name, int Value)
            {
                return (Set(Name, Value.ToString(), 'X'));
            }

            internal bool Set(string Name, long Value)
            {
                return (Set(Name, Value.ToString(), 'X'));
            }

            internal bool Set(string Name, bool Value)
            {
                return (Set(Name, Value.ToString(), 'X'));
            }


        }


        #endregion


        #region Structs

        internal struct PluginStruct
        {
            internal string PluginName;
            internal string SHA512Calculated;
            internal string SHA512Database;
            internal bool SHA512Matched;
            internal string DBSerialNumber;
            internal string MetaSerialNumber;
            internal string PluginVersion;
            internal string PluginDescription;
            internal bool Foreign;
            internal bool Ignore;
            internal string DLLType;
            internal string Uniqueid;
            internal bool Initialized;
            internal bool Loaded;
            internal bool LoadError;
            internal bool PluginInErrorState;
            internal bool PluginDeactivated;
            internal int PluginUniqueID;
            internal int ListBoxItemIndex;
            internal Assembly Plugin;
            internal Type AssemblyType;
            internal object Instance;
            internal Type ServerAssemblyType;
            internal object ServerInstance;
            internal PluginStatusStruct Status;
            internal List<string> PluginsToAcceptDataFrom;
            internal int NumberOfResets;
            internal bool SendAllIncedentFlags;
            internal bool SendJustFlagChangeIncedentFlags;
            internal string[] OriginalDataBaseInfo;
        };

        #endregion

        #region System Constants
        internal const string MODULESERIALNUMBER = "00001-00000";
        protected const string MESSAGEQUEUEFORMATSTRING = "{0,25} {1,11} {2,12:+#;-#;0} {3,-40} {4,-40}";
        protected const string DEBUGQUEUEFORMATSTRING = "*{0,25}  {1,11} {2,12:+#;-#;0} {3} {4}";
        protected const string DEBUGDATETIMEFORMATSTRING = "MM/dd/yyyy hh:mm:ss.fffffff tt";
        #endregion

        #region System Variables and Other Things
//Function Classes
        static internal SystemData SysData;
        internal DatabaseAccess MainDB;
        //internal static DatabaseAccess HTTPDB;
        internal static DatabaseAccess ModuleDB;
        static internal FlagData FlagAccess;
        static internal Evaluator ev;
        static internal CHMPluginAPI.EvalFunctions EvalMathFunctions;

//Containers
        internal static ConcurrentQueue<string> PendingMessageQueue;
        internal ConcurrentQueue<Tuple<string, ListBox, string>> OtherGUIBoxesQueue;
        internal static ConcurrentQueue<Tuple<string, ListBox, string, string>> ActionItemsListBoxQueue;
        internal static ConcurrentQueue<Tuple<string, ListBox, string, string>> EventsItemsListBoxQueue;
        internal ConcurrentQueue<ListBox> LBToClearQueue;
        internal ConcurrentQueue<string> SavedMessageQueue;
        internal ConcurrentQueue<string> ErrorMessageQueue;
        internal ConcurrentQueue<string> DebugMessageQueue;
        internal static ConcurrentQueue<FlagArchiveStruct> ArchiveFlagChangesQueue;
        internal ConcurrentQueue<PluginServerDataStruct> ServerToPluginQueue;
        internal ConcurrentQueue<Tuple<PluginIncedentFlags, object>> IncedentFlagQueue;
        static internal ConcurrentDictionary<string, PluginStruct> PluginDictionary;
        static internal Dictionary<string, InterfaceStruct> InterfaceDictionary;
        static internal Dictionary<string, DeviceStruct> DeviceDictionary;
        internal Dictionary<string, DeviceTemplateStruct> DeviceTemplateDictionary;
        internal Dictionary<string, PasswordStruct> PasswordDictionary;
        internal Dictionary<string, StatusMessagesStruct> StatusMessagesDictionary;
        static internal List<Tuple<string, string, string, string>> RoomList;
        static List<SystemFlagStruct> SystemFlagsList;
        static SortedList<int, Tuple<string, string, string>> UOM;


        //Global Variables
        static long UTCOffsetTicks = -1;
        static int _NextSequencialNumber = 0;
        static ServerStatusCodes ServerStatusCode = ServerStatusCodes.InStartup;
        static bool[] DebugSettings;
        static internal string EncryptedMasterPassword;
        static string[] DecryptedStartupFile;
        static bool IsStartupFinished = false;
        static string CommandDLL = "";
        static string HTMLDLL = "";
        static string MENUDLL = "";
        static string MachineName;
        static string ArchiveDLL = "";
        static bool Archiving = false;
        static string BackupDLL;


        //Semaphores
        static internal SemaphoreSlim DoMathSlim;
        static internal SemaphoreSlim GetFlagsFromListSlim;


        //Device Menu List Stuff
        static internal ConcurrentQueue<Tuple<string, FlagDataStruct>> MenuDeviceListFlagChanges;
        static internal bool IsTheMenuDeviceListActive = false;


 
        #endregion

        #region Timer Static Variables

        internal DateTime MaintanenceTimerLastDate;
        internal long MaintanenceTimerLastTick;
        internal bool MaintanenceTimerChangedMinute;
        internal long MaintanenceTimerLastLogSave;
        internal int MaintanenceTimerLastSecond = -1;
        internal int MaintanenceTimerLastMinute = -1;
        internal int MaintanenceTimerLastHour = -1;
        internal int MaintanenceTimerLastDay = -1;
        internal int MaintanenceTimerLastWeek = -1;
        internal int MaintanenceTimerLastMonth = -1;
        internal int MaintanenceTimerLastYear = -1;
        internal int LastMaintTimerIndex = 0;
        internal int LastPluginTimerIndex = 0;
        internal int LastHeartBeatTimerIndex = 0;


        internal long PluginTimerLastLogSave;
        internal long HeartbeatTimerLastLogSave;
        internal long PluginWatchDogTimerLastLogSave;


        internal DateTime HeartBeatLastProcess;
        internal DateTime PluginWatchDogLastProcess;
        internal DateTime DisplayLastProcess;
        internal int DisplayTimesliceLastProcess = 0;

        internal Stopwatch StartTime;
        internal Stopwatch PluginWatchdogTotalTime;
        internal Stopwatch HeartbeatTotalTime;
        internal Stopwatch MaintanenceTotalTime;
        internal Stopwatch PluginTotalTime;




        #endregion

        #region Thread Variables
        internal Thread StartupThread;
        internal Thread MainThread;
        internal Thread HTTPThread;
        #endregion

        #region Timers
        internal static System.Timers.Timer MaintanenceTimer;
        internal static System.Timers.Timer PluginTimer;
        internal static System.Timers.Timer HeartbeatTimer;
        internal static System.Timers.Timer PluginWatchDogTimer;
        internal static System.Timers.Timer DisplayTimer;
        internal static System.Timers.Timer CHMWatchDogTimer;
        internal static System.Timers.Timer IncedentFlagTimer;
        internal static System.Timers.Timer ArchiveTimer;
        internal static System.Windows.Threading.DispatcherTimer MainGUIDispatchTimer;


        #endregion

        #region PluginMonitoringVariable

        internal static long CHMAPI_ShutDownPlugin_StartTime;
        internal static int CHMAPI_ShutDownPlugin_PluginUniqueID;
        internal static long CHMAPI_ShutDownPlugin_EndTime;

        internal static long CHMAPI_PluginStartupCompleted_StartTime;
        internal static int CHMAPI_PluginStartupCompleted_PluginUniqueID;
        internal static long CHMAPI_PluginStartupCompleted_EndTime;

        internal static long CHMAPI_WatchdogProcess_StartTime;
        internal static int CHMAPI_WatchdogProcess_PluginUniqueID;
        internal static long CHMAPI_WatchdogProcess_EndTime;

        internal static int CHMAPI_SetFlagOnServer_StartTime;
        internal static int CHMAPI_SetFlagOnServer_PluginUniqueID;
        internal static long CHMAPI_SetFlagOnServer_EndTime;

        internal static long CHMAPI_RequestFlagFromServer_StartTime;
        internal static int CHMAPI_RequestFlagFromServer_PluginUniqueID;
        internal static long CHMAPI_RequestFlagFromServer_EndTime;

        internal static long CHMAPI_FlagCommingFromServer_StartTime;
        internal static int CHMAPI_FlagCommingFromServer_PluginUniqueID;
        internal static long CHMAPI_FlagCommingFromServer_EndTime;

        internal static long CHMAPI_PluginInformationGoingToServer_StartTime;
        internal static int CHMAPI_PluginInformationGoingToServer_PluginUniqueID;
        internal static long CHMAPI_PluginInformationGoingToServer_EndTime;

        internal static long CHMAPI_InformationCommingFromServerServer_StartTime;
        internal static int CHMAPI_InformationCommingFromServerServer_PluginUniqueID;
        internal static long CHMAPI_InformationCommingFromServerServer_EndTime;

        internal static long CHMAPI_PluginInformationGoingToPlugin_StartTime;
        internal static int CHMAPI_PluginInformationGoingToPlugin_PluginUniqueID;
        internal static long CHMAPI_PluginInformationGoingToPlugin_EndTime;

        internal static long CHMAPI_PluginInformationCommingFromPlugin_StartTime;
        internal static int CHMAPI_PluginInformationCommingFromPlugin_PluginUniqueID;
        internal static long CHMAPI_PluginInformationCommingFromPlugin_EndTime;

        internal static long CHMAPI_Heartbeat_StartTime;
        internal static int CHMAPI_Heartbeat_PluginUniqueID;
        internal static long CHMAPI_Heartbeat_EndTime;

        internal static long CHMAPI_InitializePlugin_StartTime;
        internal static int CHMAPI_InitializePlugin_PluginUniqueID;
        internal static long CHMAPI_InitializePlugin_EndTime;

        internal static long CHMAPI_StartupInfoFromServer_StartTime;
        internal static int CHMAPI_StartupInfoFromServer_PluginUniqueID;
        internal static long CHMAPI_StartupInfoFromServer_EndTime;

        #endregion

        #region Local Watchdog Variables Stuff
        internal static DateTime MaintanenceTimerStart;
        internal static DateTime MaintanenceTimerEnd;
        internal static int MaintanenceTimerInterval;
        internal static long PluginTimerStart;
        internal static long PluginTimerEnd;
        internal static long HeartbeatTimerStart;
        internal static long HeartbeatTimerEnd;
        internal static long PluginWatchDogTimerStart;
        internal static long PluginWatchDogTimerEnd;
        internal static long DisplayTimerStart;
        internal static long DisplayTimerEnd;

        internal static long LastMaintanenceTimerStart = 0;
        internal static long LastPluginTimerStart = 0;
        internal static long LastHeartbeatTimerStart = 0;
        internal static long LastPluginWatchDogTimerStart = 0;
        internal static long LastDisplayTimerStart = 0;

        internal static long MaintanenceTimerSequence = 0;
        internal static long PluginTimerSequence = 0;
        internal static long HeartbeatTimerSequence = 0;
        internal static long PluginWatchDogTimerSequence = 0;
        internal static long DisplayTimerSequence = 0;

        #endregion

        #region Windows Form Stuff
        internal MainCHMForm()
        {
            InitializeComponent();
        }

        internal void AddToUnexpectedErrorQueue(Exception EMessage)
        {
            try
            {
                PluginErrorMessage PEM;

                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 2000002, EMessage + "\r\n\r\n", ""));
            }
            catch
            {

            }
        }

        static int RecoveryProcedure(RecoveryData parameter)
        {
            ArrImports.ApplicationRecoveryFinished(true);
            return 0;
        }

        private static void RegisterForRestart()
        {
            // Register for automatic restart if the application 
            // was terminated for any reason.
            ArrImports.RegisterApplicationRestart("/restart",
                (int)RestartRestrictions.None);
        }

        private static void RegisterForRecovery()
        {
            // Create the delegate that will invoke the recovery method.
            RecoveryDelegate recoveryCallback =
                 new RecoveryDelegate(RecoveryProcedure);
            uint pingInterval = 5000, flags = 0;
            RecoveryData parameter = new RecoveryData(Environment.UserName);

            // Register for recovery notification.
            int regReturn = ArrImports.RegisterApplicationRecoveryCallback(
                recoveryCallback,
                parameter,
                pingInterval,
                flags);
        }

        internal void MainCHMForm_Load(object sender, EventArgs e)
        {
            RegisterForRecovery();
            RegisterForRestart();

            PendingMessageQueue = new ConcurrentQueue<string>();
            ActionItemsListBoxQueue = new ConcurrentQueue<Tuple<string, ListBox, string, string>>();
            EventsItemsListBoxQueue = new ConcurrentQueue<Tuple<string, ListBox, string, string>>();
            OtherGUIBoxesQueue = new ConcurrentQueue<Tuple<string, ListBox, string>>();
            SavedMessageQueue = new ConcurrentQueue<string>();
            ErrorMessageQueue = new ConcurrentQueue<string>();
            DebugMessageQueue = new ConcurrentQueue<string>();
            LBToClearQueue = new ConcurrentQueue<ListBox>();
            IncedentFlagQueue = new ConcurrentQueue<Tuple<PluginIncedentFlags, object>>();
            ServerToPluginQueue = new ConcurrentQueue<PluginServerDataStruct>();
            SystemFlagsList = new List<SystemFlagStruct>();
            DoMathSlim = new SemaphoreSlim(1, 1);
            GetFlagsFromListSlim = new SemaphoreSlim(1, 1);
            ev = new Evaluator(Eval3.eParserSyntax.cSharp, false);
            EvalMathFunctions = new CHMPluginAPI.EvalFunctions();
            //                ev.AddEnvironmentFunctions(this);
            ev.AddEnvironmentFunctions(EvalMathFunctions);
            ArchiveFlagChangesQueue = new ConcurrentQueue<FlagArchiveStruct>();


            //ConfigureNLP();

            SysData = new SystemData(_GetCurrentTick, PendingMessageQueue);
            MainDB = new DatabaseAccess();
            //HTTPDB = new DatabaseAccess();
            ModuleDB = new DatabaseAccess();
            PluginDictionary = new ConcurrentDictionary<string, PluginStruct>();
            InterfaceDictionary = new Dictionary<string, InterfaceStruct>();
            DeviceDictionary = new Dictionary<string, DeviceStruct>();
            DeviceTemplateDictionary = new Dictionary<string, DeviceTemplateStruct>();
            PasswordDictionary = new Dictionary<string, PasswordStruct>();
            StatusMessagesDictionary = new Dictionary<string, StatusMessagesStruct>();
            MenuDeviceListFlagChanges = new ConcurrentQueue<Tuple<string, FlagDataStruct>>();


            RoomList = new List<Tuple<string, string, string, string>>();
            FlagAccess = new FlagData(_GetCurrentTick, SysData, ActionItemsListBox, EventsItemsListBox);
            SysData.AddFlagAccess(FlagAccess);

            //SysData Variable Initialization            

            SysData.Set("LocalMessageSequenceNumber", 0, 'X');
            SysData.Set("IsMessageDatabaseAvailable", false, 'X');
            SysData.Set("HaltAllOperations", false, 'N');
            SysData.Set("HTMLLoaded", false, 'X');
            SysData.Set("SessionCode", Guid.NewGuid().ToString("N") + _GetCurrentTick().ToString("X"), 'N');
            ImmediateCommands.Focus();






            //Timer To Update Main GUI
            MainGUIDispatchTimer = new DispatcherTimer();
            MainGUIDispatchTimer.Tick += new EventHandler(_ProcessMainGUIUpdate);
            MainGUIDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            MainGUIDispatchTimer.Start();

        }


        internal void MainCHMForm_Shown(object sender, EventArgs e)
        {
            StartupThread = new Thread(new ThreadStart(StartupThreadProcessing));
            StartupThread.Start();
        }

        internal void Exit_Click(object sender, EventArgs e)
        {
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 900000, "", ""));

            SysData._SaveQueuedValuesToDataBase(MainDB); //Write SysData Configuration Info to Database

            //Set Shutdown Flags
            ServerStatusCode = ServerStatusCodes.InShutdown;
            SysData.Set("HaltAllOperations", true);

            //Shutdown Timers
            if (MaintanenceTimer != null)
                MaintanenceTimer.Enabled = false;
            if (PluginTimer != null)
                PluginTimer.Enabled = false;
            if (HeartbeatTimer != null)
                HeartbeatTimer.Enabled = false;
            if (PluginWatchDogTimer != null)
                PluginWatchDogTimer.Enabled = false;
            if (DisplayTimer != null)
                DisplayTimer.Enabled = false;
            if (CHMWatchDogTimer != null)
                CHMWatchDogTimer.Enabled = false;
            if (IncedentFlagTimer != null)
                IncedentFlagTimer.Enabled = false;

            //Shutdown Main Thread          
            if (MainThread != null)
                MainThread.Abort();
            if (StartupThread != null)
                StartupThread.Abort();


            //Shutdown Plugins 
            PluginStruct PluginProcess;

            foreach (var pair in PluginDictionary)
            {
                PluginProcess = pair.Value;
                if (!PluginProcess.Initialized)
                {
                    continue;
                }
                try
                {
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_ShutDownPlugin-Invoke", 9999999, PluginProcess.PluginName, ""));
                    CHMAPI_ShutDownPlugin_StartTime = _GetCurrentTick();
                    CHMAPI_ShutDownPlugin_EndTime = 0;
                    CHMAPI_ShutDownPlugin_PluginUniqueID = PluginProcess.PluginUniqueID;
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_ShutDownPlugin", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, null);
                    CHMAPI_ShutDownPlugin_EndTime = _GetCurrentTick();
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_ShutDownPlugin-Return", 9999999, PluginProcess.PluginName, ""));
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 56, "", " (" + PluginProcess.PluginName + ")"));


                }
                catch (Exception err)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 53, err.Message, " (" + PluginProcess.PluginName + " CHMAPI_ShutDownPlugin )" + err.StackTrace));

                }
            }


            //Wait For Unfinished Processing
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 59, "", " (" + SysData.GetValueInt("Shutdown Delay", 10) + ")"));

            Thread.Sleep(SysData.GetValueInt("Shutdown Delay", 10) * 1000);

            //Write Logs
            SysData.WriteSysDataToLogFile(SysData.GetValue("LogFileLocation"), "F", SysData.GetValueInt("EndingLogVersions"));
            FlagAccess.WriteFlagDataToLogFile(SysData.GetValue("LogFileLocation"), "F", SysData.GetValueInt("EndingLogVersions"));
            _WriteMessageLogToFile(SysData.GetValue("LogFileLocation"), "F", SysData.GetValueInt("EndingLogVersions"));

            //RE-Write Startup FIle
            bool ValidData;
            var Reader = MainDB.ExecuteSQLCommandWithReader("StartupFile", "Language= \"" + SysData.GetValue("LanguageCode") + "\"", out ValidData);
            Reader = MainDB.GetNextRecordWithReader(ref Reader, out ValidData);
            string Filename = MainDB.GetStringFieldByReader(Reader, "FileName");
            Byte[] Blob;
            int BLobSize = MainDB.GetBlobFieldByReader(Reader, "StartupFile", out Blob);
            MainDB.CloseNextRecordWithReader(ref Reader);
            EncryptAndWriteToFile(Filename, BLobSize, Blob);

            //Bye-Bye 
            Environment.Exit(0);
        }
        #endregion


        #region Threads Functions
        void StartupThreadProcessing()
        {
            int a;
            string S, T;

            //Load Startup File
            try
            {
                ReadAndDecryptFile("CHMInit.001", out DecryptedStartupFile);

                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.SetAndReturnValue("StartupMessage", DecryptedStartupFile[0], 'X'), ""));

                SysData.Set("LanguageCode", DecryptedStartupFile[1], 'N');
                SysData.Set("LanguageName", DecryptedStartupFile[2], 'X');
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.SetAndReturnValue("LanguageStatement", DecryptedStartupFile[3], 'X'), ""));

                SysData.Set("DBLocation", DecryptedStartupFile[4], 'X');
                SysData.Set("DBOpenMessage", DecryptedStartupFile[5], 'X');

                SysData.Set("DBFailureStatement", DecryptedStartupFile[6], 'X');
                EncryptedMasterPassword = DecryptedStartupFile[7];
                SysData.Set("NetWorkWaitingStatement", DecryptedStartupFile[8], 'X');
                SysData.Set("EnterDatabasePasswordStatement", DecryptedStartupFile[9], 'X');
                SysData.Set("TerminateProgramStatement", DecryptedStartupFile[10], 'X');

            }
            catch (Exception err)
            {
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, "Cannot Open 'CHMInit.001' File", ""));
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, err.Message, ""));
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, "Hit <EXIT> to Terminate Program", ""));

                SysData.Set("HaltAllOperations", true);
                return;
            }

            //Verify that a wireless or ethernet network is available
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("NetWorkWaitingStatement"), ""));


            while (true)
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

                bool flag = false;
                foreach (System.Net.NetworkInformation.NetworkInterface Adaptor in adapters)
                {
                    if (Adaptor.OperationalStatus == OperationalStatus.Down)
                        continue;
                    string SA = Adaptor.NetworkInterfaceType.ToString().ToLower();
                    if (SA.IndexOf("ethernet") == -1 && SA.IndexOf("wireless") == -1)
                        continue;

                    IPInterfaceProperties properties = Adaptor.GetIPProperties();
                    if (properties.GatewayAddresses.Count == 0)
                        continue;
                    flag = true;

                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            FlagAccess.Set("Machine IP Address", "", ip.ToString(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, 0);
                            break;
                        }
                    }
                }
                if (flag)
                    break;
                System.Threading.Thread.Sleep(1000);
            }

            MachineName= System.Environment.MachineName;
            FlagAccess.Set("Machine Name", "", MachineName, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, 0);

            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("DBOpenMessage"), ""));

            S = "";
            int ENC = MainDB.OpenMainDB(SysData.GetValue("DBLocation"), ref S, EncryptedMasterPassword);

            if (ENC == -1) //Cannot OPEN Main Database
            {
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("DBFailureStatement"), "Main"));

                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("TerminateProgramStatement"), ""));

                SysData.Set("HaltAllOperations", true);
                return;
            }
            if (ENC >= 0)
            {
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 1, "", S));

            }


            if (ENC == -3)
            {
                ChangePasswordForm EnterPassword = new ChangePasswordForm(SysData.GetValue("EnterDatabasePasswordStatement"), false);
                EnterPassword.ShowDialog();
                EnterPassword.Close();
            }

            if (ENC == -2)
            {
                while (true)
                {
                    EnterPasswordForm EnterPassword = new EnterPasswordForm(SysData.GetValue("EnterDatabasePasswordStatement"));
                    EnterPassword.ShowDialog();
                    EnterPassword.Close();
                    if (!EnterPassword.Saved)
                    {
                        PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("DBFailureStatement"), ""));

                        PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("TerminateProgramStatement"), ""));

                        SysData.Set("HaltAllOperations", true);
                        return;
                    }
                    EncryptedMasterPassword = Encrypt(EnterPassword.EnteredPassword);
                    if (MainDB.OpenMainDB(SysData.GetValue("DBLocation"), ref S, EncryptedMasterPassword) >= 0)
                    {
                        PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 1, "", S));

                        ResetDatabasePasswordFile(EncryptedMasterPassword);
                        break;
                    }

                }
            }
            //if (HTTPDB.OpenMainDB(SysData.GetValue("DBLocation"), ref S, EncryptedMasterPassword) >= 0)
            //{
            //    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 10, "", S));

            //}
            //else
            //{
            //    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("DBFailureStatement"), "HTTP"));

            //    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 0, SysData.GetValue("TerminateProgramStatement"), ""));

            //    SysData.Set("HaltAllOperations", true);
            //    return;
            //}

            //Lookup SysData Values From MainDatabase
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 4, "", ""));
            bool ValidData;
            string[] Stuff;
            var SQL = MainDB.ExecuteSQLCommandWithReaderandFields("Configuration", "*", "ModuleSerialNumber = " + DatabaseAccess.DATABASEQUOTE + MODULESERIALNUMBER + DatabaseAccess.DATABASEQUOTE + " and fieldname= " + DatabaseAccess.DATABASEQUOTE + "TimeZone" + DatabaseAccess.DATABASEQUOTE, out ValidData);
            if (ValidData)
            {
                MainDB.GetNextRecordWithReader(ref SQL, out Stuff, out ValidData);
                SysData._SetX(Stuff[1], Stuff[1], Stuff[3], Convert.ToChar(Stuff[4]), true);
            }

            UTCOffsetTicks = _UTCOffset();
            FlagAccess.Set("UTCOffset", "", ((int)(Math.Abs(UTCOffsetTicks) / TimeSpan.TicksPerHour)) * Math.Sign(UTCOffsetTicks), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, 0);
            SysData.LoadConfigurationDataFromDatabase(MainDB);


            S = SysData.GetValue("DebugCode");
            if (string.IsNullOrEmpty(S))
                S = "0";
            S = S + "00000000000000000000000000000000000000000000000000000";
            S = S.Substring(0, 50);
            DebugSettings = new bool[S.Length];
            for (a = 0; a < S.Length; a++)
            {
                if (S[a] == '0')
                    DebugSettings[a] = false;
                else
                    DebugSettings[a] = true;
            }

            FlagData.FlagChangeHistoryMaxSize = SysData.GetValueInt("FlagChangeHistoryMaxSize", 1000);


            // Writeto error EventLog when sysdata is not found

            //Load Internal Tables, such as rooms


            _LoadAllInternalTables();
            //Load Independent Plugins 

            //Setup Dynamic Menu
            ToolStripMenuItem Mitem = new ToolStripMenuItem();

            //Devices Menu
            S = SysData.GetValue("MenuDeviceNumber");
            if (!string.IsNullOrEmpty(S))
            {
                string[] SX = S.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int i1, i2;
                Int32.TryParse(SX[0], out i1);
                Int32.TryParse(SX[1], out i2);
                S = "";
                T = "";
                MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, i1, ref S, ref T);


                Mitem.Text = S;

                for (int i = i1 + 1; i <= i2; i++)
                {
                    ToolStripMenuItem t1 = new ToolStripMenuItem();
                    if (MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, i, ref S, ref T) == 0)
                    {
                        t1.Text = S;
                        t1.Tag = i.ToString();

                        t1.Click += DevicesMenuItem_Click;
                        Mitem.DropDownItems.Add(t1);
                    }
                }
                MainMenuStrip.Items.Add(Mitem);
            }

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            
            FlagAccess.Set("Module 0000", "Name", Path.GetFileName(fvi.FileName), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
            FlagAccess.Set("Module 0000", "Serial Number", MODULESERIALNUMBER, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
            FlagAccess.Set("Module 0000", "Version", fvi.FileVersion, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
            FlagAccess.Set("Module 0000", "Description", fvi.FileDescription, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);


            //Populate Plugin Dictionary with Valid and Ignored Plugins
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 40, "", ""));

            string[] Independent;
            bool IndependentValidData;
            var IndependentReader = MainDB.ExecuteSQLCommandWithReader("PluginReference", "Ignore='I'", out IndependentValidData);
            IndependentReader = MainDB.GetNextRecordWithReader(ref IndependentReader, out Independent, out IndependentValidData);

            while (IndependentValidData)
            {
                _InstallAndInitializePlugin(SysData.GetValue("PluginLocation"), Independent[0], "", "PluginReference", Independent);
                IndependentReader = MainDB.GetNextRecordWithReader(ref IndependentReader, out Independent, out IndependentValidData);
            }
            MainDB.CloseNextRecordWithReader(ref IndependentReader);





            //Tell all the Plugin-startup To Do Any Final Initializations
            PluginStruct PluginProcess;
            foreach (var pair in PluginDictionary)
            {
                //                  fixme;
                PluginProcess = pair.Value;
                if (!PluginProcess.Initialized)
                {
                    continue;
                }
                try
                {
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginStartupInitialize-Invoke", 9999999, PluginProcess.PluginName, ""));
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginStartupInitialize", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, null);
                    int LastMessage = 0;
                    string LastSuffix = "";
                    int NumberOfLoopsSinceLastUpdate = 0;
                    int LastAliveSequence = 0;
                    while (true)
                    {
                        Thread.Sleep(1000);
                        NumberOfLoopsSinceLastUpdate++;
                        //var us = PluginProcess.ServerAssemblyType.GetField("PluginStatus").GetValue(PluginProcess.Plugin);

                        PluginProcess.Status = (PluginStatusStruct)PluginProcess.ServerAssemblyType.GetField("PluginStatus").GetValue(PluginProcess.Plugin);

                        if (PluginProcess.Status.StartupInitializedMessage != LastMessage || (!string.IsNullOrEmpty(PluginProcess.Status.StartupInitializedMessageSuffix) && PluginProcess.Status.StartupInitializedMessageSuffix != LastSuffix))
                        {
                            LastMessage = PluginProcess.Status.StartupInitializedMessage;
                            LastSuffix = PluginProcess.Status.StartupInitializedMessageSuffix;
                            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), PluginProcess.DBSerialNumber, LastMessage, "", LastSuffix));

                        }
                        if (PluginProcess.Status.StartupInitializedFinished == true)
                        {
                            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, "42", PluginProcess.DBSerialNumber, "(" + PluginProcess.PluginName + ")"));
                            break;
                        }
                    }




                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginStartupInitialize-Return", 9999999, PluginProcess.PluginName, ""));

                }
                catch (Exception err)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 53, err.Message, " (" + PluginProcess.PluginName + " CHMAPI_PluginStartupInitialize) " + err.StackTrace));
                }
            }

            //Final Startup Stuff

            StartTime = new Stopwatch();
            SysData.Set("SystemStartTick", _GetCurrentTick(), 'N');
            StartTime.Start();

            //Save Startup Data as Config Data             
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 32, "", ""));

            SysData.UpdateConfigInformation();


            S = SysData.GetValue("SaveLogsCode");  //E=End Only, H=Hourly S=Startup SH=Startup & Hourly
            if (S.IndexOf('S') != -1)
            {
                SysData.WriteSysDataToLogFile(SysData.GetValue("LogFileLocation"), "S", SysData.GetValueInt("EndingLogVersions"));
                FlagAccess.WriteFlagDataToLogFile(SysData.GetValue("LogFileLocation"), "S", SysData.GetValueInt("EndingLogVersions"));
                _WriteMessageLogToFile(SysData.GetValue("LogFileLocation"), "S", SysData.GetValueInt("EndingLogVersions"));
            }


            //Start Message Threads
            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 31, "", ""));


            //Tell all the Plugin-startup Complete That We Are finished Setting Up
            foreach (var pair in PluginDictionary)
            {
                PluginProcess = pair.Value;
                if (!PluginProcess.Initialized)
                {
                    continue;
                }
                try
                {
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_ServerFunctions-Invoke", 9999999, PluginProcess.PluginName, ""));

                    PluginDirectCalls PD = new PluginDirectCalls(PluginProcess.PluginName);
                    ServerFunctionsStruct SCF = new ServerFunctionsStruct();
                    SCF.GetFlags = new ServerFunctionsStruct.ServerGetFlagsInListDelegate(PD.ServerGetFlagsInList);
                    SCF.GetSingleFlag = new ServerFunctionsStruct.ServerGetFlagDelegate(PD.ServerGetFlag);
                    SCF.GetSingleFlagFromServerFull = new ServerFunctionsStruct.GetSingleFlagFromServerFullDelegate(PD.GetSingleFlagFromServerFull);
                    SCF.GetMacro = new ServerFunctionsStruct.GetMacroDelegate(PD.GetMacro);
                    SCF.GetAutomation = new ServerFunctionsStruct.GetAutomationDelegate(PD.GetAutomation);

        SCF.RunDirectCommand = new ServerFunctionsStruct.RunDirectCommandDelegate(PD.RunDirectCommand);
                    SCF.GetDeviceFromDB = new ServerFunctionsStruct.GetDeviceFromDBDelegate(PD.GetDeviceFromDB);
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_ServerFunctions", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { SCF });

                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_ServerFunctions-Return", 9999999, PluginProcess.PluginName, ""));

                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginStartupCompleted-Invoke", 9999999, PluginProcess.PluginName, ""));
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginStartupCompleted", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, null);
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginStartupCompleted-Return", 9999999, PluginProcess.PluginName, ""));

                    if (PluginProcess.OriginalDataBaseInfo[4].ToUpper()=="COMMAND")
                    {
                        PluginProcess.ServerAssemblyType = PluginProcess.Plugin.GetType("CHMModules.AutomationProcesses");
                        PluginProcess.ServerInstance = PluginProcess.ServerAssemblyType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null);

                        object[] DeviceDatastreamProcesses = new object[] { FlagData.FlagDataDictionary, DeviceDictionary, RoomList };
                        PluginProcess.ServerAssemblyType.InvokeMember("CommandProcessing_ServerLinks", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, DeviceDatastreamProcesses);
                    }
                }
                catch (Exception err)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 53, err.Message, " (" + PluginProcess.PluginName + " CHMAPI_PluginStartupCompleted) " + err.StackTrace));
                }
            }


            //Main Thread Startup
            MainThread = new Thread(new ThreadStart(MainThreadProcessing));
            MainThread.Start();


            ServerStatusCode = ServerStatusCodes.Running;
            //            Invoke(new Action(() => { MCH_Form.WindowState = FormWindowState.Minimized; }));

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        //int CHMServerEventHandler(Int32 Event, ref IntPtr Cookie, ref IntPtr CookieSize, ref IntPtr Headders, ref IntPtr HeadderSize, ref IntPtr PageData, ref IntPtr PageDataSize, ref IntPtr URI, ref IntPtr URISize, ref IntPtr Type, ref IntPtr TypeSize)
        //{
        //    byte[] arr = new byte[999999];
        //    Int64 HSize, CSize, PDSize, URSize;
        //    byte[] LCookie = new byte[999];
        //    byte[] LHeadders = new byte[99999];
        //    byte[] LPageData = new byte[999999];
        //    byte[] LURI = new byte[2999];

        //    CSize = CookieSize.ToInt64();
        //    HSize = HeadderSize.ToInt64();
        //    PDSize = PageDataSize.ToInt64();
        //    URSize = URISize.ToInt64();

        //    bool fl;

        //    string SURI = Marshal.PtrToStringAnsi(URI, (Int32)URSize);
        //    if (SURI[0] == '/')
        //        SURI = SURI.Substring(1);
        //    string SessionID = "";
        //    HTMLSessionData LocalHTMLSessionData, OldLocalHTMLSessionData;

        //    Console.WriteLine(Event.ToString() + " " + SURI);
        //    if (Event == 102)
        //    {
        //        if (CSize > 0)
        //        {
        //            SessionID = Marshal.PtrToStringAnsi(Cookie, (Int32)CSize);
        //            if (!HTMLSessionTable.TryGetValue(SessionID, out LocalHTMLSessionData))
        //                CSize = 0;
        //            else//Check for Timeout
        //            {
        //                Console.WriteLine(((_GetCurrentTick() - LocalHTMLSessionData.TickAtLastActivity) / TimeSpan.TicksPerSecond).ToString());
        //                if ((_GetCurrentTick() - LocalHTMLSessionData.TickAtLastActivity) / TimeSpan.TicksPerSecond > SysData.GetValueInt("HTTPSessionTimeout", 300))
        //                {
        //                    HTMLSessionTable.TryRemove(SessionID, out LocalHTMLSessionData);
        //                    CSize = 0;
        //                }
        //            }
        //        }


        //        if (CSize == 0)
        //        {
        //            Console.WriteLine("Old Session '" + SessionID + "'");

        //            Guid g = Guid.NewGuid();
        //            SessionID = Convert.ToBase64String(g.ToByteArray());
        //            SessionID = SessionID.Replace("=", "");
        //            SessionID = SessionID.Replace("+", "");
        //            SURI = SysData.GetValue("HTMLLoginPage");
        //            for (int i = 0; i < SURI.Length; i++)
        //            {
        //                arr[i] = (byte)SURI[i];
        //            }
        //            Marshal.Copy(arr, 0, URI, SURI.Length);
        //            URISize = new IntPtr(SURI.Length);
        //            LocalHTMLSessionData = new HTMLSessionData();
        //            LocalHTMLSessionData.SessionID = SessionID;
        //            LocalHTMLSessionData.Loggedin = false;
        //            LocalHTMLSessionData.LastPageAccessed = "";
        //            LocalHTMLSessionData.CurrentPageRequested = SURI;
        //            LocalHTMLSessionData.PageAfterLogin = "";
        //            LocalHTMLSessionData.LastPageAccessed = SURI;
        //            LocalHTMLSessionData.TickAtLastActivity = _GetCurrentTick();
        //            HTMLSessionTable.TryAdd(SessionID, LocalHTMLSessionData);
        //            Console.WriteLine("New Session " + SessionID);
        //            for (int i = 0; i < SessionID.Length; i++)
        //            {
        //                arr[i] = (byte)SessionID[i];
        //            }
        //            Marshal.Copy(arr, 0, Cookie, SessionID.Length);
        //            CookieSize = new IntPtr(SessionID.Length);
        //            return (0);
        //        }
        //    }
        //    if (Event == 102) //It Is Authenticated
        //    {
        //        return (1);
        //    }

        //    if (Event == 106)//Close
        //    {
        //        return (1);
        //    }

        //    if (Event == 103)
        //    {
        //        string S = "";
        //        if (CSize > 0)
        //        {
        //            SessionID = Marshal.PtrToStringAnsi(Cookie, (Int32)CSize);
        //        }
        //        else
        //            SURI = "";

        //        if (string.IsNullOrEmpty(SURI))
        //        {
        //            SURI = SysData.GetValue("HTMLLoginPage");
        //            for (int i = 0; i < SURI.Length; i++)
        //            {
        //                arr[i] = (byte)SURI[i];
        //            }
        //            Marshal.Copy(arr, 0, URI, SURI.Length);
        //            URISize = new IntPtr(SURI.Length);
        //        }

        //        string FileExtension = Path.GetExtension(SURI).ToLower();
        //        if (string.IsNullOrEmpty(FileExtension))
        //            FileExtension = ".html";
        //        string FileName = Path.GetFileNameWithoutExtension(SURI).ToLower();

        //        byte[] Pagearr;
        //        int flag = HTTPDB.GetHTMLObjectByName(FileName + FileExtension, "", out Pagearr);

        //        if (flag < 0)
        //        {
        //            {
        //                PageDataSize = new IntPtr(0);
        //                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 9, "", FileName + FileExtension));
        //                if (FileExtension != ".html")
        //                    return (99);
        //            }
        //            flag = HTTPDB.GetHTMLObjectByName(SysData.GetValue("HTMLHomePage"), "", out Pagearr);
        //        }

        //        if (flag > 0)
        //        {
        //            Marshal.Copy(Pagearr, 0, PageData, flag);
        //            PageDataSize = new IntPtr(flag);
        //        }
        //        else
        //            return (99);
        //        try
        //        {
        //            string T = HTMLObjectTypes[FileExtension].ToLower();
        //            for (int i = 0; i < T.Length; i++)
        //            {
        //                arr[i] = (byte)T[i];
        //            }
        //            Marshal.Copy(arr, 0, Type, T.Length);
        //            TypeSize = new IntPtr(T.Length);
        //        }
        //        catch
        //        {
        //            TypeSize = new IntPtr(0);
        //        }
        //        if (FileExtension == ".html" && SURI.ToLower() != SysData.GetValue("HTMLLoginPage").ToLower())
        //        {
        //            if (HTMLSessionTable.TryGetValue(SessionID, out LocalHTMLSessionData))
        //            {
        //                OldLocalHTMLSessionData = new HTMLSessionData();
        //                OldLocalHTMLSessionData = LocalHTMLSessionData;
        //                LocalHTMLSessionData.LastPageAccessed = LocalHTMLSessionData.CurrentPageRequested;
        //                LocalHTMLSessionData.CurrentPageRequested = SURI;
        //                LocalHTMLSessionData.TickAtLastActivity = _GetCurrentTick();
        //                fl = HTMLSessionTable.TryUpdate(SessionID, LocalHTMLSessionData, OldLocalHTMLSessionData);
        //            }
        //        }



        //        return (2);
        //    }

        //    if (Event == 105)
        //    {
        //        return (-1);
        //    }

        //    return (-1);
        //}


        void MainThreadProcessing()
        {
            //Starting Timers
            MaintanenceTotalTime = new Stopwatch();
            MaintanenceTimer = new System.Timers.Timer(SysData.GetValueInt("MaintanenceTimerTimeslice", 100));
            MaintanenceTimer.Elapsed += new ElapsedEventHandler(MaintanenceTimerProcess);
            MaintanenceTimer.Interval = SysData.GetValueInt("MaintanenceTimerTimeslice", 100);
            MaintanenceTimerInterval = (int) MaintanenceTimer.Interval;
            MaintanenceTimerLastTick = -1;
            MaintanenceTimerLastLogSave = _GetCurrentTick();
            MaintanenceTimerChangedMinute = true;
            MaintanenceTimerLastDate = DateTime.MinValue;
            MaintanenceTimer.Enabled = true;

            PluginTotalTime = new Stopwatch();
            PluginTimer = new System.Timers.Timer(SysData.GetValueInt("PluginTimerTimeslice", 100));
            PluginTimer.Elapsed += new ElapsedEventHandler(PluginTimerProcess);
            PluginTimer.Interval = SysData.GetValueInt("PluginTimerTimeslice", 100);
            PluginTimerLastLogSave = _GetCurrentTick();
            PluginTimer.Enabled = true;

            HeartbeatTotalTime = new Stopwatch();
            HeartbeatTimer = new System.Timers.Timer(SysData.GetValueInt("HeartbeatInterval", 100));
            HeartbeatTimer.Elapsed += new ElapsedEventHandler(HeartbeatTimerProcess);
            HeartbeatTimer.Interval = SysData.GetValueInt("HeartbeatInterval", 100);
            HeartbeatTimerLastLogSave = _GetCurrentTick();
            HeartbeatTimer.Enabled = true;
            HeartBeatLastProcess = _GetCurrentDateTime();

            PluginWatchdogTotalTime = new Stopwatch();
            PluginWatchDogTimer = new System.Timers.Timer(SysData.GetValueInt("PluginWatchDogTimerTimeslice", 5000));
            PluginWatchDogTimer.Elapsed += new ElapsedEventHandler(PluginWatchDogTimerProcess);
            PluginWatchDogTimer.Interval = SysData.GetValueInt("PluginWatchDogTimerTimeslice", 5000);
            PluginWatchDogTimerLastLogSave = _GetCurrentTick();
            PluginWatchDogTimer.Enabled = true;

            DisplayTimer = new System.Timers.Timer(1000);
            DisplayTimer.Elapsed += new ElapsedEventHandler(DisplayTimerProcess);
            DisplayTimer.Interval = 1000;
            DisplayTimer.Enabled = true;

            CHMWatchDogTimer = new System.Timers.Timer(SysData.GetValueInt("CHMWatchDogTimerTimeslice", 60000));
            CHMWatchDogTimer.Elapsed += new ElapsedEventHandler(CHMWatchDogTimerProcess);
            CHMWatchDogTimer.Interval = SysData.GetValueInt("CHMWatchDogTimerTimeslice", 60000);
            CHMWatchDogTimer.Enabled = true;

            IncedentFlagTimer = new System.Timers.Timer(SysData.GetValueInt("IncedentFlagTimer", 100));
            IncedentFlagTimer.Elapsed += new ElapsedEventHandler(IncedentFlagTimerProcess);
            IncedentFlagTimer.Interval = SysData.GetValueInt("IncedentFlagTimer", 100);
            IncedentFlagTimer.Enabled = true;
            
        }

        #endregion

        #region Timer Functions

        internal void DisplayTimerProcess(object source, ElapsedEventArgs e)
        {
            string S;
            long T = 0;

            //Pause Timer;
            DisplayTimer.Enabled = false;
            DisplayTimerStart = _GetCurrentTick();
            DisplayTimerEnd = -1;
            DateTime WT = new DateTime(_GetCurrentTick());

            DisplayLastProcess = WT;
            int ses = StartTime.Elapsed.Seconds;

            S = string.Format("Time Running {0} Days  {1} Hours  {2} Minutes  {3} Seconds", StartTime.Elapsed.Days,
                StartTime.Elapsed.Hours, StartTime.Elapsed.Minutes, ses);
            OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, ElapsedTime, "Time Running"));

            if (ses < DisplayTimesliceLastProcess)
            {
                S = string.Format("WatchDog Processes {0,5:#0.00}", (double)PluginWatchdogTotalTime.ElapsedMilliseconds / (double)1000.0);
                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, TimeSliceStatus, "WatchDog Processes"));
                Interlocked.Add(ref T, PluginWatchdogTotalTime.ElapsedMilliseconds);
                PluginWatchdogTotalTime.Reset();

                S = string.Format("Heartbeat Processes {0,5:#0.00}", (double)HeartbeatTotalTime.ElapsedMilliseconds / (double)1000.0);
                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, TimeSliceStatus, "Heartbeat Processes"));
                Interlocked.Add(ref T, HeartbeatTotalTime.ElapsedMilliseconds);
                HeartbeatTotalTime.Reset();

                S = string.Format("Maintenance Processes {0,5:#0.00}", (double)MaintanenceTotalTime.ElapsedMilliseconds / (double)1000.0);
                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, TimeSliceStatus, "Maintenance Processes"));
                Interlocked.Add(ref T, MaintanenceTotalTime.ElapsedMilliseconds);
                MaintanenceTotalTime.Reset();

                S = string.Format("Plugin Processes {0,5:#0.00}", (double)PluginTotalTime.ElapsedMilliseconds / (double)1000.0);
                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, TimeSliceStatus, "Plugin Processes"));
                Interlocked.Add(ref T, PluginTotalTime.ElapsedMilliseconds);
                PluginTotalTime.Reset();

                S = string.Format("Idle Processes {0,5:#0.00}", (double)(60000 - T) / (double)1000.0);
                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, TimeSliceStatus, "Idle Process"));

            }

            DisplayTimesliceLastProcess = ses;
            //Restart Timer and exit
            DisplayTimerEnd = _GetCurrentTick();
            DisplayTimer.Enabled = true;
            return;
        }

        internal void IncedentFlagTimerProcess(object source, ElapsedEventArgs e)
        {
            //Pause Timer;
            IncedentFlagTimer.Enabled = false;
            Tuple<PluginIncedentFlags, object> Incedent;

            while (IncedentFlagQueue.TryDequeue(out Incedent))
            {
                for (int index = 0; index < PluginDictionary.Count; index++) //Plugin Loops
                {
                    var a = PluginDictionary.ElementAt(index);
                    if (a.Value.SendAllIncedentFlags)
                    {
                        try
                        {
                            a.Value.ServerAssemblyType.InvokeMember("CHMAPI_IncedentFlag", BindingFlags.InvokeMethod, null, a.Value.ServerInstance, new object[] { Incedent.Item1, Incedent.Item2 });

                        }
                        catch
                        {

                        }
                        continue;
                    }

                    if (a.Value.SendJustFlagChangeIncedentFlags && Incedent.Item1 == PluginIncedentFlags.FlagChange)
                    {
                        try
                        {
                            a.Value.ServerAssemblyType.InvokeMember("CHMAPI_IncedentFlag", BindingFlags.InvokeMethod, null, a.Value.ServerInstance, new object[] { Incedent.Item1, Incedent.Item2 });

                        }
                        catch
                        {

                        }
                        continue;
                    }
                }

            }



            //Restart Timer and exit
            IncedentFlagTimer.Enabled = true;
            return;
        }

        internal void CHMWatchDogTimerProcess(object source, ElapsedEventArgs e)
        {

            //Pause Timer;
            CHMWatchDogTimer.Enabled = false;

            if (DebugSettings[13])
                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Overall Watchdog-Start", 9999999, "", ""));

            //Check for Overtime Error


            //if (LastMaintanenceTimerStart != MaintanenceTimerStart)
            //{
            //    LastMaintanenceTimerStart = MaintanenceTimerStart;
            //}
            //else //This has hung up
            //{

            //}

            //if (LastPluginTimerStart != PluginTimerStart)
            //{
            //    LastPluginTimerStart = PluginTimerStart;
            //}
            //else //This has hung up
            //{
            //    PluginTimer.Enabled = true;
            //}

            //if (LastHeartbeatTimerStart != HeartbeatTimerStart)
            //{
            //    LastHeartbeatTimerStart = HeartbeatTimerStart;
            //}
            //else //This has hung up
            //{

            //}

            //if (LastPluginWatchDogTimerStart != PluginWatchDogTimerStart)
            //{
            //    LastPluginWatchDogTimerStart = PluginWatchDogTimerStart;
            //}
            //else //This has hung up
            //{

            //}

            //if (LastDisplayTimerStart != DisplayTimerStart)
            //{
            //    LastDisplayTimerStart = DisplayTimerStart;
            //}
            //else //This has hung up
            //{

            //}


            if (DebugSettings[13])
                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Overall Watchdog-End", 9999999, "", ""));


            //Restart Timer and exit
            CHMWatchDogTimer.Enabled = true;
            return;

        }

        internal void PluginWatchDogTimerProcess(object source, ElapsedEventArgs e)
        {

            PluginStruct PluginProcess;

            //Pause Timer;
            PluginWatchDogTimer.Enabled = false;
            PluginWatchDogTimerStart = _GetCurrentTick();
            PluginWatchDogTimerEnd = -1;

            PluginWatchdogTotalTime.Start();
            DateTime WT = new DateTime(_GetCurrentTick());

            foreach (var pair in PluginDictionary)
            {
                PluginProcess = pair.Value;
                if (!PluginProcess.Initialized || PluginProcess.Ignore)
                {
                    continue;
                }
                try
                {
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_WatchdogProcess-Invoke", 9999999, PluginProcess.PluginName, ""));
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_WatchdogProcess", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, null);
                    if (DebugSettings[2])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_WatchdogProcess-Return", 9999999, PluginProcess.PluginName, ""));

                    PluginProcess.Status = (PluginStatusStruct)PluginProcess.ServerAssemblyType.GetField("PluginStatus").GetValue(PluginProcess.Plugin);

                    AddToStatusDisplay(PluginProcess);

                    while (PluginProcess.Status.UEErrors) //There are errors to Process
                    {
                        //                        PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), PluginProcess.DBSerialNumber, PSS.CommandNumber, PSS.String, " (" + PluginProcess.PluginName + " " + PSS.String2 + ")"));

                    }
                }
                catch (Exception err)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 53, err.Message, " (" + PluginProcess.PluginName + " CHMAPI_WatchdogProcess) " + err.StackTrace));
                }
            }



            PluginWatchDogLastProcess = WT;
            PluginWatchdogTotalTime.Stop();

            //Restart Timer and exit
            PluginWatchDogTimerEnd = _GetCurrentTick();
            PluginWatchDogTimer.Enabled = true;
            return;
        }



        internal void MaintanenceTimerProcess(object source, ElapsedEventArgs e)
        {
            string S;

            //Pause Timer;
            MaintanenceTimerStart = _GetCurrentDateTime();
            int LastInterval = new TimeSpan(MaintanenceTimerStart.Ticks - MaintanenceTimerEnd.Ticks).Milliseconds;
            if (DebugSettings[12])
                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Maintenance Timer-Start", 9999999, "", ""));

            MaintanenceTotalTime.Start();
            
            //Check and Process Queues
            //Write System Message
            SysData._SaveQueuedValuesToDataBase(MainDB); //Write SysData Configuration Info to Database

            //Check and Process Time Changes


            if (MaintanenceTimerStart.IsDaylightSavingTime() != MaintanenceTimerLastDate.IsDaylightSavingTime()) //Check for Daylight Savings Time Change
            {
                UTCOffsetTicks = _UTCOffset();
                FlagAccess.Set("UTCOffset", "", ((int)(Math.Abs(UTCOffsetTicks) / TimeSpan.TicksPerHour)) * Math.Sign(UTCOffsetTicks), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, 0);
            }


            if (MaintanenceTimerStart.Second!= MaintanenceTimerLastDate.Second) //The Seconds Have Changed
            {
                FlagAccess.Set("CurrentSecond", "", MaintanenceTimerStart.Second.ToString("#0"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentMinute", "", MaintanenceTimerStart.Minute.ToString("#0"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentHour", "", MaintanenceTimerStart.Hour.ToString("#0"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentDay", "", MaintanenceTimerStart.Day, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentMonth", "", MaintanenceTimerStart.Month, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentMonthName", "", MaintanenceTimerStart.DayOfWeek.ToString(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentYear", "", MaintanenceTimerStart.Year, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentDayOfYear", "", MaintanenceTimerStart.DayOfYear, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentMonthDaysInMOnth", "", System.DateTime.DaysInMonth(MaintanenceTimerStart.Year, MaintanenceTimerStart.Month), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentMonthName", "", MaintanenceTimerStart.ToString("MMMM"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentIsDaylightSavingsTime", "", MaintanenceTimerStart.IsDaylightSavingTime(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentDayOfWeekName", "", MaintanenceTimerStart.DayOfWeek.ToString(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentDayOfWeekNumber", "", (int)MaintanenceTimerStart.DayOfWeek, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentDate", "", MaintanenceTimerStart.Date.ToShortDateString(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentSortableDateTime", "", MaintanenceTimerStart.ToString("yyyyMMddHHmmss"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentTime", "", MaintanenceTimerStart.ToString("HH':'mm':'ss"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentTick", "", MaintanenceTimerStart.Ticks.ToString(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set("CurrentFullDate", "", MaintanenceTimerStart.DayOfWeek.ToString()+" "+ MaintanenceTimerStart.Date.ToShortDateString()+" " + MaintanenceTimerStart.ToString("HH':'mm':'ss"), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);

                if (MaintanenceTimerStart.Ticks - MaintanenceTimerLastDate.Ticks > (TimeSpan.TicksPerSecond * 2) || MaintanenceTimerStart.Second == 0 || MaintanenceTimerStart.Second == 1 || MaintanenceTimerChangedMinute == true)
                {
                    FlagAccess.Set("CurrentTimeChangeMinute", "", MaintanenceTimerLastDate.Minute != MaintanenceTimerStart.Minute, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                    FlagAccess.Set("CurrentTimeChangeHour", "", MaintanenceTimerLastDate.Hour != MaintanenceTimerStart.Hour, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                    FlagAccess.Set("CurrentTimeChangeMonth", "", MaintanenceTimerLastDate.Month != MaintanenceTimerStart.Month, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                    FlagAccess.Set("CurrentTimeChangeYear", "", MaintanenceTimerLastDate.Year != MaintanenceTimerStart.Year, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                    FlagAccess.Set("CurrentTimeChangeIsDaylightSavingsTime", "", MaintanenceTimerLastDate.IsDaylightSavingTime() != MaintanenceTimerStart.IsDaylightSavingTime(), FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                    if (MaintanenceTimerStart.Ticks - MaintanenceTimerLastDate.Ticks > TimeSpan.TicksPerSecond || MaintanenceTimerStart.Second == 0)
                        MaintanenceTimerChangedMinute = true;
                    else
                        MaintanenceTimerChangedMinute = false;

                }

                S = SysData.GetValue("SaveLogsCode");  //E=End Only, H=Hourly S=Startup SH=Startup & Hourly
                if (S.IndexOf('H') != -1 && MaintanenceTimerStart.Ticks - MaintanenceTimerLastLogSave > TimeSpan.TicksPerHour)
                {
                    SysData.WriteSysDataToLogFile(SysData.GetValue("LogFileLocation"), "H", SysData.GetValueInt("SavedLogVersions"));
                    FlagAccess.WriteFlagDataToLogFile(SysData.GetValue("LogFileLocation"), "H", SysData.GetValueInt("SavedLogVersions"));
                    _WriteMessageLogToFile(SysData.GetValue("LogFileLocation"), "H", SysData.GetValueInt("SavedLogVersions"));
                    MaintanenceTimerLastLogSave = MaintanenceTimerStart.Ticks;
                }

                if (MaintanenceTimerStart.Ticks - MaintanenceTimerLastDate.Ticks > (TimeSpan.TicksPerSecond * 3))
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 57, "", ((MaintanenceTimerStart.Ticks - MaintanenceTimerLastDate.Ticks) / TimeSpan.TicksPerSecond)) + " Seconds");

                }

                MaintanenceTimerLastDate = MaintanenceTimerStart;
                MaintanenceTimerLastTick = MaintanenceTimerStart.Ticks;
            }

            MaintanenceTotalTime.Stop();

            if (DebugSettings[12])
                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Maintenance Timer-End", 9999999, "", ""));
            //Restart Timer and exit
            MaintanenceTimerEnd = _GetCurrentDateTime();
            TimeSpan duration = MaintanenceTimerEnd - MaintanenceTimerStart;
            MaintanenceTimer.Interval = Math.Min(Math.Max((MaintanenceTimerInterval - duration.Milliseconds)-(LastInterval - MaintanenceTimerInterval), 1), MaintanenceTimerInterval);
            return;
        }

        internal void PluginTimerProcess(object source, ElapsedEventArgs e)
        {
            //Pause Timer;
            PluginTimerEnd = -1;
            PluginTimer.Enabled = false;
            PluginTimerStart = _GetCurrentTick();
            PluginTotalTime.Start();
            PluginTimerSequence = PluginTimerStart;
            long LocalPluginTimerSequence = PluginTimerStart;
            DateTime DT = new DateTime(PluginTimerStart);


            PluginStruct PluginProcess, OldPluginProcess, SendingPluginProcess;
            NewFlagStruct PluginFlag;
            bool flag;
            int index, TransactionCount = 0;
            bool dllFlag;

            int MaxTransactions = SysData.GetValueInt("PluginThreadMaxTransactions", 60);

            PluginProcess = new PluginStruct();
            OldPluginProcess = new PluginStruct();
            PluginFlag = new NewFlagStruct();
            SendingPluginProcess = new PluginStruct();

            if (DebugSettings[1])
                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Beginning of Plugin Loop", 9999999, "", ""));
            try
            {
                for (index = 0; index < PluginDictionary.Count; index++) //Plugin Loops
                {

                    if (DebugSettings[1])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Top O Loop", 9999999, index.ToString(), ""));

                    var a = PluginDictionary.ElementAt(index);
                    OldPluginProcess = a.Value;
                    PluginProcess = a.Value;
                    if (!PluginProcess.Initialized)
                        continue;
                    //Get Stats
                    try
                    {
                        PluginProcess.Status = (PluginStatusStruct)PluginProcess.ServerAssemblyType.GetField("PluginStatus").GetValue(PluginProcess.Plugin);
                    }
                    catch (Exception err)
                    {
                    }


                    //Receive FLag From Plugin
                    TransactionCount = 0;
                    if (PluginProcess.Status.SetFlag && TransactionCount <= MaxTransactions)
                    {
                        if (DebugSettings[1])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Start of Flag From Plugin", 9999999, index.ToString(), ""));

                        Tuple<string, string, DateTime> PluginEvent= null;
                          object[] FlagSet = new object[] { PluginFlag, PluginEvent };
                        if (DebugSettings[3])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_SetFlagOnServer-Invoke", 9999999, PluginProcess.PluginName, ""));

                        while ((bool)PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_SetFlagOnServer", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, FlagSet))
                        {
                            if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                return;
                            PluginFlag = (NewFlagStruct)FlagSet[0];
                            PluginEvent = (Tuple<string, string, DateTime>)FlagSet[1];

                            if (!string.IsNullOrEmpty(PluginFlag.FlagName))
                            {
                                if (FlagAccess.FlagValidityCheck(PluginFlag.FlagName + " " + PluginFlag.FlagSubType, PluginProcess.DBSerialNumber))
                                {
                                    if (PluginFlag.Operation == FlagActionCodes.addorupdate)
                                        flag = FlagAccess.FullSet(PluginFlag.FlagName, PluginFlag.FlagSubType, PluginFlag.RoomUniqueID, PluginFlag.SourceUniqueID, PluginFlag.FlagValue, PluginFlag.FlagRawValue, PluginFlag.Type, PluginProcess.DBSerialNumber, PluginFlag.MaxHistoryToSave, PluginFlag.ValidValues, PluginFlag.UOM);

                                    if (PluginFlag.Operation == FlagActionCodes.delete)
                                        flag = FlagAccess.DeleteFlag(PluginFlag.FlagName, PluginFlag.FlagSubType);

                                    if (PluginFlag.Operation == FlagActionCodes.addonly)
                                    {
                                        flag = FlagAccess.FullSetAddOnly(PluginFlag.FlagName, PluginFlag.FlagSubType, PluginFlag.RoomUniqueID, PluginFlag.UniqueID, PluginFlag.FlagValue, PluginFlag.FlagRawValue, PluginFlag.Type, PluginProcess.DBSerialNumber, PluginFlag.MaxHistoryToSave, PluginFlag.ValidValues, PluginFlag.UOM);
                                    }


                                    if (PluginFlag.Operation == FlagActionCodes.updateonly)
                                    {
                                        flag = FlagAccess.FullSetUpdateOnly(PluginFlag.FlagName, PluginFlag.FlagSubType, PluginFlag.RoomUniqueID, PluginFlag.UniqueID, PluginFlag.FlagValue, PluginFlag.FlagRawValue, PluginFlag.Type, PluginProcess.DBSerialNumber, PluginFlag.MaxHistoryToSave, PluginFlag.ValidValues, PluginFlag.UOM);
                                    }

                                    if (PluginFlag.ChangeArchiveStatus)
                                    {
                                        FlagAccess.ChangeFlagArchiveStatus(PluginFlag.FlagName, PluginFlag.FlagSubType, PluginFlag.NewArchiveStatus);
                                        flag = FlagAccess.FullSetUpdateOnly(PluginFlag.FlagName, PluginFlag.FlagSubType, PluginFlag.RoomUniqueID, PluginFlag.UniqueID, PluginFlag.FlagValue, PluginFlag.FlagRawValue, PluginFlag.Type, PluginProcess.DBSerialNumber, PluginFlag.MaxHistoryToSave, PluginFlag.ValidValues, PluginFlag.UOM);

                                    }
                                }
                                else
                                {
                                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 113, "", " (" + PluginFlag.FlagName + PluginFlag.FlagSubType + "-" + PluginProcess.DBSerialNumber + ")"));
                                    ;
                                }
                            }

                            if (PluginEvent!=null)
                            {
                                FlagAccess.AddOrUpdateEventData(PluginEvent.Item1, PluginProcess.PluginName, PluginEvent.Item2, PluginEvent.Item3);

                            }
                            TransactionCount++;
                        }
                        if (DebugSettings[3])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_SetFlagOnServer-Return", 9999999, PluginProcess.PluginName, ""));
                        if (DebugSettings[1])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "End of Flag From Plugin", 9999999, index.ToString(), ""));
                    }

                    //Check for Incoming Server Data
                    TransactionCount = 0;
                    while (PluginProcess.Status.ToServer && TransactionCount <= MaxTransactions)
                    {
                        PluginServerDataStruct PSS = new PluginServerDataStruct();
                        if (DebugSettings[1])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Start of Incomming Server Data", 9999999, index.ToString(), ""));

                        object[] PSSSet = new object[] { PSS };
                        try
                        {
                            if (DebugSettings[6])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationGoingToServer-Invoke", 9999999, PluginProcess.PluginName, ""));
                            flag = (bool)PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationGoingToServer", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, PSSSet);
                            if (DebugSettings[6])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationGoingToServer-Remote", 9999999, PluginProcess.PluginName, ""));
                            if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                return;
                            TransactionCount++;
                        }
                        catch (Exception e2)
                        {
                            break;
                        }

                        if (flag)
                        {
                            if (DebugSettings[0])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 9999999, PluginProcess.DBSerialNumber, " (Info to Server " + PSS.String2 + ")"));
                            PSS = (PluginServerDataStruct)PSSSet[0];



                            if (PSS.Command == ServerPluginCommands.ErrorMessage)
                            {
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, PSS.CommandNumber, PSS.String, " (" + PluginProcess.PluginName + " " + PSS.String2 + ") " + PSS.String4));
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.LocalErrorMessage)
                            {
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), PluginProcess.DBSerialNumber, PSS.CommandNumber, PSS.String, PSS.String2 + " (" + PluginProcess.PluginName + ") "));
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.LocalGeneralMesssage)
                            {
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), PluginProcess.DBSerialNumber, PSS.CommandNumber, PSS.String, PSS.String2+" (" + PluginProcess.PluginName + ") "));
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.GeneralMessage)
                            {
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, PSS.CommandNumber, PSS.String, " (" + PluginProcess.PluginName + " " + PSS.String2 + ") "));
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.GetEncryptionCode)
                            {
                                if (MainDB.GetEncryptionCodesInfo(PluginProcess.DBSerialNumber, PSS.String, ref PSS.String2, ref PSS.String3))
                                    PSS.ResultCode = CommunicationResultCode.Successful;
                                else
                                    PSS.ResultCode = CommunicationResultCode.UnSuccessful;
                                DateTime CT = _GetCurrentDateTime();
                                if (DebugSettings[7])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_InformationCommingFromServerServer-Invoke", 9999999, PluginProcess.PluginName, ""));
                                PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_InformationCommingFromServerServer", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { CT, PSS });
                                if (DebugSettings[7])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_InformationCommingFromServerServer-Invoke", 9999999, PluginProcess.PluginName, ""));
                                if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                    return;
                            }

                            if (PSS.Command == ServerPluginCommands.GetDataBaseInfo)
                            {
                                string[] Internal;
                                bool InternalValidData;
                                DateTime CT = _GetCurrentDateTime();
                                var InternalReader = MainDB.ExecuteSQLCommandWithReader(PSS.String, PSS.String2, out InternalValidData);
                                InternalReader = MainDB.GetNextRecordWithReader(ref InternalReader, out Internal, out InternalValidData);
                                List<string[]> DataStuff = new List<string[]>();
                                while (InternalValidData)
                                {
                                    DataStuff.Add(Internal);
                                    InternalReader = MainDB.GetNextRecordWithReader(ref InternalReader, out Internal, out InternalValidData);
                                }
                                MainDB.CloseNextRecordWithReader(ref InternalReader);
                                PSS.ReferenceObject = DataStuff;
                                PSS.ServerEventReturnCommand = ServerEvents.RequestedDBInfoReady;
                                PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_InformationCommingFromServerServer", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { CT, PSS });
                                continue;
                            }



                            if (PSS.Command == ServerPluginCommands.ProcessWordFlagCompleted)
                            {
                                if (PSS.Bool)
                                    LBToClearQueue.Enqueue(ImmediateCommandsResponse);
                                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(PSS.String, ImmediateCommandsResponse, null));
                                foreach (Tuple<string, string> DC in (List<Tuple<string, string>>)PSS.ReferenceObject)
                                {
                                    if (FlagAccess.FlagValidityCheck(DC.Item1, PSS.UniqueNumber.Substring(0, 11)))
                                    {
                                        flag = FlagAccess.Set(DC.Item1, "", DC.Item2, PSS.UniqueNumber.Substring(0, 11));
                                        OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(DC.Item1 + "=" + DC.Item2 + " (" + flag + ")", ImmediateCommandsResponse, null));

                                    }
                                    else
                                    {
                                        string E = string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 113, "", " (" + DC.Item1 + "-" + PSS.UniqueNumber.Substring(0, 11) + ")");
                                        PendingMessageQueue.Enqueue(E);
                                        OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(E, ImmediateCommandsResponse, null));
                                        //
                                    }
                                }
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.ProcessWordDisplayCompleted)
                            {
                                if (PSS.Bool)
                                    LBToClearQueue.Enqueue(ImmediateCommandsResponse);
                                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(PSS.String, ImmediateCommandsResponse, null));
                                foreach (Tuple<string, string> DC in (List<Tuple<string, string>>)PSS.ReferenceObject)
                                {
                                    string Value, Raw;
                                    Value = FlagAccess.GetValue(DC.Item1, "", out Raw);
                                    OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(DC.Item1 + "=" + Value + " (" + Raw + ")", ImmediateCommandsResponse, null));
                                }
                                continue;

                            }

                            if (PSS.Command == ServerPluginCommands.ProcessMacroDeviceCommandCompleted)
                            {
                                DeviceStruct DS;
                                if (PSS.Bool)
                                    LBToClearQueue.Enqueue(ImmediateCommandsResponse);
                                Tuple<string, string> DC = (Tuple<string, string>) PSS.ReferenceObject;
                                DeviceDictionary.TryGetValue(DC.Item1, out DS);
                                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(RoomList.Find(c => c.Item1 == DS.RoomUniqueID).Item2 + " " + DS.DeviceName + ": " + PSS.String2, ImmediateCommandsResponse, null));
                                continue;
                            }


                            if (PSS.Command == ServerPluginCommands.ProcessWordCommandCompleted)
                            {
                                DeviceStruct DS;
                                if (PSS.Bool)
                                    LBToClearQueue.Enqueue(ImmediateCommandsResponse);
                                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(PSS.String, ImmediateCommandsResponse, null));

                                int count = 0;
                                if (PSS.ReferenceObject != null)
                                {
                                    foreach (Tuple<string, string, string> DC in (List<Tuple<string, string, string>>)PSS.ReferenceObject)
                                    {
                                        count++;
                                        if (string.IsNullOrEmpty(DC.Item1) || string.IsNullOrEmpty(DC.Item2))
                                        {
                                            string S = "", T = "";
                                            MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 1000, ref S, ref T);
                                            if (!string.IsNullOrEmpty(DC.Item2))
                                            {
                                                DeviceDictionary.TryGetValue(DC.Item2, out DS);
                                                S = RoomList.Find(c => c.Item1 == DS.RoomUniqueID).Item2 + " " + DS.DeviceName + " " + S;
                                            }
                                            OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, ImmediateCommandsResponse, null));
                                            continue;
                                        }
                                        PluginCommunicationStruct PCS = new PluginCommunicationStruct();
                                        DateTime CT = _GetCurrentDateTime();
                                        PCS.Command = PluginCommandsToPlugins.ProcessCommandWords;
                                        PCS.DeviceUniqueID = DC.Item2;
                                        PCS.String = PSS.String;
                                        PCS.String2 = DC.Item1;
                                        PCS.String4 = DC.Item3;
                                        PCS.UniqueNumber = string.Format("{0:0000}-{1:0000000000}", "XXXX", NextSequencialNumber);
                                        PCS.OriginPlugin = PSS.Plugin;
                                        bool fl0 = DeviceDictionary.TryGetValue(DC.Item2, out DS);

                                        if (fl0)
                                        {
                                            InterfaceStruct IS;
                                            bool fl1 = InterfaceDictionary.TryGetValue(DS.InterfaceUniqueID, out IS);
                                            if (fl1)
                                            {
                                                PCS.DestinationPlugin = IS.ControllingDLL;
                                                PluginStruct PIS = new PluginStruct();
                                                bool fl2 = PluginDictionary.TryGetValue(PCS.DestinationPlugin, out PIS);
                                                if (fl2)
                                                {
                                                    if (!TestOnly.Checked)
                                                        PIS.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PIS.ServerInstance, new object[] { CT, PCS });
                                                }
                                                else
                                                {
                                                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 111, PCS.DestinationPlugin, ""));
                                                }
                                            }
                                            else
                                            {
                                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 110, DS.InterfaceUniqueID, ""));
                                            }
                                            OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(RoomList.Find(c => c.Item1 == DS.RoomUniqueID).Item2 + " " + DS.DeviceName + " " + DC.Item2 + " " + DC.Item3, ImmediateCommandsResponse, null));
                                        }
                                        else
                                        {
                                            PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 112, DC.Item2, ""));
                                        }
                                    }
                                }

                                if (FullDebug.Checked)
                                {
                                    if (count == 0)
                                    {
                                        string S = "", T = "";
                                        MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 1001, ref S, ref T);
                                        OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, ImmediateCommandsResponse, null));
                                    }

                                    if (PSS.Strings != null)
                                    {
                                        foreach (string NLP in PSS.Strings)
                                        {
                                            OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(NLP, ImmediateCommandsResponse, null));
                                        }
                                    }

                                    if (PSS.Strings2 != null)
                                    {
                                        foreach (string NLP in PSS.Strings2)
                                        {
                                            OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(NLP, ImmediateCommandsResponse, null));
                                        }
                                    }

                                    if (PSS.Doubles != null)
                                    {
                                        string S = "";
                                        for (int q = 0; q < PSS.Doubles.Length; q++)
                                        {
                                            if (Math.Round(PSS.Doubles[q], 4) != 0)
                                                S = S + PSS.Doubles[q].ToString() + ", ";
                                        }
                                        OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, ImmediateCommandsResponse, null));
                                    }

                                    if (PSS.Doubles2 != null)
                                    {
                                        string S = "";
                                        for (int q = 0; q < PSS.Doubles2.Length; q++)
                                        {
                                            if (Math.Round(PSS.Doubles2[q], 4) != 0)
                                                S = S + PSS.Doubles2[q].ToString() + ", ";
                                        }
                                        OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, ImmediateCommandsResponse, null));
                                    }
                                }
                                continue;

                            }

                            if (PSS.Command == ServerPluginCommands.ProcessWordMacroCompleted)
                            {
                                if(PSS.Bool)
                                    LBToClearQueue.Enqueue(ImmediateCommandsResponse);
                                string S1 = "", EC = "";
                                MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 1002, ref S1, ref EC);                               
                                OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S1+" "+ PSS.String, ImmediateCommandsResponse, null));

                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.AddRoom)
                            {
                                string[] FieldNames = { "UniqueID", "RoomName", "Location", "InterfaceUniqueIDs" };
                                string[] Values = new string[4];
                                Values[0] = PSS.String;
                                Values[1] = PSS.String2;
                                Values[2] = PSS.String3;
                                Values[3] = PSS.String4;
                                MainDB.WriteRecord("Rooms", FieldNames, Values);
                                IncedentFlagQueue.Enqueue(new Tuple<PluginIncedentFlags, object>(PluginIncedentFlags.NewRoom, (object)Values));
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 200, PSS.String2 + " (" + PSS.String + ") ", ""));
                                RoomList.Add(Tuple.Create(PSS.String, PSS.String2, PSS.String3, PSS.String4));
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.AddPassword)
                            {
                                string[] FieldNames = { "PluginNumber", "PWCode", "UserName", "Password", "PWLevel" };
                                string[] Values = new string[5];
                                Values[0] = PluginProcess.DBSerialNumber;
                                Values[1] = PSS.String;
                                Values[2] = PSS.String2;
                                Values[3] = PSS.String3;
                                Values[4] = PSS.String4;
                               
                                MainDB.WriteRecord("AccessCodes", FieldNames, Values);
                                PasswordStruct PasswordStructData = new PasswordStruct();
                                PasswordStructData.PluginID = PluginProcess.DBSerialNumber;
                                PasswordStructData.PWCode = PSS.String;
                                PasswordStructData.Account = PSS.String2;
                                PasswordStructData.Password = PSS.String3;
                                PasswordStructData.PWLevel = PSS.String4;
                                PasswordDictionary.Add(PSS.String+PSS.String2, PasswordStructData);
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.AddActionItem)
                            {
                                string S1 = "", EC = "";
                                MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), PluginProcess.DBSerialNumber, PSS.CommandNumber, ref S1, ref EC);
                                string S = _GetCurrentTime() + " " + PSS.String + " " + S1 + PSS.String2 + " (" + PluginProcess.PluginName + ")";
                                ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>(S, ActionItemsListBox, "", PSS.String3));
                                continue;


                            }

                            if (PSS.Command == ServerPluginCommands.DeleteActionItem)
                            {
                                ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>("", ActionItemsListBox, "", PSS.String3));
                                continue;


                            }

                            if (PSS.Command == ServerPluginCommands.DeleteDevice)
                            {
                                DeviceStruct DS = (DeviceStruct)PSS.ReferenceObject;
                                MainDB.DeleteRecord("Devices", "UniqueID", DS.DeviceUniqueID);
                                DeviceDictionary.Remove(DS.DeviceUniqueID);
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 202, RoomList.Find(c => c.Item1 == DS.RoomUniqueID).Item2 + " " + DS.DeviceName + " (" + DS.DeviceUniqueID + ") ", ""));
                                IncedentFlagQueue.Enqueue(new Tuple<PluginIncedentFlags, object>(PluginIncedentFlags.DeleteDevice, (object)DS));
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.UpdateDevice)
                            {
                                throw new NotImplementedException();

                            }

                            if(PSS.Command==ServerPluginCommands.DeviceIsOffline)
                            {
                                DeviceStruct DS;
                                string S = "", T = "";
                                MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 1000, ref S, ref T);

                                DeviceDictionary.TryGetValue(PSS.String, out DS);
                                S = RoomList.Find(c => c.Item1 == DS.RoomUniqueID).Item2 + " " + DS.DeviceName;
 
                                try
                                {
                                    XmlDocument XML = new XmlDocument();
                                    XML.LoadXml(DS.XMLConfiguration);
                                    XmlNodeList FlagList = XML.SelectNodes("/root/flags/flag");
                                    if (FlagList.Count == 0)
                                        FlagList = XML.SelectNodes("/flags/flag");
                                    bool FoundSubfield = false;
                                    bool IsCurrentlyOffline = true;
                                    foreach (XmlElement el in FlagList)
                                    {
  
                                        for (int i = 0; i < el.Attributes.Count; i++)
                                        {
                                            if(el.Attributes[i].Name.ToLower().Trim()== "subfield")
                                            {
                                                FoundSubfield = true;
                                                bool AlreadyOffline=false;
                                                if (!FlagAccess.TakeDeviceOffLine(S, el.Attributes[i].Value, PluginProcess.DBSerialNumber, out AlreadyOffline))
                                                {
                                                    FlagAccess.FullSetAddOnly(S, el.Attributes[i].Value, DS.RoomUniqueID, DS.DeviceUniqueID, SysData.GetValue("OffLineName"), SysData.GetValue("OffLineName"), FlagChangeCodes.Changeable, PluginProcess.DBSerialNumber, FlagData.FlagChangeHistoryMaxSize, "", DS.UOMCode);
                                                    FlagAccess.TakeDeviceOffLine(S, el.Attributes[i].Value, PluginProcess.DBSerialNumber, out AlreadyOffline);
                                                }
                                                if (!AlreadyOffline)
                                                {
                                                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 203, S + " " + el.Attributes[i].Value, DS.DeviceIdentifier + " (" + PSS.String + ") "));
                                                    IsCurrentlyOffline = false;
                                                }

                                            }
                                        }
                                        if (!FoundSubfield)
                                        {
                                            bool AlreadyOffline = false;
                                            if (!FlagAccess.TakeDeviceOffLine(S, "", PluginProcess.DBSerialNumber, out AlreadyOffline))
                                            {
                                                FlagAccess.FullSetAddOnly(S, "", DS.RoomUniqueID, DS.DeviceUniqueID, SysData.GetValue("OffLineName"), SysData.GetValue("OffLineName"), FlagChangeCodes.Changeable, PluginProcess.DBSerialNumber, FlagData.FlagChangeHistoryMaxSize, "", DS.UOMCode);
                                                FlagAccess.TakeDeviceOffLine(S, "", PluginProcess.DBSerialNumber, out AlreadyOffline);
                                            }
                                            if (!AlreadyOffline)
                                            {
                                                IsCurrentlyOffline = false;
                                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 203, S + " " + "", DS.DeviceIdentifier + " (" + PSS.String + ") "));
                                            }
                                        }

                                    }

                                    if(!IsCurrentlyOffline)
                                        ActionItemsListBoxQueue.Enqueue(new Tuple<string, ListBox, string, string>(_GetCurrentTime() + " " + S + " "+ SysData.GetValue("OffLineName"), ActionItemsListBox, "", DS.DeviceUniqueID + SysData.GetValue("OffLineName")));
                                }
                                catch (Exception CHMAPIEx)
                                {
                                    AddToUnexpectedErrorQueue(CHMAPIEx);
                                }
                            }

                            if (PSS.Command == ServerPluginCommands.AddDevice)
                            {
                                string[] FieldNames = { "UniqueID", "DeviceName", "DeviceType", "DeviceClassID", "RoomUniqueID", "InterfaceUniqueID", "DeviceIdentifier", "NativeDeviceIdentifier", "UOMCode", "Origin", "AdditionalFlagName", "HTMLDisplayName", "SpokenNames", "AFUOMCode", "DeviceGrouping", "XMLConfiguration", "OffLine", "UndesignatedFieldsInfo", "IntVal01", "IntVal02", "IntVal03", "IntVal04", "StrVal01", "StrVal02", "StrVal03", "StrVal04", "Comments"};
                                string[] Values = new string[27];
                                DeviceStruct DS = (DeviceStruct)PSS.ReferenceObject;
                                Values[0] = DS.DeviceUniqueID;
                                Values[1] = DS.DeviceName;
                                Values[2] = DS.DeviceType;
                                Values[3] = DS.DeviceClassID;
                                Values[4] = DS.RoomUniqueID;
                                Values[5] = DS.InterfaceUniqueID;
                                Values[6] = DS.DeviceIdentifier;
                                Values[7] = DS.NativeDeviceIdentifier;
                                Values[8] = DS.UOMCode;
                                Values[09] = DS.Origin;
                                Values[10] = DS.AdditionalFlagName;
                                Values[11] = DS.HTMLDisplayName;
                                Values[12] = DS.SpokenNames;
                                Values[13] = DS.AFUOMCode;
                                Values[14] = DS.DeviceGrouping;
                                Values[15] = DS.XMLConfiguration;
                                Values[16] = DS.OffLine;
                                Values[17] = DS.UndesignatedFieldsInfo;
                                Values[18] = DS.IntVal01.ToString();
                                Values[19] = DS.IntVal02.ToString();
                                Values[20] = DS.IntVal03.ToString();
                                Values[21] = DS.IntVal04.ToString();
                                Values[22] = DS.StrVal01;
                                Values[23] = DS.StrVal02;
                                Values[24] = DS.StrVal03;
                                Values[25] = DS.StrVal04;
                                Values[26] = DS.Comments;
                                MainDB.WriteRecord("Devices", FieldNames, Values);
                                DeviceDictionary.Add(DS.DeviceUniqueID, DS.DeepCopy());
                                IncedentFlagQueue.Enqueue(new Tuple<PluginIncedentFlags, object>(PluginIncedentFlags.NewDevice, (object)DS));
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 201, RoomList.Find(c => c.Item1 == DS.RoomUniqueID).Item2 + " " + DS.DeviceName + " (" + DS.DeviceUniqueID + ") ", ""));
                                continue;
                            }

                            if(PSS.Command== ServerPluginCommands.AddToConfigurationInfo)
                            {

                                Tuple<string, string, string, string> CF = (Tuple<string, string, string, string>)PSS.ReferenceObject;

                                MainDB.AddorUpdateConfiguration(PSS.Plugin, CF.Item1, CF.Item3, CF.Item4.ToCharArray()[0]);
                                continue;

                            }

                            if (PSS.Command == ServerPluginCommands.SendAllIncedentFlags)
                            {
                                PluginProcess.SendAllIncedentFlags = true;
                                bool Updateflag = PluginDictionary.TryUpdate(PluginProcess.PluginName, PluginProcess, OldPluginProcess);
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.DontSendAllIncedentFlags)
                            {
                                PluginProcess.SendAllIncedentFlags = false;
                                bool Updateflag = PluginDictionary.TryUpdate(PluginProcess.PluginName, PluginProcess, OldPluginProcess);
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.SendJustFlagChangeIncedentFlags)
                            {
                                PluginProcess.SendJustFlagChangeIncedentFlags = true;
                                bool Updateflag = PluginDictionary.TryUpdate(PluginProcess.PluginName, PluginProcess, OldPluginProcess);
                                continue;
                            }

                            if (PSS.Command == ServerPluginCommands.DontSendJustFlagChangeIncedentFlags)
                            {
                                PluginProcess.SendJustFlagChangeIncedentFlags = false;
                                bool Updateflag = PluginDictionary.TryUpdate(PluginProcess.PluginName, PluginProcess, OldPluginProcess);
                                continue;
                            }

                        }
                        else
                        {
                            break;
                        }
                        if (DebugSettings[1])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "End of Incomming Server Data", 9999999, index.ToString(), ""));
                    }

                    //Check for Incoming Plugin Data
                    TransactionCount = 0;
                    while (PluginProcess.Status.ToPlugin && TransactionCount <= MaxTransactions)
                    {
                        PluginCommunicationStruct PCS = new PluginCommunicationStruct();
                        object[] PCSSet = new object[] { PCS };
                        if (DebugSettings[1])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Start of Incomming Plugin Data", 9999999, index.ToString(), ""));
                        try
                        {
                            if (DebugSettings[8])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationGoingToPlugin-Invoke", 9999999, PCS.DestinationPlugin, ""));
                            flag = (bool)PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationGoingToPlugin", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, PCSSet);
                            if (DebugSettings[8])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationGoingToPlugin-Return", 9999999, PCS.DestinationPlugin, ""));
                            if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                return;
                            TransactionCount++;
                        }
                        catch (Exception CHMAPIEx)
                        {
                            AddToUnexpectedErrorQueue(CHMAPIEx);
                            break;
                        }

                        if (!flag)
                        {
                            break;
                        }

                        PCS = (PluginCommunicationStruct)PCSSet[0];
                        PCS.OriginPlugin = PluginProcess.PluginName;
                        dllFlag = false;
                        if (!string.IsNullOrEmpty(PCS.DestinationPlugin))
                        {
                            try
                            {

                                dllFlag = (Path.GetExtension(PCS.DestinationPlugin.Trim()) == ".dll");

                            }
                            catch
                            {
                                dllFlag = false;
                            }
                        }

                        if (DebugSettings[0])
                        {
                            OutgoingDataStruct ODS;
                            ODS = (OutgoingDataStruct)PCS.OutgoingDS;
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 9999999, PluginProcess.DBSerialNumber, " (Plugin Data In " + PCS.Command + " " + PCS.DestinationPlugin + " " + ODS.LocalIDTag + ")"));
                        }
                        if (!dllFlag)
                        {
                            try
                            {
                                PCS.ResultCode = CommunicationResultCode.InvalidPlugin;
                                PCS.ReferenceUniqueNumber = PCS.UniqueNumber;
                                PCS.UniqueNumber = string.Format("{0:0000}-{1:0000000000}", "XXXX", NextSequencialNumber);
                                PCS.OriginPlugin = "XXXX";
                                DateTime CT = _GetCurrentDateTime();
                                if (DebugSettings[9])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationCommingFromPlugin-Invoke", 9999999, PCS.DestinationPlugin, ""));
                                PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { CT, PCS });
                                if (DebugSettings[9])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationCommingFromPlugin-Return", 9999999, PCS.DestinationPlugin, ""));
                                if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                    return;
                                TransactionCount++;
                                if (DebugSettings[0])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 9999999, PluginProcess.DBSerialNumber, " (Plugin Data Out " + PCS.ResultCode + " " + PCS.DestinationPlugin + ")"));
                                continue;
                            }
                            catch (Exception e2)
                            {
                                continue;
                            }

                        }

                        switch (PCS.Command)
                        {
                            case PluginCommandsToPlugins.LinkAccepted:
                                if (!PluginProcess.PluginsToAcceptDataFrom.Contains(PCS.DestinationPlugin))
                                    PluginProcess.PluginsToAcceptDataFrom.Add(PCS.DestinationPlugin);
                                break;
                            case PluginCommandsToPlugins.LinkRejected:
                                PluginProcess.PluginsToAcceptDataFrom.Remove(PCS.DestinationPlugin);
                                break;
                        }



                        try
                        {
                            bool IFlag = PluginDictionary.TryGetValue(PCS.DestinationPlugin, out SendingPluginProcess);
                            if (IFlag && SendingPluginProcess.Initialized)
                            {
                                DateTime CT = _GetCurrentDateTime();
                                if (DebugSettings[10])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationCommingFromPlugin-Invoke", 9999999, PCS.DestinationPlugin, ""));
                                SendingPluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, SendingPluginProcess.ServerInstance, new object[] { CT, PCS });
                                if (DebugSettings[10])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationCommingFromPlugin-Return", 9999999, PCS.DestinationPlugin, ""));
                                if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                    return;
                                TransactionCount++;
                            }
                        }
                        catch (Exception e3)
                        {
                            try
                            {
                                PCS.ResultCode = CommunicationResultCode.InvalidPlugin;
                                PCS.ReferenceUniqueNumber = PCS.UniqueNumber;
                                PCS.UniqueNumber = string.Format("{0:0000}-{1:0000000000}", "XXXX", NextSequencialNumber);
                                PCS.OriginPlugin = "XXXX";
                                PCS.ResultCode = CommunicationResultCode.UnableToLoadPlugin;
                                DateTime CT = _GetCurrentDateTime();
                                if (DebugSettings[10])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationCommingFromPlugin-Invoke", 9999999, PCS.DestinationPlugin, ""));
                                PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { CT, PCS });
                                if (DebugSettings[10])
                                    PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_PluginInformationCommingFromPlugin-Return", 9999999, PCS.DestinationPlugin, ""));
                                if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                    return;
                                TransactionCount++;
                                continue;
                            }
                            catch (Exception e4)
                            {
                                continue;
                            }
                        }
                    }

                    //Check for Unexpected Errors
                    TransactionCount = 0;
                    while (PluginProcess.Status.UEErrors && TransactionCount <= MaxTransactions)
                    {
                        PluginErrorMessage PCS = new PluginErrorMessage();
                        PluginErrorMessage PEM;
                        object[] PCSSet = new object[] { PCS };
                        if (DebugSettings[1])
                            PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "Start of Incomming UE", 9999999, index.ToString(), ""));
                        try
                        {
                            if (DebugSettings[8])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_UE-Invoke", 9999999, PluginProcess.ServerInstance, ""));
                            flag = (bool)PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_GetUnexpectedErrors", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, PCSSet);
                            if (DebugSettings[8])
                                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_UE-Return", 9999999, PluginProcess.ServerInstance, ""));
                            if (flag)
                            {
                                PEM = (PluginErrorMessage)PCSSet[0];
                                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), PluginProcess.DBSerialNumber, 2000002, PEM.ExceptionData + "\r\n\r\n" + PEM.Comment + "\r\n", " (" + PluginProcess.Plugin.ManifestModule.Name + ")"));


                            }


                            if (LocalPluginTimerSequence != PluginTimerStart)  //Watchdog Restarted this
                                return;
                            TransactionCount++;
                        }
                        catch (Exception e1)
                        {
                            break;
                        }

                        if (!flag)
                        {
                            break;
                        }



                    }

                    if (DebugSettings[1])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "End of Incomming Plugin Data", 9999999, index.ToString(), ""));
                }//End of Plugin Loop

            }
            catch (Exception e4)
            {
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 2000001, e4.Message, " (" + PluginProcess.Plugin + ") " + e4.StackTrace));

            }

            PluginTotalTime.Stop();



            //Restart Timer and exit
            if (DebugSettings[1])
                PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), " Timer Reset ", 9999999, PluginTimer.Interval.ToString(), ""));
            PluginTimerEnd = _GetCurrentTick();
            PluginTimer.Enabled = true;
            return;
        }

        internal void HeartbeatTimerProcess(object source, ElapsedEventArgs e)
        {

            PluginStruct PluginProcess;



            //Pause Timer;
            HeartbeatTimer.Enabled = false;
            HeartbeatTimerStart = _GetCurrentTick();
            HeartbeatTimerEnd = -1;
            HeartbeatTotalTime.Start();


            PluginProcess = new PluginStruct();

            DateTime CT = _GetCurrentDateTime();
            TimeSpan FromLastHeartbeat = new TimeSpan(CT.Ticks - HeartBeatLastProcess.Ticks);
            HeartbeatTimeCode HBTC = HeartbeatTimeCode.Nothing;


            if (CT.Year != HeartBeatLastProcess.Year)
            {
                HBTC = HeartbeatTimeCode.NewYear;
            }
            else
                if (CT.Month != HeartBeatLastProcess.Month)
            {
                HBTC = HeartbeatTimeCode.NewMonth;
            }
            else
                    if (CT.Day != HeartBeatLastProcess.Day)
            {
                HBTC = HeartbeatTimeCode.NewDay;
            }
            else
                        if (CT.Hour != HeartBeatLastProcess.Hour)
            {
                HBTC = HeartbeatTimeCode.NewHour;
            }
            else
                            if (CT.Minute != HeartBeatLastProcess.Minute)
            {
                HBTC = HeartbeatTimeCode.NewMinute;
            }
            else
                                if (CT.Second != HeartBeatLastProcess.Second)
            {
                HBTC = HeartbeatTimeCode.NewSecond;

            }


            foreach (var pair in PluginDictionary)
            {
                PluginProcess = pair.Value;
                if (!PluginProcess.Initialized)
                    continue;
                try
                {
                    if (DebugSettings[11])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_Heartbeat-Invoke", 9999999, PluginProcess.PluginName, ""));
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_Heartbeat", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { CT, FromLastHeartbeat.Milliseconds, HBTC, ServerStatusCode });
                    if (DebugSettings[11])
                        PendingMessageQueue.Enqueue(string.Format(DEBUGQUEUEFORMATSTRING, _GetCurrentTick(), "CHMAPI_Heartbeat-Return", 9999999, PluginProcess.PluginName, ""));
                }
                catch (Exception err)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 53, err.Message, " (" + PluginProcess.PluginName + " CHMAPI_Heartbeat) " + err.StackTrace));
                }
            }
            HeartBeatLastProcess = CT;

            HeartbeatTotalTime.Stop();

            //Restart Timer and exit
            HeartbeatTimerEnd = _GetCurrentTick();
            HeartbeatTimer.Enabled = true;
            return;
        }

        #endregion


        #region Functions

        internal int _ConvertToInt32(string Value)
        {
            try
            {
                double x;
                Double.TryParse(Value, out x);
                int xx = Convert.ToInt32(x);
                return (xx);

            }
            catch
            {
                return (0);
            }
        }


        internal Int64 _ConvertToInt64(string Value)
        {
            try
            {
                double x;
                Double.TryParse(Value, out x);
                Int64 xx = Convert.ToInt64(x);
                return (xx);

            }
            catch
            {
                return (0);
            }
        }

        static bool _UpdateSysData<T>(Expression<Func<T>> expr, ref string CurrentValue, string NewValue)
        {
            var body = ((MemberExpression)expr.Body);
            string VarName = body.Member.Name;
            return (true);
        }

        static string Check<T>(Expression<Func<T>> expr)
        {
            var body = ((MemberExpression)expr.Body);
            return (body.Member.Name);
        }

        static char FlagTypeFromFlagChangeCodes(FlagChangeCodes FCC)
        {
            return (Convert.ToChar("NOC".Substring((int)FCC, 1)));
        }


        static internal int NextSequencialNumber
        {
            get
            {
                Interlocked.Increment(ref _NextSequencialNumber);
                return (_NextSequencialNumber);
            }
        }

        string _CreateSaltedPasswordHash(int SaltValue, string Password) //Returns Salted Hashcode for Password
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(Password, salt, SaltValue);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            return (savedPasswordHash);
        }

        static long _GetCurrentTick()
        {
            DateTime CDate = DateTime.UtcNow;
            long CurrentTicks = CDate.Ticks;
            if (UTCOffsetTicks == -1)
                return (0);
            return (CurrentTicks + UTCOffsetTicks);
        }

        static long _UTCOffset()
        {
            string L;
            DateTime CDate = DateTime.UtcNow;

            if (!SysData.PeekValue("TimeZone", out L))
                return (0);
            TimeZoneInfo TZone = TimeZoneInfo.FindSystemTimeZoneById(L);
            return (((long)TZone.BaseUtcOffset.Hours + Convert.ToInt32(TZone.IsDaylightSavingTime(CDate))) * TimeSpan.TicksPerHour);
        }

        string _GetCurrentTime()
        {
            DateTime dtresult = new DateTime(_GetCurrentTick());
            return (dtresult.ToString());
        }

        internal static DateTime _GetCurrentDateTime()
        {
            return (new DateTime(_GetCurrentTick()));
        }

        void _LoadAllInternalTables()  //Loads Rooms, interfaces, etc.
        {
            string[] Internal;
            bool InternalValidData;

            UOM = new SortedList<int, Tuple<string, string, string>>();
            string[] UOMs;
            bool UOMValidData;
            var UOMReader = MainDB.ExecuteSQLCommandWithReaderandFields("UnitsOfMeasure", "Code, " + SysData.GetValue("LanguageCode") + ", " + SysData.GetValue("LanguageCode") + "_Abbreviation" + ", " + SysData.GetValue("LanguageCode") + "_Subvalues", "", out UOMValidData);
            UOMReader = MainDB.GetNextRecordWithReader(ref UOMReader, out UOMs, out UOMValidData);

            while (UOMValidData)
            {
                try
                {
                    UOM.Add(_ConvertToInt32(UOMs[0]), new Tuple<string, string, string>(UOMs[1], UOMs[2], UOMs[3]));
                    UOMReader = MainDB.GetNextRecordWithReader(ref UOMReader, out UOMs, out UOMValidData);
                }
                catch
                {
                }
            }
            MainDB.CloseNextRecordWithReader(ref UOMReader);


            var InternalReader = MainDB.ExecuteSQLCommandWithReader("Rooms", "", out InternalValidData);
            InternalReader = MainDB.GetNextRecordWithReader(ref InternalReader, out Internal, out InternalValidData);

            while (InternalValidData)
            {
                RoomList.Add(Tuple.Create(Internal[0], Internal[1], Internal[2], Internal[3]));
                InternalReader = MainDB.GetNextRecordWithReader(ref InternalReader, out Internal, out InternalValidData);
            }
            MainDB.CloseNextRecordWithReader(ref InternalReader);




            string[] StatusMessages;
            string LangValue;
            bool StatusMessagesValidData;
            var StatusMessagesReader = MainDB.ExecuteSQLCommandWithReader("StatusMessages", "", out StatusMessagesValidData);
            StatusMessagesReader = MainDB.GetNextRecordWithReaderWithLanguageValue(ref StatusMessagesReader, out StatusMessages, out StatusMessagesValidData, out LangValue, SysData.GetValue("LanguageCode"));
            while (StatusMessagesValidData)
            {
                StatusMessagesStruct StatusMessagesStructData = new StatusMessagesStruct();
                StatusMessagesStructData.ModuleSerialNumber = StatusMessages[0];
                StatusMessagesStructData.Module = StatusMessages[1];
                StatusMessagesStructData.StatusCode = StatusMessages[2];
                StatusMessagesStructData.Status = StatusMessages[3];
                StatusMessagesStructData.Comment = StatusMessages[4];
                StatusMessagesStructData.StatusMessage = LangValue;
                StatusMessagesStructData.LogCode = StatusMessages[6];
                StatusMessagesDictionary.Add(StatusMessages[0] + StatusMessages[2], StatusMessagesStructData);
                StatusMessagesReader = MainDB.GetNextRecordWithReaderWithLanguageValue(ref StatusMessagesReader, out StatusMessages, out StatusMessagesValidData, out LangValue, SysData.GetValue("LanguageCode"));
            }
            MainDB.CloseNextRecordWithReader(ref StatusMessagesReader);


            bool DeviceValidData;
            string[] Device;

            var DeviceReader = MainDB.ExecuteSQLCommandWithReader("Devices", "", out DeviceValidData);
            DeviceReader = MainDB.GetNextRecordWithReader(ref DeviceReader, out Device, out DeviceValidData);
            while (DeviceValidData)
            {
                DeviceStruct DeviceStructData= new DeviceStruct();
                DeviceStructData.DeviceUniqueID = Device[0];
                DeviceStructData.DeviceName = Device[1];
                DeviceStructData.DeviceType = Device[2];
                DeviceStructData.DeviceClassID = Device[3];
                DeviceStructData.RoomUniqueID = Device[4];
                DeviceStructData.InterfaceUniqueID = Device[5];
                DeviceStructData.DeviceIdentifier = Device[6];
                DeviceStructData.NativeDeviceIdentifier = Device[7];
                DeviceStructData.UOMCode = Device[8];
                DeviceStructData.Origin = Device[9];
                DeviceStructData.AdditionalFlagName = Device[10];
                DeviceStructData.HTMLDisplayName = Device[11];
                DeviceStructData.SpokenNames = Device[12];
                DeviceStructData.AFUOMCode = Device[13];
                DeviceStructData.DeviceGrouping = Device[14];
                DeviceStructData.XMLConfiguration = Device[15];
                DeviceStructData.OffLine = Device[16];
                DeviceStructData.UndesignatedFieldsInfo = Device[17];
                DeviceStructData.IntVal01 = _ConvertToInt32(Device[18]);
                DeviceStructData.IntVal02 = _ConvertToInt32(Device[19]);
                DeviceStructData.IntVal03 = _ConvertToInt32(Device[20]);
                DeviceStructData.IntVal04 = _ConvertToInt32(Device[21]);
                DeviceStructData.StrVal01 = Device[22];
                DeviceStructData.StrVal02 = Device[23];
                DeviceStructData.StrVal03 = Device[24];
                DeviceStructData.StrVal04 = Device[25];
                DeviceStructData.Comments = Device[26];
                DeviceStructData.Local_TableLoc = -1;
                DeviceStructData.Local_Flag1 = false;
                DeviceStructData.Local_Flag2 = false;
                DeviceStructData.Local_IsLocalDevice = false;
                DeviceStructData.Local_CommandStatementIgnore = "";
                DeviceStructData.Local_OriginalInfo = "";

                MainDB.GetBlobFieldByReader(DeviceReader, "ObjVal", out DeviceStructData.objVal);
                DeviceDictionary.Add(Device[0], DeviceStructData);
                DeviceReader = MainDB.GetNextRecordWithReader(ref DeviceReader, out Device, out DeviceValidData);
            }
            MainDB.CloseNextRecordWithReader(ref DeviceReader);

            //Now We Add the System Flags
            string[] SystemFlags;
            bool SystemFlagsValidData;
            var SystemFlagsReader = MainDB.ExecuteSQLCommandWithReader("SystemFlags", "", out SystemFlagsValidData);
            SystemFlagsReader = MainDB.GetNextRecordWithReader(ref SystemFlagsReader, out SystemFlags, out SystemFlagsValidData);

            while (SystemFlagsValidData)
            {
                bool flag = FlagAccess.FullSetSystemFlag(SystemFlags[2], "", SystemFlags[3], SystemFlags[0], SystemFlags[1], SystemFlags[4], MODULESERIALNUMBER, true);
                SystemFlagStruct SFD = new SystemFlagStruct();
                SFD.FlagType = SystemFlags[0];
                SFD.FlagCatagory = SystemFlags[1];
                SFD.FlagName = SystemFlags[2];
                SFD.StartupValue = SystemFlags[3];
                SFD.ValidValues = SystemFlags[4];
                SFD.Comments = SystemFlags[5];
                SystemFlagsList.Add(SFD);
                SystemFlagsReader = MainDB.GetNextRecordWithReader(ref SystemFlagsReader, out SystemFlags, out SystemFlagsValidData);
            }

            MainDB.CloseNextRecordWithReader(ref SystemFlagsReader);


            string[] DeviceTemplate;
            bool DeviceTemplateValidData;

            var DeviceTemplateReader = MainDB.ExecuteSQLCommandWithReader("DeviceTemplates", "", out DeviceTemplateValidData);
            DeviceTemplateReader = MainDB.GetNextRecordWithReader(ref DeviceTemplateReader, out DeviceTemplate, out DeviceTemplateValidData);

            while (DeviceTemplateValidData)
            {
                DeviceTemplateStruct DeviceTemplateStructData = new DeviceTemplateStruct();
                DeviceTemplateStructData.DeviceUniqueID = DeviceTemplate[0];
                DeviceTemplateStructData.DeviceKey = DeviceTemplate[1];
                DeviceTemplateStructData.DeviceType = DeviceTemplate[2];
                DeviceTemplateStructData.DeviceClassID = DeviceTemplate[3];
                DeviceTemplateStructData.ControllingDLL = DeviceTemplate[4];
                DeviceTemplateStructData.UOMCode = DeviceTemplate[5];
                DeviceTemplateStructData.XMLConfiguration = DeviceTemplate[6];
                DeviceTemplateStructData.UndesignatedFieldsInfo = DeviceTemplate[7];
                DeviceTemplateStructData.IntVal01 = _ConvertToInt32(DeviceTemplate[8]);
                DeviceTemplateStructData.IntVal02 = _ConvertToInt32(DeviceTemplate[9]);
                DeviceTemplateStructData.IntVal03 = _ConvertToInt32(DeviceTemplate[10]);
                DeviceTemplateStructData.IntVal04 = _ConvertToInt32(DeviceTemplate[11]);
                DeviceTemplateStructData.StrVal01 = DeviceTemplate[12];
                DeviceTemplateStructData.StrVal02 = DeviceTemplate[13];
                DeviceTemplateStructData.StrVal03 = DeviceTemplate[14];
                DeviceTemplateStructData.StrVal04 = DeviceTemplate[15];
                DeviceTemplateStructData.Comments = DeviceTemplate[16];

                MainDB.GetBlobFieldByReader(DeviceTemplateReader, "ObjVal", out DeviceTemplateStructData.objVal);


                DeviceTemplateStructData.Local_TableLoc = -1;


                DeviceTemplateDictionary.Add(DeviceTemplate[0], DeviceTemplateStructData);
                DeviceTemplateReader = MainDB.GetNextRecordWithReader(ref DeviceTemplateReader, out DeviceTemplate, out DeviceTemplateValidData);
            }
            MainDB.CloseNextRecordWithReader(ref DeviceTemplateReader);

            string[] Passwords;
            bool PasswordValidData;
            var PasswordReader = MainDB.ExecuteSQLCommandWithReader("AccessCodes", "", out PasswordValidData);
            PasswordReader = MainDB.GetNextRecordWithReader(ref PasswordReader, out Passwords, out PasswordValidData);
            while (PasswordValidData)
            {
                PasswordStruct PasswordStructData = new PasswordStruct();
                PasswordStructData.PluginID = Passwords[0];
                PasswordStructData.PWCode = Passwords[1];
                PasswordStructData.Account = Passwords[2];
                PasswordStructData.Password = Passwords[3];
                PasswordStructData.PWLevel = Passwords[4];
                PasswordDictionary.Add(Passwords[1]+Passwords[2], PasswordStructData);
                PasswordReader = MainDB.GetNextRecordWithReader(ref PasswordReader, out Passwords, out PasswordValidData);
            }
            MainDB.CloseNextRecordWithReader(ref PasswordReader);

            string[] Interface;
            bool InterfaceValidData;
            var InterfaceReader = MainDB.ExecuteSQLCommandWithReader("Interfaces", "", out InterfaceValidData);
            InterfaceReader = MainDB.GetNextRecordWithReader(ref InterfaceReader, out Interface, out InterfaceValidData);
            while (InterfaceValidData)
            {
                InterfaceStruct InterfaceStructData = new InterfaceStruct();
                InterfaceStructData.InterfaceUniqueID = Interface[0];
                InterfaceStructData.InterfaceName = Interface[1];
                InterfaceStructData.RoomUniqueID = Interface[2];
                InterfaceStructData.InterfaceType = Interface[3];
                InterfaceStructData.InterfaceHardware = Interface[4];
                InterfaceStructData.HardwareSettings = Interface[5];
                InterfaceStructData.PluginName = Interface[6];
                InterfaceStructData.StartupInformation = Interface[7];
                InterfaceStructData.Comments = Interface[11];
                InterfaceStructData.PreInitializeTimeOut = Interface[10];
                InterfaceStructData.ControllingDLL = Interface[9];
                InterfaceStructData.HardwareIdentifier = Interface[8];
                InterfaceStructData.TableLoc = -1;
                InterfaceDictionary.Add(Interface[0], InterfaceStructData);
                InterfaceReader = MainDB.GetNextRecordWithReader(ref InterfaceReader, out Interface, out InterfaceValidData);
            }
            MainDB.CloseNextRecordWithReader(ref InterfaceReader);

            InterfaceReader = MainDB.ExecuteSQLCommandWithReader("Interfaces", "", out InterfaceValidData);
            InterfaceReader = MainDB.GetNextRecordWithReader(ref InterfaceReader, out Interface, out InterfaceValidData);

            while (InterfaceValidData)
            {
                _InstallAndInitializePlugin(SysData.GetValue("PluginLocation"), Interface[6], "", "Interfaces", Interface); //Communication DLL
                _InstallAndInitializePlugin(SysData.GetValue("PluginLocation"), Interface[9], "", "Interfaces", Interface);  //Controlling DLL

                InterfaceReader = MainDB.GetNextRecordWithReader(ref InterfaceReader, out Interface, out InterfaceValidData);
            }
            MainDB.CloseNextRecordWithReader(ref InterfaceReader);
        }


        internal void _InstallAndInitializePlugin(string Directory, string PluginName, string PluginsToAcceptDataFrom, string Origin, string[] OrignalDB)
        {
            string[] SA;
            bool ValidData;
            PluginStruct PluginData;

            //Load Plugins
            PluginData = new PluginStruct();
            if (!PluginDictionary.ContainsKey(PluginName))
            {
                if (SysData.GetValue("PluginTestMode") == "Y")
                {
                    if (PluginName.ToLower() != SysData.GetValue("PluginTestModePluginName").ToLower())
                        return;
                }
                //if (SysData.GetValue("UseAI") == "N")
                //{
                //    if (PluginName.ToLower() == SysData.GetValue("AIDLLName").ToLower())
                //        return;
                //}
                var Reader = MainDB.ExecuteSQLCommandWithReader("PluginReference", "FileName='" + PluginName + "'", out ValidData);
                Reader = MainDB.GetNextRecordWithReader(ref Reader, out SA, out ValidData);
                MainDB.CloseNextRecordWithReader(ref Reader);

                PluginData.Foreign = !ValidData;
                PluginData.LoadError = true;
                PluginData.Initialized = false;
                PluginData.Loaded = false;
                PluginData.PluginInErrorState = false;
                PluginData.PluginDeactivated = true;

                if (PluginData.Foreign)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 46, "", " (" + PluginName + ")"));

                    PluginData.PluginName = PluginName;

                    PluginDictionary.TryAdd(PluginName, PluginData);
                    return;
                }

                PluginData.PluginName = SA[0];
                PluginData.DBSerialNumber = SA[1];
                PluginData.SHA512Database = SA[2];
                PluginData.Ignore = SA[3] == "Y";
                PluginData.DLLType = SA[4].ToLower();
                if (PluginData.DLLType == "command" || PluginData.DLLType == "aicommand")
                    CommandDLL = PluginData.PluginName;
                if (PluginData.DLLType == "html")
                    HTMLDLL = PluginData.PluginName;
                if (PluginData.DLLType == "menu")
                    MENUDLL = PluginData.PluginName;
                if (PluginData.DLLType.Contains("archive"))
                {
                    ArchiveDLL = PluginData.PluginName;
                    Archiving = true;
                }

                PluginData.Uniqueid = SA[5];

                if (PluginData.Ignore)
                {
                    return;
                }

                try
                {
                    var stream = new BufferedStream(File.OpenRead(Path.Combine(Directory, PluginName)), 100000);
                    HashAlgorithm SHA512 = new SHA512Managed();
                    byte[] hash = SHA512.ComputeHash(stream);
                    PluginData.SHA512Calculated = BitConverter.ToString(hash).Replace("-", string.Empty);
                    PluginData.SHA512Matched = (PluginData.SHA512Calculated == PluginData.SHA512Database);
                }
                catch (Exception err)
                {
                    PluginDictionary.TryAdd(SA[0], PluginData);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 52, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                    return;
                }

                PluginData.PluginUniqueID = NextSequencialNumber;
                PluginData.OriginalDataBaseInfo = (string[])OrignalDB.Clone();
                AddToStatusDisplay(PluginData);

                try
                {

                    PluginData.Plugin = Assembly.LoadFrom(Path.Combine(Directory, PluginName));
                    PluginData.ServerAssemblyType = PluginData.Plugin.GetType("CHMPluginAPI.ServerAccessFunctions");
                    PluginData.ServerInstance = PluginData.ServerAssemblyType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null);
                    PluginData.AssemblyType = PluginData.Plugin.GetType("CHMModules." + Path.GetFileNameWithoutExtension(PluginData.PluginName));
                    PluginData.Instance = PluginData.AssemblyType.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null);
                    PluginData.LoadError = false;

                    //MethodInfo[] m = PluginData.ServerAssemblyType.GetMethods();
                }
                catch (Exception err)
                {
                    PluginDictionary.TryAdd(SA[0], PluginData);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 49, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                    return;
                }
                try
                {
                    PluginData.AssemblyType.InvokeMember("PluginInitialize", BindingFlags.InvokeMethod, null, PluginData.Instance, new object[] { PluginData.PluginUniqueID });
                }
                catch (Exception err)
                {
                    PluginDictionary.TryAdd(SA[0], PluginData);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 50, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                    return;
                }

                try
                {

                    PluginData.ServerAssemblyType.InvokeMember("CHMAPI_InitializePlugin", BindingFlags.InvokeMethod, null, PluginData.ServerInstance, new object[] { PluginData.PluginUniqueID, _GetCurrentDateTime(), SysData.GetValue("PluginDataFileLocations") });
                    PluginData.Initialized = true;
                }
                catch (Exception err)
                {
                    PluginDictionary.TryAdd(SA[0], PluginData);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 50, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                    return;
                }
                string[] Field, Value, SubFields;
                MainDB.LoadPluginConfigurationData(PluginData.DBSerialNumber, out Field, out Value, out SubFields);

                string[] MainCHMField, MainCHMValue, MainCHMSubFields;
                MainDB.LoadPluginConfigurationData(MODULESERIALNUMBER, out MainCHMField, out MainCHMValue, out MainCHMSubFields);

                Queue<DeviceStruct> LDS = new Queue<DeviceStruct>();
                Queue<InterfaceStruct> ISD = new Queue<InterfaceStruct>();
                Queue<PasswordStruct> PSD = new Queue<PasswordStruct>();
                Queue<StatusMessagesStruct> MSD = new Queue<StatusMessagesStruct>();
                Queue<DeviceTemplateStruct> DTS = new Queue<DeviceTemplateStruct>();


                if (PluginData.DLLType.ToLower() == "command" || PluginData.DLLType.ToLower() == "ai" || PluginData.DLLType.ToLower() == "html")
                {
                    PluginData.SendAllIncedentFlags = true;
                    foreach (DeviceStruct DS in DeviceDictionary.Values)
                    {
                        LDS.Enqueue(DS.DeepCopy());
                    }
                    foreach (PasswordStruct PS in PasswordDictionary.Values)
                    {
                        if (PS.PluginID == PluginData.Uniqueid)
                            PSD.Enqueue(PS);
                    }
                    foreach (StatusMessagesStruct SM in StatusMessagesDictionary.Values)
                    {
                        if (Path.GetFileNameWithoutExtension(PluginName.ToLower()) == Path.GetFileNameWithoutExtension(SM.Module.ToLower()))
                            MSD.Enqueue(SM);
                    }

                    foreach (DeviceTemplateStruct DT in DeviceTemplateDictionary.Values)
                    {
                        if (DT.ControllingDLL.ToLower() == PluginName.ToLower())
                            DTS.Enqueue(DT);
                    }
                    foreach (InterfaceStruct IS in InterfaceDictionary.Values)
                    {
                        ISD.Enqueue(IS);
                    }
                }
                else
                {

                    foreach (InterfaceStruct IS in InterfaceDictionary.Values)
                    {
                        ISD.Enqueue(IS);
                        if (IS.ControllingDLL == PluginName)
                        {

                            foreach (DeviceStruct DS in DeviceDictionary.Values)
                            {

                                if (DS.InterfaceUniqueID == IS.InterfaceUniqueID)
                                    LDS.Enqueue(DS.DeepCopy());
                            }
                            foreach (PasswordStruct PS in PasswordDictionary.Values)
                            {
                                if (PS.PluginID == PluginData.DBSerialNumber)
                                    PSD.Enqueue(PS);
                            }
                            foreach (StatusMessagesStruct SM in StatusMessagesDictionary.Values)
                            {
                                if (Path.GetFileNameWithoutExtension(IS.ControllingDLL) == Path.GetFileNameWithoutExtension(SM.Module))
                                    MSD.Enqueue(SM);
                            }

                            foreach (DeviceTemplateStruct DT in DeviceTemplateDictionary.Values)
                            {
                                if (DT.ControllingDLL.ToLower() == PluginName.ToLower())
                                    DTS.Enqueue(DT);
                            }
                        }
                    }
                }

                foreach (PasswordStruct PS in PasswordDictionary.Values)
                {
                    if (PS.PluginID == PluginData.DBSerialNumber)
                        PSD.Enqueue(PS);
                }
                bool b;
                string DBPassword = UnEncrypt(EncryptedMasterPassword, out b);
                object[] stuff = new object[] { LDS.ToArray(), RoomList.ToArray(), ISD.ToArray(), PSD.ToArray(), Field, Value, OrignalDB, Origin, SubFields, MSD.ToArray(), DTS.ToArray(), SystemFlagsList.ToArray(), UOM.ToArray(), DBPassword,  MainCHMField, MainCHMSubFields, MainCHMValue};

                try
                {
                    PluginData.ServerAssemblyType.InvokeMember("CHMAPI_StartupInfoFromServer", BindingFlags.InvokeMethod, null, PluginData.ServerInstance, new object[] { stuff });
                }
                catch (Exception err)
                {
                    PluginDictionary.TryAdd(SA[0], PluginData);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 51, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                    return;
                }


                try
                {
                    if (PluginData.PluginName==ArchiveDLL)
                    {
                        object[] FlagSet = new object[] { ArchiveFlagChangesQueue };
                        PluginData.ServerAssemblyType.InvokeMember("CHMAPI_QueuesCommingFromServer", BindingFlags.InvokeMethod, null, PluginData.ServerInstance, new object[] { FlagSet });
                    }
                }
                catch (Exception err)
                {
                    PluginDictionary.TryAdd(SA[0], PluginData);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 51, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                    return;
                }

                try
                {
                    PluginData.PluginVersion = (string)PluginData.ServerAssemblyType.Assembly.GetName().Version.ToString();
                    //GetGetField("Version").GetValue(PluginData.Plugin);
                    PluginData.Status = (PluginStatusStruct)PluginData.ServerAssemblyType.GetField("PluginStatus").GetValue(PluginData.Plugin);
                }
                catch (Exception err)
                {
                }

                try
                {
                    PluginData.MetaSerialNumber = (string)PluginData.ServerAssemblyType.GetField("PluginSerialNumber").GetValue(PluginData.Plugin);
                }
                catch
                {
                }

                try
                {
                    PluginData.PluginDescription = (string)PluginData.ServerAssemblyType.GetField("PluginDescription").GetValue(PluginData.Plugin);
                }
                catch
                {
                }



                if (!PluginData.SHA512Matched)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 45, "", " (" + PluginData.PluginName + ")"));
                    //    PluginDictionary.TryAdd(SA[0], PluginData);
                    //    PluginData.PluginDeactivated = true;
                    //    return;
                }
                PluginData.PluginDeactivated = false;
                PluginData.PluginsToAcceptDataFrom = new List<string>();
                if (!string.IsNullOrEmpty(PluginsToAcceptDataFrom))
                    PluginData.PluginsToAcceptDataFrom.Add(PluginsToAcceptDataFrom);


                PluginDictionary.TryAdd(SA[0], PluginData);


                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 41, "", " (" + PluginData.PluginName + ")"));
                string S = string.Format("Module {0:0000}", PluginData.PluginUniqueID);
                FlagAccess.Set(S, "Name", PluginData.PluginName, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set(S, "Serial Number", PluginData.DBSerialNumber, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set(S, "Version", PluginData.PluginVersion, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
                FlagAccess.Set(S, "Description", PluginData.PluginDescription, FlagChangeCodes.OwnerOnly, MODULESERIALNUMBER, -1);
            }
            else
            {
                bool flag = PluginDictionary.TryGetValue(PluginName, out PluginData);
                if (!flag)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 54, Origin, " (" + PluginData.PluginName + ")"));
                    return;
                }
                if (!PluginData.Initialized)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 55, Origin, " (" + PluginData.PluginName + ")"));
                    return;

                }
                try
                {
                    PluginData.ServerAssemblyType.InvokeMember("CHMAPI_AddDBRecord", BindingFlags.InvokeMethod, null, PluginData.ServerInstance, new object[] { OrignalDB, Origin });
                }
                catch (Exception err)
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 51, err.Message, " (" + PluginData.PluginName + ") " + err.StackTrace));
                }
            }
        }

        void _WriteMessageLogToFile(string Location, string Type, int Versions)
        {
            StreamWriter sw = null, tw = null;

            try
            {
                CHM.MainCHMForm._SetupFileVersions(Location + "\\MessageLogs" + Type + ".txt", Versions);
                sw = File.CreateText(Location + "\\MessageLogs" + Type + ".txt");

                foreach (var e in SavedMessageQueue)
                {
                    sw.WriteLine(e);
                }
                sw.Close();

            }
            catch (Exception err)
            {
                if (sw != null)
                {
                    sw.Write(err);
                    sw.Close();
                }
            }

            try
            {
                CHM.MainCHMForm._SetupFileVersions(Location + "\\ErrorLogs" + Type + ".txt", Versions);
                tw = File.CreateText(Location + "\\ErrorLogs" + Type + ".txt");

                foreach (var e in ErrorMessageQueue)
                {
                    tw.WriteLine(e);
                }
                tw.Close();

            }
            catch (Exception err)
            {
                if (tw != null)
                {
                    tw.Write(err);
                    tw.Close();
                }
            }

            if (DebugMessageQueue.Count > 0)
            {
                try
                {
                    CHM.MainCHMForm._SetupFileVersions(Location + "\\DebugLogs" + Type + ".txt", Versions);
                    tw = File.CreateText(Location + "\\DebugLogs" + Type + ".txt");

                    foreach (var e in DebugMessageQueue)
                    {
                        tw.WriteLine(e);
                    }
                    tw.Close();

                }
                catch (Exception err)
                {
                    if (tw != null)
                    {
                        tw.Write(err);
                        tw.Close();
                    }
                }

            }
        }

        internal void _SendMessageToBalloonTip(string Title, string Message)
        {

            notifyIcon1.ShowBalloonTip(SysData.GetValueInt("BalloonTipDisplayTime", 15), Title, Message, ToolTipIcon.Info);
        }


        internal void AddToStatusDisplay(PluginStruct PluginData)
        {
            string S;

            S = string.Format("{0,-25} {6} {1,-5} {2,3:+#;-#;0} {3,3:+#;-#;0} {4,3:+#;-#;0} {5,3:+#;-#;0} {6,3:+#;-#;0}",
                Truncate(Path.GetFileNameWithoutExtension(PluginData.PluginName), 25), PluginData.Initialized, PluginData.Status.SetFlagCount, PluginData.Status.ToServerCount, PluginData.Status.ToPluginCount, PluginData.Status.NumberOfUEErrorsCount, PluginData.DBSerialNumber);

            OtherGUIBoxesQueue.Enqueue(new Tuple<string, ListBox, string>(S, PluginStatus, Truncate(Path.GetFileNameWithoutExtension(PluginData.PluginName), 25)));

        }

        internal string Truncate(string S, int Len)
        {
            return (S.Substring(0, Math.Min(Len, S.Length)));
        }

        static void _SetupFileVersions(string MainFileName, int Versions)
        {
            if (Versions == 0)
                return;

            string S = Path.GetDirectoryName(MainFileName);
            string T = Path.GetFileNameWithoutExtension(MainFileName);
            string U = Path.GetExtension(MainFileName);

            string Q = S + "\\" + T + Versions.ToString() + U;
            File.Delete(S + "\\" + T + Versions.ToString() + U);
            try
            {
                for (int i = Versions; i > 1; i--)
                {
                    if (File.Exists(S + "\\" + T + (i - 1).ToString() + U))
                        File.Move(S + "\\" + T + (i - 1).ToString() + U, S + "\\" + T + i.ToString() + U);
                }
            }
            catch
            {
            }

            try
            {
                File.Move(S + "\\" + T + U, S + "\\" + T + "1" + U);
            }
            catch
            {
            }

        }
        #endregion



 

        #region Special Routines To Allow Updating of Main Message Window From Any Thread

        internal void _ProcessMainGUIUpdate(object sender, EventArgs ea)
        {
            string ModuleSerialNumber;
            int MessageNumber;
            string Pre;
            string Post;
            string S, T, e, EC, V;
            int U;
            long CT;
            DateTime d1 = new DateTime();
            int MFlag = 0;
            FlagDataStruct Flag;
            string TagMessage = "";

            MainGUIDispatchTimer.IsEnabled = false;
            ListBox LB;
            while (LBToClearQueue.Count > 0)
            {
                LBToClearQueue.TryDequeue(out LB);
                LB.Items.Clear();

            }

            while (PendingMessageQueue.Count > 0)
            {
                PendingMessageQueue.TryDequeue(out e);
                S = "";
                T = "";
                EC = "";
                V = "";

                if (e.Substring(0, 1) == "*")
                {
                    long.TryParse(e.Substring(1, 25), out CT);
                    MessageNumber = 9999999;
                    ModuleSerialNumber = MODULESERIALNUMBER;

                }
                else
                {
                    ModuleSerialNumber = e.Substring(26, 11).Trim();
                    long.TryParse(e.Substring(0, 25), out CT);
                    int.TryParse(e.Substring(38, 12), out MessageNumber);
                }

                if (MessageNumber < 0)
                    MFlag = MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), ModuleSerialNumber, -MessageNumber, ref S, ref EC);

                if (MessageNumber > 0)
                    MFlag = MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), ModuleSerialNumber, MessageNumber, ref S, ref EC);

                if (MessageNumber == 0)
                    MFlag = 0;

                if (MFlag < 0)
                {
                    S = "Message Error " + MessageNumber.ToString() + " " + ModuleSerialNumber;
                    EC = "Y";
                }

                if (EC != "D")
                {
                    if (e.Length < 133)
                        e = e + e.PadRight(132 - e.Length, ' ');
                    Pre = e.Substring(51, 40).Trim();
                    Post = e.Substring(92, 40).Trim();
                }
                else
                {
                    Post = e.Substring(28);
                    Pre = "";
                }

                S = Pre + " " + S.Trim() + " " + Post;

                U = SysData.GetValueInt("LocalMessageSequenceNumber");
                SysData.Set("LocalMessageSequenceNumber", U + 1);
                if (CT > 0)
                {
                    d1 = d1.AddTicks(-d1.Ticks + CT);

                    T = ModuleSerialNumber + " (" + MessageNumber.ToString("0000000;-000000") + ") " + d1.ToShortDateString() + " " + d1.ToLongTimeString() + " ";
                }
                else
                    T = ModuleSerialNumber + " (" + MessageNumber.ToString("0000000;-000000") + ") " + "                      ";

                if (EC != "D")
                    SavedMessageQueue.Enqueue(U.ToString("D6") + " " + ModuleSerialNumber + " (" + MessageNumber.ToString("0000000;-000000") + ") " + d1.ToString(DEBUGDATETIMEFORMATSTRING) + " " + S);
                if (EC == "Y")
                {
                    ErrorMessageQueue.Enqueue(U.ToString("D6") + " " + ModuleSerialNumber + " (" + MessageNumber.ToString("0000000;-000000") + ") " + d1.ToString(DEBUGDATETIMEFORMATSTRING) + " " + e.Substring(51).Trim());
                    MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 102, ref V, ref EC);
                    _SendMessageToBalloonTip(V, ModuleSerialNumber + " " + S);
                    TagMessage = U.ToString("D6") + " " + ModuleSerialNumber + " (" + MessageNumber.ToString("0000000;-000000") + ") " + d1.ToString(DEBUGDATETIMEFORMATSTRING) + " " + e.Substring(51).Trim();
                    MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 102, ref V, ref EC);
                }
                if (EC == "D")
                {
                    DebugMessageQueue.Enqueue(U.ToString("D6") + " " + ModuleSerialNumber + " (" + MessageNumber.ToString("0000000;-000000") + ") " + d1.ToString(DEBUGDATETIMEFORMATSTRING) + " " + S);
                }

                _UpdateMainGUIListBox(T + S, CHMMainFormMessageBox, "", TagMessage);
            }

            MainCHMForm.FlagData FlagAccess = new FlagData();
            while (FlagAccess.GetFlagsToDisplayValues(out Flag))
            {
                
                S = string.Format("{0,-55}   {1}", Flag.Name + " " + Flag.SubType+ "\t", Flag.Value+Flag.UOM);
                _UpdateMainGUIListBox(S, FlagBox, S.Substring(0, 55).TrimEnd(' '), Flag.UniqueID.ToString());
            }

            Tuple<string, string, string> FTD;
            while (FlagAccess.GetFlagsToDeleteValues(out FTD))
            {
                S = string.Format("{0,-55}", FTD.Item1 + " " + FTD.Item2) + "\t";
                _UpdateMainGUIListBox("", FlagBox, S, FTD.Item3);
            }
                        Tuple<string, ListBox, string> OGUI;
            while (OtherGUIBoxesQueue.Count > 0)
            {
                OtherGUIBoxesQueue.TryDequeue(out OGUI);
                _UpdateMainGUIListBox(OGUI.Item1, OGUI.Item2, OGUI.Item3, "");
            }

            Tuple<string, ListBox, string, string> OGUI2;
            while (ActionItemsListBoxQueue.Count > 0)
            {
                ActionItemsListBoxQueue.TryDequeue(out OGUI2);
                _UpdateMainGUIListBox(OGUI2.Item1, OGUI2.Item2, OGUI2.Item3, OGUI2.Item4);
            }

            while (EventsItemsListBoxQueue.Count > 0)
            {
                EventsItemsListBoxQueue.TryDequeue(out OGUI2);
                _UpdateMainGUIListBox(OGUI2.Item1, OGUI2.Item2, OGUI2.Item3, OGUI2.Item4);
            }



            MainGUIDispatchTimer.IsEnabled = true;

        }

        internal void _ClearListBoxInGUI(ListBox LB)
        {
            LB.Items.Clear();

        }

 
        internal void _UpdateMainGUIListBox(string whatmessage, ListBox LB, string ReplaceKey, string TagMessage)
        {
            int selected = LB.SelectedIndex;
            int index = LB.TopIndex;

            if (string.IsNullOrEmpty(whatmessage) && string.IsNullOrEmpty(ReplaceKey) && string.IsNullOrEmpty(TagMessage))
            {
                LB.Items.Clear();
                return;
            }

            LB.BeginUpdate();
            CHMListBoxItems item = new CHMListBoxItems(whatmessage, TagMessage);

            if(string.IsNullOrEmpty(whatmessage) && string.IsNullOrEmpty(ReplaceKey))//Delete By Tag
            {
                for(index=0;index<LB.Items.Count;index++)
                {
                    CHMListBoxItems LBI = (CHMListBoxItems)LB.Items[index];
                    if(LBI.Tag== TagMessage)
                    {
                        LB.Items.RemoveAt(index);
                        break;
                    }
                }
                LB.EndUpdate();
                LB.Refresh();
                return;
            }

            if (string.IsNullOrEmpty(whatmessage))
            {
                int i = LB.FindString(ReplaceKey);

                if (i > -1)
                {
                    LB.Items.RemoveAt(i);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ReplaceKey))
                {
                    LB.Items.Add(item);
                }
                else
                {
                    int i = LB.FindString(ReplaceKey);

                    if (i == -1)
                    {
                        LB.Items.Add(item);
                    }
                    else
                    {
                        CHMListBoxItems LBI = (CHMListBoxItems)LB.Items[i];
                        if (LBI.Text != whatmessage)
                        {
                            item.Tag = LBI.Tag;
                            LB.Items[i] = item;
                        }
                    }
                }
            }
            LB.EndUpdate();
            if (LB.Name == "FlagBox")
            {
                if (selected > 0 && selected < LB.Items.Count)
                    LB.TopIndex = selected;
                else
                    LB.TopIndex = index;
            }
            else
                LB.TopIndex = LB.Items.Count - (LB.DisplayRectangle.Height / LB.ItemHeight);
        }
        #endregion

        #region Various Event Processing
        internal void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();//shows the program on taskbar
            this.WindowState = FormWindowState.Normal;//undoes the minimized state of the form
            notifyIcon1.Visible = false;//hides tray icon again
        }

        internal void MainCHMForm_Resize(object sender, EventArgs e)
        {

            //try
            //{
            //    if (MCH_Form.WindowState == FormWindowState.Minimized)//this code gets fired on every resize so we check if the form was minimized
            //    {
            //        string S = "", T = "", EC = "";
            //        Hide();//hides the program on the taskbar
            //        notifyIcon1.Visible = true;//shows our tray icon
            //        MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 100, ref S, ref EC);
            //        MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 101, ref T, ref EC);
            //        notifyIcon1.ShowBalloonTip(SysData.GetValueInt("BalloonTipDisplayTime"), S, T, ToolTipIcon.Info);
            //    }

            //}
            //catch 
            //{
            //}
        }

        internal void debugOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < DebugSettings.Length; i++)
                DebugSettings[i] = true;
        }

        internal void debugOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < DebugSettings.Length; i++)
                DebugSettings[i] = false;
        }

        internal void pluginMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugSettings[0] = true;
        }

        internal void setDebugVariablesToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            DebugList.Items.Clear();
            for (int i = 0; i < DebugSettings.Length; i++)
                DebugList.Items.Add(i.ToString("0000 ") + DebugSettings[i].ToString());
            DebugList.SelectedIndex = 0;

        }

        internal void MainCHMForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
                Exit_Click(sender, e);

            e.Cancel = true;
            Invoke(new Action(() => { this.WindowState = FormWindowState.Minimized; }));

        }

        internal void DebugList_Click(object sender, EventArgs e)
        {
            DebugSettings[DebugList.SelectedIndex] = !DebugSettings[DebugList.SelectedIndex];
            DebugList.Items[DebugList.SelectedIndex] = DebugList.SelectedIndex.ToString("0000 ") + DebugSettings[DebugList.SelectedIndex].ToString();
        }

        #endregion

        #region Various Functions
        internal void exitAndDecryptDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainDB.ClearDatabasePassword();
            Exit_Click(sender, e);
        }

        internal void changeDatabasePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangePasswordForm ChangePassword = new ChangePasswordForm("Database", true);
            ChangePassword.Owner = this;
            ChangePassword.ShowDialog();
            ChangePassword.Close();
            if (!ChangePassword.ResultFlag)
                return;
            bool Encflag;
            if (ChangePassword.CurrentPassword != UnEncrypt(EncryptedMasterPassword, out Encflag))
            {
                MessageBox.Show("Incorrect Current Database Password", "Password Change Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Okay, let's change the password
            EncryptedMasterPassword = Encrypt(ChangePassword.BrandNewPassword);
            MainDB.ChangeDatabasePassword(EncryptedMasterPassword);
            ResetDatabasePasswordFile(EncryptedMasterPassword);
        }

        internal string UnEncrypt(string EncryptedPassword, out bool Decrypted)
        {
            try
            {
                string[] arr = EncryptedPassword.Split('-');
                if (arr.Length < EncryptedPassword.Length / 3)
                {
                    Decrypted = false;
                    return ("");
                }
                byte[] array = new byte[arr.Length];
                for (int i = 0; i < arr.Length; i++) array[i] = Convert.ToByte(arr[i], 16);
                Byte[] E = ProtectedData.Unprotect(array, null, DataProtectionScope.LocalMachine);
                string US = System.Text.Encoding.UTF8.GetString(E);
                Decrypted = true;
                return (US);
            }
            catch (Exception e)
            {
                try
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 58, e.Message, " (Encrypt)"));

                    Decrypted = false;
                    return ("");

                }
                catch
                {
                    Decrypted = false;
                    return ("");
                }
            }
        }

        internal string Encrypt(string UnEncryptedPassword)
        {
            try
            {
                Byte[] B = System.Text.Encoding.UTF8.GetBytes(UnEncryptedPassword);
                Byte[] EncryptedPassword = ProtectedData.Protect(B, null, DataProtectionScope.LocalMachine);
                string S = BitConverter.ToString(EncryptedPassword);
                return (S);
            }
            catch (Exception e)
            {
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 58, e.Message, " (Encrypt)"));

                return ("");
            }
        }

        internal bool EncryptAndWriteToFile(string FileName, int Size, byte[] FileData)
        {
            try
            {
                Byte[] EncryptedFile = ProtectedData.Protect(FileData, null, DataProtectionScope.LocalMachine);

                FileStream _FileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write);
                _FileStream.Write(EncryptedFile, 0, EncryptedFile.Length);
                _FileStream.Close();
                return (true);
            }
            catch (Exception e)
            {
                PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 58, e.Message, " (Encrypt)"));

                return (false);
            }
        }

        internal bool ResetDatabasePasswordFile(string NewEncryptedPassword)
        {
            //Read Blob
            bool ValidData;
            var Reader = MainDB.ExecuteSQLCommandWithReader("StartupFile", "Language= \"" + SysData.GetValue("LanguageCode") + "\"", out ValidData);
            Reader = MainDB.GetNextRecordWithReader(ref Reader, out ValidData);
            string Filename = MainDB.GetStringFieldByReader(Reader, "FileName");
            Byte[] Blob;
            int BLobSize = MainDB.GetBlobFieldByReader(Reader, "StartupFile", out Blob);
            string S = Encoding.UTF8.GetString(Blob);
            string[] FileData;
            FileData = S.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            //Change Password
            FileData[7] = NewEncryptedPassword;

            //Write Blob
            StringBuilder Sb = new StringBuilder();
            for (int i = 0; i < FileData.Length; i++)
                Sb.Append(FileData[i] + "\r\n");
            S = Sb.ToString();
            byte[] OutBlob = System.Text.Encoding.UTF8.GetBytes(Sb.ToString());
            string[] FieldNames = { "Language", "FileName" };
            string[] FieldValues = { SysData.GetValue("LanguageCode"), "CHMInit.001" };
            MainDB.WriteBlobRecord("StartupFile", FieldNames, FieldValues, "StartupFile", OutBlob);


            //Write Startup File
            EncryptAndWriteToFile("CHMInit.001", OutBlob.Length, OutBlob);


            return (true);
        }


        internal bool ReadAndDecryptFile(string FileName, out string[] FileData)
        {
            try
            {
                FileStream fs = File.OpenRead(FileName);
                byte[] EncryptedFile = new byte[fs.Length];
                fs.Read(EncryptedFile, 0, Convert.ToInt32(fs.Length));
                Byte[] DecryptedFile = ProtectedData.Unprotect(EncryptedFile, null, DataProtectionScope.LocalMachine);
                fs.Close();
                string Sx = System.Text.Encoding.UTF8.GetString(DecryptedFile);
                FileData = Sx.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                if (FileData.Length > 9)
                    return (true);
                char[] chars = new char[DecryptedFile.Length / sizeof(char)];
                System.Buffer.BlockCopy(DecryptedFile, 0, chars, 0, DecryptedFile.Length);
                string S = new string(chars);
                FileData = S.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                return (true);
            }
            catch (Exception e)
            {
                try
                {
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), MODULESERIALNUMBER, 58, e.Message, " (Encrypt)"));

                    FileData = null;
                    return (false);
                }
                catch
                {
                    FileData = null;
                    return (false);
                }
            }
        }


        internal void ImmediateCommands_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Execute.PerformClick();
                e.Handled = true;
            }
            if (e.KeyChar == (char)Keys.Escape)
            {
                Cancel.PerformClick();
                e.Handled = true;
            }
        }

        internal void Execute_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ImmediateCommands.Text))
            {
                PluginServerDataStruct PSD = new PluginServerDataStruct();
                PSD.Plugin = CommandDLL;
                PSD.ServerEventReturnCommand = ServerEvents.ProcessWordCommand;
                PSD.String = ImmediateCommands.Text;
                PSD.String4 = "Immediate";
                PSD.ReferenceUniqueNumber = string.Format("{0:0000}-{1:0000000000}", "XXXX", NextSequencialNumber);
 //               ServerToPluginQueue.Enqueue(PSD);

                PluginStruct PluginProcess;
                if (PluginDictionary.TryGetValue(PSD.Plugin, out PluginProcess))
                {
                    DateTime CT = _GetCurrentDateTime();
                    PluginProcess.ServerAssemblyType.InvokeMember("CHMAPI_InformationCommingFromServerServer", BindingFlags.InvokeMethod, null, PluginProcess.ServerInstance, new object[] { CT, PSD });
                }
                else
                {
                    string S = "";
                    string EC = "";
                    MainDB.GetMessageByCode(SysData.GetValue("LanguageCode"), MODULESERIALNUMBER, 20003, ref S, ref EC);
                    PendingMessageQueue.Enqueue(string.Format(MESSAGEQUEUEFORMATSTRING, _GetCurrentTick(), "00000-00000", PSD.Plugin, PSD.String, " (" + S + ") "));
                }
            }
            LBToClearQueue.Enqueue(ImmediateCommandsResponse);
            ImmediateCommands.Clear();
            ImmediateCommands.Focus();
        }

        internal void Cancel_Click(object sender, EventArgs e)
        {
            ImmediateCommands.Clear();
            ImmediateCommands.Focus();
        }

        internal void MainCHMForm_Activated(object sender, EventArgs e)
        {
            ImmediateCommands.Focus();

        }

        internal void FlagBox_DoubleClick(object sender, EventArgs e)
        {
            ListBox LB = (ListBox)sender;


            if (LB.SelectedItem != null)
            {
                TimedDialogBoxWithListbox TB;

                CHMListBoxItems item = (CHMListBoxItems)LB.SelectedItem;
                FlagDataStruct Flag;
                bool b = FlagAccess.GetFlag(item.Text.Substring(0,55).Trim().ToLower().Replace("\t", ""), "", out Flag);
                String S = "Value: " + Flag.Value + " " + Flag.UOM + " Raw Value: " + Flag.RawValue + " " + Flag.UOM + "    " + Flag.ChangedBy + " " + new DateTime(Flag.ChangeTick).ToString() + " (Prev Value: " + Flag.LastChangeHistory.Value  +" "+ Flag.LastChangeHistory.RawValue + " " + new DateTime(Flag.LastChangeHistory.ChangeTime).ToString()+")\r\n" +
                    "Creation Information: " + Flag.CreatedBy + " " + new DateTime(Flag.CreateTick).ToString() + " Value: " + Flag.CreatedValue + " (" + Flag.CreatedRawValue + ")\r\n";

                List<string> FlagChangeList = new List<string>();
                for (int i = Flag.ChangeHistory.Count - 1; i >= 0; i--)
                {
                    FlagChangeList.Add(Flag.ChangeHistory[i].ChangedBy + " " + new DateTime(Flag.ChangeHistory[i].ChangeTime).ToString() + " Value: " + Flag.ChangeHistory[i].Value + " (" + Flag.ChangeHistory[i].RawValue + ")");
                }

                TB = new TimedDialogBoxWithListbox(Flag.Name + " " + Flag.SubType, S, FlagChangeList, 30, SysData.GetValue("LogFileLocation"));
                TB.ShowDialog();
            }
        }

        internal void CHMMainFormMessageBox_DoubleClick(object sender, EventArgs e)
        {
            ListBox LB = (ListBox)sender;

            if (LB.SelectedItem != null)
            {
                TimedDialogBox TB;

                CHMListBoxItems item = (CHMListBoxItems)LB.SelectedItem;
                if (!string.IsNullOrEmpty((string)item.Tag))
                    TB = new TimedDialogBox("Message", (string)item.Tag, 30);
                else
                    TB = new TimedDialogBox("Message", (string)item.Text, 30);
                TB.ShowDialog();
            }
        }

        internal void DevicesMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            DeviceMenuStuff DMF = new DeviceMenuStuff(this);
            DMF.DevicesMenuItemProcess((string)menuItem.Tag, menuItem.Text);
            return;
        }

        private void UserInterfaceButton_Click(object sender, EventArgs e)
        {
            PluginCommunicationStruct PCS = new PluginCommunicationStruct();
            PCS.UniqueNumber = string.Format("{0:0000}-{1:0000000000}", MODULESERIALNUMBER, MainCHMForm.NextSequencialNumber);
            PCS.Command = PluginCommandsToPlugins.HTMLProcess;
            PCS.HTMLSubCommand = PluginCommandsToPluginsHTMLSubCommands.StartHTMLSession;
            PCS.PWStruct = new PasswordStruct();
            PCS.PWStruct.PWLevel = "Admin";
            DateTime CT = MainCHMForm._GetCurrentDateTime();
            PCS.DestinationPlugin = HTMLDLL;
            MainCHMForm.PluginStruct PIS = new MainCHMForm.PluginStruct();
            bool fl2 = MainCHMForm.PluginDictionary.TryGetValue(PCS.DestinationPlugin, out PIS);
            if (fl2)
            {
                PIS.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PIS.ServerInstance, new object[] { CT, PCS });
            }



        }
    }



    internal class CHMListBoxItems
    {
        internal virtual string Text { get; set; }
        internal virtual string Tag { get; set; }

        internal CHMListBoxItems(string Text, string Tag)
        {
            this.Text = Text;
            this.Tag = Tag;
        }

        public  override string ToString()
        {
            return Text;
        }
    }
    #endregion



    #region DirectCallsFrom Plugins

    class PluginDirectCalls
    {
        string InstanceOwner;
        internal DatabaseAccess GeneralDB;

        internal int _ConvertToInt32(string Value)
        {
            try
            {
                double x;
                Double.TryParse(Value, out x);
                int xx = Convert.ToInt32(x);
                return (xx);

            }
            catch
            {
                return (0);
            }
        }

        public PluginDirectCalls(string PluginName)
        {
            InstanceOwner = PluginName;
            GeneralDB = new DatabaseAccess();
            string S="";
            GeneralDB.OpenMainDB(MainCHMForm.SysData.GetValue("DBLocation"), ref S, MainCHMForm.EncryptedMasterPassword);
        }

        public string ServerGetFlag(string Flag)
        {
            if (Flag.Substring(0, 2) == "$$")
            {
                string SV, RV;
                MainCHMForm.FlagData FD = new MainCHMForm.FlagData();
                FD.GetSpecialFlagInfo(Flag, out SV, out RV);
                return (SV);
            }

            FlagDataStruct PluginFlag;
            if (MainCHMForm.FlagData.FlagDataDictionary.TryGetValue(Flag.ToLower(), out PluginFlag))
                return (PluginFlag.Value);
            return ("");
        }

        public Tuple<string, string, string>[] ServerGetFlagsInList(string[] ListOfFlags)
        {
            Tuple<string, string, string>[] FlagValues;
            FlagDataStruct PluginFlag;

            if (ListOfFlags == null || ListOfFlags.Length == 0)
            {
                FlagValues = null;
                return (FlagValues);
            }
            FlagValues = new Tuple<string, string, string>[ListOfFlags.Length];
            for (int i = 0; i < ListOfFlags.Length; i++)
            {
                if (string.IsNullOrEmpty(ListOfFlags[i]))
                {
                    FlagValues[i] = new Tuple<string, string, string>("","","");
                    continue;

                }
                if (ListOfFlags[i].Substring(0, 2) == "$$")
                {
                    string SV, RV;
                    MainCHMForm.FlagData FD = new MainCHMForm.FlagData();
                    FD.GetSpecialFlagInfo(ListOfFlags[i], out SV, out RV);
                    FlagValues[i] = new Tuple<string, string, string>(SV, RV, "");
                    continue;
                }

                if (!MainCHMForm.FlagData.FlagDataDictionary.TryGetValue(ListOfFlags[i].ToLower(), out PluginFlag))
                {
                    FlagValues[i] = new Tuple<string, string, string>("", "", "");
                    continue;
                }
                FlagValues[i] = new Tuple<string, string, string>(PluginFlag.Value, PluginFlag.RawValue, PluginFlag.UOM);
            }
            return (FlagValues);
        }

        public FlagDataStruct GetSingleFlagFromServerFull(string Flag)
        {
            FlagDataStruct NF = new FlagDataStruct();

            FlagDataStruct PluginFlag;

            if (MainCHMForm.FlagData.FlagDataDictionary.TryGetValue(Flag.ToLower(), out PluginFlag))
            {
                NF.Name = PluginFlag.Name;
                NF.SubType = PluginFlag.SubType;
                NF.Value = PluginFlag.Value;
                NF.UOM = PluginFlag.UOM;
                NF.RawValue = PluginFlag.RawValue;
                NF.RoomUniqueID = PluginFlag.RoomUniqueID;
                NF.SourceUniqueID = PluginFlag.SourceUniqueID;
                NF.LogCode = PluginFlag.LogCode;
                NF.MaxHistoryToSave = PluginFlag.MaxHistoryToSave;
                NF.ValidValues = PluginFlag.ValidValues;
            }
            return (NF);
        }

        public Tuple<string, string, string> GetAutomation(string AutomationName, string AutomationType)
        {
            string[] Fields = new string[] { "AutomationProcessName", "AutomationProcessType" };
            string[] Values = new string[] { AutomationName, AutomationType};
            bool ValidData;
            string[] AutomationData;


            var Reader = GeneralDB.ExecuteSQLCommandWithReader("AutomationProcesses", Fields, Values, out ValidData);
            if (ValidData)
            {
                Reader = GeneralDB.GetNextRecordWithReader(ref Reader, out AutomationData, out ValidData);
                if (ValidData)
                {
                    return (new Tuple<string, string, string>(AutomationData[2], AutomationData[3], AutomationData[4]));
                }
            }
            return (null);
        }

        public string GetMacro(string MacroName, string MacroType, string MacroOwner)
        {
            string ReturnValue;
            string [] Fields = new string[] { "MacroName","MacroType","MacroOwner"};
            string[] Values = new string[] { MacroName, MacroType, MacroOwner };

            GeneralDB.GetItemByFieldsIntoString("Macros", Fields, Values, "MacroXML", out ReturnValue);
            return (ReturnValue);

        }

        public  bool  RunDirectCommand(ref PluginCommunicationStruct PCS, DeviceStruct DS)
        {

            InterfaceStruct IS;
            bool fl1 = MainCHMForm.InterfaceDictionary.TryGetValue(DS.InterfaceUniqueID, out IS);
            if (fl1)
            {
                DateTime CT = MainCHMForm._GetCurrentDateTime();

                PCS.DestinationPlugin = IS.ControllingDLL;
                MainCHMForm.PluginStruct PIS = new MainCHMForm.PluginStruct();
                bool fl2 = MainCHMForm.PluginDictionary.TryGetValue(PCS.DestinationPlugin, out PIS);
                if (fl2)
                {
                    PIS.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PIS.ServerInstance, new object[] { CT, PCS });
                }
                return (true);
            }
            return (false);
        }

        public bool GetDeviceFromDB(string UniqueID, ref DeviceStruct DS, ref RoomStruct Room)
        {
            //bool DeviceValidData;

            if (MainCHMForm.DeviceDictionary.TryGetValue(UniqueID, out DS))
            {
                string Q = DS.RoomUniqueID;
                Tuple<string, string, string, string> S = MainCHMForm.RoomList.Find(c => c.Item1 == Q);
                Room.UniqueID = S.Item1;
                Room.RoomName = S.Item2;
                Room.Location = S.Item3;
                Room.InterfaceUniqueIDs = S.Item4;
                return (true);
            }
            return (false);
            
            //string[] Device;

            //var DeviceReader = MacroDB.ExecuteSQLCommandWithReader("Devices", "UniqueID='"+ UniqueID+"'", out DeviceValidData);
            //DeviceReader = MacroDB.GetNextRecordWithReader(ref DeviceReader, out Device, out DeviceValidData);
            //if (!DeviceValidData)
            //{
            //    MacroDB.CloseNextRecordWithReader(ref DeviceReader);
            //    return (false);
            //}
            //else
            //{
            //    DS.DeviceUniqueID = Device[0];
            //    DS.DeviceName = Device[1];
            //    DS.DeviceType = Device[2];
            //    DS.DeviceClassID = Device[3];
            //    DS.RoomUniqueID = Device[4];
            //    DS.InterfaceUniqueID = Device[5];
            //    DS.DeviceIdentifier = Device[6];
            //    DS.NativeDeviceIdentifier = Device[7];
            //    DS.UOMCode = Device[8];
            //    DS.Origin = Device[9];
            //    DS.AdditionalFlagName = Device[10];
            //    DS.HTMLDisplayName = Device[11];
            //    DS.AFUOMCode = Device[12];
            //    DS.DeviceGrouping = Device[13];
            //    DS.XMLConfiguration = Device[14];
            //    DS.IgnoreOffLineWarning = Device[15];
            //    DS.UndesignatedFieldsInfo = Device[16];
            //    DS.IntVal01 = _ConvertToInt32(Device[17]);
            //    DS.IntVal02 = _ConvertToInt32(Device[18]);
            //    DS.IntVal03 = _ConvertToInt32(Device[19]);
            //    DS.IntVal04 = _ConvertToInt32(Device[20]);
            //    DS.StrVal01 = Device[21];
            //    DS.StrVal02 = Device[22];
            //    DS.StrVal03 = Device[23];
            //    DS.StrVal04 = Device[24];
            //    DS.Comments = Device[25];
            //    DS.Local_TableLoc = -1;
            //    DS.Local_Flag1 = false;
            //    DS.Local_Flag2 = false;
            //    DS.Local_IsLocalDevice = false;
            //    DS.Local_CommandStatementIgnore = "";
            //    DS.Local_OriginalInfo = "";

            //    MacroDB.GetBlobFieldByReader(DeviceReader, "ObjVal", out DS.objVal);
            //    MacroDB.CloseNextRecordWithReader(ref DeviceReader);

            //    string[] Internal;
            //    bool InternalValidData;

            //    var InternalReader = MacroDB.ExecuteSQLCommandWithReader("Rooms", "UniqueID='" + DS.RoomUniqueID + "'", out InternalValidData);
            //    InternalReader = MacroDB.GetNextRecordWithReader(ref InternalReader, out Internal, out InternalValidData);

            //    if (InternalValidData)
            //    {
            //        Room.UniqueID = Internal[0];
            //        Room.RoomName = Internal[1];
            //        Room.Location = Internal[2];
            //        Room.InterfaceUniqueIDs = Internal[3];
            //        Room.AIProcessCode = Internal[4];
            //    }
            //    MacroDB.CloseNextRecordWithReader(ref InternalReader);
            //}
            //return (true);
        }

    }
    #endregion








    #region Menu Click Class
    class DeviceMenuStuff
    {
        MainCHMForm MCH_Form;
        Dictionary<string, GroupBox> DevicesDisplayed;
        DeviceListForm DLF;

        internal int _ConvertToInt32(string Value)
        {
            try
            {
                double x;
                Double.TryParse(Value, out x);
                int xx = Convert.ToInt32(x);
                return (xx);

            }
            catch
            {
                return (0);
            }
        }

        internal DeviceMenuStuff(MainCHMForm MCH)
        {
            MCH_Form = MCH;
            DevicesDisplayed = new Dictionary<string, GroupBox>();
        }


        internal void DevicesMenuItemProcess(string WhichItem, string WhichItemTitle)
        {
            int spacing = 25;
            int ControlStart = 0;
            SortedSet<string> CL = new SortedSet<string>();

            MainCHMForm.FlagData FlagAccess = new MainCHMForm.FlagData();

            Dictionary<string, FlagDataStruct> LocalFlagDictionary = FlagAccess.CreateSpecialFlagDictionary();

            foreach (FlagDataStruct fd in LocalFlagDictionary.Values)
            {
                if (string.IsNullOrEmpty(fd.SourceUniqueID))
                    continue;
                if (fd.SourceUniqueID.Substring(0, 1) != "D")
                    continue;

                DeviceStruct d;
                if (!MainCHMForm.DeviceDictionary.TryGetValue(fd.SourceUniqueID, out d))
                    return;


                if ((string)WhichItem == "8000001")
                {
                    try
                    {
                        CL.Add(d.DeviceType);
                    }
                    catch
                    {

                    }
                }

                if ((string)WhichItem == "8000002")
                {
                    InterfaceStruct IS;
                    try
                    {
                        bool fl1 = MainCHMForm.InterfaceDictionary.TryGetValue(d.InterfaceUniqueID, out IS);
                        if (fl1)
                        {
                            CL.Add(IS.InterfaceName);
                        }
                        else
                        {
                            CL.Add(d.InterfaceUniqueID);
                        }
                    }
                    catch
                    {

                    }

                }

                if ((string)WhichItem == "8000003")
                {

                    try
                    {
                        string fl1 = MainCHMForm.RoomList.Find(c => c.Item1 == d.RoomUniqueID).Item2;
                        CL.Add(fl1);
                    }
                    catch
                    {

                    }

                }

                if ((string)WhichItem == "8000010")
                {
                    try
                    {
                        string fl1 = MainCHMForm.RoomList.Find(c => c.Item1 == d.RoomUniqueID).Item2;
                        CL.Add((fl1 + " " + d.DeviceName+" "+ fd.SubType).Trim());
                    }
                    catch
                    {

                    }

                }
            }

            DeviceSelectForm DevForm = new DeviceSelectForm();
            if ((string)WhichItem == "8000010")
                DevForm.SelectionBox.Enabled = false;

            DevForm.Tag = (string)WhichItem;
            DevForm.Text = WhichItemTitle;
            ControlStart = DevForm.Controls.Count;
            for (int i = 0; i < ControlStart; i++)
            {
                DevForm.Controls[i].Click += DeviceSelectForm_Click;
            }

            int size = CL.Count * spacing;
            int max = DevForm.Controls[0].Location.Y;
            int MaxHt = MCH_Form.Size.Height - 50;

            int cols = (size / MaxHt) + 1;
            int percol = CL.Count / cols;
            int add = percol * spacing;
            int Overhd = DevForm.Size.Height;

            DevForm.ClientSize = new System.Drawing.Size(DevForm.Size.Width * cols, add + DevForm.Controls[0].Size.Height + (DevForm.Controls[0].Size.Height / 2));
            int x = 15;
            for (int i = 0; i < ControlStart; i++)
            {
                DevForm.Controls[i].Location = new System.Drawing.Point(DevForm.Controls[i].Location.X, DevForm.ClientSize.Height - (DevForm.Controls[i].Size.Height + (DevForm.Controls[i].Size.Height / 2)));

            }

            int count = 1;

            foreach (string s in CL)
            {
                DevForm.Controls.Add(new CheckBox());
                int i = DevForm.Controls.Count - 1;
                DevForm.Controls[i].Text = s;
                DevForm.Controls[i].AutoSize = true;

                if (count > percol)
                {
                    count = 1;
                    x = x + DevForm.Width / cols;
                }
                DevForm.Controls[i].Location = new System.Drawing.Point(x, (count - 1) * spacing);
                count++;
            }

            DevForm.StartPosition = FormStartPosition.Manual;
            DevForm.Location = new Point(MCH_Form.Location.X + (MCH_Form.Width - DevForm.Width) / 2, MCH_Form.Location.Y + (MCH_Form.Height - DevForm.Height) / 2);
            DevForm.Show(MCH_Form);

            return;


        }



        internal void DeviceSelectForm_Click(object sender, EventArgs e)
        {
            int spacing = 5;
            Button btn = sender as Button;
            DeviceSelectForm DevForm = (DeviceSelectForm)btn.Parent;


            if (btn.Name == "RBAll" || btn.Name == "RBDevices" || btn.Name == "RBSensors" || btn.Name == "RBSetable")
                return;

            if (btn.Name == "Cancel")
            {

                DevForm.Close();
                return;

            }

            if (btn.Name == "All")
            {
                foreach (Control ctr in DevForm.Controls)
                {
                    if (ctr is CheckBox)
                    {
                        CheckBox Chk = (CheckBox)ctr;
                        Chk.Checked = true;
                    }
                }
                btn.Name = "None";
                btn.Text = "None";
                return;
            }

            if (btn.Name == "None")
            {
                for (int i = 3; i < DevForm.Controls.Count; i++)
                {
                    CheckBox Chk = (CheckBox)DevForm.Controls[i];
                    Chk.Checked = false;
                }
                btn.Name = "All";
                btn.Text = "All";
                return;
            }

            if (btn.Name == "Display")
            {
                SortedSet<string> DeviceMenuDisplayList = new SortedSet<String>();
                List<Tuple<string, string>> InterfaceList = new List<Tuple<string, string>>();
                if ((string)DevForm.Tag == "8000002")
                {
                    foreach (InterfaceStruct intf in MainCHMForm.InterfaceDictionary.Values)
                    {
                        InterfaceList.Add(new Tuple<string, string>(intf.InterfaceUniqueID, intf.InterfaceName));

                    }

                }

                DLF = new DeviceListForm();
                DLF.FormClosed += DevFormForm_Closed;
                string DevicesSelectionMode = "";

                for (int i = 0; i < DevForm.Controls.Count; i++)
                {
                    if (DevForm.Controls[i].GetType() == typeof(GroupBox))
                    {
                        DevicesSelectionMode = DevForm.Controls[i].Controls.OfType<RadioButton>().SingleOrDefault(rad => rad.Checked == true).Name;
                        continue;
                    }

                    if (DevForm.Controls[i].GetType() != typeof(CheckBox))
                        continue;

                    CheckBox Chk = (CheckBox)DevForm.Controls[i];

                    if (Chk.Checked)
                    {
                        if ((string)DevForm.Tag == "8000010")
                        {

                            FlagDataStruct Flag;
                            if (!MainCHMForm.FlagAccess.GetFlag(Chk.Text, "", out Flag))
                                continue;
                            DeviceStruct d;
                            if (!MainCHMForm.DeviceDictionary.TryGetValue(Flag.SourceUniqueID, out d))
                                continue;
                            string fl1 = MainCHMForm.RoomList.Find(c => c.Item1 == d.RoomUniqueID).Item2;
                            DeviceMenuDisplayList.Add(Chk.Text + d.DeviceUniqueID);
                        }

                        if ((string)DevForm.Tag == "8000003")
                        {
                            string Room = MainCHMForm.RoomList.Find(c => c.Item2 == Chk.Text).Item1;
                            MainCHMForm.FlagData FlagAccess = new MainCHMForm.FlagData();
                            Dictionary<string, FlagDataStruct> LocalFlagDictionary = FlagAccess.CreateSpecialFlagDictionary();

                            foreach (FlagDataStruct Flag in LocalFlagDictionary.Values)
                            {
                                DeviceStruct d;
                                if (string.IsNullOrWhiteSpace((Flag.SourceUniqueID)))
                                    continue;
                                if (!MainCHMForm.DeviceDictionary.TryGetValue(Flag.SourceUniqueID, out d))
                                    continue;
                                if (d.RoomUniqueID != Room)
                                    continue;
                                DeviceMenuDisplayList.Add((Flag.Name+" "+Flag.SubType).Trim()+ d.DeviceUniqueID);
                            }
                        }

                        if ((string)DevForm.Tag == "8000002")
                        {
                            string Interface = InterfaceList.Find(c => c.Item2 == Chk.Text).Item1;
                            MainCHMForm.FlagData FlagAccess = new MainCHMForm.FlagData();
                            Dictionary<string, FlagDataStruct> LocalFlagDictionary = FlagAccess.CreateSpecialFlagDictionary();
                            foreach (FlagDataStruct Flag in LocalFlagDictionary.Values)
                            {
                                DeviceStruct d;
                                if (string.IsNullOrWhiteSpace((Flag.SourceUniqueID)))
                                    continue;
                                if (!MainCHMForm.DeviceDictionary.TryGetValue(Flag.SourceUniqueID, out d))
                                    continue;

                                if (d.InterfaceUniqueID != Interface)
                                    continue;
                                string fl1 = MainCHMForm.RoomList.Find(c => c.Item1 == d.RoomUniqueID).Item2;
                                DeviceMenuDisplayList.Add("(" + Chk.Text + ")\x01" + (Flag.Name + " " + Flag.SubType).Trim() + d.DeviceUniqueID);
                            }
                        }


                        if ((string)DevForm.Tag == "8000001")
                        {
                            MainCHMForm.FlagData FlagAccess = new MainCHMForm.FlagData();
                            Dictionary<string, FlagDataStruct> LocalFlagDictionary = FlagAccess.CreateSpecialFlagDictionary();
                            foreach (FlagDataStruct Flag in LocalFlagDictionary.Values)
                            {
                                DeviceStruct d;
                                if (string.IsNullOrWhiteSpace((Flag.SourceUniqueID)))
                                    continue;
                                if (!MainCHMForm.DeviceDictionary.TryGetValue(Flag.SourceUniqueID, out d))
                                    continue;
                                if (d.DeviceType != Chk.Text)
                                    continue;
                                string fl1 = MainCHMForm.RoomList.Find(c => c.Item1 == d.RoomUniqueID).Item2;
                                DeviceMenuDisplayList.Add("(" + Chk.Text + ")\x01" + (Flag.Name + " " + Flag.SubType).Trim() + d.DeviceUniqueID);
                            }
                        }
                    }
                }

                int index = 0;
                int max = 10;
                int MHeight = 0;
                foreach (string DU in DeviceMenuDisplayList)
                {
                    DeviceStruct DV;

                    if (DU.Length < 14)
                        continue;
                    string X = DU.Substring(DU.Length - 13);
                    if (!MainCHMForm.DeviceDictionary.TryGetValue(X, out DV))
                        continue;
                    GroupBox groupBox1 = new GroupBox();
                    groupBox1.Size = new Size(250, spacing - 1);
                    groupBox1.FlatStyle = FlatStyle.Flat;
                    groupBox1.Tag = new Tuple<string, string>(DV.DeviceUniqueID, DevicesSelectionMode);
                    groupBox1.TabStop = false;
                    TextBox TB = new TextBox();
                    TB.Text = DU.Substring(0, DU.Length - 13).Replace("\x01"," ");
                    Size size = TextRenderer.MeasureText(TB.Text, TB.Font);
                    MHeight = Math.Max(MHeight, TB.Height);
                    TB.Width = size.Width;
                    //TB.Height = MHeight;
                    TB.ReadOnly = true;
                    TB.BorderStyle = System.Windows.Forms.BorderStyle.None;
                    TB.TabStop = false;
                    groupBox1.Controls.Add(TB);
                    TB.Location = new Point(5, 10);
                    max = TB.Width + 5;

                    string fl1 = MainCHMForm.RoomList.Find(c => c.Item1 == DV.RoomUniqueID).Item2;
                    String QQ = DU.Substring(0,DU.Length - 13);
                    int q = QQ.IndexOf("\x01");
                    if (q > -1)
                        QQ = QQ.Substring(q + 1);
                    FlagDataStruct Flag;

                    if (MainCHMForm.FlagAccess.GetFlag(QQ, "", out Flag))
                    {
                        //Available States to Change To
                        MHeight = 0;
                        DisplayDeviceValue(ref groupBox1, Flag, DV, DevicesSelectionMode, ref max, ref MHeight, ref size);
                    }
                    if(groupBox1.Controls.Count==0)
                    {
                        continue;
                    }

                    groupBox1.Location = new Point(5, index);
                    groupBox1.Size = new Size(max, MHeight + 10);
                    index = index + MHeight+spacing;
                    DLF.Controls.Add(groupBox1);
                    DevicesDisplayed.Add((string)DV.DeviceUniqueID+Flag.UniqueID, groupBox1);
                }
                DevForm.Close();

                int C = DLF.Controls.Count;

                if (C == 0)
                {
                    C = DLF.Size.Height;
                }
                else
                {
                    C = DLF.Controls[C - 1].Location.Y + DLF.Controls[C - 1].Size.Height;
                }

                DLF.Size = new Size(DLF.Size.Width, Math.Min(MCH_Form.Size.Height - 100, C));

                int newHeight = C;
                foreach (Control ctr in DLF.Controls)
                {
                    if (ctr is Button)
                    {
                        ctr.Location = new Point(ctr.Location.X, C + 5);
                        newHeight = Math.Max(ctr.Size.Height + 10, C + 10 + ctr.Size.Height);
                    }
                }
                DLF.ClientSize = new Size(DLF.Size.Width, Math.Min(MCH_Form.Size.Height - 50, newHeight));
                DLF.StartPosition = FormStartPosition.Manual;
                DLF.Location = new Point(MCH_Form.Location.X + (MCH_Form.Width - DevForm.Width) / 2, MCH_Form.Location.Y + (MCH_Form.Height - DevForm.Height) / 2);
                DLF.Show(MCH_Form);
                Tuple<string, FlagDataStruct> ignored;
                while (MainCHMForm.MenuDeviceListFlagChanges.TryDequeue(out ignored)) ;
                MainCHMForm.IsTheMenuDeviceListActive = true;
                DLF.DeviceListFormTimer.Tick += DeviceListFormTimer_Tick;
            }

        }

        internal void DevFormForm_Closed(object sender, EventArgs e)
        {
            MainCHMForm.IsTheMenuDeviceListActive = false;
            DLF.DeviceListFormTimer.Enabled = false;
        }

        internal void DeviceListFormTimer_Tick(object sender, EventArgs e)
        {
            Tuple<string, FlagDataStruct> Ltpl;
            GroupBox DevContrl;
            DeviceStruct Dev;

            while (MainCHMForm.MenuDeviceListFlagChanges.TryDequeue(out Ltpl))
            {
                if (!DevicesDisplayed.TryGetValue(Ltpl.Item2.SourceUniqueID+ Ltpl.Item2.UniqueID, out DevContrl))
                    continue;
                if (!MainCHMForm.DeviceDictionary.TryGetValue(Ltpl.Item2.SourceUniqueID, out Dev))
                    continue;
                Tuple<string, string> TBV;
                TBV = ((Tuple<string, string>)DevContrl.Tag);

                int MHeight = DevContrl.Height, max = DevContrl.Controls[0].Width + 5;

                while (DevContrl.Controls.Count > 1)
                    DevContrl.Controls.RemoveAt(1);
                Size size = DevContrl.Size;
                DisplayDeviceValue(ref DevContrl, Ltpl.Item2, Dev, TBV.Item2, ref max, ref MHeight, ref size);
                DevContrl.Size = new Size(max, MHeight);

            }
        }

        void DisplayDeviceValue(ref GroupBox groupBox1, FlagDataStruct Flag, DeviceStruct DV, string DevicesSelectionMode, ref int max, ref int MHeight, ref Size size)
        {

            XmlDocument XML = new XmlDocument();
            PluginCommunicationStruct PCS = new PluginCommunicationStruct();
            int ValidCommand = 0;
            bool HasSettableControl = false;
            try
            {
                if (!string.IsNullOrEmpty(DV.XMLConfiguration))
                {
                    XML.LoadXml(DV.XMLConfiguration);
                    XmlNodeList CommandList = XML.SelectNodes("/root/commands/command");
                    if (CommandList.Count == 0)
                        CommandList = XML.SelectNodes("/commands/command");

                    bool ThisIsAValidSubfield = false;
                    bool RangeEqual = false;
                    string SpecialRangeEqual = "";

                    foreach (XmlElement el in CommandList)
                    {
                        string State = "", SubField = null, RangeStart = "", RangeEnd = "", FlagValueToUse = "";
                        for (int i = 0; i < el.Attributes.Count; i++)
                        {
                            if (el.Attributes[i].Name.ToLower() == "state")
                            {
                                State = el.Attributes[i].Value;
                                ValidCommand = 2;
                            }

                            if (el.Attributes[i].Name.ToLower() == "subfield")
                            {
                                SubField = el.Attributes[i].Value.ToLower();
                            }

                            if (el.Attributes[i].Name.ToLower() == "rangestart")
                            {
                                RangeStart = el.Attributes[i].Value;
                                ValidCommand++;
                            }

                            if (el.Attributes[i].Name.ToLower() == "rangeend")
                            {
                                RangeEnd = el.Attributes[i].Value;
                                ValidCommand++;
                            }

                            if (el.Attributes[i].Name.ToLower() == "flagvaluetouse")
                            {
                                FlagValueToUse = el.Attributes[i].Value;
                            }
                        }

                        if (RangeStart == RangeEnd && !string.IsNullOrEmpty(RangeStart))
                        {
                            RangeEqual = true;
                            SpecialRangeEqual = RangeStart;
                            continue;
                        }

                        if (ValidCommand >= 2 && (SubField == null || SubField == Flag.SubType.ToLower())) //A Valid Choice
                        {
                            ThisIsAValidSubfield = true;
                            if (RangeStart != "" && RangeEnd != "") //Range Value
                            {
                                TrackBar TB = new TrackBar();
                                Label LB = new Label();
                                TB.Tag = new Tuple<string, Label, string>(DV.DeviceUniqueID, LB, SubField);
                                TB.Minimum = _ConvertToInt32(RangeStart);
                                TB.Maximum = _ConvertToInt32(RangeEnd);
                                if(RangeEqual)
                                {
                                    int r =_ConvertToInt32(SpecialRangeEqual);
                                    if (r < TB.Minimum)
                                        TB.Minimum = r;
                                    if (r > TB.Maximum)
                                        TB.Maximum = r;
                                }
                                MHeight = Math.Max(MHeight, TB.Height);
                                groupBox1.Controls.Add(TB);
                                HasSettableControl = true;
                                TB.Location = new Point(max, 10);
                                TB.TabStop = false;
                                max = max + TB.Width + 5;
                                groupBox1.Controls.Add(LB);
                                LB.Width = 50;
                                LB.Location = new Point(max, 10);
                                LB.TabStop = false;
                                max = max + LB.Width + 5;



                                if (FlagValueToUse == "RawValue")
                                    TB.Value = _ConvertToInt32(Flag.RawValue);
                                else
                                    TB.Value = _ConvertToInt32(Flag.Value);
                                TB.Scroll += new EventHandler(TrackBarScroll);
                                TB.MouseUp += new MouseEventHandler(TrackBarMouseUp);
                                LB.Text = "" + TB.Value;
                            }
                            else
                            {
                                RadioButton RB = new RadioButton();
                                RB.Text = State;
                                RB.Tag = new Tuple<string, string, string>(DV.DeviceUniqueID, RB.Text, SubField);
                                MHeight = Math.Max(MHeight, RB.Height);
                                groupBox1.Controls.Add(RB);
                                HasSettableControl = true;
                                RB.Location = new Point(max, 10);
                                RB.TabStop = false;
                                max = max + RB.Width + 5;
                                if (RB.Text == Flag.Value)
                                    RB.Checked = true;
                                else
                                    RB.Checked = false;
                                RB.CheckedChanged += new EventHandler(DisplayDeviceradioButtonsCheckedChanged);
                            }
                        }
                    }
                    if (DevicesSelectionMode == "RBSetable" && !HasSettableControl)
                    {
                        groupBox1.Controls.Clear();
                        return;
                    }

                    if ((DevicesSelectionMode == "RBSensors" && CommandList.Count > 0) || CommandList.Count == 0 || !ThisIsAValidSubfield)
                    {
                        TextBox TBV = new TextBox();
                        TBV.Text = Flag.Value + Flag.UOM;
                        size = TextRenderer.MeasureText(TBV.Text, TBV.Font);
                        MHeight = Math.Max(MHeight, TBV.Height);
                        TBV.Width = size.Width;
                        //TBV.Height = MHeight;
                        TBV.ReadOnly = true;
                        TBV.BorderStyle = System.Windows.Forms.BorderStyle.None;
                        TBV.Tag = DV.DeviceUniqueID;
                        groupBox1.Controls.Add(TBV);
                        TBV.Location = new Point(max, 10);
                        TBV.TabStop = false;
                        max = max + TBV.Width + 5;
                        return;

                    }
                }
                if (DevicesSelectionMode == "RBSetable" && !HasSettableControl)
                {
                    groupBox1.Controls.Clear();
                    return;
                }
            }
            catch
            {
                if (DevicesSelectionMode == "RBSetable" && !HasSettableControl)
                {
                    groupBox1.Controls.Clear();
                    return;
                }

                if (DevicesSelectionMode == "RBDevices")
                    return;
                TextBox TBV = new TextBox();
                TBV.Text = Flag.Value + Flag.UOM;
                size = TextRenderer.MeasureText(TBV.Text, TBV.Font);
                MHeight = Math.Max(MHeight, TBV.Height);
                TBV.Width = size.Width;
                //TBV.Height = MHeight;
                TBV.ReadOnly = true;
                TBV.BorderStyle = System.Windows.Forms.BorderStyle.None;
                TBV.Tag = DV.DeviceUniqueID;
                groupBox1.Controls.Add(TBV);
                TBV.Location = new Point(max, 10);
                TBV.TabStop = false;
                max = max + TBV.Width + 5;
            }



        }

        private void TrackBarMouseUp(object sender, MouseEventArgs e)
        {
            TrackBar trackbar = sender as TrackBar;
            DeviceStruct DS;
            Tuple<string, Label, string> tp = (Tuple<string, Label, string>)trackbar.Tag;
            string Dev = tp.Item1;
            PluginCommunicationStruct PCS = new PluginCommunicationStruct();
            DateTime CT = MainCHMForm._GetCurrentDateTime();
            PCS.Command = PluginCommandsToPlugins.ProcessCommandWords;
            PCS.DeviceUniqueID = Dev;
            PCS.String = Dev;
            PCS.String2 = trackbar.Value.ToString();
            PCS.String5 = tp.Item3;
            PCS.UniqueNumber = string.Format("{0:0000}-{1:0000000000}", "XXXX", MainCHMForm.NextSequencialNumber);
            PCS.OriginPlugin = MainCHMForm.MODULESERIALNUMBER;
            bool fl0 = MainCHMForm.DeviceDictionary.TryGetValue(Dev, out DS);

            if (fl0)
            {
                InterfaceStruct IS;
                bool fl1 = MainCHMForm.InterfaceDictionary.TryGetValue(DS.InterfaceUniqueID, out IS);
                if (fl1)
                {
                    PCS.DestinationPlugin = IS.ControllingDLL;
                    MainCHMForm.PluginStruct PIS = new MainCHMForm.PluginStruct();
                    bool fl2 = MainCHMForm.PluginDictionary.TryGetValue(PCS.DestinationPlugin, out PIS);
                    if (fl2)
                    {
                        PIS.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PIS.ServerInstance, new object[] { CT, PCS });
                    }

                }
            }

        }

        private void TrackBarScroll(object sender, EventArgs e)
        {
            TrackBar trackbar = sender as TrackBar;
            Tuple<string, Label, string> tp = (Tuple<string, Label, string>)trackbar.Tag;
            tp.Item2.Text = "" + trackbar.Value;
        }



        internal void DisplayDeviceradioButtonsCheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.Checked == false)
                return;
            DeviceStruct DS;
            //string Dev = (string)radioButton.Tag;
            Tuple<string, string, string> tp = (Tuple<string, string, string>)radioButton.Tag;
            PluginCommunicationStruct PCS = new PluginCommunicationStruct();
            DateTime CT = MainCHMForm._GetCurrentDateTime();
            PCS.Command = PluginCommandsToPlugins.ProcessCommandWords;
            PCS.DeviceUniqueID = tp.Item1;
            PCS.String = tp.Item1; 
            PCS.String2 = radioButton.Text;
            PCS.String5 = tp.Item3;
            PCS.UniqueNumber = string.Format("{0:0000}-{1:0000000000}", "XXXX", MainCHMForm.NextSequencialNumber);
            PCS.OriginPlugin = MainCHMForm.MODULESERIALNUMBER;
            bool fl0 = MainCHMForm.DeviceDictionary.TryGetValue(tp.Item1, out DS);

            if (fl0)
            {
                InterfaceStruct IS;
                bool fl1 = MainCHMForm.InterfaceDictionary.TryGetValue(DS.InterfaceUniqueID, out IS);
                if (fl1)
                {
                    PCS.DestinationPlugin = IS.ControllingDLL;
                    MainCHMForm.PluginStruct PIS = new MainCHMForm.PluginStruct();
                    bool fl2 = MainCHMForm.PluginDictionary.TryGetValue(PCS.DestinationPlugin, out PIS);
                    if (fl2)
                    {
                        PIS.ServerAssemblyType.InvokeMember("CHMAPI_PluginInformationCommingFromPlugin", BindingFlags.InvokeMethod, null, PIS.ServerInstance, new object[] { CT, PCS });
                    }

                }
            }
        }
        #endregion

        #region Utilities Class
        class Utilities
        {

            internal Utilities()
            {

            }

            internal string[] ConvertCSVRecordtoStringArray(string CSVFile)
            {
                string[] s;

                try
                {
                    s = Regex.Split(CSVFile, "\",\"", RegexOptions.Compiled);
                    if (s[0] == CSVFile)
                    {
                        s = CSVFile.Split(',');
                    }
                    else
                    {
                        if (s[0].StartsWith("\""))
                            s[0] = s[0].Substring(1);
                        if (s[s.Length - 1].EndsWith("\""))
                            s[s.Length - 1] = s[s.Length - 1].Substring(0, s[s.Length - 1].Length - 1);
                    }

                    return (s);
                }
                catch
                {
                    string[] x = new string[1];
                    x[0] = CSVFile;
                    return (x);
                }

            }

        }
        #endregion
    }
}


