using System;
using System.Collections.Generic;
using System.Text;

namespace gk3game.Game
{
    // are these stored in a .brn somewhere? it'd be
    // better to load them from somewhere than hardcode them.
    struct LocationCodes
    {
        public const string AbbeOffice = "off";
        public const string Balconies = "bal";
        
        public const string BuchelliBathroom = "b21";
        public const string BuchelliHidingPlace = "bmb";
        public const string BuchelliHidingPlaceParkingLot = "pl3";
        public const string BuchelliRoom = "r21";

        public const string ButhaneBathroom = "b29";
        public const string ButhaneRoom = "r29";

        public const string ChateauBlanchBottom = "cdb";
        public const string ChateauDeBlanchfort = "cd1";
        public const string ChateauDeSerrasAttic = "cs3";
        public const string ChateauDeSerrasBarn = "cs8";
        public const string ChateauDeSerrasBasement = "cs5";
        public const string ChateauDeSerrasCourtyard = "cse";
        public const string ChateauDeSerrasExt = "pl6";
        public const string ChateauDeSerrasGarage = "gri";
        public const string ChateauDeSerrasLibrary = "cs2";
        public const string ChateauDeSerrasWinepress = "cs6";

        public const string CouizaExt = "tr1";
        public const string CouizaInt = "tr2";

        public const string CoumeSourde = "csd";
        public const string CoumeSourdeParkingLot = "pl2";
        public const string DumbWaiter1 = "du1";
        public const string DumbWaiter2 = "du2";

        public const string EmilioBathroom = "b27";
        public const string EmilioRoom = "r27";
        public const string GabeBathroom = "b25";
        public const string GabeRoom = "r25";

        public const string HotelHall = "hal";
        public const string HotelKitchen = "kit";
        public const string HotelDiningRoom = "din";
        public const string HotelExterior = "rc1";
        public const string HotelLobby = "lby";
        public const string HotelPhoneRoom = "pho";

        public const string HowardBathroom = "b31";
        public const string HowardRoom = "r31";

        public const string LarryHouseExt = "lhe";
        public const string LarryManuscriptPlace = "lmb";
        public const string LarryStudy = "lhi";
        
        public const string LeErmitage = "ler";
        public const string LeErmitageParkingLot = "pl4";

        public const string LeFauteuilAcrossRoad = "vg1";
        public const string LeFauteuilDuDiable = "arm";
        public const string LeFauteuilParking = "vgr";

        public const string LeHommeMort = "lhm";
        public const string LeHommeMortParkingLot = "pl1";

        public const string Map = "map";
        public const string MopedCourtyard = "mop";
        public const string MoselyRoom = "r33";
        public const string MountCardouSite = "mcf";
        public const string MuseumLower = "ms2";
        public const string MuseumUpper = "ms3";

        public const string NEArmHexagram = "mcb";
        public const string SWArmHexagram = "bec";

        public const string BlanchCardouParkingLot = "plo";
        public const string PoussinTomb = "pou";
        
        public const string RennesLeBainsBar = "rl2";
        public const string RennesLeBainsExt = "rl1";
        public const string Cemetary = "cem";
        public const string Church = "chu";
        public const string RlcStreet2 = "rc2";
        public const string RlcStreet3 = "rc3";
        public const string RlcStreet4 = "rc4";

        public const string RoqueNegre = "roq";
        public const string SupplyCloset = "clo";
        public const string TempleCircleChamber = "te3";
        public const string TempleHexagram = "te4";
        public const string TempleHexagramPrechamber = "t4a";
        public const string TempleHolyOfHolies = "te6";
        public const string TemplePorch = "te1";
        public const string TempleSquare = "te2";
        public const string TempleVeilChamber = "te5";

        public const string TemporaryLocation = "xxx";

        public const string TourMagdalaBottom = "ma1";
        public const string TourMagdalaExt = "mag";
        public const string TourMagdalaStairs = "ma2";
        public const string TourMagdalaTop = "ma3";

        public const string ValleyRoads = "vr1";
        public const string VillaBethaniaInt = "bet";
        public const string WilkesBathroom = "b23";
        public const string WilkesDeadPlace = "wdb";
        public const string WilkesRoom = "r23";

        public const string WoodsSite = "wod";
        public const string WoodsSiteParking = "pl5";
    }

    struct TimeCodes
    {
        public const string Day1_10AM = "110a";
        public const string Day1_12PM = "112p";
        public const string Day1_02PM = "102p";
        public const string Day1_04PM = "104p";
        public const string Day1_06PM = "106p";

        public const string Day2_07AM = "207a";
        public const string Day2_10AM = "210a";
        public const string Day2_12PM = "212p";
        public const string Day2_02PM = "202p";
        public const string Day2_05PM = "205p";
        public const string Day2_02AM = "202a";

        public const string Day3_07AM = "307a";
        public const string Day3_10AM = "310a";
        public const string Day3_12PM = "312p";
        public const string Day3_03PM = "303p";
        public const string Day3_06PM = "306p";
        public const string Day3_09PM = "309p";
    }
}
