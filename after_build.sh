rm -rf BuildOutput &&
mkdir BuildOutput &&
mkdir BuildOutput/BepInEx/ &&
mkdir BuildOutput/BepInEx/plugins/ &&
mkdir BuildOutput/BepInEx/plugins/TestAccount666."$CURRENT_PROJECT"/ &&
mkdir BuildOutput/BepInEx/plugins/TestAccount666."$CURRENT_PROJECT"/MoreCompanyCosmetics &&
cp -f "$CURRENT_PROJECT"/bin/Debug/netstandard2.1/TestAccount666."$CURRENT_PROJECT".dll BuildOutput/BepInEx/plugins/TestAccount666."$CURRENT_PROJECT"/"$CURRENT_PROJECT".dll &&
cp -f "$CURRENT_PROJECT"/README.md BuildOutput/ &&
cp -f "$CURRENT_PROJECT"/CHANGELOG.md BuildOutput/ &&
cp -f "$CURRENT_PROJECT"/icon.png BuildOutput/ &&
cp -f LICENSE BuildOutput/ &&
cp -f voidleak BuildOutput/BepInEx/plugins/TestAccount666."$CURRENT_PROJECT"/ &&
cp -f VoidLeak.cosmetics BuildOutput/BepInEx/plugins/TestAccount666."$CURRENT_PROJECT"/MoreCompanyCosmetics/ &&
./generate_manifest.sh &&
./generate_zipfile.sh &&
dolphin "./BuildOutput"