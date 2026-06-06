namespace GoodWe;

public enum SensorKind
{
    PV = 1,
    AC = 2,
    UPS = 3,
    BAT = 4,
    GRID = 5,
    BMS = 6,
}

public enum OperationMode
{
    General = 0,
    OffGrid = 1,
    Backup = 2,
    Eco = 3,
    PeakShaving = 4,
    SelfUse = 5,
    EcoCharge = 98,
    EcoDischarge = 99,
}

public enum EMSMode
{
    Auto = 1,
    ChargePV = 2,
    DischargePV = 3,
    ImportAC = 4,
    ExportAC = 5,
    Conserve = 6,
    OffGrid = 7,
    BatteryStandby = 8,
    BuyPower = 9,
    SellPower = 10,
    ChargeBattery = 11,
    DischargeBattery = 12,
}
