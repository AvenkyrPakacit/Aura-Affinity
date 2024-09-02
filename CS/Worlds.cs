//using XRL;
//using XRL.World;
//using XRL.World.WorldBuilders;
//using XRL.World.ZoneBuilders;

//namespace AA.AuraAffinity
//{
//    [JoppaWorldBuilderExtension]
//    public class YourJoppaWorldBuilderExtension : IJoppaWorldBuilderExtension
//    {
//        public override void OnAfterBuild(JoppaWorldBuilder builder)
//        {
//            var location = builder.popMutableLocationOfTerrain("Hills", centerOnly: false);
//            var zoneID = builder.ZoneIDFromXY("JoppaWorld", location.X, location.Y);
//            // Change these parameters as appropriate for the secret that you're
//            // adding.
//            // - The second parameter affects how the secret appears in the journal
//            // - The third parameter affects which factions will sell the secret.
//            // - The fourth parameter affects the category under which the secret
//            //   shows up in the journal.
//            // - The fifth parameter is the ID for the secret, which can be revealed
//            //   using e.g. the revealsecret wish.
//            var secret = builder.AddSecret(
//                zoneID,
//                "the location of a Archaic Vault",
//                new string[2] { "lair", "eidrati" },
//                "Lairs",
//                "$AA_Vault"
//            );
//            // Add zone builders to the zone.
//            //
//            // Each zone builder has a different priority (the integer parameter that
//            // is passed in). Builders with lower priority run before builders with
//            // higher priority.
//            var zoneManager = The.ZoneManager;
//            // Add some roads to the north and south
//            zoneManager.AddZoneBuilder(zoneID, ZoneBuilderPriority.LATE, nameof(RoadNorthMouth));
//            zoneManager.AddZoneBuilder(zoneID, ZoneBuilderPriority.LATE, nameof(RoadSouthMouth));
//            // Add an object to the zone
//            // Replace "Oboroqoru" with the object ID of the creature you want to add
//            var creature = GameObject.Create("Oboroqoru");
//            zoneManager.AddZonePostBuilder(zoneID, nameof(AddObjectBuilder), "Object", zoneManager.CacheObject(creature));
//            // You can also set various properties on the zone, if you wish.
//            zoneManager.SetZoneName(zoneID, "Archaic Vault", Article: "a", Proper: true);
//            zoneManager.SetZoneIncludeStratumInZoneDisplay(zoneID, false);
//            zoneManager.SetZoneProperty(zoneID, "NoBiomes", "Yes");
//        }
//    }
//}