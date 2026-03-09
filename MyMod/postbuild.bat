copy /y "C:\Users\Kendal\Documents\Github\BartKFSentinels\MyMod\bin\Release\BartKFMod.dll" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod"
copy /y "C:\Users\Kendal\Documents\Github\BartKFSentinels\MyMod\manifest.json" "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\"
copy /y "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\manifest.json" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod"
copy /y "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\preview.png" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod"
robocopy "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\DeckBrowser" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod\DeckBrowser" /e
robocopy "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\Atlas" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod\Atlas" /e
robocopy "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\Cutouts" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod\Cutouts" /e
robocopy "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\Endings" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod\Endings" /e
robocopy "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\Fonts" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod\Fonts" /e
robocopy "C:\Users\Kendal\Documents\Github\BartKFSentinels\Resources\LargeCardTextures" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\BartKFMod\LargeCardTextures" /e
exit 0