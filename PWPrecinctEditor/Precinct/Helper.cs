using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWPrecinctEditor
{
    public static class Helper
    {
        public static List<Offset> PWVersion = new List<Offset>();
        public static Offset selectedVersion;
        public static void LoadOffset()
        {
            PWVersion.Add(new Offset(
                "PW155v156",
                "00E444A4 0000001C 00000034",
                "0000003C",
                "00000040",
                "00000044"));
            PWVersion.Add(new Offset(
                "PW153v145",
                "00DA433C 0000001C 00000028",
                "0000003C",
                "00000040",
                "00000044"));
            PWVersion.Add(new Offset(
                "PW152v127",
                "1E844F58 0000000C 000014A0",
                "00000180",
                "00000184",
                "00000188"));
            PWVersion.Add(new Offset(
                "PW151v101",
                "00C76DCC 0000002C 00001420",
                "00000180",
                "00000184",
                "00000188"));
            PWVersion.Add(new Offset(
                "PW150v88",
                "00C392CC 00000034 00001340",
                "00000180",
                "00000184",
                "00000188"));
            PWVersion.Add(new Offset(
                "PW148v85",
                "00C0CDEC 00000034 131C",
                "00000180",
                "00000184",
                "00000188"));
            PWVersion.Add(new Offset(
                "PW146v80",
                "00BBC9CC 00000034 000012B4",
                "0000017C",
                "00000180",
                "00000184"));
            PWVersion.Add(new Offset(
                "PW146v70",
                "00B9029C 00000034 000011D8",
                "0000017C",
                "00000180",
                "00000184"));
            PWVersion.Add(new Offset(
                "PW145v650",
                "00B3B904 00000034 0000116C",
                "0000017C",
                "00000180",
                "00000184"));
            PWVersion.Add(new Offset(
                "PW145v615",
                "00A52AAC 00000034 00001134",
                "0000017C",
                "00000180",
                "00000184"));
            PWVersion.Add(new Offset(
                "PW144v606",
                "00B29184 00000034 000010E8",
                "0000017C",
                "00000180",
                "00000184"));
            PWVersion.Add(new Offset(
                "PW136v101",
                "0092764C 00000020 00000C30",
                "0000017C",
                "00000180",
                "00000184"));
        }

        public static void GetElementClientProcess()
        {
            Process pw = Process.GetProcessesByName("elementclient").FirstOrDefault();
            if(pw != null)
            {

            }
        }
    }
}
