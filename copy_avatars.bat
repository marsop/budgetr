@echo off
echo Copying to kawaii...
copy /Y "src\Budgetr.Web\wwwroot\img\tutorial-avatar*.png" "src\Budgetr.Web\wwwroot\img\avatars\kawaii\"
if errorlevel 1 echo Error copying to kawaii

echo Copying to zarzaparrilla...
copy /Y "src\Budgetr.Web\wwwroot\img\tutorial-avatar*.png" "src\Budgetr.Web\wwwroot\img\avatars\zarzaparrilla\"
if errorlevel 1 echo Error copying to zarzaparrilla

echo Listing kawaii...
dir "src\Budgetr.Web\wwwroot\img\avatars\kawaii"

echo Listing zarzaparrilla...
dir "src\Budgetr.Web\wwwroot\img\avatars\zarzaparrilla"
