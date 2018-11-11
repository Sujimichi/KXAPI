#Collie - rounds all the files up into release

rm -rf bin/Release/KXAPI


mkdir bin/Release/KXAPI -p
mkdir bin/Release/KXAPI/Plugins -p
mkdir bin/Release/KXAPI/Assets -p

cp bin/Release/*.dll bin/Release/KXAPI/Plugins/
cp KXAPI.version bin/Release/KXAPI/Plugins/

cp -a assets/*.* bin/Release/KXAPI/
mv bin/Release/KXAPI/*.jpg bin/Release/KXAPI/Assets/
mv bin/Release/KXAPI/*.png bin/Release/KXAPI/Assets/

cp LICENCE.txt bin/Release/KXAPI/LICENCE.txt


MODVER=$(ruby -e "i=%x(cat KXAPI.cs | grep version); i=i.split(';')[0].split('=')[1].sub(';','').gsub('\"','').strip; puts i;")
KSPVER=$(ruby -e "i=%x(cat KXAPI.cs | grep 'Built Against KSP'); i=i.split(' ').last; puts i")

echo "version $MODVER" > bin/Release/KXAPI/version

rm bin/Release/*.dll
rm bin/Release/*.dll.mdb

cd bin/Release
rm -rf $MODVER/

mkdir $MODVER
#rm KXAPI_$MODVER.zip
zip -r $MODVER/KXAPI.zip KXAPI/

rm -rf ref_for_ksp_$KSPVER/
mkdir ref_for_ksp_$KSPVER
cp KXAPI/Plugins/KXAPI.dll ref_for_ksp_$KSPVER


rm -rf /home/sujimichi/KSP/dev_KSP-$KSPVER/GameData/KXAPI/
cp -R KXAPI/ /home/sujimichi/KSP/dev_KSP-$KSPVER/GameData/KXAPI/
touch /home/sujimichi/KSP/dev_KSP-$KSPVER/GameData/KXAPI/mode=dev

