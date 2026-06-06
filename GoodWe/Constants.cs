namespace GoodWe;

public static class Constants
{
    public const int GoodWeUdpPort = 8899;
    public const int GoodWeTcpPort = 502;

    public static readonly IReadOnlyDictionary<int, string> BatteryModes = new Dictionary<int, string>
    {
        { 0, "No battery" }, { 1, "Standby" }, { 2, "Discharge" }, { 3, "Charge" },
        { 4, "To be charged" }, { 5, "To be discharged" },
    };

    public static readonly IReadOnlyDictionary<int, string> EnergyModes = new Dictionary<int, string>
    {
        { 0, "Check" }, { 1, "Wait" }, { 2, "On-Grid" }, { 3, "Off-Grid" },
        { 4, "Flash" }, { 5, "Fault" }, { 6, "PV Power Off" }, { 7, "Close" },
    };

    public static readonly IReadOnlyDictionary<int, string> GridModes = new Dictionary<int, string>
    {
        { 0, "Not connected" }, { 1, "Connected" }, { 2, "Fault" },
    };

    public static readonly IReadOnlyDictionary<int, string> GridInOutModes = new Dictionary<int, string>
    {
        { 0, "Idle" }, { -1, "Exporting" }, { 1, "Importing" },
    };

    public static readonly IReadOnlyDictionary<int, string> PvModes = new Dictionary<int, string>
    {
        { 0, "Not connected" }, { 1, "Connected, no power" }, { 2, "Connected, producing" },
    };

    public static readonly IReadOnlyDictionary<int, string> WorkModesET = new Dictionary<int, string>
    {
        { 0, "Check" }, { 1, "Wait" }, { 2, "On-Grid" }, { 3, "Off-Grid" },
        { 4, "Bypass" }, { 5, "Fault" }, { 6, "Flash" },
    };

    public static readonly IReadOnlyDictionary<int, string> SafetyCountries = new Dictionary<int, string>
    {
        { 0, "Italy" }, { 1, "Czech" }, { 2, "Germany" }, { 3, "Spain" },
        { 4, "Denmark" }, { 5, "Belgium" }, { 6, "Romania" }, { 7, "G98/G99 UK" },
        { 8, "Australia" }, { 9, "Greece" }, { 10, "Netherlands" }, { 11, "Austria" },
        { 12, "Switzerland" }, { 13, "Poland" }, { 14, "Sweden" }, { 15, "Slovakia" },
        { 16, "Ukraine" }, { 17, "Finland" }, { 18, "Hungary" }, { 19, "Bulgaria" },
        { 20, "G83/G59 UK" }, { 21, "Norway" }, { 22, "Portugal" }, { 23, "Turkey" },
        { 24, "Europe General" }, { 25, "Croatia" }, { 26, "Lithuania" }, { 27, "Estonia" },
        { 28, "Latvia" }, { 29, "Macedonia" }, { 30, "Bosnia" }, { 31, "Serbia" },
        { 32, "Montenegro" }, { 33, "Albania" }, { 34, "Slovenia" }, { 35, "Belarus" },
        { 255, "User defined" },
    };

    public static readonly IReadOnlyDictionary<int, string> ErrorCodes = new Dictionary<int, string>
    {
        { 0, "Utility Loss" }, { 1, "Grid Voltage Fault" }, { 2, "Grid Frequency Fault" },
        { 3, "DC Injection Fault" }, { 4, "Temperature Fault" }, { 5, "Fan Fault" },
        { 6, "Battery Voltage too High" }, { 7, "Battery Voltage too Low" },
        { 8, "Battery Open Circuit" }, { 9, "BMS Communication Fault" },
        { 10, "Battery Overload" }, { 11, "Battery Short Circuit" },
        { 12, "AFCI Arc Fault" }, { 13, "Ground Fault" }, { 14, "PV Over Voltage" },
        { 15, "PV Over Current" },
    };

    public static readonly IReadOnlyDictionary<int, string> DiagStatusCodes = new Dictionary<int, string>
    {
        { 0, "Battery SOC low (<10%)" }, { 1, "Battery SOC critical (<5%)" },
        { 2, "Battery not connected" }, { 4, "Upload to Sems failed" },
        { 7, "Meter communication fault" }, { 8, "CT not installed" },
        { 16, "Charge limited by battery" }, { 18, "Discharge limited by battery" },
        { 22, "Export power limited" }, { 29, "Self-use off" },
    };

    public static readonly IReadOnlyDictionary<int, string> DeratingModeCodes = new Dictionary<int, string>
    {
        { 0, "Over temperature" }, { 1, "Low AC voltage" }, { 2, "High AC voltage" },
        { 3, "Low PV voltage" }, { 4, "High DC bus voltage" }, { 5, "Low DC bus voltage" },
        { 6, "Grid frequency out of range" }, { 7, "Over output current" },
        { 8, "Over input current" }, { 9, "Power grid" }, { 10, "Overload" },
        { 11, "Over VA" }, { 12, "Grid monitoring" }, { 13, "MPPT" },
    };

    public static readonly IReadOnlyDictionary<int, string> MeterCommStatus = new Dictionary<int, string>
    {
        { 1, "Normal" }, { 2, "Disconnected" },
    };

    public static readonly IReadOnlyDictionary<int, string> WorkModes = new Dictionary<int, string>
    {
        { 0, "Wait" }, { 1, "On-Grid" }, { 2, "Off-Grid" }, { 3, "Fault" },
        { 4, "Flash" }, { 5, "PV Power Off" },
    };

    public static readonly IReadOnlyDictionary<int, string> BmsAlarmCodes = new Dictionary<int, string>
    {
        { 0, "Cell voltage too high" }, { 1, "Cell voltage too low" },
        { 2, "Module temperature too high" }, { 3, "Module temperature too low" },
        { 4, "Discharging overcurrent" }, { 5, "Charging overcurrent" },
        { 6, "Short circuit" }, { 7, "Cell voltage difference too large" },
    };
}
